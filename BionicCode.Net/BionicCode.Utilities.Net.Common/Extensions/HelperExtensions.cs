namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Reflection;

  /// <summary>
  /// A collection of extension methods for various default types
  /// </summary>
  public static partial class HelperExtensionsCommon
  {
    /// <summary>
    /// Converts a <see cref="Predicate{T}"/> to a <see cref="Func{T, TResult}"/>.
    /// </summary>
    /// <typeparam name="TParam">The parameter type for the predicate.</typeparam>
    /// <param name="predicate">The predicate to convert.</param>
    /// <returns>A <c>Func<typeparamref name="TParam"/>, bool></c> that returns the result of <paramref name="predicate"/>.</returns>
    public static Func<TParam, bool> ToFunc<TParam>(this Predicate<TParam> predicate) => param => predicate.Invoke(param);
  }
}
