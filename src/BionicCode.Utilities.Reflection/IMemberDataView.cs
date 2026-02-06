namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Reflection;
using BionicCode.Utilities.Net;

public interface IMemberDataView : ISymbolInfoDataView
{
    AccessModifier AccessModifier { get; }
    new IList<CustomAttributeData> AttributeData { get; }
    BindingFlags BindingFlagsVisibilityMask { get; }
    RuntimeTypeHandle DeclaringInterfaceHandle { get; }
    RuntimeTypeHandle DeclaringTypeHandle { get; }
    RuntimeTypeHandle ImplementingTypeHandle { get; }
    bool IsAssembly { get; }
    bool IsExplicitInterfaceImplementation { get; }
    bool IsFamily { get; }
    bool IsFamilyAndAssembly { get; }
    bool IsFamilyOrAssembly { get; }
    bool IsPrivate { get; }
    bool IsPublic { get; }
    bool IsStatic { get; }
}