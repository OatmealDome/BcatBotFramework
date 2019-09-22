using System.Collections.Concurrent;
using System.Collections.Generic;
using Nintendo.Bcat;
using BcatBotFramework.Core.Config.Discord;
using BcatBotFramework.Social.Discord.Settings;

namespace BcatBotFramework.Core.Config
{
    public class DiscordConfig : ISubConfiguration
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

        public LoggingChannel LoggingTargetChannel
        {
            get;
            set;
        }

        public List<GuildSettings> GuildSettings
        {
            get;
            set;
        }

        public List<NotificationsSettings> NotificationsSettings
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

        public void SetDefaults()
        {
            Token = "cafebabe";
            ClientId = 0;
            Permissions = 322624;
            AdministratorIds = new List<ulong>()
            {
                112966101368901632, // OatmealDome
                180994059542855681 // Simonx22
            };
            LoggingTargetChannel = new LoggingChannel()
            {
                GuildId = 194559160572968961, // OatmealDome Test Server
                ChannelId = 194559160572968961, // #general
            };
            GuildSettings = new List<GuildSettings>();
            NotificationsSettings = new List<NotificationsSettings>();
            CommandPrefix = "test.";
            AlternatorRate = 10;
            MessageCacheSize = 10;
            InteractiveMessageTimeout = 5; // minutes
            CommandStatistics = new ConcurrentDictionary<string, ulong>();
        }

    }
}