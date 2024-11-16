namespace BionicCode.Utilities.Net
{
  #region Info
  // //  
  // BionicUtilities.Net.Standard
  #endregion

  using System;
  using System.Collections.Generic;

  internal class WeakEventRegistrationService<TEventSource>
  {
    private readonly WeakCollection<object> eventSourceInstances;
    private readonly List<IClientEventHandlerRegistrar<TEventSource>> handlerRegistrars;
    private readonly object syncLock;

    public WeakEventRegistrationService()
    {
      this.eventSourceInstances = new WeakCollection<object>();
      this.handlerRegistrars = new List<IClientEventHandlerRegistrar<TEventSource>>();
      this.syncLock = new object();
    }

    public void AddSourceInstance(object instance)
    {
      lock (this.syncLock)
      {
        if (this.eventSourceInstances.Contains(instance))
        {
          throw new ArgumentException("Event source instance was already added");
        }

        this.eventSourceInstances.Add(instance);
        foreach (IClientEventHandlerRegistrar<TEventSource> registrar in this.handlerRegistrars)
        {
          registrar.RegisterDelegate((TEventSource)instance);
        }
      }
    }

    public void RemoveSourceInstance(object instance)
    {
      lock (this.syncLock)
      {
        if (!this.eventSourceInstances.Contains(instance))
        {
          return;
        }

        _ = this.eventSourceInstances.Remove(instance);
        foreach (IClientEventHandlerRegistrar<TEventSource> registrar in this.handlerRegistrars)
        {
          registrar.UnregisterDelegate((TEventSource)instance);
        }
      }
    }

    public void RemoveAllSourceInstances()
    {
      lock (this.syncLock)
      {
        foreach (object instance in this.eventSourceInstances)
        {
          _ = this.eventSourceInstances.Remove(instance);
          foreach (IClientEventHandlerRegistrar<TEventSource> registrar in this.handlerRegistrars)
          {
            registrar.UnregisterDelegate((TEventSource)instance);
          }
        }
      }
    }

    public void RegisterHandler(IClientEventHandlerRegistrar<TEventSource> registrar)
    {
      lock (this.syncLock)
      {
        this.handlerRegistrars.Add(registrar);
        foreach (object instance in this.eventSourceInstances)
        {
          registrar.RegisterDelegate((TEventSource)instance);
        }
      }
    }

      public void UnregisterHandler(Delegate clientHandler)
    {
      lock (this.syncLock)
      {
        foreach (IClientEventHandlerRegistrar<TEventSource> registrar in this.handlerRegistrars)
        {
          if (registrar.ContainsDelegate(clientHandler))
          {
            _ = this.handlerRegistrars.Remove(registrar);
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
        foreach (IClientEventHandlerRegistrar<TEventSource> registrar in this.handlerRegistrars)
        {
          _ = this.handlerRegistrars.Remove(registrar);
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
        foreach (IClientEventHandlerRegistrar<TEventSource> registrar in this.handlerRegistrars)
        {
          _ = this.handlerRegistrars.Remove(registrar);
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