namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

internal sealed class EventList : IReadOnlyList<EventData>, IEquatable<EventList>
{
    private static EventList Empty { get; } = new EventList();
    private readonly int _hashCode; // precomputed
    private readonly SymbolReflectionInfoCacheKey _declaringTypeCacheKey;
    public readonly Dictionary<string, EventData> _eventNameIndex;

    public EventList(EventData[] items) : this((IEnumerable<EventData>)items)
    {
    }

    public EventList(IEnumerable<EventData> items)
    {
        Events = items?.ToImmutableList() ?? ImmutableList<EventData>.Empty;
        _eventNameIndex = Events.ToDictionary(eventData => eventData.Name, StringComparer.Ordinal);

        if (HasItems)
        {
            _declaringTypeCacheKey = Events.First().DeclaringTypeData.CacheKey;

            ArgumentExceptionAdvanced.ThrowIfAny(
                Events,
                eventData => eventData.DeclaringTypeData.CacheKey != _declaringTypeCacheKey,
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(EventData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All events must belong to the same declaring type.");

        }

        _hashCode = ComputeHashCode();
    }

    internal EventList(IEnumerable<EventData> items, bool isIntegrityValidationEnabled)
    {
        Events = items?.ToImmutableList() ?? ImmutableList<EventData>.Empty;
        _eventNameIndex = Events.ToDictionary(eventData => eventData.Name, StringComparer.Ordinal);

        if (isIntegrityValidationEnabled && HasItems)
        {
            _declaringTypeCacheKey = Events.First().DeclaringTypeData.CacheKey;

            ArgumentExceptionAdvanced.ThrowIfAny(
                Events,
                eventData => eventData.DeclaringTypeData.CacheKey != _declaringTypeCacheKey,
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(EventData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All events must belong to the same declaring type.");

        }

        _hashCode = ComputeHashCode();
    }

    private EventList()
    {
        Events = ImmutableList<EventData>.Empty;
        _eventNameIndex = new Dictionary<string, EventData>(0, StringComparer.Ordinal);
    }

    public bool TryGetEventByName(string eventName, out EventData? eventData)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(eventName);
        return _eventNameIndex.TryGetValue(eventName, out eventData);
    }

    public int Count => Events.Count;
    public bool IsEmpty => Events.IsEmpty;
    public bool HasItems => !IsEmpty;
    public ImmutableList<EventData> Events { get; }
    public SymbolReflectionInfoCacheKey DeclaringTypeCacheKey
        => HasItems
            ? _declaringTypeCacheKey
            : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(MethodList), nameof(DeclaringTypeCacheKey)));

    public TypeData DeclaringTypeData
    {
        get
        {
            SymbolReflectionInfoCacheKey cacheKey = DeclaringTypeCacheKey;
            return HasItems
                ? SymbolReflectionInfoCache.GetOrCreateTypeDataCacheEntry(ref cacheKey)
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(MethodList), nameof(DeclaringTypeData)));
        }
    }

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

        if (!DeclaringTypeCacheKey.Equals(other.DeclaringTypeCacheKey))
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

    public override bool Equals(object? obj)
        => obj is EventList other && Equals(other);

    public override int GetHashCode() => _hashCode;

    private int ComputeHashCode()
    {
        unchecked
        {
            var hashCode = new HashCode();
            hashCode.Add(Count);
            hashCode.Add(DeclaringTypeCacheKey);
            for (int index = 0; index < Events.Count; index++)
            {
                hashCode.Add(Events[index]);
            }

            return hashCode.ToHashCode();
        }
    }

    public static bool operator ==(EventList? left, EventList? right)
        => left?.Equals(right) ?? (right is null);
    public static bool operator !=(EventList? left, EventList? right)
        => !(left == right);
}

