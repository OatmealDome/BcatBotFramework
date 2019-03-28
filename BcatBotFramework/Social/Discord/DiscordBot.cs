using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Nintendo.Bcat;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Quartz;
using BcatBotFramework.Internationalization;
using BcatBotFramework.Core.Config;
using BcatBotFramework.Core.Config.Discord;
using BcatBotFramework.Social.Discord;
using BcatBotFramework.Scheduler;
using BcatBotFramework.Scheduler.Job;

namespace BcatBotFramework.Social.Discord
{
    public class DiscordBot
    {
        // Instances
        private static DiscordSocketClient DiscordClient = null;
        private static CommandService CommandService = null;

        // Members
        public static bool IsReady
        {
            get;
            set;
        }

        public static SocketTextChannel LoggingChannel
        {
            get;
            set;
        }
        
        public static PlayingState PlayingState
        {
            get;
            set;
        }

        public static List<InteractiveMessage> ActiveInteractiveMessages
        {
            get;
            set;
        }

        public static async Task Initialize()
        {
            // Check if we're already initialized
            if (DiscordClient != null)
            {
                throw new Exception("Can't initialize while already initialized");
            }

            // Reset ready flag
            IsReady = false;

            // Create a new configuration
            DiscordSocketConfig socketConfig = new DiscordSocketConfig()
            {
                MessageCacheSize = Configuration.LoadedConfiguration.DiscordConfig.MessageCacheSize
            };

            // Create a new DiscordSocketClient
            DiscordClient = new DiscordSocketClient(socketConfig);

            // Create a new CommandService
            CommandService = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
            });

            // Register callbacks
            DiscordClient.Ready += ClientReady;
            DiscordClient.MessageReceived += MessageReceived;
            DiscordClient.ReactionAdded += ReactionAdded;
            DiscordClient.JoinedGuild += JoinedGuild;
            DiscordClient.LeftGuild += LeftGuild;

            // Register all commands
            await CommandService.AddModulesAsync(Assembly.GetEntryAssembly(), null);

            // Create the interactive messages list
            ActiveInteractiveMessages = new List<InteractiveMessage>();

