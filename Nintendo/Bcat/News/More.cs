using Nintendo.Bcat.News.Buttons;
using MessagePack;

namespace Nintendo.Bcat.News
{
    [MessagePackObject]
    public class More
    {
        [Key("game")]
        public GameButton GameButton
        {
            get;
            set;
        }

        [Key("shop")]
        public ShopButton ShopButton
        {
            get;
            set;
        }

    }
}
