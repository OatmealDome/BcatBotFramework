using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Quartz;
using BcatBotFramework.Core.Config;
using BcatBotFramework.Core.Config.Discord;
using BcatBotFramework.Social.Discord;

namespace SmashBcatDetector.Scheduler.Job
{
    public abstract class RecurringHousekeepingJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await DiscordBot.LoggingChannel.SendMessageAsync($"**[RecurringHousekeepingJob]** Processing deregistrations because of no write permissions");

            await DiscordUtil.FindBadGuilds();

            // Run app-specific tasks
            await RunAppSpecificRecurringTasks();

            await DiscordBot.LoggingChannel.SendMessageAsync($"**[RecurringHousekeepingJob]** Saving configuration");

            // Save the configuration
            Configuration.LoadedConfiguration.Write();
        }

        protected abstract Task RunAppSpecificRecurringTasks();

    }
}