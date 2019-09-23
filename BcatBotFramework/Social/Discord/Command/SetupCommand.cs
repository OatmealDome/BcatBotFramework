using System.Threading.Tasks;
using BcatBotFramework.Social.Discord.Interactive.Setup;
using Discord.Commands;
using Nintendo.Bcat;

namespace BcatBotFramework.Social.Discord.Command
{
    public class Setupommand : ModuleBase<SocketCommandContext>
    {
        [Command("setup"), Summary("temp")]
        public async Task Execute()
        {
            // Check if there are already setups running
            if (await DiscordBot.IsSetupFlowRunningInGuild(Context.Guild))
            {
                // Error
                await DiscordUtil.SendErrorMessageByLocalizedDescription(Context.Guild, Context.Channel, "discord.setup.error.already_running");
            }
            else
            {
                // Run the setup
                await DiscordBot.ActivateInteractiveFlow(new SetupFlow(Context.User, Context.Guild, Context.Channel, new Language[] { Language.EnglishUS }));
            }
            
        }
    }
}