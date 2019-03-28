using Syroot.BinaryData;

namespace Nintendo.SmashUltimate.Bcat.EventData
{
    public class Rates
    {
        // guess based off event 1005
        public float Experience
        {
            get;
            set;
        }

        public float UnknownOne
        {
            get;
            set;
        }

        public float UnknownTwo
        {
            get;
            set;
        }

        // guess based off event 1005
        public float SpiritPoints
        {
            get;
            set;
        }

        public float UnknownThree
        {
            get;
            set;
        }
        
        public Rates(BinaryDataReader reader)
        {
            Experience = reader.ReadSingle();
            UnknownOne = reader.ReadSingle();
            UnknownTwo = reader.ReadSingle();
            SpiritPoints = reader.ReadSingle();
            UnknownThree = reader.ReadSingle();
        }
        
    }
}