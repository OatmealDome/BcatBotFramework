using System.Collections.Generic;
using Nintendo.Bcat;
using Discord;

namespace BcatBotFramework.Internationalization.Discord
{
    public class LocalizedEmbedBuilder
    {
        // EmbedBuilders for all Languages
        private Dictionary<Language, EmbedBuilder> embedBuilders;

        public LocalizedEmbedBuilder(IEnumerable<Language> supportedLanguages)
        {
            // Create a new Dictionary for EmbedBuilders
            embedBuilders = new Dictionary<Language, EmbedBuilder>();

            // Populate the Dictionary with blank EmbedBuilders
            foreach (Language language in supportedLanguages)
            {
                embedBuilders.Add(language, new EmbedBuilder());
            }
        }

        public LocalizedEmbedBuilder WithTitle(string title)
        {
            // Create a dummy value Dictionary and call the overload
            return WithTitle(Localizer.CreateDummyLocalizedValues(title));
        }

        public LocalizedEmbedBuilder WithTitle(Dictionary<Language, string> localizedTitles)
        {
             // Add the title to each EmbedBuilder
            foreach (KeyValuePair<Language, EmbedBuilder> pair in embedBuilders)
            {
                pair.Value.WithTitle(localizedTitles[pair.Key]);
            }

            // Return ourselves
            return this;
        }

        public LocalizedEmbedBuilder WithDescription(string description)
        {
            // Create a dummy value Dictionary and call the overload
            return WithDescription(Localizer.CreateDummyLocalizedValues(description));
        }

        public LocalizedEmbedBuilder WithDescription(Dictionary<Language, string> localizedTitles)
        {
             // Add the description to each EmbedBuilder
            foreach (KeyValuePair<Language, EmbedBuilder> pair in embedBuilders)
            {
                pair.Value.WithDescription(localizedTitles[pair.Key]);
            }

            // Return ourselves
            return this;
        }

        public LocalizedEmbedBuilder AddField(string nameKey, string valueKey, bool inline = false)
        {
            // Localize the key and call the overload
            return AddField(nameKey, Localizer.LocalizeToAllLanguages(valueKey), inline);
        }

        public LocalizedEmbedBuilder AddField(string nameKey, Dictionary<Language, string> localizedValues, bool inline = false)
        {
            // Localize the name and call the overload
            return AddField(Localizer.LocalizeToAllLanguages(nameKey), localizedValues, inline);
        }

        public LocalizedEmbedBuilder AddField(Dictionary<Language, string> localizedNames, Dictionary<Language, string> localizedValues, bool inline = false)
        {
            // Add the field to each EmbedBuilder
            foreach (KeyValuePair<Language, EmbedBuilder> pair in embedBuilders)
            {
                pair.Value.AddField(localizedNames[pair.Key], localizedValues[pair.Key], inline);
            }

            // Return ourselves
            return this;
        }

        public LocalizedEmbedBuilder WithImageUrl(string url)
        {
            // Add the image to each EmbedBuilder
            foreach (KeyValuePair<Language, EmbedBuilder> pair in embedBuilders)
            {
                pair.Value.WithImageUrl(url);
            }

            // Return ourselves
            return this;
        }

        public Dictionary<Language, Embed> Build()
        {
            // Create a new Dictionary
            Dictionary<Language, Embed> embeds = new Dictionary<Language, Embed>();

            // Build every embed
            foreach (KeyValuePair<Language, EmbedBuilder> pair in embedBuilders)
            {
                embeds.Add(pair.Key, pair.Value.Build());
            }

            // Return the Dictionary
            return embeds;
        }

    }
}