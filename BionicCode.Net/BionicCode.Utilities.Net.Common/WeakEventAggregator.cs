namespace BionicCode.Utilities.Net
{
  #region Info
  // //  
  // BionicUtilities.Net.Standard
  #endregion

  using System;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Diagnostics.Tracing;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using System.Threading;

  /// <inheritdoc />
  public class WeakEventAggregator : IWeakEventAggregator, IWeakEventAggregatorListenerService, IWeakEventAggregatorPublisherService
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
    public void StopBroadcasting(object eventSource, bool removeListeners, params string[] eventNames)
    {
      ArgumentNullExceptionEx.ThrowIfNull(eventSource, nameof(eventSource));
      ArgumentNullExceptionEx.ThrowIfNull(eventNames, nameof(eventNames));

      StopBroadcastingInternal(eventSource, eventNames, removeListeners);
    }

    /// <inheritdoc />
    public void StopBroadcasting(object eventSource, IEnumerable<string> eventNames, bool removeListeners)
    {
      ArgumentNullExceptionEx.ThrowIfNull(eventSource, nameof(eventSource));
      ArgumentNullExceptionEx.ThrowIfNull(eventNames, nameof(eventNames));

      StopBroadcastingInternal(eventSource, eventNames, removeListeners);
    }

    /// <inheritdoc />
    public void StopBroadcasting(object eventSource, bool removeListeners)
    {
      ArgumentNullExceptionEx.ThrowIfNull(eventSource, nameof(eventSource));

      StopBroadcastingInternal(eventSource, null, removeListeners);
    }

    private void StopBroadcastingInternal(object eventSource, IEnumerable<string> eventNames, bool removeListeners)
    {
      if (eventNames is null)
      {
        // Stop broadcasting all events
        this.registrationService.RemoveSourceInstance(eventSource, removeListeners);
      }
      else
      {
        // Stop broadcasting specified events
        foreach (string eventName in eventNames)
        {
          this.registrationService.RemoveSourceInstance(eventSource, eventName, removeListeners);
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

    /// <inheritdoc /> 
    public bool TryStartListeningAll<TEventSource, TDelegate>(TDelegate eventHandler) where TDelegate : Delegate
    {
      ArgumentNullExceptionEx.ThrowIfNull(eventHandler, nameof(eventHandler));

      return TryStartListeningAllInternal<TEventSource, TDelegate>(eventHandler, null);
    }

    /// <inheritdoc /> 
    public bool TryStartListeningAll<TEventSource, TDelegate>(TDelegate eventHandler, SynchronizationContext synchronizationContext) where TDelegate : Delegate
    {
      ArgumentNullExceptionEx.ThrowIfNull(eventHandler, nameof(eventHandler));
      ArgumentNullExceptionEx.ThrowIfNull(synchronizationContext, nameof(synchronizationContext));

      return TryStartListeningAllInternal<TEventSource, TDelegate>(eventHandler, synchronizationContext);
    }

    /// <inheritdoc /> 
    public void StartListeningAll<TEventSource, TDelegate>(TDelegate eventHandler) where TDelegate : Delegate
    {
      ArgumentNullExceptionEx.ThrowIfNull(eventHandler, nameof(eventHandler));

      StartListeningAllInternal<TEventSource, TDelegate>(eventHandler, null);
    }

    /// <inheritdoc /> 
    public void StartListeningAll<TEventSource, TDelegate>(TDelegate eventHandler, SynchronizationContext synchronizationContext) where TDelegate : Delegate
    {
      ArgumentNullExceptionEx.ThrowIfNull(eventHandler, nameof(eventHandler));
      ArgumentNullExceptionEx.ThrowIfNull(synchronizationContext, nameof(synchronizationContext));

      StartListeningAllInternal<TEventSource, TDelegate>(eventHandler, synchronizationContext);
    }

    private void StartListeningInternal<TEventSource>(string eventName, Delegate eventHandler, SynchronizationContext synchronizationContext)
    {
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
        var registrarFactoryCacheKey = new RegistrarCacheKey(typeof(TEventSource), eventArgsType);
        if (!WeakEventAggregator.RegistrarCache.TryGetValue(registrarFactoryCacheKey, out Func<Delegate, string, SynchronizationContext, IClientEventHandlerRegistrar> registrarFactory))
        {
          registrarFactory = GenerateRegistrarFactory(eventHandlerType, typeof(TEventSource), typeof(EventHandlerGenericRegistrar<,>), null, eventArgsType);
          WeakEventAggregator.RegistrarCache.Add(registrarFactoryCacheKey, registrarFactory);
        }

        clientEventHandlerRegistrar = registrarFactory.Invoke(eventHandler, eventName, synchronizationContext);
      }
      else if (isGenericEventHandler && eventHandlerTypeDefinition == typeof(Action<,>))
      {
        Type eventSenderType = eventHandlerType.GetGenericArguments()[0];
        Type eventArgsType = eventHandlerType.GetGenericArguments()[1];
        var registrarFactoryCacheKey = new RegistrarCacheKey(eventSenderType, eventArgsType);
        if (!WeakEventAggregator.RegistrarCache.TryGetValue(registrarFactoryCacheKey, out Func<Delegate, string, SynchronizationContext, IClientEventHandlerRegistrar> registrarFactory))
        {
          registrarFactory = GenerateRegistrarFactory(eventHandlerType, typeof(TEventSource), typeof(ActionRegistrar<,,>), eventSenderType, eventArgsType);
          WeakEventAggregator.RegistrarCache.Add(registrarFactoryCacheKey, registrarFactory);
        }

        clientEventHandlerRegistrar = registrarFactory.Invoke(eventHandler, eventName, synchronizationContext);
      }
      else
      {
        clientEventHandlerRegistrar = new AnonymousDelegateRegistrar<TEventSource>(eventHandler, eventName);
      }

      this.registrationService.RegisterHandler(clientEventHandlerRegistrar);
    }

    private bool TryStartListeningAllInternal<TEventSource, TDelegate>(TDelegate eventHandler, SynchronizationContext synchronizationContext) where TDelegate : Delegate
    {
      ITypeDataCacheKey key = SymbolReflectionInfoCache.CreateTypeSymbolCacheKey(typeof(TEventSource).TypeHandle);
      TypeData eventSourceData;
      try
      {
        SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(key, out eventSourceData);
      }
      catch (ArgumentException)
      {
        return false;
      }

      IEnumerable<EventData> allEventsOfEventSource = eventSourceData.EnumerateEvents();
      bool hasIncompatibleEvents = false;
      foreach (EventData eventData in allEventsOfEventSource)
      {
        if (eventHandler.IsAssignable(eventData.GetEventInfo()))
        {
          StartListeningInternal<TEventSource>(eventData.Name, eventHandler, synchronizationContext);

          Debug.WriteLine($"WeakEventAggregator: Registered event handler for event {eventData.Name}.");
        }
        else
        {
          hasIncompatibleEvents = true;
        }
      }

      return !hasIncompatibleEvents;
    }

    private void StartListeningAllInternal<TEventSource, TDelegate>(TDelegate eventHandler, SynchronizationContext synchronizationContext) where TDelegate : Delegate
    {
      ITypeDataCacheKey key = SymbolReflectionInfoCache.CreateTypeSymbolCacheKey(typeof(TEventSource).TypeHandle);
      SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(key, out TypeData eventSourceData);
      IEnumerable<EventData> allEventsOfEventSource = eventSourceData.EnumerateEvents();
      foreach (EventData eventData in allEventsOfEventSource)
      {
        ArgumentExceptionEx.ThrowIfNotAssignable(eventData.GetEventInfo(), eventHandler);
        StartListeningInternal<TEventSource>(eventData.Name, eventHandler, synchronizationContext);

        Debug.WriteLine($"Registered event handler for event {eventData.Name}.");
      }
    }

    private Func<Delegate, string, SynchronizationContext, IClientEventHandlerRegistrar> GenerateRegistrarFactory(Type eventHandlerType, Type eventSourceType, Type registrarOpenType, Type eventSenderType, Type eventArgsType)
    {
      ConstructorInfo constructorInfo = eventSenderType != null
        ? registrarOpenType.MakeGenericType(eventSourceType, eventSenderType, eventArgsType)
            .GetConstructor(new Type[] { eventHandlerType, typeof(string), typeof(SynchronizationContext) })
        : registrarOpenType.MakeGenericType(eventSourceType, eventArgsType)
            .GetConstructor(new Type[] { eventHandlerType, typeof(string), typeof(SynchronizationContext) });

      ParameterExpression delegateParameterExpression = Expression.Parameter(typeof(Delegate), "eventHandler");
      UnaryExpression eventHandlerParameterExpression = Expression.TypeAs(delegateParameterExpression, eventHandlerType);
      ParameterExpression eventNameParameterExpression = Expression.Parameter(typeof(string), "eventName");
      ParameterExpression synchronizationContextParameterExpression = Expression.Parameter(typeof(SynchronizationContext), "synchronizationContext");
      NewExpression registrarConstructorExpression = Expression.New(constructorInfo, eventHandlerParameterExpression, eventNameParameterExpression, synchronizationContextParameterExpression);
      MemberInitExpression memberInitExpression = Expression.MemberInit(registrarConstructorExpression);
      Func<Delegate, string, SynchronizationContext, IClientEventHandlerRegistrar>  eventSourceHandler = Expression.Lambda<Func<Delegate, string, SynchronizationContext, IClientEventHandlerRegistrar>>(memberInitExpression, delegateParameterExpression, eventNameParameterExpression, synchronizationContextParameterExpression).Compile();

      return eventSourceHandler;
    }

    /// <inheritdoc />
    public void StopListening<TEventSource, TDelegate>(string eventName, TDelegate eventHandler) where TDelegate : Delegate
    {
      ArgumentExceptionEx.ThrowIfNullOrWhiteSpace(eventName, nameof(eventName));
      ArgumentNullExceptionEx.ThrowIfNull(eventHandler, nameof(eventHandler));

      this.registrationService.UnregisterHandler<TEventSource>(eventName, eventHandler);
    }

    /// <inheritdoc />
    public void StopListeningAll<TEventSource>(string eventName)
    {
      ArgumentExceptionEx.ThrowIfNullOrWhiteSpace(eventName, nameof(eventName));

      this.registrationService.UnregisterAllHandlersFromEvent<TEventSource>(eventName);
    }

    /// <inheritdoc />
    public void StopListeningAll<TEventSource>() 
      => this.registrationService.UnregisterAllHandlers<TEventSource>();

    private void ThrowIfEventHandlerInvalid<TEventSource>(EventInfoTableEntry entry, Delegate eventHandler)
    {
      //ParameterInfo[] invocatorParameters = entry.InvocatorMethod.GetParameters();
      //ParameterInfo[] eventHandlerParameters = eventHandler.GetType().GetMethod("Invoke")?.GetParameters();
      //if (eventHandlerParameters != null)
      //{
      //  if (invocatorParameters.Length != eventHandlerParameters.Length)
      //  {
      //    throw new EventHandlerMismatchException($"Wrong event handler signature. The parameter count of the registered event handler does not match the event delegate {entry.EventInfo.EventHandlerType.FullName}.");
      //  }

      //  for (int index = 0; index < invocatorParameters.Length; index++)
      //  {
      //    Type invocatorParameterType = invocatorParameters[index].ParameterType;
      //    Type eventHandlerParameterType = eventHandlerParameters[index].ParameterType;
      //    if (!eventHandlerParameterType.IsAssignableFrom(invocatorParameterType))
      //    {
      //      throw new EventHandlerMismatchException($"Wrong event handler signature. The parameter eventHandlerTypeDefinition at index {index} of the registered event handler does not match the event delegate {entry.EventInfo.EventHandlerType.FullName}. Found eventHandlerTypeDefinition {eventHandlerParameterType.FullName}. Expected eventHandlerTypeDefinition {invocatorParameterType.FullName}.");
      //    }
      //  }
      //}
    }

    #endregion Implementation of IWeakEventAggregator

    private readonly WeakEventRegistrationService registrationService;
    private static readonly Dictionary<RegistrarCacheKey, Func<Delegate, string, SynchronizationContext, IClientEventHandlerRegistrar>> RegistrarCache = new Dictionary<RegistrarCacheKey, Func<Delegate, string, SynchronizationContext, IClientEventHandlerRegistrar>>();
  }

  internal readonly struct RegistrarCacheKey : IEquatable<RegistrarCacheKey>
  {
    public RegistrarCacheKey(Type senderType, Type eventArgsType)
    {
      this.SenderType = senderType;
      this.EventArgsType = eventArgsType;
    }

    public Type SenderType { get; }
    public Type EventArgsType { get; }

    public bool Equals(RegistrarCacheKey other) => this.SenderType.Equals(other.SenderType) && this.EventArgsType.Equals(other.EventArgsType);
    public override bool Equals(object obj) => obj is RegistrarCacheKey registrarCacheKey && Equals(registrarCacheKey);

    public override int GetHashCode()
    {
      int hashCode = -1091271162;
      hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(this.SenderType);
      hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(this.EventArgsType);
      return hashCode;
    }

    public static bool operator==(RegistrarCacheKey left, RegistrarCacheKey right) => left.Equals(right);
    public static bool operator!=(RegistrarCacheKey left, RegistrarCacheKey right) => !left.Equals(right);
  }
}