namespace BionicCode.Utilities.Net
{
    #region Info
    // //  
    // BionicUtilities.Net.Standard
    #endregion

    using System;
    using System.Collections.Generic;

    internal readonly struct EventInfoTableKey : IEquatable<EventInfoTableKey>
    {
        public EventInfoTableKey(string eventName, Type eventSourceType)
        {
            EventName = eventName;
            EventSourceType = eventSourceType;
        }

        public string EventName { get; }
        public Type EventSourceType { get; }

        public bool Equals(EventInfoTableKey other) => other.EventName.Equals(EventName, StringComparison.OrdinalIgnoreCase) && other.EventSourceType.Equals(EventSourceType);
        public override bool Equals(object obj) => obj is EventInfoTableKey key && Equals(key);

        public override int GetHashCode()
        {
            int hashCode = 433094870;
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(EventName);
            hashCode = (hashCode * -1521134295) + EqualityComparer<Type>.Default.GetHashCode(EventSourceType);
            return hashCode;
        }

        public static bool operator ==(EventInfoTableKey first, EventInfoTableKey second) => first.Equals(second);
        public static bool operator !=(EventInfoTableKey first, EventInfoTableKey second) => !first.Equals(second);
    }
}
