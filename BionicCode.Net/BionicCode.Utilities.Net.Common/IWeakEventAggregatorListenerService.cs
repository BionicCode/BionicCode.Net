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
    /// Register an event handler to handle a specified event that is published by a specific event source. 
    /// The events can have an arbitrary signature which does not have to follow the common C# event practices.
    /// </summary>
    /// <param name="eventName">The name of the observed event.</param>
    /// <typeparam name="TEventSource">The type of the event publisher object. This can be any derived type like a class or interface.</typeparam>
    /// <typeparam name="TDelegate">The type of the event handler.</typeparam>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <exception cref="EventHandlerMismatchException">The signature of the event handler and the event are incompatible. See remarks.</exception>   
    /// <exception cref="ArgumentNullException">The <paramref name="eventHandler"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="eventName"/> is <see langword="null"/> or an empty string.</exception>

    void StartListening<TEventSource, TDelegate>(string eventName, TDelegate eventHandler) where TDelegate : Delegate;

    /// <summary>
    /// Register an event handler to handle a specified event that is published by a specific event source. 
    /// The events can have an arbitrary signature which does not have to follow the common C# event practices.
    /// <br/>This overload supports marshalling to the current thread by capturing the <see cref="SynchronizationContext"/> of the caller.
    /// </summary>
    /// <param name="eventName">The name of the observed event.</param>
    /// <typeparam name="TEventSource">The type of the event publisher object. This can be any derived type like a class or interface.</typeparam>
    /// <typeparam name="TDelegate">The type of the event handler.</typeparam>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="isMarshalEventToCurrentThreadEnabled"><see langword="true"/> if the current <see cref="SynchronizationContext"/> should be captured to execute the event handler.</param>
    /// <returns><see langword="true"/> when registration was successful, otherwise <see langword="false"/>.</returns>
    /// <exception cref="EventHandlerMismatchException">The signature of the event handler and the event are incompatible. See remarks.</exception>   
    /// <exception cref="ArgumentNullException">The <paramref name="eventHandler"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="eventName"/> is <see langword="null"/> or an empty string.</exception>
    void StartListening<TEventSource, TDelegate>(string eventName, TDelegate eventHandler, bool isMarshalEventToCurrentThreadEnabled) where TDelegate : Delegate;

    /// <summary>
    /// Register an event handler to handle a specified event that is published by a specific event source. 
    /// The events can have an arbitrary signature which does not have to follow the common C# event practices.
    /// <br/>This overload supports marshalling to the specified <see cref="SynchronizationContext"/>.
    /// </summary>
    /// <param name="eventName">The name of the observed event.</param>
    /// <typeparam name="TEventSource">The type of the event publisher object. This can be any derived type like a class or interface.</typeparam>
    /// <typeparam name="TDelegate">The type of the event handler.</typeparam>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="synchronizationContext">The <see cref="SynchronizationContext"/> that the event delegate is to be executed on.</param>
    /// <exception cref="EventHandlerMismatchException">The signature of the event handler and the event are incompatible. See remarks.</exception>   
    /// <exception cref="ArgumentNullException">The <paramref name="eventHandler"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="synchronizationContext"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="eventName"/> is <see langword="null"/> or an empty string.</exception>
    void StartListening<TEventSource, TDelegate>(string eventName, TDelegate eventHandler, SynchronizationContext synchronizationContext) where TDelegate : Delegate;

    /// <summary>
    /// Tries to register an event handler to handle all compatible events that are published by a specific observable type. 
    /// The events can have an arbitrary signature which does not have to follow the common C# event practices.
    /// </summary>
    /// <typeparam name="TEventSource">The type of the event publisher object. This can be any derived type like a class or interface.</typeparam>
    /// <typeparam name="TDelegate">The type of the event handler.</typeparam>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <returns><see langword="true"/> when registration was successful, which means all events where compatible with the provided <paramref name="eventHandler"/>. If there is at least a single incompatible event, the method returns <see langword="false"/> but still registers the <paramref name="eventHandler"/> for all compatible events.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="eventHandler"/> is <see langword="null"/>.</exception>
    /// <remarks>This method will observe any event on the specified event source if the event delegate's signature is compatible to the provided <paramref name="eventHandler"/>.
    /// <br/>An event handler is considered compatible when all of the following conditions are true:
    /// <list type="bullet">
    /// <item>The parameter count matches the parameter count of the event delegate</item>
    /// <item>Each parameter is assignable to the event delegate's parameter at the same position.</item>
    /// </list>
    /// Events that are not compatible with the provided <paramref name="eventHandler"/> are ignored.
    /// <para></para> 
    /// Call <see cref="StartListeningAll{TEventSource, TDelegate}(TDelegate)"/> if an exception should be thrown if there are incompatible events.
    /// </remarks>
    bool TryStartListeningAll<TEventSource, TDelegate>(TDelegate eventHandler) where TDelegate : Delegate;

    /// <summary>
    /// Tries to register an event handler to handle all compatible events that are published by a specific observable type. 
    /// The events can have an arbitrary signature which does not have to follow the common C# event practices.
    /// <br/>This overload supports marshalling to the specified <see cref="SynchronizationContext"/>.
    /// </summary>
    /// <typeparam name="TEventSource">The type of the event publisher object. This can be any derived type like a class or interface.</typeparam>
    /// <typeparam name="TDelegate">The type of the event handler.</typeparam>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="synchronizationContext">The <see cref="SynchronizationContext"/> that the event delegate is to be executed on.</param>
    /// <returns><see langword="true"/> when registration was successful, which means all events where compatible with the provided <paramref name="eventHandler"/>. If there is at least a single incompatible event, the method returns <see langword="false"/> but still registers the <paramref name="eventHandler"/> for all compatible events.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="eventHandler"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="synchronizationContext"/> is <see langword="null"/>.</exception>
    /// <remarks>This method will observe any event on the specified event source if the event delegate's signature is compatible to the provided <paramref name="eventHandler"/>.
    /// <br/>An event handler is considered compatible when all of the following conditions are true:
    /// <list type="bullet">
    /// <item>The parameter count matches the parameter count of the event delegate</item>
    /// <item>Each parameter is assignable to the event delegate's parameter at the same position.</item>
    /// </list>
    /// Events that are not compatible with the provided <paramref name="eventHandler"/> are ignored.
    /// <para></para> 
    /// Call <see cref="StartListeningAll{TEventSource, TDelegate}(TDelegate, SynchronizationContext)"/> if an exception should be thrown if there are incompatible events.
    /// </remarks>
    bool TryStartListeningAll<TEventSource, TDelegate>(TDelegate eventHandler, SynchronizationContext synchronizationContext) where TDelegate : Delegate;

    /// <summary>
    /// Registers an event handler to handle all events that are published by a specific observable type. 
    /// The events can have an arbitrary signature which does not have to follow the common C# event practices.
    /// <br/>This overload supports marshalling to the specified <see cref="SynchronizationContext"/>.
    /// </summary>
    /// <typeparam name="TEventSource">The type of the event publisher object. This can be any derived type like a class or interface.</typeparam>
    /// <typeparam name="TDelegate">The type of the event handler.</typeparam>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <param name="synchronizationContext">The <see cref="SynchronizationContext"/> that the event delegate is to be executed on.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="eventHandler"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="synchronizationContext"/> is <see langword="null"/>.</exception>
    /// <exception cref="EventHandlerMismatchException">The <paramref name="eventHandler"/> is not compatible to an event of the event source. See remarks.</exception>
    /// <remarks>This method will observe any event on the specified event source if the event delegate's signature is compatible to the provided <paramref name="eventHandler"/>.
    /// <br/>An event handler is considered compatible when all of the following conditions are true:
    /// <list type="bullet">
    /// <item>The parameter count matches the parameter count of the event delegate</item>
    /// <item>Each parameter is assignable to the event delegate's parameter at the same position.</item>
    /// </list>
    /// This method will throw an <see cref="EventHandlerMismatchException"/> if there are any events that are not compatible with the provided <paramref name="eventHandler"/>.
    /// <para></para> 
    /// Call <see cref="TryStartListeningAll{TEventSource, TDelegate}(TDelegate, SynchronizationContext)"/> if a <see langword="bool"/> value to indicate incompatible events should be returned instead of throwing an exception.
    /// </remarks>
    void StartListeningAll<TEventSource, TDelegate>(TDelegate eventHandler, SynchronizationContext synchronizationContext) where TDelegate : Delegate;

    /// <summary>
    /// Registers an event handler to handle all events that are published by a specific observable type. 
    /// The events can have an arbitrary signature which does not have to follow the common C# event practices.
    /// </summary>
    /// <typeparam name="TEventSource">The type of the event publisher object. This can be any derived type like a class or interface.</typeparam>
    /// <typeparam name="TDelegate">The type of the event handler.</typeparam>
    /// <param name="eventHandler">A delegate that handles the specified event.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="eventHandler"/> is <see langword="null"/>.</exception>
    /// <exception cref="EventHandlerMismatchException">The <paramref name="eventHandler"/> is not compatible to an event of the event source. See remarks.</exception>
    /// <remarks>This method will observe any event on the specified event source if the event delegate's signature is compatible to the provided <paramref name="eventHandler"/>.
    /// <br/>An event handler is considered compatible when all of the following conditions are true:
    /// <list type="bullet">
    /// <item>The parameter count matches the parameter count of the event delegate</item>
    /// <item>Each parameter is assignable to the event delegate's parameter at the same position.</item>
    /// </list>
    /// This method will throw an <see cref="EventHandlerMismatchException"/> if there are any events that are not compatible with the provided <paramref name="eventHandler"/>.
    /// <para></para> 
    /// Call <see cref="TryStartListeningAll{TEventSource, TDelegate}(TDelegate)"/> if a <see langword="bool"/> value to indicate incompatible events should be returned instead of throwing an exception.
    /// </remarks>
    void StartListeningAll<TEventSource, TDelegate>(TDelegate eventHandler) where TDelegate : Delegate;

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