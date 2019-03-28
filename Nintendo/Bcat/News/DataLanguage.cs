using MessagePack;

namespace Nintendo.Bcat.News
{
    [MessagePackObject]
    public class DataLanguage
    {
        [Key("language")]
        public string LanguageCode
        {
            get;
            set;
        }

        [Key("data_id")]
        public int Id
        {
            get;
            set;
        }

        [Key("url")]
        public string Url
        {
            get;
            set;
        }

        [Key("size")]
        public ulong Size
        {
            get;
            set;
        }

    }
}
