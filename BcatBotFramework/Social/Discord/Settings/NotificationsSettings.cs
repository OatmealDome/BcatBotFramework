using Newtonsoft.Json;

namespace BcatBotFramework.Social.Discord.Settings
{
    public class NotificationsSettings : DynamicSettingsData
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

        public NotificationsSettings(ulong guild, ulong channel) : base()
        {
            GuildId = guild;
            ChannelId = channel;
        }

    }

}