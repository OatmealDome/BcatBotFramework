using System.Threading.Tasks;
using BcatBotFramework.Core.Config;
using BcatBotFramework.Scheduler;
using BcatBotFramework.Scheduler.Job;
using BcatBotFramework.Social.Discord.Precondition;
using Discord.Commands;

namespace BcatBotFramework.Social.Discord.Command
{
    [RequireBotAdministratorPrecondition]
    public class AdminCommands : ModuleBase<SocketCommandContext>
    {
        [Command("saveconfig"), Summary("saves config")]
        public async Task SaveConfig()
        {
            Configuration.LoadedConfiguration.Write();

            await Context.Channel.SendMessageAsync("**[Admin]** Configuration saved");
        }

        [Command("shutdown"), Summary("saves config")]
        public async Task Shutdown()
        {
            await QuartzScheduler.ScheduleJob<ShutdownJob>("Trigger");

            await Context.Channel.SendMessageAsync("**[Admin]** Shutdown scheduled");
        }
        
    }
}