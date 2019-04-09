using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BcatBotFramework.Core.Config;
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

            // Populate the Dictionary for each Languages
            foreach (Language language in LanguageExtensions.GetAllLanguages())
            {
                // Create the localization file path
                string path = Path.Combine(Configuration.LoadedConfiguration.LocalizerConfig.LocalizationsDirectory, language.GetCode() + ".json");

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

        public static void Dispose()
        {

        }

        public static string Localize(string key, Language language)
        {
            // Try to get the localization
            if (Localizations[language].TryGetValue(key, out string value))
            {
                // Return the value
                return value;
            }

            // Try to get the localization in en-US
            if (Localizations[Language.EnglishUS].TryGetValue(key, out string englishValue))
            {
                // Return the value
                return englishValue;
            }

            // TODO: Real localization
            return $"String not found ({key}) - report this bug to OatmealDome";
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