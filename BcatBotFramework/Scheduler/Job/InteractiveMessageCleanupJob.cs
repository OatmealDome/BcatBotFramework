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
            InteractiveMessage targetMessage = DiscordBot.ActiveInteractiveMessages.Where(x => x.MessageId == (ulong)context.JobDetail.JobDataMap["messageId"]).FirstOrDefault();
            
            // Remove this from the active list
            DiscordBot.ActiveInteractiveMessages.Remove(targetMessage);
            
            // Clear all reactions
            await targetMessage.ClearReactions();
        }

    }
}