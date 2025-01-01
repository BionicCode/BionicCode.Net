namespace BionicCode.Utilities.Net
{
  using System;
  using System.Linq;
  using System.Reflection;

  internal readonly struct TypeDataCacheKey : ITypeDataCacheKey, IEquatable<TypeDataCacheKey>
  {
    /// <summary>
    /// Constructor for type symbols (e.g. class or delegate).
    /// </summary>
    /// <param name="typeHandle">Either the handle of the type (e.g. class or delegate).</param>
    /// <param name="parameterList">Optional. In case of a delegate, the argument list.</param>
    public TypeDataCacheKey(RuntimeTypeHandle typeHandle)
    {
      this.TypeHandle = typeHandle;
      this.ParameterList = Array.Empty<MemberParameterInfo>();

      int hashCode = 1248511333;
      hashCode = hashCode * -1521134295 + this.TypeHandle.GetHashCode();
      this.hashCode = hashCode;
    }

    /// <summary>
    /// Constructor for type symbols (e.g. class or delegate).
    /// </summary>
    /// <param name="typeHandle">Either the handle of the type (e.g. class or delegate).</param>
    /// <param name="parameterList">Optional. In case of a delegate, the argument list.</param>
    public TypeDataCacheKey(RuntimeTypeHandle typeHandle, params MemberParameterInfo[] parameterList)
    {
      this.TypeHandle = typeHandle;
      this.ParameterList = parameterList;

      int hashCode = 1248511333;
      foreach (object argument in this.ParameterList)
      {
        hashCode = hashCode * -1521134295 + argument.GetHashCode();
      }

      hashCode = hashCode * -1521134295 + this.TypeHandle.GetHashCode();
      this.hashCode = hashCode;
    }

    /// <summary>
    /// Constructor for type symbols (e.g. class or delegate).
    /// </summary>
    /// <param name="typeHandle">Either the handle of the type (e.g. class or delegate).</param>
    /// <param name="parameterList">Optional. In case of a delegate, the argument list.</param>
    public TypeDataCacheKey(RuntimeTypeHandle typeHandle, params ParameterInfo[] parameterList)
    {
      this.TypeHandle = typeHandle;
      this.ParameterList = MemberParameterInfo.ConvertFrom(parameterList);

      int hashCode = 1248511333;
      foreach (object argument in this.ParameterList)
      {
        hashCode = hashCode * -1521134295 + argument.GetHashCode();
      }

      hashCode = hashCode * -1521134295 + this.TypeHandle.GetHashCode();
      this.hashCode = hashCode;
    }

    /// <summary>
    /// Constructor for type symbols (e.g. class or delegate).
    /// </summary>
    /// <param name="typeHandle">Either the handle of the type (e.g. class or delegate).</param>
    /// <param name="parameterList">Optional. In case of a delegate, the argument list.</param>
    public TypeDataCacheKey(RuntimeTypeHandle typeHandle, params ParameterData[] parameterList)
    {
      this.TypeHandle = typeHandle;
      this.ParameterList = MemberParameterInfo.ConvertFrom(parameterList);

      int hashCode = 1248511333;
      foreach (object argument in this.ParameterList)
      {
        hashCode = hashCode * -1521134295 + argument.GetHashCode();
      }

      hashCode = hashCode * -1521134295 + this.TypeHandle.GetHashCode();
      this.hashCode = hashCode;
    }

    private readonly int? hashCode;
    public RuntimeTypeHandle TypeHandle { get; }
    public MemberParameterInfo[] ParameterList { get; }

    public bool Equals(TypeDataCacheKey other) => other.ParameterList.SequenceEqual(this.ParameterList)
      && other.TypeHandle.Equals(this.TypeHandle);

    public override bool Equals(object obj)
      => obj is TypeDataCacheKey key && Equals(key);

    public override int GetHashCode()
      => this.hashCode.Value;

    bool IEquatable<ISymbolInfoDataCacheKey>.Equals(ISymbolInfoDataCacheKey other)
      => other is TypeDataCacheKey cacheKey && Equals(cacheKey);

    public static bool operator ==(TypeDataCacheKey left, TypeDataCacheKey right) => left.Equals(right);
    public static bool operator !=(TypeDataCacheKey left, TypeDataCacheKey right) => !(left == right);
  }
}