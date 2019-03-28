using MessagePack;

namespace Nintendo.Bcat.News
{
    [MessagePackObject]
    public class Subject
    {
        [Key("caption")]
        public int Caption
        {
            get;
            set;
        }

        [Key("text")]
        public string Text
        {
            get;
            set;
        }

    }
}
