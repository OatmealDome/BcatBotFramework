using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BcatBotFramework.Core.Config;
using BcatBotFramework.Internationalization;
using BcatBotFramework.Social.Discord.Interactive;
using BcatBotFramework.Social.Discord.Settings;
using Discord;
using Discord.WebSocket;
using Nintendo.Bcat;

namespace BcatBotFramework.Social.Discord.Interactive.Setup
{
    public class SetupEnterChannelMessage : InteractiveMessage
    {
        private static IEmote EMOTE_EXIT = new Emoji("\u274C"); // X

        private readonly SetupFlow SetupFlow;

        public SetupEnterChannelMessage(SetupFlow flow, IUser user) : base(user)
        {
            SetupFlow = flow;
        }

        public SetupEnterChannelMessage(SetupFlow flow, IUser user, IUserMessage message) : base(user, message)
        {
            SetupFlow = flow;
        }

        public override async Task AddReactions()
        {
            await TargetMessage.AddReactionAsync(EMOTE_EXIT);
        }

        public override MessageProperties CreateMessageProperties()
        {
            return new MessageProperties()
            {
                Content = null,
                Embed = new EmbedBuilder()
                            .WithTitle(Localizer.Localize("discord.setup.enter_channel.title", SetupFlow.DefaultLanguage))
                            .WithDescription($"{Localizer.Localize("discord.setup.enter_channel.prompt", SetupFlow.DefaultLanguage)}\n\n{EMOTE_EXIT.Name} {Localizer.Localize("discord.setup.enter_channel.exit", SetupFlow.DefaultLanguage)}")
                            .Build()
            };
        }

        public override async Task<bool> HandleReaction(IEmote emote)
        {
            if (emote.Name == EMOTE_EXIT.Name)
            {
                SetupFlow.ModeSelectPrePromptLocalizable = "discord.setup.mode_select.pre_prompt.edits_not_saved";
                
                await SetupFlow.SetPage((int)SetupFlowPage.ModeSelect);
            }

            return false;
        }

        public override async Task<bool> HandleTextMessage(SocketMessage message)
        {
            // Get the guild
            SocketGuild guild = (message.Channel as SocketGuildChannel).Guild;

            // Parse the channel ID
            ulong channelId;
            try
            {
                channelId = MentionUtils.ParseChannel(message.Content);   
            }
            catch (ArgumentException)
            {
                await DiscordUtil.SendErrorMessageByLocalizedDescription(guild, this.Channel, "discord.setup.enter_channel.bad_channel");

                return false;
            }

            // Get the channel
            SocketChannel foundChannel = DiscordBot.GetChannel(channelId);

            // Check if it exists
            if (foundChannel == null)
            {
                await DiscordUtil.SendErrorMessageByLocalizedDescription(guild, this.Channel, "discord.setup.enter_channel.bad_channel");

                return false;
            }

            // Check this channel's guild
            SocketGuildChannel socketGuildChannel = foundChannel as SocketGuildChannel;

            // Check that this exists and that it is in the user's guild
            if (socketGuildChannel == null || socketGuildChannel.Guild.Id != SetupFlow.Guild.Id)
            {
                await DiscordUtil.SendErrorMessageByLocalizedDescription(guild, this.Channel, "discord.setup.enter_channel.bad_channel");

                return false;
            }

            // Get the text channel
            SocketTextChannel socketTextChannel = foundChannel as SocketTextChannel;

            // Check if this is a text channel
            if (socketTextChannel == null)
            {
                await DiscordUtil.SendErrorMessageByLocalizedDescription(guild, this.Channel, "discord.setup.enter_channel.bad_channel");

                return false;
            }

            // Check if this already exists as settings
            if (Configuration.LoadedConfiguration.DiscordConfig.GuildSettings.SelectMany(g => g.ChannelSettings.Keys).Where(id => id == channelId).Count() != 0)
            {
                await DiscordUtil.SendErrorMessageByLocalizedDescription(guild, this.Channel, "discord.setup.enter_channel.already_exists");

                return false;
            }

            // Get all required permissions
            List<ChannelPermission> requiredPermissions = new ChannelPermissions(Configuration.LoadedConfiguration.DiscordConfig.Permissions).ToList();

            // Get current permissions for this channel
            ChannelPermissions channelPermissions = guild.CurrentUser.GetPermissions(socketTextChannel);

            // Get a list of permissions that the bot does not have
            IEnumerable<ChannelPermission> missingPermissions = requiredPermissions.Where(x => !channelPermissions.Has(x));

            // Check if we are missing any
            if (missingPermissions.Count() > 0)
            {
                // Try to get the default language
                Language language = DiscordUtil.GetDefaultLanguage(guild);

                // Create the description string for the error message
                string description = Localizer.Localize("discord.setup.enter_channel.missing_permissions", language) + "\n\n";

                // Append the permissions
                foreach (ChannelPermission permission in missingPermissions)
                {
                    description += permission.ToString() + "\n";
                }

                // Create an embed
                await DiscordUtil.SendErrorMessageByDescription(guild, this.Channel, description);

                return false;
            }

            if (SetupFlow.ChannelSettings == null)
            {
                // Create a DynamicSettingsData instance
                SetupFlow.ChannelSettings = new DynamicSettingsData();

                // Set the ID
                SetupFlow.TargetChannelId = channelId;

                // Proceed to language selection
                await SetupFlow.SetPage((int)SetupFlowPage.SelectLanguage);
            }
            else
            {
                // Move the data
                SetupFlow.GuildSettings.ChannelSettings.TryRemove(SetupFlow.TargetChannelId, out DynamicSettingsData data);
                SetupFlow.GuildSettings.ChannelSettings.TryAdd(channelId, data);

                // Set a pre-prompt
                SetupFlow.ModeSelectPrePromptLocalizable = "discord.setup.mode_select.pre_prompt.edits_saved";
                
                // Change to mode select
                await SetupFlow.SetPage((int)SetupFlowPage.ModeSelect);
            }
            
            return false;
        }
    }
}