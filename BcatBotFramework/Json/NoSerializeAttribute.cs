namespace BcatBotFramework.Json
{
    [System.AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    sealed class NoSerializeAttribute : System.Attribute
    {
        public NoSerializeAttribute()
        {
        }

    }
}