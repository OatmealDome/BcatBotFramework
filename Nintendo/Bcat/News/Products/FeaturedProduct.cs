using MessagePack;

namespace Nintendo.Bcat.News.Products
{
    [MessagePackObject]
    public class FeaturedProduct : Product
    {
        [Key("publisher")]
        public string Publisher
        {
            get;
            set;
        }

        [Key("demo_info")]
        public string DemoInfo
        {
            get;
            set;
        }

        [Key("comment")]
        public string Comment
        {
            get;
            set;
        }

    }
}
