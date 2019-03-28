using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using BcatBotFramework.Core.Config;
using SmashBcatDetector.Util;

namespace BcatBotFramework.Social.Discord.Precondition
{
    // Inherit from PreconditionAttribute
    public class RequireBotAdministratorPrecondition : PreconditionAttribute
    {
        // Override the CheckPermissions method
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (Configuration.LoadedConfiguration.DiscordConfig.AdministratorIds.Contains(context.User.Id))
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }

            return Task.FromResult(PreconditionResult.FromError("You are not allowed to do this."));
        }
    }

}