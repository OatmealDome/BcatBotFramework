using Nintendo.Bcat.News.Semantics.Three;
using MessagePack;
using System;
using System.Collections.Generic;

namespace Nintendo.Bcat.News.Semantics
{
    [MessagePackObject]
    public class NewsThree : News
    {
        [Key("body")]
        private Body _Body
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
                // Return a list with one entry
                return new List<Body>
                {
                    _Body
                };
            }
            set
            {
                // The list must only be one entry
                if (value.Count != 1)
                {
                    throw new Exception($"News using semantics {this.Version.Semantics} can only support one Body");
                }

                // Set the _Body property to the single entry
                _Body = value[0];
            }
        }

    }
}
