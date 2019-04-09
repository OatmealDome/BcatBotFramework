using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BcatBotFramework.Difference;

namespace BcatBotFramework.Difference
{
    public class HandlerMapper
    {
        private static bool Initialized = false;

        // Dictionary
        private static Dictionary<int, Dictionary<DifferenceType, SortedList<int, MethodInfo>>> typeCatalog;

        public static void Initialize()
        {
            // Check initialization
            if (Initialized)
            {
                throw new Exception("Cannot initialize when already initialized");
            }

            // Initialize the outer Dictionary
            typeCatalog = new Dictionary<int, Dictionary<DifferenceType, SortedList<int, MethodInfo>>>();

            // Get ourselves
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Get all methods with the DifferenceHandler assembly
            IEnumerable<MethodInfo> methodInfos = assembly.GetTypes()
                .SelectMany(type => type.GetMethods())
                .Where(method => method.GetCustomAttributes(typeof(DifferenceHandlerAttribute), false).Length > 0);

            // Loop over every method
            foreach (MethodInfo methodInfo in methodInfos)
            {
                // Get the attributes for this method
                IEnumerable<Attribute> attributes = methodInfo.GetCustomAttributes(typeof(DifferenceHandlerAttribute));

                foreach (Attribute attribute in attributes)
                {
                    // Cast the attribute to DifferenceHandlerAttribute
                    DifferenceHandlerAttribute diffAttribute = (DifferenceHandlerAttribute)attribute;

                    // Check if we need to add a new Dictionary to the catalog
                    if (!typeCatalog.ContainsKey(diffAttribute.Type))
                    {
                        // Create the Dictionary
                        typeCatalog[diffAttribute.Type] = new Dictionary<DifferenceType, SortedList<int, MethodInfo>>();
                    }

                    // Check if we need to make a new SortedList
                    if (!typeCatalog[diffAttribute.Type].ContainsKey(diffAttribute.DifferenceType))
                    {
                        // Create the SortedList
                        typeCatalog[diffAttribute.Type][diffAttribute.DifferenceType] = new SortedList<int, MethodInfo>();
                    }

                    // Add this to the Dictionary
                    typeCatalog[diffAttribute.Type][diffAttribute.DifferenceType].Add(diffAttribute.Priority, methodInfo);
                }
                
            }

            // Set initialized flag
            Initialized = true;
        }

        public static void Dispose()
        {
            // Check initialization
            if (!Initialized)
            {
                throw new Exception("Cannot dispose when not initialized");
            }

            // Null out the Dictionary
            typeCatalog = null;
        }

        public static SortedList<int, MethodInfo> GetHandlers(int type, DifferenceType differenceType)
        {
            if (!typeCatalog.TryGetValue(type, out Dictionary<DifferenceType, SortedList<int, MethodInfo>> dict)
                || !dict.TryGetValue(differenceType, out SortedList<int, MethodInfo> sortedList))
            {
                return new SortedList<int, MethodInfo>();
            }
            
            return sortedList;
        }

    }
}