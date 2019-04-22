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

namespace BcatBotFramework.Core
{
    public abstract class Shutdown
    {
        private static int ShutdownWaitTime = 5; // in seconds

        public async Task Run(bool fastShutdown = false)
        {
            if (!fastShutdown)
            {
                // Shutdown the Scheduler
                await QuartzScheduler.Dispose();

                // Shutdown the DiscordBot
                await DiscordBot.Dispose();
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
        }

        protected abstract void ShutdownAppSpecificItems();

    }
}