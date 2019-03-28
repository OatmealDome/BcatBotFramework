using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nintendo.Bcat.News;
using Nintendo.Bcat.News.Catalog;
using MessagePack;
using BcatBotFramework.Core.Config;
using BcatBotFramework.Core;

namespace Nintendo.Bcat
{
    public class BcatApi
    {
        // Bcat API
        private static string BCAT_LIST_BASE_URL = "https://bcat-list-%.cdn.nintendo.net/api/nx/v1";
        private static string BCAT_TOPIC_URL = BCAT_LIST_BASE_URL + "/list/{0}";

        private static string BCAT_TOPICS_BASE_URL = "https://bcat-topics-%.cdn.nintendo.net/api/nx/v1";
        private static string BCAT_TOPIC_ICON_URL = BCAT_TOPICS_BASE_URL + "/topics/{0}/icon";
        private static string BCAT_TOPIC_DETAIL_URL = BCAT_TOPICS_BASE_URL + "/topics/{0}/detail";
        private static string BCAT_TITLE_ID_TO_TOPICS_URL = BCAT_TOPICS_BASE_URL + "/titles/{0}/topics";
        private static string BCAT_CATALOG_URL = BCAT_TOPICS_BASE_URL + "/topics/catalog";

        private static string BCAT_QLAUNCH_TITLE_ID = "0100000000001000";
        private static string BCAT_QLAUNCH_PASSPHRASE = "acda358b4d32d17fd4037c1b5e0235427a8563f93b0fdb42a4a536ee95bbf80f";

        private static string BCAT_USER_AGENT = "libcurl (nnBcat; 789f928b-138e-4b2f-afeb-1acae8c21d897; SDK 7.3.0.0)";
        
        // Generated on initialization
        private static HttpClient httpClient;
        private static bool initialized = false;

        public static void Initialize()
        {
            // Create custom HttpClient
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
            httpClient = new HttpClient(httpClientHandler);

            // Add BCAT user agent
            httpClient.DefaultRequestHeaders.Add("User-Agent", BCAT_USER_AGENT);

            // Add serial
            httpClient.DefaultRequestHeaders.Add("X-Nintendo-Serial-Number", Configuration.LoadedConfiguration.CdnConfig.SerialNumber);

            // Set initialized
            initialized = true;
        }

        public static void Dispose()
        {
            httpClient.Dispose();
            initialized = false;
        }

        public static async Task<List<Entry>> GetCatalog(Country country, Language? language = null)
        {
            return await GetCatalog(new Country[] { country }, language);
        }

        public static async Task<List<Entry>> GetCatalog(Country[] countries, Language? language = null)
        {
            // Format the URL
            string url = BuildCountryAndLanguageUrl(BCAT_CATALOG_URL, countries, language);

            // Fetch the data
            return MessagePackSerializer.Deserialize<List<Entry>>(await DownloadContainerAndDecrypt(url, BCAT_QLAUNCH_TITLE_ID, BCAT_QLAUNCH_PASSPHRASE));
        }

        public static async Task<byte[]> GetTopicIcon(string topicId)
        {
            return await DownloadContainerAndDecrypt(string.Format(BCAT_TOPIC_ICON_URL, topicId), BCAT_QLAUNCH_TITLE_ID, BCAT_QLAUNCH_PASSPHRASE);
        }

        public static async Task<Detail> GetTopicDetail(string topicId, Country country, Language? language = null)
        {
            return await GetTopicDetail(topicId, new Country[] { country }, language);
        }

        public static async Task<Detail> GetTopicDetail(string topicId, Country[] countries, Language? language = null)
        {
            // Format the URL
            string url = BuildCountryAndLanguageUrl(string.Format(BCAT_TOPIC_DETAIL_URL, topicId), countries, language);

            // Fetch the data
            return MessagePackSerializer.Deserialize<Detail>(await DownloadContainerAndDecrypt(url, BCAT_QLAUNCH_TITLE_ID, BCAT_QLAUNCH_PASSPHRASE));
        }

