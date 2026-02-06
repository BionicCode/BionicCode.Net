namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Reflection;

internal sealed class EventData : MemberData
{
    private string? _displayName;
    private string? _shortDisplayName;
    private string? _fullyQualifiedDisplayName;
    private string? _signature;
    private string? _shortSignature;
    private string? _shortCompactSignature;
    private string? _fullyQualifiedSignature;
    private string? _fullyQualifiedRuntimeSignature;
    private string? _runtimeSignature;
    private string? _runtimeShortSignature;
    private string? _runtimeShortCompactSignature;
    private SymbolAttributes _symbolAttributes;
    private bool? _isOverride;
    private MethodData? _addMethodData;
    private MethodData? _removeMethodData;
    private MethodData? _invocatorMethodData;
    private AccessModifier _accessModifier;
    private bool? _isStatic;
    private bool? _isPublic;
    private bool? _isPrivate;
    private bool? _isAssembly;
    private bool? _isFamily;
    private bool? _isFamilyOrAssembly;
    private bool? _isFamilyAndAssembly;
    private bool? _canAdd;
    private bool? _canRemove;
    private TypeData? _eventHandlerTypeData;
    private string? _assemblyName;
    private SymbolComponentInfo? _symbolComponentInfo;
    private EventInfo _eventInfo;
    private readonly WellKnownEventDescriptor _descriptor;

    internal EventData(SymbolReflectionInfoCacheKey symbolInfoDataCacheKey)
        : base(symbolInfoDataCacheKey.EventDescriptor.EventName, SymbolKind.MemberEvent, symbolInfoDataCacheKey)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(symbolInfoDataCacheKey.EventDescriptor, nameof(symbolInfoDataCacheKey));

