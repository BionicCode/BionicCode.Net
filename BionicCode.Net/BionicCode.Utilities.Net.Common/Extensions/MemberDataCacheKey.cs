namespace BionicCode.Utilities.Net
{
  using System;
  using System.Linq;

  internal readonly struct MemberDataCacheKey : ISymbolInfoDataCacheKey, IEquatable<MemberDataCacheKey>
  {
    public MemberDataCacheKey(string symbolName, string symbolNamespace, RuntimeTypeHandle declaringTypeHandle, params object[] arguments)
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

    public bool Equals(MemberDataCacheKey other) => other.Name.Equals(this.Name, StringComparison.Ordinal)
      && other.Arguments.SequenceEqual(this.Arguments)
      && other.DeclaringTypeHandle.Equals(this.DeclaringTypeHandle)
      && other.Namespace.Equals(this.Namespace);

    public override bool Equals(object obj) 
      => obj is MemberDataCacheKey key && Equals(key);

    public override int GetHashCode()
      => this.hashCode.Value;

    bool IEquatable<ISymbolInfoDataCacheKey>.Equals(ISymbolInfoDataCacheKey other)
      => other is MemberDataCacheKey cacheKey && Equals(cacheKey);

    public static bool operator ==(MemberDataCacheKey left, MemberDataCacheKey right) => left.Equals(right);
    public static bool operator !=(MemberDataCacheKey left, MemberDataCacheKey right) => !(left == right);
  }
}