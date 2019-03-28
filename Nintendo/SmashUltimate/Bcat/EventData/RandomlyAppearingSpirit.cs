using Syroot.BinaryData;

namespace Nintendo.SmashUltimate.Bcat.EventData
{
    public class RandomlyAppearingSpirit
    {
        public uint SpiritId
        {
            get;
            set;
        }

        public float Weight
        {
            get;
            set;
        }

        public RandomlyAppearingSpirit(BinaryDataReader reader)
        {
            // Read the fields
            SpiritId = reader.ReadUInt32();
            Weight = reader.ReadSingle();
        }
        
    }
}