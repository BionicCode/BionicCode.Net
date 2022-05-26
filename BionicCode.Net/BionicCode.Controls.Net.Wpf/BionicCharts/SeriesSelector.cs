namespace BionicCode.Controls.Net.Wpf.BionicCharts
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;
  using System.Windows;
  using System.Windows.Media;
  
  public class SeriesSelector
  {
    public virtual IEnumerable<ISeriesInfo> ProvideSeriesCollection(object dataContext, ResourceLocator resourceLocator, IEnumerable itemsSource)
    {
      return new List<ISeriesInfo> { new CartesianSeriesInfo<Point>(itemsSource.Cast<Point>(), Brushes.OrangeRed, 1) };
    }
  }
}
