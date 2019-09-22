using System.Collections.Generic;
using Newtonsoft.Json;

namespace BcatBotFramework.Social.Discord.Settings
{
    public abstract class DynamicSettingsData
    {
        [JsonProperty("Data")]
        private Dictionary<string, dynamic> Data;

        public DynamicSettingsData()
        {
            Data = new Dictionary<string, dynamic>();
        }

        public dynamic GetSetting(string name)
        {
            if (Data.TryGetValue(name, out dynamic val))
            {
                return val;
            }

            return null;
        }

        public void SetSetting(string name, dynamic val)
        {
            Data[name] = val;
        }
        
    }
}