namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;

  /// <summary>
  /// Interface that encapsulates the publisher side API. For example, use this interface to constrain the access to the provided the <see cref="WeakEventAggregator"/> e.g., in context of dependency injection.
  /// </summary>
  public interface IWeakEventAggregatorPublisher
  {
    /// <summary>
    /// Register an object as event source for a  specified event.
    /// </summary>
    /// <param name="eventSource">The event publisher instance.</param>
    /// <param name="eventNames">A collection of event names that specify the published events of the <paramref name="eventSource"/>.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="eventSource"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="eventNames"/> is <see langword="null"/>.</exception>
    void StartBroadcasting(object eventSource, IEnumerable<string> eventNames);
    /// <summary>
    /// Register an object as event source for a specified event.
    /// </summary>
    /// <param name="eventSource">The event publisher instance.</param>
    /// <param name="eventNames">A collection of event names that specify the published events of the <paramref name="eventSource"/>.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="eventSource"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="eventNames"/> is <see langword="null"/>.</exception>
    void StartBroadcasting(object eventSource, params string[] eventNames);
    /// <summary>
    /// Register an object as event source for all its events.
    /// </summary>
    /// <param name="eventSource">The event publisher instance.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="eventSource"/> is <see langword="null"/>.</exception>
    void StartBroadcasting(object eventSource);

    /// <summary>
    /// Unregister the specified events of the event publisher.This method overload ignores all static events by default. 
    /// <br/>Can be configured to include static events.
    /// </summary>
    /// <param name="eventSource">The event publisher instance. </param>
    /// <param name="eventNames">The names of the events to unregister. Pass <see langword="null"/> to unregister all events</param>
    /// <exception cref="ArgumentNullException">The <paramref name="eventSource"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="eventNames"/> is <see langword="null"/>.</exception>
    void StopBroadcasting(object eventSource, IEnumerable<string> eventNames);

    /// <summary>
    /// Stops broadcasting the specified events of the event publisher.
    /// </summary>
    /// <param name="eventNames">The names of the events to unregister. Pass <see langword="null"/> to unregister all static events.</param>
    /// <param name="eventSource">The event publisher instance.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="eventSource"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="eventNames"/> is <see langword="null"/>.</exception>
    void StopBroadcasting(object eventSource, params string[] eventNames);

    /// <summary>
    /// Stops all events of the event publisher.
    /// </summary>
    /// <param name="eventSource">The event publisher instance.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="eventSource"/> is <see langword="null"/>.</exception>
    void StopBroadcasting(object eventSource);
  }
}