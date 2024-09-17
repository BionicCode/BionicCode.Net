namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  internal readonly struct SymbolInfoDataCacheKey : IEquatable<SymbolInfoDataCacheKey>
  {
    public SymbolInfoDataCacheKey(string name, RuntimeTypeHandle declaringTypeHandle, object handle, params object[] arguments)
    {
      this.Name = name;
      this.Arguments = arguments;
      this.DeclaringTypeHandle = declaringTypeHandle;
      this.Handle = handle;
    }

    public string Name { get; }
    public object[] Arguments { get; }
    public RuntimeTypeHandle DeclaringTypeHandle { get; }
    public object Handle { get; }

    public bool Equals(SymbolInfoDataCacheKey other) => other.Name.Equals(this.Name, StringComparison.Ordinal) 
      && other.Arguments.SequenceEqual(this.Arguments) 
      && other.DeclaringTypeHandle.Equals(this.DeclaringTypeHandle)
      && other.Handle.Equals(this.Handle);

    public override bool Equals(object obj) => obj is SymbolInfoDataCacheKey key && Equals(key);

    public override int GetHashCode()
    {
      int hashCode = 1248511333;
      hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Name);
      hashCode = hashCode * -1521134295 + EqualityComparer<object[]>.Default.GetHashCode(this.Arguments);
      hashCode = hashCode * -1521134295 + this.DeclaringTypeHandle.GetHashCode();
      hashCode = hashCode * -1521134295 + this.Handle.GetHashCode();
      return hashCode;
    }

    public static bool operator ==(SymbolInfoDataCacheKey left, SymbolInfoDataCacheKey right) => left.Equals(right);
    public static bool operator !=(SymbolInfoDataCacheKey left, SymbolInfoDataCacheKey right) => !(left == right);
  }
}