namespace BionicCode.Controls.Net.Wpf
{

/* Unmerged change from project 'BionicCode.Controls.Net.Wpf (net6.0-windows)'
Before:
  using System.Windows.Controls;
After:
  using System;
*/
  using System.Windows;
  using System.Windows
/* Unmerged change from project 'BionicCode.Controls.Net.Wpf (net6.0-windows)'
Before:
  using System;
    using BionicCode.Utilities.Net;
After:
  using System.Windows.Controls;
  using System.Windows.Controls.Primitives;
*/
.Controls;
  using System.Windows.Controls.Primitives;
  using BionicCode.Utilities.Net;

  public class ChartPanelPresenter : ContentPresenter
  {
    private ChartPanel panel;

    static ChartPanelPresenter() => ContentPresenter.ContentProperty.OverrideMetadata(typeof(ChartPanelPresenter), new FrameworkPropertyMetadata(default, OnContentChanged));

    public PointInfoGenerator GetPointInfoGenerator() => this.PointInfoGenerator ??= this.TemplatedParent is Chart owner
        ? owner switch
        {
          CartesianChart cartesianChart => new CartesianPointInfoGenerator(cartesianChart) { PanelHost = this },
          _ => null
        }
        : null;

    private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as ChartPanelPresenter).OnContentChanged(e.OldValue, e.NewValue);
    private void OnContentChanged(object oldValue, object newValue)
    {
      if (this.TemplatedParent.TryFindVisualChildElement(out ScrollViewer scrollViewer))
      {
        scrollViewer.Content = newValue;
        scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
        scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
        if (!scrollViewer.CanContentScroll)
        {
          ((IScrollInfo)newValue).ScrollOwner = scrollViewer;
        }
      }

      if (!ReferenceEquals(this.ChartPanel, newValue))
      {
        this.ChartPanel = newValue as ChartPanel;
      }
    }

    public PointInfoGenerator PointInfoGenerator { get; set; }
    public ChartPanel ChartPanel
    {
      get => this.panel;
      set
      {
        this.panel = value;
        this.Content = this.ChartPanel;
      }
    }
  }
}
