namespace BionicCode.Utilities.Net
{
  #region Info
  // //  
  // BionicUtilities.Net.Standard
  #endregion

  using System;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Diagnostics.Tracing;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using System.Threading;

  /// <inheritdoc />
  public class WeakEventAggregator : IWeakEventAggregator, IWeakEventAggregatorListener, IWeakEventAggregatorPublisher
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public WeakEventAggregator()
    {
      this.registrationService = new WeakEventRegistrationService();
    }

    #region Implementation of IWeakEventAggregator

    /// <inheritdoc />
    public void StartBroadcasting(object eventSource)
    {
      ArgumentNullExceptionEx.ThrowIfNull(eventSource, nameof(eventSource));

      StartBroadcastingInternal(eventSource, null);
    }

    /// <inheritdoc />
    public void StartBroadcasting(object eventSource, params string[] eventNames)
    {
      ArgumentNullExceptionEx.ThrowIfNull(eventSource, nameof(eventSource));
      ArgumentNullExceptionEx.ThrowIfNull(eventNames, nameof(eventNames));

      StartBroadcastingInternal(eventSource, eventNames);
    }

    /// <inheritdoc />
    public void StartBroadcasting(object eventSource, IEnumerable<string> eventNames)
    {
      ArgumentNullExceptionEx.ThrowIfNull(eventSource, nameof(eventSource));
      ArgumentNullExceptionEx.ThrowIfNull(eventNames, nameof(eventNames));

      StartBroadcastingInternal(eventSource, eventNames);
    }

    private void StartBroadcastingInternal(object eventSource, IEnumerable<string> eventNames)
    {
      if (eventNames is null)
      {
        this.registrationService.AddSourceInstance(eventSource);
      }
      else
      {
        foreach (string eventName in eventNames)
        {
          this.registrationService.AddSourceInstance(eventSource, eventName);
        }
      }
    }

    /// <inheritdoc />
    public void StopBroadcasting(object eventSource, params string[] eventNames)
    {
      ArgumentNullExceptionEx.ThrowIfNull(eventSource, nameof(eventSource));
      ArgumentNullExceptionEx.ThrowIfNull(eventNames, nameof(eventNames));

      StopBroadcastingInternal(eventSource, eventNames);
    }

    /// <inheritdoc />
    public void StopBroadcasting(object eventSource, IEnumerable<string> eventNames)
    {
      ArgumentNullExceptionEx.ThrowIfNull(eventSource, nameof(eventSource));
      ArgumentNullExceptionEx.ThrowIfNull(eventNames, nameof(eventNames));

      StopBroadcastingInternal(eventSource, eventNames);
    }

    /// <inheritdoc />
    public void StopBroadcasting(object eventSource)
    {
      ArgumentNullExceptionEx.ThrowIfNull(eventSource, nameof(eventSource));

      StopBroadcastingInternal(eventSource, null);
    }

    private void StopBroadcastingInternal(object eventSource, IEnumerable<string> eventNames)
    {
      if (eventNames is null)
      {
        this.registrationService.RemoveSourceInstance(eventSource);
      }
      else
      {
        foreach (string eventName in eventNames)
        {
          this.registrationService.RemoveSourceInstance(eventSource, eventName);
        }
      }
    }

    /// <inheritdoc /> 
    public void StartListening<TEventSource, TDelegate>(string eventName, TDelegate eventHandler) where TDelegate : Delegate
    {
      ArgumentExceptionEx.ThrowIfNullOrWhiteSpace(eventName, nameof(eventName));
      ArgumentNullExceptionEx.ThrowIfNull(eventHandler, nameof(eventHandler));

      StartListeningInternal<TEventSource>(eventName, eventHandler, null);
    }

    /// <inheritdoc /> 
    public void StartListening<TEventSource, TDelegate>(string eventName, TDelegate eventHandler, bool executeOnCurrentSynchronizationContext) where TDelegate : Delegate
    {
      ArgumentExceptionEx.ThrowIfNullOrWhiteSpace(eventName, nameof(eventName));
      ArgumentNullExceptionEx.ThrowIfNull(eventHandler, nameof(eventHandler));

      SynchronizationContext capturedSynchronizationContext = executeOnCurrentSynchronizationContext 
        ? SynchronizationContext.Current 
        : null;
      StartListeningInternal<TEventSource>(eventName, eventHandler, capturedSynchronizationContext);
    }

