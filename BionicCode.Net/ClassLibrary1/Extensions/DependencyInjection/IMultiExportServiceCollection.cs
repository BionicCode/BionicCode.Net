namespace Microsoft.Extensions.DependencyInjection
{
  using System.Collections;

  public interface IMultiExportServiceCollection : IServiceCollection
  {
    /// <summary>
    /// Add another interface service to the implementation registered with a call to <see cref="DependencyInjection.AddMultiExportSingleton{TImplementation}(IServiceCollection)"/>.
    /// </summary>
    /// <typeparam name="TService">Interface type or base type.</typeparam>
    /// <returns>An <see cref="IMultiExportServiceCollection"/> which implements <see cref="ICollection"/> and allows to map multiple services with an implementation.</returns>
    IMultiExportServiceCollection AsService<TService>() where TService : class;

    /// <summary>
    /// Map all implemented interfaces and the potential base class to the implementation 
    /// previously registered with a call to <see cref="DependencyInjection.AddMultiExportSingleton{TImplementation}(IServiceCollection)"/>.
    /// </summary>
    /// <returns>An <see cref="IServiceCollection"/> to enable method chaining.</returns>
    /// <remarks>Ensure that all required dependencies are registered before calling this method.</remarks>
    IServiceCollection AsImplementedServices();
  }
}