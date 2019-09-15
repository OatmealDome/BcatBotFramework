using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace BcatBotFramework.Social.Discord.Interactive
{
    public abstract class InteractiveFlow
    {
        public int CurrentPageIdx
        {
            get;
            protected set;
        }

        public InteractiveMessage CurrentInteractiveMessage
        {
            get;
            protected set;
        }

        protected IUser User
        {
            get;
            set;
        }

        protected ISocketMessageChannel Channel
        {
            get;
            set;
        }

        public InteractiveFlow(IUser user, ISocketMessageChannel channel)
        {
            User = user;
            Channel = channel;
        }

        public abstract Task SetPage(int page);

    }
}