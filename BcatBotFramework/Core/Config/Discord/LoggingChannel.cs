using Nintendo.Bcat;

namespace BcatBotFramework.Core.Config.Discord
{
    public class LoggingChannel
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
        
    }
}