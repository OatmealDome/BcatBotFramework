using MessagePack;

namespace Nintendo.Bcat.News
{
    [MessagePackObject]
    public class Footer
    {
        [Key("text")]
        public string Text
        {
            get;
            set;
        }

    }
}
