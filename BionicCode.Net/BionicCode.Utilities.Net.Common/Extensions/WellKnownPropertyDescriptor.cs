namespace BionicCode.Utilities.Net
{
    using System;
    using System.Reflection;

    /// <summary>
    /// A descriptor that provides information about a well-known property or indexer property.
    /// </summary>
    /// <remarks>The <see cref="WellKnownPropertyDescriptor"/> is used to provide information for well-known property symbols, which is when the caller has the direct <see cref="System.Reflection.PropertyInfo"/> representation.
    /// <para/>When the caller does not have the direct <see cref="System.Reflection.PropertyInfo"/> representation and only signature information is available the property symbol is considered anonymous. In such case use the <see cref="AnonymousPropertyDescriptor"/> instead.
    /// <para/>Important: well-known descriptors are preferred over anonymous descriptors when the <see cref="System.Reflection.PropertyInfo"/> is available to ensure maximum accuracy and performance.
    /// </remarks>
    internal readonly struct WellKnownPropertyDescriptor : IEquatable<WellKnownPropertyDescriptor>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="WellKnownPropertyDescriptor"/> struct for a well-known property.
        /// </summary>
        /// <remarks>
        /// If the property is an explicit interface implementation, ensure to set the <paramref name="isExplicitInterfaceImplementation"/> parameter to <see langword="true"/> and provide the <paramref name="propertyInfo"/> obtained from the declaring interface type (it's crucial to provide the interface type as the declaring type).
        /// <para/>If the <see cref="System.Reflection.PropertyInfo"/> or the corresponding accessor methods to satisfy <paramref name="explicitGetterImplementationMethod"/> and <paramref name="explicitSetterImplementationMethod"/> are unknow create an anonymous descriptor using the <see cref="AnonymousPropertyDescriptor"/>.
        /// <para/>For best accuracy and performance always use this <see cref="WellKnownPropertyDescriptor"/> when the caller has direct access to the <see cref="System.Reflection.PropertyInfo"/> representation of the property.
        /// </remarks>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> that the descriptor represents. If <paramref name="isExplicitInterfaceImplementation"/> is s et to <see langword="true"/> then the <see cref="PropertyInfo"/> must be obtained from the declaring interface type.</param>
        /// <param name="isExplicitInterfaceImplementation"><see langword="true"/> if the property is an explicit interface implementation; otherwise, <see langword="false"/>. If set to <see langword="true"/>, the <paramref name="propertyInfo"/> must be obtained from the declaring interface type.
        /// </param>
        /// <param name="explicitGetterImplementationMethod"></param>
        /// <param name="explicitSetterImplementationMethod"></param>
        /// <param name="isIndexerProperty">Specifies whether the property is an indexer.</param>
        /// <returns>A new instance of <see cref="WellKnownPropertyDescriptor"/> representing the specified anonymous or well-known property.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when
        /// <list type="bullet">
        /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="true"/> but <paramref name="propertyInfo"/> was not obtained from an interface type.</item>
        /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="true"/> but neither <paramref name="explicitGetterImplementationMethod"/> (for a readable property) nor <paramref name="explicitSetterImplementationMethod"/> (for a writeable property) is provided.</item>
        /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="false"/> but either <paramref name="explicitGetterImplementationMethod"/> or <paramref name="explicitSetterImplementationMethod"/> is provided.</item>
        /// </list>
        /// </exception>
        public WellKnownPropertyDescriptor(
            PropertyInfo propertyInfo,
            bool isExplicitInterfaceImplementation,
            MethodInfo? explicitGetterImplementationMethod,
            MethodInfo? explicitSetterImplementationMethod,
            bool isIndexerProperty)
        {
            ArgumentNullException.ThrowIfNull(propertyInfo);
            Type? declaringType = propertyInfo.DeclaringType;
            ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);

            this.IsReadableProperty = propertyInfo.CanRead;
            if (this.IsReadableProperty)
            {
                this.DeclaredPropertyAccessor = PropertyAccessor.PropertyGet;
            }

            this.IsWriteableProperty = propertyInfo.CanWrite;
            if (this.IsWriteableProperty)
            {
                this.DeclaredPropertyAccessor = this.IsReadableProperty
                    ? PropertyAccessor.PropertyGetAndPropertySet
                    : PropertyAccessor.PropertySet;
            }

            if (isExplicitInterfaceImplementation)
            {
                ArgumentExceptionAdvanced.ThrowIfFalse(declaringType is not null && declaringType.IsInterface,
                    nameof(isExplicitInterfaceImplementation),
                    $"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' returns 'true' while the declaring type represented by the argument '{nameof(propertyInfo)}.{nameof(propertyInfo.DeclaringType)}' is not an interface. Reason: Only interface types can provide the declaration of explicit interface implementations.");

                if (this.IsReadableProperty && explicitGetterImplementationMethod is null)
                {
                    throw new ArgumentException($"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' is set to 'true' for a readable property, but a '{nameof(explicitGetterImplementationMethod)}' is not provided. Reason: All property accessors must have a matching explicit implementation.");
                }

                if (this.IsWriteableProperty && explicitSetterImplementationMethod is null)
                {
                    throw new ArgumentException($"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' is set to 'true' for a writeable property, but a '{nameof(explicitSetterImplementationMethod)}' is not provided. Reason: All property accessors must have a matching explicit implementation.");
                }
            }
            else
            {
                if (explicitGetterImplementationMethod is not null || explicitSetterImplementationMethod is not null)
                {
                    throw new ArgumentException($"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' is set to 'false', but either a '{nameof(explicitGetterImplementationMethod)}' or a '{nameof(explicitSetterImplementationMethod)}' is provided. Reason: Accessor method descriptors can only be provided for explicit interface implementation properties.");
                }
            }

            this.HasExplicitGetPropertyAccessor = explicitGetterImplementationMethod is not null;
            this.HasExplicitSetPropertyAccessor = explicitSetterImplementationMethod is not null;
            this.IsExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
            this.IsIndexerProperty = isIndexerProperty;
            this._explicitGetterImplementationMethodDescriptor = this.IsReadableProperty && isExplicitInterfaceImplementation
                ? new WellKnownMethodOrConstructorDescriptor(propertyInfo.GetGetMethod(), PropertyAccessor.PropertyGet, true)
                : default;
            this._explicitSetterImplementationMethodDescriptor = this.IsWriteableProperty && isExplicitInterfaceImplementation
                ? new WellKnownMethodOrConstructorDescriptor(propertyInfo.GetSetMethod(), PropertyAccessor.PropertySet, true)
                : default;
            this.IsAnonymous = false;
            this._propertyInfo = propertyInfo;
        }

        public PropertyAccessor DeclaredPropertyAccessor { get; }
        public bool IsExplicitInterfaceImplementation { get; init; }
        public bool HasExplicitGetPropertyAccessor { get; }
        public bool HasExplicitSetPropertyAccessor { get; }
        public bool IsIndexerProperty { get; }
        public bool IsAnonymous { get; }

        private readonly WellKnownMethodOrConstructorDescriptor _explicitGetterImplementationMethodDescriptor;
        public WellKnownMethodOrConstructorDescriptor ExplicitGetterImplementationMethodDescriptor
            => this.IsExplicitInterfaceImplementation && this.IsReadableProperty
                ? this._explicitGetterImplementationMethodDescriptor
                : throw new InvalidOperationException($"The property '{nameof(this.ExplicitGetterImplementationMethodDescriptor)}' cannot be accessed for implicit property implementations or write-only properties.");

        private readonly WellKnownMethodOrConstructorDescriptor _explicitSetterImplementationMethodDescriptor;
        public WellKnownMethodOrConstructorDescriptor ExplicitSetterImplementationMethodDescriptor
            => this.IsExplicitInterfaceImplementation && this.IsWriteableProperty
                ? this._explicitSetterImplementationMethodDescriptor
                : throw new InvalidOperationException($"The property '{nameof(this.ExplicitSetterImplementationMethodDescriptor)}' cannot be accessed for implicit property implementations or write-only properties.");

        private readonly PropertyInfo? _propertyInfo;
        public PropertyInfo PropertyInfo
            => this.IsAnonymous
                ? throw new InvalidOperationException($"The property '{nameof(this.PropertyInfo)}' cannot be accessed for an anonymous property descriptor.")
                : this._propertyInfo!;

        public bool IsReadableProperty { get; }
        public bool IsWriteableProperty { get; }

        public bool Equals(WellKnownPropertyDescriptor other)
            => this.IsExplicitInterfaceImplementation.Equals(other.IsExplicitInterfaceImplementation)
            && this.HasExplicitGetPropertyAccessor.Equals(other.HasExplicitGetPropertyAccessor)
            && this.HasExplicitSetPropertyAccessor.Equals(other.HasExplicitSetPropertyAccessor)
            && this.ExplicitGetterImplementationMethodDescriptor.Equals(other.ExplicitGetterImplementationMethodDescriptor)
            && this.ExplicitSetterImplementationMethodDescriptor.Equals(other.ExplicitSetterImplementationMethodDescriptor)
            && this.IsIndexerProperty == other.IsIndexerProperty
            && this.IsAnonymous == other.IsAnonymous
            && ReferenceEquals(this._propertyInfo, other._propertyInfo)
            && this.IsReadableProperty == other.IsReadableProperty
            && this.IsWriteableProperty == other.IsWriteableProperty;

        public override int GetHashCode()
        {
            var hasCode = new HashCode();
            hasCode.Add(this.IsExplicitInterfaceImplementation);
            hasCode.Add(this.HasExplicitGetPropertyAccessor);
            hasCode.Add(this.HasExplicitSetPropertyAccessor);
            hasCode.Add(this.ExplicitGetterImplementationMethodDescriptor);
            hasCode.Add(this.ExplicitSetterImplementationMethodDescriptor);
            hasCode.Add(this.IsIndexerProperty);
            hasCode.Add(this.IsReadableProperty);
            hasCode.Add(this.IsWriteableProperty);
            hasCode.Add(this.IsAnonymous);
            hasCode.Add(this._propertyInfo);

            return hasCode.ToHashCode();
        }

        public static bool operator ==(WellKnownPropertyDescriptor left, WellKnownPropertyDescriptor right)
            => left.Equals(right);
        public static bool operator !=(WellKnownPropertyDescriptor left, WellKnownPropertyDescriptor right)
            => !(left == right);

        public override bool Equals(object obj)
            => obj is WellKnownPropertyDescriptor other && Equals(other);
    }

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
            MethodInfo? explicitAddImplementationMethod,
            MethodInfo? explicitRemoveImplementationMethod)
        {
            ArgumentNullException.ThrowIfNull(eventInfo);
            Type? declaringType = eventInfo.DeclaringType;
            ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);

            this.IsAttachableEvent = eventInfo.AddMethod is not null;
            if (this.IsAttachableEvent)
            {
                this.DeclaredEventAccessor = PropertyAccessor.EventAdd;
            }

            this.IsRemovableEvent = eventInfo.RemoveMethod is not null;
            if (this.IsRemovableEvent)
            {
                this.DeclaredEventAccessor = this.IsAttachableEvent
                    ? PropertyAccessor.EventAddAndEventRemove
                    : PropertyAccessor.EventRemove;
            }

            if (isExplicitInterfaceImplementation)
            {
                ArgumentExceptionAdvanced.ThrowIfFalse(declaringType is not null && declaringType.IsInterface,
                    nameof(isExplicitInterfaceImplementation),
                    $"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' returns 'true' while the declaring type represented by the argument '{nameof(eventInfo)}.{nameof(eventInfo.DeclaringType)}' is not an interface. Reason: Only interface types can provide the declaration of explicit interface implementations.");

                if (this.IsAttachableEvent && explicitAddImplementationMethod is null)
                {
                    throw new ArgumentException($"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' is set to 'true' for a readable event, but a '{nameof(explicitAddImplementationMethod)}' is not provided. Reason: All event accessors must have a matching explicit implementation.");
                }

                if (this.IsRemovableEvent && explicitRemoveImplementationMethod is null)
                {
                    throw new ArgumentException($"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' is set to 'true' for a removable event, but a '{nameof(explicitRemoveImplementationMethod)}' is not provided. Reason: All event accessors must have a matching explicit implementation.");
                }
            }
            else
            {
                if (explicitRemoveImplementationMethod is not null || explicitAddImplementationMethod is not null)
                {
                    throw new ArgumentException($"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' is set to 'false', but either a '{nameof(explicitAddImplementationMethod)}' or a '{nameof(explicitRemoveImplementationMethod)}' is provided. Reason: Accessor method descriptors can only be provided for explicit interface implementation properties.");
                }
            }

            this.HasExplicitAddEventAccessor = explicitAddImplementationMethod is not null;
            this.HasExplicitRemoveEventAccessor = explicitRemoveImplementationMethod is not null;
            this.IsExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
            this._explicitAddImplementationMethodDescriptor = this.IsAttachableEvent && isExplicitInterfaceImplementation
                ? new WellKnownMethodOrConstructorDescriptor(eventInfo.AddMethod!, PropertyAccessor.EventAdd, true)
                : default;
            this._explicitRemoveImplementationMethodDescriptor = this.IsRemovableEvent && isExplicitInterfaceImplementation
                ? new WellKnownMethodOrConstructorDescriptor(eventInfo.RemoveMethod!, PropertyAccessor.EventRemove, true)
                : default;
            this.IsAnonymous = false;
            this.EventInfo = eventInfo;
        }

        public PropertyAccessor DeclaredEventAccessor { get; }
        public bool IsExplicitInterfaceImplementation { get; init; }
        public bool HasExplicitAddEventAccessor { get; }
        public bool HasExplicitRemoveEventAccessor { get; }
        public bool IsAnonymous { get; }

        private readonly WellKnownMethodOrConstructorDescriptor _explicitAddImplementationMethodDescriptor;
        public WellKnownMethodOrConstructorDescriptor ExplicitAddImplementationMethodDescriptor
            => this.IsExplicitInterfaceImplementation && this.IsAttachableEvent
                ? this._explicitAddImplementationMethodDescriptor
                : throw new InvalidOperationException($"The property '{nameof(this.ExplicitAddImplementationMethodDescriptor)}' cannot be accessed for implicit property implementations or write-only properties.");

        private readonly WellKnownMethodOrConstructorDescriptor _explicitRemoveImplementationMethodDescriptor;
        public WellKnownMethodOrConstructorDescriptor ExplicitRemoveImplementationMethodDescriptor
            => this.IsExplicitInterfaceImplementation && this.IsRemovableEvent
                ? this._explicitRemoveImplementationMethodDescriptor
                : throw new InvalidOperationException($"The property '{nameof(this.ExplicitRemoveImplementationMethodDescriptor)}' cannot be accessed for implicit property implementations or write-only properties.");

        public EventInfo EventInfo { get; }

        public bool IsAttachableEvent { get; }
        public bool IsRemovableEvent { get; }

        public bool Equals(WellKnownEventDescriptor other)
            => this.IsExplicitInterfaceImplementation.Equals(other.IsExplicitInterfaceImplementation)
            && this.HasExplicitAddEventAccessor.Equals(other.HasExplicitAddEventAccessor)
            && this.HasExplicitRemoveEventAccessor.Equals(other.HasExplicitRemoveEventAccessor)
            && this.ExplicitAddImplementationMethodDescriptor.Equals(other.ExplicitAddImplementationMethodDescriptor)
            && this.ExplicitRemoveImplementationMethodDescriptor.Equals(other.ExplicitRemoveImplementationMethodDescriptor)
            && this.IsAnonymous == other.IsAnonymous
            && ReferenceEquals(this.EventInfo, other.EventInfo)
            && this.IsAttachableEvent == other.IsAttachableEvent
            && this.IsRemovableEvent == other.IsRemovableEvent;

        public override int GetHashCode()
        {
            var hasCode = new HashCode();
            hasCode.Add(this.IsExplicitInterfaceImplementation);
            hasCode.Add(this.HasExplicitAddEventAccessor);
            hasCode.Add(this.HasExplicitRemoveEventAccessor);
            hasCode.Add(this.ExplicitAddImplementationMethodDescriptor);
            hasCode.Add(this.ExplicitRemoveImplementationMethodDescriptor);
            hasCode.Add(this.IsAttachableEvent);
            hasCode.Add(this.IsRemovableEvent);
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
        /// If the event is an explicit interface implementation, ensure to set the <paramref name="isExplicitInterfaceImplementation"/> parameter to <see langword="true"/> and provide the <paramref name="eventInfo"/> obtained from the declaring interface type (it's crucial to provide the interface type as the declaring type).
        /// <para/>If the <see cref="System.Reflection.EventInfo"/> and the corresponding accessor methods to satisfy <paramref name="explicitAddImplementationMethodDescriptor"/> and <paramref name="explicitRemoveImplementationMethodDescriptor"/> are known create an well-known descriptor using the <see cref="WellKnownEventDescriptor"/> instead.
        /// <para/>For best accuracy and performance always use this <see cref="WellKnownEventDescriptor"/> when the caller has direct access to the <see cref="System.Reflection.EventInfo"/> representation of the event.
        /// </remarks>
        /// <param name="eventInfo">The <see cref="System.Reflection.EventInfo"/> that the descriptor represents. If <paramref name="isExplicitInterfaceImplementation"/> is set to <see langword="true"/> then the <see cref="System.Reflection.EventInfo"/> must be obtained from the declaring interface type.</param>
        /// <param name="isExplicitInterfaceImplementation"><see langword="true"/> if the event is an explicit interface implementation; otherwise, <see langword="false"/>. If set to <see langword="true"/>, the <paramref name="eventInfo"/> must be obtained from the declaring interface type.
        /// </param>
        /// <param name="explicitAddImplementationMethodDescriptor"></param>
        /// <param name="explicitRemoveImplementationMethodDescriptor"></param>
        /// <returns>A new instance of <see cref="WellKnownPropertyDescriptor"/> representing the specified anonymous or well-known property.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when
        /// <list type="bullet">
        /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="true"/> but <paramref name="eventInfo"/> was not obtained from an interface type.</item>
        /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="true"/> but neither <paramref name="explicitAddImplementationMethodDescriptor"/> (for a readable property) nor <paramref name="explicitRemoveImplementationMethodDescriptor"/> (for a writeable property) is provided.</item>
        /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="false"/> but either <paramref name="explicitAddImplementationMethodDescriptor"/> or <paramref name="explicitRemoveImplementationMethodDescriptor"/> is provided.</item>
        /// </list>
        /// </exception>
        public AnonymousEventDescriptor(RuntimeTypeHandle declaringTypeHandle,
            string eventName,
            bool isExplicitInterfaceImplementation,
            AnonymousMethodOrConstructorDescriptor? explicitAddImplementationMethodDescriptor,
            AnonymousMethodOrConstructorDescriptor? explicitRemoveImplementationMethodDescriptor)
        {
            ArgumentNullException.ThrowIfNull(eventInfo);
            Type? declaringType = eventInfo.DeclaringType;
            ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);

            this.IsAttachableEvent = eventInfo.AddMethod is not null;
            if (this.IsAttachableEvent)
            {
                this.DeclaredEventAccessor = PropertyAccessor.EventAdd;
            }

            this.IsRemovableEvent = eventInfo.RemoveMethod is not null;
            if (this.IsRemovableEvent)
            {
                this.DeclaredEventAccessor = this.IsAttachableEvent
                    ? PropertyAccessor.EventAddAndEventRemove
                    : PropertyAccessor.EventRemove;
            }

            if (isExplicitInterfaceImplementation)
            {
                ArgumentExceptionAdvanced.ThrowIfFalse(declaringType is not null && declaringType.IsInterface,
                    nameof(isExplicitInterfaceImplementation),
                    $"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' returns 'true' while the declaring type represented by the argument '{nameof(eventInfo)}.{nameof(eventInfo.DeclaringType)}' is not an interface. Reason: Only interface types can provide the declaration of explicit interface implementations.");

                if (this.IsAttachableEvent && explicitAddImplementationMethodDescriptor is null)
                {
                    throw new ArgumentException($"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' is set to 'true' for a readable event, but a '{nameof(explicitAddImplementationMethodDescriptor)}' is not provided. Reason: All event accessors must have a matching explicit implementation.");
                }

                if (this.IsRemovableEvent && explicitRemoveImplementationMethodDescriptor is null)
                {
                    throw new ArgumentException($"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' is set to 'true' for a removable event, but a '{nameof(explicitRemoveImplementationMethodDescriptor)}' is not provided. Reason: All event accessors must have a matching explicit implementation.");
                }
            }
            else
            {
                if (explicitRemoveImplementationMethodDescriptor is not null || explicitAddImplementationMethodDescriptor is not null)
                {
                    throw new ArgumentException($"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' is set to 'false', but either a '{nameof(explicitAddImplementationMethodDescriptor)}' or a '{nameof(explicitRemoveImplementationMethodDescriptor)}' is provided. Reason: Accessor method descriptors can only be provided for explicit interface implementation properties.");
                }
            }

            this.HasExplicitAddEventAccessor = explicitAddImplementationMethodDescriptor is not null;
            this.HasExplicitRemoveEventAccessor = explicitRemoveImplementationMethodDescriptor is not null;
            this.IsExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
            this._explicitAddImplementationMethodDescriptor = this.IsAttachableEvent && isExplicitInterfaceImplementation
                ? new AnonymousMethodOrConstructorDescriptor(eventInfo.AddMethod!, PropertyAccessor.EventAdd, true)
                : default;
            this._explicitRemoveImplementationMethodDescriptor = this.IsRemovableEvent && isExplicitInterfaceImplementation
                ? new AnonymousMethodOrConstructorDescriptor(eventInfo.RemoveMethod!, PropertyAccessor.EventRemove, true)
                : default;
            this.IsAnonymous = false;
            this.EventInfo = eventInfo;
        }

        public PropertyAccessor DeclaredEventAccessor { get; }
        public bool IsExplicitInterfaceImplementation { get; init; }
        public bool HasExplicitAddEventAccessor { get; }
        public bool HasExplicitRemoveEventAccessor { get; }
        public bool IsAnonymous { get; }

        private readonly AnonymousMethodOrConstructorDescriptor _explicitAddImplementationMethodDescriptor;
        public AnonymousMethodOrConstructorDescriptor ExplicitAddImplementationMethodDescriptor
            => this.IsExplicitInterfaceImplementation && this.IsAttachableEvent
                ? this._explicitAddImplementationMethodDescriptor
                : throw new InvalidOperationException($"The property '{nameof(this.ExplicitAddImplementationMethodDescriptor)}' cannot be accessed for implicit property implementations or write-only properties.");

        private readonly AnonymousMethodOrConstructorDescriptor _explicitRemoveImplementationMethodDescriptor;
        public AnonymousMethodOrConstructorDescriptor ExplicitRemoveImplementationMethodDescriptor
            => this.IsExplicitInterfaceImplementation && this.IsRemovableEvent
                ? this._explicitRemoveImplementationMethodDescriptor
                : throw new InvalidOperationException($"The property '{nameof(this.ExplicitRemoveImplementationMethodDescriptor)}' cannot be accessed for implicit property implementations or write-only properties.");

        public EventInfo EventInfo { get; }

        public bool IsAttachableEvent { get; }
        public bool IsRemovableEvent { get; }

        public bool Equals(AnonymousEventDescriptor other)
            => this.IsExplicitInterfaceImplementation.Equals(other.IsExplicitInterfaceImplementation)
            && this.HasExplicitAddEventAccessor.Equals(other.HasExplicitAddEventAccessor)
            && this.HasExplicitRemoveEventAccessor.Equals(other.HasExplicitRemoveEventAccessor)
            && this.ExplicitAddImplementationMethodDescriptor.Equals(other.ExplicitAddImplementationMethodDescriptor)
            && this.ExplicitRemoveImplementationMethodDescriptor.Equals(other.ExplicitRemoveImplementationMethodDescriptor)
            && this.IsAnonymous == other.IsAnonymous
            && ReferenceEquals(this.EventInfo, other.EventInfo)
            && this.IsAttachableEvent == other.IsAttachableEvent
            && this.IsRemovableEvent == other.IsRemovableEvent;

        public override int GetHashCode()
        {
            var hasCode = new HashCode();
            hasCode.Add(this.IsExplicitInterfaceImplementation);
            hasCode.Add(this.HasExplicitAddEventAccessor);
            hasCode.Add(this.HasExplicitRemoveEventAccessor);
            hasCode.Add(this.ExplicitAddImplementationMethodDescriptor);
            hasCode.Add(this.ExplicitRemoveImplementationMethodDescriptor);
            hasCode.Add(this.IsAttachableEvent);
            hasCode.Add(this.IsRemovableEvent);
            hasCode.Add(this.IsAnonymous);
            hasCode.Add(this.EventInfo);

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
