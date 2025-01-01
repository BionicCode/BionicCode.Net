namespace BionicCode.Utilities.Net
{
  using System;
  using System.Threading;

  /// <summary>
  /// Interface that encapsulates the listener side API. For example, use this interface to constrain the access to the provided the <see cref="WeakEventAggregator"/> e.g., in context of dependency injection.
  /// </summary>
  public interface IWeakEventAggregatorListenerService
  {
    /// <summary>
    /// Registers an event delegate, with a random signature that does not have to follow the common C# event practices, to handle a specific event published by a specific observable type.
    /// </summary>
    /// <param name="eventName">The name of the observed event.</param>
    /// <typeparam name="TEventSource">The type of the event publisher object. This can be any derived type like a class or interface.</typeparam>
    /// <typeparam name="TDelegate">The type of the event handler.</typeparam>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <exception cref="EventHandlerMismatchException">The signature of the event handler and the event are incompatible.</exception>   
    /// <exception cref="ArgumentNullException">The <paramref name="eventHandler"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="eventName"/> is <see langword="null"/> or an empty string.</exception>

    void StartListening<TEventSource, TDelegate>(string eventName, TDelegate eventHandler) where TDelegate : Delegate;

    /// <summary>
    /// Registers an event delegate, with a random signature that does not have to follow the common C# event practices, to handle a specific event published by a specific observable type.
    /// <br/>This overload supports marshaling to the current thread by capturing the <see cref="SynchronizationContext"/> of the caller.
    /// </summary>
    /// <param name="eventName">The name of the observed event.</param>
    /// <typeparam name="TEventSource">The type of the event publisher object. This can be any derived type like a class or interface.</typeparam>
    /// <typeparam name="TDelegate">The type of the event handler.</typeparam>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="isMarshalEventToCurrentThreadEnabled"><see langword="true"/> if the current <see cref="SynchronizationContext"/> should be captured to execute the event handler.</param>
    /// <returns><see langword="true"/> when registration was successful, otherwise <see langword="false"/>.</returns>
    /// <exception cref="EventHandlerMismatchException">The signature of the event handler and the event are incompatible.</exception>   
    /// <exception cref="ArgumentNullException">The <paramref name="eventHandler"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="eventName"/> is <see langword="null"/> or an empty string.</exception>
    void StartListening<TEventSource, TDelegate>(string eventName, TDelegate eventHandler, bool isMarshalEventToCurrentThreadEnabled) where TDelegate : Delegate;

    /// <summary>
    /// Registers an event delegate, with a random signature that does not have to follow the common C# event practices, to handle a specific event published by a specific observable type.
    /// <br/>This overload supports marshaling to the specified <see cref="SynchronizationContext"/>.
    /// </summary>
    /// <param name="eventName">The name of the observed event.</param>
    /// <typeparam name="TEventSource">The type of the event publisher object. This can be any derived type like a class or interface.</typeparam>
    /// <typeparam name="TDelegate">The type of the event handler.</typeparam>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="synchronizationContext">The <see cref="SynchronizationContext"/> that the event delegate is to be executed on.</param>
    /// <returns><see langword="true"/> when registration was successful, otherwise <see langword="false"/>.</returns>
    /// <exception cref="EventHandlerMismatchException">The signature of the event handler and the event are incompatible.</exception>   
    /// <exception cref="ArgumentNullException">The <paramref name="eventHandler"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="synchronizationContext"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="eventName"/> is <see langword="null"/> or an empty string.</exception>
    void StartListening<TEventSource, TDelegate>(string eventName, TDelegate eventHandler, SynchronizationContext synchronizationContext) where TDelegate : Delegate;

    /// <summary>
    /// Removes the event handler for a specified event of a certain event publisher type.
    /// </summary>
    /// <param name="eventName">The event name of the event that the delegate is handling.</param>
    /// <typeparam name="TEventSource">The type of the event publisher object. This can be any derived type like a class or interface.</typeparam>
    /// <typeparam name="TDelegate">The type of the event handler.</typeparam>
    /// <param name="eventHandler">The event handler to remove.</param>    
    /// <exception cref="ArgumentNullException">The <paramref name="eventHandler"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="eventName"/> is <see langword="null"/> or an empty string.</exception>
    void StopListening<TEventSource, TDelegate>(string eventName, TDelegate eventHandler) where TDelegate : Delegate;

    /// <summary>
    /// Removes all event handlers for a specified event of a specified event publisher type.
    /// </summary>
    /// <param name="eventName">The event name of the event that the delegate is handling.</param>
    /// <typeparam name="TEventSource">The type of the event publisher object. This can be any derived type like a class or interface.</typeparam>
    /// <exception cref="ArgumentException">The <paramref name="eventName"/> is <see langword="null"/> or an empty string.</exception>
    void StopListeningAll<TEventSource>(string eventName);

    /// <summary>
    /// Removes all event handlers that are registered with a specified event publisher type, for all events.
    /// </summary>
    /// <typeparam name="TEventSource">The type of the event publisher object. This can be any derived type like a class or interface.</typeparam>
    void StopListeningAll<TEventSource>();
  }
}