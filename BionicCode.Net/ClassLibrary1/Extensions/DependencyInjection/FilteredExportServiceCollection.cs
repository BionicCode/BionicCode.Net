namespace Microsoft.Extensions.DependencyInjection
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;

  public class FilteredExportServiceCollection : IFilteredExportServiceCollection
  {
    public FilteredExportServiceCollection(IServiceCollection services, IEnumerable<Type> source, ServiceLifetime serviceLifetime)
    {
      this.Services = services;
      this.Source = source;
      this.ServiceLifetime = serviceLifetime;
    }

    public IFilteredExportServiceCollection WhereClassName(Predicate<string> filter)
    {
      this.Source = this.Source.Where(type => filter.Invoke(type.Name));
      return this;
    }

    public IFilteredExportServiceCollection WhereClassType(Predicate<Type> typeFilter)
    {
      this.Source = this.Source.Where(type => typeFilter.Invoke(type));
      return this;
    }

    public IFilteredExportServiceCollection WhereConstructor(Predicate<ConstructorInfo> constructorFilter)
    {
      this.Source = this.Source.Where(type => type.GetConstructors(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Any(constructorInfo => constructorFilter.Invoke(constructorInfo)));
      return this;
    }

    public IFilteredExportServiceCollection WhereClassAttribute(Predicate<Attribute> attributeFilter)
    {
      this.Source = this.Source.Where(type => type.GetCustomAttributes().Any(attribute => attributeFilter(attribute)));
      return this;
    }

    public IMultiExportServiceCollection Register()
    {
      foreach (Type tImplementation in this.Source)
      {
        switch (this.ServiceLifetime)
        {
          case ServiceLifetime.Transient:
            _ = this.Services.AddTransient(tImplementation);
            break;
          case ServiceLifetime.Scoped:
            _ = this.Services.AddScoped(tImplementation);
            break;
          case ServiceLifetime.Singleton:
            _ = this.Services.AddSingleton(tImplementation);
            break;
        }
      }

      return new MultiExportServiceCollection(this.Services, this.Source);
    }

    private IServiceCollection Services { get; }
    private IEnumerable<Type> Source { get; set; }
    private ServiceLifetime ServiceLifetime { get; }
  }
}