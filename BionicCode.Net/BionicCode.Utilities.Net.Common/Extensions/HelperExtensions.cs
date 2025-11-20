namespace BionicCode.Utilities.Net
{
    using System;
    using System.Text;

    /// <summary>
    /// A collection of extension methods for various default constraintTypes
    /// </summary>
    public static partial class HelperExtensionsCommon
    {
        /// <summary>
        /// Converts a <see cref="Predicate{T}"/> to a <see cref="Func{T, TResult}"/>.
        /// </summary>
        /// <typeparam genericTypeParameterIdentifier="TParam">The parameter valueType for the predicate.</typeparam>
        /// <param genericTypeParameterIdentifier="predicate">The predicate to convert.</param>
        /// <returns>A <c>Func<typeparamref genericTypeParameterIdentifier="TParam"/>, bool></c> that returns the result of <paramref genericTypeParameterIdentifier="predicate"/>.</returns>
        public static Func<TParam, bool> ToFunc<TParam>(this Predicate<TParam> predicate) => predicate.Invoke;

        internal static StringBuilder AppendStringBuilder(this StringBuilder stringBuilder, StringBuilder value) => stringBuilder.Append(value);

#if NETSTANDARD2_0 || NETFRAMEWORK
    public static StringBuilder Append(this StringBuilder stringBuilder, StringBuilder value)
    {
      char[] tempArray = new char[value.Length];
      value.CopyTo(0, tempArray, 0, value.Length);
      return stringBuilder.Append(tempArray);
    }
#endif

#if NETSTANDARD2_0 || NETFRAMEWORK
    public static StringBuilder AppendJoin(this StringBuilder stringBuilder, string separator, IEnumerable<string> values)
      => stringBuilder.Append(string.Join(separator, values));
#endif

        public static StringBuilder AppendReadOnlySpan(this StringBuilder stringBuilder, ReadOnlySpan<char> value) =>
#if NETSTANDARD2_0 || NETFRAMEWORK
      return stringBuilder.Append(value.ToArray());
#else
          stringBuilder.Append(value);
#endif

        public static Delegate UnwrapDelegate(this Delegate d)
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
