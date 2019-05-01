using BcatBotFramework.Core.Config;
using BcatBotFramework.Core.Config.Web;

namespace BcatBotFramework.Core.Config
{
    public abstract class WebConfig : ISubConfiguration
    {
        public RemoteServer RemoteServer
        {
            get;
            set;
        }

        public abstract void SetDefaults();
        
    }
}
