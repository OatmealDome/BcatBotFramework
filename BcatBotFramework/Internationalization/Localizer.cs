using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BcatBotFramework.Core.Config;
using BcatBotFramework.Social.Discord;
using Newtonsoft.Json;
using Nintendo.Bcat;

namespace BcatBotFramework.Internationalization
{
    public class Localizer
    {
        private static Dictionary<Language, Dictionary<string, string>> Localizations;
        private static bool Initialized = false;

        public static void Initialize()
        {
            Localizations = new Dictionary<Language, Dictionary<string, string>>();

            // Check if the new framework-specific strings directory is available
            if (System.IO.Directory.Exists(Path.Combine(Configuration.LoadedConfiguration.LocalizerConfig.LocalizationsDirectory, "framework")))
            {
                // Populate the Dictionary for each Language
                foreach (Language language in LanguageExtensions.GetAllLanguages())
                {
                    // Read the framework JSONs
                    string path = Path.Combine(Configuration.LoadedConfiguration.LocalizerConfig.LocalizationsDirectory, "framework", language.GetCode() + ".json");

                    // Check if the file exists
                    if (File.Exists(path))
                    {
                        // Load the file
                        Localizations[language] = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path));
                    }
                    else
                    {
                        // Create a new blank Dictionary
                        Localizations[language] = new Dictionary<string, string>();
                    }
                }
            }

            // Populate the Dictionary for each Language
            foreach (Language language in LanguageExtensions.GetAllLanguages())
            {
                // Create the localization file path
                string path = Path.Combine(Configuration.LoadedConfiguration.LocalizerConfig.LocalizationsDirectory, "application", language.GetCode() + ".json");

                // Check if the file exists
                if (File.Exists(path))
                {
                    // Load the file
                    Dictionary<string, string> applicationStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path));

                    // Get every pair
                    foreach (KeyValuePair<string, string> pair in applicationStrings)
                    {
                        // Check if this key does not exist
                        if (!Localizations[language].TryGetValue(pair.Key, out string existTestVal))
                        {
                            // Write the key into the master Dictionary
                            Localizations[language][pair.Key] = pair.Value;
                        }
                    }
                }
            }

            // Check that the localizable missing message is available
            if (!Localizations[Language.EnglishUS].TryGetValue("localizer.missing_localizable", out string missingMessage))
            {
                throw new Exception("Special message for missing localizable (\"localizer.missing_localizable\") is missing");
            }
        }

        public static void Dispose()
        {

        }

        public static string Localize(string key, Language language)
        {
            // Try to get the localization
            if (Localizations[language].TryGetValue(key, out string value))
            {
                // Return the value
                return ParseSpecialStrings(value, language);
            }

            // Try to get the localization in en-US
            if (Localizations[Language.EnglishUS].TryGetValue(key, out string englishValue))
            {
                // Return the value
                return ParseSpecialStrings(englishValue, language);
            }

            // Return the error message
            return string.Format(Localize("localizer.missing_localizable", language), key);
        }

        public static string ParseSpecialStrings(string localizedValue, Language language)
        {
            // Match every special string
            foreach (Match match in Regex.Matches(localizedValue, @"{\$.*?}"))
            {
                // Remove the brackets and dollar sign
                string specialStr = match.Value.Substring(2, match.Value.Length - 3);

                // Get the appropriate replacement
                string replacement;
                switch (specialStr)
                {
                    case "BOT_NAME":
                        replacement = DiscordBot.GetName();
                        break;
                    case "BOT_COMMAND_PREFIX": 
                        replacement = Configuration.LoadedConfiguration.DiscordConfig.CommandPrefix;
                        break;
                    default:
                        replacement = Localizer.Localize("localizer.bad_special_string", language);
                        break;
                }

                // Replace the special string
                localizedValue = localizedValue.Replace(match.Value, replacement);
            }

            return localizedValue;
        }

        public static Dictionary<Language, string> LocalizeToAllLanguages(string key)
        {
            return LocalizeToAllLanguages(key, LanguageExtensions.GetAllLanguages());
        }

        public static Dictionary<Language, string> LocalizeToAllLanguages(string key, IEnumerable<Language> languages)
        {
            // Create a new Dictionary
            Dictionary<Language, string> valueDict = new Dictionary<Language, string>();

            // Populate the Dictionary
            foreach (Language language in languages)
            {
                // Localize the key to this Language
                valueDict.Add(language, Localize(key, language));
            }

            // Return the Dictionary
            return valueDict;
        }

        public static Dictionary<Language, string> LocalizeToAllLanguagesWithFormat(string key, params object[] objs)
        {
            return LocalizeToAllLanguagesWithFormat(key, LanguageExtensions.GetAllLanguages(), objs);
        }

        public static Dictionary<Language, string> LocalizeToAllLanguagesWithFormat(string key, IEnumerable<Language> languages, params object[] objs)
        {
            // Localize the key
            Dictionary<Language, string> valueDict = LocalizeToAllLanguages(key, languages);

            // Format the values
            foreach (KeyValuePair<Language, string> pair in valueDict.ToList())
            {
                valueDict[pair.Key] = string.Format(pair.Value, objs);
            }

            // Return the Dictionary
            return valueDict;
        }

        public static Dictionary<Language, string> LocalizeToAllLanguagesWithFormat(string key, Dictionary<Language, object[]> param)
        {
            return LocalizeToAllLanguagesWithFormat(key, LanguageExtensions.GetAllLanguages(), param);
        }

        public static Dictionary<Language, string> LocalizeToAllLanguagesWithFormat(string key, IEnumerable<Language> languages, Dictionary<Language, object[]> param)
        {
            // Localize the key
            Dictionary<Language, string> valueDict = LocalizeToAllLanguages(key, languages);

            // Format the values
            foreach (KeyValuePair<Language, string> pair in valueDict.ToList())
            {
                valueDict[pair.Key] = string.Format(pair.Value, param[pair.Key]);
            }

            return valueDict;
        }

        public static Dictionary<Language, string> CreateDummyLocalizedValues(string value)
        {
            // Create a new Dictionary
            Dictionary<Language, string> valueDict = new Dictionary<Language, string>();

            // Populate the Dictionary
            foreach (Language language in LanguageExtensions.GetAllLanguages())
            {
                // Localize the key to this Language
                valueDict.Add(language, value);
            }

            // Return the Dictionary
            return valueDict;
        }

        public static string LocalizeDateTime(DateTime dateTime, Language language)
        {
            // Get the CultureInfo
            CultureInfo cultureInfo = new CultureInfo(language.ToCultureInfoCode());

            // Return the localized DateTime
            return dateTime.ToString("f", cultureInfo) + " UTC";
        }

        public static Dictionary<Language, string> LocalizeDateTimeToAllLanguages(DateTime dateTime)
        {
            // Create a new Dictionary
            Dictionary<Language, string> valueDict = new Dictionary<Language, string>();

            // Populate the Dictionary
            foreach (Language language in LanguageExtensions.GetAllLanguages())
            {
                // Localize the key to this Language
                valueDict.Add(language, LocalizeDateTime(dateTime, language));
            }

            // Return the Dictionary
            return valueDict;
        }

    }
}