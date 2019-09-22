using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nintendo.Bcat;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using BcatBotFramework.Core.Config;
using BcatBotFramework.Social.Discord;
using BcatBotFramework.Internationalization;
using System.Reflection;
using BcatBotFramework.Social.Discord.Settings;

namespace BcatBotFramework.Social.Discord
{
    public class DiscordUtil
    {
        private static string LOCAL_DIRECTORY = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string LOCAL_EXCEPTION_LOGS_DIRECTORY = Path.Combine(LOCAL_DIRECTORY, "ExceptionLogs");

        public static async Task HandleException(Exception exception, string source, IGuild sourceGuild = null, ISocketMessageChannel sourceChannel = null, IUser sourceUser = null)
        {
            // Save a strack trace to a file
            string filePath = Path.Combine(LOCAL_EXCEPTION_LOGS_DIRECTORY, $"ex_{DateTime.Now.Ticks}.txt");

            // Write out the stack trace
            File.WriteAllText(filePath, exception.ToString());

            // Create a ping list
            string pingList = " ";

            // Only in production
            if (Configuration.LoadedConfiguration.IsProduction)
            {
                foreach (ulong id in Configuration.LoadedConfiguration.DiscordConfig.AdministratorIds)
                {
                    pingList += $"<@{id}> ";
                }
            }

            // Begin building an embed
            EmbedBuilder embedBuilder = new EmbedBuilder()
                .WithTitle("Exception")
                .AddField("Message", exception.Message)
                .AddField("Source", source)
                .AddField("Stack Trace file", $"``{Path.GetFileName(filePath)}``");

            if (sourceGuild != null)
            {
                embedBuilder.AddField("Guild", $"{sourceGuild.Name} ({sourceGuild.Id})");
            }

            if (sourceChannel != null)
            {
                embedBuilder.AddField("Channel", $"#{sourceChannel.Name} ({sourceChannel.Id})");
            }

            if (sourceUser != null)
            {
                embedBuilder.AddField("User", $"{sourceUser.Username}#{sourceUser.Discriminator} ({sourceUser.Id})");
            }

            // Send a message to the logging channel
            await DiscordBot.LoggingChannel.SendMessageAsync("**[Exception]** " + pingList, embed: embedBuilder.Build());
        }

        public static async Task SendErrorMessageByException(IGuild guild, ISocketMessageChannel channel, IUser user, string source, Exception exception)
        {
            // Try to get the Exception as a LocalizedException
            LocalizedException localizedException = exception as LocalizedException;

            // Check if this really is a LocalizedException
            if (localizedException != null)
            {
                // Send the error message
                await SendErrorMessageByLocalizedDescription(guild, channel, localizedException.Message);
            }
            else
            {
                // Notify the logging channel
                await DiscordUtil.HandleException(exception, source, guild, channel, user);
                
                // Send the error message
                await SendErrorMessageByTypeAndMessage(guild, channel, exception.GetType().Name, exception.Message, true);
            }
        }

        public static async Task SendErrorMessageByTypeAndMessage(IGuild guild, ISocketMessageChannel channel, string type, string message, bool isException = false)
        {
            // Get the language for this Guild
            Language language = DiscordUtil.GetDefaultLanguage(guild);

            // Start building an Embed
            EmbedBuilder embedBuilder = new EmbedBuilder()
                .WithTitle(Localizer.Localize("discord.error", language))
                .WithColor(Color.Red)
                .WithDescription(Localizer.Localize($"discord.error.{(isException ? "exception" : "unknown")}", language))
                .AddField(Localizer.Localize("discord.error.type", language), type)
                .AddField(Localizer.Localize("discord.error.message", language), message);

            await channel.SendMessageAsync(embed: embedBuilder.Build());
        }

        public static async Task SendErrorMessageByLocalizedDescription(IGuild guild, ISocketMessageChannel channel, string localizable)
        {
            // Get the language for this Guild
            Language language = DiscordUtil.GetDefaultLanguage(guild);

            // Start building an Embed
            EmbedBuilder embedBuilder = new EmbedBuilder()
                .WithTitle(Localizer.Localize("discord.error", language))
                .WithColor(Color.Red)
                .WithDescription(Localizer.Localize(localizable, language));

            await channel.SendMessageAsync(embed: embedBuilder.Build());
        }

        public static async Task SendErrorMessageByDescription(IGuild guild, ISocketMessageChannel channel, string description)
        {
            // Get the language for this Guild
            Language language = DiscordUtil.GetDefaultLanguage(guild);

            // Start building an Embed
            EmbedBuilder embedBuilder = new EmbedBuilder()
                .WithTitle(Localizer.Localize("discord.error", language))
                .WithColor(Color.Red)
                .WithDescription(description);

            await channel.SendMessageAsync(embed: embedBuilder.Build());
        }

