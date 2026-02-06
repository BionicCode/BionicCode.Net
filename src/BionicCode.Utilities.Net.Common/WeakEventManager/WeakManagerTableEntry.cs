namespace BionicCode.Utilities.Net
{
    using System;
    using System.Diagnostics;

    internal class WeakManagerTableEntry : ManagedWeakTableEntry
    {
        public new string EventName { get; }
        public WeakEventManager WeakEventManager { get; }
        public override bool IsPurged { get; protected set; }

#if DEBUG
        public int WeakEventManagerInstanceNumber { get; }
#endif

        public WeakManagerTableEntry(object eventSource, Type eventSourceType, string eventName, WeakEventManager weakEventManager) : base(eventSource, eventSourceType, weakEventManager.Id)
        {
            EventName = eventName;
            WeakEventManager = weakEventManager;

#if DEBUG
            WeakEventManagerInstanceNumber = weakEventManager.InstanceNumber;
#endif
        }

        public bool TryGetEventSource(out object eventSource)
          => TryGetReferenceTarget(out eventSource);

        public override void Recycle()
        {
#if DEBUG
            Debug.WriteLine($"WeakEventManager instance #{WeakEventManagerInstanceNumber} of {WeakEventManager.InstanceCounter}: ######## Recycling WeakEventManger ########.");
#endif

            base.Recycle();
        }

        public override bool TryPurge(bool isForced)
        {
#if DEBUG
            Debug.WriteLine($"WeakEventManager instance #{WeakEventManagerInstanceNumber} of {WeakEventManager.InstanceCounter}: TryPurge called for WeakEventManager (event source).");
#endif

            if (IsRecycled || IsPurged
              || (!isForced && IsAlive))
            {
#if DEBUG
                Debug.WriteLine($"WeakEventManager instance #{WeakEventManagerInstanceNumber} of {WeakEventManager.InstanceCounter}: Nothing to purge here (event source).");
#endif

                return false;
            }

#if DEBUG
            Debug.WriteLine($"WeakEventManager instance #{WeakEventManagerInstanceNumber} of {WeakEventManager.InstanceCounter}: ******** Purging event source (weak event manager)...Forced: {isForced} ********.");
#endif

            WeakEventManager.Purge();
            Recycle();

            IsPurged = true;
            return true;
        }
    }
}
