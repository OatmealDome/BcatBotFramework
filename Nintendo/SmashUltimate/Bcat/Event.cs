using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nintendo.Bcat;
using Nintendo.SmashUltimate.Bcat.EventData;
using Syroot.BinaryData;

namespace Nintendo.SmashUltimate.Bcat
{
    public class Event : Container
    {
        public string TitleKey
        {
            get;
            set;
        }

        public DateTime StartDateTimeDuplicate
        {
            get;
            set;
        }

        public byte[] UnknownDataOne
        {
            get;
            set;
        }

        public Dictionary<Language, string> TitleText
        {
            get;
            set;
        } = new Dictionary<Language, string>();

        public byte[] Image
        {
            get;
            set;
        }

        public List<FixedTimeSpirit> FixedTimeSpirits
        {
            get;
            set;
        } = new List<FixedTimeSpirit>();

        public byte[] UnknownDataTwo
        {
            get;
            set;
        }

        public List<RandomlyAppearingSpirit> RandomSpiritsOne
        {
            get;
            set;
        } = new List<RandomlyAppearingSpirit>();

        public uint RandomSpiritsOneUnknown;

        public List<RandomlyAppearingSpirit> RandomSpiritsTwo
        {
            get;
            set;
        } = new List<RandomlyAppearingSpirit>();

        public uint RandomSpiritsTwoUnknown;

        public List<RandomlyAppearingSpirit> RandomSpiritsThree
        {
            get;
            set;
        } = new List<RandomlyAppearingSpirit>();

        public uint RandomSpiritsThreeUnknown;

        public Rates Rates
        {
            get;
            set;
        }
        
        public Event(Stream stream) : base(stream)
        {
            using (BinaryDataReader reader = new BinaryDataReader(stream, true))
            {
                // Read the title key
                reader.Seek(0x40, SeekOrigin.Begin);
                TitleKey = reader.ReadString(StringDataFormat.ZeroTerminated, Encoding.ASCII);

                // Parse the ID from the title key
                Id = TitleKey.Split("_").Last();

                // Read the third DateTime
                reader.Seek(0x80, SeekOrigin.Begin);
                StartDateTimeDuplicate = ReadDateTime(reader);

                // Read the unknown data
                UnknownDataOne = reader.ReadBytes(0x44);

                // Read the MSBTs
                Dictionary<Language, Dictionary<string, string>> msbts = this.ReadMsbts(reader);

                // Loop over every MSBT combination
                foreach (KeyValuePair<Language, Dictionary<string, string>> pair in msbts)
                {
                    // Add the title text
                    TitleText.Add(pair.Key, pair.Value[TitleKey]);
                }

                // Read the image
                Image = this.ReadDataEntry(reader);

                // Read the URL
                Url = this.ReadDataEntryAsString(reader);

                // Read the fixed time spirits
                ReadFixedTimeSpirits(reader, FixedTimeSpirits);

                // Read the unknown data offset
                uint unknownDataTwoOffset = reader.ReadUInt32();

                // Read more unknown data
                using (reader.TemporarySeek(unknownDataTwoOffset, SeekOrigin.Begin))
                {
                    UnknownDataTwo = reader.ReadBytes(0x10);
                }

                // Read randomly appearing spirits
                ReadRandomlyAppearingSpirits(reader, RandomSpiritsOne, ref RandomSpiritsOneUnknown);
                ReadRandomlyAppearingSpirits(reader, RandomSpiritsTwo, ref RandomSpiritsTwoUnknown);
                ReadRandomlyAppearingSpirits(reader, RandomSpiritsThree, ref RandomSpiritsThreeUnknown);

                // Read the rates offset
                uint ratesOffset = reader.ReadUInt32();

                // Seek to the rates
                using (reader.TemporarySeek(ratesOffset, SeekOrigin.Begin))
                {
                    // Read the rates
                    Rates = new Rates(reader);
                }
            }
        }

        private void ReadFixedTimeSpirits(BinaryDataReader reader, List<FixedTimeSpirit> spiritList)
        {
            // Read the offset
            uint sectionOffset = reader.ReadUInt32();

            // Check if the offset is invalid
            if (sectionOffset == 0)
            {
                // Do nothing
                return;
            }

            // Seek to the offset
            using (reader.TemporarySeek(sectionOffset, SeekOrigin.Begin))
            {
                // Read the spirit count
                uint spiritCount = reader.ReadUInt32();

                // Skip over padding(?)
                uint padding = reader.ReadUInt32();
                if (padding != 0)
                {
                    throw new Exception("Padding not zero in FixedTimeSpirit section");
                }

                // Read all the spirits
                for (uint i = 0; i < spiritCount; i++)
                {
                    // Read and add the spirit to the List
                    spiritList.Add(new FixedTimeSpirit(reader));
                }
            }
        }

        private void ReadRandomlyAppearingSpirits(BinaryDataReader reader, List<RandomlyAppearingSpirit> spiritList, ref uint flag)
        {
            // Read the offset
            uint sectionOffset = reader.ReadUInt32();

            // Check if the offset is invalid
            if (sectionOffset == 0)
            {
                // Set the flag to nothing
                flag = 0;

                return;
            }

            // Seek to the offset
            using (reader.TemporarySeek(sectionOffset, SeekOrigin.Begin))
            {
                // Read the spirit count
                uint spiritCount = reader.ReadUInt32();

                // Read the flag
                flag = reader.ReadUInt32();

                // Read all the spirits
                for (uint i = 0; i < spiritCount; i++)
                {
                    // Read and add the spirit to the List
                    spiritList.Add(new RandomlyAppearingSpirit(reader));
                }
            }
        }

    }
}