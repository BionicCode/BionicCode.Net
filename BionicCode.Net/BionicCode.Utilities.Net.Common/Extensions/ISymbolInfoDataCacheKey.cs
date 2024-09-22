namespace BionicCode.Utilities.Net
{
  using System;

  internal interface ISymbolInfoDataCacheKey : IEquatable<ISymbolInfoDataCacheKey>
  {
    object[] Arguments { get; }
    RuntimeTypeHandle DeclaringTypeHandle { get; }
    string Name { get; }
    string Namespace { get; }
  }

  internal interface ISymbolInfoDataCacheKey<THandle> : ISymbolInfoDataCacheKey, IEquatable<ISymbolInfoDataCacheKey<THandle>>
  {
    THandle SymbolHandle { get; }
  }
}