namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

internal sealed class EventList : IReadOnlyList<EventData>, IEquatable<EventList>
{
    public static EventList Empty { get; } = new EventList();
    private readonly int _hashCode; // precomputed
    private readonly TypeData _declaringType;
    private readonly Dictionary<string, EventData> _eventNameIndex;

    public EventList(EventData[] items, TypeData? declaringType) : this((IEnumerable<EventData>)items, declaringType)
    {
    }

    public EventList(IEnumerable<EventData> items, TypeData? declaringType)
    {
        ArgumentNullException.ThrowIfNull(declaringType);

        _declaringType = declaringType;
        Events = items?.ToImmutableList() ?? ImmutableList<EventData>.Empty;
        _eventNameIndex = Events.ToDictionary(eventData => eventData.Name, StringComparer.Ordinal);

        if (HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfAny(
                Events,
                eventData => !ReferenceEquals(eventData.DeclaringTypeData, _declaringType),
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(EventData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All events must belong to the same declaring type.");

        }

        _hashCode = ComputeHashCode();
    }

    internal EventList(IEnumerable<EventData> items, TypeData? declaringType, bool isIntegrityValidationEnabled)
    {
        ArgumentNullException.ThrowIfNull(declaringType);

        _declaringType = declaringType;

        Events = items?.ToImmutableList() ?? ImmutableList<EventData>.Empty;
        _eventNameIndex = Events.ToDictionary(eventData => eventData.Name, StringComparer.Ordinal);

        if (isIntegrityValidationEnabled && HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfAny(
                Events,
                eventData => !ReferenceEquals(eventData.DeclaringTypeData, _declaringType),
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(EventData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All events must belong to the same declaring type.");

        }

        _hashCode = ComputeHashCode();
    }

    private EventList()
    {
        _declaringType = default!;
        Events = ImmutableList<EventData>.Empty;
        _eventNameIndex = [];
        _hashCode = ComputeHashCode();
    }

    public bool TryGetEventByName(string eventName, out EventData? eventData)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(eventName);
        return _eventNameIndex.TryGetValue(eventName, out eventData);
    }

    public bool ContainsEventWithName(string eventName)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(eventName);
        return _eventNameIndex.ContainsKey(eventName);
    }

    public int Count => Events.Count;
    public bool IsEmpty => Events.IsEmpty;
    public bool HasItems => !IsEmpty;
    public ImmutableList<EventData> Events { get; }
    public TypeData DeclaringTypeData => _declaringType ?? throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(EventList), nameof(DeclaringTypeData)));

