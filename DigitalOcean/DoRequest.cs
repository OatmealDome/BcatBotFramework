using System;
using System.Net.Http;
using Newtonsoft.Json;

namespace DigitalOcean
{
    [JsonObject]
    public abstract class DoRequest
    {
        [JsonIgnore]
        public abstract HttpMethod Method
        {
            get;
        }

        [JsonIgnore]
        protected abstract string Query
        {
            get;
        }

        [JsonIgnore]
        public abstract Type ResponseType
        {
            get;
        }

        [JsonIgnore]
        public string Url
        {
            get
            {
                return $"https://api.digitalocean.com/v2{Query}";
            }
        }
        
    }
}