using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BcatBotFramework.Core;
using BcatBotFramework.Core.Config;
using BcatBotFramework.Internationalization;
using BcatBotFramework.Social.Discord.Settings;
using Discord;
using Discord.WebSocket;
using Nintendo.Bcat;

namespace BcatBotFramework.Social.Discord.Interactive.Setup
{
    public class SetupFlow : InteractiveFlow
    {
        public SetupFlowMode Mode
        {
            get;
            set;
        }

        public IGuild Guild
        {
            get;
            private set;
        }

        public IEnumerable<Language> ValidLanguages
        {
            get;
            private set;
        }

        public GuildSettings GuildSettings
        {
            get;
            private set;
        }

        public ulong TargetChannelId
        {
            get;
            set;
        }

        public DynamicSettingsData ChannelSettings
        {
            get;
            set;
        }

        public Language DefaultLanguage
        {
            get;
            private set;
        }

        public bool IsAdminMode
        {
            get;
            private set;
        } = false;
        
        public string ModeSelectPrePromptLocalizable
        {
            get;
            set;
        } = null;

        public bool ShouldSendNewMessage
        {
            get;
            set;
        } = false;

        public SetupFlow(IUser user, IGuild guild, ISocketMessageChannel channel, IEnumerable<Language> validLangs, bool isAdmin) : base(user, channel)
        {
            Guild = guild;
            ValidLanguages = validLangs;
            DefaultLanguage = DiscordUtil.GetDefaultLanguage(guild, channel);
            IsAdminMode = isAdmin;
        }

        public override async Task SetPageImpl(int page)
        {
            // Deactivate the previous interactive message if necessary
            if (CurrentInteractiveMessage != null)
            {
                await DiscordBot.DeactivateInteractiveMessage(CurrentInteractiveMessage);
            }

            // Check if a new message should be sent
            IUserMessage messageToModify;
            if (ShouldSendNewMessage || CurrentInteractiveMessage == null)
            {
                messageToModify = null;
            }
            else
            {
                messageToModify = CurrentInteractiveMessage.TargetMessage;
            }

            // Flip the message flag if needed
            if (ShouldSendNewMessage)
            {
                ShouldSendNewMessage = false;
            }
            
            switch ((SetupFlowPage)page)
            {
                case SetupFlowPage.Root:
                    // Check if a GuildSettings instance exists
                    GuildSettings = Configuration.LoadedConfiguration.DiscordConfig.GuildSettings.Where(g => g.GuildId == Guild.Id).FirstOrDefault();
                    if (GuildSettings != null && GuildSettings.ChannelSettings.Count != 0)
                    {
                        // Go to mode select
                        goto case SetupFlowPage.ModeSelect;
                    }
                    else
                    {
                        // Create a new GuildSettings instance
                        GuildSettings = new GuildSettings(Guild.Id);
                        Configuration.LoadedConfiguration.DiscordConfig.GuildSettings.Add(GuildSettings);

                        // Set mode to add initial
                        Mode = SetupFlowMode.AddInitial;

                        // Go to enter channel
                        goto case SetupFlowPage.ServerLanguageSelect;
                    }
                case SetupFlowPage.ModeSelect:
                    // Reset some variables
                    TargetChannelId = 0;
                    ChannelSettings = null;

                    CurrentInteractiveMessage = new SetupModeSelectMessage(this, this.User, messageToModify);
                    
                    break;
                case SetupFlowPage.ServerLanguageSelect:
                    // Check if there is one or no valid language
                    if (ValidLanguages.Count() <= 1)
                    {
                        // Set the only language
                        GuildSettings.SetSetting("default_language", ValidLanguages.FirstOrDefault());

                        // Set this SetupFlow's language
                        DefaultLanguage = ValidLanguages.FirstOrDefault();

                        // Skip to select notifications
                        goto case SetupFlowPage.EnterChannel;
                    }

                    CurrentInteractiveMessage = new LanguageChooserMessage(User, messageToModify, ValidLanguages, async (language) =>
                    {
                        // Set chosen language
                        GuildSettings.SetSetting("default_language", language);

                        // Set this SetupFlow's language
                        DefaultLanguage = language;

                        // Go to next page
                        await SetPage((int)SetupFlowPage.EnterChannel);
                    }, "discord.setup.language_chooser.server", Language.EnglishUS);

                    break;
                case SetupFlowPage.ChannelChooser:
                    CurrentInteractiveMessage = new SetupChannelChooserMessage(this, this.User, messageToModify);

                    break;
                case SetupFlowPage.EnterChannel:
                    CurrentInteractiveMessage = new SetupEnterChannelMessage(this, this.User, messageToModify);

                    break;
                case SetupFlowPage.SelectEditMode:
                    CurrentInteractiveMessage = new SetupEditModeSelectMessage(this, this.User, messageToModify);
                    
                    break;
                case SetupFlowPage.SelectLanguage:
                    // Check if there is one or no valid language
                    if (ValidLanguages.Count() <= 1)
                    {
                        // Set the only language
                        ChannelSettings.SetSetting("language", ValidLanguages.FirstOrDefault());

                        // Skip to select notifications
                        goto case SetupFlowPage.SelectNotifications;
                    }

                    CurrentInteractiveMessage = new LanguageChooserMessage(User, messageToModify, ValidLanguages, async (language) =>
                    {
                        // Set chosen language
                        ChannelSettings.SetSetting("language", language);

                        // Check if the message to modify is null
                        if (messageToModify == null)
                        {
                            // Reflip the new message flag
                            ShouldSendNewMessage = true;
                        }

                        // Go to next page
                        await SetPage((int)SetupFlowPage.SelectNotifications);
                    }, "discord.setup.language_chooser.channel", DefaultLanguage);

                    break;
                case SetupFlowPage.SelectNotifications:
                    CurrentInteractiveMessage = (InteractiveMessage)Activator.CreateInstance(TypeUtils.GetSubclassOfType<SetupNotificationsSelectorMessage>(), this, this.User, messageToModify);

                    break;
                case SetupFlowPage.Exit:
                    await CurrentInteractiveMessage.TargetMessage.ModifyAsync(p =>
                    {
                        p.Embed = new EmbedBuilder()
                                    .WithTitle(Localizer.Localize("discord.setup.goodbye.title", DefaultLanguage))
                                    .WithDescription(Localizer.Localize(LastPageIdx != (int)SetupFlowPage.ChannelChooser ? "discord.setup.goodbye.prompt" : "discord.setup.goodbye.prompt_last_deleted", DefaultLanguage))
                                    .Build();
                    });

                    await DiscordBot.DeactivateInteractiveFlow(this);

                    return;
                default:
                    throw new Exception("Invalid Setup page");
            }

            await DiscordBot.SendInteractiveMessageAsync(this.Channel, CurrentInteractiveMessage);
        }

    }
}