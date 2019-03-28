using System.Threading.Tasks;
using Discord.WebSocket;
using Quartz;
using BcatBotFramework.Core.Config;
using BcatBotFramework.Social.Discord;

namespace BcatBotFramework.Scheduler.Job
{
    public class DiscordPlayingAlternatorJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            // Increment the current state
            DiscordBot.PlayingState++;

            // Check if we need to loop
            if (DiscordBot.PlayingState == PlayingState.LoopToBeginning)
            {
                DiscordBot.PlayingState = (PlayingState)0;
            }

            // Get the command prefix
            string commandPrefix = Configuration.LoadedConfiguration.DiscordConfig.CommandPrefix;

            // Set the correct text
            string text;
            switch (DiscordBot.PlayingState)
            {
                case PlayingState.UserCount:
                    // Count the total number of users
                    int users = 0;
                    foreach (SocketGuild guild in DiscordBot.GetGuilds())
                    {
                        users += guild.MemberCount;
                    }

                    text = $"with {users} users | {commandPrefix}help";
                    break;
                case PlayingState.Help:
                    text = $"command list | {commandPrefix}help";
                    break;
                case PlayingState.ServerCount:
                    text = $"in {DiscordBot.GetGuilds().Count} servers | {commandPrefix}help";
                    break;
                case PlayingState.Invite:
                    text = $"invite me | {commandPrefix}invite";
                    break;
                default:
                    text = "Error";
                    break;
            }

            // Set the text
            await DiscordBot.SetGameAsync(text);
        }

    }
}