        public static Language GetDefaultLanguage(IGuild guild, string specifiedLanguage = null)
        {
            // Check if a language was specified
            if (specifiedLanguage == null)
            {
                // Check if this is not a DM
                if (guild != null)
                {
                    // Attempt to get the GuildSettings for this guild
                    GuildSettings guildSettings = Configuration.LoadedConfiguration.DiscordConfig.GuildSettings.Where(x => x.GuildId == guild.Id).FirstOrDefault();

                    // Check if there is a GuildSettings
                    if (guildSettings != null)
                    {
                        // Set the target language
                        // TODO: Don't use this
                        return guildSettings.DefaultLanguage;
                    }
                }

                // Default to en-US
                return Language.EnglishUS;
            }
            else
            {
                try
                {
                    return LanguageExtensions.FromCode(specifiedLanguage);
                }
                catch (Exception)
                {
                    throw new LocalizedException("discord.error.bad_code");
                }
            }
        }

        public static async Task ProcessJoinedGuild(SocketGuild socketGuild)
        {
            // Build an Embed
            Embed embed = new EmbedBuilder()
                .WithTitle("Welcome")
                .WithDescription(Localizer.Localize("discord.guild.join", Language.EnglishUS))
                .WithColor(Color.Blue)
                .Build();

            await DiscordBot.SendMessageToFirstWritableChannel(socketGuild, embed: embed);

            await DiscordBot.LoggingChannel.SendMessageAsync($"**[Guild]** Joined \"{socketGuild.Name}\" ({socketGuild.Id})");
        }

        public static async Task ProcessLeftGuild(ulong id, string name = null)
        {
            // Remove any GuildSettings for this guild
            Configuration.LoadedConfiguration.DiscordConfig.GuildSettings.RemoveAll((guildSettings) =>
            {
                return guildSettings.GuildId == id;
            });

            Configuration.LoadedConfiguration.Write();

            if (name != null)
            {
                await DiscordBot.LoggingChannel.SendMessageAsync($"**[Guild]** Left \"{name}\" ({id})");
            }
            else
            {
                await DiscordBot.LoggingChannel.SendMessageAsync($"**[Guild]** Left guild with ID {id}");
            }
        }

        public static async Task FindBadGuilds(bool ignoreExcessiveAmount = false)
        {
            // Create a copy of the GuildSettings
            List<GuildSettings> allGuildSettings = new List<GuildSettings>(Configuration.LoadedConfiguration.DiscordConfig.GuildSettings);

            // Create a list for deregistration candidates
            List<Tuple<NotificationsSettings, SocketGuild>> deregistrationCandidates = new List<Tuple<NotificationsSettings, SocketGuild>>();

            foreach (GuildSettings guildSettings in allGuildSettings)
            {
                foreach (NotificationsSettings channelSettings in guildSettings.ChannelSettings)
                {
                    // Get the channel
                    SocketGuildChannel channel = (SocketGuildChannel)DiscordBot.GetChannel(channelSettings.ChannelId);

                    // Check if the channel no longer exists
                    if (channel == null)
                    {
                        continue;
                    }

                    // Get the Permissions
                    ChannelPermissions permissions = channel.Guild.CurrentUser.GetPermissions(channel);

                    // Check if we can't write to this channel
                    if (!permissions.Has(ChannelPermission.SendMessages))
                    {
                        deregistrationCandidates.Add(new Tuple<NotificationsSettings, SocketGuild>(channelSettings, channel.Guild));
                    }
                }
            }

            // Skip deregistration if there are a large number of guilds to deregister
            if (deregistrationCandidates.Count > 5 && !ignoreExcessiveAmount)
            {
                await DiscordBot.LoggingChannel.SendMessageAsync($"**[DiscordUtil]** Skipping deregistration, there seems to be an excessive amount of guilds to deregister ({deregistrationCandidates.Count})");

                return;
            }

            foreach (Tuple<NotificationsSettings, SocketGuild> tuple in deregistrationCandidates)
            {
                // Remove the channel settings
                Configuration.LoadedConfiguration.DiscordConfig.GuildSettings.Where(g => g.ChannelSettings.Contains(tuple.Item1)).First().ChannelSettings.Remove(tuple.Item1);

                // Send a message to this server that their guild has been deregistered
                Embed embed = new EmbedBuilder()
                    .WithTitle("Warning")
                    .WithDescription(Localizer.Localize("discord.guild.deregister", tuple.Item1.GetSetting("language")))
                    .WithColor(Color.Orange)
                    .Build();
                
                await DiscordBot.SendMessageToFirstWritableChannel(tuple.Item2, embed: embed);

                await DiscordBot.LoggingChannel.SendMessageAsync($"**[Guild]** Deregistered \"{tuple.Item2.Name}\" ({tuple.Item2.Id})");
            }
        }
        
    }
}