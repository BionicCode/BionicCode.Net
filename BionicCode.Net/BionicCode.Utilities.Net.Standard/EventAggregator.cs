#region Info
// //  
// BionicUtilities.Net.Standard
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BionicCode.Utilities.Net.Standard.Exception;

namespace BionicCode.Utilities.Net.Standard
{
  public class EventAggregator : IEventAggregator
  {
    public EventAggregator()
    {
      this.EventHandlerTable = new ConcurrentDictionary<string, List<Delegate>>();
      this.EventPublisherTable = new ConditionalWeakTable<object, List<(EventInfo EventInfo, Delegate Handler)>>();
    }

    #region Implementation of IEventAggregator


    /// <inheritdoc />
    public bool TryRegisterObservable(object eventSource, IEnumerable<string> eventNames)
    {
      if (object.Equals(eventSource, default))
      {
        return false;
      }
      foreach (string eventName in eventNames.Distinct())
      {
        EventInfo eventInfo = eventSource.GetType()
          .GetEvent(
            eventName,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
        if (eventInfo == null)
        {
          throw new ArgumentException($"The event {eventName} was not found on the event source {eventSource.GetType().Name} or on its declaring base type.");
        }

        Type normalizedEventHandlerType = NormalizeEventHandlerType(eventInfo.EventHandlerType);
        ICollection<string> eventIds = CreateEventIdsOfConcreteType(eventSource, normalizedEventHandlerType, eventName);

        if (TryCreateEventIdOfInterfaceType(eventSource, eventName, out string interfaceEventId))
        {
          eventIds.Add(interfaceEventId);
        }

        (Type EventHandlerType, ICollection<string> EventIds) eventIdArg = (normalizedEventHandlerType, eventIds);

        Action<object, object> clientHandlerInvocator = (sender, args) => DelegateHandleEvent(eventIdArg, sender, args);
        var eventSourceHandler = Delegate.CreateDelegate(
          eventInfo.EventHandlerType,
          clientHandlerInvocator.Target,
          clientHandlerInvocator.Method);

        List<(EventInfo EventInfo, Delegate Handler)> publishers = this.EventPublisherTable.GetOrCreateValue(eventSource);
        publishers.Add((eventInfo, eventSourceHandler));
        
        eventInfo.AddEventHandler(eventSource, eventSourceHandler);
      }

      return true;
    }

    /// <inheritdoc />
    public bool TryRemoveObservable(object eventSource, IEnumerable<string> eventNames, bool removeEventObservers = false)
    {
      bool hasRemovedObservable = false;
      if (!this.EventPublisherTable.TryGetValue(eventSource, out List<(EventInfo EventInfo, Delegate Handler)> publisherHandlerInfos))
      {
        return false;
      }
      foreach (string eventName in eventNames)
      {
        (EventInfo EventInfo, Delegate Handler) publisherHandlerInfo = publisherHandlerInfos.FirstOrDefault(
          handlerInfo => handlerInfo.EventInfo.Name.Equals(eventName, StringComparison.Ordinal));

        publisherHandlerInfo.EventInfo?.RemoveEventHandler(eventSource, publisherHandlerInfo.Handler);
        hasRemovedObservable = publisherHandlerInfos.Remove(publisherHandlerInfo); 

        if (removeEventObservers)
        {
          TryRemoveAllObservers(eventName, eventSource.GetType());
        }
      }

      if (!publisherHandlerInfos.Any())
      {
        this.EventPublisherTable.Remove(eventSource);
      }

      return hasRemovedObservable;
    }

    /// <inheritdoc />
    public bool TryRemoveObservable(object eventSource, bool removeObserversOfEvents = false)
    {
      bool hasRemovedObservable = false;

      if (this.EventPublisherTable.TryGetValue(eventSource, out List<(EventInfo EventInfo, Delegate Handler)> handlerInfo))
      {
        this.EventPublisherTable.Remove(eventSource);

        handlerInfo.ForEach(publisherHandlerInfo => publisherHandlerInfo.EventInfo.RemoveEventHandler(eventSource, publisherHandlerInfo.Handler));
        hasRemovedObservable = true;
      }

      if (removeObserversOfEvents)
      {
        TryRemoveAllObservers(eventSource.GetType());
      }

      return hasRemovedObservable;
    }

    /// <inheritdoc />
    public bool TryRegisterObserver(string eventName, Type eventSourceType, Delegate eventHandler)
    {
      var fullyQualifiedEventName = CreateFullyQualifiedEventIdOfSpecificSource(eventSourceType, eventName);
      return TryRegisterObserverInternal(eventHandler, fullyQualifiedEventName);
    }

    /// <inheritdoc />
    public bool TryRegisterObserver<TEventSource>(string eventName, Type eventSourceType, Action<object, TEventSource> eventHandler) => TryRegisterObserver(eventName, eventSourceType, (Delegate) eventHandler);

    /// <inheritdoc />
    public bool TryRegisterGlobalObserver(string eventName, Delegate eventHandler)
    {
      Type normalizedEventHandlerType = NormalizeEventHandlerType(eventHandler.GetType());
      var fullyQualifiedEventName = CreateFullyQualifiedEventIdOfGlobalSource(normalizedEventHandlerType, eventName);
      return TryRegisterObserverInternal(eventHandler, fullyQualifiedEventName);
    }

    /// <inheritdoc />
    public bool TryRegisterGlobalObserver<TEventArgs>(string eventName, Action<object, TEventArgs> eventHandler)
    {
      Type normalizedEventHandlerType = NormalizeEventHandlerType<TEventArgs>(eventHandler.GetType());
      var fullyQualifiedEventName = CreateFullyQualifiedEventIdOfGlobalSource(normalizedEventHandlerType, eventName);
      return TryRegisterObserverInternal(eventHandler, fullyQualifiedEventName);
    }

    /// <inheritdoc />
    public bool TryRegisterGlobalObserver(Delegate eventHandler)
    {
      Type normalizedEventHandlerType = NormalizeEventHandlerType(eventHandler.GetType());
      var fullyQualifiedEventName = CreateFullyQualifiedEventIdOfGlobalSource(normalizedEventHandlerType, string.Empty);
      return TryRegisterObserverInternal(eventHandler, fullyQualifiedEventName);
    }

    /// <inheritdoc />
    public bool TryRegisterGlobalObserver<TEventArgs>(Action<object, TEventArgs> eventHandler)
    {
      Type normalizedEventHandlerType = NormalizeEventHandlerType<TEventArgs>(eventHandler.GetType());
      var fullyQualifiedEventName = CreateFullyQualifiedEventIdOfGlobalSource(normalizedEventHandlerType, string.Empty);
      return TryRegisterObserverInternal(eventHandler, fullyQualifiedEventName);
    }

    /// <inheritdoc />
    public bool TryRemoveObserver(string eventName, Type eventSourceType, Delegate eventHandler)
    {
      string fullyQualifiedEventIdOfSpecificSource =
        CreateFullyQualifiedEventIdOfSpecificSource(eventSourceType, eventName);
      return this.EventHandlerTable.TryRemove(fullyQualifiedEventIdOfSpecificSource, out List<Delegate> _);
    }

    /// <inheritdoc />
    public bool TryRemoveObserver<TEventSource>(
      string eventName,
      Type eventSourceType,
      Action<object, TEventSource> eventHandler) =>
      TryRemoveObserver(eventName, eventSourceType, (Delegate) eventHandler);

    /// <inheritdoc />
    public bool TryRemoveGlobalObserver(string eventName, Delegate eventHandler)
    {
      Type normalizedEventHandlerType = NormalizeEventHandlerType(eventHandler.GetType());

      string fullyQualifiedEventIdOfGlobalSource =
        CreateFullyQualifiedEventIdOfGlobalSource(normalizedEventHandlerType, eventName);
      return this.EventHandlerTable.TryRemove(fullyQualifiedEventIdOfGlobalSource, out List<Delegate> _);
    }


    /// <inheritdoc />
    public bool TryRemoveGlobalObserver<TEventArgs>(string eventName, Action<object, TEventArgs> eventHandler)
    {
      Type normalizedEventHandlerType = NormalizeEventHandlerType< TEventArgs>(eventHandler.GetType());

      string fullyQualifiedEventIdOfGlobalSource =
        CreateFullyQualifiedEventIdOfGlobalSource(normalizedEventHandlerType, eventName);
      return this.EventHandlerTable.TryRemove(fullyQualifiedEventIdOfGlobalSource, out List<Delegate> _);
    }

    /// <inheritdoc />
    public bool TryRemoveGlobalObserver(Delegate eventHandler)
    {
      bool result = false;
      Type normalizedEventHandlerType = NormalizeEventHandlerType(eventHandler.GetType());

      return TryRemoveGlobalObserverInternal(normalizedEventHandlerType);
    }

    /// <inheritdoc />
    public bool TryRemoveGlobalObserver<TEventArgs>(Action<object, TEventArgs> eventHandler)
    {
      Type normalizedEventHandlerType = NormalizeEventHandlerType< TEventArgs>(eventHandler.GetType());
      return TryRemoveGlobalObserverInternal(normalizedEventHandlerType);
    }

    /// <inheritdoc />
    public bool TryRemoveAllObservers(string eventName, Type eventSourceType)
    {
      string fullyQualifiedEventIdOfSpecificSource =
        CreateFullyQualifiedEventIdOfSpecificSource(eventSourceType, eventName);
      return this.EventHandlerTable.TryRemove(fullyQualifiedEventIdOfSpecificSource,
        out List<Delegate> _);
    }

    /// <inheritdoc />
    public bool TryRemoveAllObservers(Type eventSourceType)
    {
      bool result = false;
      string fullyQualifiedEventIdOfSpecificSourcePrefix =
        CreateFullyQualifiedEventIdOfSpecificSource(eventSourceType, string.Empty);
      for (var index = this.EventHandlerTable.Count - 1; index >= 0; index--)
      {
        KeyValuePair<string, List<Delegate>> handlersEntry = this.EventHandlerTable.ElementAt(index);
        if (handlersEntry.Key.StartsWith(fullyQualifiedEventIdOfSpecificSourcePrefix, StringComparison.Ordinal))
        {
          result |= this.EventHandlerTable.TryRemove(handlersEntry.Key, out _);
        }
      }

      return result;
    }

    /// <inheritdoc />
    public bool TryRemoveAllObservers(string eventName)
    {
      bool result = false;
      string fullyQualifiedEventIdSuffix = $".{eventName}";
      for (var index = this.EventHandlerTable.Count - 1; index >= 0; index--)
      {
        KeyValuePair<string, List<Delegate>> handlersEntry = this.EventHandlerTable.ElementAt(index);
        if (handlersEntry.Key.EndsWith(fullyQualifiedEventIdSuffix, StringComparison.Ordinal))
        {
          result |= this.EventHandlerTable.TryRemove(handlersEntry.Key, out List<Delegate> _);
        }
      }

      return result;
    }

    #endregion Implementation of IEventAggregator

    private bool TryRegisterObserverInternal(
      Delegate eventHandler,
      string fullyQualifiedEventName)
    {
      if (this.EventHandlerTable.TryGetValue(fullyQualifiedEventName, out List<Delegate> handlers))
      {
        handlers.Add(eventHandler);
        return true;
      }

      return this.EventHandlerTable.TryAdd(fullyQualifiedEventName, new List<Delegate>() { eventHandler });
    }

    private bool TryRemoveGlobalObserverInternal(Type normalizedEventHandlerType)
    {
      bool result = false;
      string fullyQualifiedEventIdOfGlobalSourcePrefix =
        CreateFullyQualifiedEventIdOfGlobalSource(normalizedEventHandlerType, string.Empty);
      for (var index = this.EventHandlerTable.Count - 1; index >= 0; index--)
      {
        KeyValuePair<string, List<Delegate>> handlersEntry = this.EventHandlerTable.ElementAt(index);
        if (handlersEntry.Key.StartsWith(fullyQualifiedEventIdOfGlobalSourcePrefix, StringComparison.Ordinal))
        {
          result |= this.EventHandlerTable.TryRemove(handlersEntry.Key, out List<Delegate> _);
        }
      }

      return result;
    }

    private void DelegateHandleEvent((Type EventHandlerType, ICollection<string> EventIds) eventInfo, object sender, object args)
    {
      IEnumerable<Delegate> handlers = eventInfo.EventIds
        .SelectMany(
          eventId => this.EventHandlerTable.TryGetValue(eventId, out List<Delegate> delegates)
            ? delegates
            : new List<Delegate>());

      foreach (Delegate handler in handlers)
      {
        try
        {
          handler.DynamicInvoke(sender, args);
        }
        catch (ArgumentException e)
        {
          MethodInfo delegateInvokeMethodInfo = handler.GetType().GetMethod("Invoke");
          string handlerSignatureParameterList = delegateInvokeMethodInfo?
            .GetParameters()
            .Select(parameterInfo => parameterInfo.ParameterType.FullName)
            .Aggregate((result, current) => result += ", " + current).TrimEnd(',', ' ');

          throw new WrongEventHandlerSignatureException(
            $"The found callback signature does not match the registered delegate{Environment.NewLine}'{FormatTypeName(handler.GetType())}'. {Environment.NewLine}{Environment.NewLine}Expected: '{delegateInvokeMethodInfo.ReturnType.Name} {handler.Method.Name}({handlerSignatureParameterList})'.{Environment.NewLine}Actual: '{handler.Method}'.", e);
        }
      }
    }

    private void ThrowIfHandlerSignatureIncompatible(object args, Delegate handler)
    {
      MethodInfo delegateInvokeMethodInfo = handler.GetType().GetMethod("Invoke");
      Type handlerEventArgsType = delegateInvokeMethodInfo?
        .GetParameters()
        .ElementAt(1).ParameterType;
      if (!handlerEventArgsType?.IsInstanceOfType(args) ?? true)
      {
        string handlerSignature = delegateInvokeMethodInfo?
          .GetParameters()
          .Select(parameterInfo => parameterInfo.ParameterType.FullName)
          .Aggregate((result, current) => result += ", " + current).TrimEnd(',', ' ');
        throw new WrongEventHandlerSignatureException(
          $"The found callback signature does not match the registered delegate{Environment.NewLine}'{FormatTypeName(handler.GetType())}'. {Environment.NewLine}Expected: '{delegateInvokeMethodInfo.ReturnType.Name} {handler.Method.Name}({handlerSignature})'. Actual: '{handler.Method}'.");
      }
    }
    private string FormatTypeName(Type typeToFormat, bool isFullyQualified = true)
    {
      if (!typeToFormat.IsGenericType)
      {
        return isFullyQualified ? typeToFormat.FullName : typeToFormat.Name;
      }
      Type[] genericArguments = typeToFormat.GetGenericArguments();
      string originalTypeName = isFullyQualified ? typeToFormat.FullName : typeToFormat.Name;
      return originalTypeName.Substring(0, originalTypeName.IndexOf("`", StringComparison.OrdinalIgnoreCase)) + "<" + String.Join(", ", genericArguments.Select(type => FormatTypeName(type))) + ">";
    }

    private bool TryCreateEventIdOfInterfaceType(object eventSource, string eventName, out string eventId)
    {
      eventId = string.Empty;

      Type eventSourceInterfaceType = eventSource.GetType().GetInterfaces().FirstOrDefault(
        interfaceType => interfaceType.GetEvent(
          eventName,
          BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance |
          BindingFlags.Static) != null);

      if (eventSourceInterfaceType != null)
      {
        var fullyQualifiedInterfaceEventIdOfSpecificEvent =
          CreateFullyQualifiedEventIdOfSpecificSource(eventSourceInterfaceType, eventName);
        eventId = fullyQualifiedInterfaceEventIdOfSpecificEvent;
      }

      return !string.IsNullOrWhiteSpace(eventId);
    }

    private List<string> CreateEventIdsOfConcreteType(
      object eventSource,
      Type normalizedEventHandlerType,
      string eventName)
    {
      var fullyQualifiedEventIdOfGlobalEvent =
        CreateFullyQualifiedEventIdOfGlobalSource(normalizedEventHandlerType, eventName);
      var fullyQualifiedEventIdOfUnknownGlobalEvent =
        CreateFullyQualifiedEventIdOfGlobalSource(normalizedEventHandlerType, string.Empty);
      var fullyQualifiedImplementationEventIdOfSpecificEvent =
        CreateFullyQualifiedEventIdOfSpecificSource(eventSource.GetType(), eventName);

      var eventIds = new List<string>
      {
        fullyQualifiedImplementationEventIdOfSpecificEvent, fullyQualifiedEventIdOfGlobalEvent,
        fullyQualifiedEventIdOfUnknownGlobalEvent
      };
      return eventIds;
    }

    private Type NormalizeEventHandlerType(Type eventHandlerType) =>
      eventHandlerType == typeof(EventHandler) || eventHandlerType == typeof(Action<object, EventArgs>)
        ? typeof(EventHandler<EventArgs>)
        : eventHandlerType;

    private Type NormalizeEventHandlerType<TEventArgs>(Type eventHandlerType) =>
      eventHandlerType == typeof(EventHandler) || eventHandlerType == typeof(Action<object, EventArgs>)
        ? typeof(EventHandler<EventArgs>)
        : eventHandlerType == typeof(Action<object, TEventArgs>)
          ? typeof(EventHandler<TEventArgs>)
          : eventHandlerType;

    private string CreateFullyQualifiedEventIdOfGlobalSource(Type eventHandlerType, string eventName) => eventHandlerType.FullName.ToLowerInvariant() + "." + eventName;

    private string CreateFullyQualifiedEventIdOfSpecificSource(Type eventSource, string eventName) => eventSource.AssemblyQualifiedName.ToLowerInvariant() + "." + eventSource.FullName.ToLowerInvariant() + "." + eventName;

    private ConcurrentDictionary<string, List<Delegate>> EventHandlerTable { get; set; }
    private ConditionalWeakTable<object, List<(EventInfo EventInfo, Delegate Handler)>> EventPublisherTable { get; }
  }
}