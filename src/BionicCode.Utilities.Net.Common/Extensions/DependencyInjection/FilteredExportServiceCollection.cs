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
            Services = services;
            Source = source;
            ServiceLifetime = serviceLifetime;
        }

        public IFilteredExportServiceCollection WhereClassName(Func<string, bool> filter)
        {
            Source = Source.Where(type => filter.Invoke(type.Name));
            return this;
        }

        public IFilteredExportServiceCollection WhereClassType(Func<Type, bool> typeFilter)
        {
            Source = Source.Where(typeFilter.Invoke);
            return this;
        }

        public IFilteredExportServiceCollection WhereConstructor(Func<ConstructorInfo, bool> constructorFilter)
        {
            Source = Source.Where(type => type.GetConstructors(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Any(constructorInfo => constructorFilter.Invoke(constructorInfo)));
            return this;
        }

        public IFilteredExportServiceCollection WhereClassAttribute(Func<Attribute, bool> attributeFilter)
        {
            Source = Source.Where(type => type.GetCustomAttributes().Any(attribute => attributeFilter(attribute)));
            return this;
        }

        public IMultiExportServiceCollection Register()
        {
            foreach (Type tImplementation in Source)
            {
                switch (ServiceLifetime)
                {
                    case ServiceLifetime.Transient:
                        _ = Services.AddTransient(tImplementation);
                        break;
                    case ServiceLifetime.Scoped:
                        _ = Services.AddScoped(tImplementation);
                        break;
                    case ServiceLifetime.Singleton:
                        _ = Services.AddSingleton(tImplementation);
                        break;
                }
            }

            return new MultiExportServiceCollection(Services, Source);
        }

        private IServiceCollection Services { get; }
        private IEnumerable<Type> Source { get; set; }
        private ServiceLifetime ServiceLifetime { get; }
    }
}
