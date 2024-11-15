namespace BionicCode.Utilities.Net
{
  using System;
  using System.Threading;

  /// <summary>
  /// Interface that encapsulates the listener side API. For example, use this interface to constrain the access to the provided the <see cref="WeakEventAggregator"/> e.g., in context of dependency injection.
  /// </summary>
  public interface IWeakEventAggregatorListener
  {
    /// <summary>
    /// Registers an event delegate, with a random signature that does not follow the common C# event practices, to handle a specific event published by a specific observable type.
    /// </summary>
    /// <param name="eventName">The name of the observed event.</param>
    /// <typeparam name="TEventSource">The type of the event publisher object.</typeparam>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <returns><see langword="true"/> when registration was successful, otherwise <see langword="false"/>.</returns>
    /// <exception cref="EventHandlerMismatchException">The signature of the event handler and the event are incompatible.</exception>
    void RegisterObserver<TEventSource>(string eventName, Delegate eventHandler);

    /// <summary>
    /// Registers an event delegate, with a random signature that does not follow the common C# event practices, to handle a specific event published by a specific observable type.
    /// <br/>This overload supports marshaling to the current thread by capturing the <see cref="SynchronizationContext"/> of the caller.
    /// </summary>
    /// <param name="eventName">The name of the observed event.</param>
    /// <typeparam name="TEventSource">The type of the event publisher object.</typeparam>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="isMarshalEventToCurrentThreadEnabled"><see langword="true"/> if the current <see cref="SynchronizationContext"/> should be captured to execute the event handler.</param>
    /// <returns><see langword="true"/> when registration was successful, otherwise <see langword="false"/>.</returns>
    void RegisterObserver<TEventSource>(string eventName, Delegate eventHandler, bool isMarshalEventToCurrentThreadEnabled);

    /// <summary>
    /// Registers an event delegate, with a random signature that does not follow the common C# event practices, to handle a specific event published by a specific observable type.
    /// <br/>This overload supports marshaling to the specified <see cref="SynchronizationContext"/>.
    /// </summary>
    /// <param name="eventName">The name of the observed event.</param>
    /// <typeparam name="TEventSource">The type of the event publisher object.</typeparam>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="synchronizationContext">The <see cref="SynchronizationContext"/> that the event delegate is to be executed on.</param>
    /// <returns><see langword="true"/> when registration was successful, otherwise <see langword="false"/>.</returns>
    void RegisterObserver<TEventSource>(string eventName, Delegate eventHandler, SynchronizationContext synchronizationContext);

    ///// <summary>
    ///// Registers an event delegate of type <see cref="EventHandler{TEventArgs}"/> to handle a specific event published by a specific publisher type.
    ///// </summary>
    ///// <param name="eventName">The name of the observed event.</param>
    ///// <typeparam name="TEventSource">The type of the event publisher object.</typeparam>
    ///// <typeparam name="TEventArgs">The type of the event handler's event object.</typeparam>
    ///// <param name="eventHandler">A delegate that handles the specified event.</param>
    ///// <returns><see langword="true"/> when registration was successful, otherwise <see langword="false"/>.</returns>
    //void RegisterObserver<TEventSource, TEventArgs>(string eventName, EventHandler<TEventArgs> eventHandler);

    ///// <summary>
    ///// Registers an event delegate of type <see cref="EventHandler{TEventArgs}"/> to handle a specific event published by a specific publisher type.
    ///// <br/>This overload supports marshaling to the current thread by capturing the <see cref="SynchronizationContext"/> of the caller.
    ///// </summary>
    ///// <param name="eventName">The name of the observed event.</param>
    ///// <typeparam name="TEventSource">The type of the event publisher object.</typeparam>
    ///// <typeparam name="TEventArgs">The type of the event handler's event object.</typeparam>
    ///// <param name="eventHandler">A delegate that handles the specified event.</param>
    ///// <param name="isMarshalEventToCurrentThreadEnabled"><see langword="true"/> if the current <see cref="SynchronizationContext"/> should be captured to execute the event handler.</param>
    ///// <returns><see langword="true"/> when registration was successful, otherwise <see langword="false"/>.</returns>
    //void RegisterObserver<TEventSource, TEventArgs>(string eventName, EventHandler<TEventArgs> eventHandler, bool isMarshalEventToCurrentThreadEnabled);

    ///// <summary>
    ///// Registers an event delegate of type <see cref="EventHandler{TEventArgs}"/> to handle a specific event published by a specific publisher type.
    ///// <br/>This overload supports marshaling to the specified <see cref="SynchronizationContext"/>.
    ///// </summary>
    ///// <param name="eventName">The name of the observed event.</param>
    ///// <typeparam name="TEventSource">The type of the event publisher object.</typeparam>
    ///// <typeparam name="TEventArgs">The type of the event handler's event object.</typeparam>
    ///// <param name="eventHandler">A delegate that handles the specified event.</param>
    ///// <param name="synchronizationContext">The <see cref="SynchronizationContext"/> that the event delegate is to be executed on.</param>
    ///// <returns><see langword="true"/> when registration was successful, otherwise <see langword="false"/>.</returns>
    //void RegisterObserver<TEventSource, TEventArgs>(string eventName, EventHandler<TEventArgs> eventHandler, SynchronizationContext synchronizationContext);

    ///// <summary>
    ///// Registers an event delegate of type <see cref="EventHandler"/> to handle a specific event published by a specific publisher type.
    ///// </summary>
    ///// <param name="eventName">The name of the observed event.</param>
    ///// <typeparam name="TEventSource">The type of the event publisher object.</typeparam>
    ///// <param name="eventHandler">A delegate that handles the specified event.</param>
    ///// <returns><see langword="true"/> when registration was successful, otherwise <see langword="false"/>.</returns>
    //void RegisterObserver<TEventSource>(string eventName, EventHandler eventHandler);

    ///// <summary>
    ///// Registers an event delegate of type <see cref="EventHandler"/> to handle a specific event published by a specific publisher type.
    ///// <br/>This overload supports marshaling to the current thread by capturing the <see cref="SynchronizationContext"/> of the caller.
    ///// </summary>
    ///// <param name="eventName">The name of the observed event.</param>
    ///// <typeparam name="TEventSource">The type of the event publisher object.</typeparam>
    ///// <param name="eventHandler">A delegate that handles the specified event.</param>
    ///// <param name="isMarshalEventToCurrentThreadEnabled"><see langword="true"/> if the current <see cref="SynchronizationContext"/> should be captured to execute the event handler.</param>
    ///// <returns><see langword="true"/> when registration was successful, otherwise <see langword="false"/>.</returns>
    //void RegisterObserver<TEventSource>(string eventName, EventHandler eventHandler, bool isMarshalEventToCurrentThreadEnabled);

    ///// <summary>
    ///// Registers an event delegate of type <see cref="EventHandler"/> to handle a specific event published by a specific publisher type.
    ///// <br/>This overload supports marshaling to the specified <see cref="SynchronizationContext"/>.
    ///// </summary>
    ///// <param name="eventName">The name of the observed event.</param>
    ///// <typeparam name="TEventSource">The type of the event publisher object.</typeparam>
    ///// <param name="eventHandler">A delegate that handles the specified event.</param>
    ///// <param name="synchronizationContext">The <see cref="SynchronizationContext"/> that the event delegate is to be executed on.</param>
    ///// <returns><see langword="true"/> when registration was successful, otherwise <see langword="false"/>.</returns>
    //void RegisterObserver<TEventSource>(string eventName, EventHandler eventHandler, SynchronizationContext synchronizationContext);

    /// <summary>
    /// Register an event delegate to handle a specific event which could be published by any type.
    /// </summary>
    /// <typeparam name="TEventSource">The type of the event publisher object.</typeparam>
    /// <typeparam name="TEventArgs">The type of the event handler's event object.</typeparam>
    /// <param name="eventName">The name of the observed event.</param>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <returns><see langword="true"/> when registration was successful, otherwise <see langword="false"/>.</returns>
    void RegisterGlobalObserver<TEventSource, TEventArgs>(string eventName, Delegate eventHandler);

    /// <summary>
    /// Register an event delegate to handle a specific event which could be published by any instance of a specified type.
    /// <br/>This overload supports marshaling to the current thread by capturing the <see cref="SynchronizationContext"/> of the caller.
    /// </summary>
    /// <typeparam name="TEventSource">The type of the event publisher object.</typeparam>
    /// <typeparam name="TEventArgs">The type of the event handler's event object.</typeparam>
    /// <param name="eventName">The name of the observed event.</param>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="isMarshalEventToCurrentThreadEnabled"><see langword="true"/> if the current <see cref="SynchronizationContext"/> should be captured to execute the event handler.</param>
    /// <returns><see langword="true"/> when registration was successful, otherwise <see langword="false"/>.</returns>
    void RegisterGlobalObserver<TEventSource, TEventArgs>(string eventName, Delegate eventHandler, bool isMarshalEventToCurrentThreadEnabled);

    /// <summary>
    /// Register an event delegate to handle a specific event which could be published by any type.
    /// <br/>This overload supports marshaling to the specified <see cref="SynchronizationContext"/>.
    /// </summary>
    /// <param name="eventName">The name of the observed event.</param>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="synchronizationContext">The <see cref="SynchronizationContext"/> that the event delegate is to be executed on.</param>
    /// <returns><see langword="true"/> when registration was successful, otherwise <see langword="false"/>.</returns>
    void RegisterGlobalObserver<TEventSource, TEventArgs>(string eventName, Delegate eventHandler, SynchronizationContext synchronizationContext);

    /// <summary>
    /// Register an event delegate to handle a specific event which could be published by any type.
    /// </summary>
    /// <typeparam name="TEventSource">The type of the event publisher object.</typeparam>
    /// <typeparam name="TEventArgs">The type of the event handler's event object.</typeparam>
    /// <param name="eventName">The name of the observed event.</param>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <returns><see langword="true"/> when registration was successful, otherwise <see langword="false"/>.</returns>
    void TryRegisterGlobalObserver<TEventSource, TEventArgs>(string eventName, EventHandler<TEventArgs> eventHandler);

    /// <summary>
    /// Register an event delegate to handle a specific event which could be published by any type.
    /// <br/>This overload supports marshaling to the current thread by capturing the <see cref="SynchronizationContext"/> of the caller.
    /// </summary>
    /// <typeparam name="TEventSource">The type of the event publisher object.</typeparam>
    /// <typeparam name="TEventArgs">The type of the event handler's event object.</typeparam>
    /// <param name="eventName">The name of the observed event.</param>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="isMarshalEventToCurrentThreadEnabled"><see langword="true"/> if the current <see cref="SynchronizationContext"/> should be captured to execute the event handler.</param>
    /// <returns><see langword="true"/> when registration was successful, otherwise <see langword="false"/>.</returns>
    void TryRegisterGlobalObserver<TEventSource, TEventArgs>(string eventName, EventHandler<TEventArgs> eventHandler, bool isMarshalEventToCurrentThreadEnabled);

    /// <summary>
    /// Register an event delegate to handle a specific event which could be published by any type.
    /// <br/>This overload supports marshaling to the specified <see cref="SynchronizationContext"/>.
    /// </summary>
    /// <typeparam name="TEventSource">The type of the event publisher object.</typeparam>
    /// <typeparam name="TEventArgs">The type of the event handler's event object.</typeparam>
    /// <param name="eventName">The name of the observed event.</param>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="synchronizationContext">The <see cref="SynchronizationContext"/> that the event delegate is to be executed on.</param>
    /// <returns><see langword="true"/> when registration was successful, otherwise <see langword="false"/>.</returns>
    void TryRegisterGlobalObserver<TEventSource, TEventArgs>(string eventName, EventHandler<TEventArgs> eventHandler, SynchronizationContext synchronizationContext);

    /// <summary>
    /// Registers a handler for any registered event source with a compatible event delegate signature.
    /// </summary>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <returns><see langword="true"/> when registration was successful, otherwise <see langword="false"/>.</returns>
    void TryRegisterGlobalObserver(Delegate eventHandler);

    /// <summary>
    /// Registers a handler for any registered event source with a compatible event delegate signature.
    /// <br/>This overload supports marshaling to the current thread by capturing the <see cref="SynchronizationContext"/> of the caller.
    /// </summary>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="isMarshalEventToCurrentThreadEnabled"><see langword="true"/> if the current <see cref="SynchronizationContext"/> should be captured to execute the event handler.</param>
    /// <returns><see langword="true"/> when registration was successful, otherwise <see langword="false"/>.</returns>
    void TryRegisterGlobalObserver(Delegate eventHandler, bool isMarshalEventToCurrentThreadEnabled);

    /// <summary>
    /// Registers a handler for any registered event source with a compatible event delegate signature.
    /// <br/>This overload supports marshaling to the specified <see cref="SynchronizationContext"/>.
    /// </summary>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="synchronizationContext">The <see cref="SynchronizationContext"/> that the event delegate is to be executed on.</param>
    /// <returns><see langword="true"/> when registration was successful, otherwise <see langword="false"/>.</returns>
    void TryRegisterGlobalObserver(Delegate eventHandler, SynchronizationContext synchronizationContext);

    /// <summary>
    /// Registers a handler for any registered event source with a compatible event delegate signature.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the handler's event args object.</typeparam>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <returns><see langword="true"/> when registration was successful, otherwise <see langword="false"/>.</returns>
    void TryRegisterGlobalObserver<TEventArgs>(EventHandler<TEventArgs> eventHandler);

    /// <summary>
    /// Registers a handler for any registered event source with a compatible event delegate signature.
    /// <br/>This overload supports marshaling to the current thread by capturing the <see cref="SynchronizationContext"/> of the caller.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the handler's event args object.</typeparam>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="isMarshalEventToCurrentThreadEnabled"><see langword="true"/> if the current <see cref="SynchronizationContext"/> should be captured to execute the event handler.</param>
    /// <returns><see langword="true"/> when registration was successful, otherwise <see langword="false"/>.</returns>
    void TryRegisterGlobalObserver<TEventArgs>(EventHandler<TEventArgs> eventHandler, bool isMarshalEventToCurrentThreadEnabled);

    /// <summary>
    /// Registers a handler for any registered event source with a compatible event delegate signature.
    /// <br/>This overload supports marshaling to the specified <see cref="SynchronizationContext"/>.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the handler's event args object.</typeparam>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="synchronizationContext">The <see cref="SynchronizationContext"/> that the event delegate is to be executed on.</param>
    /// <returns><see langword="true"/> when registration was successful, otherwise <see langword="false"/>.</returns>
    void TryRegisterGlobalObserver<TEventArgs>(EventHandler<TEventArgs> eventHandler, SynchronizationContext synchronizationContext);

    /// <summary>
    /// Removes the event handler for a specified event of a certain event publisher type.
    /// </summary>
    /// <param name="eventName">The event name of the event that the delegate is handling.</param>
    /// <typeparam name="TEventSource">The type of the event publisher object.</typeparam>
    /// <param name="eventHandler">The event handler to remove.</param>
    /// <returns><see langword="true"/> when removal was successful, otherwise <see langword="false"/>.</returns>
    bool TryRemoveObserver<TEventSource>(string eventName, Delegate eventHandler);

    /// <summary>
    /// Removes all event handlers for a specified event no matter event publisher's type.
    /// </summary>
    /// <param name="eventName">The event name of the event that the delegate is handling.</param>
    /// <returns><see langword="true"/> when removal was successful, otherwise <see langword="false"/>.</returns>
    bool TryRemoveAllObservers(string eventName);

    /// <summary>
    /// Removes all event handlers for a specific event publisher type and specific event.
    /// </summary>
    /// <param name="eventName">The event name of the event that the delegate is handling.</param>
    /// <typeparam name="TEventSource">The type of the event publisher object.</typeparam>
    /// <returns><see langword="true"/> when removal was successful, otherwise <see langword="false"/>.</returns>
    bool TryRemoveAllObservers<TEventSource>(string eventName);

    /// <summary>
    /// Removes all event handlers for a specified event publisher type.
    /// </summary>
    /// <typeparam name="TEventSource">The type of the event publisher object.</typeparam>
    /// <returns><see langword="true"/> when removal was successful, otherwise <see langword="false"/>.</returns>
    bool TryRemoveAllObservers<TEventSource>();

    /// <summary>
    /// Removes the event handler for a specified event from all publisher instances of the specified type.
    /// </summary>
    /// <typeparam name="TEventSource">The type of the event publisher object.</typeparam>
    /// <param name="eventName">The event name of the event that the delegate is handling.</param>
    /// <param name="eventHandler">The event handler to remove</param>
    /// <returns><see langword="true"/> when removal was successful, otherwise <see langword="false"/>.</returns>
    bool TryRemoveGlobalObserver<TEventSource>(string eventName, Delegate eventHandler);

    /// <summary>
    /// Removes the event handler for a specified event no matter the event publisher's type.
    /// </summary>
    /// <param name="eventName">The event name of the event that the delegate is handling.</param>
    /// <param name="eventHandler">The event handler to remove</param>
    /// <returns><see langword="true"/> when removal was successful, otherwise <see langword="false"/>.</returns>
    bool TryRemoveGlobalObserver(string eventName, Delegate eventHandler);

    /// <summary>
    /// Removes the event handler for all registered events with a compatible event delegate signature.
    /// </summary>
    /// <param name="eventHandler">The event handler to remove.</param>
    /// <returns><see langword="true"/> when removal was successful, otherwise <see langword="false"/>.</returns>
    bool TryRemoveGlobalObserver(Delegate eventHandler);
  }
}