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

    public abstract ParameterList Parameters { get; }
    public abstract bool HasParamsParameter { get; }
    public abstract RuntimeMethodHandle Handle { get; }
    public abstract ParameterizedSymbolKind ParameterizedSymbolKind { get; }
    public abstract MethodBase GetMethodBase();

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
    public bool IsSpecializedName => ((MethodBase)GetMethodBase()).IsSpecialName;

    public bool IsAbstract => _isAbstract ??= GetMethodBase().IsAbstract;

    public bool IsVirtual => _isVirtual ??= GetMethodBase().IsVirtual;

    public override bool IsStatic => _isStatic ??= GetMethodBase().IsStatic;

    public bool IsSealed => IsFinal;

    public bool IsFinal => _isFinal ??= GetMethodBase().IsFinal;

    public override bool IsPublic => _isPublic ??= GetMethodBase().IsPublic;

    public override bool IsPrivate => _isPrivate ??= GetMethodBase().IsPrivate;

    public override bool IsAssembly => _isAssembly ??= GetMethodBase().IsAssembly;

    public override bool IsFamily => _isFamily ??= GetMethodBase().IsFamily;

    public override bool IsFamilyOrAssembly => _isFamilyOrAssembly ??= GetMethodBase().IsFamilyOrAssembly;

    public override bool IsFamilyAndAssembly => _isFamilyAndAssembly ??= GetMethodBase().IsFamilyAndAssembly;

    public bool IsConstructor => _isConstructor ??= GetMethodBase() is ConstructorInfo;

    public bool IsMethod => _isMethod ??= GetMethodBase() is MethodInfo;

    public bool IsSpecialName => _isSpecialName ??= GetMethodBase().IsSpecialName;
}
