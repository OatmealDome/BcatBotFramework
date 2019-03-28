using System.Collections.Generic;
using BcatBotFramework.Core.Config.Twitter;

namespace BcatBotFramework.Core.Config
{
    public class TwitterConfig
    {
        public Dictionary<string, CachedTwitterCredentials> TwitterCredentials
        {
            get;
            set;
        }
        
        public string CharacterCounterBinary
        {
            get;
            set;
        }

        public bool IsActivated
        {
            get;
            set;
        }
        
    }
}