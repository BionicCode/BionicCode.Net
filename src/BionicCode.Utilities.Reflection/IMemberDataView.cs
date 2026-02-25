namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Reflection;
using BionicCode.Utilities.Net;

public interface IMemberDataView : ISymbolInfoDataView
{
    AccessModifier AccessModifier { get; }
    BindingFlags BindingFlagsVisibilityMask { get; }
    ITypeDataView DeclaringType { get; }
    RuntimeTypeHandle DeclaringTypeHandle { get; }
    RuntimeTypeHandle ImplementingTypeHandle { get; }
    ITypeDataView ImplementingType { get; }
    bool IsAssembly { get; }
    bool IsExplicitInterfaceImplementation { get; }
    bool IsFamily { get; }
    bool IsFamilyAndAssembly { get; }
    bool IsFamilyOrAssembly { get; }
    bool IsPrivate { get; }
    bool IsPublic { get; }
    bool IsStatic { get; }
}
