namespace BionicCode.Utilities.Net.Reflection;

using System;

public interface IParameterizedMemberDataView : ISymbolInfoDataView, IMemberDataView
{
    RuntimeMethodHandle Handle { get; }
    bool HasParamsParameter { get; }
    bool IsAbstract { get; }
    bool IsConstructor { get; }
    bool IsFinal { get; }
    bool IsMethod { get; }
    bool IsSealed { get; }
    bool IsSpecializedName { get; }
    bool IsSpecialName { get; }
    bool IsVirtual { get; }
    ParameterizedSymbolKind ParameterizedSymbolKind { get; }
    IParameterListView Parameters { get; }
}