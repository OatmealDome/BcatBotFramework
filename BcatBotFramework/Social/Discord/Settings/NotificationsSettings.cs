using Newtonsoft.Json;

namespace BcatBotFramework.Social.Discord.Settings
{
    public class NotificationsSettings : DynamicSettingsData
    {
        [JsonProperty("Channel")]
        public ulong ChannelId
        {
            get;
            private set;
        }

        public NotificationsSettings(ulong channel) : base()
        {
            ChannelId = channel;
        }

    }

}