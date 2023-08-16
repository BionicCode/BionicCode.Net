namespace Microsoft.Extensions.DependencyInjection
{
  using System;
  using System.Collections;
  using System.Collections.Generic;

  public class MultiExportServiceCollection : IServiceCollection, IMultiExportServiceCollection
  {
    public MultiExportServiceCollection(IServiceCollection serviceCollection, IEnumerable<Type> tImplementations)
    {
      if (serviceCollection is null)
      {
        throw new ArgumentNullException(nameof(serviceCollection));
      }
      if (tImplementations is null)
      {
        throw new ArgumentNullException(nameof(tImplementations));
      }

      this.ServiceCollection = serviceCollection;
      this.TImplementations = tImplementations;
    }

    public MultiExportServiceCollection(IServiceCollection serviceCollection, Type tImplementation) : this(serviceCollection, new List<Type> { tImplementation })
    {
    }

    /// <inheritdoc/>
    public IMultiExportServiceCollection AsService<TService>() where TService : class
    {
      foreach (Type tImplementation in this.TImplementations)
      {
        _ = this.ServiceCollection.AddSingleton(typeof(TService), serviceProvider => serviceProvider.GetService(tImplementation));
      }
      return this;
    }

    /// <inheritdoc/>
    public IServiceCollection AsImplementedServices()
    {
      foreach (Type tImplementation in this.TImplementations)
      {
        Type[] interfaces = tImplementation.GetInterfaces();
        foreach (Type interfaceType in interfaces)
        {
          _ = this.ServiceCollection.AddSingleton(interfaceType, serviceProvider => serviceProvider.GetRequiredService(tImplementation));
        }

        if (tImplementation.BaseType != typeof(object))
        {
          _ = this.ServiceCollection.AddSingleton(tImplementation.BaseType, serviceProvider => serviceProvider.GetRequiredService(tImplementation));
        }
      }

      return this.ServiceCollection;
    }

    public int IndexOf(ServiceDescriptor item) => this.ServiceCollection.IndexOf(item);

    public void Insert(int index, ServiceDescriptor item) => this.ServiceCollection.Insert(index, item);

    public void RemoveAt(int index) => this.ServiceCollection.RemoveAt(index);

    public void Add(ServiceDescriptor item) => this.ServiceCollection.Add(item);

    public void Clear() => this.ServiceCollection.Clear();

    public bool Contains(ServiceDescriptor item) => this.ServiceCollection.Contains(item);

    public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => this.ServiceCollection.CopyTo(array, arrayIndex);

    public bool Remove(ServiceDescriptor item) => this.ServiceCollection.Remove(item);

    public IEnumerator<ServiceDescriptor> GetEnumerator() => this.ServiceCollection.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.ServiceCollection).GetEnumerator();

    public int Count => this.ServiceCollection.Count;
    public bool IsReadOnly => this.ServiceCollection.IsReadOnly;
    public ServiceDescriptor this[int index] { get => ((IList<ServiceDescriptor>)this.ServiceCollection)[index]; set => ((IList<ServiceDescriptor>)this.ServiceCollection)[index] = value; }
    private IServiceCollection ServiceCollection { get; }
    private IEnumerable<Type> TImplementations { get; }
  }
}
