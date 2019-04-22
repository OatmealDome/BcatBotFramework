using System;
using System.Threading.Tasks;
using Nintendo.Bcat;
using Nintendo.DAuth;
using Quartz;
using BcatBotFramework.Core.Config;
using S3;
using BcatBotFramework.Social.Discord;
using BcatBotFramework.Difference;
using BcatBotFramework.Scheduler;
using BcatBotFramework.Social.Twitter;
using System.IO;

namespace BcatBotFramework.Core
{
    public abstract class Shutdown
    {
        private static bool IsShutdown = false;
        private static int ShutdownWaitTime = 5; // in seconds

        public static async Task CreateAndRun(bool fastShutdown)
        {
            if (IsShutdown)
            {
                return;
            }

            // Get the Shutdown subclass
            Type shutdownType = TypeUtils.GetSubclassOfType<Shutdown>();

            // Create a new instance of it
            Shutdown shutdownInstance = (Shutdown)Activator.CreateInstance(shutdownType);
            
            // Call the Run method
            await shutdownInstance.Run(fastShutdown);

            IsShutdown = true;
        }

        private async Task Run(bool fastShutdown)
        {
            Console.WriteLine($"{(fastShutdown ? "Fast" : "Slow")} shutdown started");

            if (!fastShutdown)
            {
                // Shutdown the Scheduler
                await QuartzScheduler.Dispose();

                // Shutdown the DiscordBot
                await DiscordBot.Dispose();
            }
            else
            {
                // Create a backup of the current config.json just in case
                File.Copy(Boot.LOCAL_CONFIGURATION, Boot.LOCAL_CONFIGURATION_AUTOMATIC_BACKUP, true);
            }

            // Shutdown Twitter
            TwitterManager.Dispose();

            // Shutdown S3
            S3Api.Dispose();

            // Shutdown BCAT
            BcatApi.Dispose();

            // Shutdown DAuth
            DAuthApi.Dispose();

            // Shutdown the HandlerMapper
            HandlerMapper.Dispose();

            // Shutdown anything app-specific
            ShutdownAppSpecificItems();

            // Save the configuration
            Configuration.LoadedConfiguration.Write();

            if (!fastShutdown)
            {
                // Wait a little while
                await Task.Delay(1000 * ShutdownWaitTime);
            }

            Console.WriteLine("Shutdown complete");
        }

        protected abstract void ShutdownAppSpecificItems();

    }
}