namespace BionicCode.Utilities.Net
{
  using System;
  using Microsoft.CodeAnalysis.CSharp.Syntax;

  public static partial class HelperExtensionsCommon
  {

    /// <summary>
    /// Converts the value of <see cref="AccessModifier"/> to a string representation.
    /// </summary>
    /// <param name="accessModifier"></param>
    /// <param name="toUpperCase">Controls whether to convert the string's first character to uppercase (<paramref name="toUpperCase"/>=<c>true</c>). The default is lowercase (<paramref name="toUpperCase"/>=<c>true</c>.</param>
    /// <returns>The readable string representation of the enum value.</returns>
    /// <exception cref="NotSupportedException">The enum value is not supported. (This exception is only intended for internal maintenance and will never be thrown in production code)</exception>
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
          return toUpperCase ? "Private Protected" : "private protected";
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
    /// <exception cref="NotSupportedException">The enum value is not supported. (This exception is only intended for internal maintenance and will never be thrown in production code)</exception>
    internal static string ToDisplayStringValue(this ProfiledTargetType profiledTargetType, bool toUpperCase = false, bool toBaseType = false)
    {
      switch (profiledTargetType)
      {
        case ProfiledTargetType.None:
          return toUpperCase ? "None" : "none";
        case ProfiledTargetType.PropertyGet:
          {
            string result = toUpperCase ? "Property" : "property";
            return toBaseType ? result : $"{result} get()";
          }
        case ProfiledTargetType.PropertySet:
          {
            string result = toUpperCase ? "Property" : "property";
            return toBaseType ? result : $"{result} set()";
          }
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
        case ProfiledTargetType.Indexer:
          return toUpperCase ? "Indexer" : "indexer";
        case ProfiledTargetType.IndexerGet:
          {
            string result = toUpperCase ? "Indexer" : "indexer";
            return toBaseType ? result : $"{result} get()";
          }
        case ProfiledTargetType.IndexerSet:
          {
            string result = toUpperCase ? "Indexer" : "indexer";
            return toBaseType ? result : $"{result} set()";
          }
        case ProfiledTargetType.Scope:
          return toUpperCase ? "Scope" : "scope";
        default:
          throw new NotSupportedException(ExceptionMessages.GetValueNotSupportedExceptionMessage(profiledTargetType));
      }
    }

