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

                // Create a new NotificationSettings instance
                NotificationsSettings notificationsSettings = new NotificationsSettings(guildSettings.TargetChannelId);

                // Add it to the GuildSettings
                guildSettings.ChannelSettings.Add(notificationsSettings);
            }

            return Task.FromResult(0);
        }

    }
}