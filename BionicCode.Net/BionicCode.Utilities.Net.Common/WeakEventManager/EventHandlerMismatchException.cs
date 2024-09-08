namespace BionicCode.Utilities.Net
{
  using System;
  using System.Runtime.Serialization;

  [Serializable]
  public class EventHandlerMismatchException : Exception
  {
    public EventHandlerMismatchException()
    {
    }

    public EventHandlerMismatchException(string message) : base(message)
    {
    }

    public EventHandlerMismatchException(string message, Exception innerException) : base(message, innerException)
    {
    }
  }
}