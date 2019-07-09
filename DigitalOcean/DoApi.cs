using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BcatBotFramework.Core.Config;
using Newtonsoft.Json;

namespace DigitalOcean
{
    public static class DoApi
    {
        // Generated on initialization
        private static HttpClient httpClient;
        private static bool initialized = false;

        public static void Initialize()
        {
            // Create custom HttpClient
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            httpClient = new HttpClient(httpClientHandler);

            // Add headers
            httpClient.DefaultRequestHeaders.Add("User-Agent", "BcatBotFramework/1.0");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Configuration.LoadedConfiguration.DoConfig.Token}");

            // Set long timeout b/c of cache clearing requests
            httpClient.Timeout = TimeSpan.FromMinutes(10);

            // Set initialized
            initialized = true;
        }

        public static void Dispose()
        {
            httpClient.Dispose();
            initialized = false;
        }

        public static async Task<DoResponse> SendRequest(DoRequest request)
        {
            // Check if initialized
            if (!initialized)
            {
                throw new Exception("Cannot make requests to DigitalOcean while uninitialized");
            }

            // Declare a retry counter
            int retries = 3;

            while (true)
            {
                try 
                {
                    // Create the request message
                    HttpRequestMessage message = new HttpRequestMessage(request.Method, request.Url);
                    message.Content = new StringContent(JsonConvert.SerializeObject(request));

                    // Send the request
                    using (HttpResponseMessage response = await httpClient.SendAsync(message))
                    using (HttpContent content = response.Content)
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            // Read in the error
                            string json = await content.ReadAsStringAsync();

                            try
                            {
                                // Create a new DoErrorResponse object
                                DoErrorResponse errorResponse = JsonConvert.DeserializeObject<DoErrorResponse>(json);

                                // Check if this is a retry error
                                if (response.StatusCode == HttpStatusCode.ServiceUnavailable && errorResponse.Message == "Failed to forward the request you made, please try again.")
                                {
                                    // Add another retry
                                    retries++;
                                }

                                throw new Exception($"DigitalOcean request not a success ({response.StatusCode}, \"{errorResponse.Message}\")");   
                            }
                            catch (JsonReaderException)
                            {
                                throw new Exception($"DigitalOcean request not a success ({response.StatusCode}, bad json returned)");   
                            }
                        }

                        if (response.StatusCode == HttpStatusCode.NoContent)
                        {
                            return new DoNoContentResponse();
                        }
                        else
                        {
                            // Read in the error
                            string json = await content.ReadAsStringAsync();

                            // Deserialize the response object
                            return (DoResponse)JsonConvert.DeserializeObject(json, request.ResponseType);
                        }
                    }
                }
                catch (Exception e)
                {
                    // Decrement the retries
                    retries--;

                    // Check the number of retries
                    if (retries <= 0)
                    {
                        throw new Exception("HTTP request failure", e);
                    }

                    Console.WriteLine("WARNING: DigitalOcean request failed, waiting 5 seconds (retries remaining = " + retries + ")");
                    Console.WriteLine(e.ToString());

                    // Wait for 5 seconds
                    Thread.Sleep(5000);
                }
            }
        }
        
    }
}