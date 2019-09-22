using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace BcatBotFramework.Social.Discord.Interactive
{
    public abstract class PagedInteractiveMessage : InteractiveMessage
    {
        protected int CurrentPage
        {
            get;
            set;
        }

        protected abstract int LastPage
        {
            get;
        }
        
        // Emotes
        private IEmote EMOTE_TO_BEGINNING = new Emoji("\u23EA"); // ⏪
        private IEmote EMOTE_BACK = new Emoji("\u25C0"); // ◀️
        private IEmote EMOTE_FORWARD = new Emoji("\u25B6"); // ▶️
        private IEmote EMOTE_TO_END = new Emoji("\u23E9"); // ⏩

        protected PagedInteractiveMessage(IUser user) : base(user)
        {

        }

        public override Task<bool> HandleReaction(IEmote emote)
        {
            if (emote.Name == EMOTE_TO_BEGINNING.Name)
            {
                CurrentPage = 0;
                return Task.FromResult(true);
            }
            else if (emote.Name == EMOTE_BACK.Name)
            {
                CurrentPage--;

                // Don't go below zero
                if (CurrentPage <= 0)
                {
                    CurrentPage = 0;
                }
                
                return Task.FromResult(true);
            }
            else if (emote.Name == EMOTE_FORWARD.Name)
            {
                CurrentPage++;

                // Don't go above maximum
                if (CurrentPage > LastPage)
                {
                    CurrentPage = LastPage;
                }

                return Task.FromResult(true);
            }
            else if (emote.Name == EMOTE_TO_END.Name)
            {
                CurrentPage = LastPage;
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public override async Task AddReactions()
        {
            if (CurrentPage == 0 && LastPage > 1)
            {
                await this.TargetMessage.RemoveAllReactionsAsync();

                await this.TargetMessage.AddReactionAsync(EMOTE_FORWARD);
                await this.TargetMessage.AddReactionAsync(EMOTE_TO_END);
            }
            else if ((CurrentPage == 1 && !HasReaction(EMOTE_BACK)) || (CurrentPage == LastPage - 1 && !HasReaction(EMOTE_FORWARD))) // first page or second to last page
            {
                await this.TargetMessage.RemoveAllReactionsAsync();

                await this.TargetMessage.AddReactionAsync(EMOTE_TO_BEGINNING);
                await this.TargetMessage.AddReactionAsync(EMOTE_BACK);
                await this.TargetMessage.AddReactionAsync(EMOTE_FORWARD);
                await this.TargetMessage.AddReactionAsync(EMOTE_TO_END);
            }
            else if (CurrentPage == LastPage)
            {
                await this.TargetMessage.RemoveAllReactionsAsync();

                await this.TargetMessage.AddReactionAsync(EMOTE_TO_BEGINNING);
                await this.TargetMessage.AddReactionAsync(EMOTE_BACK);
            }
        }

        public override Task<bool> HandleTextMessage(SocketMessage message)
        {
            return Task.FromResult(false);
        }

    }
}