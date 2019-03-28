using MessagePack;
using System;
using System.Collections.Generic;

namespace Nintendo.Bcat
{
    [MessagePackObject]
    public class Data
    {
        // Download data specific

        [Key("data_id")]
        public uint _DataId = uint.MinValue;

        [Key("filename")]
        public string Name
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

        [Key("digest")]
        public string Digest
        {
            get;
            set;
        }

        [Key("size")]
        public int Size
        {
            get;
            set;
        }

        // News data specific

        [Key("news_id")]
        public uint _NewsId = uint.MinValue;

        [Key("version")]
        public News.Version Version
        {
            get;
            set;
        }

        [Key("default_language")]
        public string DefaultLanguage
        {
            get;
            set;
        }

        [Key("languages")]
        public List<Bcat.News.DataLanguage> Languages
        {
            get;
            set;
        }

        // Global

        [IgnoreMember]
        public DataType Type
        {
            get
            {
                if (_DataId == uint.MinValue)
                {
                    return DataType.News;
                }
                else
                {
                    return DataType.Download;
                }
            }
            private set
            {
                throw new Exception("Invalid operation");
            }
        }

        [IgnoreMember]
        public uint Id
        {
            get
            {
                if (Type == DataType.Download)
                {
                    return _DataId;
                }
                else
                {
                    return _NewsId;
                }
            }
            private set
            {
                throw new Exception("Invalid operation");
            }
        }

    }
}
