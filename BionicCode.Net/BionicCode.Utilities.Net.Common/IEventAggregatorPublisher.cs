namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The encapsulated publisher-side API for the <see cref="EventAggregator"/>.
    /// </summary>
    [Obsolete("Please use the IWeakEventAggregatorPublisherService along with the WeakEventAggregator implementation instead (same namespace)! This type offers a cleaned-up API and several performance and feature improvements and uses weak events under the hoods.")]
    public interface IEventAggregatorPublisher
    {
        /// <summary>
        /// Register a type as event source.
        /// </summary>
        /// <param name="eventSource">The publisher instance.</param>
        /// <param name="eventNames">A collection of event names that define the observed events of the <paramref name="eventSource"/></param>
        /// <returns><see langword="true"/> when registration was successful, otherwise <see langword="false"/>.</returns>
        bool TryRegisterObservable(object eventSource, IEnumerable<string> eventNames);

        /// <summary>
        /// Unregister the event publisher for a collection of specified events.
        /// </summary>
        /// <param name="eventSource">The event publisher instance.</param>
        /// <param name="eventNames">The names of the events to unregister.</param>
        /// <param name="removeEventObservers">If <see langword="true"/> removes all event listeners of the specified events. The value is <see langword="false"/> by default.</param>
        /// <returns><see langword="true"/> when removal was successful, otherwise <see langword="false"/>.</returns>
        bool TryRemoveObservable(
          object eventSource,
          IEnumerable<string> eventNames,
          bool removeEventObservers = false);

        /// <summary>
        /// Unregister the event publisher for all events.
        /// </summary>
        /// <param name="eventSource">The event publisher instance.</param>
        /// <param name="removeEventObservers">If <see langword="true"/> removes all event listeners of the specified events. The value is <see langword="false"/> by default.</param>
        /// <returns><see langword="true"/> when removal was successful, otherwise <see langword="false"/>.</returns>
        bool TryRemoveObservable(object eventSource, bool removeEventObservers = false);
    }
}
