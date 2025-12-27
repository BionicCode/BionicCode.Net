namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public class ArgumentNullExceptionEx : System.ArgumentNullException
    {
        public ArgumentNullExceptionEx()
        {
        }

        public ArgumentNullExceptionEx(string paramName) : base(paramName)
        {
        }

        public ArgumentNullExceptionEx(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ArgumentNullExceptionEx(string paramName, string message) : base(paramName, message)
        {
        }

        /// <summary>
        /// Throws an ArgumentNullException if the specified value is equal to the default value of its type.
        /// </summary>
        /// <remarks>Use this method to ensure that a value type parameter has been initialized and is not
        /// equal to its default value. This is useful for validating struct parameters where the default value may be
        /// invalid or unintended.</remarks>
        /// <typeparam name="TStruct">The value type to check for the default value. Must be a struct.</typeparam>
        /// <param name="value">The value to validate against its default value.</param>
        /// <param name="paramName">The name of the parameter to include in the exception message. This value is typically provided
        /// automatically and should not be set explicitly.</param>
        public static void ThrowIfDefault<TStruct>(TStruct value, [CallerArgumentExpression(nameof(value))] string? paramName = null) where TStruct : struct
            => ArgumentNullException.ThrowIfNull(value.Equals(default(TStruct)) ? null : value, paramName);

        /// <summary>
        /// Throws an exception if the specified enumerable is null or contains no elements.
        /// </summary>
        /// <remarks>This method is typically used to validate method arguments that are expected to be
        /// non-null and contain at least one element. If the enumerable is null, an ArgumentNullException is thrown by
        /// ArgumentNullException.ThrowIfNull.</remarks>
        /// <param name="value">The enumerable to validate. Cannot be null.</param>
        /// <param name="paramName">The name of the parameter representing the enumerable. This value is used in the exception message if an
        /// exception is thrown.</param>
        /// <param name="message">The custom error message to include in the exception if the enumerable is empty. If null, a default message
        /// is used.</param>
        /// <exception cref="ArgumentException">Thrown if the enumerable is empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown if the enumerable is null.</exception>"
        public static void ThrowIfNullOrEmpty(IEnumerable value, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null)
        {
            ArgumentNullException.ThrowIfNull(value, paramName);
            IEnumerator enumerator = value.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                throw new ArgumentException(
                    message ?? "The enumerable is empty.",
                    paramName);
            }
        }

        /// <summary>
        /// Throws an ArgumentException if the specified value is not null.
        /// </summary>
        /// <param name="value">The object to check for null. No exception is thrown if this value is null.</param>
        /// <param name="paramName">The name of the parameter being checked. This value is used in the exception message. If not specified, the
        /// caller argument expression is used.</param>
        /// <param name="message">The custom error message to include in the exception. If null, a default message is used.</param>
        /// <exception cref="ArgumentException">Thrown if value is not null.</exception>
        public static void ThrowIfNotNull(object value, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null)
        {
            if (value is not null)
            {
                throw new ArgumentException(
                    message ?? "The argument must be null.",
                    paramName);
            }
        }
    }

    public class ArgumentExceptionEx : ArgumentException
    {
        public ArgumentExceptionEx()
        {
        }

        public ArgumentExceptionEx(string message) : base(message)
        {
        }

        public ArgumentExceptionEx(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ArgumentExceptionEx(string message, string paramName) : base(message, paramName)
        {
        }

        public ArgumentExceptionEx(string message, string paramName, Exception innerException) : base(message, paramName, innerException)
        {
        }

        /// <summary>
        /// Validates that the specified delegate is compatible with the signature of the given event. Throws an exception
        /// if the delegate cannot be assigned as an event handler.
        /// </summary>
        /// <remarks>Use this method to ensure that a delegate can be safely attached to an event at runtime. This
        /// validation checks that the number and types of parameters in the delegate match those expected by the event.
        /// This method does not check for null arguments; callers should ensure arguments are not null before
        /// calling.</remarks>
        /// <param name="eventInfo">The event metadata that defines the expected event handler signature. Cannot be null.</param>
        /// <param name="clientHandler">The delegate to validate as a potential event handler for the event. Cannot be null.</param>
        /// <param name="paramName"></param>
        /// <param name="message"></param>
        /// <exception cref="EventHandlerMismatchException">Thrown if the delegate's signature does not match the event handler type required by the event.</exception>
        public static void ThrowIfNotAssignable(EventInfo eventInfo, Delegate clientHandler, [CallerArgumentExpression(nameof(eventInfo))] string? paramName = null, string? message = null)
        {
            ArgumentNullException.ThrowIfNull(eventInfo, nameof(eventInfo));
            ArgumentNullException.ThrowIfNull(clientHandler, nameof(clientHandler));

            MethodInfo eventDelegateInvokeMethod = eventInfo.EventHandlerType.GetMethod(HelperExtensionsCommon.DelegateInvocatorMethodName);
            ParameterInfo[] eventDelegateParameters = eventDelegateInvokeMethod.GetParameters();

            MethodInfo eventHandlerMethod = clientHandler.Method;
            ParameterInfo[] clientHandlerParameters = eventHandlerMethod.GetParameters();

            /* Validate the event EventHandler */

            if (eventDelegateParameters.Length != clientHandlerParameters.Length)
            {
                throw new EventHandlerMismatchException(message ?? ExceptionMessages.GetHandlerDelegateSignatureMismatchExceptionMessage(eventInfo, eventHandlerMethod, "Invalid parameter count."));
            }

            for (int parameterIndex = 0; parameterIndex < eventDelegateParameters.Length; parameterIndex++)
            {
                Type eventDelegateParameterType = eventDelegateParameters[parameterIndex].ParameterType;
                Type eventHandlerParameterType = clientHandlerParameters[parameterIndex].ParameterType;
                if (!eventHandlerParameterType.IsAssignableFrom(eventDelegateParameterType))
                {
                    string exceptionMessage = message ?? ExceptionMessages.GetHandlerDelegateSignatureMismatchExceptionMessage(
                        eventInfo,
                        eventHandlerMethod,
                        $"Unable to cast parameter of type '{eventDelegateParameterType.FullName}' at parameter index '{parameterIndex}' of the event delegate to type '{eventHandlerParameterType.FullName}' of the event handler.");
                    throw new EventHandlerMismatchException(exceptionMessage);
                }
            }
        }

        /// <summary>
        /// Validates that the specified value corresponds to a defined value of the specified enumeration type, and
        /// throws an exception if it does not.
        /// </summary>
        /// <remarks>Use this method to ensure that a value is a valid member of a specific enum type
        /// before using it in code that requires a defined enum value. This is especially useful when working with
        /// values from untrusted sources or deserialization.</remarks>
        /// <typeparam name="TEnum">The enumeration type against which to validate the value. Must be a struct that implements Enum.</typeparam>
        /// <param name="value">The value to validate. Can be an enum value or a convertible value representing an enum member (e.g. an <see langword="int"/> value).</param>
        /// <param name="paramName">The name of the parameter being validated. This value is used in any thrown exception to identify the
        /// invalid argument. Optional.</param>
        /// <param name="message">An optional exception message.</param>
        /// <exception cref="ArgumentException">Thrown if the provided value is an enum of a different type than <typeparamref name="TEnum"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the provided value does not correspond to a defined member of <typeparamref name="TEnum"/>.</exception>
        public static void ThrowIfEnumIsNotDefined<TEnum>(IConvertible value, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null) where TEnum : struct, Enum
        {
            ArgumentNullException.ThrowIfNull(value, paramName);

            TEnum parsedEnum = value is Enum rawEnum
                ? (rawEnum is TEnum castEnum
                    ? castEnum
                    : throw new ArgumentException(
                        $"Type mismatch. The enum value '{rawEnum.GetType().FullName}' is not of the expected type '{typeof(TEnum).FullName}'.",
                        paramName))
                : Enum.Parse<TEnum>(value.ToString(System.Globalization.CultureInfo.InvariantCulture), ignoreCase: true);

            if (!Enum.IsDefined<TEnum>(parsedEnum))
            {
                throw new ArgumentOutOfRangeException(
                    paramName,
                    message ?? $"The value '{parsedEnum}' is not defined in enum '{typeof(TEnum).FullName}'.");
            }
        }

        /// <summary>
        /// Validates that the specified value corresponds to a defined value of the specified enumeration type, and
        /// throws an exception if it does not.
        /// </summary>
        /// <remarks>Use this method to ensure that a value is a valid member of a specific enum type
        /// before using it in code that requires a defined enum value. This is especially useful when working with
        /// values from untrusted sources or deserialization.</remarks>
        /// <typeparam name="TEnum">The enumeration type against which to validate the value. Must be a struct that implements Enum.</typeparam>
        /// <param name="value">The value to validate. Can be an enum value or a convertible value representing an enum member.</param>
        /// <param name="others">A list of valid enum values that <paramref name="value"/> must match.</param>
        /// <param name="paramName">The name of the parameter being validated. This value is used in any thrown exception to identify the
        /// invalid argument. Optional.</param>
        /// <param name="message">An optional exception message.</param>
        /// <exception cref="ArgumentException">Thrown if the provided value is an enum of a different type than <typeparamref name="TEnum"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the provided value does not correspond to a defined member of <typeparamref name="TEnum"/>.</exception>
        public static void ThrowIfEnumIsNotEqual<TEnum>(IConvertible value, IEnumerable<TEnum> others, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null) where TEnum : struct, Enum
        {
            ArgumentNullException.ThrowIfNull(value, paramName);
            ArgumentNullException.ThrowIfNull(others, nameof(others));

            foreach (TEnum other in others)
            {
                if (!value.Equals(other))
                {
                    throw new ArgumentOutOfRangeException(
                        paramName,
                        message ?? $"The value '{value}' is not equal to '{other}' in enum '{typeof(TEnum).FullName}'.");
                }
            }
        }

        /// <summary>
        /// Throws an exception if the specified type does not match the expected type.
        /// </summary>
        /// <param name="value">The type to validate. Cannot be null.</param>
        /// <param name="other">The expected type to compare against. Cannot be null.</param>
        /// <param name="paramName">The name of the parameter representing the type to validate. This value is typically provided automatically
        /// and should not be set explicitly.</param>
        /// <param name="message">An optional exception message.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not equal to <paramref name="other"/>.</exception>
        public static void ThrowIfNotOfType(Type value, Type other, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null)
        {
            ArgumentNullException.ThrowIfNull(value, paramName);
            ArgumentNullException.ThrowIfNull(other, nameof(other));
            if (value != other)
            {
                throw new ArgumentException(
                    message ?? $"The type '{value.FullName}' is not of the expected type '{other.FullName}'.",
                    paramName);
            }
        }

        /// <summary>
        /// Throws an exception if the specified type is not assignable to the target type.
        /// </summary>
        /// <param name="value">The type to validate for assignability. Cannot be null.</param>
        /// <param name="target">The target type to check assignability against. Cannot be null.</param>
        /// <param name="paramName">The name of the parameter representing the type to validate. This value is typically provided automatically
        /// and should not be set explicitly in most cases.</param>
        /// <param name="message">An optional exception message.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not assignable to <paramref name="target"/>.</exception>
        public static void ThrowIfNotAssignableTo(Type value, Type target, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null)
        {
            ArgumentNullException.ThrowIfNull(value, paramName);
            ArgumentNullException.ThrowIfNull(target, nameof(target));
            if (!value.IsAssignableTo(target))
            {
                throw new ArgumentException(
                    message ?? $"The type '{value.FullName}' is not assignable to the type '{target.FullName}'.",
                    paramName);
            }
        }
    }

    public class ArgumentOutOfRangeExceptionEx : System.ArgumentOutOfRangeException
    {
        public ArgumentOutOfRangeExceptionEx()
        {
        }

        public ArgumentOutOfRangeExceptionEx(string paramName) : base(paramName)
        {
        }

        public ArgumentOutOfRangeExceptionEx(string paramName, string message) : base(paramName, message)
        {
        }

        public ArgumentOutOfRangeExceptionEx(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ArgumentOutOfRangeExceptionEx(string paramName, object actualValue, string message) : base(paramName, actualValue, message)
        {
        }
    }
}