internal sealed class ExplicitMemberImplementationList<TMemberData> : IReadOnlyList<TMemberData>, IEquatable<ExplicitMemberImplementationList<TMemberData>>
    where TMemberData : MemberData
{
    public static readonly ExplicitMemberImplementationList<TMemberData> Empty = new ExplicitMemberImplementationList<TMemberData>();
    private readonly int _hashCode; // precomputed
    private readonly Dictionary<(RuntimeTypeHandle, string), TMemberData> _memberIndex;

    public ExplicitMemberImplementationList(EventData[] items) : this((IEnumerable<EventData>)items)
    {
    }

    public ExplicitMemberImplementationList(IEnumerable<TMemberData> items)
    {
        Events = items?.ToImmutableList() ?? ImmutableList<TMemberData>.Empty;
        _memberIndex = Events.ToDictionary(eventData => eventData.Name, StringComparer.Ordinal);

        if (HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfAny(
                Events,
                eventData => !eventData.IsExplicitInterfaceImplementation,
                nameof(items),
                $"Invalid argument '{nameof(items)}'. All events must be explicit interface implementations.");

        }

        _hashCode = ComputeHashCode();
    }

    internal ExplicitMemberImplementationList(IEnumerable<TMemberData> items, bool isIntegrityValidationEnabled)
    {
        Events = items?.ToImmutableList() ?? ImmutableList<TMemberData>.Empty;
        _memberIndex = Events.ToDictionary(eventData => eventData.Name, StringComparer.Ordinal);

        if (isIntegrityValidationEnabled && HasItems)
        {
            _declaringTypeCacheKey = Events.First().DeclaringTypeData.CacheKey;

            ArgumentExceptionAdvanced.ThrowIfAny(
                Events,
                eventData => eventData.DeclaringTypeData.CacheKey != _declaringTypeCacheKey,
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(EventData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All events must belong to the same declaring type.");

        }

        _hashCode = ComputeHashCode();
    }

    private ExplicitMemberImplementationList()
    {
        Events = ImmutableList<EventData>.Empty;
        _memberIndex = new Dictionary<string, EventData>(0, StringComparer.Ordinal);
    }

    public bool TryGetEventByName(string eventName, out EventData? eventData)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(eventName);
        return _memberIndex.TryGetValue(eventName, out eventData);
    }

    public int Count => Events.Count;
    public bool IsEmpty => Events.IsEmpty;
    public bool HasItems => !IsEmpty;
    public ImmutableList<TMemberData> Events { get; }
    public SymbolReflectionInfoCacheKey DeclaringTypeCacheKey
        => HasItems
            ? _declaringTypeCacheKey
            : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(MethodList), nameof(DeclaringTypeCacheKey)));

    public TypeData DeclaringTypeData
    {
        get
        {
            SymbolReflectionInfoCacheKey cacheKey = DeclaringTypeCacheKey;
            return HasItems
                ? SymbolReflectionInfoCache.GetOrCreateTypeDataCacheEntry(ref cacheKey)
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(MethodList), nameof(DeclaringTypeData)));
        }
    }

    TMemberData IReadOnlyList<TMemberData>.this[int index] { get; }

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

        if (!DeclaringTypeCacheKey.Equals(other.DeclaringTypeCacheKey))
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

    public override bool Equals(object? obj)
        => obj is EventList other && Equals(other);

    public override int GetHashCode() => _hashCode;

    private int ComputeHashCode()
    {
        unchecked
        {
            var hashCode = new HashCode();
            hashCode.Add(Count);
            hashCode.Add(DeclaringTypeCacheKey);
            for (int index = 0; index < Events.Count; index++)
            {
                hashCode.Add(Events[index]);
            }

            return hashCode.ToHashCode();
        }
    }

    IEnumerator<TMemberData> IEnumerable<TMemberData>.GetEnumerator() => throw new NotImplementedException();
    public bool Equals(ExplicitMemberImplementationList<TMemberData>? other) => throw new NotImplementedException();

    public static bool operator ==(EventList? left, EventList? right)
        => left?.Equals(right) ?? (right is null);
    public static bool operator !=(EventList? left, EventList? right)
        => !(left == right);
}
