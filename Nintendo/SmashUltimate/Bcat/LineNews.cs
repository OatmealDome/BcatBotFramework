using System.Collections.Generic;
using System.IO;
using Nintendo.Bcat;
using Nintendo.SmashUltimate.Bcat.LineNewsData;
using Syroot.BinaryData;

namespace Nintendo.SmashUltimate.Bcat
{
    public class LineNews : Container
    {
        public List<OneLine> OneLines
        {
            get;
            set;
        } = new List<OneLine>();
        
        public LineNews(Stream stream) : base(stream)
        {
            using (BinaryDataReader reader = new BinaryDataReader(stream, true))
            {
                // Seek to the beginning of the data list
                reader.Seek(0x40, SeekOrigin.Begin);

                // Read the OneLine count and start offset
                uint oneLineCount = reader.ReadUInt32();
                uint oneLineOffset = reader.ReadUInt32();

                // Seek to the OneLines
                using (reader.TemporarySeek(oneLineOffset, SeekOrigin.Begin))
                {
                    // Read the OneLines
                    for (int i = 0; i < oneLineCount; i++)
                    {
                        OneLines.Add(new OneLine(reader));
                    }
                }

                // Read the MSBTs
                Dictionary<Language, Dictionary<string, string>> msbts = this.ReadMsbts(reader);

                // Loop over every MSBT combination
                foreach (KeyValuePair<Language, Dictionary<string, string>> pair in msbts)
                {
                    // Loop over every OneLine
                    foreach (OneLine oneLine in OneLines)
                    {
                        // Get the text
                        string text = pair.Value[oneLine.Key];
                        
                        // Temporary hack
                        text = text.Replace("","✉️"). Replace("\u000e\u0001\u0005\u0002", "ZR").Replace('\r', ' ');

                        // Add the text
                        oneLine.Text.Add(pair.Key, text);
                    }
                }

                // Parse the ID
                Id = OneLines[0].Key.Split('_')[1];
            }        
        }

    }
}