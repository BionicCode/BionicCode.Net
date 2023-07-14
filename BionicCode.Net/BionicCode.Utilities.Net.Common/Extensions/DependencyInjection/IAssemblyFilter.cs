namespace Microsoft.Extensions.DependencyInjection
{
  public interface IAssemblyFilter
  {
    IFilteredExportServiceCollection AsSingleton(); 
    IFilteredExportServiceCollection AsTransient();
    IFilteredExportServiceCollection AsScoped();
  }
}