        public static async Task<Topic> GetNewsTopic(string topicId, Country country, Language? language = null)
        {
            return await GetNewsTopic(topicId, new Country[] { country }, language);
        }

        public static async Task<Topic> GetNewsTopic(string topicId, Country[] countries, Language? language = null)
        {
            // Format the URL
            string url = BuildCountryAndLanguageUrl(string.Format(BCAT_TOPIC_URL, topicId), countries, language);

            // Fetch the data
            return MessagePackSerializer.Deserialize<Topic>(await DownloadContainerAndDecrypt(url, BCAT_QLAUNCH_TITLE_ID, BCAT_QLAUNCH_PASSPHRASE));
        }

        public static async Task<byte[]> GetNewsRaw(string url)
        {
            // Download and decrypt
            return await DownloadContainerAndDecrypt(url, BCAT_QLAUNCH_TITLE_ID, BCAT_QLAUNCH_PASSPHRASE);
        }

        public static async Task<Bcat.News.News> GetNews(string url)
        {
            // Download and decrypt
            byte[] rawNews = await GetNewsRaw(url);

            // Return the parsed news
            return Bcat.News.News.Deserialize(rawNews);
        }

        public static async Task<Topic> GetDataTopic(string topicId, string titleId, string passphrase)
        {
            // Format the URL
            string url = string.Format(BCAT_TOPIC_URL, topicId);

            // Fetch the data
            return MessagePackSerializer.Deserialize<Topic>(await DownloadContainerAndDecrypt(url, titleId, passphrase));
        }
        
        private static string BuildCountryAndLanguageUrl(string baseUrl, Country[] countries, Language? language = null)
        {
            string url = baseUrl;
            bool questionMarkPlaced = false;

            // Check if the language is specified
            if (language.HasValue)
            {
                url += "?l=" + language.Value.GetCode();
                questionMarkPlaced = true;
            }

            // Loop over every country
            foreach (Country country in countries)
            {
                // Check if the URL has a ? yet
                if (!questionMarkPlaced)
                {
                    url += "?";
                    questionMarkPlaced = true;
                }
                else
                {
                    url += "&";
                }

                // Append the country
                url += "c[]=" + country.ToBcatString();
            }

            return url;
        }

        public static async Task<byte[]> DownloadContainerAndDecrypt(string url, string titleId, string passphrase)
        {
            Container container = new Container(await DownloadFile(url));
            
            return container.GetDecryptedData(titleId, passphrase);
        }

        private static async Task<byte[]> DownloadFile(string url)
        {
            // Check if initialized
            if (!initialized)
            {
                throw new Exception("Cannot make requests to BCAT while uninitialized");
            }

            // Add the environment to the URL
            url = url.Replace("%", Configuration.LoadedConfiguration.CdnConfig.Environment);

            // Declare a retry counter
            int retries = 3;

            while (true)
            {
                try 
                {
                    // Create the request message
                    HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, url);
                    message.Headers.Add("X-Nintendo-DenebEdgeToken", await EdgeTokenManager.GetEdgeToken(EdgeTokenManager.QLAUNCH_CLIENT_ID));

                    // Send the request
                    using (HttpResponseMessage response = await httpClient.SendAsync(message))
                    using (HttpContent content = response.Content)
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            if (response.StatusCode == HttpStatusCode.Forbidden)
                            {
                                // Probably isn't recoverable without human intervention
                                retries = 0;

                                throw new Exception("Received 403 Forbidden from BCAT");
                            }
                            else
                            {
                                throw new Exception("BCAT request not a success (" + response.StatusCode + ")");
                            }
                        }

                        return await content.ReadAsByteArrayAsync();
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

                    Console.WriteLine("WARNING: BCAT request failed, waiting 5 seconds (retries remaining = " + retries + ")");
                    Console.WriteLine(e.ToString());

                    // Wait for 5 seconds
                    Thread.Sleep(5000);
                }
            }
        }

    }
}