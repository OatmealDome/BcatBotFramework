using MessagePack;

namespace Nintendo.Bcat.News.Semantics
{
    [MessagePackObject]
    public class NewsBarebones
    {
        [Key("version")]
        public Version Version
        {
            get;
            set;
        }

    }
}
