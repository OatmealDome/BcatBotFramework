using System;

namespace BcatBotFramework.Difference
{
    public class DifferenceTypeExtensions
    {
        public static DifferenceType[] GetAllDifferenceTypes()
        {
            return (DifferenceType[])Enum.GetValues(typeof(DifferenceType));
        }

    }
}