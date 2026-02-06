namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;

    internal readonly struct AddClientHandlerInvocatorTableKey : IEquatable<AddClientHandlerInvocatorTableKey>
    {
        public AddClientHandlerInvocatorTableKey(Type eventSourceType, Type eventHandlerType)
        {
            EventSourceType = eventSourceType;
            EventHandlerType = eventHandlerType;
        }

        public Type EventHandlerType { get; }
        public Type EventSourceType { get; }

        public bool Equals(AddClientHandlerInvocatorTableKey other) => other.EventHandlerType.Equals(EventHandlerType) && other.EventSourceType.Equals(EventSourceType);
        public override bool Equals(object obj) => obj is AddClientHandlerInvocatorTableKey key && Equals(key);

        public override int GetHashCode()
        {
            int hashCode = 433094870;
            hashCode = (hashCode * -1521134295) + EqualityComparer<Type>.Default.GetHashCode(EventHandlerType);
            hashCode = (hashCode * -1521134295) + EqualityComparer<Type>.Default.GetHashCode(EventSourceType);
            return hashCode;
        }

        public static bool operator ==(AddClientHandlerInvocatorTableKey first, AddClientHandlerInvocatorTableKey second) => first.Equals(second);
        public static bool operator !=(AddClientHandlerInvocatorTableKey first, AddClientHandlerInvocatorTableKey second) => !first.Equals(second);
    }
}
