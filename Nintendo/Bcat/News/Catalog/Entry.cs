using MessagePack;

namespace Nintendo.Bcat.News.Catalog
{
    [MessagePackObject]
    public class Entry
    {
        [Key("topic_id")]
        public string TopicId
        {
            get;
            set;
        }

        [Key("name")]
        public string Name
        {
            get;
            set;
        }

        [Key("publisher")]
        public string Publisher
        {
            get;
            set;
        }

        [Key("description")]
        public string Description
        {
            get;
            set;
        }

        [Key("publishing_time")]
        public ulong PublishingTime
        {
            get;
            set;
        }

        [Key("last_posted_at")]
        public ulong LastPostedAt
        {
            get;
            set;
        }

        [Key("important")]
        public bool Important
        {
            get;
            set;
        }

    }
}
