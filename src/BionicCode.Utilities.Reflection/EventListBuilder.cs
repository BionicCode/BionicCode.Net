namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

internal interface IEventListBuilder
{
    TypeData DeclaringType { get; }
    IEventListBuilder Add(EventData propertyData);
    EventList Build();
}

internal class EventListBuilder : SymbolDataListBuilder<EventData>, IEventListBuilder
{
    private EventList? _builderResult;
    private readonly TypeData _declaringType;

    private EventListBuilder(TypeData declaringType) : base(declaringType.Handle) => _declaringType = declaringType;

    public static IEventListBuilder New(TypeData declaringType)
    {
        ArgumentNullException.ThrowIfNull(declaringType);

        var builder = new EventListBuilder(declaringType);
        return builder;
    }

    public static EventList Create(IEnumerable<EventInfo>? items)
    {
        var eventInfoList = items?.ToList();
        if (eventInfoList is null || eventInfoList.IsEmpty())
        {
            return EventList.Empty;
        }

        var events = new List<EventData>(eventInfoList.Count);
        TypeData? declaringType = null;
        foreach (EventInfo eventInfo in eventInfoList)
        {
            EventData eventData = GetOrCreateCacheEntry(eventInfo);

            declaringType ??= eventData.DeclaringTypeData;

            if (!ReferenceEquals(eventData.DeclaringTypeData, declaringType))
            {
                throw new ArgumentException($"The argument '{nameof(items)}' contains invalid items. Reason: All '{nameof(EventInfo)}' items must belong to the same declaring type.");
            }

            events.Add(eventData);
        }

        if (declaringType is null)
        {
            throw new ArgumentException($"The argument '{nameof(items)}' contains invalid items. Reason: Unable to determine the declaring type of the provided '{nameof(EventInfo)}' items.");
        }

        return events.ToEventList(declaringType);
    }

    public static EventList Create(TypeData declaringTypeData)
    {
        ArgumentNullException.ThrowIfNull(declaringTypeData);
        return CreateInternal(declaringTypeData);
    }

    public static EventList Create(Type declaringType)
    {
        ArgumentNullException.ThrowIfNull(declaringType);
        TypeData declaringTypeData = GetOrCreateCacheEntry(declaringType);
        return CreateInternal(declaringTypeData);
    }

    private static EventList CreateInternal(TypeData declaringType) => declaringType.EnumerateEvents().ToEventList(declaringType);

    TypeData IEventListBuilder.DeclaringType => _declaringType;

    IEventListBuilder IEventListBuilder.Add(EventData eventData)
    {
        Add(eventData);
        return this;
    }

    EventList IEventListBuilder.Build()
        => _builderResult ??= new EventList(Build(), ((IEventListBuilder)this).DeclaringType, isIntegrityValidationEnabled: false);
}

internal static class EventListBuilderExtensions
{
    internal static EventList ToEventList(this IEnumerable<EventData> items, TypeData declaringType)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);

        return items is null || items.IsEmpty()
            ? EventList.Empty
            : new EventList(items, declaringType);
    }

    internal static IEventListView ToEventListView(this IEnumerable<EventData> items, TypeData declaringType)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);
        return items is null || items.IsEmpty()
            ? EventListView.Empty
            : new EventListView(items.Select(item => item.View), declaringType.View);
    }

    public static IEventListView ToEventListView(this IEnumerable<IEventDataView> items, ITypeDataView declaringType)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);

        return items is null || items.IsEmpty()
            ? EventListView.Empty
            : new EventListView(items.Select(item => item), declaringType);
    }

    /// <summary>
    /// Returns an empty <see cref="EventList"/> if the provided instance is <see langword="null"/>.
    /// </summary>
    /// <param name="items"></param>
    /// <returns>A <see cref="EventList"/> that is empty if the provided instance is <see langword="null"/>. Otherwise, returns the original instance.</returns>
    public static EventList OrEmpty(this EventList items) => items ?? EventList.Empty;
}
