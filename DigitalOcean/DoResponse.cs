using Newtonsoft.Json;

namespace DigitalOcean
{
    [JsonObject]
    public abstract class DoResponse
    {
        [JsonIgnore]
        public bool IsError
        {
            get
            {
                return this is DoErrorResponse;
            }
        }
    }
    
}