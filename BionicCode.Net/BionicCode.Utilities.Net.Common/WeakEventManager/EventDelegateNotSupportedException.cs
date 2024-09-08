namespace BionicCode.Utilities.Net
{
  using System;
  using System.Runtime.Serialization;

  [Serializable]
  public class EventDelegateNotSupportedException : Exception
  {
    public EventDelegateNotSupportedException()
    {
    }

    public EventDelegateNotSupportedException(string message) : base(message)
    {
    }

    public EventDelegateNotSupportedException(string message, Exception innerException) : base(message, innerException)
    {
    }
  }
}