using MessagePack;

namespace Nintendo.Bcat.News.Products
{
    [MessagePackObject]
    public class Product
    {
        [Key("query")]
        public string Query
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

        [Key("image")]
        public byte[] Image
        {
            get;
            set;
        }

    }
}
