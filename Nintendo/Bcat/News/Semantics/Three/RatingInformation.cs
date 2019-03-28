using MessagePack;

namespace Nintendo.Bcat.News.Semantics.Three
{
    [MessagePackObject]
    public class RatingInformation
    {
        [Key("rating_icon")]
        public byte[] RatingIcon
        {
            get;
            set;
        }

        [Key("contents_descriptors")]
        public string ContentsDescriptors
        {
            get;
            set;
        }

        [Key("interactive_elements")]
        public string InteractiveElements
        {
            get;
            set;
        }

    }
}
