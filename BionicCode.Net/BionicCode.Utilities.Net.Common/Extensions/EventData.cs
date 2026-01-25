namespace BionicCode.Utilities.Net
{
    using System;
    using System.Reflection;

    internal sealed class EventData : MemberData
    {
        private string? displayName;
        private string? shortDisplayName;
        private string? fullyQualifiedDisplayName;
        private string? signature;
        private string? shortSignature;
        private string? shortCompactSignature;
        private string? fullyQualifiedSignature;
        private string? fullyQualifiedRuntimeSignature;
        private string? runtimeSignature;
        private string? runtimeShortSignature;
        private string? runtimeShortCompactSignature;
        private SymbolAttributes symbolAttributes;
        private bool? isOverride;
        private MethodData? addMethodData;
        private MethodData? removeMethodData;
        private MethodData? invocatorMethodData;
        private AccessModifier accessModifier;
        private bool? isStatic;
        private bool? _isPublic;
        private bool? _isPrivate;
        private bool? _isAssembly;
        private bool? _isFamily;
        private bool? _isFamilyOrAssembly;
        private bool? _isFamilyAndAssembly;
        private bool? _canAdd;
        private bool? _canRemove;
        private TypeData? eventHandlerTypeData;
        private string? assemblyName;
        private SymbolComponentInfo? symbolComponentInfo;

        internal EventData(EventInfo eventInfo, SymbolInfoDataCacheKey symbolInfoDataCacheKey)
            : base(eventInfo, SymbolKind.MemberEvent, symbolInfoDataCacheKey)
        {
            ArgumentNullException.ThrowIfNull(eventInfo, nameof(eventInfo));

            this.EventInfo = eventInfo;
        }

        public EventInfo GetEventInfo()
          => this.EventInfo;

        protected override MemberInfo GetMemberInfo()
          => GetEventInfo();

        public object? RaiseEvent(object? target, params object?[]? arguments)
        {
            ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
            ThrowIfInvalidMethodArguments(arguments);

            return this.EventInvokerMethodData.Invoke(target, arguments);
        }

        private void ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(object? target)
        {
            if (!this.IsStatic)
            {
                ArgumentNullException.ThrowIfNull(target, nameof(target));

                Type targetType = target.GetType();
                Type declaringType = this.DeclaringTypeData.UnwrapType();
                ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                    targetType,
                    declaringType,
                    nameof(target),
                    ExceptionMessages.GetTypeMismatchExceptionMessage(
                            targetType,
                            nameof(target),
                            declaringType,
                            "declaring type"));
            }
        }

        private void ThrowIfInvalidMethodArguments(object?[]? args)
        {
            // Validate arguments against method parameters
            if (this.EventInvokerMethodData.Parameters.HasItems)
            {
                if (this.EventInvokerMethodData.HasParamsParameter)
                {
                    // NULL is valid for 'args' if there is only a single non-params parameter since params can be empty.
                    // Additionally, no need to check 'args' for NULL if there is only the params parameter.
                    // However, NULL is not valid for 'args' if there are more than a single non-params parameters.
                    if (this.EventInvokerMethodData.Parameters.Count > 2)
                    {
                        ArgumentNullException.ThrowIfNull(args, nameof(args));

                        // Insufficient number of arguments provided for method invocation with 'params' parameter.
                        // For a params method parameter, providing no arguments for it is valid, hence the -1 check.
                        ArgumentOutOfRangeException.ThrowIfLessThan(args.Length, this.EventInvokerMethodData.Parameters.Count - 1, nameof(args));
                    }
                }
                else
                {
                    // NULL is valid for 'args' if there is only a single non-params parameter.
                    // However, NULL is not valid for 'args' if there are more than a single non-params parameters.
                    if (this.EventInvokerMethodData.Parameters.Count > 1)
                    {
                        ArgumentNullException.ThrowIfNull(args, nameof(args));
                    }

                    if (args is not null)
                    {
                        ArgumentOutOfRangeException.ThrowIfNotEqual(args.Length, this.EventInvokerMethodData.Parameters.Count, nameof(args));
                    }
                }
            }
            else if (args is not null && args.Length > 0) // Method has no parameters but arguments were provided.
            {
                throw new ArgumentException("Method has no parameters but arguments were provided.", nameof(args));
            }
        }

        public override AccessModifier AccessModifier => this.accessModifier is AccessModifier.Undefined
          ? (this.accessModifier = EventData.GetAccessModifierInternal(this))
          : this.accessModifier;

        public void AddEventHandler(object eventSource, Delegate handler)
          => GetEventInfo().AddEventHandler(eventSource, handler);

        public void RemoveEventHandler(object eventSource, Delegate handler)
          => GetEventInfo().RemoveEventHandler(eventSource, handler);

        public MethodData AddMethodData
          => this.addMethodData ??= GetEventInfo().GetAddMethod(true) is MethodInfo addMethod
            ? SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(addMethod)
            : throw new NotSupportedException($"The underlying '{typeof(EventInfo).FullName}' for event '{GetEventInfo().Name}' does not have an add method.");

        public MethodData RemoveMethodData
          => this.removeMethodData ??= GetEventInfo().GetRemoveMethod(true) is MethodInfo removeMethod
            ? SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(removeMethod)
            : throw new NotSupportedException($"The underlying '{typeof(EventInfo).FullName}' for event '{GetEventInfo().Name}' does not have a remove method.");

        public MethodData EventInvokerMethodData
          => this.invocatorMethodData ??= this.EventHandlerTypeData?.GetMethod(ReflectionConstants.DelegateInvocatorMethodName, ReadOnlySpan<TypeData>.Empty, ReadOnlySpan<MethodParameterInfo>.Empty)!;

        public TypeData EventHandlerTypeData
          => this.eventHandlerTypeData ??= GetEventInfo().EventHandlerType is Type eventHandlerType
            ? SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventHandlerType)
            : throw new NotSupportedException($"The underlying '{typeof(EventInfo).FullName}' for event '{GetEventInfo().Name}' does not have an event handler type.");

        public EventInfo EventInfo { get; }

        public override bool IsStatic
          => this.isStatic ??= this.AddMethodData?.IsStatic ?? false;

        public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
          ? (this.symbolAttributes = EventData.GetAttributesInternal(this))
          : this.symbolAttributes;

        public override SymbolComponentInfo SymbolComponentInfo
          => this.symbolComponentInfo ??= SymbolSignatureGenerator.ToSignatureComponentsInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false);

        public override string Signature
          => this.signature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

        public override string ShortSignature
          => this.shortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: false);

        public override string ShortCompactSignature
          => this.shortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: false);

        public override string FullyQualifiedSignature
          => this.fullyQualifiedSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

        public override string FullyQualifiedRuntimeSignature
          => this.fullyQualifiedRuntimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

        public override string RuntimeSignature
          => this.runtimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

        public override string RuntimeShortSignature
          => this.runtimeShortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: true);

        public override string RuntimeShortCompactSignature
          => this.runtimeShortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: true);

        public override string DisplayName
          => this.displayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

        public override string ShortDisplayName
          => this.shortDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: false);

        public override string FullyQualifiedDisplayName
          => this.fullyQualifiedDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: true, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

        public override string AssemblyName
          => this.assemblyName ??= this.DeclaringTypeData.AssemblyName;

        public bool CanAdd
          => this._canAdd ??= GetEventInfo().GetAddMethod(true) is not null;

        public bool CanRemove
            => this._canRemove ??= GetEventInfo().GetRemoveMethod(true) is not null;

        public bool IsOverride
          => this.isOverride ??= this.AddMethodData!.IsOverride;

        public override bool IsPublic
            => this._isPublic ??= this.AccessModifier == AccessModifier.Public;

        public override bool IsPrivate
            => this._isPrivate ??= this.AccessModifier == AccessModifier.Private;

        /// <summary>
        /// Gets a value indicating whether the member has internal accessibility within its assembly.
        /// </summary>
        public override bool IsAssembly
            => this._isAssembly ??= this.AccessModifier == AccessModifier.Internal;

        /// <summary>
        /// Gets a value indicating whether the member is protected and thus accessible only within its own class or by
        /// derived class instances.
        /// </summary>
        public override bool IsFamily
            => this._isFamily ??= this.AccessModifier == AccessModifier.Protected;

        public override bool IsFamilyOrAssembly
            => this._isFamilyOrAssembly ??= this.AccessModifier == AccessModifier.ProtectedInternal;

        public override bool IsFamilyAndAssembly
            => this._isFamilyAndAssembly ??= this.AccessModifier == AccessModifier.PrivateProtected;

        /// <summary>
        /// Determines the set of symbol attributes for the specified event based on its add method characteristics.
        /// </summary>
        /// <remarks>For performance reasons avoid querying the attributes and prefer reading the particular property or properties.</remarks>
        /// <param name="eventData">The event metadata used to evaluate and derive the corresponding symbol attributes.</param>
        /// <returns>A bitwise combination of SymbolAttributes values that represent the attributes of the event, such as Final,
        /// Abstract, Static, Virtual, or Override.</returns>
        private static SymbolAttributes GetAttributesInternal(EventData eventData)
        {
            MethodData? eventAddMethodData = eventData.AddMethodData;
            SymbolAttributes eventAttributes = SymbolAttributes.Event;
            if (eventAddMethodData?.IsSealed ?? false)
            {
                eventAttributes |= SymbolAttributes.Final;
            }

            if (eventAddMethodData?.IsAbstract ?? false)
            {
                eventAttributes |= SymbolAttributes.Abstract;
            }

            if (eventAddMethodData?.IsStatic ?? false)
            {
                eventAttributes |= SymbolAttributes.Static;
            }

            if (eventAddMethodData?.IsVirtual ?? false)
            {
                eventAttributes |= SymbolAttributes.Virtual;
            }

            if (eventAddMethodData?.IsOverride ?? false)
            {
                eventAttributes |= SymbolAttributes.Override;
            }

            return eventAttributes;
        }

        private static AccessModifier GetAccessModifierInternal(EventData eventData)
          => eventData.AddMethodData?.AccessModifier ?? AccessModifier.Undefined;
    }
}
