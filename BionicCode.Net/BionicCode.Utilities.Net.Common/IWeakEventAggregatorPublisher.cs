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
    /// <returns><see langword="true"/> when registration was successful, otherwise <see langword="false"/>.</returns>
    bool TryRegisterObservable(object eventSource, IEnumerable<string> eventNames);


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
  }
}