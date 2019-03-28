using Nintendo.Bcat.News.Semantics.Five;
using Nintendo.Bcat.News.Semantics.Three;
using MessagePack;
using System.Collections.Generic;

namespace Nintendo.Bcat.News.Semantics
{
    [MessagePackObject]
    public class NewsFive : News
    {
        [Key("age_limits")]
        public List<AgeLimit> AgeLimits
        {
            get;
            set;
        }

        [Key("body")]
        private List<Body> _Bodies
        {
            get;
            set;
        }

        [Key("rating_information")]
        public List<RatingInformation> RatingInformation
        {
            get;
            set;
        }

        [IgnoreMember]
        public override List<Body> Bodies
        {
            get
            {
                return _Bodies;
            }
            set
            {
                _Bodies = value;
            }
        }

    }
}
