using System.Collections.Generic;
using Nintendo.Bcat.News.Catalog;
using MessagePack;

namespace Nintendo.Bcat.News
{
    [MessagePackObject]
    public class Detail : Entry
    {
        [Key("latest_news_urls")]
        public List<string> LatestNewsUrls
        {
            get;
            set;
        }
        
    }
}