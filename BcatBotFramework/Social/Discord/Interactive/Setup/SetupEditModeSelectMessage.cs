using System.Threading.Tasks;
using BcatBotFramework.Internationalization;
using Discord;
using Discord.WebSocket;

namespace BcatBotFramework.Social.Discord.Interactive.Setup
{
    public class SetupEditModeSelectMessage : InteractiveMessage
    {
        private static IEmote EMOTE_NOTIFICATIONS = new Emoji("\uD83D\uDCEC"); // Mailbox
        private static IEmote EMOTE_LANGUAGE = new Emoji("\uD83C\uDF0E"); // Globe
        private static IEmote EMOTE_CHANGE_TARGET = new Emoji("\u270F"); // Pencil
        private static IEmote EMOTE_EXIT = new Emoji("\u274C"); // X

        private readonly SetupFlow SetupFlow;

        public SetupEditModeSelectMessage(SetupFlow flow, IUser user, IUserMessage message) : base(user, message)
        {
            SetupFlow = flow;
        }

        public override async Task AddReactions()
        {
            await this.TargetMessage.AddReactionsAsync(new IEmote[] { EMOTE_NOTIFICATIONS, EMOTE_LANGUAGE, EMOTE_CHANGE_TARGET, EMOTE_EXIT });
        }

        public override MessageProperties CreateMessageProperties()
        {
            return new MessageProperties()
            {
                Content = null,
                Embed = new EmbedBuilder()
                            .WithTitle(Localizer.Localize("discord.setup.edit_mode_selector.title", SetupFlow.DefaultLanguage))
                            .WithDescription($"{Localizer.Localize("discord.setup.edit_mode_selector.prompt", SetupFlow.DefaultLanguage)}\n\n{EMOTE_NOTIFICATIONS.Name} {Localizer.Localize("discord.setup.edit_mode_selector.notifications", SetupFlow.DefaultLanguage)}\n{EMOTE_LANGUAGE.Name} {Localizer.Localize("discord.setup.edit_mode_selector.language", SetupFlow.DefaultLanguage)}\n{EMOTE_CHANGE_TARGET.Name} {Localizer.Localize("discord.setup.edit_mode_selector.change_target", SetupFlow.DefaultLanguage)}\n\n{EMOTE_EXIT.Name} {Localizer.Localize("discord.setup.edit_mode_selector.exit", SetupFlow.DefaultLanguage)}")
                            .Build()
            };
        }

        public override async Task<bool> HandleReaction(IEmote emote)
        {
            if (emote.Name == EMOTE_NOTIFICATIONS.Name)
            {
                await SetupFlow.SetPage((int)SetupFlowPage.SelectNotifications);
            }
            else if (emote.Name == EMOTE_LANGUAGE.Name)
            {
                await SetupFlow.SetPage((int)SetupFlowPage.SelectLanguage);
            }
            else if (emote.Name == EMOTE_CHANGE_TARGET.Name)
            {
                await SetupFlow.SetPage((int)SetupFlowPage.EnterChannel);
            }
            else if (emote.Name == EMOTE_EXIT.Name)
            {
                SetupFlow.ModeSelectPrePromptLocalizable = "discord.setup.mode_select.pre_prompt.edits_not_saved";
                
                await SetupFlow.SetPage((int)SetupFlowPage.ModeSelect);
            }

            return false;
        }

        public override Task<bool> HandleTextMessage(SocketMessage message)
        {
            return Task.FromResult(false);
        }
    }
}