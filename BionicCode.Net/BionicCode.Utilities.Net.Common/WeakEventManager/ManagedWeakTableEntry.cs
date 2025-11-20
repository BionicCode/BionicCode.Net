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
        public bool IsAlive => this.ReferenceTarget?.TryGetTarget(out _) ?? false;
        public Guid Id { get; }

        protected ManagedWeakTableEntry(object referenceTarget, Type referenceTargetType, Guid id)
        {
            ArgumentNullExceptionEx.ThrowIfNull(referenceTargetType, nameof(referenceTargetType));
            ArgumentNullExceptionEx.ThrowIfNull(referenceTarget, nameof(referenceTarget));

            this.ReferenceTarget = InitializeWeakReference(referenceTarget);
            this.ReferenceTargetType = referenceTargetType;
            this.Id = id;
        }

        public bool TryGetReferenceTarget(out object referenceTarget)
          => this.ReferenceTarget.TryGetTarget(out referenceTarget);

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
            RecycleWeakReference(this.ReferenceTarget);
            this.ReferenceTarget = null;
            this.IsRecycled = true;
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
