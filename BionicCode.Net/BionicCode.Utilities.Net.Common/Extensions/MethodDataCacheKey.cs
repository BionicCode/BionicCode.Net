namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  internal readonly struct MethodDataCacheKey : ISymbolInfoDataCacheKey, ISymbolInfoDataCacheKey<RuntimeMethodHandle>, IEquatable<MethodDataCacheKey>
  {
    public MethodDataCacheKey(string symbolName, string symbolNamespace, RuntimeTypeHandle declaringTypeHandle, RuntimeMethodHandle symbolHandle, params object[] arguments)
    {
      this.Name = symbolName;
      this.Namespace = symbolNamespace;
      this.Arguments = arguments;
      this.DeclaringTypeHandle = declaringTypeHandle;
      this.SymbolHandle = symbolHandle;

      int hashCode = 1248511333;
      hashCode = hashCode * -1521134295 + this.Name.GetHashCode();

      foreach (object argument in arguments)
      {
        hashCode = hashCode * -1521134295 + argument.GetHashCode();
      }

      hashCode = hashCode * -1521134295 + this.DeclaringTypeHandle.GetHashCode();
      hashCode = hashCode * -1521134295 + this.Namespace.GetHashCode();
      hashCode = hashCode * -1521134295 + this.SymbolHandle.GetHashCode();
      this.hashCode = hashCode;
    }

    private readonly int? hashCode;
    public string Name { get; }
    public string Namespace { get; }
    public object[] Arguments { get; }
    public RuntimeTypeHandle DeclaringTypeHandle { get; }
    public RuntimeMethodHandle SymbolHandle { get; }

    public bool Equals(MethodDataCacheKey other) => other.Name.Equals(this.Name, StringComparison.Ordinal)
      && other.Arguments.SequenceEqual(this.Arguments)
      && other.DeclaringTypeHandle.Equals(this.DeclaringTypeHandle)
      && other.Namespace.Equals(this.Namespace)
      && other.SymbolHandle.Equals(this.SymbolHandle);

    public override bool Equals(object obj) 
      => obj is MethodDataCacheKey cacheKey && Equals(cacheKey);

    public override int GetHashCode()
      => this.hashCode.Value;

    bool IEquatable<ISymbolInfoDataCacheKey>.Equals(ISymbolInfoDataCacheKey other) 
      => other is MethodDataCacheKey cacheKey && Equals(cacheKey);

    bool IEquatable<ISymbolInfoDataCacheKey<RuntimeMethodHandle>>.Equals(ISymbolInfoDataCacheKey<RuntimeMethodHandle> other) 
      => other is MethodDataCacheKey cacheKey && Equals(cacheKey);

    public static bool operator ==(MethodDataCacheKey left, MethodDataCacheKey right) => left.Equals(right);
    public static bool operator !=(MethodDataCacheKey left, MethodDataCacheKey right) => !(left == right);
  }
}