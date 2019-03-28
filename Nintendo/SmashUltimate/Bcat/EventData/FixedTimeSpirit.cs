using System;
using Syroot.BinaryData;

namespace Nintendo.SmashUltimate.Bcat.EventData
{
    public class FixedTimeSpirit
    {
        public uint SpiritId
        {
            get;
            set;
        }

        public ushort UnknownOne
        {
            get;
            set;
        }

        public ushort UnknownTwo
        {
            get;
            set;
        }

        // in minutes
        public uint AppearanceDuration
        {
            get;
            set;
        }

        // in hours
        public uint AppearanceInterval
        {
            get;
            set;
        }

        public DateTime StartDateTime
        {
            get;
            set;
        }

        public FixedTimeSpirit(BinaryDataReader reader)
        {
            // Read the fields
            SpiritId = reader.ReadUInt32();
            UnknownOne = reader.ReadUInt16();
            UnknownTwo = reader.ReadUInt16();
            AppearanceDuration = reader.ReadUInt32();
            AppearanceInterval = reader.ReadUInt32();
            StartDateTime = Container.ReadDateTime(reader);
        }

    }
}