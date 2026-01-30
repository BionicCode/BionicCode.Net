namespace BionicCode.Utilities.Net
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
        /// <param name="declaringTypeHandle">The <see cref="RuntimeTypeHandle"/> for the type that declares the event.
        /// <para/>If the event is an explicit interface implementation then the handle must point to the declaring interface type.</param>
        /// <param name="eventName">The name of the event. Cannot be <see langword="null"/>, empty or consist of white-space characters.</param>
        /// <param name="declaredEventAccessors">Specifies the accessors that the event declares. Cannot be <see cref="EventAccessors.None"/>.</param>
        /// <param name="isExplicitInterfaceImplementation"><see langword="true"/> if the event is an explicit interface implementation; otherwise, <see langword="false"/>.
        /// <br/>If set to <see langword="true"/>, the <paramref name="declaringTypeHandle"/> must be obtained from the declaring interface type.
        /// </param>
        /// <returns>A new instance of <see cref="AnonymousEventDescriptor"/> representing the specified anonymous or well-known property.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventName"/> is <see langword="null"/>, empty or only consists of white-space characters.</exception>
        /// <exception cref="ArgumentException">Thrown when
        /// <list type="bullet">
        /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="true"/> but <paramref name="declaringTypeHandle"/> was not obtained from an interface type.</item>
        /// <item>the provided <paramref name="declaredEventAccessors"/> value is not defined by the enum <see cref="EventAccessors"/>.</item>
        /// <item>the provided <paramref name="declaredEventAccessors"/> value is <see cref="EventAccessors.None"/>.</item>
        /// </list>
        /// </exception>
        public AnonymousEventDescriptor(
            RuntimeTypeHandle declaringTypeHandle,
            string eventName,
            EventAccessors declaredEventAccessors,
            bool isExplicitInterfaceImplementation,
            RuntimeTypeHandle? declaringInterfaceTypeHandle,)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle);
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
                Type? declaringType = Type.GetTypeFromHandle(declaringTypeHandle);
                ArgumentNullExceptionAdvanced.ThrowIfNull(
                    declaringType,
                    nameof(declaringTypeHandle),
                    $"Invalid argument '{nameof(declaringTypeHandle)}'. The provided declaring type handle does not resolve to a runtime type.");

                ArgumentExceptionAdvanced.ThrowIfFalse(
                    declaringType!.IsInterface,
                    nameof(isExplicitInterfaceImplementation),
                    $"Invalid argument '{nameof(declaringTypeHandle)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' returns 'true' while the declaring type represented by the argument '{nameof(declaringTypeHandle)}' is not pointing to an interface type. Reason: Only interface types can provide the declaration of explicit interface implementations.");
            }

            this.DeclaringTypeHandle = declaringTypeHandle;
            this.IsExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
            this.IsAnonymous = true;
        }

        public EventAccessors DeclaredAccessors { get; }
        public bool IsExplicitInterfaceImplementation { get; init; }
        public bool IsAnonymous { get; }
        public RuntimeTypeHandle DeclaringTypeHandle { get; }

        public bool HasAddDelegateEventAccessor { get; }
        public bool HasRemoveDelegateEventAccessor { get; }

        public bool Equals(AnonymousEventDescriptor other)
            => this.IsExplicitInterfaceImplementation.Equals(other.IsExplicitInterfaceImplementation)
            && this.IsAnonymous == other.IsAnonymous
            && this.DeclaringTypeHandle.Equals(other.DeclaringTypeHandle)
            && this.HasAddDelegateEventAccessor == other.HasAddDelegateEventAccessor
            && this.HasRemoveDelegateEventAccessor == other.HasRemoveDelegateEventAccessor
            && this.DeclaredAccessors == other.DeclaredAccessors;

        public override int GetHashCode()
        {
            var hasCode = new HashCode();
            hasCode.Add(this.IsExplicitInterfaceImplementation);
            hasCode.Add(this.DeclaredAccessors);
            hasCode.Add(this.HasAddDelegateEventAccessor);
            hasCode.Add(this.HasRemoveDelegateEventAccessor);
            hasCode.Add(this.IsAnonymous);
            hasCode.Add(this.DeclaringTypeHandle);

            return hasCode.ToHashCode();
        }

        public static bool operator ==(AnonymousEventDescriptor left, AnonymousEventDescriptor right)
            => left.Equals(right);
        public static bool operator !=(AnonymousEventDescriptor left, AnonymousEventDescriptor right)
            => !(left == right);

        public override bool Equals(object obj)
            => obj is AnonymousEventDescriptor other && Equals(other);
    }
}