            // Log in
            await DiscordClient.LoginAsync(TokenType.Bot, Configuration.LoadedConfiguration.DiscordConfig.Token);
            await DiscordClient.StartAsync();
        }

        public static async Task Dispose()
        {
            // Check if we're unintialized
            if (DiscordClient == null)
            {
                throw new Exception("Cannot dispose while uninitialized");
            }

            // Dispose the DiscordSocketClient
            await DiscordClient.LogoutAsync();
            DiscordClient.Dispose();

            // Reset fields
            DiscordClient = null;
            IsReady = false;
            ActiveInteractiveMessages = null;
        }

        private static async Task ClientReady()
        {
            Console.WriteLine("DiscordBot started successfully");

            // Fetch the logging channel
            LoggingChannel = GetChannel(Configuration.LoadedConfiguration.DiscordConfig.LoggingTargetChannel);

            // Set that we're ready
            IsReady = true;

            // Set the shown game
            PlayingState = (PlayingState)0;

            // Schedule jobs
            await QuartzScheduler.ScheduleJob<DiscordPlayingAlternatorJob>("Normal", Configuration.LoadedConfiguration.DiscordConfig.AlternatorRate);
        }

        private static async Task MessageReceived(SocketMessage socketMessage)
        {
            // Skip if this is not a real user
            if (socketMessage.Source == MessageSource.Bot || socketMessage.Source == MessageSource.Webhook || socketMessage.Source == MessageSource.System)
            {
                return;
            }

            // Get the SocketUserMessage
            SocketUserMessage userMessage = socketMessage as SocketUserMessage;

            // Create the CommandContext
            SocketCommandContext commandContext = new SocketCommandContext(DiscordClient, userMessage);

            // Ignore empty messages and bots
            if (commandContext.Message == null || commandContext.Message.Content.Length == 0)
            {
                return;
            }

            // Check if this message has a command
            int commandPosition = 0;
            if (userMessage.HasStringPrefix(Configuration.LoadedConfiguration.DiscordConfig.CommandPrefix, ref commandPosition))
            {
                // Trigger typing
                await commandContext.Channel.TriggerTypingAsync();

                // Execute the command
                IResult result = await CommandService.ExecuteAsync(commandContext, commandPosition, null);
                if (!result.IsSuccess)
                {
                    // Get the language for this Guild
                    Language language = DiscordUtil.GetDefaultLanguage(commandContext);

                    // Start building an Embed
                    EmbedBuilder embedBuilder = new EmbedBuilder()
                        .WithTitle(Localizer.Localize("Error", language))
                        .WithColor(Color.Red);

                    switch (result.Error)
                    {
                        case CommandError.UnknownCommand:
                            embedBuilder.WithDescription(Localizer.Localize("Unknown command. For a list of commands, see ``ssbu.help``.", language));

                            break;
                        case CommandError.BadArgCount:
                            embedBuilder.WithDescription(Localizer.Localize("Not enough arguments. See ``ssbu.help`` for command help.", language));

                            break;
                        case CommandError.UnmetPrecondition:
                            // Get the PreconditionResult
                            PreconditionResult preconditionResult = (PreconditionResult)result;

                            // Set the description
                            embedBuilder.WithDescription(Localizer.Localize(result.ErrorReason, language));

                            break;
                        case CommandError.Exception:
                            // Get the IResult as an ExecuteResult
                            ExecuteResult executeResult = (ExecuteResult)result;

                            // Try to get the Exception as a LocalizedException
                            LocalizedException localizedException = executeResult.Exception as LocalizedException;

                            // Check if this really is a LocalizedException
                            if (localizedException != null)
                            {
                                embedBuilder.WithDescription(Localizer.Localize(localizedException.Message, language));
                            }
                            else
                            {
                                // Notify the logging channel
                                await DiscordUtil.HandleException(executeResult.Exception, $"with command ``{userMessage.Content}``", commandContext.Guild, commandContext.Channel, commandContext.User);
                                
                                embedBuilder.WithDescription(Localizer.Localize("An error has occurred. SSBUBot's owners have been notified.", language));
                                embedBuilder.AddField(Localizer.Localize("Type", language), executeResult.Exception.GetType().Name);
                                embedBuilder.AddField(Localizer.Localize("Message", language), executeResult.Exception.Message);
                            }
                            
                            break;
                        default:
                            embedBuilder.WithDescription(Localizer.Localize("An unknown error has occurred.", language));
                            embedBuilder.AddField(Localizer.Localize("Type", language), result.Error);
                            embedBuilder.AddField(Localizer.Localize("Message", language), result.ErrorReason);
                            
                            break;
                    }

                    await commandContext.Channel.SendMessageAsync(embed: embedBuilder.Build());
                }
                else
                {
                    // Declare a variable to hold the length
                    int length;

                    // Get the index of the first space
                    int spaceIdx = userMessage.Content.IndexOf(' ');

                    // Check if there is no space
                    if (spaceIdx == -1)
                    {
                        // Default to the command string length
                        length = userMessage.Content.Length - commandPosition;
                    }
                    else
                    {
                        // Get the length of the string in between the space and the command
                        length = spaceIdx - commandPosition;
                    }

                    // Get the command
                    string command = userMessage.Content.Substring(commandPosition, length);

                    // Check if this is not command stats
                    if (command != "commandstats")
                    {
                         // Increment this command in the statistics
                        Configuration.LoadedConfiguration.DiscordConfig.CommandStatistics.AddOrUpdate(command, 1, (cmd, val) => val + 1);
                    }
                }
            }
        }

        private static async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel messageChannel, SocketReaction reaction)
        {
            // Skip if the reaction is from ourselves
            if (reaction.UserId == DiscordClient.CurrentUser.Id)
            {
                return;
            }

            // TODO a better way?
            foreach (InteractiveMessage interactiveMessage in ActiveInteractiveMessages.Where(x => x.MessageId == message.Id))
            {
                await interactiveMessage.ReactionAdded(reaction);
            }
        }

        private static async Task JoinedGuild(SocketGuild socketGuild)
        {
            await DiscordUtil.ProcessJoinedGuild(socketGuild);
        }

        private static async Task LeftGuild(SocketGuild socketGuild)
        {
            await DiscordUtil.ProcessLeftGuild(socketGuild.Id, socketGuild.Name);
        }

        public static IReadOnlyCollection<SocketGuild> GetGuilds()
        {
            return DiscordClient.Guilds;
        }

        public static SocketGuild GetGuild(ulong id)
        {
            return DiscordClient.GetGuild(id);
        }

        public static SocketTextChannel GetChannel(GuildSettings guildSettings)
        {
            return GetChannel(guildSettings.GuildId, guildSettings.TargetChannelId);
        }

        public static SocketTextChannel GetChannel(ulong guildId, ulong channelId)
        {
            return GetGuild(guildId).GetTextChannel(channelId);
        }

        public static async Task SendNotificationAsync(string message = null, Dictionary<Language, Embed> localizedEmbeds = null)
        {
            foreach (GuildSettings guildSettings in Configuration.LoadedConfiguration.DiscordConfig.GuildSettings)
            {
                // Get the guild
                SocketGuild socketGuild = DiscordBot.GetGuild(guildSettings.GuildId);

                // Get the channel
                SocketTextChannel textChannel = socketGuild.GetTextChannel(guildSettings.TargetChannelId);

                // Get the Embed if it exists
                Embed embed = (localizedEmbeds != null) ? localizedEmbeds[guildSettings.DefaultLanguage] : null;

                // Send the message if possible
                try
                {
                    await textChannel.SendMessageAsync(text: message, embed: embed);
                }
                catch (Exception exception)
                {
                    await DiscordUtil.HandleException(exception, "in ``SendNotificationsAsync()``", socketGuild, textChannel);
                }
            }
        }

        public static async Task SendInteractiveMessageAsync(ISocketMessageChannel channel, InteractiveMessage message)
        {
            // Send the initial message
            await message.SendInitialMessage(channel);

            // Add this to the active messages
            ActiveInteractiveMessages.Add(message);

            // Create a JobDataMap
            JobDataMap dataMap = new JobDataMap();
            dataMap.Put("messageId", (object)message.MessageId);

            // Create the timeout time
            DateTime timeoutTime = DateTime.Now.AddMinutes(Configuration.LoadedConfiguration.DiscordConfig.InteractiveMessageTimeout);

            // Schedule the cleanup job
            await QuartzScheduler.ScheduleJob<InteractiveMessageCleanupJob>("Normal" + message.MessageId, timeoutTime, dataMap);
        }

        public static async Task SendMessageToFirstWritableChannel(SocketGuild socketGuild, string message = null, Embed embed = null)
        {
            // Get the first channel in the list we can write to
            SocketTextChannel textChannel = socketGuild.TextChannels
                .Where(x => socketGuild.CurrentUser.GetPermissions(x).Has(ChannelPermission.SendMessages))
                .OrderBy(x => x.Position)
                .FirstOrDefault();
            
            // Check if a channel exists
            if (textChannel != null)
            {
                await textChannel.SendMessageAsync(text: message, embed: embed);
            }
        }

        public static async Task SetGameAsync(string game)
        {
            await DiscordClient.SetGameAsync(game);
        }

    }
}