using MessagePack;

namespace Nintendo.Bcat.News.Semantics.Five
{
    [MessagePackObject]
    public class Rating
    {
        [Key("name")]
        public string Name
        {
            get;
            set;
        }

        [Key("age")]
        public int Age
        {
            get;
            set;
        }

    }
}
