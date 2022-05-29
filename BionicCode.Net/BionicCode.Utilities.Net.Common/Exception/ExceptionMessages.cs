namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Text;

  internal class ExceptionMessages
  {
    public static string GetArgumentNullExceptionMessage_ValidationPropertyName() => "Please provide a valid property name. A validation error must always map to a prooperty.";
    public static string GetArgumentExceptionMessage_ValidationFailed() => "Property validation failed.";
  }
}
