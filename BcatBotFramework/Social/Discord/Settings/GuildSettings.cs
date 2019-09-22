using Nintendo.Bcat;

namespace BcatBotFramework.Social.Discord.Settings
{
    public class GuildSettings : DynamicSettingsData
    {
        public ulong GuildId
        {
            get;
            set;
        }

        public ulong TargetChannelId
        {
            get;
            set;
        }

        public Language DefaultLanguage
        {
            get;
            set;
        }
        
    }
}