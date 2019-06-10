using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace BcatBotFramework.Social.Discord.Command
{
    public class VersionCommand : ModuleBase<SocketCommandContext>
    {
        [Command("version"), Summary("Prints version details")]
        public async Task Execute()
        {
            string commitStr = $"{ThisAssembly.Git.Branch}-{ThisAssembly.Git.Commit}";
            if (ThisAssembly.Git.IsDirty)
            {
                commitStr += "-dirty";
            }

            Embed embed = new EmbedBuilder()
                .WithTitle("Version")
                .WithDescription(commitStr)
                .WithColor(Color.Purple)
                .Build();

            await Context.Channel.SendMessageAsync(embed: embed);
        }
        
    }
}