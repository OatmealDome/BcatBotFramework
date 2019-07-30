using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BcatBotFramework.Social.Discord;
using Quartz;

namespace BcatBotFramework.Scheduler.Job
{
    public class InteractiveMessageCleanupJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            // Get the target message
            InteractiveMessage targetMessage = (InteractiveMessage)context.JobDetail.JobDataMap["messageInstance"];
            
            // Remove it
            await DiscordBot.DeactivateInteractiveMessage(targetMessage);
        }

    }
}