[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("BionicCode.Controls.Net.Wpf")]
namespace BionicCode.Utilities.Net.Reflection;

using System;

internal static class ExceptionMessages
{
    public static string GetTypeMismatchExceptionMessage(Type providedType, string parameterName, Type expectedType, string referenceName) => $"Type mismatch. The type of the '{parameterName}' must be assignable to the type of '{referenceName}'. Expected {expectedType.FullName} but found {providedType.FullName}.";
    public static string GetDeclaringTypeOfMemberIsValueTypeWrongInvokerExceptionMessage(MemberData propertyData, string correctInvokerName) => $"The property '{propertyData.FullyQualifiedSignature}' is declared on a value type, but a method for properties declared on a reference type was called. Use the '{correctInvokerName}' method to set property values on value types.";
    public static string GetDeclaringTypeOfMemberIsReferenceTypeWrongInvokerExceptionMessage(MemberData propertyData, string correctInvokerName) => $"The property '{propertyData.FullyQualifiedSignature}' is declared on a reference type, but a method for properties declared on a value type was called. Use the '{correctInvokerName}' method to set property values on reference types.";
    public static string GetInvalidAccessCollectionEmptyExceptionMessage(string throwingTypeName, string accessedMemberName) => $"The '{throwingTypeName}' is empty. Therefore the '{accessedMemberName}' property is not accessible.";
}
