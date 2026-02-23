namespace BionicCode.Utilities.Net;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

/// <summary>
/// Represents an exception that is thrown when a <see langword="null"/> argument is passed to a method that does not accept it, with
/// additional static methods for argument validation.
/// </summary>
/// <remarks> Use<see cref="ArgumentNullExceptionAdvanced"/> to perform advanced argument validation scenarios, such as
/// checking for default struct values or empty enumerables, in addition to standard <see langword="null"/> checks. This class extends
///<see cref="ArgumentNullException"/>and provides static helper methods to simplify common validation patterns.</remarks>
public class ArgumentNullExceptionAdvanced : System.ArgumentNullException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentNullExceptionAdvanced"/> class.
    /// </summary>
    public ArgumentNullExceptionAdvanced()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentNullExceptionAdvanced"/> class with the name of the parameter that caused
    /// the exception.
    /// </summary>
    /// <param name="paramName">The name of the parameter that is<see langword="null"/>and caused the exception.</param>
    public ArgumentNullExceptionAdvanced(string paramName) : base(paramName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentNullExceptionAdvanced"/> class with a specified error message and a
    /// reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a <see langword="null"/> reference if no inner exception is
    /// specified.</param>
    public ArgumentNullExceptionAdvanced(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentNullExceptionAdvanced"/> class with a specified parameter name and error
    /// message.
    /// </summary>
    /// <param name="paramName">The name of the parameter that caused the exception.</param>
    /// <param name="message">The message that describes the error.</param>
    public ArgumentNullExceptionAdvanced(string paramName, string message) : base(paramName, message)
    {
    }

    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the specified value is equal to the default value of its type.
    /// </summary>
    /// <remarks>Use this method to ensure that a value type parameter has been initialized and is not
    /// equal to its default value. This is useful for validating struct parameters where the default value may be
    /// invalid or unintended.</remarks>
    /// <typeparam name="TStruct">The value type to check for the <see langword="default"/> value. Must be a struct.</typeparam>
    /// <param name="value">The value to validate against its default value.</param>
    /// <param name="paramName">The name of the parameter to include in the exception message. This value is typically provided
    /// automatically and should not be set explicitly.</param>
    /// <param name="message">The message that describes the error.</param>
    public static void ThrowIfDefault<TStruct>(TStruct value, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null) where TStruct : struct
    {
        if (EqualityComparer<TStruct>.Default.Equals(value, default))
        {
            throw new ArgumentNullException(
                paramName,
                message ?? "The argument is equal to the default value of its value type.");
        }
    }

    /// <summary>
    /// Throws an exception if the specified span is <see langword="default"/> (which is <see cref="ReadOnlySpan{T}.Empty"/>), indicating that a required value was not provided.
    /// </summary>
    /// <typeparam name="TStruct">The value type of the elements in the span to check.</typeparam>
    /// <param name="value">The span of value type elements to validate. The method throws if this span is <see langword="default"/> (which is <see cref="ReadOnlySpan{T}.Empty"/>).</param>
    /// <param name="paramName">The name of the parameter being validated. Used in the exception message to identify the argument. Optional.</param>
    /// <param name="message">An optional custom message to include in the exception if the span is <see langword="default"/> (which is <see cref="ReadOnlySpan{T}.Empty"/>).</param>
    /// <exception cref="ArgumentNullExceptionAdvanced">Thrown if <paramref name="value"/> is <see langword="default"/> (which is <see cref="ReadOnlySpan{T}.Empty"/>).</exception>
    public static void ThrowIfDefault<TStruct>(ReadOnlySpan<TStruct> value, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null) where TStruct : struct
    {
        if (value.IsEmpty)
        {
            throw new ArgumentNullException(
                paramName,
                message ?? "The argument must not be empty.");
        }
    }

    /// <summary>
    /// Throws an exception if the specified enumerable is<see langword="null"/>or contains no elements.
    /// </summary>
    /// <remarks>This method is typically used to validate method arguments that are expected to be
    /// non-null and contain at least one element. If the enumerable is <see langword="null"/>, an <see cref="ArgumentNullException"/> is thrown by
    /// <see cref="ArgumentNullException.ThrowIfNull(object, string)"/>.</remarks>
    /// <param name="value">The enumerable to validate. Cannot be <see langword="null"/>.</param>
    /// <param name="paramName">The name of the parameter representing the enumerable. This value is used in the exception message if an
    /// exception is thrown.</param>
    /// <param name="message">The custom error message to include in the exception if the enumerable is empty. If <see langword="null"/>, a default message
    /// is used.</param>
    /// <exception cref="ArgumentException">Thrown if the enumerable is empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the enumerable is <see langword="null"/><see langword="null"/>.</exception>"
    public static void ThrowIfNullOrEmpty([NotNull] IEnumerable value, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null)
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
    /// Throws an<see cref="ArgumentException"/>if the specified value is not <see langword="null"/>.
    /// </summary>
    /// <param name="value">The object to check for <see langword="null"/>. No exception is thrown if this value is <see langword="null"/>.</param>
    /// <param name="paramName">The name of the parameter being checked. This value is used in the exception message. If not specified, the
    /// caller argument expression is used.</param>
    /// <param name="message">The custom error message to include in the exception. If <see langword="null"/>, a default message is used.</param>
    /// <exception cref="ArgumentException">Thrown if value is not <see langword="null"/>.</exception>
    public static void ThrowIfNotNull<TValue>(TValue value, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null)
    {
        if (value is not null)
        {
            throw new ArgumentException(
                message ?? $"The argument must be 'null'. Allowed: 'null', Found: '{value.GetType().FullName}'",
                paramName);
        }
    }

    /// <summary>
    /// Throws an<see cref="ArgumentException"/>if the specified value is a reference type and <see langword="null"/>.
    /// </summary>
    /// <param name="value">The object to validate for <see langword="null"/>. If this value is <see langword="null"/>, an exception is thrown.</param>
    /// <param name="paramName">The name of the parameter being validated. This value is used in the exception message to identify the
    /// parameter. If not specified, the caller argument expression is used.</param>
    /// <param name="message">An optional custom message to include in the exception. If <see langword="null"/>, a default message is used.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static void ThrowIfNull<TValue>([NotNull] TValue value, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null)
    {
        if (value is null)
        {
            throw new ArgumentNullException(
                paramName,
                message ?? "The argument must not be 'null'.");
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the specified value is a reference type and <see langword="null"/> or otherwise returns the value.
    /// </summary>
    /// <param name="value">The object to validate for <see langword="null"/>. If this value is <see langword="null"/>, an exception is thrown.</param>
    /// <param name="paramName">The name of the parameter being validated. This value is used in the exception message to identify the
    /// parameter. If not specified, the caller argument expression is used.</param>
    /// <param name="message">An optional custom message to include in the exception. If <see langword="null"/>, a default message is used.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <returns>The validated value if it is not <see langword="null"/>.</returns>
    [return: NotNull]
    public static TValue ThrowIfNullOrReturn<TValue>([NotNull] TValue value, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null)
    {
        if (value is null)
        {
            throw new ArgumentNullException(
                paramName,
                message ?? "The argument must not be 'null'.");
        }

        return value;
    }

    /// <summary>
    /// Validates that the specified value is not <see langword="null"/>, and throws an <see cref="ArgumentNullException"/> if it is. Returns true if the value is not <see langword="null"/>.
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <param name="message">An optional custom message to include in the exception.</param>
    /// <returns><see langword="true"/> if the value is not <see langword="null"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsNotNullOrThrow<TValue>([NotNull] TValue value, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null)
    {
        if (value is null)
        {
            throw new ArgumentNullException(
                paramName,
                message ?? "The argument must not be 'null'.");
        }

        return true;
    }
}

/// <summary>
/// Represents an exception that is thrown when an argument does not meet the requirements of a method, providing
/// additional static validation utilities for argument checking.
/// </summary>
/// <remarks>Use <see cref="ArgumentExceptionAdvanced"/> to perform advanced argument validation scenarios, such as verifying
/// delegate compatibility with events, enum value validity, or type assignability. The static methods in this class
/// throw appropriate exceptions when validation fails, helping to enforce correct usage of method parameters and
/// improve error reporting. This class extends<see cref="ArgumentException"/>to provide more specialized argument validation
/// patterns commonly needed in application and library development.</remarks>
public class ArgumentExceptionAdvanced : ArgumentException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentExceptionAdvanced"/> class.
    /// </summary>
    public ArgumentExceptionAdvanced()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentExceptionAdvanced"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ArgumentExceptionAdvanced(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentExceptionAdvanced"/> class with a specified error message and a reference
    /// to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a<see langword="null"/>reference if no inner exception is
    /// specified.</param>
    public ArgumentExceptionAdvanced(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentExceptionAdvanced"/> class with a specified error message and the name of
    /// the parameter that caused this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="paramName">The name of the parameter that caused the exception.</param>
    public ArgumentExceptionAdvanced(string message, string paramName) : base(message, paramName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentExceptionAdvanced"/> class with a specified error message, the name of the
    /// parameter that caused the exception, and a reference to the inner exception that is the cause of this
    /// exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="paramName">The name of the parameter that caused the current exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a<see langword="null"/>reference if no inner exception is
    /// specified.</param>
    public ArgumentExceptionAdvanced(string message, string paramName, Exception innerException) : base(message, paramName, innerException)
    {
    }

    /// <summary>
    /// Validates that the specified delegate is compatible with the signature of the given event. Throws an exception
    /// if the delegate cannot be assigned as an event handler.
    /// </summary>
    /// <remarks>Use this method to ensure that a delegate can be safely attached to an event at runtime. This
    /// validation checks that the number and types of parameters in the delegate match those expected by the event.
    /// This method does not check for<see langword="null"/>arguments; callers should ensure arguments are not<see langword="null"/>before
    /// calling.</remarks>
    /// <param name="targetEvent">The event metadata that defines the expected event handler signature. Cannot be <see langword="null"/>.</param>
    /// <param name="clientHandler">The delegate to validate as a potential event handler for the event. Cannot be <see langword="null"/>.</param>
    /// <param name="paramName"></param>
    /// <param name="message"></param>
    /// <exception cref="EventHandlerMismatchException">Thrown if the delegate's signature does not match the event handler type required by the event.</exception>
    public static void ThrowIfEventHandlerNotAssignable([NotNull] Delegate clientHandler, [NotNull] EventInfo targetEvent, [CallerArgumentExpression(nameof(clientHandler))] string? paramName = null, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(targetEvent);
        ArgumentNullException.ThrowIfNull(clientHandler, paramName);

        Type eventType = targetEvent.EventHandlerType!;

        MethodInfo eventDelegateInvokeMethod = eventType.GetMethod(ReflectionConstants.DelegateInvocatorMethodName)!;
        ParameterInfo[] eventDelegateParameters = eventDelegateInvokeMethod.GetParameters();

        MethodInfo clientHandlerMethod = clientHandler.Method;
        ParameterInfo[] clientHandlerParameters = clientHandlerMethod.GetParameters();

        /* Validate the event EventHandler */

        if (eventDelegateParameters.Length != clientHandlerParameters.Length)
        {
            throw new EventHandlerMismatchException(message ?? ExceptionMessages.GetHandlerDelegateSignatureMismatchExceptionMessage(targetEvent, clientHandlerMethod, "Invalid parameter count."));
        }

        for (int parameterIndex = 0; parameterIndex < eventDelegateParameters.Length; parameterIndex++)
        {
            Type eventDelegateParameterType = eventDelegateParameters[parameterIndex].ParameterType;
            Type eventHandlerParameterType = clientHandlerParameters[parameterIndex].ParameterType;
            if (!eventHandlerParameterType.IsAssignableFrom(eventDelegateParameterType))
            {
                string exceptionMessage = message ?? ExceptionMessages.GetHandlerDelegateSignatureMismatchExceptionMessage(
                    targetEvent,
                    clientHandlerMethod,
                    $"The parameter '{paramName}' is incompatible with the event {eventType.ToFullyQualifiedSignatureName()}. Reason: Unable to cast parameter of type '{eventDelegateParameterType.ToFullyQualifiedSignatureName()}' at parameter index '{parameterIndex}' of the event delegate to type '{eventHandlerParameterType.ToFullyQualifiedSignatureName()}' of the event handler.");
                throw new EventHandlerMismatchException(exceptionMessage);
            }
        }
    }

    /// <summary>
    /// Validates that the specified event handler delegate is compatible with the target event's signature, and
    /// throws an exception if the handler cannot be assigned to the event.
    /// </summary>
    /// <remarks>This method checks both the number and types of parameters in the event handler
    /// delegate against the target event's expected signature. Use this method to ensure that event handler
    /// assignment will succeed at runtime and to provide clear error reporting when mismatches occur.</remarks>
    /// <param name="clientHandler">The delegate to validate as an event handler. Cannot be <see langword="null"/>.</param>
    /// <param name="targetEvent">The event metadata describing the target event whose handler signature is to be validated. Cannot be <see langword="null"/>.</param>
    /// <param name="paramName">The name of the parameter representing the event handler delegate, used in exception messages for clarity.
    /// Optional.</param>
    /// <param name="message">A custom error message to include in the exception if the handler is not assignable. If <see langword="null"/>, a default
    /// message is used.</param>
    /// <exception cref="EventHandlerMismatchException">Thrown if the event handler delegate does not match the target event's signature, such as parameter count or
    /// type incompatibility.</exception>
    internal static void ThrowIfEventHandlerNotAssignable([NotNull] Delegate clientHandler, [NotNull] EventData targetEvent, [CallerArgumentExpression(nameof(clientHandler))] string? paramName = null, string? message = null)
    {
        // TODO::Make this method public (requires EventData to be public too)

        ArgumentNullException.ThrowIfNull(targetEvent);
        ArgumentNullException.ThrowIfNull(clientHandler, paramName);

        MethodData eventDelegateInvokeMethod = targetEvent.EventInvokerMethodData;
        ParameterList eventDelegateParameters = eventDelegateInvokeMethod.Parameters;

        MethodInfo clientHandlerMethod = clientHandler.Method;
        ParameterInfo[] clientHandlerParameters = clientHandlerMethod.GetParameters();

        /* Validate the event EventHandler */

        if (eventDelegateParameters.Count != clientHandlerParameters.Length)
        {
            throw new EventHandlerMismatchException(message ?? ExceptionMessages.GetHandlerDelegateSignatureMismatchExceptionMessage(
                targetEvent.GetEventInfo(),
                clientHandlerMethod,
                "Invalid parameter count."));
        }

        for (int parameterIndex = 0; parameterIndex < eventDelegateParameters.Count; parameterIndex++)
        {
            Type eventDelegateParameterType = eventDelegateParameters[parameterIndex].ParameterTypeData.Type;
            Type eventHandlerParameterType = clientHandlerParameters[parameterIndex].ParameterType;
            if (!eventHandlerParameterType.IsAssignableFrom(eventDelegateParameterType))
            {
                string exceptionMessage = message ?? ExceptionMessages.GetHandlerDelegateSignatureMismatchExceptionMessage(
                    targetEvent.GetEventInfo(),
                    clientHandlerMethod,
                    $"The parameter '{paramName}' is incompatible with the event {targetEvent.FullyQualifiedSignature}. Reason: Unable to cast parameter of type '{eventDelegateParameterType.ToFullyQualifiedSignatureName()}' at parameter index '{parameterIndex}' of the event delegate to type '{eventHandlerParameterType.ToFullyQualifiedSignatureName()}' of the event handler.");
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
    /// <param name="value">The value to validate. Can be an enum value or a convertible value representing an enum member (e.g. an <see langword="int"/> or <see langword="string"/> value).</param>
    /// <param name="paramName">The name of the parameter being validated. This value is used in any thrown exception to identify the
    /// invalid argument. Optional.</param>
    /// <param name="message">An optional exception message.</param>
    /// <exception cref="ArgumentException">Thrown if the provided value is an enum of a different type than <typeparamref name="TEnum"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the provided value does not correspond to a defined member of <typeparamref name="TEnum"/>.</exception>
    public static void ThrowIfEnumIsNotDefined<TEnum>([NotNull] IConvertible value, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null) where TEnum : struct, Enum
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(value, paramName);

        TEnum parsedEnum = ParseEnumValue<TEnum>(value, paramName);

        if (!Enum.IsDefined<TEnum>(parsedEnum))
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                message ?? $"The value '{parsedEnum}' of argument '{paramName}' is not defined in enum '{typeof(TEnum).FullName}'.");
        }
    }

    /// <summary>
    /// Throws an exception if the specified enum value does not match any of the provided allowed values.
    /// </summary>
    /// <remarks>Use this method to enforce that an enum argument matches one of a set of allowed
    /// values. This is useful for validating method parameters or configuration values at runtime.</remarks>
    /// <typeparam name="TEnum">The enum type to compare against. Must be a value type that implements <see cref="System.Enum"/>.</typeparam>
    /// <param name="value">The enum value to validate. Cannot be null.</param>
    /// <param name="allowedValues">A collection of allowed enum values to compare against. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="paramName">The name of the parameter to include in the exception message. This is typically provided automatically and
    /// is optional.</param>
    /// <param name="message">An optional custom message to include in the exception if the value is not allowed.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> does not equal any of the allowed values in <paramref name="allowedValues"/>.</exception>
    public static void ThrowIfEnumNotEqualsAny<TEnum>(IConvertible value, ReadOnlySpan<TEnum> allowedValues, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null) where TEnum : struct, Enum
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(value, paramName);
        ArgumentExceptionAdvanced.ThrowIfTrue(
            allowedValues.IsEmpty,
            nameof(allowedValues),
            "The collection of allowed values cannot be empty.");

        TEnum parsedEnum = ParseEnumValue<TEnum>(value, paramName);

        EqualityComparer<TEnum> equalityComparer = EqualityComparer<TEnum>.Default;
        foreach (TEnum other in allowedValues)
        {
            if (equalityComparer.Equals(parsedEnum, other))
            {
                return;
            }
        }

        if (message is null)
        {
            var disallowedValues = Enum.GetValues<TEnum>()
                .Except(allowedValues.ToArray())
                .ToList();

            Type enumType = typeof(TEnum);
            string fullyQualifiedEnumTypeName = enumType.FullName ?? enumType.Name;
            string messageStart = $"The argument '{paramName}' is not one of the allowed values. Found: '{parsedEnum}'.";
            message = disallowedValues.Count <= allowedValues.Length
                ? $"{messageStart} Disallowed: {disallowedValues.JoinToString(value => $"{fullyQualifiedEnumTypeName}.{value.ToString()}", ", ")}."
                : $"{messageStart} Allowed: {allowedValues.JoinToString(value => $"{fullyQualifiedEnumTypeName}.{value.ToString()}", ", ")}.";
        }

        throw new ArgumentOutOfRangeException(paramName, message);
    }

    /// <summary>
    /// Throws an exception if the specified enum value is equal to any of the provided disallowed values.
    /// </summary>
    /// <typeparam name="TEnum">The enum type to check against the disallowed values.</typeparam>
    /// <param name="value">The enum value to validate. Cannot be <see langword="null"/>.</param>
    /// <param name="disallowedValues">A collection of enum values that are not allowed. Cannot be <see langword="null"/>.</param>
    /// <param name="paramName">The name of the parameter representing the value being checked. This is used in the exception message.</param>
    /// <param name="message">An optional custom message to include in the exception. If <see langword="null"/>, a default message is used.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is equal to any of the disallowed values defined  in <paramref name="disallowedValues"/>.</exception>
    public static void ThrowIfEnumEqualsAny<TEnum>(IConvertible value, ReadOnlySpan<TEnum> disallowedValues, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null) where TEnum : struct, Enum
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(value, paramName);
        ArgumentExceptionAdvanced.ThrowIfTrue(
            disallowedValues.IsEmpty,
            nameof(disallowedValues),
            "The collection of disallowed values cannot be empty.");

        TEnum parsedEnum = ParseEnumValue<TEnum>(value, paramName);

        EqualityComparer<TEnum> equalityComparer = EqualityComparer<TEnum>.Default;
        foreach (TEnum other in disallowedValues)
        {
            if (equalityComparer.Equals(parsedEnum, other))
            {
                if (message is null)
                {
                    var allowedValues = Enum.GetValues<TEnum>()
                        .Except(disallowedValues.ToArray())
                        .ToList();

                    Type enumType = typeof(TEnum);
                    string fullyQualifiedEnumTypeName = enumType.FullName ?? enumType.Name;
                    string messageStart = $"The argument '{paramName}' is one of the disallowed values. Found: '{parsedEnum}'.";
                    message = disallowedValues.Length <= allowedValues.Count
                        ? $"{messageStart} Disallowed: {disallowedValues.JoinToString(value => $"{fullyQualifiedEnumTypeName}.{value.ToString()}", ", ")}."
                        : $"{messageStart} Allowed: {allowedValues.JoinToString(value => $"{fullyQualifiedEnumTypeName}.{value.ToString()}", ", ")}.";
                }

                throw new ArgumentOutOfRangeException(paramName, message);
            }
        }
    }

    private static TEnum ParseEnumValue<TEnum>(IConvertible value, string? paramName) where TEnum : struct, Enum
    {
        TEnum result;

        if (value is Enum rawEnum)
        {
            result = rawEnum is TEnum castEnum
                ? castEnum
                : throw new ArgumentException(
                    $"Type mismatch. The enum value '{rawEnum.GetType().FullName}' is not of the expected type '{typeof(TEnum).FullName}'.",
                    paramName);
        }
        else
        {
            if (value.ToString(CultureInfo.InvariantCulture) is string stringValue)
            {
                try
                {
                    result = Enum.Parse<TEnum>(stringValue, ignoreCase: true);
                }
                catch (Exception e)
                    when (e is ArgumentException
                        or ArgumentNullException
                        or InvalidOperationException
                        or OverflowException
                        or FormatException)
                {
                    throw new ArgumentException($"The argument {paramName} is not a valid enum value.", paramName, e);
                }
            }
            else
            {
                ITypeDataView iConvertibleTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(typeof(IConvertible));
                const string iConvertibleToStringMethodName = nameof(IConvertible.ToString);
                MethodData? iConvertibleToStringMethodData = iConvertibleTypeData.Methods.TryGetMethodsByName(iConvertibleToStringMethodName, out MethodList methods) && methods.HasItems
                    ? methods[0]
                    : null;

                throw new ArgumentException(
                    $"Invalid value. The '{iConvertibleToStringMethodData?.FullyQualifiedSignature ?? $"{nameof(IConvertible.ToString)}.{iConvertibleToStringMethodName}"}' conversion of the argument '{paramName}' returned 'null'.",
                    paramName);
            }
        }

        return result;
    }

    /// <summary>
    /// Throws an exception if the specified type does not match the expected type.
    /// </summary>
    /// <param name="value">The type to validate. Cannot be <see langword="null"/>.</param>
    /// <param name="other">The expected type to compare against. Cannot be <see langword="null"/>.</param>
    /// <param name="paramName">The name of the parameter representing the type to validate. This value is typically provided automatically
    /// and should not be set explicitly.</param>
    /// <param name="message">An optional exception message.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not equal to <paramref name="other"/>.</exception>
    public static void ThrowIfNotEqualsType([NotNull] Type value, [NotNull] Type other, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        ArgumentNullException.ThrowIfNull(other);
        if (value != other)
        {
            throw new ArgumentException(
                message ?? $"The provided type is not of the expected type. Allowed: '{other.FullName}', Found: '{value.FullName}'.",
                paramName);
        }
    }

    /// <summary>
    /// Throws an exception if the specified type is not assignable to the target type.
    /// </summary>
    /// <param name="value">The type to validate for assignability. Cannot be <see langword="null"/>.</param>
    /// <param name="target">The target type to check assignability against. Cannot be <see langword="null"/>.</param>
    /// <param name="paramName">The name of the parameter representing the type to validate. This value is typically provided automatically
    /// and should not be set explicitly in most cases.</param>
    /// <param name="message">An optional exception message.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not assignable to <paramref name="target"/>.</exception>
    public static void ThrowIfNotAssignableTo([NotNull] Type value, [NotNull] Type target, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        ArgumentNullException.ThrowIfNull(target);
        if (!value.IsAssignableTo(target))
        {
            throw new ArgumentException(
                message ?? $"The provided type is not assignable to the expected type. Allowed: '{target.FullName}', Found: '{value.FullName}'.",
                paramName);
        }
    }

    /// <summary>
    /// Throws an exception if the specified type is not assignable to the target type.
    /// </summary>
    /// <param name="value">The type to validate for assignability. Cannot be <see langword="null"/>.</param>
    /// <param name="target">The target type to check assignability against. Cannot be <see langword="null"/>.</param>
    /// <param name="paramName">The name of the parameter representing the type to validate. This value is typically provided automatically
    /// and should not be set explicitly in most cases.</param>
    /// <param name="message">An optional exception message.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not assignable to <paramref name="target"/>.</exception>
    public static void ThrowIfNotAssignableTo<TValue, TTarget>([NotNull] TValue value, [NotNull] TTarget target, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        ArgumentNullException.ThrowIfNull(target);

        Type targetType = target.GetType();
        Type valueType = value.GetType();
        if (!valueType.IsAssignableTo(targetType))
        {
            throw new ArgumentException(
                message ?? $"The provided type is not assignable to the expected type. Allowed: '{targetType.FullName}', Found: '{valueType.FullName}'.",
                paramName);
        }
    }

    /// <summary>
    /// Throws an exception if the specified type is not assignable to the target type.
    /// </summary>
    /// <typeparam name="TValue">The type to validate for assignability. Cannot be <see langword="null"/>.</typeparam>
    /// <typeparam name="TTarget">The target type to check assignability against. Cannot be <see langword="null"/>.</typeparam>
    /// <param name="message">An optional exception message.</param>
    /// <exception cref="ArgumentException">Thrown if <typeparamref name="TValue"/> is not assignable to <typeparamref name="TTarget"/>.</exception>
    public static void ThrowIfNotAssignableTo<TValue, TTarget>(string? message = null)
    {
        Type targetType = typeof(TTarget);
        Type valueType = typeof(TValue);
        if (!valueType.IsAssignableTo(targetType))
        {
            throw new ArgumentException(
                message ?? $"The provided type is not assignable to the expected type. Allowed: '{targetType.FullName}', Found: '{valueType.FullName}'.",
                string.Empty);
        }
    }

    /// <summary>
    /// Throws an exception if the specified type is not assignable to the target type.
    /// </summary>
    /// <param name="value">The type to validate for assignability. Cannot be <see langword="null"/>.</param>
    /// <param name="paramName">The name of the parameter representing the type to validate. This value is typically provided automatically
    /// and should not be set explicitly in most cases.</param>
    /// <typeparam name="TTarget"> The target type to check assignability against. Cannot be <see langword="null"/>.</typeparam>
    /// <param name="message">An optional exception message.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not assignable to <typeparamref name="TTarget"/>.</exception>
    public static void ThrowIfNotAssignableTo<TTarget>([NotNull] object? value, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        Type targetType = typeof(TTarget);
        Type valueType = value.GetType();
        if (!valueType.IsAssignableTo(targetType))
        {
            throw new ArgumentException(
                message ?? $"The provided type is not assignable to the expected type. Allowed: '{targetType.FullName}', Found: '{valueType.FullName}'.",
                paramName);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the specified condition is false.
    /// </summary>
    /// <param name="value">The condition to evaluate. If <see langword="false"/>, an exception is thrown.</param>
    /// <param name="paramName">The name of the parameter or expression that failed the condition. If not specified, the caller argument
    /// expression is used.</param>
    /// <param name="message">The error message to include in the exception. If null, a default message is used.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is <see langword="false"/>.</exception>
    public static void ThrowIfFalse(bool value, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null)
    {
        if (!value)
        {
            throw new ArgumentException(
                message ?? "The condition is 'FALSE'. Allowed: 'TRUE'.",
                paramName);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the specified condition is <see langword="true"/>.
    /// </summary>
    /// <param name="value">The condition to evaluate. If <see langword="true"/>, an exception is thrown.</param>
    /// <param name="paramName">The name of the parameter that caused the exception. This value is typically provided automatically by the
    /// compiler.</param>
    /// <param name="message">The error message to include in the exception. If <see langword="null"/>, a default message is used.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is <see langword="true"/>.</exception>
    public static void ThrowIfTrue(bool value, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null)
    {
        if (value)
        {
            throw new ArgumentException(
                message ?? "The condition is 'TRUE'. Allowed: 'FALSE'.",
                paramName);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if any element in the sequence satisfies the specified condition.
    /// </summary>
    /// <typeparam name="TItem">The type of the elements in the sequence to check.</typeparam>
    /// <param name="items">The sequence of items to evaluate against the condition. Cannot be <see langword="null"/>.</param>
    /// <param name="condition">A predicate function that defines the condition to test for each element. Cannot be <see langword="null"/>.</param>
    /// <param name="paramName">The name of the parameter that caused the exception. If not specified, the expression for the condition is
    /// used.</param>
    /// <param name="message">The error message to include in the exception. If <see langword="null"/>, a default message is used.</param>
    /// <exception cref="ArgumentException">Thrown if any element in the sequence satisfies the specified condition.</exception>
    public static void ThrowIfAny<TItem>(IEnumerable<TItem> items, Func<TItem, bool> condition, [CallerArgumentExpression(nameof(items))] string? paramName = null, string? message = null)
    {
        if (items.Any(condition))
        {
            throw new ArgumentException(
                message ?? "The sequence contains  invalid items.",
                paramName);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the specified sequence contains duplicate items, using the provided equality comparer.
    /// </summary>
    /// <typeparam name="TItem">The type of the elements in the sequence to check.</typeparam>
    /// <param name="items">The sequence of items to evaluate for duplicates. Cannot be <see langword="null"/>.</param>
    /// <param name="equalityComparer">The equality comparer to use for determining duplicates. If <see langword="null"/>, the default equality comparer is used.</param>
    /// <param name="paramName">The name of the parameter that caused the exception. If not specified, the expression for the items is used.</param>
    /// <param name="message">The error message to include in the exception. If <see langword="null"/>, a default message is used.</param>
    /// <exception cref="ArgumentException">Thrown if the sequence contains duplicate items.</exception>
    public static void ThrowIfContainsDuplicate<TItem>(IEnumerable<TItem> items, IEqualityComparer<TItem>? equalityComparer = null, [CallerArgumentExpression(nameof(items))] string? paramName = null, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(items);
        equalityComparer ??= EqualityComparer<TItem>.Default;

        var set = new HashSet<TItem>(equalityComparer);
        foreach (TItem item in items)
        {
            if (!set.Add(item))
            {
                throw new ArgumentException(
                    message ?? "The sequence contains duplicate items.",
                    paramName);
            }
        }
    }
}

/// <summary>
/// Represents an exception that is thrown when the value of an argument is outside the allowable range, providing
/// additional context or customization beyond the standard <see cref="ArgumentOutOfRangeException"/>
/// </summary>
/// <remarks>Use <see cref="ArgumentOutOfRangeExceptionAdvanced"/> to signal that a method argument falls outside the expected
/// range and to provide enhanced or customized exception details. This class extends <see cref="ArgumentOutOfRangeException"/>
/// and can be used in scenarios where more specific exception handling or messaging is required.</remarks>
public class ArgumentOutOfRangeExceptionAdvanced : System.ArgumentOutOfRangeException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentOutOfRangeExceptionAdvanced"/> class.
    /// </summary>
    public ArgumentOutOfRangeExceptionAdvanced()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentOutOfRangeExceptionAdvanced"/> class with the name of the parameter that
    /// caused the exception.
    /// </summary>
    /// <param name="paramName">The name of the parameter that caused the exception.</param>
    public ArgumentOutOfRangeExceptionAdvanced(string paramName) : base(paramName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentOutOfRangeExceptionAdvanced"/> class with a specified parameter name and
    /// error message.
    /// </summary>
    /// <param name="paramName">The name of the parameter that caused the exception.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public ArgumentOutOfRangeExceptionAdvanced(string paramName, string message) : base(paramName, message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentOutOfRangeExceptionAdvanced"/> class with a specified error message and a
    /// reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a<see langword="null"/>reference if no inner exception is
    /// specified.</param>
    public ArgumentOutOfRangeExceptionAdvanced(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentOutOfRangeExceptionAdvanced"/> class with the name of the parameter that
    /// caused the exception, the actual value of the argument, and a specified error message.
    /// </summary>
    /// <param name="paramName">The name of the parameter that caused the exception. Cannot be <see langword="null"/>.</param>
    /// <param name="actualValue">The actual value of the argument that caused the exception. This value is typically outside the allowable
    /// range of values as defined by the invoked method.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public ArgumentOutOfRangeExceptionAdvanced(string paramName, object actualValue, string message) : base(paramName, actualValue, message)
    {
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the specified value is less than the provided comparison value.
    /// </summary>
    /// <typeparam name="TValue">The type of the values to compare. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="value">The value to validate against the comparison value.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>. <paramref name="value"/> must not be less than this
    /// value.</param>
    /// <param name="paramName">The name of the parameter representing <paramref name="value"/>. Used in the exception message if an
    /// exception is thrown.</param>
    /// <param name="message">An optional custom error message for the exception. If <see langword="null"/>, a default message is used.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is less than <paramref name="other"/>.</exception>
    public static void ThrowIfLessThan<TValue>([NotNull] TValue value, [NotNull] TValue other, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null)
        where TValue : IComparable<TValue>
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        ArgumentNullException.ThrowIfNull(other);
        if (value.CompareTo(other) < 0)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                value,
                message ?? $"Value is less than the expected value. Allowed: {paramName} < {other}, Found: '{value}'");
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the specified value is not equal to the expected value.
    /// </summary>
    /// <remarks>Both <paramref name="value"/> and <paramref name="other"/> must not be <see langword="null"/>. Equality
    /// is determined using <see cref="EqualityComparer{T}.Default"/>.</remarks>
    /// <typeparam name="TValue">The type of the values to compare.</typeparam>
    /// <param name="value">The value to validate for equality.</param>
    /// <param name="other">The value to compare against the validated value.</param>
    /// <param name="paramName">The name of the parameter representing the value being validated. Used in the exception message if thrown.</param>
    /// <param name="message">The custom error message to include in the exception if the values are not equal. If <see langword="null"/>, a default message
    /// is used.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not equal to <paramref name="other"/>.</exception>
    public static void ThrowIfNotEqual<TValue>([NotNull] TValue value, [NotNull] TValue other, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        ArgumentNullException.ThrowIfNull(other);
        if (!EqualityComparer<TValue>.Default.Equals(value, other))
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                value,
                message ?? $"Value is not equal to the expected value. Allowed: {paramName} = {other}, Found: '{value}'");
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the specified value is negative.
    /// </summary>
    /// <typeparam name="TValue">The numeric type of the value to check. Must implement <see cref="INumberBase{TValue}"/>.</typeparam>
    /// <param name="value">The value to validate. If this value is negative, an exception is thrown.</param>
    /// <param name="paramName">The name of the parameter that caused the exception. This value is typically provided automatically and is
    /// used in the exception message.</param>
    /// <param name="message">An optional custom message to include in the exception. If <see langword="null"/>, a default message is used.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is negative.</exception>
    public static void ThrowIfNegative<TValue>(TValue value, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null)
        where TValue : INumberBase<TValue>
    {
        if (TValue.IsNegative(value))
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                value,
                message ?? $"The argument must not be negative. Allowed: {paramName} >= 0, Found: '{value}'.");
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the specified value is less than or equal to zero.
    /// </summary>
    /// <typeparam name="TValue">The numeric type of the value to validate. Must implement <see cref="INumberBase{TValue}"/>.</typeparam>
    /// <param name="value">The value to validate. Must be greater than zero.</param>
    /// <param name="paramName">The name of the parameter being validated. This value is used in the exception if one is thrown. If not
    /// specified, the compiler will supply the argument expression.</param>
    /// <param name="message">The error message to include in the exception. If <see langword="null"/>, a default message is used.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is less than or equal to zero.</exception>
    public static void ThrowIfNegativeOrZero<TValue>(TValue value, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null)
        where TValue : INumberBase<TValue>
    {
        if (TValue.IsNegative(value) || TValue.IsZero(value))
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                value,
                message ?? $"The argument must be greater than zero. Allowed: {paramName} > 0, Found: '{value}'.");
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the specified value is equal to a disallowed value.
    /// </summary>
    /// <remarks>Use this method to enforce that a value does not match a specific disallowed value,
    /// such as a sentinel or reserved value. The comparison uses the default equality comparer for the
    /// type.</remarks>
    /// <typeparam name="TValue">The type of the values to compare.</typeparam>
    /// <param name="value">The value to validate against the disallowed value.</param>
    /// <param name="other">The value that is not allowed. If <paramref name="value"/> is equal to this value, an exception is thrown.</param>
    /// <param name="paramName">The name of the parameter that caused the exception. This value is typically provided automatically and
    /// should not be set manually.</param>
    /// <param name="message">The error message to include in the exception. If <see langword="null"/>, a default message is used.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is equal to <paramref name="other"/>.</exception>
    public static void ThrowIfEqual<TValue>(TValue value, TValue other, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null)
    {
        if (EqualityComparer<TValue>.Default.Equals(value, other))
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                value,
                message ?? $"Value is equal to the disallowed value. Allowed: {paramName} != {other}, Found: '{value}'");
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the specified value is greater than the allowed maximum.
    /// </summary>
    /// <typeparam name="TValue">The type of the values to compare. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="value">The value to validate against the maximum allowed value.</param>
    /// <param name="other">The maximum allowed value. If <paramref name="value"/> is greater than this value, an exception is thrown.</param>
    /// <param name="paramName">The name of the parameter representing the value being checked. This is used in the exception message.
    /// Optional.</param>
    /// <param name="message">The custom error message to include in the exception. If <see langword="null"/>, a default message is used. Optional.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is greater than <paramref name="other"/>.</exception>
    public static void ThrowIfGreaterThan<TValue>(TValue value, TValue other, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null)
        where TValue : IComparable<TValue>
    {
        if (value.CompareTo(other) > 0)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                value,
                message ?? $"Value is greater than the allowed maximum. Allowed: {paramName} <= {other}, Found: '{value}'");
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if a specified value is greater than or equal to a given comparison value.
    /// </summary>
    /// <typeparam name="TValue">The type of the values to compare. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="value">The value to validate against the comparison value.</param>
    /// <param name="other">The value to compare against. The method throws if <paramref name="value"/> is greater than or equal to this
    /// value.</param>
    /// <param name="paramName">The name of the parameter that caused the exception. This value is typically provided automatically and
    /// should not be set manually.</param>
    /// <param name="message">The error message to include in the exception. If <see langword="null"/>, a default message is used.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is greater than or equal to <paramref name="other"/>.</exception>
    public static void ThrowIfGreaterThanOrEqual<TValue>(TValue value, TValue other, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null)
        where TValue : IComparable<TValue>
    {
        if (value.CompareTo(other) >= 0)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                value,
                message ?? $"Value is greater than or equal to the disallowed value. Allowed: {paramName} < {other}, Found: '{value}'");
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the specified value is less than or equal to a given comparison value.
    /// </summary>
    /// <typeparam name="TValue">The type of the values to compare. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="value">The value to validate. Must be greater than <paramref name="other"/> to avoid an exception.</param>
    /// <param name="other">The value to compare against. <paramref name="value"/> must be greater than this value.</param>
    /// <param name="paramName">The name of the parameter representing <paramref name="value"/>. Used in the exception message. This
    /// parameter is typically supplied automatically and should not be set manually.</param>
    /// <param name="message">An optional custom message to include in the exception. If <see langword="null"/>, a default message is used.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is less than or equal to <paramref name="other"/>.</exception>
    public static void ThrowIfLessThanOrEqual<TValue>(TValue value, TValue other, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null)
        where TValue : IComparable<TValue>
    {
        if (value.CompareTo(other) <= 0)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                value,
                message ?? $"Value is less than or equal to the disallowed value. Allowed: {paramName} > {other}, Found: '{value}'");
        }
    }
}
