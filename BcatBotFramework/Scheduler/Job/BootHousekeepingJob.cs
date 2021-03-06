using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Quartz;
using BcatBotFramework.Core.Config;
using BcatBotFramework.Social.Discord;
using BcatBotFramework.Scheduler;
using System.Reflection;
using BcatBotFramework.Core;
using BcatBotFramework.Social.Discord.Settings;

namespace BcatBotFramework.Scheduler.Job
{
    public abstract class BootHousekeepingJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            try {
                // Run app-specific boot tasks
                await RunAppSpecificBootTasks();

                // Run one-time tasks
                await DiscordBot.LoggingChannel.SendMessageAsync($"**[BootHousekeepingJob]** Running one-time tasks");

                foreach (Type type in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(OneTimeTask)) && !x.IsAbstract))
                {
                    // Create a new instance and run it
                    await ((OneTimeTask)Activator.CreateInstance(type)).RunImpl();
                }

                await DiscordBot.LoggingChannel.SendMessageAsync($"**[BootHousekeepingJob]** Processing joined/left guilds");

                // Get a list of guilds
                IReadOnlyCollection<SocketGuild> socketGuilds = DiscordBot.GetGuilds();

                // Get all guild IDs that we have settings for
                List<ulong> configurationGuilds = new List<ulong>();
                foreach (GuildSettings guildSettings in Configuration.LoadedConfiguration.DiscordConfig.GuildSettings)
                {
                    configurationGuilds.Add(guildSettings.GuildId);
                }

                // Get all IDs for guilds that we are in
                List<ulong> currentGuilds = new List<ulong>();
                foreach (SocketGuild socketGuild in DiscordBot.GetGuilds())
                {
                    currentGuilds.Add(socketGuild.Id);
                }

                // Get all the guilds we have joined
                IEnumerable<ulong> joinedGuilds = currentGuilds.Except(configurationGuilds);
                foreach (ulong id in joinedGuilds)
                {
                    // TODO: find a better solution instead of spamming the Welcome message
                    //await DiscordUtil.ProcessJoinedGuild(socketGuilds.Where(guild => guild.Id == id).FirstOrDefault());
                }

                // Get all the guilds we have been removed from
                IEnumerable<ulong> removedGuilds = configurationGuilds.Except(currentGuilds);
                foreach (ulong id in removedGuilds)
                {
                    await DiscordUtil.ProcessLeftGuild(id, null);
                }
                
                await DiscordBot.LoggingChannel.SendMessageAsync($"**[BootHousekeepingJob]** Saving configuration");

                // Save the configuration
                Configuration.LoadedConfiguration.Write();

                // Schedule the RecurringHousekeepingJob
                await QuartzScheduler.ScheduleJob(TypeUtils.GetSubclassOfType<RecurringHousekeepingJob>(), "Normal", Configuration.LoadedConfiguration.JobSchedules["Recurring"]);

                // Schedule jobs
                await SchedulePostBootJobs();
            }
            catch (Exception e)
            {
                await DiscordUtil.HandleException(e, "in ``BootHousekeepingJob``");
            }
        }

        protected abstract Task RunAppSpecificBootTasks();
        
        protected abstract Task SchedulePostBootJobs();

    }
}