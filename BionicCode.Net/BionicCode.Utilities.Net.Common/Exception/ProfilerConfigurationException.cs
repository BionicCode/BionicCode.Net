namespace BionicCode.Utilities.Net
{
  using System;
  using System.Runtime.Serialization;

  [Serializable]
  internal class ProfilerConfigurationException : Exception
  {
    public ProfilerConfigurationException()
    {
    }

    public ProfilerConfigurationException(string message) : base(message)
    {
    }

    public ProfilerConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
    }
  }
}