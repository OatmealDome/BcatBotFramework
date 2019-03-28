using MessagePack;
using System;

namespace Nintendo.Bcat.News
{
    [MessagePackObject]
    public class Body
    {
        [Key("text")]
        public string Text
        {
            get;
            set;
        }

        [Key("main_image_height")]
        public int MainImageHeight
        {
            get;
            set;
        }

        [Key("movie_url")]
        public string MovieUrl
        {
            get;
            set;
        }

        [Key("main_image")]
        public byte[] MainImage
        {
            get;
            set;
        }

    }
}
