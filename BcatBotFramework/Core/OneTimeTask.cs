using System.Threading.Tasks;
using BcatBotFramework.Core.Config;

namespace BcatBotFramework.Core
{
    public abstract class OneTimeTask
    {
        public async Task RunImpl()
        {
            // Check if this task needs to be run
            if (!Configuration.LoadedConfiguration.CompletedOneTimeTasks.Contains(this.GetType().Name))
            {
                await Run();

                Configuration.LoadedConfiguration.CompletedOneTimeTasks.Add(this.GetType().Name);
            }
        }

        protected abstract Task Run();
    }
}