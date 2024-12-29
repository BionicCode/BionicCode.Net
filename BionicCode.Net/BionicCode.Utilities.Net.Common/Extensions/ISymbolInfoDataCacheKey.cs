namespace BionicCode.Utilities.Net
{
  using System;

  internal interface ISymbolInfoDataCacheKey : IEquatable<ISymbolInfoDataCacheKey>
  {
    MemberParameterInfo[] ParameterList { get; }
  }

  internal interface ITypeDataCacheKey : ISymbolInfoDataCacheKey
  {
    RuntimeTypeHandle TypeHandle { get; }
  }

  internal interface IMemberDataCacheKey : ISymbolInfoDataCacheKey
  {
    string MemberName { get; }
    RuntimeTypeHandle DeclaringTypeHandle { get; }
  }
}