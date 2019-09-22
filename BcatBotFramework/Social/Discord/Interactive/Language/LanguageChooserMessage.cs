using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BcatBotFramework.Internationalization;
using BcatBotFramework.Social.Discord;
using Discord;
using Discord.WebSocket;
using Nintendo.Bcat;

namespace BcatBotFramework.Social.Discord.Interactive.Setup
{
    public class LanguageChooserMessage : InteractiveMessage
    {
        // Emotes
        private static IEmote EMOTE_AMERICAN_ENGLISH = new Emoji("\uD83C\uDDFA\uD83C\uDDF8"); // 🇺🇸
        private static IEmote EMOTE_CANADIAN_FRENCH = new Emoji("\uD83C\uDDE8\uD83C\uDDE6"); // 🇨🇦
        private static IEmote EMOTE_LATIN_AMERICAN_SPANISH = new Emoji("\uD83C\uDDF2\uD83C\uDDFD"); // 🇲🇽 
        private static IEmote EMOTE_BRITISH_ENGLISH = new Emoji("\uD83C\uDDEC\uD83C\uDDE7"); // 🇬🇧
        private static IEmote EMOTE_FRENCH = new Emoji("\uD83C\uDDEB\uD83C\uDDF7"); // 🇫🇷
        private static IEmote EMOTE_DUTCH = new Emoji("\uD83C\uDDF3\uD83C\uDDF1"); // 🇳🇱 
        private static IEmote EMOTE_GERMAN = new Emoji("\uD83C\uDDE9\uD83C\uDDEA"); // 🇩🇪 
        private static IEmote EMOTE_ITALY = new Emoji("\uD83C\uDDEE\uD83C\uDDF9"); // 🇮🇹 
        private static IEmote EMOTE_RUSSIAN = new Emoji("\uD83C\uDDF7\uD83C\uDDFA"); // 🇷🇺
        private static IEmote EMOTE_SPANISH = new Emoji("\uD83C\uDDEA\uD83C\uDDF8"); // 🇪🇸
        private static IEmote EMOTE_PORTUGUESE = new Emoji("\uD83C\uDDF5\uD83C\uDDF9"); // 🇵🇹
        private static IEmote EMOTE_JAPANESE = new Emoji("\uD83C\uDDEF\uD83C\uDDF5"); // 🇯🇵 
        private static IEmote EMOTE_KOREAN = new Emoji("\uD83C\uDDF0\uD83C\uDDF7"); // 🇰🇷 
        private static IEmote EMOTE_SIMPLIFIED_CHINESE = new Emoji("\uD83C\uDDE8\uD83C\uDDF3"); // 🇨🇳 
        private static IEmote EMOTE_TRADITIONAL_CHINESE = new Emoji("\uD83C\uDDED\uD83C\uDDF0"); // 🇭🇰 

        private static readonly List<Tuple<Language, IEmote>> LanguageEmotes = new List<Tuple<Language, IEmote>>()
        {
            new Tuple<Language, IEmote>(Language.EnglishUS, EMOTE_AMERICAN_ENGLISH),
            new Tuple<Language, IEmote>(Language.FrenchCA, EMOTE_CANADIAN_FRENCH),
            new Tuple<Language, IEmote>(Language.SpanishLA, EMOTE_LATIN_AMERICAN_SPANISH),
            new Tuple<Language, IEmote>(Language.EnglishUK, EMOTE_BRITISH_ENGLISH),
            new Tuple<Language, IEmote>(Language.FrenchFR, EMOTE_FRENCH),
            new Tuple<Language, IEmote>(Language.Dutch, EMOTE_DUTCH),
            new Tuple<Language, IEmote>(Language.German, EMOTE_GERMAN),
            new Tuple<Language, IEmote>(Language.Italian, EMOTE_ITALY),
            new Tuple<Language, IEmote>(Language.Russian, EMOTE_RUSSIAN),
            new Tuple<Language, IEmote>(Language.SpanishES, EMOTE_SPANISH),
            new Tuple<Language, IEmote>(Language.Portuguese, EMOTE_PORTUGUESE),
            new Tuple<Language, IEmote>(Language.Japanese, EMOTE_JAPANESE),
            new Tuple<Language, IEmote>(Language.Korean, EMOTE_KOREAN),
            new Tuple<Language, IEmote>(Language.ChineseSimplified, EMOTE_SIMPLIFIED_CHINESE),
            new Tuple<Language, IEmote>(Language.ChineseTraditional, EMOTE_TRADITIONAL_CHINESE)
        };

        private static readonly Dictionary<Language, string> LanguageNames = new Dictionary<Language, string>()
        {
            { Language.EnglishUS, "English (American)" },
            { Language.FrenchCA, "Français (Canadien)" },
            { Language.SpanishLA, "Español (América Latina)" },
            { Language.EnglishUK, "English (British)" },
            { Language.FrenchFR, "Français" },
            { Language.Dutch, "Nederlands" },
            { Language.German, "Deutsch" },
            { Language.Italian, "Italiano" },
            { Language.Russian, "Pусский" },
            { Language.SpanishES, "Español" },
            { Language.Portuguese, "Português" },
            { Language.Japanese, "日本語" },
            { Language.Korean, "한국어" },
            { Language.ChineseSimplified, "簡體中文" },
            { Language.ChineseTraditional, "繁體中文" }
        };

        private string LocalizedPrompt;
        private IEnumerable<Language> ValidLanguages;
        private Func<Language, Task> Callback;

        public LanguageChooserMessage(IUser user, IUserMessage message, IEnumerable<Language> validLangs, Func<Language, Task> callback, string promptLocalizable = null, Language? language = null) : base(user, message)
        {
            LocalizedPrompt = (promptLocalizable != null && language.HasValue) ? Localizer.Localize(promptLocalizable, language.Value) : Localizer.Localize("discord.language_chooser.default_prompt", Language.EnglishUS);
            ValidLanguages = validLangs;
            Callback = callback;
        }

        public LanguageChooserMessage(IUser user, IEnumerable<Language> validLangs, Func<Language, Task> callback, string promptLocalizable = null, Language? language = null) : this(user, null, validLangs, callback, promptLocalizable, language)
        {

        }

        public override async Task AddReactions()
        {
            //await TargetMessage.AddReactionsAsync(LanguageEmotes.Where(x => ValidLanguages.Contains(x.Item1)) // Get all from ValidLangauges
            //    .Select(x => x.Item2) // Get all IEmotes
            //    .ToArray());

            foreach (Language language in ValidLanguages)
            {
                await TargetMessage.AddReactionAsync(LanguageEmotes.Where(x => x.Item1 == language).FirstOrDefault().Item2);
            }
        }

        public override MessageProperties CreateMessageProperties()
        {
            // Create the description
            string description = $"{LocalizedPrompt}\n\n";
            foreach (Language language in ValidLanguages)
            {
                description += $"{LanguageEmotes.Where(x => x.Item1 == language).FirstOrDefault().Item2.Name} - {LanguageNames[language]}\n";
            }

            return new MessageProperties()
            {
                Content = null,
                Embed = new EmbedBuilder()
                            .WithTitle("Select Language")
                            .WithDescription(description)
                            .Build()
            };
        }

        public override async Task<bool> HandleReaction(IEmote emote)
        {
            // Run the callback
            await Callback(LanguageEmotes.Where(x => x.Item2.Name == emote.Name).FirstOrDefault().Item1);
            
            return false;
        }

        public override Task<bool> HandleTextMessage(SocketMessage message)
        {
            return Task.FromResult(false);
        }

    }
}