    /// <summary>
    /// Converts the value of <see cref="TimeUnit"/> to a string representation.
    /// </summary>
    /// <param name="timeUnit"></param>
    /// <param name="toUpperCase">Controls whether to convert the string's first character to uppercase (<paramref name="toUpperCase"/>=<c>true</c>). The default is lowercase (<paramref name="toUpperCase"/>=<c>true</c>.</param>
    /// <returns>The readable string representation of the enum value.</returns>
    /// <exception cref="NotSupportedException">The enum value is not supported. (This exception is only intended for internal maintenance and will never be thrown in production code)</exception>
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
          throw new NotSupportedException(ExceptionMessages.GetValueNotSupportedExceptionMessage(timeUnit));
      }
    }

    internal static string ToDisplayTypeKind(this SymbolAttributes symbolAttributes, bool toUpperCase = false)
    {
      switch (symbolAttributes)
      {
        case SymbolAttributes.Class:
        case SymbolAttributes.SealedClass:
        case SymbolAttributes.AbstractClass:
        case SymbolAttributes.StaticClass:
        case SymbolAttributes.GenericClass:
        case SymbolAttributes.GenericSealedClass:
        case SymbolAttributes.GenericAbstractClass:
        case SymbolAttributes.GenericStaticClass:
          return toUpperCase ? "Class" : "class";
        case SymbolAttributes.Interface:
        case SymbolAttributes.GenericInterface:
          return toUpperCase ? "Interface" : "interface";
        case SymbolAttributes.Delegate:
        case SymbolAttributes.GenericDelegate:
          return toUpperCase ? "Delegate" : "delegate";
        case SymbolAttributes.Struct:
        case SymbolAttributes.RefStruct:
        case SymbolAttributes.ReadOnlyStruct:
        case SymbolAttributes.GenericStruct:
        case SymbolAttributes.GenericReadOnlyStruct:
          return toUpperCase ? "Struct" : "struct";
        case SymbolAttributes.Enum:
          return toUpperCase ? "Enum" : "enum";
        case SymbolAttributes.Method:
        case SymbolAttributes.VirtualMethod:
        case SymbolAttributes.AbstractMethod:
        case SymbolAttributes.StaticMethod:
        case SymbolAttributes.SealedOverrideMethod:
        case SymbolAttributes.OverrideMethod:
        case SymbolAttributes.GenericVirtualMethod:
        case SymbolAttributes.GenericMethod:
        case SymbolAttributes.GenericAbstractMethod:
        case SymbolAttributes.GenericStaticMethod:
        case SymbolAttributes.GenericSealedOverrideMethod:
        case SymbolAttributes.GenericOverrideMethod:
          return toUpperCase ? "Method" : "method";
        case SymbolAttributes.Property:
        case SymbolAttributes.ReadOnlyProperty:
        case SymbolAttributes.InitProperty:
        case SymbolAttributes.AbstractProperty:
        case SymbolAttributes.AbstractReadOnlyProperty:
        case SymbolAttributes.StaticProperty:
        case SymbolAttributes.StaticReadOnlyProperty:
        case SymbolAttributes.VirtualProperty:
        case SymbolAttributes.VirtualReadOnlyProperty:
        case SymbolAttributes.OverrideProperty:
        case SymbolAttributes.OverrideReaOnlyProperty:
          return toUpperCase ? "Property" : "property";
        case SymbolAttributes.Field:
        case SymbolAttributes.ReadOnlyField:
        case SymbolAttributes.RefField:
        case SymbolAttributes.StaticReadOnlyField:
        case SymbolAttributes.ConstantField:
          return toUpperCase ? "Field" : "field";
        case SymbolAttributes.Event:
        case SymbolAttributes.VirtualEvent:
        case SymbolAttributes.AbstractEvent:
        case SymbolAttributes.OverrideEvent:
          return toUpperCase ? "Event" : "event";
        case SymbolAttributes.Constructor:
        case SymbolAttributes.StaticConstructor:
          return toUpperCase ? "Constructor" : "constructor";
        case SymbolAttributes.IndexerProperty:
        case SymbolAttributes.ReadOnlyIndexerProperty:
        case SymbolAttributes.OverrideReadOnlyIndexerProperty:
        case SymbolAttributes.OverrideIndexerProperty:
        case SymbolAttributes.StaticIndexerProperty:
        case SymbolAttributes.StaticReadOnlyIndexerProperty:
        case SymbolAttributes.AbstractIndexerProperty:
        case SymbolAttributes.AbstractReadOnlyIndexerProperty:
          return toUpperCase ? "Indexer" : "indexer";
        case SymbolAttributes.Undefined:
        case SymbolAttributes.Final:
        case SymbolAttributes.Virtual:
        case SymbolAttributes.Abstract:
        case SymbolAttributes.Static:
        case SymbolAttributes.Override:
        case SymbolAttributes.Generic:
        case SymbolAttributes.Member:
        case SymbolAttributes.Type:
        case SymbolAttributes.GenericType:
        case SymbolAttributes.Parameter:
        case SymbolAttributes.InParameter:
        case SymbolAttributes.OutParameter:
        case SymbolAttributes.RefParameter:
        case SymbolAttributes.OptionalParameter:
        case SymbolAttributes.ByReference:
        case SymbolAttributes.Init:
        case SymbolAttributes.Constant:
          return toUpperCase ? "Not a type or member" : "not a type or member";
        default:
          throw new NotSupportedException(ExceptionMessages.GetValueNotSupportedExceptionMessage(symbolAttributes));
      }
    }
  }
}
