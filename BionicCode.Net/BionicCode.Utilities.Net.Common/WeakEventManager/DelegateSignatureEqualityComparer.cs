namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Reflection;

  /// <summary>
  /// Compares two delegates for equality.
  /// </summary>
  /// <remarks>
  /// This equality comparison differs from the default implementation of <see cref="Delegate.Equals(object)"/> and <see cref="MulticastDelegate.Equals(object)"/>
  /// in that it does not compare the delegate type. Different delegate types that reference the same target and have method signature and invocation list are considered equal.
  /// <br/>
  /// For example:
  /// <code>
  /// Action&lt;object, EventArgs&gt; action = SomeMethod;
  /// EventHandler&lt;EventArgs&gt; eventHandler = SomeMethod;
  /// 
  /// // Using the default comparison
  /// bool isEqual = action.Equals(eventHandler); // FALSE
  /// 
  /// // Using DelegateEqualityComparer comparison
  /// var delegateSignatureEqualityComparer = new DelegateSignatureEqualityComparer();
  /// isEqual = delegateSignatureEqualityComparer.Equals(action, eventHandler); // TRUE
  /// 
  /// private void SomeMethod(object, EventArgs)
  /// {
  /// }
  /// </code>
  /// The two delegates do not have to be of the same type in order to be considered equal.
  /// <para/>
  /// Two <see cref="Delegate"/> are compared for equality as follows:
  /// <list type="number">
  /// <item>
  /// The <see cref="Delegate.Method"/> values are compared using <see cref="MethodInfo.Equals(object)"/> to ensure one of the following conditions is true:
  ///   <list type="bullet">
  ///     <item>
  ///       If the two methods being compared are both static and are the same method on the same class, the methods are considered equal and the targets are also considered equal.
  ///     </item>
  ///     <item>
  ///        If the two methods being compared are instance methods and are the same method on the same object, the methods are considered equal.
  ///     </item>
  ///     <item>
  ///       Otherwise, the methods are not considered to be equal and the targets are also not considered to be equal.
  ///     </item>
  ///   </list>
  /// </item>
  /// <item>
  /// The <see cref="Delegate.Target"/> values are compared using <see cref="object.ReferenceEquals(object, object)"/> to ensure one of the following conditions is true:
  ///   <list type="bullet">
  ///     <item>
  ///       If the two delegate targets being compared are both <see langword="null"/> (in case of static methods), the targets are considered equal and the targets are also considered equal.
  ///     </item>
  ///     <item>
  ///       If the two delegate targets being compared are the same instance, the targets are considered equal.
  ///     </item>
  ///     <item>
  ///       Otherwise, the delegate targets are not considered to be equal.
  ///     </item>
  ///   </list>
  /// </item>
  /// <item>
  /// The invocation lists are compared to ensure all of the following conditions are satisfied:
  ///   <list type="bullet">
  ///     <item>
  ///       Both invocation lists contain the same number of elements.
  ///     </item>
  ///     <item>
  ///       Both invocation lists are ordered exactly the same,
  ///       where both rules 1) and 2) must be satisfied. 
  ///       <br/>This means every element in the invocation list of the first delegate is equal to the corresponding element in the invocation list of the second delegate.
  ///     </item>
  ///   </list>
  /// </item>
  /// </list> 
  /// </remarks>
  public class DelegateSignatureEqualityComparer : EqualityComparer<Delegate>
  {
    /// <summary>
    /// Compares two delegates for equality.
    /// </summary>
    /// <remarks>
    /// This equality comparison differs from the default implementation of <see cref="Delegate.Equals(object)"/> and <see cref="MulticastDelegate.Equals(object)"/>
    /// in that it does not compare the delegate type. Different delegate types that reference the same target and have method signature and invocation list are considered equal.
    /// <br/>
    /// For example:
    /// <code>
    /// Action&lt;object, EventArgs&gt; action = SomeMethod;
    /// EventHandler&lt;EventArgs&gt; eventHandler = SomeMethod;
    /// 
    /// // Using the default comparison
    /// bool isEqual = action.Equals(eventHandler); // FALSE
    /// 
    /// // Using DelegateEqualityComparer comparison
    /// var delegateSignatureEqualityComparer = new DelegateSignatureEqualityComparer();
    /// isEqual = delegateSignatureEqualityComparer.Equals(action, eventHandler); // TRUE
    /// 
    /// private void SomeMethod(object, EventArgs)
    /// {
    /// }
    /// </code>
    /// The two delegates do not have to be of the same type in order to be considered equal.
    /// <para/>
    /// Two <see cref="Delegate"/> are compared for equality as follows:
    /// <list type="number">
    /// <item>
    /// The <see cref="Delegate.Method"/> values are compared using <see cref="MethodInfo.Equals(object)"/> to ensure one of the following conditions is true:
    ///   <list type="bullet">
    ///     <item>
    ///       If the two methods being compared are both static and are the same method on the same class, the methods are considered equal and the targets are also considered equal.
    ///     </item>
    ///     <item>
    ///        If the two methods being compared are instance methods and are the same method on the same object, the methods are considered equal.
    ///     </item>
    ///     <item>
    ///       Otherwise, the methods are not considered to be equal and the targets are also not considered to be equal.
    ///     </item>
    ///   </list>
    /// </item>
    /// <item>
    /// The <see cref="Delegate.Target"/> values are compared using <see cref="object.ReferenceEquals(object, object)"/> to ensure one of the following conditions is true:
    ///   <list type="bullet">
    ///     <item>
    ///       If the two delegate targets being compared are both <see langword="null"/> (in case of static methods), the targets are considered equal and the targets are also considered equal.
    ///     </item>
    ///     <item>
    ///       If the two delegate targets being compared are the same instance, the targets are considered equal.
    ///     </item>
    ///     <item>
    ///       Otherwise, the delegate targets are not considered to be equal.
    ///     </item>
    ///   </list>
    /// </item>
    /// <item>
    /// The invocation lists are considered identical when all of the following conditions are satisfied:
    ///   <list type="bullet">
    ///     <item>
    ///       Both invocation lists contain the same number of elements.
    ///     </item>
    ///     <item>
    ///       Both invocation lists are ordered exactly the same,
    ///       where both rules 1) and 2) must be satisfied. 
    ///       <br/>This means every element in the invocation list of the first delegate is equal to the corresponding element in the invocation list of the second delegate.
    ///     </item>
    ///   </list>
    /// </item>
    /// </list> 
    /// </remarks>
    public override bool Equals(Delegate x, Delegate y) => AreDelegatesEqual(x, y);
    public override int GetHashCode(Delegate obj) => obj.GetHashCode();

    private static bool AreDelegatesEqual(Delegate d1, Delegate d2)
    {
      if (d1 is null && d2 is null
        || ReferenceEquals(d1, d2))
      {
        return true;
      }

      if (d1 is null || d2 is null)
      {
        return false;
      }

      d1 = UnwrapDelegate(d1);
      d2 = UnwrapDelegate(d2);

      if (ReferenceEquals(d1.Target, d2.Target) 
        && d1.Method.Equals(d2.Method))
      {
        Delegate[] d1InvocationList = d1.GetInvocationList();
        Delegate[] d2InvocationList = d2.GetInvocationList();
        if (d1InvocationList.Length == d2InvocationList.Length)
        {
          if (d1InvocationList.Length == 0)
          {
            return true;
          }

          for (int invocatorIndex = 0; invocatorIndex < d1InvocationList.Length; invocatorIndex++)
          {
            Delegate d1Invocator = d1InvocationList[invocatorIndex];
            Delegate d2Invocator = d2InvocationList[invocatorIndex];
            d1 = UnwrapDelegate(d1Invocator);
            d2 = UnwrapDelegate(d2Invocator);
            if (!(ReferenceEquals(d1.Target, d2.Target)
              && d1.Method.Equals(d2.Method)))
            {
              return false;
            }
          }

          return true;
        }
      }

      return false;
    }

    private static Delegate UnwrapDelegate(Delegate d)
    {
      object dTarget = d.Target;

      // Unwrap delegate if wrapped
      while (dTarget is Delegate dTemp)
      {
        d = dTemp;
        dTarget = d.Target;
      }

      return d;
    }
  }
}