        _descriptor = symbolInfoDataCacheKey.EventDescriptor;
        EventInfo = symbolInfoDataCacheKey.EventDescriptor.EventInfo;
        IsExplicitInterfaceImplementation = _descriptor.IsExplicitInterfaceImplementation;
        DeclaringTypeHandle = _descriptor.DeclaringTypeHandle;
        ImplementingTypeHandle = _descriptor.ImplementingTypeHandle;
    }

    protected override MemberInfo GetMemberInfo()
      => EventInfo;

    internal object? RaiseEvent(object? target, params object?[]? arguments)
    {
        ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
        ThrowIfInvalidMethodArguments(arguments);

        return EventInvokerMethodData.Invoke(target, arguments);
    }

    private void ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(object? target)
    {
        if (!IsStatic)
        {
            ArgumentNullException.ThrowIfNull(target, nameof(target));

            Type targetType = target.GetType();
            Type declaringType = DeclaringTypeData.UnwrapType();
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
        if (EventInvokerMethodData.Parameters.HasItems)
        {
            if (EventInvokerMethodData.HasParamsParameter)
            {
                // NULL is valid for 'args' if there is only a single non-params parameter since params can be empty.
                // Additionally, no need to check 'args' for NULL if there is only the params parameter.
                // However, NULL is not valid for 'args' if there are more than a single non-params parameters.
                if (EventInvokerMethodData.Parameters.Count > 2)
                {
                    ArgumentNullException.ThrowIfNull(args, nameof(args));

                    // Insufficient number of arguments provided for method invocation with 'params' parameter.
                    // For a params method parameter, providing no arguments for it is valid, hence the -1 check.
                    ArgumentOutOfRangeException.ThrowIfLessThan(args.Length, EventInvokerMethodData.Parameters.Count - 1, nameof(args));
                }
            }
            else
            {
                // NULL is valid for 'args' if there is only a single non-params parameter.
                // However, NULL is not valid for 'args' if there are more than a single non-params parameters.
                if (EventInvokerMethodData.Parameters.Count > 1)
                {
                    ArgumentNullException.ThrowIfNull(args, nameof(args));
                }

                if (args is not null)
                {
                    ArgumentOutOfRangeException.ThrowIfNotEqual(args.Length, EventInvokerMethodData.Parameters.Count, nameof(args));
                }
            }
        }
        else if (args is not null && args.Length > 0) // Method has no parameters but arguments were provided.
        {
            throw new ArgumentException("Method has no parameters but arguments were provided.", nameof(args));
        }
    }

    /// <inheritdoc/>
    internal override AccessModifier AccessModifier => _accessModifier is AccessModifier.Undefined
      ? (_accessModifier = EventData.GetAccessModifierInternal(this))
      : _accessModifier;

    internal void AddEventHandler(object eventSource, Delegate handler)
      => AddMethodData.Invoke(eventSource, handler);

    internal void AddEventHandler<TEventSource>(TEventSource eventSource, Delegate handler)
      => AddMethodData.Invoke(eventSource, handler);

    internal void RemoveEventHandler(object eventSource, Delegate handler)
      => RemoveMethodData.Invoke(eventSource, handler);

    internal void RemoveEventHandler<TEventSource>(TEventSource eventSource, Delegate handler)
      => RemoveMethodData.Invoke(eventSource, handler);

    internal MethodData AddMethodData
      => _addMethodData ??= IsExplicitInterfaceImplementation
            ? MethodInfo.GetMethodFromHandle(_descriptor.AddAccessorImplementationMethodHandle) is MethodInfo explicitImplementationAccessor
                ? SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(explicitImplementationAccessor)
                : throw new NotSupportedException($"The underlying '{typeof(EventInfo).FullName}' for event '{EventInfo.Name}' does not have an add method.")
            : EventInfo.GetAddMethod() is MethodInfo methodInfo
                ? SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo)
                : throw new NotSupportedException($"The underlying '{typeof(EventInfo).FullName}' for event '{EventInfo.Name}' does not have an add method.");

    internal MethodData RemoveMethodData
      => _removeMethodData ??= IsExplicitInterfaceImplementation
            ? MethodInfo.GetMethodFromHandle(_descriptor.RemoveAccessorImplementationMethodHandle) is MethodInfo explicitImplementationAccessor
                ? SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(explicitImplementationAccessor)
                : throw new NotSupportedException($"The underlying '{typeof(EventInfo).FullName}' for event '{EventInfo.Name}' does not have a remove method.")
            : EventInfo.GetRemoveMethod() is MethodInfo methodInfo
                ? SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo)
                : throw new NotSupportedException($"The underlying '{typeof(EventInfo).FullName}' for event '{EventInfo.Name}' does not have a remove method.");

    internal MethodData EventInvokerMethodData
      => _invocatorMethodData ??= EventHandlerTypeData?.DelegateInvokeMethodData!;

    internal TypeData EventHandlerTypeData
      => _eventHandlerTypeData ??= EventInfo.EventHandlerType is Type eventHandlerType
        ? SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventHandlerType)
        : throw new NotSupportedException($"The underlying '{typeof(EventInfo).FullName}' for event '{EventInfo.Name}' does not have an event handler type.");

    internal EventInfo EventInfo
    {
        get
        {
            ExceptionThrower.ThrowIfDisposed(this);

            return base.IsDisposed ? throw new ObjectDisposedException(nameof(EventData)) : _eventInfo;
        }

        private set => _eventInfo = value;
    }

    /// <inheritdoc/>
    internal override bool IsExplicitInterfaceImplementation { get; }

    /// <inheritdoc/>
    internal override RuntimeTypeHandle DeclaringTypeHandle { get; }
    /// <inheritdoc/>
    internal override RuntimeTypeHandle ImplementingTypeHandle { get; }

    /// <inheritdoc/>
    internal override bool IsStatic
      => _isStatic ??= AddMethodData?.IsStatic ?? false;

    /// <inheritdoc/>
    internal override SymbolAttributes SymbolAttributes => _symbolAttributes is SymbolAttributes.Undefined
      ? (_symbolAttributes = EventData.GetAttributesInternal(this))
      : _symbolAttributes;

    /// <inheritdoc/>
    internal override SymbolComponentInfo SymbolComponentInfo
      => _symbolComponentInfo ??= SymbolSignatureGenerator.ToSignatureComponentsInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false);

    /// <inheritdoc/>
    internal override string Signature
      => _signature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

    /// <inheritdoc/>
    internal override string ShortSignature
      => _shortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: false);

    /// <inheritdoc/>
    internal override string ShortCompactSignature
      => _shortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: false);

    /// <inheritdoc/>
    internal override string FullyQualifiedSignature
      => _fullyQualifiedSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

    /// <inheritdoc/>
    internal override string FullyQualifiedRuntimeSignature
      => _fullyQualifiedRuntimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

    /// <inheritdoc/>
    internal override string RuntimeSignature
      => _runtimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

    /// <inheritdoc/>
    internal override string RuntimeShortSignature
      => _runtimeShortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: true);

    /// <inheritdoc/>
    internal override string RuntimeShortCompactSignature
      => _runtimeShortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: true);

    /// <inheritdoc/>
    internal override string DisplayName
      => _displayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

    /// <inheritdoc/>
    internal override string ShortDisplayName
      => _shortDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: false);

    /// <inheritdoc/>
    internal override string FullyQualifiedDisplayName
      => _fullyQualifiedDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: true, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

    /// <inheritdoc/>
    internal override string AssemblyName
      => _assemblyName ??= DeclaringTypeData.AssemblyName;

    internal bool CanAdd
      => _canAdd ??= _addMethodData is not null || (_addMethodData = EventInfo.GetAddMethod(true)?.ToMethodData()) is not null;

    internal bool CanRemove
        => _canRemove ??= _removeMethodData is not null || (_removeMethodData = EventInfo.GetRemoveMethod(true)?.ToMethodData()) is not null;

    internal bool IsOverride
      => _isOverride ??= AddMethodData!.IsOverride;

    /// <inheritdoc/>
    internal override bool IsPublic
        => _isPublic ??= AccessModifier == AccessModifier.Public;

    /// <inheritdoc/>
    internal override bool IsPrivate
        => _isPrivate ??= AccessModifier == AccessModifier.Private;

    /// <inheritdoc/>
    internal override bool IsAssembly
        => _isAssembly ??= AccessModifier == AccessModifier.Internal;

    /// <inheritdoc/>
    internal override bool IsFamily
        => _isFamily ??= AccessModifier == AccessModifier.Protected;

    /// <inheritdoc/>
    internal override bool IsFamilyOrAssembly
        => _isFamilyOrAssembly ??= AccessModifier == AccessModifier.ProtectedInternal;

    /// <inheritdoc/>
    internal override bool IsFamilyAndAssembly
        => _isFamilyAndAssembly ??= AccessModifier == AccessModifier.PrivateProtected;

    /// <summary>
    /// Determines the set of symbol attributes for the specified event based on its add method characteristics.
    /// </summary>
    /// <remarks>For performance reasons avoid querying the attributes and prefer reading the particular property or properties.</remarks>
    /// <param name="eventData">The event metadata used to evaluate and derive the corresponding symbol attributes.</param>
    /// <returns>A bitwise combination of SymbolAttributes values that represent the attributes of the event, such as Final,
    /// Abstract, Static, Virtual, or Override.</returns>
    private static SymbolAttributes GetAttributesInternal(EventData eventData)
    {
        ArgumentNullException.ThrowIfNull(eventData);

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
      => eventData is EventData checkedEventData
        ? checkedEventData.AddMethodData?.AccessModifier ?? AccessModifier.Undefined
        : throw new ArgumentNullException(nameof(eventData));

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        EventInfo = null!;
    }
}
