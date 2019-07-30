using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace BcatBotFramework.Social.Discord
{
    public abstract class InteractiveMessage
    {
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
        
        protected RestUserMessage TargetMessage
        {
            get;
            set;
        }

        public IUser User
        {
            get;
            private set;
        }

        public ISocketMessageChannel Channel
        {
            get;
            private set;
        }

        public ulong MessageId
        {
            get
            {
                return TargetMessage != null ? TargetMessage.Id : 0;
            }
        }

        protected InteractiveMessage(IUser user)
        {
            this.User = user;
        }

        public async Task SendInitialMessage(ISocketMessageChannel targetChannel)
        {
            // Check if we've already sent the initial message
            if (TargetMessage != null)
            {
                throw new Exception("Cannot send initial message twice");
            }

            // Create the initial message
            MessageProperties properties = CreateMessageProperties();

            // Send the message
            TargetMessage = await targetChannel.SendMessageAsync(text: properties.Content.GetValueOrDefault(), embed: properties.Embed.GetValueOrDefault());

            // Set the channel
            Channel = targetChannel;

            // Add the reactions
            await AddReactions(null);
        }

        public async Task ReactionAdded(SocketReaction reaction)
        {
            // Acquire the semaphore
            await Semaphore.WaitAsync();

            // Handle the reaction if needed and if it's from the executor
            if (reaction.UserId != User.Id || !await HandleReaction(reaction.Emote))
            {
                // Release the semaphore
                Semaphore.Release();

                return;
            }

            // Create the initial message
            MessageProperties newProperties = CreateMessageProperties();

            // Modify the message
            await TargetMessage.ModifyAsync(properties =>
            {
                properties.Content = newProperties.Content;
                properties.Embed = newProperties.Embed;
            });

            // Add and clear any reactions if needed
            await AddReactions(reaction);

            // Release the semaphore
            Semaphore.Release();
        }

        public async Task TextMessageReceived(SocketMessage message)
        {
            // Acquire the semaphore
            await Semaphore.WaitAsync();

            // Handle the message if needed and if it's from the executor
            if (!await HandleTextMessage(message))
            {
                // Release the semaphore
                Semaphore.Release();

                return;
            }

            // Create the initial message
            MessageProperties newProperties = CreateMessageProperties();

            // Modify the message
            await TargetMessage.ModifyAsync(properties =>
            {
                properties.Content = newProperties.Content;
                properties.Embed = newProperties.Embed;
            });

            // Release the semaphore
            Semaphore.Release();
        }

        public async Task ClearReactions()
        {
            await TargetMessage.RemoveAllReactionsAsync();
        }

        protected bool HasReaction(IEmote emote)
        {
            return TargetMessage.Reactions.Count(x => x.Key.Name == emote.Name) > 0;
        }

        public abstract MessageProperties CreateMessageProperties();

        public abstract Task<bool> HandleReaction(IEmote emote);

        public abstract Task<bool> HandleTextMessage(SocketMessage message);

        public abstract Task AddReactions(SocketReaction reaction);

    }
}