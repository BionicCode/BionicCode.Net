
namespace BionicCode.Controls.Net.Wpf
{
  using System;
  using System.Collections.Generic;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Data;

  public abstract class PointInfoGenerator
  {
    protected PointInfoGenerator(Chart owner)
    {
      this.Owner = owner;
      this.InternalItems = this.Owner.Items;
      this.ChartPanelTable = new Dictionary<object, ChartPanel>();
      this.SeriesInfoToDrawingInfoDataMap = new Dictionary<ISeriesInfo, Dictionary<IChartPointInfo, object>>();
      this.ScrollViewerToChartPanelMap = new Dictionary<ScrollViewer, ChartPanel>();
    }

    public abstract void Generate(); 
    public abstract Rect GetValueBounds();
    protected abstract ChartPanel CreateDefaultPanel();

    public virtual void OnOwnerItemsChanged()
    {
      this.InternalItems = this.Owner.Items;
      Generate();
    }

    public void ChangeView(object viewId)
    {
      if (!this.ChartPanelTable.TryGetValue(viewId, out ChartPanel panel))
      {
        panel = this.Owner.ItemsPanel?.GetType().GetConstructor(new[] { typeof(PointInfoGenerator) }).Invoke(new[] { this }) as ChartPanel
          ?? CreateDefaultPanel();
        this.ChartPanelTable[viewId] = panel;
        InvalidatePanelLayout(panel);
      }

      CheckDisplayMode(panel);
      this.PanelHost.Content = panel;
    }

    public void UpdateDisplayMode(DisplayMode displayMode)
    {
      if (displayMode != this.CurrentDisplayMode)
      {
        this.CurrentDisplayMode = displayMode;
        ChartPanel panel = this.PanelHost.ChartPanel;
        if (panel.DisplayMode != this.CurrentDisplayMode)
        {
          InvalidatePanelLayout(panel);

          foreach (ChartPanel hiddenPanel in this.ChartPanelTable.Values)
          {
            if (ReferenceEquals(hiddenPanel, panel))
            {
              continue;
            }

            if (hiddenPanel.DisplayMode != this.CurrentDisplayMode)
            {
              InvalidatePanelLayout(hiddenPanel);
            }
          }
        }
      }
    }

    private void CheckDisplayMode(ChartPanel panel)
    {
      if (panel.DisplayMode != this.CurrentDisplayMode)
      {
        InvalidatePanelLayout(panel);
      }
    }

    private void InvalidatePanelLayout(ChartPanel panel)
    {
      panel.DisplayMode = this.CurrentDisplayMode;
      panel.InvalidatePlotData();
    }


    public Chart Owner { get; }
    internal protected CollectionView InternalItems { get; set; }
    internal Dictionary<object, ChartPanel> ChartPanelTable { get; }
    internal Dictionary<ISeriesInfo, Dictionary<IChartPointInfo, object>> SeriesInfoToDrawingInfoDataMap { get; }
    internal Dictionary<ScrollViewer, ChartPanel> ScrollViewerToChartPanelMap { get; }
    internal ChartPanelPresenter PanelHost { get; init; }
    private DisplayMode CurrentDisplayMode { get; set; }
  }
}