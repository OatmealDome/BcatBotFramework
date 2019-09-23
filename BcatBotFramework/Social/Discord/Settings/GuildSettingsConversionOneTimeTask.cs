using System.Threading.Tasks;
using BcatBotFramework.Core;
using BcatBotFramework.Core.Config;

namespace BcatBotFramework.Social.Discord.Settings
{
    public class GuildSettingsConversionOneTimeTask : OneTimeTask
    {
        protected override Task Run()
        {
            // Loop over every GuildSettings
            foreach (GuildSettings guildSettings in Configuration.LoadedConfiguration.DiscordConfig.GuildSettings)
            {
                // Add the default language to the settings data
                guildSettings.SetSetting("default_language", guildSettings.DefaultLanguage);

                // Create a new DynamicSettingsData instance and add the language
                DynamicSettingsData channelSettings = new DynamicSettingsData();
                channelSettings.SetSetting("language", guildSettings.DefaultLanguage);

                // Create a new DynamicSettingsData instance and add it to the GuildSettings
                guildSettings.ChannelSettings.TryAdd(guildSettings.TargetChannelId, channelSettings);
            }

            return Task.FromResult(0);
        }

    }
}