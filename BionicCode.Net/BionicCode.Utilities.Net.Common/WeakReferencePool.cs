namespace BionicCode.Utilities.Net
{
  #region Info
  // //  
  // BionicUtilities.Net.Standard
  #endregion

  using System;
  using System.Collections.Generic;

  internal static class WeakReferencePool
  {
    private static Queue<WeakReference<object>> WeakReferences { get; } = new Queue<WeakReference<object>>();
    public static void Add(WeakReference<object> weakReference)
    {
      weakReference.SetTarget(null);
      WeakReferences.Enqueue(weakReference);
    }

    public static WeakReference<object> GetOrCreate(object reference)
    {
      WeakReference<object> weakReference;
      if (WeakReferences.Count > 0)
      {
        weakReference = WeakReferences.Dequeue();
        weakReference.SetTarget(reference);
      }
      else
      {
        weakReference = new WeakReference<object>(reference);
      }

      return weakReference;
    }
  }
}