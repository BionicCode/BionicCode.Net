namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  internal abstract class ManagedWeakTableEntry : IPurgeable
  {
    public WeakReference<object> EventSource { get; }
    public Type EventSourceType { get; }
    public string EventName { get; }
    public bool IsRecycled { get; private set; }
    public abstract bool IsPurged { get; protected set; }

    protected ManagedWeakTableEntry(object eventSource, Type eventSourceType, string eventName)
    {
      if (eventSourceType is null)
      {
        throw new ArgumentNullException(nameof(eventSourceType));
      }

      if (eventName is null)
      {
        throw new ArgumentNullException(nameof(eventName));
      }

      if (string.IsNullOrWhiteSpace(eventName))
      {
        throw new ArgumentException("No valid event name", nameof(eventName));
      }

      this.EventSource = InitializeWeakReference(eventSource);
      this.EventSourceType = eventSourceType;
      this.EventName = eventName;
    }

    public abstract bool TryPurge(bool isForced);

    public virtual void Recycle()
      => RecycleInternal();

    protected WeakReference<object> InitializeWeakReference(object eventParticipant)
    {
      WeakReference<object> eventTargetReference = ManagedWeakTable.GetOrCreateWeakReference(eventParticipant);
      return eventTargetReference;
    }

    private void RecycleInternal()
    {
      RecycleWeakReference(this.EventSource);
      this.IsRecycled = true;
    }

    protected void RecycleWeakReference(WeakReference<object> weakReference)
    {
      if (weakReference is null)
      {
        return;
      }

      weakReference.SetTarget(null);
      ManagedWeakTable.RecycleWeakReference(weakReference);
    }
  }
}