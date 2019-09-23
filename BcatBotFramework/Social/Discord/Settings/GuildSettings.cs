using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public ConcurrentDictionary<ulong, DynamicSettingsData> ChannelSettings
        {
            get;
            set;
        }

        public GuildSettings(ulong guildId)
        {
            GuildId = guildId;
            ChannelSettings = new ConcurrentDictionary<ulong, DynamicSettingsData>();
        }

        // Legacy fields start
        // Only here for conversion purposes - should never be used by applications
        // Will be removed when all applications have been updated

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

        // Legacy fields end
        
    }
}