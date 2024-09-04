namespace BionicCode.Utilities.Net
{
  using System;
  using System.Runtime.Serialization;

  [Serializable]
  internal class MissingProfilerArgumentAttributeException : Exception
  {
    public MissingProfilerArgumentAttributeException()
    {
    }

    public MissingProfilerArgumentAttributeException(string message) : base(message)
    {
    }

    public MissingProfilerArgumentAttributeException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected MissingProfilerArgumentAttributeException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
  }
}