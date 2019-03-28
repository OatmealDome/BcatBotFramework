using MessagePack;

namespace Nintendo.Bcat.News
{
    [MessagePackObject]
    public class Movie
    {
        [Key("movie_url")]
        public string Url
        {
            get;
            set;
        }

        [Key("movie_name")]
        public string Name
        {
            get;
            set;
        }

        [Key("movie_image")]
        public byte[] Image
        {
            get;
            set;
        }

    }
}
