using System.Threading.Tasks;
using BcatBotFramework.Social.Discord.Interactive.Setup;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Nintendo.Bcat;

namespace BcatBotFramework.Social.Discord.Command
{
    public class Setupommand : ModuleBase<SocketCommandContext>
    {
        [Command("setup"), Summary("temp")]
        public async Task Execute(ulong guildId = 0)
        {
            // Check if we are an admin
            bool isAdmin = DiscordUtil.IsAdministrator(Context.User);

            // Check if a guild is specified
            IGuild guild;
            if (guildId != 0)
            {
                // Check if the user does not permission
                if (!DiscordUtil.IsAdministrator(Context.User))
                {
                    // Return an error
                    await DiscordUtil.SendErrorMessageByLocalizedDescription(Context.Guild, Context.Channel, "discord.error.not_admin");

                    return;
                }
                else
                {
                    // Set the guild
                    guild = DiscordBot.GetGuild(guildId);

                    // Check if it exists
                    if (guild == null)
                    {
                        // Return an error
                        await DiscordUtil.SendErrorMessageByLocalizedDescription(Context.Guild, Context.Channel, "discord.setup.error.guild_not_exist");

                        return;
                    }
                }
            }
            else
            {
                guild = Context.Guild;

                // Check if this is a DM
                if (guild == null)
                {
                    // Return an error
                    await DiscordUtil.SendErrorMessageByLocalizedDescription(Context.Guild, Context.Channel, "discord.setup.error.in_dm");

                    return;
                }
            }

            // Check if there are already setups running
            if (await DiscordBot.IsSetupFlowRunningInGuild(guild))
            {
                // Error
                await DiscordUtil.SendErrorMessageByLocalizedDescription(Context.Guild, Context.Channel, "discord.setup.error.already_running");

                return;
            }

            // Run the setup
            await DiscordBot.ActivateInteractiveFlow(new SetupFlow(Context.User, guild, Context.Channel, new Language[] { Language.EnglishUS }, guildId != 0));
        }

    }
}