namespace BionicCode.Utilities.Net
{
  using System;
  using System.Linq;

  internal readonly struct SymbolInfoDataCacheKey<THandle> : ISymbolInfoDataCacheKey, ISymbolInfoDataCacheKey<THandle>, IEquatable<SymbolInfoDataCacheKey<THandle>> where THandle : struct
  {
    public SymbolInfoDataCacheKey(string symbolName, string symbolNamespace, RuntimeTypeHandle declaringTypeHandle, THandle symbolHandle, params object[] arguments)
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
    public THandle SymbolHandle { get; }

    public bool Equals(SymbolInfoDataCacheKey<THandle> other) => other.Name.Equals(this.Name, StringComparison.Ordinal)
      && other.Arguments.SequenceEqual(this.Arguments)
      && other.DeclaringTypeHandle.Equals(this.DeclaringTypeHandle)
      && other.Namespace.Equals(this.Namespace)
      && other.SymbolHandle.Equals(this.SymbolHandle);

    public override bool Equals(object obj)
      => obj is SymbolInfoDataCacheKey<THandle> key && Equals(key);

    public override int GetHashCode()
      => this.hashCode.Value;

    bool IEquatable<ISymbolInfoDataCacheKey>.Equals(ISymbolInfoDataCacheKey other)
      => other is SymbolInfoDataCacheKey<THandle> cacheKey && Equals(cacheKey);

    bool IEquatable<ISymbolInfoDataCacheKey<THandle>>.Equals(ISymbolInfoDataCacheKey<THandle> other)
      => other is SymbolInfoDataCacheKey<THandle> cacheKey && Equals(cacheKey);

    public static bool operator ==(SymbolInfoDataCacheKey<THandle> left, SymbolInfoDataCacheKey<THandle> right) => left.Equals(right);
    public static bool operator !=(SymbolInfoDataCacheKey<THandle> left, SymbolInfoDataCacheKey<THandle> right) => !(left == right);
  }

  internal readonly struct SymbolInfoDataCacheKey : ISymbolInfoDataCacheKey, IEquatable<SymbolInfoDataCacheKey>
  {
    public SymbolInfoDataCacheKey(string symbolName, string symbolNamespace, RuntimeTypeHandle declaringTypeHandle, params object[] arguments)
    {
      this.Name = symbolName;
      this.Namespace = symbolNamespace;
      this.Arguments = arguments;
      this.DeclaringTypeHandle = declaringTypeHandle;

      int hashCode = 1248511333;
      hashCode = hashCode * -1521134295 + this.Name.GetHashCode();

      foreach (object argument in arguments)
      {
        hashCode = hashCode * -1521134295 + argument.GetHashCode();
      }

      hashCode = hashCode * -1521134295 + this.DeclaringTypeHandle.GetHashCode();
      hashCode = hashCode * -1521134295 + this.Namespace.GetHashCode();
      this.hashCode = hashCode;
    }

    private readonly int? hashCode;
    public string Name { get; }
    public string Namespace { get; }
    public object[] Arguments { get; }
    public RuntimeTypeHandle DeclaringTypeHandle { get; }

    public bool Equals(SymbolInfoDataCacheKey other) => other.Name.Equals(this.Name, StringComparison.Ordinal)
      && other.Arguments.SequenceEqual(this.Arguments)
      && other.DeclaringTypeHandle.Equals(this.DeclaringTypeHandle)
      && other.Namespace.Equals(this.Namespace);

    public override bool Equals(object obj)
      => obj is SymbolInfoDataCacheKey key && Equals(key);

    public override int GetHashCode()
      => this.hashCode.Value;

    bool IEquatable<ISymbolInfoDataCacheKey>.Equals(ISymbolInfoDataCacheKey other)
      => other is SymbolInfoDataCacheKey cacheKey && Equals(cacheKey);

    public static bool operator ==(SymbolInfoDataCacheKey left, SymbolInfoDataCacheKey right) => left.Equals(right);
    public static bool operator !=(SymbolInfoDataCacheKey left, SymbolInfoDataCacheKey right) => !(left == right);
  }
}