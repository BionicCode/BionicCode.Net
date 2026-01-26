namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal interface IEventListBuilder
    {
        IEventListBuilder Add(EventData propertyData);
        EventList Build();
    }

    internal class EventListBuilder : SymbolDataListBuilder<EventData>, IEventListBuilder
    {
        private EventList? _builderResult;

        private EventListBuilder(RuntimeTypeHandle declaringTypeHandle) : base(declaringTypeHandle)
        {
        }

        public static IEventListBuilder New(RuntimeTypeHandle declaringTypeHandle)
        {
            var builder = new EventListBuilder(declaringTypeHandle);
            return builder;
        }

        public static EventList Create(IEnumerable<EventInfo>? items)
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
                    throw new ArgumentException($"The argument '{nameof(items)}' contains invalid items. Reason: All '{nameof(EventInfo)}' items must belong to the same declaring type.");
                }

                events.Add(eventData);
            }

            return events.ToEventList();
        }

        public static EventList Create(TypeData declaringTypeData)
        {
            ArgumentNullException.ThrowIfNull(declaringTypeData);
            return CreateInternal(declaringTypeData.UnwrapType());
        }

        public static EventList Create(Type declaringType)
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

        IEventListBuilder IEventListBuilder.Add(EventData eventData)
        {
            Add(eventData);
            return this;
        }

        EventList IEventListBuilder.Build()
            => this._builderResult ??= new EventList(Build(), isIntegrityValidationEnabled: false);
    }

    internal static class EventListBuilderExtensions
    {
        public static EventList ToEventList(this IEnumerable<EventData> items)
            => items is null || items.IsEmpty() ? EventList.Empty : new EventList(items);
    }
}
