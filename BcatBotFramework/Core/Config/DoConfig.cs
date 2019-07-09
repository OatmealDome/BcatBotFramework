namespace BcatBotFramework.Core.Config
{
    public class DoConfig : ISubConfiguration
    {
        public string Token
        {
            get;
            set;
        }

        public string EndpointId
        {
            get;
            set;
        }

        public void SetDefaults()
        {
            Token = "cafebabe";
            EndpointId = "deadbeef";
        }

    }
}