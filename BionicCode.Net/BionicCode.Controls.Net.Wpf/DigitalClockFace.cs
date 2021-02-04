using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace BionicCode.Controls.Net.Wpf
{
  public class DigitalClockFace : ClockFace
  {
    static DigitalClockFace()
    {
      FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DigitalClockFace), new FrameworkPropertyMetadata(typeof(DigitalClockFace)));
    }

    public DigitalClockFace()
    {
    }

    #region Overrides of Control

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size arrangeBounds)
    {
      DrawClockFace(arrangeBounds);
      return base.ArrangeOverride(arrangeBounds);
    }
    #endregion

    #region Overrides of ClockFace

    /// <inheritdoc />
    protected override Size GetNaturalSize()
    {
      double totalWidth = 0;
      double totalHeight = 0;
      foreach (UIElement child in this.ClockFaceCanvas.Children)
      {
        if (!child.IsMeasureValid)
        {
          child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        }
        totalHeight = Math.Max(totalHeight, child.DesiredSize.Height);
        totalWidth += child.DesiredSize.Width;
      }

      return new Size(totalWidth, totalHeight);
    }

    private void DrawClockFace(Size arrangeBounds)
    {
      this.ClockFaceCanvas.Children.Clear();
      var digit0 = new SevenSegmentDigit();
      this.ClockFaceCanvas.Children.Add(digit0);
    }


    #endregion
  }
}
