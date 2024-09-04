namespace Microsoft.Extensions.DependencyInjection
{
  using System;
  using System.Reflection;

  public interface IFilteredExportServiceCollection
  {
    IFilteredExportServiceCollection WhereClassName(Func<string, bool> nameFilter);
    IFilteredExportServiceCollection WhereClassType(Func<Type, bool> typeFilter);
    IFilteredExportServiceCollection WhereConstructor(Func<ConstructorInfo, bool> constructorFilter);
    IFilteredExportServiceCollection WhereClassAttribute(Func<Attribute, bool> attributeFilter);
    IMultiExportServiceCollection Register();
  }
}