using Nintendo.Bcat.News.Products;
using Nintendo.Bcat.News.Semantics;
using MessagePack;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;

namespace Nintendo.Bcat.News
{
    public abstract class News : TopicInformation
    {
        [Key("version")]
        public Version Version
        {
            get;
            set;
        }

        [Key("news_id")]
        public int Id
        {
            get;
            set;
        }

        [Key("published_at")]
        public ulong PublishTime
        {
            get;
            set;
        }

        [Key("expire_at")]
        public ulong ExpireTime
        {
            get;
            set;
        }

        [Key("pickup_limit")]
        public ulong PickupLimitTime
        {
            get;
            set;
        }

        [Key("priority")]
        public int Priority
        {
            get;
            set;
        }

        [Key("deletion_priority")]
        public int DeletionPriority
        {
            get;
            set;
        }

        [Key("language")]
        public string Language
        {
            get;
            set;
        }

        [Key("supported_languages")]
        public List<string> SupportedLanguages
        {
            get;
            set;
        }

        [Key("display_type")]
        public string DisplayType
        {
            get;
            set;
        }

        [Key("no_photography")]
        public int NoPhotography
        {
            get;
            set;
        }

        [Key("surprise")]
        public int Surprise
        {
            get;
            set;
        }

        [Key("bashotorya")]
        public int Bashotorya
        {
            get;
            set;
        }

        [Key("movie")]
        public int HasMovie
        {
            get;
            set;
        }

        [Key("subject")]
        public Subject Subject
        {
            get;
            set;
        }

        [Key("list_image")]
        public byte[] ListImage
        {
            get;
            set;
        }

        [Key("footer")]
        public Footer Footer
        {
            get;
            set;
        }

        [Key("more")]
        public More More
        {
            get;
            set;
        }

        [Key("related_products")]
        public List<Product> RelatedProducts
        {
            get;
            set;
        }

        [Key("related_channels")]
        public List<TopicInformation> RelatedTopics
        {
            get;
            set;
        }

        [Key("related_movies")]
        public List<Movie> RelatedMovies
        {
            get;
            set;
        }

        [Key("featured_products")]
        public List<FeaturedProduct> FeaturedProducts
        {
            get;
            set;
        }

        [IgnoreMember]
        public abstract List<Body> Bodies
        {
            get;
            set;
        }

        public static News Deserialize(byte[] rawBytes)
        {
            // Deserialize as barebones first so we can read the version info
            NewsBarebones barebones = MessagePackSerializer.Deserialize<NewsBarebones>(rawBytes);

            // Check format
            if (barebones.Version.Format != 1)
            {
                // Unsupported format
                throw new Exception("Unsupported news format " + barebones.Version.Format);
            }

            // Deserialize based on semantics
            switch (barebones.Version.Semantics)
            {
                case 1: // hack
                case 2:
                    return MessagePackSerializer.Deserialize<NewsTwo>(rawBytes, StandardResolverAllowPrivate.Instance);
                case 3:
                case 4: // hack
                    return MessagePackSerializer.Deserialize<NewsThree>(rawBytes, StandardResolverAllowPrivate.Instance);
                case 5:
                    return MessagePackSerializer.Deserialize<NewsFive>(rawBytes, StandardResolverAllowPrivate.Instance);
                default:
                    throw new Exception("Unsupported news semantics " + barebones.Version.Semantics);
            }
        }

    }
}
