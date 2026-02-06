namespace BionicCode.Utilities.Net
{
    using System;

    internal abstract class ManagedWeakTableEntry : IPurgeable
    {
        public WeakReference<object> ReferenceTarget { get; private set; }
        public Type ReferenceTargetType { get; }
        public string EventName { get; }
        public bool IsRecycled { get; private set; }
        public abstract bool IsPurged { get; protected set; }
        public bool IsAlive => ReferenceTarget?.TryGetTarget(out _) ?? false;
        public Guid Id { get; }

        protected ManagedWeakTableEntry(object referenceTarget, Type referenceTargetType, Guid id)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(referenceTargetType, nameof(referenceTargetType));
            ArgumentNullExceptionAdvanced.ThrowIfNull(referenceTarget, nameof(referenceTarget));

            ReferenceTarget = InitializeWeakReference(referenceTarget);
            ReferenceTargetType = referenceTargetType;
            Id = id;
        }

        public bool TryGetReferenceTarget(out object referenceTarget)
          => ReferenceTarget.TryGetTarget(out referenceTarget);

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
            RecycleWeakReference(ReferenceTarget);
            ReferenceTarget = null;
            IsRecycled = true;
        }

        protected void RecycleWeakReference(WeakReference<object> weakReference)
        {
            if (weakReference is null)
            {
                return;
            }

            ManagedWeakTable.RecycleWeakReference(weakReference);
        }
    }
}
