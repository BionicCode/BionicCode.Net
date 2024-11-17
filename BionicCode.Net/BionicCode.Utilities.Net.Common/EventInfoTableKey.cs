namespace BionicCode.Utilities.Net
{
  #region Info
  // //  
  // BionicUtilities.Net.Standard
  #endregion

  using System;
  using System.Collections.Generic;
  using System.Collections.Immutable;
  using System.Linq;

  internal readonly struct EventInfoTableKey : IEquatable<EventInfoTableKey>
  {
    public EventInfoTableKey(string eventName, Type eventSourceType)
    {
      this.EventName = eventName;
      this.EventSourceType = eventSourceType;
      this.typeHierarchyFactory = new Lazy<ImmutableHashSet<Type>>(() => ImmutableHashSet.CreateRange(eventSourceType.GetTypeHierarchy(includeInterfaces: true)));
    }

    public string EventName { get; }
    public Type EventSourceType { get; }
    private readonly Lazy<ImmutableHashSet<Type>> typeHierarchyFactory;
    public ImmutableHashSet<Type> TypeHierarchy => this.typeHierarchyFactory.Value;

    public bool Equals(EventInfoTableKey other) => other.EventName.Equals(this.EventName, StringComparison.OrdinalIgnoreCase) && other.EventSourceType.Equals(this.EventSourceType);
    public override bool Equals(object obj) => obj is EventInfoTableKey key && Equals(key);

    public override int GetHashCode()
    {
      int hashCode = 433094870;
      hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.EventName);
      hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(this.EventSourceType);
      return hashCode;
    }

    public static bool operator ==(EventInfoTableKey first, EventInfoTableKey second) => first.Equals(second);
    public static bool operator !=(EventInfoTableKey first, EventInfoTableKey second) => !first.Equals(second);
  }

  internal readonly struct EventHandlerTableKey : IEquatable<EventInfoTableKey>
  {
    public EventHandlerTableKey(string eventName, Type eventHandlerType)
    {
      this.EventName = eventName;
      this.EventSourceType = eventSourceType;
      this.typeHierarchyFactory = new Lazy<ImmutableHashSet<Type>>(() => ImmutableHashSet.CreateRange(eventSourceType.GetTypeHierarchy(includeInterfaces: true)));
    }

    public string EventName { get; }
    public Type EventSourceType { get; }
    private readonly Lazy<ImmutableHashSet<Type>> typeHierarchyFactory;
    public ImmutableHashSet<Type> TypeHierarchy => this.typeHierarchyFactory.Value;

    public bool Equals(EventInfoTableKey other) => other.EventName.Equals(this.EventName, StringComparison.OrdinalIgnoreCase) && other.EventSourceType.Equals(this.EventSourceType);
    public override bool Equals(object obj) => obj is EventInfoTableKey key && Equals(key);

    public override int GetHashCode()
    {
      int hashCode = 433094870;
      hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.EventName);
      hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(this.EventSourceType);
      return hashCode;
    }

    public static bool operator ==(EventInfoTableKey first, EventInfoTableKey second) => first.Equals(second);
    public static bool operator !=(EventInfoTableKey first, EventInfoTableKey second) => !first.Equals(second);
  }
}