using Newtonsoft.Json;

namespace BcatBotFramework.Social.Discord.Settings
{
    public class ChannelSettings : DynamicSettingsData
    {
        [JsonProperty("Channel")]
        public ulong ChannelId
        {
            get;
            private set;
        }

        public ChannelSettings(ulong channel) : base()
        {
            ChannelId = channel;
        }

    }

}