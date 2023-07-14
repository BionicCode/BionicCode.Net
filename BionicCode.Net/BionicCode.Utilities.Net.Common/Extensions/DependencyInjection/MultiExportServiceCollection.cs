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

      ServiceCollection = serviceCollection;
      TImplementations = tImplementations;
    }

    public MultiExportServiceCollection(IServiceCollection serviceCollection, Type tImplementation) : this(serviceCollection, new List<Type> { tImplementation })
    {
    }

    /// <inheritdoc/>
    public IMultiExportServiceCollection AsService<TService>() where TService : class
    {
      foreach (Type tImplementation in TImplementations)
      {
        _ = ServiceCollection.AddSingleton(typeof(TService), serviceProvider => serviceProvider.GetService(tImplementation));
      }
      return this;
    }

    /// <inheritdoc/>
    public IServiceCollection AsImplementedServices()
    {
      foreach (Type tImplementation in TImplementations)
      {
        Type[] interfaces = tImplementation.GetInterfaces();
        foreach (Type interfaceType in interfaces)
        {
          _ = ServiceCollection.AddSingleton(interfaceType, serviceProvider => serviceProvider.GetRequiredService(tImplementation));
        }

        if (tImplementation.BaseType != typeof(object))
        {
          _ = ServiceCollection.AddSingleton(tImplementation.BaseType, serviceProvider => serviceProvider.GetRequiredService(tImplementation));
        }
      }

      return ServiceCollection;
    }

    public int IndexOf(ServiceDescriptor item) => ServiceCollection.IndexOf(item);

    public void Insert(int index, ServiceDescriptor item) => ServiceCollection.Insert(index, item);

    public void RemoveAt(int index) => ServiceCollection.RemoveAt(index);

    public void Add(ServiceDescriptor item) => ServiceCollection.Add(item);

    public void Clear() => ServiceCollection.Clear();

    public bool Contains(ServiceDescriptor item) => ServiceCollection.Contains(item);

    public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => ServiceCollection.CopyTo(array, arrayIndex);

    public bool Remove(ServiceDescriptor item) => ServiceCollection.Remove(item);

    public IEnumerator<ServiceDescriptor> GetEnumerator() => ServiceCollection.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)ServiceCollection).GetEnumerator();

    public int Count => ServiceCollection.Count;
    public bool IsReadOnly => ServiceCollection.IsReadOnly;
    public ServiceDescriptor this[int index] { get => ((IList<ServiceDescriptor>)ServiceCollection)[index]; set => ((IList<ServiceDescriptor>)ServiceCollection)[index] = value; }
    private IServiceCollection ServiceCollection { get; }
    private IEnumerable<Type> TImplementations { get; }
  }
}
