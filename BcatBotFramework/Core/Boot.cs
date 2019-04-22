using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using BcatBotFramework.Core.Config;
using BcatBotFramework.Difference;
using BcatBotFramework.Internationalization;
using BcatBotFramework.Scheduler;
using BcatBotFramework.Scheduler.Job;
using BcatBotFramework.Social.Discord;
using BcatBotFramework.Social.Twitter;
using Nintendo.Bcat;
using Nintendo.DAuth;
using S3;

namespace BcatBotFramework.Core
{
    class Boot
    {
        // Local Directory
        private static string LOCAL_DIRECTORY = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string LOCAL_CONFIGURATION = Path.Combine(LOCAL_DIRECTORY, "config.json");
        public static string LOCAL_CONFIGURATION_AUTOMATIC_BACKUP = Path.Combine(LOCAL_DIRECTORY, "config-automatic-backup.json");
        public static string LOCAL_EXCEPTION_LOGS_DIRECTORY = Path.Combine(LOCAL_DIRECTORY, "ExceptionLogs");
        
        static async Task Main(string[] args)
        {
            // Wait for the debugger to attach if requested
            if (args.Length > 0 && args[0].ToLower() == "--waitfordebugger")
            {
                Console.WriteLine("Waiting for debugger...");

                while (!Debugger.IsAttached)
                {
                    await Task.Delay(1000);
                }

                Console.WriteLine("Debugger attached!");
            }

            // Get the type of the Configuration
            Type configType = TypeUtils.GetSubclassOfType<Configuration>();

            // Declare variable to hold the configuration
            Configuration configuration;

            // Check if the config file exists
            if (!File.Exists(LOCAL_CONFIGURATION))
            {
                // Create a new dummy Configuration
                configuration = (Configuration)Activator.CreateInstance(TypeUtils.GetSubclassOfType<Configuration>());

                // Write out the default config
                configuration.Write();

                Console.WriteLine("Wrote default configuration to " + LOCAL_CONFIGURATION);

                return;
            }

            // Create the Exception logs directory
            System.IO.Directory.CreateDirectory(LOCAL_EXCEPTION_LOGS_DIRECTORY);

            // Load the Configuration
            Configuration.Load(configType, LOCAL_CONFIGURATION);

            // Initialize the Localizer
            Localizer.Initialize();

            // Initialize the HandlerMapper
            HandlerMapper.Initialize();

            // Initialize DAuth
            DAuthApi.Initialize();

            // Initialize BCAT
            BcatApi.Initialize();

            // Initialize S3
            S3Api.Initialize();

            // Initialize Twitter
            TwitterManager.Initialize();

            // Initialize the DiscordBot
            await DiscordBot.Initialize();

            // Initialize the Scheduler
            await QuartzScheduler.Initialize();

            // Wait for the bot to fully initialize
            while (!DiscordBot.IsReady)
            {
                await Task.Delay(1000);
            }

            // Print out to the logging channel that we're initialized
            await DiscordBot.LoggingChannel.SendMessageAsync("\\*\\*\\* **Initialized**");

            // Schedule the BootHousekeepingJob
            await QuartzScheduler.ScheduleJob(TypeUtils.GetSubclassOfType<BootHousekeepingJob>(), "Immediate");

            // Register the SIGTERM handler
            AssemblyLoadContext.Default.Unloading += async x =>
            {
                // Run Shutdown in fast mode
                await Shutdown.CreateAndRun(true);
            };
            
            await Task.Delay(-1);

        }

    }
}
