using MessagePack;
using System.Collections.Generic;

namespace Nintendo.Bcat.News.Buttons
{
    [MessagePackObject]
    public class GameButton : Button
    {
        [Key("application_ids")]
        public List<ulong> ApplicationIds
        {
            get;
            set;
        }

        [Key("application_arg")]
        public byte[] ApplicationArguments
        {
            get;
            set;
        }

    }
}
