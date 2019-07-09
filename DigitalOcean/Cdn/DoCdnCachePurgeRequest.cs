using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace DigitalOcean.Cdn
{
    public class DoCdnCachePurgeRequest : DoRequest
    {
        public override HttpMethod Method => HttpMethod.Delete;

        protected override string Query => $"/cdn/endpoints/{EndpointId}/cache";

        public override Type ResponseType => typeof(DoNoContentResponse);

        public DoCdnCachePurgeRequest(string endpointId, List<string> files)
        {
            EndpointId = endpointId;
            Files = files;
        }

        public DoCdnCachePurgeRequest(string endpointId, string file)
        {
            EndpointId = endpointId;
            Files = new List<string>()
            {
                file
            };
        }

        [JsonIgnore]
        public string EndpointId
        {
            get;
            private set;
        }

        [JsonProperty("files")]
        public List<string> Files
        {
            get;
            private set;
        }

    }
}