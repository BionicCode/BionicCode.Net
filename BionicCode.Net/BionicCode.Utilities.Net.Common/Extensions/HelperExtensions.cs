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
