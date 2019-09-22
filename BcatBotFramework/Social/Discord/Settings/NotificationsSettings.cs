using System.Collections.Generic;
using Newtonsoft.Json;

namespace BcatBotFramework.Social.Discord.Settings
{
    public class NotificationsSettings
    {
        [JsonProperty("Guild")]
        public ulong GuildId
        {
            get;
            private set;
        }

        [JsonProperty("Channel")]
        public ulong ChannelId
        {
            get;
            private set;
        }

        [JsonProperty("Data")]
        private Dictionary<string, dynamic> Data;

        public NotificationsSettings(ulong guild, ulong channel)
        {
            GuildId = guild;
            ChannelId = channel;
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