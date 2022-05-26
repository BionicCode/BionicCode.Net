namespace BionicCode.Controls.Net.Wpf.BionicCharts
{
  using System.Windows;

  public class ResourceLocator
  {
    public ResourceLocator(FrameworkElement resourcesHost)
    {
      this.ResourcesHost = resourcesHost;
    }

    public TResource FindResource<TResource>(object resourceKey) => (TResource)this.ResourcesHost.FindResource(resourceKey);
    public TResource TryFindResources<TResource>(object resourceKey) => this.ResourcesHost.TryFindResource(resourceKey) is object resource 
      ? (TResource) resource 
      : default;

    public FrameworkElement ResourcesHost { get; }
  }
}