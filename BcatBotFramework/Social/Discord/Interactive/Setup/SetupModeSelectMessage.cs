using System.Threading.Tasks;
using BcatBotFramework.Internationalization;
using BcatBotFramework.Social.Discord;
using Discord;
using Discord.WebSocket;

namespace BcatBotFramework.Social.Discord.Interactive.Setup
{
    public class SetupModeSelectMessage : InteractiveMessage
    {
        private static IEmote EMOTE_ADD = new Emoji("\uD83D\uDCDD"); // 📝
        private static IEmote EMOTE_EDIT = new Emoji("\uD83D\uDD28"); // 🛠️
        private static IEmote EMOTE_DELETE = new Emoji("\uD83D\uDEAB"); // 🚫
        private static IEmote EMOTE_EXIT = new Emoji("\u274C"); // X

        private readonly SetupFlow SetupFlow;

        public SetupModeSelectMessage(SetupFlow flow, IUser user) : base(user)
        {
            SetupFlow = flow;
        }

        public SetupModeSelectMessage(SetupFlow flow, IUser user, IUserMessage message) : base(user, message)
        {
            SetupFlow = flow;
        }

        public override async Task AddReactions()
        {
            await this.TargetMessage.AddReactionsAsync(new IEmote[] { EMOTE_ADD, EMOTE_EDIT, EMOTE_DELETE, EMOTE_EXIT });
        }

        public override MessageProperties CreateMessageProperties()
        {
            // Create the prompt
            string prompt = "";
            if (!string.IsNullOrEmpty(SetupFlow.ModeSelectPrePromptLocalizable))
            {
                prompt += Localizer.Localize(SetupFlow.ModeSelectPrePromptLocalizable, SetupFlow.DefaultLanguage) + "\n\n";
            }

            prompt += Localizer.Localize("discord.setup.mode_select.prompt", SetupFlow.DefaultLanguage);

            return new MessageProperties()
            {
                Content = null,
                Embed = new EmbedBuilder()
                            .WithTitle(Localizer.Localize("discord.setup.mode_select.title", SetupFlow.DefaultLanguage))
                            .WithDescription($"{prompt}\n\n{EMOTE_ADD.Name} {Localizer.Localize("discord.setup.mode_select.add", SetupFlow.DefaultLanguage)}\n{EMOTE_EDIT.Name} {Localizer.Localize("discord.setup.mode_select.edit", SetupFlow.DefaultLanguage)}\n{EMOTE_DELETE.Name} {Localizer.Localize("discord.setup.mode_select.delete", SetupFlow.DefaultLanguage)}\n\n{EMOTE_EXIT.Name} {Localizer.Localize("discord.setup.mode_select.exit", SetupFlow.DefaultLanguage)}")
                            .Build()
            };
        }

        public override async Task<bool> HandleReaction(IEmote emote)
        {
            if (emote.Name == EMOTE_ADD.Name)
            {
                await SetupFlow.SetPage((int)SetupFlowPage.EnterChannel);

                SetupFlow.Mode = SetupFlowMode.Add;

                return false;
            }
            else if (emote.Name == EMOTE_EDIT.Name)
            {
                SetupFlow.Mode = SetupFlowMode.Edit;
            }
            else if (emote.Name == EMOTE_DELETE.Name)
            {
                SetupFlow.Mode = SetupFlowMode.Delete;
            }
            else if (emote.Name == EMOTE_EXIT.Name)
            {
                await SetupFlow.SetPage((int)SetupFlowPage.Exit);

                return false;
            }

            await SetupFlow.SetPage((int)SetupFlowPage.ChannelChooser);

            return false;
        }

        public override Task<bool> HandleTextMessage(SocketMessage message)
        {
            return Task.FromResult(false);
        }

    }
}