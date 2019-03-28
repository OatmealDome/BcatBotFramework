using System;

namespace BcatBotFramework.Internationalization
{
    [System.Serializable]
    public class LocalizedException : Exception
    {
        public LocalizedException() { }
        public LocalizedException(string message) : base(message) { }
        public LocalizedException(string message, System.Exception inner) : base(message, inner) { }
        protected LocalizedException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    
}