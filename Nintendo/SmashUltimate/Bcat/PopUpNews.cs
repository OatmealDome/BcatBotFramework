using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nintendo.Bcat;
using Nintendo.Text;
using Syroot.BinaryData;

namespace Nintendo.SmashUltimate.Bcat
{
    public class PopUpNews : Container
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

        public bool IsPopUpForEvent
        {
            get;
            set;
        }

        public PopUpNews(Stream stream) : base(stream)
        {
            using (BinaryDataReader reader = new BinaryDataReader(stream, true))
            {
                // Read the title key
                reader.Seek(0x50, SeekOrigin.Begin);
                TitleKey = reader.ReadString(StringDataFormat.ZeroTerminated, Encoding.ASCII);

                // Read the content key
                reader.Seek(0x90, SeekOrigin.Begin);
                ContentKey = reader.ReadString(StringDataFormat.ZeroTerminated, Encoding.ASCII);

                // Split the title key by underscores
                string[] splitKey = TitleKey.Split("_");

                // Check if this is a pop-up for an event
                if (splitKey[2] == "event")
                {
                    // Combine the event ID and the pop-up ID
                    Id = splitKey[3] + "_" + splitKey[4];

                    // Set the event flag
                    IsPopUpForEvent = true;
                }
                else
                {
                    // The ID is simply the last entry
                    Id = splitKey.Last();

                    // Set the event flag
                    IsPopUpForEvent = false;
                }

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

                // Read the URL
                Url = this.ReadDataEntryAsString(reader);
            }
        }

    }
}