namespace BionicCode.Utilities.Net
{
    using System;
    using System.Reflection;

    /// <summary>
    /// A descriptor that provides information about a well-known event.
    /// </summary>
    /// <remarks>The <see cref="WellKnownEventDescriptor"/> is used to provide information for well-known event symbols, which is when the caller has the direct <see cref="System.Reflection.EventInfo"/> representation.
    /// <para/>When the caller does not have the direct <see cref="System.Reflection.EventInfo"/> representation and only signature information is available the event symbol is considered anonymous. In such case use the <see cref="AnonymousEventDescriptor"/> instead.
    /// <para/>Important: well-known descriptors are preferred over anonymous descriptors when the <see cref="System.Reflection.EventInfo"/> is available to ensure maximum accuracy and performance.
    /// </remarks>
    internal readonly struct WellKnownEventDescriptor : IEquatable<WellKnownEventDescriptor>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="WellKnownEventDescriptor"/> struct for a well-known event.
        /// </summary>
        /// <remarks>
        /// If the event is an explicit interface implementation, ensure to set the <paramref name="isExplicitInterfaceImplementation"/> parameter to <see langword="true"/> and provide the <paramref name="eventInfo"/> obtained from the declaring interface type (it's crucial to provide the interface type as the declaring type).
        /// <para/>If the <see cref="System.Reflection.EventInfo"/> or the corresponding accessor methods to satisfy <paramref name="explicitAddImplementationMethod"/> and <paramref name="explicitRemoveImplementationMethod"/> are unknown create an anonymous descriptor using the <see cref="AnonymousEventDescriptor"/>.
        /// <para/>For best accuracy and performance always use this <see cref="WellKnownEventDescriptor"/> when the caller has direct access to the <see cref="System.Reflection.EventInfo"/> representation of the event.
        /// </remarks>
        /// <param name="eventInfo">The <see cref="System.Reflection.EventInfo"/> that the descriptor represents. If <paramref name="isExplicitInterfaceImplementation"/> is set to <see langword="true"/> then the <see cref="System.Reflection.EventInfo"/> must be obtained from the declaring interface type.</param>
        /// <param name="isExplicitInterfaceImplementation"><see langword="true"/> if the event is an explicit interface implementation; otherwise, <see langword="false"/>. If set to <see langword="true"/>, the <paramref name="eventInfo"/> must be obtained from the declaring interface type.
        /// </param>
        /// <param name="explicitAddImplementationMethod"></param>
        /// <param name="explicitRemoveImplementationMethod"></param>
        /// <returns>A new instance of <see cref="WellKnownPropertyDescriptor"/> representing the specified anonymous or well-known property.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when
        /// <list type="bullet">
        /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="true"/> but <paramref name="eventInfo"/> was not obtained from an interface type.</item>
        /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="true"/> but neither <paramref name="explicitAddImplementationMethod"/> (for a readable property) nor <paramref name="explicitRemoveImplementationMethod"/> (for a writeable property) is provided.</item>
        /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="false"/> but either <paramref name="explicitAddImplementationMethod"/> or <paramref name="explicitRemoveImplementationMethod"/> is provided.</item>
        /// </list>
        /// </exception>
        public WellKnownEventDescriptor(
            EventInfo eventInfo,
            bool isExplicitInterfaceImplementation,
            RuntimeTypeHandle? declaringInterfaceTypeHandle
            MethodInfo? explicitAddImplementationMethod,
            MethodInfo? explicitRemoveImplementationMethod)
        {
            ArgumentNullException.ThrowIfNull(eventInfo);
            Type? declaringType = eventInfo.DeclaringType;
            ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);

            this.HasAddDelegateEventAccessor = eventInfo.AddMethod is not null;
            if (this.HasAddDelegateEventAccessor)
            {
                this.DeclaredEventAccessors = EventAccessors.Add;
            }

            this.HasRemoveDelegateEventAccessor = eventInfo.RemoveMethod is not null;
            if (this.HasRemoveDelegateEventAccessor)
            {
                this.DeclaredEventAccessors |= EventAccessors.Remove;
            }

            if (isExplicitInterfaceImplementation)
            {
                ArgumentExceptionAdvanced.ThrowIfFalse(declaringType is not null && declaringType.IsInterface,
                    nameof(isExplicitInterfaceImplementation),
                    $"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' returns 'true' while the declaring type represented by the argument '{nameof(eventInfo)}.{nameof(eventInfo.DeclaringType)}' is not an interface. Reason: Only interface types can provide the declaration of explicit interface implementations.");

                if (this.HasAddDelegateEventAccessor ^ explicitAddImplementationMethod is not null)
                {
                    throw new ArgumentException($"Invalid argument combination for the explicit interface implementation. The argument '{nameof(eventInfo)}' declares an add accessor, but the '{nameof(explicitAddImplementationMethod)}' is null. Or vice versa. Reason: Both arguments must be provided to describe an event add accessor.");
                }

                if (this.HasRemoveDelegateEventAccessor && explicitRemoveImplementationMethod is null)
                {
                    throw new ArgumentException($"Invalid argument combination for the explicit interface implementation. The argument '{nameof(eventInfo)}' declares a remove accessor, but the '{nameof(explicitRemoveImplementationMethod)}' is null. Or vice versa. Reason: Both arguments must be provided to describe an event remove accessor.");
                }
            }
            else
            {
                if (explicitRemoveImplementationMethod is not null || explicitAddImplementationMethod is not null)
                {
                    throw new ArgumentException($"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' is set to 'false', but either a '{nameof(explicitAddImplementationMethod)}' or a '{nameof(explicitRemoveImplementationMethod)}' is provided. Reason: Accessor method descriptors can only be provided for explicit interface implementation properties.");
                }
            }

