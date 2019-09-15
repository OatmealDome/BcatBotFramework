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
using BcatBotFramework.Social.Discord.Notifications;
using System.Threading;
using BcatBotFramework.Social.Discord.Interactive;

namespace BcatBotFramework.Social.Discord
{
    public class DiscordBot
    {
        // Instances
        private static DiscordSocketClient DiscordClient = null;
        private static CommandService CommandService = null;
        private static readonly SemaphoreSlim InteractiveMessageSemaphore = new SemaphoreSlim(1, 1);
        private static readonly SemaphoreSlim InteractiveFlowSemaphore = new SemaphoreSlim(1, 1);
        private static uint InteractiveMessageJobCounter = 0;

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

        public static List<InteractiveFlow> ActiveInteractiveFlows
        {
            get;
            private set;
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

            // Set the counter
            InteractiveMessageJobCounter = 0;

            // Create the flows list
            ActiveInteractiveFlows = new List<InteractiveFlow>();

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
            ActiveInteractiveFlows = null;
            InteractiveMessageJobCounter = 0;
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
            if (socketMessage.Source == MessageSource.Webhook || socketMessage.Source == MessageSource.System)
            {
                return;
            }

            // Skip bots that aren't QA
            if (socketMessage.Source == MessageSource.Bot && socketMessage.Author.Id != 563718045806362642)
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
                //await commandContext.Channel.TriggerTypingAsync();

                // Execute the command
                IResult result = await CommandService.ExecuteAsync(commandContext, commandPosition, null);
                if (!result.IsSuccess)
                {
                    switch (result.Error)
                    {
                        case CommandError.UnknownCommand:
                            //await DiscordUtil.SendErrorMessageByLocalizedDescription(commandContext.Guild, commandContext.Channel, "discord.error.unknown_command");
                            //
                            //break;

                            return; // ignore
                        case CommandError.BadArgCount:
                            await DiscordUtil.SendErrorMessageByLocalizedDescription(commandContext.Guild, commandContext.Channel, "discord.error.bad_arguments");

                            break;
                        case CommandError.UnmetPrecondition:
                            // Get the PreconditionResult
                            PreconditionResult preconditionResult = (PreconditionResult)result;

                            // Check if the error reason contains a localizable
                            string description = result.ErrorReason;
                            if (result.ErrorReason.StartsWith("~loc"))
                            {
                                // Localize the error reason
                                await DiscordUtil.SendErrorMessageByLocalizedDescription(commandContext.Guild, commandContext.Channel, description.Split(',')[1]);
                            }
                            else
                            {
                                // Localize the error reason
                                await DiscordUtil.SendErrorMessageByDescription(commandContext.Guild, commandContext.Channel, result.ErrorReason);
                            }

                            break;
                        case CommandError.Exception:
                            // Get the IResult as an ExecuteResult
                            ExecuteResult executeResult = (ExecuteResult)result;

                            // Send the error message
                            await DiscordUtil.SendErrorMessageByException(commandContext.Guild, commandContext.Channel, commandContext.User, $"with command``{userMessage.Content}``", executeResult.Exception);

                            break;
                        default:
                            // Get the type
                            string type = (result.Error != null) ? result.Error.Value.GetType().Name : "Unknown";

                            // Send the error message
                            await DiscordUtil.SendErrorMessageByTypeAndMessage(commandContext.Guild, commandContext.Channel, type, result.ErrorReason);

                            break;
                    }
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
            else
            {
                // Acquire the semaphore
                await InteractiveMessageSemaphore.WaitAsync();

                // Check if this will match an interactive message
                // TODO a better way?
                IEnumerable<InteractiveMessage> messages = ActiveInteractiveMessages.Where(x => x.Channel.Id == socketMessage.Channel.Id
                            && x.User.Id == socketMessage.Author.Id).ToList();

                // Release the semaphore
                InteractiveMessageSemaphore.Release();

                foreach (InteractiveMessage interactiveMessage in messages)
                {
                    try
                    {
                        await interactiveMessage.TextMessageReceived(socketMessage);
                    }
                    catch (Exception e)
                    {
                        await DiscordUtil.SendErrorMessageByException(commandContext.Guild, commandContext.Channel, commandContext.User, $"in {interactiveMessage.GetType().Name}.TextMessageReceived()", e);
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

            // Acquire the semaphore
            await InteractiveMessageSemaphore.WaitAsync();

            // Check if this will match an interactive message
            // TODO a better way?
            IEnumerable<InteractiveMessage> messages = ActiveInteractiveMessages.Where(x => x.MessageId == message.Id).ToList();

            // Release the semaphore
            InteractiveMessageSemaphore.Release();

            // TODO a better way?
            foreach (InteractiveMessage interactiveMessage in messages)
            {
                try
                {
                    await interactiveMessage.ReactionAdded(reaction);
                }
                catch (Exception e)
                {
                    await DiscordUtil.SendErrorMessageByException(null, messageChannel, reaction.User.Value, $"in {interactiveMessage.GetType().Name}.ReactionAdded()", e);
                }
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

        public static SocketChannel GetChannel(ulong channelId)
        {
            return DiscordClient.GetChannel(channelId);
        }

        public static async Task SendNotificationAsync(Predicate<NotificationsSettings> shouldPost, string message = null, Dictionary<Language, Embed> localizedEmbeds = null)
        {
            // Make a copy of the list just in case it is modified while notifications are sent
            List<NotificationsSettings> guildList = new List<NotificationsSettings>(Configuration.LoadedConfiguration.DiscordConfig.NotificationsSettings);

            foreach (NotificationsSettings settings in guildList)
            {
                // Run the Predicate to see if a message should be sent
                if (!shouldPost(settings))
                {
                    continue;
                }

                // Get the guild
                SocketGuild socketGuild = DiscordBot.GetGuild(settings.GuildId);

                // Get the channel
                SocketTextChannel textChannel = socketGuild.GetTextChannel(settings.ChannelId);

                // Get the Embed if it exists
                Embed embed = (localizedEmbeds != null) ? localizedEmbeds[settings.GetSetting("language")] : null;

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
            // Acquire the semaphore
            await InteractiveMessageSemaphore.WaitAsync();

            // Send the initial message
            await message.SendInitialMessage(channel);

            // Add this to the active messages
            ActiveInteractiveMessages.Add(message);

            // Create a JobDataMap
            JobDataMap dataMap = new JobDataMap();
            dataMap.Put("messageInstance", message);

            // Create the timeout time
            DateTime timeoutTime = DateTime.Now.AddMinutes(Configuration.LoadedConfiguration.DiscordConfig.InteractiveMessageTimeout);

            // Schedule the cleanup job
            await QuartzScheduler.ScheduleJob<InteractiveMessageCleanupJob>("Normal" + ++InteractiveMessageJobCounter, timeoutTime, dataMap);

            // Release the semaphore
            InteractiveMessageSemaphore.Release();
        }

        public static async Task DeactivateInteractiveMessage(InteractiveMessage message)
        {
            // Acquire the semaphore
            await InteractiveMessageSemaphore.WaitAsync();

            // Check if this message is inactive
            if (!message.IsActive)
            {
                goto done;
            }

            // Set as inactive
            message.IsActive = false;

            // Add this to the active messages
            bool isSuccess = ActiveInteractiveMessages.Remove(message);

            // Clear all reactions if needed
            if (isSuccess)
            {
                await message.ClearReactions();
            }

done:
            // Release the semaphore
            InteractiveMessageSemaphore.Release();
        }

        public static async Task ActivateInteractiveFlow(InteractiveFlow interactiveFlow)
        {
            // Acquire the semaphore
            await InteractiveFlowSemaphore.WaitAsync();

            // Set the page to the root
            await interactiveFlow.SetPage(0);

            // Add this to the active messages
            ActiveInteractiveFlows.Add(interactiveFlow);

            // TODO: schedule cleanup job

            // Release the semaphore
            InteractiveFlowSemaphore.Release();
        }

        public static async Task DeactivateInteractiveFlow(InteractiveFlow interactiveFlow)
        {
            // Acquire the semaphore
            await InteractiveFlowSemaphore.WaitAsync();

            // Remove this from the active flows
            bool isSuccess = ActiveInteractiveFlows.Remove(interactiveFlow);

            // Deactivate the current interactive message if needed
            if (isSuccess && interactiveFlow.CurrentInteractiveMessage != null && interactiveFlow.CurrentInteractiveMessage.IsActive)
            {
                await DeactivateInteractiveMessage(interactiveFlow.CurrentInteractiveMessage);
            }
            
            // Release the semaphore
            InteractiveFlowSemaphore.Release();
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