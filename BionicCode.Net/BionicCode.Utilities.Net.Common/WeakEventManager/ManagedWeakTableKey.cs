namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;

  internal readonly struct ManagedWeakTableKey : IEquatable<ManagedWeakTableKey>
  {
    public ManagedWeakTableKey(object referenceTargetId, Type referenceTargetType)
    {
      this.ReferenceTargetId = referenceTargetId;
      this.ReferenceTargetType = referenceTargetType;
    }

    public object ReferenceTargetId { get; }
    public Type ReferenceTargetType { get; }

    public bool Equals(ManagedWeakTableKey other) => other.ReferenceTargetId.Equals(this.ReferenceTargetId) && other.ReferenceTargetType.Equals(this.ReferenceTargetType);
    public override bool Equals(object obj) => obj is ManagedWeakTableKey key && Equals(key);

    public override int GetHashCode()
    {
      int hashCode = 433094870;
      hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(this.ReferenceTargetId);
      hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(this.ReferenceTargetType);
      return hashCode;
    }

    public static bool operator ==(ManagedWeakTableKey first, ManagedWeakTableKey second) => first.Equals(second);
    public static bool operator !=(ManagedWeakTableKey first, ManagedWeakTableKey second) => !first.Equals(second);
  }
}