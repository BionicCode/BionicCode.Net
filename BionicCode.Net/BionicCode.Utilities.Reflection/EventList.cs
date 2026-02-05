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
        this.Events = items?.ToImmutableList() ?? ImmutableList<EventData>.Empty;
        this._eventNameIndex = this.Events.ToDictionary(eventData => eventData.Name, StringComparer.Ordinal);

        if (this.HasItems)
        {
            this._declaringTypeCacheKey = this.Events.First().DeclaringTypeData.CacheKey;

            ArgumentExceptionAdvanced.ThrowIfAny(
                this.Events,
                eventData => eventData.DeclaringTypeData.CacheKey != this._declaringTypeCacheKey,
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(EventData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All events must belong to the same declaring type.");

        }

        this._hashCode = ComputeHashCode();
    }

    internal EventList(IEnumerable<EventData> items, bool isIntegrityValidationEnabled)
    {
        this.Events = items?.ToImmutableList() ?? ImmutableList<EventData>.Empty;
        this._eventNameIndex = this.Events.ToDictionary(eventData => eventData.Name, StringComparer.Ordinal);

        if (isIntegrityValidationEnabled && this.HasItems)
        {
            this._declaringTypeCacheKey = this.Events.First().DeclaringTypeData.CacheKey;

            ArgumentExceptionAdvanced.ThrowIfAny(
                this.Events,
                eventData => eventData.DeclaringTypeData.CacheKey != this._declaringTypeCacheKey,
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(EventData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All events must belong to the same declaring type.");

        }

        this._hashCode = ComputeHashCode();
    }

    private EventList()
    {
        this.Events = ImmutableList<EventData>.Empty;
        this._eventNameIndex = new Dictionary<string, EventData>(0, StringComparer.Ordinal);
    }

    public bool TryGetEventByName(string eventName, out EventData? eventData)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(eventName);
        return this._eventNameIndex.TryGetValue(eventName, out eventData);
    }

    public int Count => this.Events.Count;
    public bool IsEmpty => this.Events.IsEmpty;
    public bool HasItems => !this.IsEmpty;
    public ImmutableList<EventData> Events { get; }
    public SymbolReflectionInfoCacheKey DeclaringTypeCacheKey
        => this.HasItems
            ? this._declaringTypeCacheKey
            : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(MethodList), nameof(this.DeclaringTypeCacheKey)));

    public TypeData DeclaringTypeData
    {
        get
        {
            SymbolReflectionInfoCacheKey cacheKey = this.DeclaringTypeCacheKey;
            return this.HasItems
                ? SymbolReflectionInfoCache.GetOrCreateTypeDataCacheEntry(ref cacheKey)
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(MethodList), nameof(this.DeclaringTypeData)));
        }
    }

    public EventData this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this.Events.Count, nameof(index));

            return this.HasItems
                ? this.Events[index]
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(EventList), ReflectionConstants.IndexerGetMethodName));
        }
    }

    public IEnumerator<EventData> GetEnumerator()
        => ((IEnumerable<EventData>)this.Events).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => this.Events.GetEnumerator();

    public bool Equals(EventList? other)
    {
        if (other is null)
        {
            return false;
        }

        if (this.Count != other.Count)
        {
            return false;
        }

        if (!this.DeclaringTypeCacheKey.Equals(other.DeclaringTypeCacheKey))
        {
            return false;
        }

        for (int index = 0; index < this.Count; index++)
        {
            if (!this.Events[index].Equals(other.Events[index]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj)
        => obj is EventList other && Equals(other);

    public override int GetHashCode() => this._hashCode;

    private int ComputeHashCode()
    {
        unchecked
        {
            var hashCode = new HashCode();
            hashCode.Add(this.Count);
            hashCode.Add(this.DeclaringTypeCacheKey);
            for (int index = 0; index < this.Events.Count; index++)
            {
                hashCode.Add(this.Events[index]);
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
        this.Events = items?.ToImmutableList() ?? ImmutableList<TMemberData>.Empty;
        this._memberIndex = this.Events.ToDictionary(eventData => eventData.Name, StringComparer.Ordinal);

        if (this.HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfAny(
                this.Events,
                eventData => !eventData.IsExplicitInterfaceImplementation,
                nameof(items),
                $"Invalid argument '{nameof(items)}'. All events must be explicit interface implementations.");

        }

        this._hashCode = ComputeHashCode();
    }

    internal ExplicitMemberImplementationList(IEnumerable<TMemberData> items, bool isIntegrityValidationEnabled)
    {
        this.Events = items?.ToImmutableList() ?? ImmutableList<TMemberData>.Empty;
        this._memberIndex = this.Events.ToDictionary(eventData => eventData.Name, StringComparer.Ordinal);

        if (isIntegrityValidationEnabled && this.HasItems)
        {
            this._declaringTypeCacheKey = this.Events.First().DeclaringTypeData.CacheKey;

            ArgumentExceptionAdvanced.ThrowIfAny(
                this.Events,
                eventData => eventData.DeclaringTypeData.CacheKey != this._declaringTypeCacheKey,
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(EventData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All events must belong to the same declaring type.");

        }

        this._hashCode = ComputeHashCode();
    }

    private ExplicitMemberImplementationList()
    {
        this.Events = ImmutableList<EventData>.Empty;
        this._memberIndex = new Dictionary<string, EventData>(0, StringComparer.Ordinal);
    }

    public bool TryGetEventByName(string eventName, out EventData? eventData)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(eventName);
        return this._memberIndex.TryGetValue(eventName, out eventData);
    }

    public int Count => this.Events.Count;
    public bool IsEmpty => this.Events.IsEmpty;
    public bool HasItems => !this.IsEmpty;
    public ImmutableList<TMemberData> Events { get; }
    public SymbolReflectionInfoCacheKey DeclaringTypeCacheKey
        => this.HasItems
            ? this._declaringTypeCacheKey
            : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(MethodList), nameof(this.DeclaringTypeCacheKey)));

    public TypeData DeclaringTypeData
    {
        get
        {
            SymbolReflectionInfoCacheKey cacheKey = this.DeclaringTypeCacheKey;
            return this.HasItems
                ? SymbolReflectionInfoCache.GetOrCreateTypeDataCacheEntry(ref cacheKey)
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(MethodList), nameof(this.DeclaringTypeData)));
        }
    }

    TMemberData IReadOnlyList<TMemberData>.this[int index] { get; }

    public EventData this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this.Events.Count, nameof(index));

            return this.HasItems
                ? this.Events[index]
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(EventList), ReflectionConstants.IndexerGetMethodName));
        }
    }

    public IEnumerator<EventData> GetEnumerator()
        => ((IEnumerable<EventData>)this.Events).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => this.Events.GetEnumerator();

    public bool Equals(EventList? other)
    {
        if (other is null)
        {
            return false;
        }

        if (this.Count != other.Count)
        {
            return false;
        }

        if (!this.DeclaringTypeCacheKey.Equals(other.DeclaringTypeCacheKey))
        {
            return false;
        }

        for (int index = 0; index < this.Count; index++)
        {
            if (!this.Events[index].Equals(other.Events[index]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj)
        => obj is EventList other && Equals(other);

    public override int GetHashCode() => this._hashCode;

    private int ComputeHashCode()
    {
        unchecked
        {
            var hashCode = new HashCode();
            hashCode.Add(this.Count);
            hashCode.Add(this.DeclaringTypeCacheKey);
            for (int index = 0; index < this.Events.Count; index++)
            {
                hashCode.Add(this.Events[index]);
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
