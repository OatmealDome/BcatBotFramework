using System.Threading.Tasks;
using BcatBotFramework.Core;
using BcatBotFramework.Core.Config;

namespace BcatBotFramework.Social.Discord.Settings
{
    public abstract class GuildSettingsConversionOneTimeTask : OneTimeTask
    {
        protected override async Task Run()
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

                // Do post conversion things
                await DoPostConversionThings(guildSettings, channelSettings);
            }
        }

        protected abstract Task DoPostConversionThings(GuildSettings guildSettings, DynamicSettingsData channelSettings);

    }
}