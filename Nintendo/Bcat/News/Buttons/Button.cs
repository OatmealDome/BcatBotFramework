using MessagePack;

namespace Nintendo.Bcat.News.Buttons
{
    [MessagePackObject]
    public class Button
    {
        [Key("query")]
        public string Query
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