    public EventData this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Events.Count, nameof(index));

            return HasItems
                ? Events[index]
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(EventList), ReflectionConstants.IndexerGetMethodName));
        }
    }

    public IEnumerator<EventData> GetEnumerator()
        => ((IEnumerable<EventData>)Events).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => Events.GetEnumerator();

    public bool Equals(EventList? other)
    {
        if (other is null)
        {
            return false;
        }

        if (Count != other.Count)
        {
            return false;
        }

        if (!ReferenceEquals(DeclaringTypeData, other.DeclaringTypeData))
        {
            return false;
        }

        for (int index = 0; index < Count; index++)
        {
            if (!ReferenceEquals(Events[index], other.Events[index]))
            {
                return false;
            }
        }

        return true;
    }

    public bool Equals(IEventListView? other)
    {
        if (other is null)
        {
            return false;
        }

        if (Count != other.Count)
        {
            return false;
        }

        if (!ReferenceEquals(DeclaringTypeData.View, other.DeclaringType))
        {
            return false;
        }

        for (int index = 0; index < Count; index++)
        {
            if (!Events[index].View.Equals(other.Events[index]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj)
        => obj is EventList other && Equals(other);

    public override int GetHashCode() => _hashCode;

    private int ComputeHashCode()
    {
        unchecked
        {
            var hashCode = new HashCode();
            hashCode.Add(Count);
            hashCode.Add(DeclaringTypeData);
            for (int index = 0; index < Events.Count; index++)
            {
                hashCode.Add(Events[index]);
            }

            return hashCode.ToHashCode();
        }
    }

    public static bool operator ==(EventList? left, EventList? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(EventList? left, EventList? right) => !(left == right);

    public static bool operator ==(EventList? left, IEventListView? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(EventList? left, IEventListView? right) => !(left == right);

    public static bool operator ==(IEventListView? left, EventList? right) => right == left;
    public static bool operator !=(IEventListView? left, EventList? right) => !(left == right);
}

public sealed class EventListView : IReadOnlyList<IEventDataView>, IEquatable<IEventListView>, IEventListView
{
    public static IEventListView Empty { get; } = new EventListView();
    private readonly int _hashCode; // precomputed
    private readonly ITypeDataView _declaringType;
    private readonly Dictionary<string, IEventDataView> _eventNameIndex;

    public EventListView(IEventDataView[] items, ITypeDataView? declaringType) : this((IEnumerable<IEventDataView>)items, declaringType)
    {
    }

    public EventListView(IEnumerable<IEventDataView> items, ITypeDataView? declaringType)
    {
        ArgumentNullException.ThrowIfNull(declaringType);

        _declaringType = declaringType;
        Events = items?.ToImmutableList() ?? ImmutableList<IEventDataView>.Empty;
        _eventNameIndex = Events.ToDictionary(eventData => eventData.Name, StringComparer.Ordinal);

        if (HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfAny(
                Events,
                eventData => !ReferenceEquals(eventData.DeclaringType, _declaringType),
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(IEventDataView)}.{nameof(IEventDataView.DeclaringType)}' declaring type handle. All events must belong to the same declaring type.");

        }

        _hashCode = ComputeHashCode();
    }

    internal EventListView(IEnumerable<IEventDataView> items, ITypeDataView? declaringType, bool isIntegrityValidationEnabled)
    {
        ArgumentNullException.ThrowIfNull(declaringType);

        _declaringType = declaringType;

        Events = items?.ToImmutableList() ?? ImmutableList<IEventDataView>.Empty;
        _eventNameIndex = Events.ToDictionary(eventData => eventData.Name, StringComparer.Ordinal);

        if (isIntegrityValidationEnabled && HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfAny(
                Events,
                eventData => !ReferenceEquals(eventData.DeclaringType, _declaringType),
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(IEventDataView)}.{nameof(IEventDataView.DeclaringType)}' declaring type handle. All events must belong to the same declaring type.");

        }

        _hashCode = ComputeHashCode();
    }

    private EventListView()
    {
        _declaringType = default!;
        Events = ImmutableList<IEventDataView>.Empty;
        _eventNameIndex = [];
        _hashCode = ComputeHashCode();
    }

    public bool TryGetEventByName(string eventName, out IEventDataView? eventData)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(eventName);
        return _eventNameIndex.TryGetValue(eventName, out eventData);
    }

    public bool ContainsEventWithName(string eventName)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(eventName);
        return _eventNameIndex.ContainsKey(eventName);
    }

    public int Count => Events.Count;
    public bool IsEmpty => Events.IsEmpty;
    public bool HasItems => !IsEmpty;
    public ImmutableList<IEventDataView> Events { get; }
    public ITypeDataView DeclaringType => _declaringType ?? throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(EventListView), nameof(DeclaringType)));

    public IEventDataView this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Events.Count, nameof(index));

            return HasItems
                ? Events[index]
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(EventListView), ReflectionConstants.IndexerGetMethodName));
        }
    }

    public IEnumerator<IEventDataView> GetEnumerator()
        => ((IEnumerable<IEventDataView>)Events).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => Events.GetEnumerator();

    public bool Equals(IEventListView? other)
    {
        if (other is null)
        {
            return false;
        }

        if (Count != other.Count)
        {
            return false;
        }

        if (!ReferenceEquals(DeclaringType, other.DeclaringType))
        {
            return false;
        }

        for (int index = 0; index < Count; index++)
        {
            if (!Events[index].Equals(other.Events[index]))
            {
                return false;
            }
        }

        return true;
    }

    internal bool Equals(EventList? other)
    {
        if (other is null)
        {
            return false;
        }

        if (Count != other.Count)
        {
            return false;
        }

        if (!ReferenceEquals(DeclaringType, other.DeclaringTypeData.View))
        {
            return false;
        }

        for (int index = 0; index < Count; index++)
        {
            if (!Events[index].Equals(other.Events[index].View))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj)
        => obj is IEventListView other && Equals(other);

    public override int GetHashCode() => _hashCode;

    private int ComputeHashCode()
    {
        unchecked
        {
            var hashCode = new HashCode();
            hashCode.Add(Count);
            hashCode.Add(DeclaringType);
            for (int index = 0; index < Events.Count; index++)
            {
                hashCode.Add(Events[index]);
            }

            return hashCode.ToHashCode();
        }
    }

    public static bool operator ==(EventListView? left, IEventListView? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(EventListView? left, IEventListView? right) => !(left == right);

    public static bool operator ==(IEventListView? left, EventListView? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(IEventListView? left, EventListView? right) => !(left == right);
}
