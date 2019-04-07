using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using BcatBotFramework.Core.Config.Scheduler;

namespace BcatBotFramework.Core.Config
{
    public abstract class Configuration
    {
        public static Configuration LoadedConfiguration;
        private static string ConfigurationFilePath;

        [JsonProperty("NintendoCdn")]
        public NintendoCdnConfig CdnConfig
        {
            get;
            set;
        }

        [JsonProperty("Keyset")]
        public KeysetConfig KeysetConfig
        {
            get;
            set;
        }

        [JsonProperty("Discord")]
        public DiscordConfig DiscordConfig
        {
            get;
            set;
        }

        [JsonProperty("Twitter")]
        public TwitterConfig TwitterConfig
        {
            get;
            set;
        }

        [JsonProperty("Scheduler")]
        public Dictionary<string, JobSchedule> JobSchedules
        {
            get;
            set;
        }

        [JsonProperty("S3")]
        public S3Config S3Config
        {
            get;
            set;
        }

        [JsonProperty("Localizer")]
        public LocalizerConfig LocalizerConfig
        {
            get;
            set;
        }

        public bool FirstRunCompleted
        {
            get;
            set;
        }

        public bool IsProduction
        {
            get;
            set;
        }

        public static void Load<T>(string path) where T : Configuration
        {
            LoadedConfiguration = (Configuration)JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
            ConfigurationFilePath = path;
        }

        public void Write()
        {
            File.WriteAllText(ConfigurationFilePath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public void SetDefaults()
        {
            CdnConfig = new NintendoCdnConfig();
            CdnConfig.SetDefaults();
            KeysetConfig = new KeysetConfig();
            KeysetConfig.SetDefaults();
            DiscordConfig = new DiscordConfig();
            DiscordConfig.SetDefaults();
            TwitterConfig = new TwitterConfig();
            TwitterConfig.SetDefaults();
            JobSchedules = new Dictionary<string, JobSchedule>();
            S3Config = new S3Config();
            S3Config.SetDefaults();
            LocalizerConfig = new LocalizerConfig();
            LocalizerConfig.SetDefaults();
            FirstRunCompleted = false;
            IsProduction = false;
        }

        protected abstract void SetAppSpecificDefaults();

    }
}