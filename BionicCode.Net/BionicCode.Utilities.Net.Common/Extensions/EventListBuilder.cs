namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class EventListBuilder
    {
        internal static EventList Create(IEnumerable<EventInfo>? items)
        {
            List<EventInfo>? eventInfoList = items?.ToList();
            if (eventInfoList is null || eventInfoList.IsEmpty())
            {
                return EventList.Empty;
            }

            List<EventData> events = new List<EventData>(eventInfoList.Count);
            RuntimeTypeHandle declaringTypeHandle = default;
            foreach (EventInfo eventInfo in eventInfoList)
            {
                EventData eventData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventInfo);

                if (declaringTypeHandle.Equals(default))
                {
                    declaringTypeHandle = eventData.DeclaringTypeHandle;
                }

                if (!eventData.DeclaringTypeHandle.Equals(declaringTypeHandle))
                {
                    throw new ArgumentException($"All '{nameof(EventInfo)}' items must belong to the same declaring type.");
                }

                events.Add(eventData);
            }

            return events.ToEventList();
        }

        internal static EventList Create(TypeData declaringTypeData)
        {
            ArgumentNullException.ThrowIfNull(declaringTypeData);
            return CreateInternal(declaringTypeData.UnwrapType());
        }

        internal static EventList Create(Type declaringType)
        {
            ArgumentNullException.ThrowIfNull(declaringType);
            return CreateInternal(declaringType);
        }

        private static EventList CreateInternal(Type declaringType)
        {
            EventInfo[] eventInfoList = declaringType.GetEvents(HelperExtensionsCommon.AllMembersFullHierarchyFlags);
            if (eventInfoList.IsEmpty())
            {
                return EventList.Empty;
            }

            IEnumerable<EventData> events = eventInfoList.Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry);

            return events.ToEventList();
        }

        internal static EventList ToEventList(this IEnumerable<EventData> items)
            => items is null || items.IsEmpty() ? EventList.Empty : new EventList(items);
    }
}
