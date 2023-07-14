namespace Microsoft.Extensions.DependencyInjection
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;
  using System.Threading.Tasks;
  /* Unmerged change from project 'BionicCode.Utilities.Net.Common (netstandard2.0)'
  Before:
    using System.Runtime.CompilerServices;
  After:
    using System.Runtime.CompilerServices;
    using BionicCode.Utilities.Net;
  */
  using BionicCode.Utilities.Net;
  using Microsoft.Extensions.DependencyInjection.Extensions;

  static class DependencyInjection
  {
    private static IList<Type> InitializableImplementations { get; } = new List<Type>();

    /// <summary>
    /// Register <see cref="Func{TResult}"/> factory implementations
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <param name="serviceCollection"></param>
    /// <param name="productLifetime"></param>
    /// <returns><see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddFactory<TService>(this IServiceCollection serviceCollection, ServiceLifetime productLifetime)
      where TService : class
    {
      TryRegisterImplementation<TService>(serviceCollection, productLifetime);
      return serviceCollection.AddSingleton<Func<TService>>(serviceProvider => serviceProvider.GetRequiredService<TService>);
    }

    /// <summary>
    /// Register <see cref="Func{TResult}"/> factory implementations
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <param name="serviceCollection"></param>
    /// <returns><see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddFactory<TService, TImplementation>(this IServiceCollection serviceCollection, ServiceLifetime productLifetime)
      where TService : class
      where TImplementation : class, TService
    {
      TryRegisterImplementation<TImplementation>(serviceCollection, productLifetime);
      return serviceCollection.AddSingleton<Func<TService>>(serviceProvider => serviceProvider.GetRequiredService<TImplementation>);
    }

    private static void TryRegisterImplementation<TImplementation>(IServiceCollection serviceCollection, ServiceLifetime serviceLifetime)
      where TImplementation : class
    {
      switch (serviceLifetime)
      {
        case ServiceLifetime.Singleton:
          serviceCollection.TryAddSingleton<TImplementation>();
          break;
        case ServiceLifetime.Scoped:
          serviceCollection.TryAddScoped<TImplementation>();
          break;
        case ServiceLifetime.Transient:
          serviceCollection.TryAddTransient<TImplementation>();
          break;
      }
    }

    /// <summary>
    /// Register a service implementation with multiple interfaces. 
    /// Use <see cref="IMultiExportServiceCollection.AsService{TService}"/> to attach more service interfaces to the service implementation <typeparamref name="TImplementation"/>
    /// or use <see cref="IMultiExportServiceCollection.AsImplementedServices"/> to register all implemented interfaces of the implementation.
    /// </summary>
    /// <typeparam name="TImplementation"></typeparam>
    /// <param name="serviceCollection"></param>
    /// <returns>An <see cref="IMultiExportServiceCollection"/> which implements <see cref="ICollection"/> and aggreagates multiple service interfaces mapped to a single implementation.</returns>
    public static IMultiExportServiceCollection AddMultiExportSingleton<TImplementation>(this IServiceCollection serviceCollection)
      where TImplementation : class
      => new MultiExportServiceCollection(serviceCollection, typeof(TImplementation));

    /// <summary>
    /// Register an <see cref="IInitializable"/> implementation.
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    /// <param name="serviceCollection"></param>
    /// <returns><see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddSingletonAsInitializableService<TService, TImplementation>(this IServiceCollection serviceCollection)
      where TService : class
      where TImplementation : class, TService, IInitializable
    {
      DependencyInjection.InitializableImplementations.Add(typeof(TImplementation));
      return serviceCollection.AddSingleton<TService, TImplementation>();
    }

    /// <summary>
    /// Register <see cref="IInitializable"/> implementations
    /// </summary>
    /// <typeparam name="TImplementation"></typeparam>
    /// <param name="serviceCollection"></param>
    /// <returns><see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddSingletonAsInitializableService<TImplementation>(this IServiceCollection serviceCollection)
      where TImplementation : class, IInitializable
    {
      DependencyInjection.InitializableImplementations.Add(typeof(TImplementation));
      return serviceCollection.AddSingleton<TImplementation>();
    }

    public static IAssemblyFilter AddFromAssembly(this IServiceCollection services, Assembly assembly)
      => new AssemblyFilter(services, assembly);

    public static IAssemblyFilter AddFromAssemblies(this IServiceCollection services, IEnumerable<Assembly> assemblies)
      => new AssemblyFilter(services, assemblies);

    /// <summary>
    /// Initializes asll registered <see cref="IInitializable"/> implementations.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns><see cref="IServiceProvider"/></returns>
    /// <remarks>
    /// Use <see cref="InitializeServicesAsync(IServiceProvider)"/> to throw an <see cref="InvalidOperationException"/> when the initialization of an <see cref="IInitializable"/> implementations has failed. 
    /// <br/>
    /// This is when <see cref="IInitializable.InitializeAsync"/> or <see cref="IInitializable.IsInitialized"/> returns <c>false</c>.
    /// </remarks>
    public static async Task<IServiceProvider> TryInitializeServicesAsync(this IServiceProvider serviceProvider)
      => await DependencyInjection.InitializeServicesAsync(serviceProvider, false);

    /// <summary>
    /// Initializes all registered <see cref="IInitializable"/> implementations.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns><see cref="IServiceProvider"/></returns>
    /// <exception cref="InvalidOperationException">Thrown when the initialization of an <see cref="IInitializable"/> implementations has failed. This is when <see cref="IInitializable.InitializeAsync"/> or <see cref="IInitializable.IsInitialized"/> returns <c>false</c>.</exception>
    /// <remarks>
    /// Throws and <see cref="InvalidOperationException"/> when the initialization of an <see cref="IInitializable"/> implementations has failed. <br/>
    /// This is when <see cref="IInitializable.InitializeAsync"/> or <see cref="IInitializable.IsInitialized"/> returns <c>false</c>.
    /// <para>Use <see cref="TryInitializeServicesAsync(IServiceProvider)"/> to avoid throwing such an exception and proceed with the initialization.</para>
    /// </remarks>
    public static async Task<IServiceProvider> InitializeServicesAsync(this IServiceProvider serviceProvider)
      => await DependencyInjection.InitializeServicesAsync(serviceProvider, true);

    private static async Task<IServiceProvider> InitializeServicesAsync(IServiceProvider serviceProvider, bool isThrowExceptionEnabled)
    {
      foreach (object implementation in InitializableImplementations
        .Select(serviceProvider.GetRequiredService))
      {
        var initializable = (IInitializable)implementation;
        bool isInitialized = await initializable.InitializeAsync();
        if (isThrowExceptionEnabled && !isInitialized)
        {
          throw new InvalidOperationException(ExceptionMessages.GetInvalidOperationExceptionMessage_IInitializableFailed(implementation.GetType()));
        }
      }

      return serviceProvider;
    }
  }
}
