namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections;

  [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]   
  public sealed class ProfilerAutoDiscoverAttribute : Attribute
  {
    public ProfilerAutoDiscoverAttribute()
    {
    }
  }
}