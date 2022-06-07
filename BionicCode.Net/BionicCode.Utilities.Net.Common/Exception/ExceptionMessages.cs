namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Text;

  internal class ExceptionMessages
  {
    public static string GetInvalidOperationExceptionMessage_CollectionEmpty() => "The sequence is empty.";
    public static string GetInvalidOperationExceptionMessage_ItemNotFound(string predicateName) => $"The sequence contains no item that matches the predicate '{predicateName}'.";
    public static string GetArgumentNullExceptionMessage_ValidationPropertyName() => "Please provide a valid property name. A validation error must always map to a property.";
    public static string GetArgumentExceptionMessage_ValidationFailed() => "Property validation failed.";
    public static string GetInvalidOperationExceptionMessage_SetFactoryModeOnScopedFactory() => $"Modifying the lifetime of instances produced by a scoped factory is not allowed. Instances are automatically treated as {FactoryMode.Singleton} for the current scope. You can change the IFactory.FactoryMode property after leaving the scope.";
  }
}
