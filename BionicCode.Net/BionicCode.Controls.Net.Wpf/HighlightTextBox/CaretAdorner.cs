namespace BionicCode.Controls.Net.Wpf
{
  using System;
  using System.Windows;
  using System.Windows.Documents;
  using System.Windows.Media;
  using System.Windows.Media.Animation;
  using BionicCode.Utilities.Net;

  internal class CaretAdorner : Adorner
  {
    private CaretInfo CaretProperties { get; set; }
    private AnimationClock CaretAnimationClock { get; set; }
    private Lazy<AdornerLayer> caretAdornerLayer;
    private AdornerLayer CaretAdornerLayer => this.caretAdornerLayer.Value;
    private CaretVisibility CaretVisibility { get; set; }

    public CaretAdorner(UIElement adornedElement) : base(adornedElement)
      => this.caretAdornerLayer = new Lazy<AdornerLayer>(InitializeCaretRenderingContext);

    public void UpdateCaret(CaretInfo caretInfo)
    {
      this.CaretProperties = caretInfo;
      if (this.CaretVisibility == CaretVisibility.Visible)
      {
        InvalidateVisual();
      }
      else
      {
        _ = Show();
      }
    }

    public bool Show(CaretInfo caretInfo)
    {
      this.CaretProperties = caretInfo;
      if (this.CaretVisibility == CaretVisibility.Visible
        || this.CaretAdornerLayer is null)
      {
        return false;
      }

      this.CaretVisibility = CaretVisibility.Visible;
      this.CaretAdornerLayer.Add(this);
      ApplyAnimationClock(UIElement.OpacityProperty, this.CaretAnimationClock);
      this.CaretAnimationClock.Controller.Begin();
      InvalidateVisual();

      return true;
    }

    public bool Show() => Show(this.CaretProperties);

    public void Freeze() => this.CaretAnimationClock.Controller.Stop();

    public void Unfreeze() => this.CaretAnimationClock.Controller.Begin();

    public IDisposable CreateFrozenCaretScope() => new FrozenCaretScope(this);

    public void Hide()
    {
      if (this.CaretVisibility == CaretVisibility.Hidden)
      {
        return;
      }

      this.CaretVisibility = CaretVisibility.Hidden;
      this.CaretAnimationClock.Controller.Stop();
      ApplyAnimationClock(UIElement.OpacityProperty, null);
      this.CaretAdornerLayer.Remove(this);
      InvalidateVisual();
    }

    private AdornerLayer InitializeCaretRenderingContext()
    {
      TimeSpan flashTime = Win32Caret.GetFlashTime();
      var doubleAnimation = new DoubleAnimationUsingKeyFrames()
      {
        Duration = new Duration(flashTime),
        RepeatBehavior = RepeatBehavior.Forever
      };

      _ = doubleAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame(0, KeyTime.FromPercent(0.5)));
      _ = doubleAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame(1, KeyTime.FromPercent(1)));
      this.CaretAnimationClock = doubleAnimation.CreateClock();

      var adornerLayer = AdornerLayer.GetAdornerLayer(this.AdornedElement);
      if (adornerLayer is null)
      {
        // Reset factory
        this.caretAdornerLayer = new Lazy<AdornerLayer>(InitializeCaretRenderingContext);
      }

      return adornerLayer;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
      base.OnRender(drawingContext);
      var pen = new Pen(Brushes.Red, this.CaretProperties.Width);
      drawingContext.DrawLine(pen, new Point(this.CaretProperties.Position.X, this.CaretProperties.Position.Y), new Point(this.CaretProperties.Position.X, this.CaretProperties.Position.Y + this.CaretProperties.Height));
    }
  }
}
