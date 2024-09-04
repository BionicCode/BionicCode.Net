namespace BionicCode.Utilities.Net
{
  using System;

  public class ProfilerExecutionRuntimeContextExeceptionException : Exception
  {
    public ProfilerExecutionRuntimeContextExeceptionException() { }
    public ProfilerExecutionRuntimeContextExeceptionException(string message) : base(message) { }
    public ProfilerExecutionRuntimeContextExeceptionException(string message, Exception inner) : base(message, inner) { }
    protected ProfilerExecutionRuntimeContextExeceptionException(
    System.Runtime.Serialization.SerializationInfo info,
    System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
  }
}