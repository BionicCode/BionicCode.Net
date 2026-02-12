namespace BionicCode.Utilities.Net.Reflection;

using System.Collections.Generic;
using System.Reflection;

public interface ISymbolInfoDataView
{
    SymbolReflectionInfoCacheKey CacheKey { get; }
    string AssemblyName { get; }
    string Namespace { get; }
    IList<CustomAttributeData> AttributeData { get; }
    string DisplayName { get; }
    int FormattingIndentation { get; set; }
    string FullyQualifiedDisplayName { get; }
    string FullyQualifiedRuntimeSignature { get; }
    string FullyQualifiedSignature { get; }
    string IndentationString { get; }
    string Name { get; }
    string RuntimeShortCompactSignature { get; }
    string RuntimeShortSignature { get; }
    string RuntimeSignature { get; }
    string ShortCompactSignature { get; }
    string ShortDisplayName { get; }
    string ShortSignature { get; }
    string Signature { get; }
    SymbolAttributes SymbolAttributes { get; }
    SymbolComponentInfo SymbolComponentInfo { get; }
    SymbolKind SymbolKind { get; }
}