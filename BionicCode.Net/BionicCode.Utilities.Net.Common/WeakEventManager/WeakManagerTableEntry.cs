namespace BionicCode.Utilities.Net
{
  using System;
  using System.Diagnostics;

  internal class WeakManagerTableEntry : ManagedWeakTableEntry
  {
    public WeakEventManager WeakEventManager { get; }
    public override bool IsPurged { get; protected set; }

    public WeakManagerTableEntry(object eventSource, Type eventSourceType, string eventName, WeakEventManager weakEventManager) : base(eventSource, eventSourceType, eventName)
      => this.WeakEventManager = weakEventManager;

    public override void Recycle()
    {
      Debug.WriteLine($"######## Recycling WeakEventManger ########");
      base.Recycle();
    }

    public override bool TryPurge()
    {
      Debug.WriteLine($"TryPurge called for WeakEventManager (event source)");

      if (this.IsRecycled || this.IsPurged || this.EventSource.TryGetTarget(out _))
      {
        return false;
      }

      Debug.WriteLine($"******** Purging event source (weak event manager)... ********");
      this.WeakEventManager.Purge();
      Recycle();

      this.IsPurged = true;
      return true;
    }
  }
}