namespace BionicCode.Controls.Net.Wpf
{
  using System;
  using System.Collections.Generic;
  using System.Windows;
  using System.Windows.Input;
  using System.Windows.Media;
  using BionicCode.Utilities.Net;

  internal class TextSite : FrameworkElement
  {
    public event EventHandler<TextClickedEventArgs> TextClicked;
    public CaretInfo CaretInfo { get; private set; }
    private VisualCollection Children { get; }
    private DrawingVisual Child { get; set; }
    private CaretAdorner CaretAdorner { get; set; }
    private TextRenderInfo CurrentTextRenderInfo { get; set; }
    private Point LastHitTestPosition { get; set; }

    public TextSite()
    {
      this.Children = new VisualCollection(this);
      this.Child = new DrawingVisual();
      _ = this.Children.Add(this.Child);
      this.Focusable = true;
      this.CaretAdorner = new CaretAdorner(this);
    }

    public void RenderText(TextRenderInfo textRenderInfo, Point textPosition)
    {
      this.CurrentTextRenderInfo = textRenderInfo;
      InvalidateMeasure();
      using IDisposable frozenCaretScope = this.CaretAdorner.CreateFrozenCaretScope();
      using DrawingContext drawingContext = this.Child.RenderOpen();
      DrawTextRenderInfo(drawingContext, textRenderInfo, textPosition);
    }

    public void SetCaretPosition(CaretInfo caretInfo)
    {
      this.CaretInfo = caretInfo;
      this.CaretAdorner.UpdateCaret(this.CaretInfo);
    }

    protected override Visual GetVisualChild(int index) => this.Child;
    protected override int VisualChildrenCount => 1;

    protected override Size MeasureOverride(Size availableSize)
    {
      _ = base.MeasureOverride(availableSize);
      double maxTextWidth = this.CurrentTextRenderInfo?.FormattedText.MaxTextWidth ?? 0;
      double requestedWidth = maxTextWidth == 0 ? this.CurrentTextRenderInfo?.FormattedText.WidthIncludingTrailingWhitespace + 20 ?? 0 : maxTextWidth;
      return new Size(requestedWidth, 100);
      //return new Size(
      //          double.IsInfinity(availableSize.Width) ? double.MaxValue / 2 : availableSize.Width,
      //          double.IsInfinity(availableSize.Height) ? double.MaxValue / 2 : availableSize.Height);
    }

    public void Render(Geometry geometry)
    {
      using DrawingContext drawingContext = this.Child.RenderOpen();
      DrawTextRenderInfo(drawingContext, this.CurrentTextRenderInfo, new Point());
      drawingContext.DrawGeometry(Brushes.Red, new Pen(Brushes.YellowGreen, 1), geometry);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
      base.OnMouseLeftButtonUp(e);
      this.LastHitTestPosition = e.GetPosition(this);
      Rect textBox = this.CurrentTextRenderInfo.FormattedText.BuildGeometry(new Point()).Bounds;
      textBox.Inflate(0, this.CurrentTextRenderInfo.FormattedText.Height / 2);
      if (textBox.Contains(this.LastHitTestPosition))
      {
        OnTextClicked();
      }
      //VisualTreeHelper.HitTest(this, null, OnHitTestSuccessful, new PointHitTestParameters(this.LastHitTestPosition));
    }

    private HitTestResultBehavior OnHitTestSuccessful(HitTestResult result)
    {
      if (result.VisualHit == this.Child)
      {
        OnTextClicked();
        return HitTestResultBehavior.Stop;
      }

      return HitTestResultBehavior.Continue;
    }

    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
      base.OnGotKeyboardFocus(e);
      CreateCaret();
    }

    protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
      base.OnPreviewLostKeyboardFocus(e);
      DestroyCaret();
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
      base.OnLostFocus(e);
      DestroyCaret();
    }

    protected virtual void OnTextClicked()
      => this.TextClicked?.Invoke(this, new TextClickedEventArgs(this.LastHitTestPosition, this.CurrentTextRenderInfo));

    private void CreateCaret() => this.CaretAdorner.Show(this.CaretInfo);

    private void DestroyCaret() => this.CaretAdorner.Hide();

    private void DrawTextRenderInfo(DrawingContext drawingContext, TextRenderInfo textRenderInfo, Point position)
    {
      DrawHighlightBackground(drawingContext, textRenderInfo.HighlightBackgroundInfos);
      DrawText(drawingContext, textRenderInfo.FormattedText, position);
    }

    private void DrawHighlightBackground(DrawingContext drawingContext, IEnumerable<HighlightBackgroundInfo> highlightBackgroundInfos)
    {
      foreach (HighlightBackgroundInfo highlightBackgroundInfo in highlightBackgroundInfos)
      {
        var pen = new Pen(highlightBackgroundInfo.BorderBrush, highlightBackgroundInfo.BorderThickness);
        pen.Freeze();
        drawingContext.DrawGeometry(highlightBackgroundInfo.BackgroundBrush, pen, highlightBackgroundInfo.BackgroundGeometry);
      }
    }

    private void DrawText(DrawingContext drawingContext, FormattedText text, Point position) => drawingContext.DrawText(text, position);
  }
}
