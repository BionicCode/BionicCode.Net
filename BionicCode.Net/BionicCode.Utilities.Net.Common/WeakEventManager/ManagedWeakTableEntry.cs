namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  internal abstract class ManagedWeakTableEntry : IPurgeable
  {
    private static readonly List<WeakReference<object>> RecycledWeakReferences = new List<WeakReference<object>>();
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

    public abstract bool TryPurge();

    public virtual void Recycle()
      => RecycleInternal();

    protected WeakReference<object> InitializeWeakReference(object eventParticipant)
    {
      WeakReference<object> eventTargetReference;
      if (RecycledWeakReferences.Any())
      {
        eventTargetReference = RecycledWeakReferences.Last();
        eventTargetReference.SetTarget(eventParticipant);
      }
      else
      {
        eventTargetReference = new WeakReference<object>(eventParticipant, trackResurrection: false);
      }

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
      RecycledWeakReferences.Add(weakReference);
    }
  }
}