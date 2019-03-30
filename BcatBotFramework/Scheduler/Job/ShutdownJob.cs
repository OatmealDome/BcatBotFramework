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

namespace BcatBotFramework.Scheduler.Job
{
    public abstract class ShutdownJob : IJob
    {
        private static int ShutdownWaitTime = 5; // in seconds

        public async Task Execute(IJobExecutionContext context)
        {
            // Shutdown the Scheduler
            await QuartzScheduler.Dispose();

            // Shutdown the DiscordBot
            await DiscordBot.Dispose();

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

            // Save the configuration
            Configuration.LoadedConfiguration.Write();

            // Wait a little while just in case
            await Task.Delay(1000 * ShutdownWaitTime);

            Environment.Exit(0);
        }

        protected abstract void ShutdownAppSpecificItems();

    }
}