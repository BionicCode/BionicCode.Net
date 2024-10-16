namespace BionicCode.Utilities.Net
{
  using System;

  [Flags]
  public enum PropertyAccessor
  {
    Undefined = 0,
    Set = 1,
    Get = 2,
    GetAndSet = Get | Set,
  }
}