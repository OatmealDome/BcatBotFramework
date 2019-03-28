using Nintendo.SmashUltimate.Bcat;

namespace BcatBotFramework.Difference
{
    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public abstract class DifferenceHandlerAttribute : System.Attribute
    {
        public int Type
        {
            get;
            set;
        }

        public DifferenceType DifferenceType
        {
            get;
            set;
        }

        public int Priority
        {
            get;
            set;
        }
        
        public DifferenceHandlerAttribute(int type, DifferenceType differenceType, int priority)
        {
            // Set fields
            this.Type = type;
            this.DifferenceType = differenceType;
            this.Priority = priority;
        }

    }
}