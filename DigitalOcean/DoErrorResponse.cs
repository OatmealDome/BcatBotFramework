using Newtonsoft.Json;

namespace DigitalOcean
{
    public class DoErrorResponse : DoResponse
    {
        [JsonProperty("id")]
        public string Id
        {
            get;
            set;
        }

        [JsonProperty("message")]
        public string Message
        {
            get;
            set;
        }
        
    }
}