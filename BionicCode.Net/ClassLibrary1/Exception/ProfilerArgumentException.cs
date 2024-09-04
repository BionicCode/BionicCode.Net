namespace BionicCode.Utilities.Net
{
  using System;
  using System.Runtime.Serialization;

  [Serializable]
  internal class ProfilerArgumentException : Exception
  {
    public ProfilerArgumentException()
    {
    }

    public ProfilerArgumentException(string message) : base(message)
    {
    }

    public ProfilerArgumentException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected ProfilerArgumentException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
  }
}