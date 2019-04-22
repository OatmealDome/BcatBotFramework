using System;
using System.Linq;
using System.Reflection;

namespace BcatBotFramework.Core
{
    public static class TypeUtils
    {
        public static Type GetSubclassOfType<T>()
        {
            return Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(T))).FirstOrDefault();
        }
        
    }
}