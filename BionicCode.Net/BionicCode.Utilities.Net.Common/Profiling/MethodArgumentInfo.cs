namespace BionicCode.Utilities.Net
{
  using System.Collections.Generic;

  internal readonly struct MethodArgumentInfo
  {
    public MethodArgumentInfo(IList<object> arguments, int argumentListIndex)
    {
      this.Arguments = arguments;
      this.ArgumentListIndex = argumentListIndex;
    }

    public IList<object> Arguments { get; }
    public int ArgumentListIndex { get; }
  }
}