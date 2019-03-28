using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nintendo.Bcat;
using Nintendo.Text;
using Syroot.BinaryData;

namespace Nintendo.SmashUltimate.Bcat
{
    public class Present : Container
    {
        public string TitleKey
        {
            get;
            set;
        }

        public string ContentKey
        {
            get;
            set;
        }

        public Dictionary<Language, string> TitleText
        {
            get;
            set;
        } = new Dictionary<Language, string>();

        public Dictionary<Language, string> ContentText
        {
            get;
            set;
        } = new Dictionary<Language, string>();

        public byte[] Image
        {
            get;
            set;
        }
        
        public uint SpiritId
        {
            get;
            set;
        }

        public Present(Stream stream) : base(stream)
        {
            using (BinaryDataReader reader = new BinaryDataReader(stream, true))
            {
                // Read the title key
                reader.Seek(0x50, SeekOrigin.Begin);
                TitleKey = reader.ReadString(StringDataFormat.ZeroTerminated, Encoding.ASCII);

                // Read the content key
                reader.Seek(0x90, SeekOrigin.Begin);
                ContentKey = reader.ReadString(StringDataFormat.ZeroTerminated, Encoding.ASCII);

                // Parse the ID from the title key
                Id = TitleKey.Split("_").Last();

                // Seek to the data
                reader.Seek(0xF0, SeekOrigin.Begin);

                // Read the MSBTs
                Dictionary<Language, Dictionary<string, string>> msbts = this.ReadMsbts(reader);

                // Loop over every MSBT combination
                foreach (KeyValuePair<Language, Dictionary<string, string>> pair in msbts)
                {
                    // Add the title and content to their respective Dictionaries
                    TitleText.Add(pair.Key, pair.Value[TitleKey]);
                    ContentText.Add(pair.Key, pair.Value[ContentKey]);
                }
                
                // Read the image
                Image = this.ReadDataEntry(reader);

                // Seek to the unknown
                reader.Seek(0x174, SeekOrigin.Begin);
                SpiritId = reader.ReadUInt32();
            }
        }

    }
}