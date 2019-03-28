using System;

namespace Nintendo.Bcat
{
    public enum Language
    {
        Chinese, // what's the diff between this and traditional?
        ChineseSimplified,
        ChineseTaiwan,
        ChineseTraditional,
        Dutch,
        EnglishUS,
        EnglishUK,
        FrenchFR,
        FrenchCA,
        German,
        Italian,
        Japanese,
        Korean,
        Portuguese,
        Russian,
        SpanishES,
        SpanishLA,
    }

    public static class LanguageExtensions
    {
        public static string GetCode(this Language language)
        {
            switch (language)
            {
                case Language.Chinese:
                    return "zh-CN";
                case Language.ChineseSimplified:
                    return "zh-Hans";
                case Language.ChineseTaiwan:
                    return "zh-TW";
                case Language.ChineseTraditional:
                    return "zh-Hant";
                case Language.Dutch:
                    return "nl";
                case Language.EnglishUS:
                    return "en-US";
                case Language.EnglishUK:
                    return "en-GB";
                case Language.FrenchFR:
                    return "fr";
                case Language.FrenchCA:
                    return "fr-CA";
                case Language.German:
                    return "de";
                case Language.Italian:
                    return "it";
                case Language.Japanese:
                    return "ja";
                case Language.Korean:
                    return "ko";
                case Language.Portuguese:
                    return "pt";
                case Language.Russian:
                    return "ru";
                case Language.SpanishES:
                    return "es";
                case Language.SpanishLA:
                    return "es-419";
                default:
                    throw new Exception("Invalid language");
            }
        }

        public static Language FromCode(string code)
        {
            switch (code.ToLower())
            {
                case "zh-cn":
                    return Language.Chinese;
                case "zh-hans":
                    return Language.ChineseSimplified;
                case "zh-tw":
                    return Language.ChineseTaiwan;
                case "zh-hant":
                    return Language.ChineseTraditional;
                case "nl":
                    return Language.Dutch;
                case "en":
                case "en-us":
                    return Language.EnglishUS;
                case "en-gb":
                    return Language.EnglishUK;
                case "fr":
                    return Language.FrenchFR;
                case "fr-ca":
                    return Language.FrenchCA;
                case "de":
                    return Language.German;
                case "it":
                    return Language.Italian;
                case "ja":
                    return Language.Japanese;
                case "ko":
                    return Language.Korean;
                case "pt":
                    return Language.Portuguese;
                case "ru":
                    return Language.Russian;
                case "es":
                    return Language.SpanishES;
                case "es-419":
                    return Language.SpanishLA;
                default:
                    throw new Exception("Unsupported language code");
            }
        }

        public static string ToCultureInfoCode(this Language language)
        {
            switch (language)
            {
                case Language.ChineseSimplified:
                    return "zh-CN";
                case Language.ChineseTraditional:
                    return "zh-HK";
                case Language.Dutch:
                    return "nl-NL";
                case Language.FrenchFR:
                    return "fr-FR";
                case Language.German:
                    return "de-DE";
                case Language.Italian:
                    return "it-IT";
                case Language.Japanese:
                    return "ja-JP";
                case Language.Korean:
                    return "ko-KR";
                case Language.Portuguese:
                    return "pl-PT";
                case Language.Russian:
                    return "ru-RU";
                case Language.SpanishES:
                    return "es-ES";
                case Language.SpanishLA:
                    return "es-MX";
                default:
                    return language.GetCode();
            }
        }

        public static Language[] GetAllLanguages()
        {
            return (Language[])Enum.GetValues(typeof(Language));
        }

    }
}
