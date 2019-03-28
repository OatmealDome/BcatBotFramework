using System.Collections.Concurrent;
using System.Collections.Generic;
using Nintendo.Bcat;
using BcatBotFramework.Core.Config.Discord;

namespace BcatBotFramework.Core.Config
{
    public class DiscordConfig
    {
        public string Token
        {
            get;
            set;
        }

        public ulong ClientId
        {
            get;
            set;
        }

        public uint Permissions
        {
            get;
            set;
        }

        public List<ulong> AdministratorIds
        {
            get;
            set;
        }

        public GuildSettings LoggingTargetChannel
        {
            get;
            set;
        }

        public List<GuildSettings> GuildSettings
        {
            get;
            set;
        }

        public string CommandPrefix
        {
            get;
            set;
        }

        public int AlternatorRate
        {
            get;
            set;
        }

        public int MessageCacheSize
        {
            get;
            set;
        }

        public int InteractiveMessageTimeout
        {
            get;
            set;
        }

        public ConcurrentDictionary<string, ulong> CommandStatistics
        {
            get;
            set;
        }
        
    }
}