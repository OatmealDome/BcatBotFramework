using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BcatBotFramework.Core.Config;
using BcatBotFramework.Internationalization;
using Discord;
using Discord.WebSocket;

namespace BcatBotFramework.Social.Discord.Interactive.Setup
{
    public abstract class SetupNotificationsSelectorMessage : InteractiveMessage
    {
        private static Emote EMOTE_SAVE = Emote.Parse("<:OK:606588782988492810>");
        private static Emote EMOTE_EXIT = Emote.Parse("<:Cancel:606588486308462662>");

        protected readonly SetupFlow SetupFlow;

        protected SetupNotificationsSelectorMessage(SetupFlow setupFlow, IUser user, IUserMessage message) : base(user, message)
        {
            SetupFlow = setupFlow;
        }

        public override async Task AddReactions()
        {
            // Add notifications reactions
            await this.TargetMessage.AddReactionsAsync(GetNotificationsReactions().ToArray());

            // Add save and exit
            await this.TargetMessage.AddReactionsAsync(new IEmote[] { EMOTE_SAVE, EMOTE_EXIT });      
        }

        public override MessageProperties CreateMessageProperties()
        {
            // Return properties
            return new MessageProperties()
            {
                Content = null,
                Embed = new EmbedBuilder()
                            .WithTitle(Localizer.Localize("discord.setup.notifications_selector.title", SetupFlow.DefaultLanguage))
                            .WithDescription($"{Localizer.Localize("discord.setup.notifications_selector.prompt", SetupFlow.DefaultLanguage)}\n\n" + GetReactionListForDescription() + $"\n\n{EMOTE_SAVE.ToString()} {Localizer.Localize("discord.setup.notifications_selector.save", SetupFlow.DefaultLanguage)}\n{EMOTE_EXIT.ToString()} {Localizer.Localize("discord.setup.notifications_selector.exit", SetupFlow.DefaultLanguage)}")
                            .Build()
            };
        }

        public override async Task<bool> HandleReaction(IEmote emote)
        {
            if (emote.Name == EMOTE_EXIT.Name)
            {
                if (SetupFlow.Mode == SetupFlowMode.AddInitial)
                {
                    await SetupFlow.SetPage((int)SetupFlowPage.Exit);
                }
                else
                {
                    // Set a pre-prompt
                    SetupFlow.ModeSelectPrePromptLocalizable = "discord.setup.mode_select.pre_prompt.edits_not_saved";

                    await SetupFlow.SetPage((int)SetupFlowPage.ModeSelect);
                }

                return false;
            }
            else if (emote.Name == EMOTE_SAVE.Name)
            {
                // Save settings to the config
                SaveSettings();

                // Check mode
                if (SetupFlow.Mode == SetupFlowMode.AddInitial || SetupFlow.Mode == SetupFlowMode.Add)
                {
                    // Add the settings to the config
                    SetupFlow.GuildSettings.ChannelSettings.TryAdd(SetupFlow.TargetChannelId, SetupFlow.ChannelSettings);
                }

                // Modify the message to show saved
                await ModifyOriginalMessage();

                // Set a pre-prompt
                SetupFlow.ModeSelectPrePromptLocalizable = "discord.setup.mode_select.pre_prompt.edits_saved";    

                // Go back to mode select
                await SetupFlow.SetPage((int)SetupFlowPage.ModeSelect);

                return false;
            }
            else
            {
                await HandleNotificationsReaction(emote);
            }

            return true;
        }

        public override Task<bool> HandleTextMessage(SocketMessage message)
        {
            return Task.FromResult(false);
        }

        protected abstract IEnumerable<IEmote> GetNotificationsReactions();

        protected abstract string GetReactionListForDescription();

        protected abstract Task HandleNotificationsReaction(IEmote emote);

        protected abstract void SaveSettings();

    }
}