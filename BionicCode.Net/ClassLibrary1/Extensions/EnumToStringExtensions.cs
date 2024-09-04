namespace BionicCode.Utilities.Net
{
  using System;

  public static partial class HelperExtensionsCommon
  {

    /// <summary>
    /// Converts the value of <see cref="AccessModifier"/> to a string representation.
    /// </summary>
    /// <param name="accessModifier"></param>
    /// <param name="toUpperCase">Controls whether to convert the string's first character to uppercase (<paramref name="toUpperCase"/>=<c>true</c>). The default is lowercase (<paramref name="toUpperCase"/>=<c>true</c>.</param>
    /// <returns>The readable string representation of the enum value.</returns>
    /// <exception cref="NotSupportedException">The enum value is not supported. (This exception is only intended for internal maintainance and will never be thrown in production code)</exception>
    public static string ToDisplayStringValue(this AccessModifier accessModifier, bool toUpperCase = false)
    {
      switch (accessModifier)
      {
        case AccessModifier.Default:
          return toUpperCase ? "Internal" : "internal";
        case AccessModifier.Public:
          return toUpperCase ? "Public" : "public";
        case AccessModifier.ProtectedInternal:
          return toUpperCase ? "Protected Internal" : "protected internal";
        case AccessModifier.Internal:
          return toUpperCase ? "Internal" : "internal";
        case AccessModifier.Protected:
          return toUpperCase ? "Protected" : "protected";
        case AccessModifier.PrivateProtected:
          return toUpperCase ? "Provate Protected" : "private protected";
        case AccessModifier.Private:
          return toUpperCase ? "Private" : "private";
        default:
          throw new NotSupportedException();
      }
    }

    /// <summary>
    /// Converts the value of <see cref="ProfiledTargetType"/> to a string representation.
    /// </summary>
    /// <param name="profiledTargetType"></param>
    /// <param name="toUpperCase">Controls whether to convert the string's first character to uppercase (<paramref name="toUpperCase"/>=<c>true</c>). The default is lowercase (<paramref name="toUpperCase"/>=<c>true</c>.</param>
    /// <returns>The readable string representation of the enum value.</returns>
    /// <exception cref="NotSupportedException">The enum value is not supported. (This exception is only intended for internal maintainance and will never be thrown in production code)</exception>
    public static string ToDisplayStringValue(this ProfiledTargetType profiledTargetType, bool toUpperCase = false)
    {
      switch (profiledTargetType)
      {
        case ProfiledTargetType.None:
          return toUpperCase ? "None" : "none";
        case ProfiledTargetType.PropertyGet:
          return toUpperCase ? "Property get()" : "property get()";
        case ProfiledTargetType.PropertySet:
          return toUpperCase ? "Property set()" : "property set()";
        case ProfiledTargetType.Property:
          return toUpperCase ? "Property" : "property";
        case ProfiledTargetType.Constructor:
          return toUpperCase ? "Constructor" : "constructor";
        case ProfiledTargetType.Event:
          return toUpperCase ? "Event" : "event";
        case ProfiledTargetType.Delegate:
          return toUpperCase ? "Delegate" : "delegate";
        case ProfiledTargetType.Method:
          return toUpperCase ? "Method" : "method";
        default:
          throw new NotSupportedException();
      }
    }

    /// <summary>
    /// Converts the value of <see cref="TimeUnit"/> to a string representation.
    /// </summary>
    /// <param name="timeUnit"></param>
    /// <param name="toUpperCase">Controls whether to convert the string's first character to uppercase (<paramref name="toUpperCase"/>=<c>true</c>). The default is lowercase (<paramref name="toUpperCase"/>=<c>true</c>.</param>
    /// <returns>The readable string representation of the enum value.</returns>
    /// <exception cref="NotSupportedException">The enum value is not supported. (This exception is only intended for internal maintainance and will never be thrown in production code)</exception>
    public static string ToDisplayStringValue(this TimeUnit timeUnit, bool toUpperCase = false)
    {
      switch (timeUnit)
      {
        case TimeUnit.None:
          return toUpperCase ? "None" : "none";
        case TimeUnit.Seconds:
          return toUpperCase ? "S" : "s";
        case TimeUnit.Milliseconds:
          return toUpperCase ? "Ms" : "ms";
        case TimeUnit.Microseconds:
          return toUpperCase ? "µs" : "µs";
        case TimeUnit.Nanoseconds:
          return toUpperCase ? "Ns" : "ns";
        default:
          throw new NotSupportedException();
      }
    }
  }
}
