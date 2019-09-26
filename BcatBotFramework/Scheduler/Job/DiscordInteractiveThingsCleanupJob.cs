using System.Threading.Tasks;
using BcatBotFramework.Social.Discord;
using Quartz;

namespace BcatBotFramework.Scheduler.Job
{
    public class DiscordInteractiveThingsCleanupJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await DiscordBot.DeactivateInactiveInteractiveThings();
        }
        
    }
}