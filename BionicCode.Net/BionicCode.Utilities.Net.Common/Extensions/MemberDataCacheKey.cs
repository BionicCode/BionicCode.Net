namespace BionicCode.Utilities.Net
{
  using System;
  using System.Linq;
  using System.Reflection;

  internal readonly struct MemberDataCacheKey : IMemberDataCacheKey, IEquatable<MemberDataCacheKey>
  {
    /// <summary>
    /// Constructor for member symbols.
    /// </summary>
    /// <param name="declaringTypeHandle">The handle of the member's declaring type.</param>
    /// <param name="memberName">The name of the member.</param>
    /// <param name="parameterList">The argument list in case the member is a method or constructor.</param>
    public MemberDataCacheKey(RuntimeTypeHandle declaringTypeHandle, string memberName)
    {
      ArgumentNullExceptionEx.ThrowIfNullOrWhiteSpace(memberName, nameof(memberName));

      this.MemberName = memberName;
      this.ParameterList = Array.Empty<MemberParameterInfo>();
      this.DeclaringTypeHandle = declaringTypeHandle;

      int hashCode = 1248511333;
      hashCode = hashCode * -1521134295 + this.MemberName.GetHashCode();
      hashCode = hashCode * -1521134295 + this.DeclaringTypeHandle.GetHashCode();
      this.hashCode = hashCode;
    }

    /// <summary>
    /// Constructor for member symbols.
    /// </summary>
    /// <param name="declaringTypeHandle">The handle of the member's declaring type.</param>
    /// <param name="memberName">The name of the member.</param>
    /// <param name="parameterList">The argument list in case the member is a method or constructor.</param>
    public MemberDataCacheKey(RuntimeTypeHandle declaringTypeHandle, string memberName, params ParameterInfo[] parameterList)
    {
      ArgumentNullExceptionEx.ThrowIfNullOrWhiteSpace(memberName, nameof(memberName));

      this.MemberName = memberName;
      this.ParameterList = MemberParameterInfo.ConvertFrom(parameterList);
      this.DeclaringTypeHandle = declaringTypeHandle;

      int hashCode = 1248511333;
      foreach (object argument in this.ParameterList)
      {
        hashCode = hashCode * -1521134295 + argument.GetHashCode();
      }

      hashCode = hashCode * -1521134295 + this.MemberName.GetHashCode();
      hashCode = hashCode * -1521134295 + this.DeclaringTypeHandle.GetHashCode();
      this.hashCode = hashCode;
    }

    /// <summary>
    /// Constructor for member symbols.
    /// </summary>
    /// <param name="declaringTypeHandle">The handle of the member's declaring type.</param>
    /// <param name="memberName">The name of the member.</param>
    /// <param name="parameterList">The argument list in case the member is a method or constructor.</param>
    public MemberDataCacheKey(RuntimeTypeHandle declaringTypeHandle, string memberName, params ParameterData[] parameterList)
    {
      ArgumentNullExceptionEx.ThrowIfNullOrWhiteSpace(memberName, nameof(memberName));

      this.MemberName = memberName;
      this.ParameterList = MemberParameterInfo.ConvertFrom(parameterList);
      this.DeclaringTypeHandle = declaringTypeHandle;

      int hashCode = 1248511333;
      foreach (object argument in this.ParameterList)
      {
        hashCode = hashCode * -1521134295 + argument.GetHashCode();
      }

      hashCode = hashCode * -1521134295 + this.MemberName.GetHashCode();
      hashCode = hashCode * -1521134295 + this.DeclaringTypeHandle.GetHashCode();
      this.hashCode = hashCode;
    }

    /// <summary>
    /// Constructor for member symbols.
    /// </summary>
    /// <param name="declaringTypeHandle">The handle of the member's declaring type.</param>
    /// <param name="memberName">The name of the member.</param>
    /// <param name="parameterList">The argument list in case the member is a method or constructor.</param>
    public MemberDataCacheKey(RuntimeTypeHandle declaringTypeHandle, string memberName, params MemberParameterInfo[] parameterList)
    {
      ArgumentNullExceptionEx.ThrowIfNullOrWhiteSpace(memberName, nameof(memberName));

      this.MemberName = memberName;
      this.ParameterList = parameterList;
      this.DeclaringTypeHandle = declaringTypeHandle;

      int hashCode = 1248511333;
      foreach (object argument in this.ParameterList)
      {
        hashCode = hashCode * -1521134295 + argument.GetHashCode();
      }

      hashCode = hashCode * -1521134295 + this.MemberName.GetHashCode();
      hashCode = hashCode * -1521134295 + this.DeclaringTypeHandle.GetHashCode();
      this.hashCode = hashCode;
    }

    private readonly int? hashCode;
    public string MemberName { get; }
    public RuntimeTypeHandle DeclaringTypeHandle { get; }
    public MemberParameterInfo[] ParameterList { get; }

    public bool Equals(MemberDataCacheKey other) => other.MemberName.Equals(this.MemberName, StringComparison.OrdinalIgnoreCase)
      && other.ParameterList.SequenceEqual(this.ParameterList)
      && other.DeclaringTypeHandle.Equals(this.DeclaringTypeHandle);

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