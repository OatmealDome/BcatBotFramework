using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BcatBotFramework.Json
{
    public class ShouldSerializeContractResolver : DefaultContractResolver
    {
        public static ShouldSerializeContractResolver Instance { get; } = new ShouldSerializeContractResolver();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);        
            if (member.GetCustomAttribute<NoSerializeAttribute>() == null)
            {
                property.Ignored = true;
            }

            return property;
        }

    }
}