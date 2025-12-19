namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class PropertyListBuilder
    {
        internal static PropertyList Create(IEnumerable<PropertyInfo>? items)
        {
            List<PropertyInfo>? parameterInfoList = items?.ToList();
            if (parameterInfoList is null || parameterInfoList.IsEmpty())
            {
                return PropertyList.Empty;
            }

            List<PropertyData> parameters = new List<PropertyData>(parameterInfoList.Count);
            RuntimeTypeHandle declaringTypeHandle = default;
            foreach (PropertyInfo parameterInfo in parameterInfoList)
            {
                PropertyData parameterData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(parameterInfo);

                if (declaringTypeHandle.Equals(default))
                {
                    declaringTypeHandle = parameterData.DeclaringTypeHandle;
                }

                if (!parameterData.DeclaringTypeHandle.Equals(declaringTypeHandle))
                {
                    throw new ArgumentException("All PropertyInfo items must belong to the same member.");
                }

                parameters.Add(parameterData);
            }

            return new PropertyList(parameters);
        }
    }

    internal static class EventListBuilder
    {
        internal static EventList Create(IEnumerable<EventInfo>? items)
        {
            List<EventInfo>? parameterInfoList = items?.ToList();
            if (parameterInfoList is null || parameterInfoList.IsEmpty())
            {
                return PropertyList.Empty;
            }

            List<EventData> parameters = new List<EventData>(parameterInfoList.Count);
            RuntimeTypeHandle declaringTypeHandle = default;
            foreach (EventInfo eventInfo in parameterInfoList)
            {
                EventData eventData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventInfo);

                if (declaringTypeHandle.Equals(default))
                {
                    declaringTypeHandle = eventData.DeclaringTypeHandle;
                }

                if (!eventData.DeclaringTypeHandle.Equals(declaringTypeHandle))
                {
                    throw new ArgumentException("All PropertyInfo items must belong to the same member.");
                }

                parameters.Add(eventData);
            }

            return new EventList(parameters);
        }
    }
}
