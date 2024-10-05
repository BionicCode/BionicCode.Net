namespace BionicCode.Utilities.Net
{
  using System;

  [Flags]
  public enum PropertyAccessor
  {
    Set = 0,
    Get = 1,
    GetAndSet = Get | Set,
  }
}