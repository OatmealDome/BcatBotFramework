using System.IO;
using System.Reflection;

namespace BcatBotFramework.Core.Config
{
    public class LocalizerConfig : ISubConfiguration
    {
        public string LocalizationsDirectory
        {
            get;
            set;
        }

        public void SetDefaults()
        {
            LocalizationsDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Localizations");
        }
        
    }
}