    /// <inheritdoc /> 
    public void StartListening<TEventSource, TDelegate>(string eventName, TDelegate eventHandler, SynchronizationContext synchronizationContext) where TDelegate : Delegate
    {
      ArgumentExceptionEx.ThrowIfNullOrWhiteSpace(eventName, nameof(eventName));
      ArgumentNullExceptionEx.ThrowIfNull(eventHandler, nameof(eventHandler));
      ArgumentNullExceptionEx.ThrowIfNull(synchronizationContext, nameof(synchronizationContext));

      StartListeningInternal<TEventSource>(eventName, eventHandler, synchronizationContext);
    }

    private void StartListeningInternal<TEventSource>(string eventName, Delegate eventHandler, SynchronizationContext synchronizationContext)
    {
      //ThrowIfEventHandlerInvalid(eventInfoTableEntry, eventHandler);

      Type eventHandlerType = eventHandler.GetType();
      IClientEventHandlerRegistrar clientEventHandlerRegistrar;
      bool isGenericEventHandler = eventHandlerType.IsGenericType;
      Type eventHandlerTypeDefinition = isGenericEventHandler ? eventHandlerType.GetGenericTypeDefinition() : null;
      if (eventHandlerType == typeof(EventHandler))
      {
        clientEventHandlerRegistrar = new EventHandlerRegistrar<TEventSource>((EventHandler)eventHandler, eventName, synchronizationContext);
      }
      else if (isGenericEventHandler && eventHandlerTypeDefinition == typeof(EventHandler<>))
      {
        Type eventArgsType = eventHandlerType.GetGenericArguments()[0];
        clientEventHandlerRegistrar = (IClientEventHandlerRegistrar)typeof(EventHandlerGenericRegistrar<,>).MakeGenericType(typeof(TEventSource), eventArgsType)
          .GetConstructor(new Type[] { eventHandlerType, typeof(string) })
          .Invoke(new object[] { eventHandler, eventName, synchronizationContext });
      }
      else if (isGenericEventHandler && eventHandlerTypeDefinition == typeof(Action<,>))
      {
        Type eventSenderType = eventHandlerType.GetGenericArguments()[0];
        Type eventArgsType = eventHandlerType.GetGenericArguments()[1];
        clientEventHandlerRegistrar = (IClientEventHandlerRegistrar)typeof(ActionRegistrar<,,>).MakeGenericType(typeof(TEventSource), eventSenderType, eventArgsType)
          .GetConstructor(new Type[] { eventHandlerType, typeof(string) })
          .Invoke(new object[] { eventHandler, eventName, synchronizationContext });
      }
      else
      {
        clientEventHandlerRegistrar = new AnonymousDelegateRegistrar<TEventSource>(eventHandler, eventName);
      }

      this.registrationService.RegisterHandler(clientEventHandlerRegistrar);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">The <paramref name="eventHandler"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="eventName"/> is <see langword="null"/> or an empty string.</exception>
    public void StopListening<TEventSource, TDelegate>(string eventName, TDelegate eventHandler) where TDelegate : Delegate
    {
      ArgumentExceptionEx.ThrowIfNullOrWhiteSpace(eventName, nameof(eventName));
      ArgumentNullExceptionEx.ThrowIfNull(eventHandler, nameof(eventHandler));

      this.registrationService.UnregisterHandler<TEventSource>(eventName, eventHandler);
    }

    private void ThrowIfEventHandlerInvalid<TEventSource>(EventInfoTableEntry<TEventSource> entry, Delegate eventHandler)
    {
      ParameterInfo[] invocatorParameters = entry.InvocatorMethod.GetParameters();
      ParameterInfo[] eventHandlerParameters = eventHandler.GetType().GetMethod("Invoke")?.GetParameters();
      if (eventHandlerParameters != null)
      {
        if (invocatorParameters.Length != eventHandlerParameters.Length)
        {
          throw new EventHandlerMismatchException($"Wrong event handler signature. The parameter count of the registered event handler does not match the event delegate {entry.EventInfo.EventHandlerType.FullName}.");
        }

        for (int index = 0; index < invocatorParameters.Length; index++)
        {
          Type invocatorParameterType = invocatorParameters[index].ParameterType;
          Type eventHandlerParameterType = eventHandlerParameters[index].ParameterType;
          if (!eventHandlerParameterType.IsAssignableFrom(invocatorParameterType))
          {
            throw new EventHandlerMismatchException($"Wrong event handler signature. The parameter eventHandlerTypeDefinition at index {index} of the registered event handler does not match the event delegate {entry.EventInfo.EventHandlerType.FullName}. Found eventHandlerTypeDefinition {eventHandlerParameterType.FullName}. Expected eventHandlerTypeDefinition {invocatorParameterType.FullName}.");
          }
        }
      }
    }

    #endregion Implementation of IWeakEventAggregator

    private readonly WeakEventRegistrationService registrationService;
  }
}