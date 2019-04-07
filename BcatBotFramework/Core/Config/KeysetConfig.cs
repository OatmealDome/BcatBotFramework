using System;
using System.IO;

namespace BcatBotFramework.Core.Config
{
    public class KeysetConfig : ISubConfiguration
    {
        public string ProductionKeys
        {
            get;
            set;
        }

        public string TitleKeys
        {
            get;
            set;
        }

        public void SetDefaults()
        {
            // Get the user's .switch folder under their home directory
            string switchDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".switch");

            // Get the keysets
            ProductionKeys = Path.Combine(switchDir, "prod.keys");
            TitleKeys = Path.Combine(switchDir, "title.keys");
        }

    }
}