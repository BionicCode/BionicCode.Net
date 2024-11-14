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
    /// Unregisters all static events of the event publisher. This method overload ignores all instance events by default. 
    /// <br/>If configured to include instance events then all event source instances of the specified type are removed.
    /// </summary>
    /// <param name="eventSourceType">The type of the event source for the static event.</param>
    /// <param name="removeAllEventObservers">Optional. If <see langword="true"/> removes all event listeners. The value is <see langword="false"/> by default.</param>
    /// <param name="includeInstanceEvents">Optional. If <see langword="true"/> all static and instance events will be removed. In case of instance events, this will essentially unregister the instance events of all registered event sources of type <paramref name="eventSourceType"/>. 
    /// If <see langword="false"/> only static events will be removed. The default is <see langword="false"/>.</param>
    /// <returns><see langword="true"/> when removal was successful, otherwise <see langword="false"/>.</returns>
    bool TryRemoveObservable(Type eventSourceType, bool removeAllEventObservers = false, bool includeInstanceEvents = false);

    /// <summary>
    /// Unregisters the specified static events of the event publisher. This method overload ignores all instance events by default. 
    /// <br/>If configured to include instance events then all event source instances of the specified type are removed.
    /// </summary>
    /// <param name="eventSourceType">The type of the event source for the static event.</param>
    /// <param name="eventNames">The names of the events to unregister. Pass <see langword="null"/> to unregister all static events.</param>
    /// <param name="removeAllEventObservers">Optional. If <see langword="true"/> removes all event listeners of the specified events. The value is <see langword="false"/> by default.</param>
    /// <param name="includeInstanceEvents">Optional. If <see langword="true"/> all static and instance events that match an event name contained in <paramref name="eventNames"/> will be removed. In case of instance events, this will essentially unregister the instance events of all registered event sources of type <paramref name="eventSourceType"/>. 
    /// If <see langword="false"/> only static events will be removed. The default is <see langword="false"/>.</param>
    /// <returns><see langword="true"/> when removal was successful, otherwise <see langword="false"/>.</returns>
    bool TryRemoveObservable(Type eventSourceType, bool removeAllEventObservers = false, bool includeInstanceEvents = false, params string[] eventNames);

    /// <summary>
    /// Unregisters the specified static events of the event publisher. This method overload ignores all instance events by default. 
    /// <br/>If configured to include instance events then all event source instances of the specified type are removed.
    /// </summary>
    /// <param name="eventSourceType">The type of the event source for the static event.</param>
    /// <param name="eventNames">The names of the events to unregister. Pass <see langword="null"/> to unregister all static events.</param>
    /// <param name="removeAllEventObservers">Optional. If <see langword="true"/> removes all event listeners of the specified events. The value is <see langword="false"/> by default.</param>
    /// <param name="includeInstanceEvents">Optional. If <see langword="true"/> all static and instance events that match an event name contained in <paramref name="eventNames"/> will be removed. In case of instance events, this will essentially unregister the instance events of all registered event sources that of type <paramref name="eventSourceType"/>. If <see langword="false"/> only static events will be removed. The default is <see langword="false"/>.</param>
    /// <returns><see langword="true"/> when removal was successful, otherwise <see langword="false"/>.</returns>
    bool TryRemoveObservable(Type eventSourceType, IEnumerable<string> eventNames, bool removeAllEventObservers = false, bool includeInstanceEvents = false);

    /// <summary>
    /// Unregisters all static events of the event publisher. This method overload ignores all instance events by default. 
    /// <br/>If configured to include instance events then all event source instances of the specified type are removed.
    /// </summary>
    /// <typeparam name="TEventSource">The type of the event source for the static event. If <paramref name="includeInstanceEvents"/> is <see langword="true"/> then this is the type of all event source instances.</typeparam>
    /// <param name="removeAllEventObservers">Optional. If <see langword="true"/> removes all event listeners. The value is <see langword="false"/> by default.</param>
    /// <param name="includeInstanceEvents">Optional. If <see langword="true"/> all static and instance events will be removed. In case of instance events, this will essentially unregister the instance events of all registered event sources of type <typeparamref name="TEventSource"/>. 
    /// If <see langword="false"/> only static events will be removed. The default is <see langword="false"/>.</param>
    /// <returns><see langword="true"/> when removal was successful, otherwise <see langword="false"/>.</returns>
    bool TryRemoveObservable<TEventSource>(bool removeAllEventObservers = false, bool includeInstanceEvents = false);

    /// <summary>
    /// Unregisters the specified static events of the event publisher. This method overload ignores all instance events by default. 
    /// <br/>If configured to include instance events then all event source instances of the specified type are removed.
    /// </summary>
    /// <typeparam name="TEventSource">The type of the event source for the static event. If <paramref name="includeInstanceEvents"/> is <see langword="true"/> then this is the type of all event source instances.</typeparam>
    /// <param name="eventNames">The names of the events to unregister. Pass <see langword="null"/> to unregister all static events.</param>
    /// <param name="removeAllEventObservers">Optional. If <see langword="true"/> removes all event listeners of the specified events. The value is <see langword="false"/> by default.</param>
    /// <param name="includeInstanceEvents">Optional. If <see langword="true"/> all static and instance events that match an event name contained in <paramref name="eventNames"/> will be removed. In case of instance events, this will essentially unregister the instance events of all registered event sources that of type <typeparamref name="TEventSource"/>. If <see langword="false"/> only static events will be removed. The default is <see langword="false"/>.</param>
    /// <returns><see langword="true"/> when removal was successful, otherwise <see langword="false"/>.</returns>
    bool TryRemoveObservable<TEventSource>(bool removeAllEventObservers = false, bool includeInstanceEvents = false, params string[] eventNames);

    /// <summary>
    /// Unregisters the specified static events of the event publisher. This method overload ignores all instance events by default. 
    /// <br/>If configured to include instance events then all event source instances of the specified type are removed.
    /// </summary>
    /// <typeparam name="TEventSource">The type of the event source for the static event. If <paramref name="includeInstanceEvents"/> is <see langword="true"/> then this is the type of all event source instances.</typeparam>
    /// <param name="eventNames">The names of the events to unregister. Pass <see langword="null"/> to unregister all static events.</param>
    /// <param name="removeAllEventObservers">Optional. If <see langword="true"/> removes all event listeners of the specified events. The value is <see langword="false"/> by default.</param>
    /// <param name="includeInstanceEvents">Optional. If <see langword="true"/> all static and instance events that match an event name contained in <paramref name="eventNames"/> will be removed. In case of instance events, this will essentially unregister the instance events of all registered event sources that of type <typeparamref name="TEventSource"/>. If <see langword="false"/> only static events will be removed. The default is <see langword="false"/>.</param>
    /// <returns><see langword="true"/> when removal was successful, otherwise <see langword="false"/>.</returns>
    bool TryRemoveObservable<TEventSource>(IEnumerable<string> eventNames, bool removeAllEventObservers = false, bool includeInstanceEvents = false);

    /// <summary>
    /// Unregister the specified events of the event publisher.This method overload ignores all static events by default. 
    /// <br/>Can be configured to include static events.
    /// </summary>
    /// <param name="eventSource">The event publisher instance. </param>
    /// <param name="eventNames">The names of the events to unregister. Pass <see langword="null"/> to unregister all events</param>
    /// <param name="removeAllEventObservers">Optional. If <see langword="true"/> removes all event listeners of the specified events. The value is <see langword="false"/> by default.</param>
    /// <param name="includeStaticEvents">Optional. If <see langword="true"/> all static and instance events that match an event name contained in <paramref name="eventNames"/> will be removed. If <see langword="false"/> only instance events will be removed. The default is <see langword="false"/>.</param>
    /// <returns><see langword="true"/> when removal was successful, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="eventSource"/> is <see langword="null"/>.</exception>
    /// <remarks>Use <see cref="TryRemoveObservable{TEventSource}(IEnumerable{string}, bool, bool)"/> or <see cref="TryRemoveObservable(Type, IEnumerable{string}, bool, bool)"/> to remove static events.</remarks>
    /// <br/>Alternatively, set <paramref name="includeStaticEvents"/> to <see langword="true"/>.</remarks>
    bool TryRemoveObservable(object eventSource, IEnumerable<string> eventNames, bool removeAllEventObservers = false, bool includeStaticEvents = false);

    /// <summary>
    /// Unregister the specified events of the event publisher.This method overload ignores all static events by default. 
    /// <br/>Can be configured to include static events.
    /// </summary>
    /// <param name="eventNames">The names of the events to unregister. Pass <see langword="null"/> to unregister all static events.</param>
    /// <param name="eventSource">The event publisher instance.</param>
    /// <param name="removeAllEventObservers">Optional. If <see langword="true"/> removes all event listeners of the specified events. The value is <see langword="false"/> by default.</param>
    /// <param name="includeStaticEvents">Optional. If <see langword="true"/> all static and instance events that match an event name contained in <paramref name="eventNames"/> will be removed. If <see langword="false"/> only instance events will be removed. The default is <see langword="false"/>.</param>
    /// <returns><see langword="true"/> when removal was successful, otherwise <see langword="false"/>.</returns>
    /// <remarks>Use <see cref="TryRemoveObservable{TEventSource}(bool, bool, string[])"/> or <see cref="TryRemoveObservable(Type, bool, bool, string[])"/> to remove static events.</remarks>
    /// <br/>Alternatively, set <paramref name="includeStaticEvents"/> to <see langword="true"/>.</remarks>
    bool TryRemoveObservable(object eventSource, bool removeAllEventObservers = false, bool includeStaticEvents = false, params string[] eventNames);

    /// <summary>
    /// Unregister all events of the event publisher.This method overload ignores all static events by default. 
    /// <br/>Can be configured to include static events.
    /// </summary>
    /// <param name="eventSource">The event publisher instance.</param>
    /// <param name="removeAllEventObservers">Optional. If <see langword="true"/> removes all event listeners of the specified events. The value is <see langword="false"/> by default.</param>
    /// <param name="includeStaticEvents">Optional. If <see langword="true"/> all static and instance events will be removed. If <see langword="false"/> only instance events will be removed. The default is <see langword="false"/>.</param>
    /// <returns><see langword="true"/> when removal was successful, otherwise <see langword="false"/>.</returns>
    /// <remarks>Use <see cref="TryRemoveObservable{TEventSource}(bool, bool)"/> or <see cref="TryRemoveObservable(Type, bool, bool)"/> to remove static events. 
    /// <br/>Alternatively, set <paramref name="includeStaticEvents"/> to <see langword="true"/>.</remarks>
    bool TryRemoveObservable(object eventSource, bool removeAllEventObservers = false, bool includeStaticEvents = false);

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