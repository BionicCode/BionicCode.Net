namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Reflection;

public interface IParameterizedMemberDataView
{
    RuntimeMethodHandle Handle { get; }
    bool HasParamsParameter { get; }
    bool IsAbstract { get; }
    bool IsAssembly { get; }
    bool IsConstructor { get; }
    bool IsFinal { get; }
    bool IsMethod { get; }
    bool IsPublic { get; }
    bool IsSealed { get; }
    bool IsSpecializedName { get; }
    bool IsSpecialName { get; }
    bool IsVirtual { get; }
    ParameterizedSymbolKind ParameterizedSymbolKind { get; }
    ParameterListView Parameters { get; }

    MethodBase GetMethodBase();
}