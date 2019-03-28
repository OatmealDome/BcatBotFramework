using System.Collections.Generic;
using System.IO;
using Nintendo.Bcat;
using Syroot.BinaryData;

namespace Nintendo.SmashUltimate.Bcat.LineNewsData
{
    public class OneLine
    {
        public uint UnknownOne
        {
            get;
            set;
        }

        public uint UnknownTwo
        {
            get;
            set;
        }

        public uint UnknownThree
        {
            get;
            set;
        }

        public uint UnknownFour
        {
            get;
            set;
        }

        public string Id
        {
            get;
            set;
        }

        public string Key
        {
            get;
            set;
        }

        public Dictionary<Language, string> Text
        {
            get;
            set;
        } = new Dictionary<Language, string>();

        public OneLine(BinaryDataReader reader)
        {
            // Skip padding(?)
            reader.Seek(8);
            
            // Read fields
            UnknownOne = reader.ReadByte();
            UnknownTwo = reader.ReadByte();
            UnknownThree = reader.ReadByte();
            UnknownFour = reader.ReadByte();

            // Create a TemporarySeek to restore this position
            using (reader.TemporarySeek())
            {
                Key = reader.ReadString(StringDataFormat.ZeroTerminated);
            }
            
            // Seek past the empty space
            reader.Seek(0x40, SeekOrigin.Current);

            // Set the Id
            Id = Key.Split('_')[2];
        }

    }
}