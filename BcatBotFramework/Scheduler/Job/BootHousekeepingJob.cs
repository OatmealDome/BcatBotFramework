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
using BcatBotFramework.Scheduler;

namespace BcatBotFramework.Scheduler.Job
{
    public abstract class BootHousekeepingJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            // Run app-specific boot tasks
            await RunAppSpecificBootTasks();

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

            await DiscordBot.LoggingChannel.SendMessageAsync($"**[BootHousekeepingJob]** Processing deregistrations because of no write permissions");

            await DiscordUtil.FindBadGuilds();

            await DiscordBot.LoggingChannel.SendMessageAsync($"**[BootHousekeepingJob]** Saving configuration");

            // Save the configuration
            Configuration.LoadedConfiguration.Write();

            // Schedule jobs
            await SchedulePostBootJobs();
        }

        protected abstract Task RunAppSpecificBootTasks();
        
        protected abstract Task SchedulePostBootJobs();

    }
}