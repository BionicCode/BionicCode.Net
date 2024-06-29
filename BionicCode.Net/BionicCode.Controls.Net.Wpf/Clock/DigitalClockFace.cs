using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace BionicCode.Controls.Net.Wpf
{
  public class DigitalClockFace : ClockFace, ISevenSegmentDisplay
  {
    public DigitalClockFace()
    {
      this.Digits = new SortedSet<ISevenSegmentDigit>(Comparer<ISevenSegmentDigit>.Create((digit1, digit2) => digit1.DisplayIndex.CompareTo(digit2.DisplayIndex)));
      this.SegmentDisplayDriver = new SegmentDisplayDriver(this) { IsPaddingEnabled = true };
      this.ClockFaceBackgroundFrame = new Rectangle() { Fill = this.Background };
    }

    #region Overrides of ClockFace

    /// <inheritdoc />
    protected override void OnSelectedTimeChanged(DateTime oldValue, DateTime newValue)
    {
      base.OnSelectedTimeChanged(oldValue, newValue);

      double concatenatedTimeValues = (this.SelectedHour * Math.Pow(10, 4))
                                      + (this.SelectedMinute * Math.Pow(10, 2))
                                      + this.SelectedSecond;
      this.SegmentDisplayDriver.SetValue((int)concatenatedTimeValues);
    }

    #endregion

    #region Overrides of Control

    #region Overrides of ClockFace

    /// <inheritdoc />
    protected override Size MeasureOverride(Size constraint)
    {
      base.MeasureOverride(constraint);
      DrawClockFace(constraint);
      if (!IsValidArrangeSize(constraint))
      {
        constraint = base.MeasureOverride(constraint);
      }
      return constraint;
    }

    #endregion

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size arrangeBounds)
    {
      base.ArrangeOverride(arrangeBounds);
      DrawClockFace(arrangeBounds);
      Size minimumArrangeBounds = GetNaturalSize();
      this.ClockFaceBackgroundFrame.Width = minimumArrangeBounds.Width;
      this.ClockFaceBackgroundFrame.Height = minimumArrangeBounds.Height;
      this.ClockFaceBackgroundFrame.Fill = this.Background;
      return minimumArrangeBounds;
    }
    #endregion

    #region Overrides of ClockFace

    /// <inheritdoc />
    protected override Size GetNaturalSize()
    {
      if (!this.Digits.Any())
      {
        return new Size(0, 0);
      }
      var child = this.Digits.First() as UIElement;
      if (!child.IsMeasureValid)
      {
        child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
      }
      double totalWidth = Canvas.GetLeft(child) + child.DesiredSize.Width;
      double totalHeight = child.DesiredSize.Height;

      return new Size(totalWidth, totalHeight);
    }

    private void DrawClockFace(Size arrangeBounds)
    {
      if (this.ClockFaceCanvas.Children.Count != 0)
      {
        return;
      }
      //this.ClockFaceCanvas.Children.Clear();
      //this.Digits.Clear();

      AddElementToClockFace(this.ClockFaceBackgroundFrame, new Point(), 0);

      var hourDigit0 = new SevenSegmentDisplayDigit(5);
      hourDigit0.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

      double digitWidth = hourDigit0.DesiredSize.Width;
      double separatorMargin = digitWidth * 0.5;

      var digitPosition = new Point(0, 0);

      AddElementToClockFace(hourDigit0, digitPosition);
      this.Digits.Add(hourDigit0);

      digitPosition.Offset(digitWidth, 0);
      var hourDigit1 = new SevenSegmentDisplayDigit(4);
      AddElementToClockFace(hourDigit1, digitPosition);
      this.Digits.Add(hourDigit1);

      digitPosition.Offset(digitWidth + (separatorMargin / 2), 0);
      var separatorDisplayDigit0 = new SeparatorDisplayDigit();
      AddElementToClockFace(separatorDisplayDigit0, digitPosition);

      digitPosition.Offset(separatorMargin / 2, 0);
      var minuteDigit0 = new SevenSegmentDisplayDigit(3);
      AddElementToClockFace(minuteDigit0, digitPosition);
      this.Digits.Add(minuteDigit0);

      digitPosition.Offset(digitWidth, 0);
      var minuteDigit1 = new SevenSegmentDisplayDigit(2);
      AddElementToClockFace(minuteDigit1, digitPosition);
      this.Digits.Add(minuteDigit1);

      digitPosition.Offset(digitWidth + (separatorMargin / 2), 0);
      var separatorDisplayDigit1 = new SeparatorDisplayDigit();
      AddElementToClockFace(separatorDisplayDigit1, digitPosition);

      digitPosition.Offset(separatorMargin / 2, 0);
      var secondDigit0 = new SevenSegmentDisplayDigit(1);
      AddElementToClockFace(secondDigit0, digitPosition);
      this.Digits.Add(secondDigit0);

      digitPosition.Offset(digitWidth, 0);
      var secondDigit1 = new SevenSegmentDisplayDigit(0);
      AddElementToClockFace(secondDigit1, digitPosition);
      this.Digits.Add(secondDigit1);

      OnClockFaceLoaded();
    }

    #endregion

    public SortedSet<ISevenSegmentDigit> Digits { get; }
    private SegmentDisplayDriver SegmentDisplayDriver { get; }
    private Rectangle ClockFaceBackgroundFrame { get; }
  }
}
