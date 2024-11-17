namespace BionicCode.Utilities.Net
{
  #region Info
  // //  
  // BionicUtilities.Net.Standard
  #endregion

  using System;
  using System.Collections.Generic;
  using System.Collections.Immutable;

  internal class WeakEventRegistrationService<TEventSource> : WeakEventRegistrationService
  {
    private readonly WeakCollection<object> eventSourceInstances;
    private readonly Lazy<ImmutableHashSet<Type>> typeHierarchyFactory;
    public ImmutableHashSet<Type> TypeHierarchy => this.typeHierarchyFactory.Value;
    private readonly object syncLock;

    public bool HasPublishers => this.eventSourceInstances.Count > 0;

    public WeakEventRegistrationService()
    {
      this.eventSourceInstances = new WeakCollection<object>();
      this.syncLock = new object();
    }

    public void AddSourceInstance<TEventSource>(object instance)
    {
      lock (this.syncLock)
      {
        if (this.eventSourceInstances.Contains(instance))
        {
          throw new ArgumentException("Event source instance was already added");
        }

        this.eventSourceInstances.Add(instance);
        foreach (IClientEventHandlerRegistrar registrar in WeakEventRegistrationService.listenerRegistrars)
        {
          registrar.RegisterDelegate((TEventSource)instance);
        }
      }
    }

    public void RemoveSourceInstance<TEventSource>(object instance)
    {
      lock (this.syncLock)
      {
        if (!this.eventSourceInstances.Contains(instance))
        {
          return;
        }

        _ = this.eventSourceInstances.Remove(instance);
        foreach (IClientEventHandlerRegistrar registrar in WeakEventRegistrationService.listenerRegistrars)
        {
          registrar.UnregisterDelegate((TEventSource)instance);
        }
      }
    }

    public void RemoveAllSourceInstances<TEventSource>()
    {
      lock (this.syncLock)
      {
        foreach (object instance in this.eventSourceInstances)
        {
          _ = this.eventSourceInstances.Remove(instance);
          foreach (IClientEventHandlerRegistrar registrar in WeakEventRegistrationService.listenerRegistrars)
          {
            registrar.UnregisterDelegate((TEventSource)instance);
          }
        }
      }
    }

    public void RegisterHandler<TEventSource>(IClientEventHandlerRegistrar registrar)
    {
      lock (this.syncLock)
      {
        WeakEventRegistrationService.listenerRegistrars.Add(registrar);
        foreach (object instance in this.eventSourceInstances)
        {
          registrar.RegisterDelegate((TEventSource)instance);
        }
      }
    }

    public void UnregisterHandler<TEventSource>(Delegate clientHandler)
    {
      lock (this.syncLock)
      {
        foreach (IClientEventHandlerRegistrar registrar in WeakEventRegistrationService.listenerRegistrars)
        {
          if (registrar.ContainsDelegate(clientHandler))
          {
            _ = WeakEventRegistrationService.listenerRegistrars.Remove(registrar);
            foreach (object instance in this.eventSourceInstances)
            {
              registrar.UnregisterDelegate((TEventSource)instance);
            }
          }
        }
      }
    }

    public void UnregisterAllHandlers()
    {
      lock (this.syncLock)
      {
        foreach (IClientEventHandlerRegistrar registrar in WeakEventRegistrationService.listenerRegistrars)
        {
          _ = WeakEventRegistrationService.listenerRegistrars.Remove(registrar);
          foreach (object instance in this.eventSourceInstances)
          {
            registrar.UnregisterDelegate((TEventSource)instance);
          }
        }
      }
    }

    public void ClearAll()
    {
      lock (this.syncLock)
      {
        foreach (IClientEventHandlerRegistrar registrar in WeakEventRegistrationService.listenerRegistrars)
        {
          _ = WeakEventRegistrationService.listenerRegistrars.Remove(registrar);
          foreach (object instance in this.eventSourceInstances)
          {
            _ = this.eventSourceInstances.Remove(instance);
            registrar.UnregisterDelegate((TEventSource)instance);
          }
        }
      }
    }
  }
}