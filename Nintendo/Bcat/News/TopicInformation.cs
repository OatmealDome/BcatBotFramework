using MessagePack;

namespace Nintendo.Bcat.News
{
    [MessagePackObject]
    public class TopicInformation
    {
        [Key("topic_id")]
        public string TopicId
        {
            get;
            set;
        }

        [Key("topic_name")]
        public string TopicName
        {
            get;
            set;
        }

        [Key("topic_image")]
        public byte[] TopicImage
        {
            get;
            set;
        }

        [Key("topic_publisher")]
        public string TopicPublisher
        {
            get;
            set;
        }

        [Key("topic_description")]
        public string TopicDescription
        {
            get;
            set;
        }

        [Key("topic_important")]
        public int TopicImportant
        {
            get;
            set;
        }

    }
}
