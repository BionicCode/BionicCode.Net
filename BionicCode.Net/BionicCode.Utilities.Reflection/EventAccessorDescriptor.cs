namespace BionicCode.Utilities.Net.Reflection
{
    using System;
    using System.Reflection;

    /// <summary>
    /// A descriptor that provides information about an well-known event accessor, where the caller does has a direct <see cref="MethodInfo"/> representation of the method symbol.
    /// </summary>
    /// <remarks>The <see cref="PropertyAccessorDescriptor"/> is used to provide information for well-known event accessor method symbols, which is when the caller has the direct <see cref="MethodInfo"/> representation.
    /// <para/>Important: well-known descriptors are preferred over anonymous descriptors when the <see cref="MethodInfo"/> is available to ensure maximum accuracy and performance.
    /// </remarks>
    internal readonly struct EventAccessorDescriptor : IEquatable<EventAccessorDescriptor>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventAccessorDescriptor"/> struct.
        /// </summary>
        /// <remarks>The <see cref="EventAccessorDescriptor"/> is used to provide information about a event accessor method symbol of which the caller does not have a direct <see cref="MethodInfo"/> representation and instead only signature information is available.
        /// <para/>If the property or event is an explicit interface implementation, then the <paramref name="methodHandle"/> must not be obtained from an interface type.
        /// </remarks>
        /// <param name="methodHandle">The runtime type handle representing the declaring type of the event accessor method.
        /// <para/>If the property is an explicit interface implementation, then the value must not be obtained from the interface type but the implementing type.
        /// </param>
        /// <param name="isExplicitInterfaceImplementation"></param>
        /// <param name="eventName">The name of the event that the anonymous accessor method is associated with. Cannot be null, empty, or consist only of white-space characters unless the property is an indexer. For indexer properties this parameter is ignored.</param>
        /// <param name="accessorKind">This parameter specifies the kind of the accessor.
        /// <para/>This parameter is required and is not allowed to be <see cref="EventAccessors.None"/> or <see cref="EventAccessors.AddAndRemove"/>.</param>
        /// <returns>A new instance of <see cref="EventAccessorDescriptor"/> representing the specified anonymous property or event accessor.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="methodHandle"/> is <see langword="default"/> or <paramref name="eventName"/> is <see langword="null"/> (or only consists of white-space characters or  is an empty string).</exception>
        /// <exception cref="ArgumentException">Thrown when
        /// <list type="bullet">
        /// <item>Is also thrown when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/> but <paramref name="methodHandle"/> is not an interface type.</item>
        /// <item>Is also thrown when <paramref name="accessorKind"/> has a value that is not defined by the <see cref="AccessorKind"/> enum.</item>
        /// <item>Is also thrown when <paramref name="accessorKind"/> has value <see cref="PropertyAccessors.None"/> or <see cref="PropertyAccessors.None"/>.</item>
        /// </list>
        /// </exception>
        public EventAccessorDescriptor(
            RuntimeMethodHandle methodHandle,
            bool isExplicitInterfaceImplementation,
            string? eventName,
            EventAccessors accessorKind)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(methodHandle);
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<EventAccessors>(accessorKind);
            ArgumentExceptionAdvanced.ThrowIfEnumEqualsAny(
                accessorKind,
                [EventAccessors.None, EventAccessors.AddAndRemove],
                nameof(accessorKind),
                $"Invalid argument '{nameof(accessorKind)}'. The argument '{nameof(accessorKind)}' has an undefined value. The value '{accessorKind}' is not allowed.");

            ArgumentNullExceptionAdvanced.ThrowIfNullOrWhiteSpace(eventName);

            if (isExplicitInterfaceImplementation)
            {
                MethodBase? accessor = MethodInfo.GetMethodFromHandle(methodHandle);
                ArgumentNullExceptionAdvanced.ThrowIfNull(
                    accessor,
                    nameof(methodHandle),
                    $"The method represented by the argument '{nameof(methodHandle)}' could not be resolved.");
                Type? declaringType = accessor?.DeclaringType;
                ArgumentNullExceptionAdvanced.ThrowIfNull(
                    accessor,
                    nameof(declaringType),
                    $"The declaring type of the argument '{nameof(methodHandle)}' could not be resolved.");
                ArgumentExceptionAdvanced.ThrowIfTrue(declaringType!.IsInterface,
                    nameof(isExplicitInterfaceImplementation),
                    $"Invalid argument '{nameof(methodHandle)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' returns 'true' while the declaring type of the accessor method represented by the argument '{nameof(methodHandle)}' is an interface type. Reason: Only non-interface types can provide the explicit implementation.");
            }

            MethodHandle = methodHandle;
            EventName = eventName;
            AccessorKind = accessorKind;
            IsExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
            IsAnonymous = false;
        }

        public RuntimeMethodHandle MethodHandle { get; }
        public string EventName { get; }
        public EventAccessors AccessorKind { get; }
        public bool IsExplicitInterfaceImplementation { get; }
        public bool IsAnonymous { get; }

        public bool Equals(EventAccessorDescriptor other)
            => MethodHandle.Equals(other.MethodHandle)
            && EventName.Equals(other.EventName, StringComparison.Ordinal)
            && IsExplicitInterfaceImplementation.Equals(other.IsExplicitInterfaceImplementation)
            && AccessorKind.Equals(other.AccessorKind)
            && IsAnonymous == other.IsAnonymous;

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(MethodHandle);
            hashCode.Add(EventName);
            hashCode.Add(IsExplicitInterfaceImplementation);
            hashCode.Add(AccessorKind);
            hashCode.Add(IsAnonymous);

            return hashCode.ToHashCode();
        }

        public static bool operator ==(EventAccessorDescriptor left, EventAccessorDescriptor right)
            => left.Equals(right);
        public static bool operator !=(EventAccessorDescriptor left, EventAccessorDescriptor right)
            => !(left == right);

        public override bool Equals(object obj)
            => obj is EventAccessorDescriptor other && Equals(other);
    }
}
