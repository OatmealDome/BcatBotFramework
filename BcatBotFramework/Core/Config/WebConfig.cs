using BcatBotFramework.Core.Config;
using SmashBcatDetector.Core.Config.Web;

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
