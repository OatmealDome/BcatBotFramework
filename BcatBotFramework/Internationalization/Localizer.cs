using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Nintendo.Bcat;

namespace BcatBotFramework.Internationalization
{
    public class Localizer
    {
        private static bool Initialized = false;

        public static void Initialize()
        {

        }

        public static void Dispose()
        {

        }

        public static string Localize(string key, Language language)
        {
            // TODO: Real localization
            return key;
        }

        public static Dictionary<Language, string> LocalizeToAllLanguages(string key)
        {
            // Create a new Dictionary
            Dictionary<Language, string> valueDict = new Dictionary<Language, string>();

            // Populate the Dictionary
            foreach (Language language in Nintendo.SmashUltimate.Bcat.Container.LanguageOrder)
            {
                // Localize the key to this Language
                valueDict.Add(language, Localize(key, language));
            }

            // Return the Dictionary
            return valueDict;
        }

        public static Dictionary<Language, string> LocalizeToAllLanguagesWithFormat(string key, params object[] objs)
        {
            // Localize the key
            Dictionary<Language, string> valueDict = LocalizeToAllLanguages(key);

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
            foreach (Language language in Nintendo.SmashUltimate.Bcat.Container.LanguageOrder)
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
            foreach (Language language in Nintendo.SmashUltimate.Bcat.Container.LanguageOrder)
            {
                // Localize the key to this Language
                valueDict.Add(language, LocalizeDateTime(dateTime, language));
            }

            // Return the Dictionary
            return valueDict;
        }

    }
}