            this.HasExplicitAddEventAccessor = isExplicitInterfaceImplementation && this.HasAddDelegateEventAccessor;
            this.HasExplicitRemoveEventAccessor = isExplicitInterfaceImplementation && this.HasRemoveDelegateEventAccessor;
            this.IsExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
            this._explicitAddImplementationMethodDescriptor = this.HasAddDelegateEventAccessor && isExplicitInterfaceImplementation
                ? new WellKnownPropertyOrEventAccessorDescriptor(eventInfo.AddMethod!, PropertyAccessors.EventAdd, true)
                : default;
            this._explicitRemoveImplementationMethodDescriptor = this.HasRemoveDelegateEventAccessor && isExplicitInterfaceImplementation
                ? new WellKnownPropertyOrEventAccessorDescriptor(eventInfo.RemoveMethod!, PropertyAccessors.EventRemove, true)
                : default;
            this.IsAnonymous = false;
            this.EventInfo = eventInfo;
        }

        public EventAccessors DeclaredEventAccessors { get; }
        public bool IsExplicitInterfaceImplementation { get; init; }
        public bool HasExplicitAddEventAccessor { get; }
        public bool HasExplicitRemoveEventAccessor { get; }
        public bool IsAnonymous { get; }

        private readonly WellKnownPropertyOrEventAccessorDescriptor _explicitAddImplementationMethodDescriptor;
        public WellKnownPropertyOrEventAccessorDescriptor ExplicitAddImplementationMethodDescriptor
            => this.IsExplicitInterfaceImplementation && this.HasAddDelegateEventAccessor
                ? this._explicitAddImplementationMethodDescriptor
                : throw new InvalidOperationException($"The property '{nameof(this.ExplicitAddImplementationMethodDescriptor)}' cannot be accessed for implicit property implementations or write-only properties.");

        private readonly WellKnownPropertyOrEventAccessorDescriptor _explicitRemoveImplementationMethodDescriptor;
        public WellKnownPropertyOrEventAccessorDescriptor ExplicitRemoveImplementationMethodDescriptor
            => this.IsExplicitInterfaceImplementation && this.HasRemoveDelegateEventAccessor
                ? this._explicitRemoveImplementationMethodDescriptor
                : throw new InvalidOperationException($"The property '{nameof(this.ExplicitRemoveImplementationMethodDescriptor)}' cannot be accessed for implicit property implementations or write-only properties.");

        public EventInfo EventInfo { get; }

        public bool HasAddDelegateEventAccessor { get; }
        public bool HasRemoveDelegateEventAccessor { get; }

        public bool Equals(WellKnownEventDescriptor other)
            => this.IsExplicitInterfaceImplementation.Equals(other.IsExplicitInterfaceImplementation)
            && this.HasExplicitAddEventAccessor.Equals(other.HasExplicitAddEventAccessor)
            && this.HasExplicitRemoveEventAccessor.Equals(other.HasExplicitRemoveEventAccessor)
            && this.ExplicitAddImplementationMethodDescriptor.Equals(other.ExplicitAddImplementationMethodDescriptor)
            && this.ExplicitRemoveImplementationMethodDescriptor.Equals(other.ExplicitRemoveImplementationMethodDescriptor)
            && this.IsAnonymous == other.IsAnonymous
            && ReferenceEquals(this.EventInfo, other.EventInfo)
            && this.HasAddDelegateEventAccessor == other.HasAddDelegateEventAccessor
            && this.HasRemoveDelegateEventAccessor == other.HasRemoveDelegateEventAccessor;

        public override int GetHashCode()
        {
            var hasCode = new HashCode();
            hasCode.Add(this.IsExplicitInterfaceImplementation);
            hasCode.Add(this.HasExplicitAddEventAccessor);
            hasCode.Add(this.HasExplicitRemoveEventAccessor);
            hasCode.Add(this.ExplicitAddImplementationMethodDescriptor);
            hasCode.Add(this.ExplicitRemoveImplementationMethodDescriptor);
            hasCode.Add(this.HasAddDelegateEventAccessor);
            hasCode.Add(this.HasRemoveDelegateEventAccessor);
            hasCode.Add(this.IsAnonymous);
            hasCode.Add(this.EventInfo);

            return hasCode.ToHashCode();
        }

        public static bool operator ==(WellKnownEventDescriptor left, WellKnownEventDescriptor right)
            => left.Equals(right);
        public static bool operator !=(WellKnownEventDescriptor left, WellKnownEventDescriptor right)
            => !(left == right);

        public override bool Equals(object obj)
            => obj is WellKnownEventDescriptor other && Equals(other);
    }
}
