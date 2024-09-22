namespace BionicCode.Utilities.Net
{
  using System;
  using System.Linq;

  internal readonly struct FieldDataCacheKey : ISymbolInfoDataCacheKey, ISymbolInfoDataCacheKey<RuntimeFieldHandle>, IEquatable<FieldDataCacheKey>
  {
    public FieldDataCacheKey(string symbolName, string symbolNamespace, RuntimeTypeHandle declaringTypeHandle, RuntimeFieldHandle symbolHandle, params object[] arguments)
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
    public RuntimeFieldHandle SymbolHandle { get; }

    public bool Equals(FieldDataCacheKey other) => other.Name.Equals(this.Name, StringComparison.Ordinal)
      && other.Arguments.SequenceEqual(this.Arguments)
      && other.DeclaringTypeHandle.Equals(this.DeclaringTypeHandle)
      && other.Namespace.Equals(this.Namespace)
      && other.SymbolHandle.Equals(this.SymbolHandle);

    public override bool Equals(object obj) 
      => obj is FieldDataCacheKey key && Equals(key);

    public override int GetHashCode()
      => this.hashCode.Value;

    bool IEquatable<ISymbolInfoDataCacheKey>.Equals(ISymbolInfoDataCacheKey other)
      => other is FieldDataCacheKey cacheKey && Equals(cacheKey);

    bool IEquatable<ISymbolInfoDataCacheKey<RuntimeFieldHandle>>.Equals(ISymbolInfoDataCacheKey<RuntimeFieldHandle> other)
      => other is FieldDataCacheKey cacheKey && Equals(cacheKey);

    public static bool operator ==(FieldDataCacheKey left, FieldDataCacheKey right) => left.Equals(right);
    public static bool operator !=(FieldDataCacheKey left, FieldDataCacheKey right) => !(left == right);
  }
}