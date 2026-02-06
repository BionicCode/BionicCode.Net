namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Reflection;

internal abstract class ParameterizedMemberData : MemberData
{
    private bool? _isStatic;
    private bool? _isFinal;
    private bool? _isAbstract;
    private bool? _isVirtual;
    private bool? _isPublic;
    private bool? _isPrivate;
    private bool? _isAssembly;
    private bool? _isFamily;
    private bool? _isFamilyOrAssembly;
    private bool? _isFamilyAndAssembly;
    private bool? _isConstructor;
    private bool? _isMethod;
    private bool? _isSpecialName;

    protected ParameterizedMemberData(SymbolReflectionInfoCacheKey symbolReflectionInfoCacheKey)
        : base(symbolReflectionInfoCacheKey.SymbolName, symbolReflectionInfoCacheKey.SymbolKind, symbolReflectionInfoCacheKey)
        => ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(symbolReflectionInfoCacheKey.SymbolKind, [SymbolKind.MemberMethod, SymbolKind.MemberConstructor], nameof(symbolReflectionInfoCacheKey));

    internal abstract ParameterList Parameters { get; }
    internal abstract bool HasParamsParameter { get; }
    internal abstract RuntimeMethodHandle Handle { get; }
    internal abstract ParameterizedSymbolKind ParameterizedSymbolKind { get; }
    internal abstract MethodBase GetMethodBase();

    /// <summary>
    /// Gets a value indicating whether this method has a special name.
    /// </summary>
    /// <remarks>
    /// Methods with special names (IsSpecialName = true) are typically compiler-generated. 
    /// 
    /// Common special name methods: 
    /// ┌─────────────────┬──────────────────────────────────────────┬─────────────────────────┐
    /// │ Category        │ Method Names                             │ Example                 │
    /// ├─────────────────┼──────────────────────────────────────────┼─────────────────────────┤
    /// │ Properties      │ get_PropertyName, set_PropertyName       │ get_Count, set_Value    │
    /// │ Indexers        │ get_Item, set_Item                       │ get_Item(int)           │
    /// │ Events          │ add_EventName, remove_EventName,         │ add_Click,              │
    /// │                 │ raise_EventName                          │ remove_Click            │
    /// │ Operators       │ op_Addition, op_Subtraction,             │ op_Addition,            │
    /// │                 │ op_Equality, op_Implicit, op_Explicit    │ op_Equality             │
    /// │ Constructors    │ .ctor, .cctor                            │ .ctor(), .cctor()       │
    /// │ Destructors     │ Finalize                                 │ Finalize()              │
    /// │ Delegates       │ Invoke, BeginInvoke, EndInvoke           │ Invoke(int)             │
    /// └─────────────────┴──────────────────────────────────────────┴─────────────────────────┘
    /// </remarks>
    internal bool IsSpecializedName
        => ((MethodBase)GetMethodBase()).IsSpecialName;

    internal bool IsAbstract
      => _isAbstract ??= GetMethodBase().IsAbstract;

    internal bool IsVirtual
        => _isVirtual ??= GetMethodBase().IsVirtual;

    internal override bool IsStatic
      => _isStatic ??= GetMethodBase().IsStatic;

    internal bool IsSealed
      => IsFinal;

    internal bool IsFinal
      => _isFinal ??= GetMethodBase().IsFinal;

    internal override bool IsPublic
        => _isPublic ??= GetMethodBase().IsPublic;

    internal override bool IsPrivate
        => _isPrivate ??= GetMethodBase().IsPrivate;

    internal override bool IsAssembly
        => _isAssembly ??= GetMethodBase().IsAssembly;

    internal override bool IsFamily
        => _isFamily ??= GetMethodBase().IsFamily;

    internal override bool IsFamilyOrAssembly
        => _isFamilyOrAssembly ??= GetMethodBase().IsFamilyOrAssembly;

    internal override bool IsFamilyAndAssembly
        => _isFamilyAndAssembly ??= GetMethodBase().IsFamilyAndAssembly;

    internal bool IsConstructor
        => _isConstructor ??= GetMethodBase() is ConstructorInfo;

    internal bool IsMethod
        => _isMethod ??= GetMethodBase() is MethodInfo;

    internal bool IsSpecialName
        => _isSpecialName ??= GetMethodBase().IsSpecialName;
}
