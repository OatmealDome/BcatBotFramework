using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Nintendo.Bcat;
using BcatBotFramework.Core.Config;
using Nintendo.Text;
using Syroot.BinaryData;

namespace Nintendo.SmashUltimate.Bcat
{
    public abstract class Container
    {
        public uint GenericUnknown1
        {
            get;
            set;
        }

        public uint GenericUnknown2
        {
            get;
            set;
        }

        public uint GenericUnknown3
        {
            get;
            set;
        }

        public DateTime StartDateTime
        {
            get;
            set;
        }

        public DateTime EndDateTime
        {
            get;
            set;
        }

        public string Id
        {
            get;
            set;
        }

        public string Url
        {
            get;
            set;
        } = null;

        public static Language[] LanguageOrder = new Language[]
        {
            Language.Japanese,
            Language.EnglishUS,
            Language.FrenchCA,
            Language.SpanishLA,
            Language.EnglishUK,
            Language.FrenchFR,
            Language.SpanishES,
            Language.German,
            Language.Dutch,
            Language.Italian,
            Language.Russian,
            Language.Chinese,
            Language.ChineseTaiwan,
            Language.Korean
        };

        public Container(Stream stream)
        {
            using (BinaryDataReader reader = new BinaryDataReader(stream, true))
            {
                // Set endianness
                reader.ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;

                // Seek to the beginning
                reader.Seek(0, SeekOrigin.Begin);

                // Read length
                uint fileLength = reader.ReadUInt32();

                // Verify file length
                if (fileLength != reader.Length)
                {
                    throw new Exception("Invalid file length");
                }

                // Read unknowns
                uint unk1 = reader.ReadUInt32(); // all zeroes, or could be high bytes of fileLength
                uint unk2 = reader.ReadUInt32();
                uint unk3 = reader.ReadUInt32();

                // Seek to DateTimes
                reader.Seek(0x20, SeekOrigin.Begin);

                // Read DateTimes
                StartDateTime = ReadDateTime(reader);
                EndDateTime = ReadDateTime(reader);
            }
        }
        
        public static DateTime ReadDateTime(BinaryDataReader reader) // TODO: split into util class?
        {
            // Read values
            ushort year = reader.ReadUInt16();
            byte month = reader.ReadByte();
            byte day = reader.ReadByte();
            byte hour = reader.ReadByte();
            byte minute = reader.ReadByte();
            
            // Seek to next DateTime
            reader.Seek(2, SeekOrigin.Current);

            // Return the DateTime
            return new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Utc);
        }

        protected byte[] ReadDataEntry(BinaryDataReader reader)
        {
            // Read values
            uint offset = reader.ReadUInt32();
            uint size = reader.ReadUInt32();

            // Seek to the raw offset
            using (reader.TemporarySeek(offset, SeekOrigin.Begin))
            {
                // Read the bytes
                return reader.ReadBytes((int)size);
            }
        }

        protected string ReadDataEntryAsString(BinaryDataReader reader)
        {
            return Encoding.UTF8.GetString(ReadDataEntry(reader)).Trim('\0');
        }

        protected Dictionary<Language, Dictionary<string, string>> ReadMsbts(BinaryDataReader reader)
        {
            Dictionary<Language, Dictionary<string, string>> msbts = new Dictionary<Language, Dictionary<string, string>>();

            // Read the MSBTs
            foreach (Language language in Container.LanguageOrder)
            {
                // Read the MSBT
                MSBT msbt = new MSBT(this.ReadDataEntry(reader));

                // Create a Dictionary for this MSBT
                Dictionary<string, string> textMapping = new Dictionary<string, string>();

                // Loop over every string
                for (int i = 0; i < msbt.TXT2.NumberOfStrings; i++)
                {
                    // Get the IEntry
                    IEntry entry = msbt.HasLabels ? msbt.LBL1.Labels[i] : msbt.TXT2.Strings[i];

                    // Parse the value and trim the zero-byte
                    string str = msbt.FileEncoding.GetString(entry.Value).Trim('\0');

                    // Add this label and text to the mapping
                    textMapping.Add(entry.ToString(), str);
                }

                // Add this mapping to the Dictionary
                msbts.Add(language, textMapping);
            }

            return msbts;
        }

        public string GetFormattedUrl()
        {
            // Check the existence of the URL field
            if (Url == null)
            {
                throw new Exception("No URL in this Container");
            }

            // Format the URL with the environment
            return Url.Replace("%", Configuration.LoadedConfiguration.CdnConfig.Environment);
        }
        
        public string GetFormattedUrl(Language language)
        {
            // Format the URL with the language
            return GetFormattedUrl().Replace("{$lang}", language.GetCode());
        }

    }
}