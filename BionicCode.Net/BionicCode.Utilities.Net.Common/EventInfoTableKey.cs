namespace BionicCode.Utilities.Net
{
  #region Info
  // //  
  // BionicUtilities.Net.Standard
  #endregion

  using System;
  using System.Collections.Generic;

  internal readonly struct EventInfoTableKey<TEventSource> : IEquatable<EventInfoTableKey<TEventSource>>
  {
    public EventInfoTableKey(string eventName)
    {
      this.EventName = eventName;
      this.EventSourceType = typeof(TEventSource);
    }

    public string EventName { get; }
    public Type EventSourceType { get; }

    public bool Equals(EventInfoTableKey<TEventSource> other) => other.EventName.Equals(this.EventName, StringComparison.OrdinalIgnoreCase) && other.EventSourceType.Equals(this.EventSourceType);
    public override bool Equals(object obj) => obj is EventInfoTableKey<TEventSource> key && Equals(key);

    public override int GetHashCode()
    {
      int hashCode = 433094870;
      hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.EventName);
      hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(this.EventSourceType);
      return hashCode;
    }

    public static bool operator ==(EventInfoTableKey<TEventSource> first, EventInfoTableKey<TEventSource> second) => first.Equals(second);
    public static bool operator !=(EventInfoTableKey<TEventSource> first, EventInfoTableKey<TEventSource> second) => !first.Equals(second);
  }
}