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

namespace BcatBotFramework.Social.Discord.Interactive.Setup
{
    public class SetupChannelChooserMessage : InteractiveMessage
    {
        private static char EMOTE_NUMBER_SUFFIX = '\u20E3';
        private static Emote EMOTE_EXIT = Emote.Parse("<:Cancel:606588486308462662>");

        private readonly SetupFlow SetupFlow;

        public SetupChannelChooserMessage(SetupFlow flow, IUser user, IUserMessage message) : base(user, message)
        {
            SetupFlow = flow;
        }

        public override async Task AddReactions()
        {
            // Add the necessary amount of numbers
            for (int i = 0; i < SetupFlow.GuildSettings.ChannelSettings.Count(); i++)
            {
                await this.TargetMessage.AddReactionAsync(new Emoji($"{i + 1}{EMOTE_NUMBER_SUFFIX}"));
            }

            // Add the exit emoji
            await this.TargetMessage.AddReactionAsync(EMOTE_EXIT);
        }

        public override MessageProperties CreateMessageProperties()
        {
            // Create a list of channels
            string channelList = "";
            for (int i = 0; i < SetupFlow.GuildSettings.ChannelSettings.Count(); i++)
            {
                // Get the channel settings associated with this index
                KeyValuePair<ulong, DynamicSettingsData> pair = SetupFlow.GuildSettings.ChannelSettings.ElementAt(i);

                // Append this to the list
                channelList += $"{i + 1}{EMOTE_NUMBER_SUFFIX} {MentionUtils.MentionChannel(pair.Key)}\n";
            }

            return new MessageProperties()
            {
                Content = null,
                Embed = new EmbedBuilder()
                            .WithTitle(Localizer.Localize("discord.setup.channel_chooser.title", SetupFlow.DefaultLanguage))
                            .WithDescription($"{Localizer.Localize("discord.setup.channel_chooser.prompt", SetupFlow.DefaultLanguage)}\n\n" + channelList + $"\n\n{EMOTE_EXIT.ToString()} Exit")
                            .Build()
            };
        }

        public override async Task<bool> HandleReaction(IEmote emote)
        {
            // Check if this was the exit emote
            if (emote.Name == EMOTE_EXIT.Name)
            {
                // Do not use a pre-prompt
                SetupFlow.ModeSelectPrePromptLocalizable = null;

                // Go back to mode select
                await SetupFlow.SetPage((int)SetupFlowPage.ModeSelect);
            }
            else if (emote.Name[1] == EMOTE_NUMBER_SUFFIX) // Check that the ending is the number suffix
            {
                // Get the number
                // TODO remove terrible hack with char -> string conversion
                int i = int.Parse($"{emote.Name[0]}") - 1;

                // Verify that it is not -1
                if (i == -1)
                {
                    // Do nothing
                    return false;
                }

                // Set the target channel ID and DynamicSettingsData instance
                KeyValuePair<ulong, DynamicSettingsData> pair = SetupFlow.GuildSettings.ChannelSettings.ElementAt(i);
                SetupFlow.TargetChannelId = pair.Key;
                SetupFlow.ChannelSettings = pair.Value;

                // Check the mode
                if (SetupFlow.Mode == SetupFlowMode.Edit)
                {
                    // Check how many languages there are
                    if (SetupFlow.ValidLanguages.Count() <= 1)
                    {
                        // Go straight to the notifications selector
                        await SetupFlow.SetPage((int)SetupFlowPage.SelectNotifications);   
                    }
                    else
                    {
                        // Proceed to the edit mode selector 
                        await SetupFlow.SetPage((int)SetupFlowPage.SelectEditMode);
                    }
                }
                else if (SetupFlow.Mode == SetupFlowMode.Delete)
                {
                    // Remove this settings
                    SetupFlow.GuildSettings.ChannelSettings.TryRemove(pair.Key, out DynamicSettingsData data);

                    // Check if that was the final ChannelSettings
                    if (SetupFlow.GuildSettings.ChannelSettings.Count() == 0)
                    {
                        // Exit
                        await SetupFlow.SetPage((int)SetupFlowPage.Exit);
                    }
                    else
                    {
                        // Set mode select pre-prompt
                        SetupFlow.ModeSelectPrePromptLocalizable = "discord.setup.mode_select.pre_prompt.deleted";

                        await SetupFlow.SetPage((int)SetupFlowPage.ModeSelect);
                    }
                }
            }

            return false;
        }

        public override Task<bool> HandleTextMessage(SocketMessage message)
        {
            return Task.FromResult(false);
        }

    }
}