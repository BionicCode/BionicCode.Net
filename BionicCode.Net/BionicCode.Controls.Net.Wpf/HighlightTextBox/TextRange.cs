namespace BionicCode.Controls.Net.Wpf
{
  using System;
  using System.Globalization;
  using System.Windows;
  using System.Windows.Media;

  public class TextRange
  {
    public TextRange(int begin, Brush background, Brush foreground, string text)
      : this(begin, background, foreground, Brushes.Transparent, 0, text)
    {
    }

    public TextRange(int begin, Brush background, Brush foreground, Brush borderBrush, double borderThickness, string text)
    {
      if (begin < 0)
      {
        throw new ArgumentException("Value must be greater or equal to '0'.", nameof(begin));
      }

      if (text is null)
      {
        throw new ArgumentNullException(nameof(text));
      }

      this.Text = text;
      this.CultureInfo = CultureInfo.CurrentCulture;
      this.Begin = begin;
      this.Length = text.Length;
      this.Background = background;
      if (this.Background.CanFreeze)
      {
        this.Background.Freeze();
      }

      this.TextBrush = foreground;
      if (this.TextBrush.CanFreeze)
      {
        this.TextBrush.Freeze();
      }

      this.BorderBrush = borderBrush;
      if (this.BorderBrush.CanFreeze)
      {
        this.BorderBrush.Freeze();
      }

      this.BorderThickness = borderThickness;
    }

    public TextRange ClipToBounds(int newBegin, int newLength) => new(newBegin, this.Background, this.TextBrush, this.BorderBrush, this.BorderThickness, this.Text.Substring(newBegin, newLength))
    {
      CultureInfo = this.CultureInfo,
      NumberSubstitution = this.NumberSubstitution,
      FontFamily = this.FontFamily,
      FallbackFontFamily = this.FallbackFontFamily,
      FontStyle = this.FontStyle,
      FontWeight = this.FontWeight,
      FontStretch = this.FontStretch,
      FontSize = this.FontSize,
      TextDecorations = this.TextDecorations,
    };

    public int Begin { get; }
    public int Length { get; }
    public Brush Background { get; }
    public Brush TextBrush { get; }
    public Brush BorderBrush { get; }
    public double BorderThickness { get; }
    public CultureInfo CultureInfo { get; set; }
    public string Text { get; set; }
    public NumberSubstitution NumberSubstitution { get; set; }
    public FontFamily FontFamily { get; set; }

    public FontFamily FallbackFontFamily { get; set; }

    public FontStyle FontStyle { get; set; }

    public FontWeight FontWeight { get; set; }

    public FontStretch FontStretch { get; set; }

    public double FontSize { get; set; }

    public TextDecorationCollection TextDecorations { get; set; }
  }
}
