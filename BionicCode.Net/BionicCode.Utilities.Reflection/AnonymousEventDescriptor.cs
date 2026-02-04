namespace BionicCode.Utilities.Net.Reflection
{
    using System;

    /// <summary>
    /// A descriptor that provides information about a well-known event.
    /// </summary>
    /// <remarks>The <see cref="WellKnownEventDescriptor"/> is used to provide information for well-known event symbols, which is when the caller has the direct <see cref="System.Reflection.EventInfo"/> representation.
    /// <para/>When the caller does have the direct <see cref="System.Reflection.EventInfo"/> representation the event symbol is considered well-known. In such case use the <see cref="WellKnownEventDescriptor"/> instead.
    /// <para/>Important: well-known descriptors are preferred over anonymous descriptors when the <see cref="System.Reflection.EventInfo"/> is available to ensure maximum accuracy and performance.
    /// </remarks>
    internal readonly struct AnonymousEventDescriptor : IEquatable<AnonymousEventDescriptor>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AnonymousEventDescriptor"/> struct for a anonymous event.
        /// </summary>
        /// <remarks>
        /// If the event is an explicit interface implementation, ensure to set the <paramref name="isExplicitInterfaceImplementation"/> parameter to <see langword="true"/> and provide the <paramref name="declaringTypeHandle"/> obtained from the declaring interface type (it's crucial to provide the interface type as the declaring type).
        /// <para/>For best accuracy and performance always use this <see cref="WellKnownEventDescriptor"/> when the caller has direct access to the <see cref="System.Reflection.EventInfo"/> representation of the event.
        /// </remarks>
        /// <param name="declaringTypeHandle">The <see cref="RuntimeTypeHandle"/> for the type that implements the event.
        /// <para/>If the event is an explicit interface implementation (which is when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="false"/>),
        /// then the <paramref name="declaringTypeHandle"/> must be a <see cref="RuntimeTypeHandle"/> obtained from the interface type that originally declares the event.
        /// </param>
        /// <param name="implementingTypeHandle">If the event is an explicit interface implementation,
        /// then the <paramref name="implementingTypeHandle"/> must be a <see cref="RuntimeTypeHandle"/> obtained from the type that provides the explicit interface implementation.
        /// <para/>If the event is not an explicit interface implementation (which is when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="false"/>),
        /// then the <paramref name="implementingTypeHandle"/> can be <see langword="null"/> since it will be ignored.</param>
        /// <param name="eventName">The name of the event. Cannot be <see langword="null"/>, empty or consist of white-space characters.</param>
        /// <param name="declaredEventAccessors">Specifies the accessors that the event declares. Cannot be <see cref="EventAccessors.None"/>.</param>
        /// <param name="isExplicitInterfaceImplementation"><see langword="true"/> if the event is an explicit interface implementation; otherwise, <see langword="false"/>.
        /// <br/>If set to <see langword="true"/>, the <paramref name="declaringTypeHandle"/> must provide the <see cref="RuntimeTypeHandle"/> that was obtained from the interface type that originally declared the event.
        /// </param>
        /// <returns>A new instance of <see cref="AnonymousEventDescriptor"/> representing the specified anonymous event.</returns>
        /// <exception cref="ArgumentNullException">Thrown when
        /// <list type="bullet">
        /// <item><paramref name="declaringTypeHandle"/> is <see langword="default"/>.</item>
        /// <item><paramref name="eventName"/> is <see langword="null"/>, empty, or consists only of white-space characters.</item>
        /// <item>Is also thrown when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/> but <paramref name="implementingTypeHandle"/> is <see langword="null"/>.</item>
        /// <item>Is also thrown when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/> but <paramref name="implementingTypeHandle"/> is <see langword="default"/>.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when
        /// <list type="bullet">
        /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="true"/> but <paramref name="declaringTypeHandle"/> was not obtained from an interface type.</item>
        /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="true"/> but <paramref name="implementingTypeHandle"/> was obtained from an interface type.</item>
        /// <item>the provided <paramref name="declaringTypeHandle"/> refers to an interface type.</item>
        /// <item>the provided <paramref name="declaredEventAccessors"/> value is not defined by the enum <see cref="EventAccessors"/>.</item>
        /// <item>the provided <paramref name="declaredEventAccessors"/> value is <see cref="EventAccessors.None"/>.</item>
        /// </list>
        /// </exception>
        public AnonymousEventDescriptor(
            RuntimeTypeHandle declaringTypeHandle,
            string eventName,
            EventAccessors declaredEventAccessors,
            bool isExplicitInterfaceImplementation,
            RuntimeTypeHandle? implementingTypeHandle)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle);
            Type? declaringType = Type.GetTypeFromHandle(declaringTypeHandle);
            ArgumentNullExceptionAdvanced.ThrowIfNull(
                declaringType,
                nameof(declaringTypeHandle),
                $"Invalid argument '{nameof(declaringTypeHandle)}'. The provided declaring type handle does not resolve to a runtime type.");

            ArgumentNullException.ThrowIfNullOrWhiteSpace(eventName);
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<EventAccessors>(
                declaredEventAccessors,
                nameof(declaredEventAccessors),
                $"Invalid argument '{nameof(declaredEventAccessors)}'. The value '{declaredEventAccessors}' is not defined on the enum '{nameof(PropertyAccessors)}'.");
            ArgumentExceptionAdvanced.ThrowIfEnumEqualsAny(
                declaredEventAccessors,
                [EventAccessors.None],
                $"Invalid argument '{nameof(declaredEventAccessors)}'. The value '{declaredEventAccessors}' is not allowed.");

            this.DeclaredAccessors = declaredEventAccessors;
            this.HasAddDelegateEventAccessor = (this.DeclaredAccessors & EventAccessors.Add) != 0;
            this.HasRemoveDelegateEventAccessor = (this.DeclaredAccessors & EventAccessors.Remove) != 0;

            if (isExplicitInterfaceImplementation)
            {
                ArgumentExceptionAdvanced.ThrowIfFalse(
                    declaringType!.IsInterface,
                    nameof(declaringTypeHandle),
                    $"Invalid argument '{nameof(declaringTypeHandle)}'. The argument '{nameof(declaringType)}' is pointing to an non-interface type. Reason: Only interface types can declare explicit member implementations.");

                ArgumentNullExceptionAdvanced.ThrowIfNull(
                    implementingTypeHandle,
                    nameof(implementingTypeHandle),
                    $"Invalid argument '{nameof(implementingTypeHandle)}'. The provided declaring interface type handle is not 'NULL' which is not allowed for explicit interface implementations (which is when '{nameof(isExplicitInterfaceImplementation)}' is 'true').");
                ArgumentNullExceptionAdvanced.ThrowIfDefault(implementingTypeHandle!.Value);
                Type? declaringInterfaceType = Type.GetTypeFromHandle(implementingTypeHandle!.Value);
                ArgumentNullExceptionAdvanced.ThrowIfNull(
                    declaringInterfaceType,
                    nameof(implementingTypeHandle),
                    $"The declaring interface type represented by the argument '{nameof(implementingTypeHandle)}' could not be resolved.");
                ArgumentExceptionAdvanced.ThrowIfTrue(declaringInterfaceType!.IsInterface,
                    nameof(isExplicitInterfaceImplementation),
                    $"Invalid argument '{nameof(implementingTypeHandle)}'. The argument '{nameof(implementingTypeHandle)}' points to a interface type. Reason: Only non-interface types can provide the explicit interface implementations.");
            }

            this.EventName = eventName;
            this.ImplementingTypeHandle = declaringTypeHandle;
            this.DeclaringInterfaceTypeHandle = isExplicitInterfaceImplementation
                ? implementingTypeHandle!.Value
                : default;
            this.IsExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
            this.IsAnonymous = true;
        }

        public string EventName { get; }
        public EventAccessors DeclaredAccessors { get; }
        public bool IsExplicitInterfaceImplementation { get; init; }
        public bool IsAnonymous { get; }
        public RuntimeTypeHandle ImplementingTypeHandle { get; }
        public RuntimeTypeHandle DeclaringInterfaceTypeHandle { get; }

        public bool HasAddDelegateEventAccessor { get; }
        public bool HasRemoveDelegateEventAccessor { get; }

        public bool Equals(AnonymousEventDescriptor other)
            => this.IsExplicitInterfaceImplementation.Equals(other.IsExplicitInterfaceImplementation)
            && this.IsAnonymous == other.IsAnonymous
            && this.ImplementingTypeHandle.Equals(other.ImplementingTypeHandle)
            && this.DeclaringInterfaceTypeHandle.Equals(other.DeclaringInterfaceTypeHandle)
            && this.HasAddDelegateEventAccessor == other.HasAddDelegateEventAccessor
            && this.HasRemoveDelegateEventAccessor == other.HasRemoveDelegateEventAccessor
            && this.DeclaredAccessors == other.DeclaredAccessors
            && this.EventName.Equals(other.EventName, StringComparison.Ordinal);

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(this.IsExplicitInterfaceImplementation);
            hashCode.Add(this.DeclaredAccessors);
            hashCode.Add(this.HasAddDelegateEventAccessor);
            hashCode.Add(this.HasRemoveDelegateEventAccessor);
            hashCode.Add(this.IsAnonymous);
            hashCode.Add(this.ImplementingTypeHandle);
            hashCode.Add(this.DeclaringInterfaceTypeHandle);
            hashCode.Add(this.EventName, StringComparer.Ordinal);

            return hashCode.ToHashCode();
        }

        public static bool operator ==(AnonymousEventDescriptor left, AnonymousEventDescriptor right)
            => left.Equals(right);
        public static bool operator !=(AnonymousEventDescriptor left, AnonymousEventDescriptor right)
            => !(left == right);

        public override bool Equals(object obj)
            => obj is AnonymousEventDescriptor other && Equals(other);
    }
}
