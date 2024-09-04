namespace Microsoft.Extensions.DependencyInjection
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;
  using Microsoft.Extensions.DependencyInjection;

  public class AssemblyFilter : IAssemblyFilter
  {
    public AssemblyFilter(IServiceCollection services, Assembly assembly) : this(services, new List<Assembly> { assembly })
    {
    }

    public AssemblyFilter(IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
      this.Services = services;
      this.Assemblies = assemblies;
    }

    public IFilteredExportServiceCollection AsSingleton()
    {
      IEnumerable<Type> tImplementations = GetConcreteTypesFromAssemblies();

      return new FilteredExportServiceCollection(this.Services, tImplementations, ServiceLifetime.Singleton);
    }

    public IFilteredExportServiceCollection AsTransient()
    {
      IEnumerable<Type> tImplementations = GetConcreteTypesFromAssemblies();
      return new FilteredExportServiceCollection(this.Services, tImplementations, ServiceLifetime.Transient);
    }

    public IFilteredExportServiceCollection AsScoped()
    {
      IEnumerable<Type> tImplementations = GetConcreteTypesFromAssemblies();
      return new FilteredExportServiceCollection(this.Services, tImplementations, ServiceLifetime.Scoped);
    }

    private IEnumerable<Type> GetConcreteTypesFromAssemblies()
      => this.Assemblies
              .SelectMany(assembly => assembly.GetTypes())
              .Where(type => !type.IsInterface && !type.IsAbstract);

    private IServiceCollection Services { get; }
    public IEnumerable<Assembly> Assemblies { get; }
  }
}