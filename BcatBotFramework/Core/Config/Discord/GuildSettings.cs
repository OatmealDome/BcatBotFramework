using Nintendo.Bcat;

namespace BcatBotFramework.Core.Config.Discord
{
    public class GuildSettings
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