namespace BionicCode.Utilities.Net
{
  using System;

  public class ProfilerExecutionRuntimeContextExeceptionException : Exception
  {
    public ProfilerExecutionRuntimeContextExeceptionException() { }
    public ProfilerExecutionRuntimeContextExeceptionException(string message) : base(message) { }
    public ProfilerExecutionRuntimeContextExeceptionException(string message, Exception inner) : base(message, inner) { }
  }
}