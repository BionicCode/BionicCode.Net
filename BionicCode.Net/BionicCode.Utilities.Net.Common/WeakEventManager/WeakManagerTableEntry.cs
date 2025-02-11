namespace BionicCode.Utilities.Net
{
  using System;
  using System.Diagnostics;

  internal class WeakManagerTableEntry : ManagedWeakTableEntry
  {
    public string EventName { get; }
    public WeakEventManager WeakEventManager { get; }
    public override bool IsPurged { get; protected set; }

#if DEBUG
    public int WeakEventManagerInstanceNumber { get; }
#endif

    public WeakManagerTableEntry(object eventSource, Type eventSourceType, string eventName, WeakEventManager weakEventManager) : base(eventSource, eventSourceType, weakEventManager.Id)
    {
      this.EventName = eventName;
      this.WeakEventManager = weakEventManager;

#if DEBUG
      this.WeakEventManagerInstanceNumber = weakEventManager.InstanceNumber;
#endif
    }

    public bool TryGetEventSource(out object eventSource)
      => TryGetReferenceTarget(out eventSource);

    public override void Recycle()
    {
#if DEBUG
      Debug.WriteLine($"WeakEventManager instance #{this.WeakEventManagerInstanceNumber} of {WeakEventManager.InstanceCounter}: ######## Recycling WeakEventManger ########.");
#endif

      base.Recycle();
    }

    public override bool TryPurge(bool isForced)
    {
#if DEBUG
      Debug.WriteLine($"WeakEventManager instance #{this.WeakEventManagerInstanceNumber} of {WeakEventManager.InstanceCounter}: TryPurge called for WeakEventManager (event source).");
#endif

      if (this.IsRecycled || this.IsPurged
        || (!isForced && this.IsAlive))
      {
#if DEBUG
        Debug.WriteLine($"WeakEventManager instance #{this.WeakEventManagerInstanceNumber} of {WeakEventManager.InstanceCounter}: Nothing to purge here (event source).");
#endif

        return false;
      }

#if DEBUG
      Debug.WriteLine($"WeakEventManager instance #{this.WeakEventManagerInstanceNumber} of {WeakEventManager.InstanceCounter}: ******** Purging event source (weak event manager)...Forced: {isForced} ********.");
#endif

      this.WeakEventManager.Purge();
      Recycle();

      this.IsPurged = true;
      return true;
    }
  }
}