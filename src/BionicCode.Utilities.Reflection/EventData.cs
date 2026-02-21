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
    private readonly WellKnownEventDescriptor _descriptor;
    private RuntimeTypeHandle? _declaringTypeHandle;
    private RuntimeTypeHandle? _implementingTypeHandle;
    private bool? _isExplicitInterfaceImplementation;
    private IEventDataView? _eventDataView;

    internal EventData(WellKnownEventDescriptor descriptor)
        : base(descriptor.EventName, SymbolKind.MemberEvent)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(descriptor, nameof(descriptor));

        _descriptor = descriptor;
        EventInfo = _descriptor.EventInfo;
    }

    internal new IEventDataView View => _eventDataView ??= new EventDataView(GetPublicCacheKey());

    protected override MemberInfo MemberInfo => EventInfo;

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
            Type declaringType = DeclaringTypeData.Type;
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

    internal void AddEventHandler(object eventSource, Delegate handler) => AddMethodData.Invoke(eventSource, handler);

    internal void AddEventHandler<TEventSource>(TEventSource eventSource, Delegate handler) => AddMethodData.Invoke(eventSource, handler);

    internal void RemoveEventHandler(object eventSource, Delegate handler) => RemoveMethodData.Invoke(eventSource, handler);

    internal void RemoveEventHandler<TEventSource>(TEventSource eventSource, Delegate handler) => RemoveMethodData.Invoke(eventSource, handler);

    internal MethodData AddMethodData => _addMethodData ??= EventInfo.GetAddMethod() is MethodInfo methodInfo
        ? GetOrCreateCacheEntry(methodInfo)
        : throw new NotSupportedException($"The underlying '{typeof(EventInfo).FullName}' for event '{EventInfo.Name}' does not have an add method.");

    internal MethodData RemoveMethodData => EventInfo.GetRemoveMethod() is MethodInfo methodInfo
        ? GetOrCreateCacheEntry(methodInfo)
        : throw new NotSupportedException($"The underlying '{typeof(EventInfo).FullName}' for event '{EventInfo.Name}' does not have a remove method.");

    internal MethodData EventInvokerMethodData => _invocatorMethodData ??= EventHandlerTypeData?.DelegateInvokeMethodData!;

    internal TypeData EventHandlerTypeData => _eventHandlerTypeData ??= EventInfo.EventHandlerType is Type eventHandlerType
        ? GetOrCreateCacheEntry(eventHandlerType)
        : throw new NotSupportedException($"The underlying '{typeof(EventInfo).FullName}' for event '{EventInfo.Name}' does not have an event handler type.");

    internal EventInfo EventInfo { get; }

    /// <inheritdoc/>
    internal override bool IsExplicitInterfaceImplementation => _isExplicitInterfaceImplementation ??= IsExplicitImplementation(this);

    /// <inheritdoc/>
    internal override RuntimeTypeHandle DeclaringTypeHandle => _declaringTypeHandle ??= CanAdd
        ? AddMethodData.DeclaringTypeHandle
        : RemoveMethodData.DeclaringTypeHandle;

    /// <inheritdoc/>
    internal override RuntimeTypeHandle ImplementingTypeHandle => _implementingTypeHandle ??= CanAdd
        ? AddMethodData.ImplementingTypeHandle
        : RemoveMethodData.ImplementingTypeHandle;

    /// <inheritdoc/>
    internal override bool IsStatic => _isStatic ??= AddMethodData?.IsStatic ?? false;

    /// <inheritdoc/>
    internal override SymbolAttributes SymbolAttributes => _symbolAttributes is SymbolAttributes.Undefined
        ? (_symbolAttributes = EventData.GetAttributesInternal(this))
        : _symbolAttributes;

    /// <inheritdoc/>
    internal override SymbolComponentInfo SymbolComponentInfo => _symbolComponentInfo ??= SymbolSignatureGenerator.ToSignatureComponentsInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false);

    /// <inheritdoc/>
    internal override string Signature => _signature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

    /// <inheritdoc/>
    internal override string ShortSignature => _shortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: false);

    /// <inheritdoc/>
    internal override string ShortCompactSignature => _shortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: false);

    /// <inheritdoc/>
    internal override string FullyQualifiedSignature => _fullyQualifiedSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

    /// <inheritdoc/>
    internal override string FullyQualifiedRuntimeSignature => _fullyQualifiedRuntimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

    /// <inheritdoc/>
    internal override string RuntimeSignature => _runtimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

    /// <inheritdoc/>
    internal override string RuntimeShortSignature => _runtimeShortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: true);

    /// <inheritdoc/>
    internal override string RuntimeShortCompactSignature => _runtimeShortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: true);

    /// <inheritdoc/>
    internal override string DisplayName => _displayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

    /// <inheritdoc/>
    internal override string ShortDisplayName => _shortDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: false);

    /// <inheritdoc/>
    internal override string FullyQualifiedDisplayName => _fullyQualifiedDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: true, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

    /// <inheritdoc/>
    internal override string AssemblyName => _assemblyName ??= DeclaringTypeData.AssemblyName;

    internal bool CanAdd => _canAdd ??= _addMethodData is not null || (EventInfo.GetAddMethod(true) is MethodInfo addMethodInfo && (_addMethodData = GetOrCreateCacheEntry(addMethodInfo)) is not null);

    internal bool CanRemove => _canRemove ??= _removeMethodData is not null || (EventInfo.GetRemoveMethod(true) is MethodInfo removeMethodInfo && (_removeMethodData = GetOrCreateCacheEntry(removeMethodInfo)) is not null);

    internal bool IsOverride => _isOverride ??= AddMethodData!.IsOverride;

    /// <inheritdoc/>
    internal override bool IsPublic => _isPublic ??= AccessModifier == AccessModifier.Public;

    /// <inheritdoc/>
    internal override bool IsPrivate => _isPrivate ??= AccessModifier == AccessModifier.Private;

    /// <inheritdoc/>
    internal override bool IsAssembly => _isAssembly ??= AccessModifier == AccessModifier.Internal;

    /// <inheritdoc/>
    internal override bool IsFamily => _isFamily ??= AccessModifier == AccessModifier.Protected;

    /// <inheritdoc/>
    internal override bool IsFamilyOrAssembly => _isFamilyOrAssembly ??= AccessModifier == AccessModifier.ProtectedInternal;

    /// <inheritdoc/>
    internal override bool IsFamilyAndAssembly => _isFamilyAndAssembly ??= AccessModifier == AccessModifier.PrivateProtected;

    private static bool IsExplicitImplementation(EventData eventData) => eventData.CanAdd
        ? eventData.AddMethodData.IsExplicitInterfaceImplementation
        : eventData.RemoveMethodData.IsExplicitInterfaceImplementation;

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

    private static AccessModifier GetAccessModifierInternal(EventData eventData) => eventData is EventData checkedEventData
        ? checkedEventData.AddMethodData?.AccessModifier ?? AccessModifier.Undefined
        : throw new ArgumentNullException(nameof(eventData));
}
