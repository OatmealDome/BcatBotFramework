using System.Threading.Tasks;
using BcatBotFramework.Core;
using BcatBotFramework.Core.Config;
using BcatBotFramework.Core.Config.Discord;

namespace BcatBotFramework.Social.Discord.Notifications
{
    public class GuildSettingsConversionOneTimeTask : OneTimeTask
    {
        protected override Task Run()
        {
            // Loop over every GuildSettings
            foreach (GuildSettings guildSettings in Configuration.LoadedConfiguration.DiscordConfig.GuildSettings)
            {
                // Create a new NotificationSettings instance
                NotificationsSettings notificationsSettings = new NotificationsSettings(guildSettings.GuildId, guildSettings.TargetChannelId);

                // Set the language
                notificationsSettings.SetSetting("language", guildSettings.DefaultLanguage);

                // Add this instance
                Configuration.LoadedConfiguration.DiscordConfig.NotificationsSettings.Add(notificationsSettings);
            }

            return Task.FromResult(0);
        }

    }
}