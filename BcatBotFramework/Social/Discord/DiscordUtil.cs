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
using BcatBotFramework.Core.Config.Discord;
using BcatBotFramework.Social.Discord;
using BcatBotFramework.Internationalization;
using System.Reflection;

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
            // Find every guild that registered a channel we can't write to
            List<GuildSettings> badGuilds = Configuration.LoadedConfiguration.DiscordConfig.GuildSettings.Where(guildSettings =>
            {
                // Get the SocketGuild
                SocketGuild socketGuild = DiscordBot.GetGuild(guildSettings.GuildId);

                // Get the channel
                SocketGuildChannel channel = socketGuild.GetChannel(guildSettings.TargetChannelId);

                // Check if the channel no longer exists
                if (channel == null)
                {
                    return true;
                }

                // Get the Permissions
                ChannelPermissions permissions = socketGuild.CurrentUser.GetPermissions(channel);

                // Check that we can write to this channel
                return !permissions.Has(ChannelPermission.SendMessages);
            }).ToList();

            // Skip deregistration if there are a large number of guilds to deregister
            if (badGuilds.Count > 5 && !ignoreExcessiveAmount)
            {
                await DiscordBot.LoggingChannel.SendMessageAsync($"**[DiscordUtil]** Skipping deregistration, there seems to be an excessive amount of guilds to deregister ({badGuilds.Count})");

                return;
            }
            
            foreach (GuildSettings guildSettings in badGuilds)
            {
                // Deregister the guild
                await DeregisterGuild(guildSettings);
            }
        }

        public static async Task DeregisterGuild(GuildSettings guildSettings)
        {
            // Remove the GuildSettings
            Configuration.LoadedConfiguration.DiscordConfig.GuildSettings.Remove(guildSettings);

            // Get the guild
            SocketGuild socketGuild = DiscordBot.GetGuild(guildSettings.GuildId);

            // Send a message to this server that their guild has been deregistered
            Embed embed = new EmbedBuilder()
                .WithTitle("Warning")
                .WithDescription(Localizer.Localize("discord.guild.deregister", guildSettings.DefaultLanguage))
                .WithColor(Color.Orange)
                .Build();
            
            await DiscordBot.SendMessageToFirstWritableChannel(socketGuild, embed: embed);

            await DiscordBot.LoggingChannel.SendMessageAsync($"**[Guild]** Deregistered \"{socketGuild.Name}\" ({socketGuild.Id})");
        }
        
    }
}