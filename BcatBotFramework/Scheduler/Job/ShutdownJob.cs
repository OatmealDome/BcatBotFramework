using System;
using System.Threading.Tasks;
using BcatBotFramework.Core;
using Quartz;

namespace BcatBotFramework.Scheduler.Job
{
    public class ShutdownJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            // Run Shutdown in slow mode
            await Shutdown.CreateAndRun(false);

            Environment.Exit(0);
        }

    }
}