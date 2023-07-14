namespace Microsoft.Extensions.DependencyInjection
{
  using System;
  using System.Reflection;

  public interface IFilteredExportServiceCollection
  {
    IFilteredExportServiceCollection WhereClassName(Predicate<string> nameFilter);
    IFilteredExportServiceCollection WhereClassType(Predicate<Type> typeFilter);
    IFilteredExportServiceCollection WhereConstructor(Predicate<ConstructorInfo> constructorFilter);
    IFilteredExportServiceCollection WhereClassAttribute(Predicate<Attribute> attributeFilter);
    IMultiExportServiceCollection Register();
  }
}