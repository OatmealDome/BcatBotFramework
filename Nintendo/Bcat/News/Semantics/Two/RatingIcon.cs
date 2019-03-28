using MessagePack;
using System;

namespace Nintendo.Bcat.News
{
    [MessagePackObject]
    public class RatingIcon
    {
        [Key("ESRB")]
        private byte[] _ESRB;

        [Key("PEGI")]
        private byte[] _PEGI;

        [Key("CERO")]
        private byte[] _CERO;

        [IgnoreMember]
        public RatingIconType IconType
        {
            get
            {
                if (_ESRB != null)
                {
                    return RatingIconType.ESRB;
                }
                else if (_PEGI != null)
                {
                    return RatingIconType.PEGI;
                }
                else
                {
                    return RatingIconType.CERO;
                }
            }
            set
            {
                if (value == RatingIconType.ESRB)
                {
                    _ESRB = new byte[0];
                    _PEGI = null;
                    _CERO = null;
                }
                else if (value == RatingIconType.PEGI)
                {
                    _ESRB = null;
                    _PEGI = new byte[0];
                    _CERO = null;
                }
                else if (value == RatingIconType.CERO)
                {
                    _ESRB = null;
                    _PEGI = null;
                    _CERO = new byte[0];
                }
            }
        }

        [IgnoreMember]
        public byte[] Image
        {
            get
            {
                switch (IconType)
                {
                    case RatingIconType.ESRB:
                        return _ESRB;
                    case RatingIconType.PEGI:
                        return _PEGI;
                    case RatingIconType.CERO:
                        return _CERO;
                    default:
                        throw new Exception("Invalid RatingIconType");
                }
            }
            set
            {
                switch (IconType)
                {
                    case RatingIconType.ESRB:
                         _ESRB = value;
                        break;
                    case RatingIconType.PEGI:
                        _PEGI = value;
                        break;
                    case RatingIconType.CERO:
                        _CERO = value;
                        break;
                    default:
                        throw new Exception("Invalid RatingIconType");
                }
            }
        }

    }
}
