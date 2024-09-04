namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Threading;

  /// <summary>
  /// Allows listening to events without introducing direct coupling between observer and observable. The observer can handle events withou introducing a dependency to the event source.
  /// </summary>
  public interface IEventAggregator
  {
    /// <summary>
    /// Register a type as event source.
    /// </summary>
    /// <param name="eventSource">The publisher instance.</param>
    /// <param name="eventNames">A collection of event names that define the observed events of the <paramref name="eventSource"/></param>
    /// <returns><c>true</c> when registration was successful, otherwise <c>false</c>.</returns>
    bool TryRegisterObservable(object eventSource, IEnumerable<string> eventNames);

    /// <summary>
    /// Registers an event delegate to handle a specific event published by a specific observable type.
    /// </summary>
    /// <param name="eventName">The name of the observed event.</param>
    /// <param name="eventSourceType">The type of the observable.</param>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <returns><c>true</c> when registration was successful, otherwise <c>false</c>.</returns>
    bool TryRegisterObserver(string eventName, Type eventSourceType, Delegate eventHandler);

    /// <summary>
    /// Registers an event delegate to handle a specific event published by a specific observable type.
    /// </summary>
    /// <param name="eventName">The name of the observed event.</param>
    /// <param name="eventSourceType">The type of the observable.</param>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="isMarshalEventToCurrentThreadEnabled"><c>true</c> if the current <see cref="SynchronizationContext"/> should be captured to execute the event handler.</param>
    /// <returns><c>true</c> when registration was successful, otherwise <c>false</c>.</returns>
    bool TryRegisterObserver(string eventName, Type eventSourceType, Delegate eventHandler, bool isMarshalEventToCurrentThreadEnabled);

    /// <summary>
    /// Registers an event delegate to handle a specific event published by a specific observable type.
    /// </summary>
    /// <param name="eventName">The name of the observed event.</param>
    /// <param name="eventSourceType">The type of the observable.</param>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="synchronizationContext">The <see cref="SynchronizationContext"/> that the event delegate is to be executed on.</param>
    /// <returns><c>true</c> when registration was successful, otherwise <c>false</c>.</returns>
    bool TryRegisterObserver(string eventName, Type eventSourceType, Delegate eventHandler, SynchronizationContext synchronizationContext);

    /// <summary>
    /// Registers an event delegate to handle a specific event published by a specific observable type.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event args object.</typeparam>
    /// <param name="eventName">The name of the observed event.</param>
    /// <param name="eventSourceType">The type of the observable.</param>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <returns><c>true</c> when registration was successful, otherwise <c>false</c>.</returns>
    bool TryRegisterObserver<TEventArgs>(string eventName, Type eventSourceType, EventHandler<TEventArgs> eventHandler);

    /// <summary>
    /// Registers an event delegate to handle a specific event published by a specific observable type.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event args object.</typeparam>
    /// <param name="eventName">The name of the observed event.</param>
    /// <param name="eventSourceType">The type of the observable.</param>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="isMarshalEventToCurrentThreadEnabled"><c>true</c> if the current <see cref="SynchronizationContext"/> should be captured to execute the event handler.</param>
    /// <returns><c>true</c> when registration was successful, otherwise <c>false</c>.</returns>
    bool TryRegisterObserver<TEventArgs>(string eventName, Type eventSourceType, EventHandler<TEventArgs> eventHandler, bool isMarshalEventToCurrentThreadEnabled);

    /// <summary>
    /// Registers an event delegate to handle a specific event published by a specific observable type.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event args object.</typeparam>
    /// <param name="eventName">The name of the observed event.</param>
    /// <param name="eventSourceType">The type of the observable.</param>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="synchronizationContext">The <see cref="SynchronizationContext"/> that the event delegate is to be executed on.</param>
    /// <returns><c>true</c> when registration was successful, otherwise <c>false</c>.</returns>
    bool TryRegisterObserver<TEventArgs>(string eventName, Type eventSourceType, EventHandler<TEventArgs> eventHandler, SynchronizationContext synchronizationContext);

    /// <summary>
    /// Register an event delegate to handle a specific event which could be published by any type.
    /// </summary>
    /// <param name="eventName">The name of the observed event.</param>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <returns><c>true</c> when registration was successful, otherwise <c>false</c>.</returns>
    bool TryRegisterGlobalObserver(string eventName, Delegate eventHandler);

    /// <summary>
    /// Register an event delegate to handle a specific event which could be published by any type.
    /// </summary>
    /// <param name="eventName">The name of the observed event.</param>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="isMarshalEventToCurrentThreadEnabled"><c>true</c> if the current <see cref="SynchronizationContext"/> should be captured to execute the event handler.</param>
    /// <returns><c>true</c> when registration was successful, otherwise <c>false</c>.</returns>
    bool TryRegisterGlobalObserver(string eventName, Delegate eventHandler, bool isMarshalEventToCurrentThreadEnabled);

    /// <summary>
    /// Register an event delegate to handle a specific event which could be published by any type.
    /// </summary>
    /// <param name="eventName">The name of the observed event.</param>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="synchronizationContext">The <see cref="SynchronizationContext"/> that the event delegate is to be executed on.</param>
    /// <returns><c>true</c> when registration was successful, otherwise <c>false</c>.</returns>
    bool TryRegisterGlobalObserver(string eventName, Delegate eventHandler, SynchronizationContext synchronizationContext);

    /// <summary>
    /// Register an event delegate to handle a specific event which could be published by any type.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event args object.</typeparam>
    /// <param name="eventName">The name of the observed event.</param>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <returns><c>true</c> when registration was successful, otherwise <c>false</c>.</returns>
    bool TryRegisterGlobalObserver<TEventArgs>(string eventName, EventHandler<TEventArgs> eventHandler);

    /// <summary>
    /// Register an event delegate to handle a specific event which could be published by any type.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event args object.</typeparam>
    /// <param name="eventName">The name of the observed event.</param>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="isMarshalEventToCurrentThreadEnabled"><c>true</c> if the current <see cref="SynchronizationContext"/> should be captured to execute the event handler.</param>
    /// <returns><c>true</c> when registration was successful, otherwise <c>false</c>.</returns>
    bool TryRegisterGlobalObserver<TEventArgs>(string eventName, EventHandler<TEventArgs> eventHandler, bool isMarshalEventToCurrentThreadEnabled);

    /// <summary>
    /// Register an event delegate to handle a specific event which could be published by any type.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event args object.</typeparam>
    /// <param name="eventName">The name of the observed event.</param>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="synchronizationContext">The <see cref="SynchronizationContext"/> that the event delegate is to be executed on.</param>
    /// <returns><c>true</c> when registration was successful, otherwise <c>false</c>.</returns>
    bool TryRegisterGlobalObserver<TEventArgs>(string eventName, EventHandler<TEventArgs> eventHandler, SynchronizationContext synchronizationContext);

    /// <summary>
    /// Registers a handler for any registered event source with a compatible event delegate signature.
    /// </summary>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <returns><c>true</c> when registration was successful, otherwise <c>false</c>.</returns>
    bool TryRegisterGlobalObserver(Delegate eventHandler);

    /// <summary>
    /// Registers a handler for any registered event source with a compatible event delegate signature.
    /// </summary>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="isMarshalEventToCurrentThreadEnabled"><c>true</c> if the current <see cref="SynchronizationContext"/> should be captured to execute the event handler.</param>
    /// <returns><c>true</c> when registration was successful, otherwise <c>false</c>.</returns>
    bool TryRegisterGlobalObserver(Delegate eventHandler, bool isMarshalEventToCurrentThreadEnabled);

    /// <summary>
    /// Registers a handler for any registered event source with a compatible event delegate signature.
    /// </summary>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="synchronizationContext">The <see cref="SynchronizationContext"/> that the event delegate is to be executed on.</param>
    /// <returns><c>true</c> when registration was successful, otherwise <c>false</c>.</returns>
    bool TryRegisterGlobalObserver(Delegate eventHandler, SynchronizationContext synchronizationContext);

    /// <summary>
    /// Registers a handler for any registered event source with a compatible event delegate signature.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event args object.</typeparam>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <returns><c>true</c> when registration was successful, otherwise <c>false</c>.</returns>
    bool TryRegisterGlobalObserver<TEventArgs>(EventHandler<TEventArgs> eventHandler);

    /// <summary>
    /// Registers a handler for any registered event source with a compatible event delegate signature.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event args object.</typeparam>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="isMarshalEventToCurrentThreadEnabled"><c>true</c> if the current <see cref="SynchronizationContext"/> should be captured to execute the event handler.</param>
    /// <returns><c>true</c> when registration was successful, otherwise <c>false</c>.</returns>
    bool TryRegisterGlobalObserver<TEventArgs>(EventHandler<TEventArgs> eventHandler, bool isMarshalEventToCurrentThreadEnabled);

    /// <summary>
    /// Registers a handler for any registered event source with a compatible event delegate signature.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event args object.</typeparam>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="synchronizationContext">The <see cref="SynchronizationContext"/> that the event delegate is to be executed on.</param>
    /// <returns><c>true</c> when registration was successful, otherwise <c>false</c>.</returns>
    bool TryRegisterGlobalObserver<TEventArgs>(EventHandler<TEventArgs> eventHandler, SynchronizationContext synchronizationContext);

    /// <summary>
    /// Unregister the event publisher for a collection of specified events.
    /// </summary>
    /// <param name="eventSource">The event publisher instance.</param>
    /// <param name="eventNames">The names of the events to unregister.</param>
    /// <param name="removeEventObservers">If <c>true</c> removes all event listenerHandlerMap of the specified events. The value is <c>false</c> by default.</param>
    /// <returns><c>true</c> when removal was successful, otherwise <c>false</c>.</returns>
    bool TryRemoveObservable(
      object eventSource,
      IEnumerable<string> eventNames,
      bool removeEventObservers = false);

    /// <summary>
    /// Unregister the event publisher for all events.
    /// </summary>
    /// <param name="eventSource">The event publisher instance.</param>
    /// <param name="removeEventObservers">If <c>true</c> removes all event listenerHandlerMap of the specified events. The value is <c>false</c> by default.</param>
    /// <returns><c>true</c> when removal was successful, otherwise <c>false</c>.</returns>
    bool TryRemoveObservable(object eventSource, bool removeEventObservers = false);

    /// <summary>
    /// Removes the event handler for a specified event of a certain event publisher type.
    /// </summary>
    /// <param name="eventName">The event name of the event that the delegate is handling.</param>
    /// <param name="eventSourceType">The type of the event publisher.</param>
    /// <param name="eventHandler">The event handler to remove.</param>
    /// <returns><c>true</c> when removal was successful, otherwise <c>false</c>.</returns>
    bool TryRemoveObserver(string eventName, Type eventSourceType, Delegate eventHandler);

    /// <summary>
    /// Removes the event handler for a specified event of a certain event publisher type.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event args object.</typeparam>
    /// <param name="eventName">The event name of the event that the delegate is handling.</param>
    /// <param name="eventSourceType">The type of the event publisher.</param>
    /// <param name="eventHandler">The event handler to remove.</param>
    /// <returns><c>true</c> when removal was successful, otherwise <c>false</c>.</returns>
    bool TryRemoveObserver<TEventArgs>(string eventName, Type eventSourceType, EventHandler<TEventArgs> eventHandler);

    /// <summary>
    /// Removes all event handlers for a specified event no matter event publisher type.
    /// </summary>
    /// <param name="eventName">The event name of the event that the delegate is handling.</param>
    /// <returns><c>true</c> when removal was successful, otherwise <c>false</c>.</returns>
    bool TryRemoveAllObservers(string eventName);

    /// <summary>
    /// Removes all event handlers for a specific event publisher type and specific event.
    /// </summary>
    /// <param name="eventName">The event name of the event that the delegate is handling.</param>
    /// <param name="eventSourceType">The type of the event publisher.</param>
    /// <returns><c>true</c> when removal was successful, otherwise <c>false</c>.</returns>
    bool TryRemoveAllObservers(string eventName, Type eventSourceType);

    /// <summary>
    /// Removes all event handlers for a specified event publisher type.
    /// </summary>
    /// <param name="eventSourceType">The type of the event publisher.</param>
    /// <returns><c>true</c> when removal was successful, otherwise <c>false</c>.</returns>
    bool TryRemoveAllObservers(Type eventSourceType);

    /// <summary>
    /// Removes the event handler for a specified event no matter the event publisher type.
    /// </summary>
    /// <param name="eventName">The event name of the event that the delegate is handling.</param>
    /// <param name="eventHandler">The event handler to remove</param>
    /// <returns><c>true</c> when removal was successful, otherwise <c>false</c>.</returns>
    bool TryRemoveGlobalObserver(string eventName, Delegate eventHandler);

    /// <summary>
    /// Removes the event handler for a specified event no matter the event publisher type.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event args object.</typeparam>
    /// <param name="eventName">The event name of the event that the delegate is handling.</param>
    /// <param name="eventHandler">The event handler to remove</param>
    /// <returns><c>true</c> when removal was successful, otherwise <c>false</c>.</returns>
    bool TryRemoveGlobalObserver<TEventArgs>(string eventName, EventHandler<TEventArgs> eventHandler);

    /// <summary>
    /// Removes the event handler for all registered events with a compatible event delegate signature.
    /// </summary>
    /// <param name="eventHandler">The event handler to remove.</param>
    /// <returns><c>true</c> when removal was successful, otherwise <c>false</c>.</returns>
    bool TryRemoveGlobalObserver(Delegate eventHandler);

    /// <summary>
    /// Removes the event handler for all registered events with a compatible event delegate signature.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event args object.</typeparam>
    /// <param name="eventHandler">The event handler to remove.</param>
    /// <returns><c>true</c> when removal was successful, otherwise <c>false</c>.</returns>
    bool TryRemoveGlobalObserver<TEventArgs>(EventHandler<TEventArgs> eventHandler);
  }
}