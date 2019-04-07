using BcatBotFramework.Core.Config;
using LibHac;

namespace BcatBotFramework.Core
{
    public static class KeysetManager
    {
        private static Keyset Keyset;

        public static void Initialize()
        {
            // Get the KeysetConfig
            KeysetConfig keysetConfig = Configuration.LoadedConfiguration.KeysetConfig;

            Keyset = ExternalKeys.ReadKeyFile(keysetConfig.ProductionKeys, keysetConfig.TitleKeys);   
        }

        public static void Dispose()
        {

        }

        public static Keyset GetKeyset()
        {
            return Keyset;
        }
        
    }
}