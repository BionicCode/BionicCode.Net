namespace Microsoft.Extensions.DependencyInjection
{
  /// <summary>
  /// Register type exporst/services from a particular assembly.
  /// </summary>
  public interface IAssemblyFilter
  {
    IFilteredExportServiceCollection AsSingleton();
    IFilteredExportServiceCollection AsTransient();
    IFilteredExportServiceCollection AsScoped();
  }
}