using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace BionicCode.Utilities.Net.Core.Wpf.AttachedBehaviors
{
  public class TextControl : DependencyObject
  {
    #region TextValue attached property

    public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
      "TextValue",
      typeof(string),
      typeof(TextControl),
      new PropertyMetadata(string.Empty, TextControl.OnTextChanged));

    public static void SetText([NotNull] DependencyObject attachingElement, string value)
    {
      attachingElement.SetValue(TextControl.TextProperty, value);
    }

    public static string GetText([NotNull] DependencyObject attachingElement)
    {
      return (string)attachingElement.GetValue(TextControl.TextProperty);
    }

    #endregion

    #region HighlightBackgroundColor attached property

    public static readonly DependencyProperty HighlightBackgroundProperty = DependencyProperty.RegisterAttached(
      "HighlightBackground",
      typeof(Brush),
      typeof(TextControl),
      new PropertyMetadata(Brushes.DarkRed));

    public static void SetHighlightBackground([NotNull] DependencyObject attachingElement, Brush value)
    {
      attachingElement.SetValue(TextControl.HighlightBackgroundProperty, value);
    }

    public static Brush GetHighlightBackground([NotNull] DependencyObject attachingElement)
    {
      return (Brush)attachingElement.GetValue(TextControl.HighlightBackgroundProperty);
    }

    #endregion

    #region HighlightForeground attached property

    public static readonly DependencyProperty HighlightForegroundProperty = DependencyProperty.RegisterAttached(
      "HighlightForeground",
      typeof(Brush),
      typeof(TextControl),
      new PropertyMetadata(default(Brush)));

    public static void SetHighlightForeground([NotNull] DependencyObject attachingElement, Brush value)
    {
      attachingElement.SetValue(TextControl.HighlightForegroundProperty, value);
    }

    public static Brush GetHighlightForeground([NotNull] DependencyObject attachingElement)
    {
      return (Brush)attachingElement.GetValue(TextControl.HighlightForegroundProperty);
    }

    #endregion

    #region IsEnabled attached property

    public static readonly DependencyProperty IsHighlightingEnabledProperty = DependencyProperty.RegisterAttached(
      "IsAutoHighlightingEnabled",
      typeof(bool),
      typeof(TextControl),
      new PropertyMetadata(default(bool), TextControl.OnIsHighlightingEnabledChanged));

    public static void SetIsHighlightingEnabled([NotNull] DependencyObject attachingElement, bool value)
    {
      attachingElement.SetValue(TextControl.IsHighlightingEnabledProperty, value);
    }

    public static bool GetIsHighlightingEnabled([NotNull] DependencyObject attachingElement)
    {
      return (bool)attachingElement.GetValue(TextControl.IsHighlightingEnabledProperty);
    }

    #endregion

    #region HighlightRanges attached property

    public static readonly DependencyProperty HighlightRangesProperty = DependencyProperty.RegisterAttached(
      "HighlightRanges",
      typeof(HighLightRangeCollection),
      typeof(TextControl),
      new PropertyMetadata(new HighLightRangeCollection(), TextControl.OnRangeAdded));

    public static void SetHighlightRanges([NotNull] DependencyObject attachingElement, HighLightRangeCollection value) => attachingElement.SetValue(TextControl.HighlightRangesProperty, value);

    public static HighLightRangeCollection GetHighlightRanges([NotNull] DependencyObject attachingElement) => (HighLightRangeCollection)attachingElement.GetValue(TextControl.HighlightRangesProperty);

    #endregion

    #region IsInitialized attached property

    private static readonly DependencyProperty IsInitializedProperty = DependencyProperty.RegisterAttached(
      "IsInitialized",
      typeof(bool),
      typeof(TextControl),
      new PropertyMetadata(default(bool)));

    private static void SetIsInitialized([NotNull] DependencyObject attachingElement, bool value)
    {
      attachingElement.SetValue(TextControl.IsInitializedProperty, value);
    }

    private static bool GetIsInitialized([NotNull] DependencyObject attachingElement)
    {
      return (bool)attachingElement.GetValue(TextControl.IsInitializedProperty);
    }

    #endregion


    private static void OnIsHighlightingEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if ((bool)e.OldValue && (bool)e.NewValue || TextControl.GetIsInitialized(d))
      {
        return;
      }

      if ((bool)e.NewValue)
      {
        TextControl.InitializeHighlighting(d as FrameworkElement);
      }
      else // Remove highlighting
      {

        var text = TextControl.GetText(d);
        TextControl.OnTextChanged(d, new DependencyPropertyChangedEventArgs(TextControl.TextProperty, text, text));
      }
    }

    private static void InitializeHighlighting(FrameworkElement frameworkElement)
    {
      if (!frameworkElement?.IsLoaded ?? false)
      {
        switch (frameworkElement)
        {
          case TextBlock textBlock:
            frameworkElement.Loaded += TextControl.InitializeAttachedTextBlockOnLoaded;
            break;
          case System.Windows.Controls.RichTextBox textBox:
            frameworkElement.Loaded += TextControl.InitializeAttachedTextBoxOnLoaded;
            break;
          default: return;
        }
      }

      switch (frameworkElement)
      {
        case TextBlock textBlock:
          TextControl.InitializeAttachedTextBlockOnLoaded(textBlock, EventArgs.Empty);
          break;
        case System.Windows.Controls.RichTextBox textBox:
          TextControl.InitializeAttachedTextBoxOnLoaded(textBox, EventArgs.Empty);
          break;
        default: return;
      }
    }

    private static void InitializeAttachedTextBoxOnLoaded(object sender, EventArgs e)
    {
      var textBox = sender as System.Windows.Controls.RichTextBox;
      TextControl.GetHighlightRanges(textBox).CollectionChanged +=
        (s, e) => TextControl.CreateTextBoxHighlights(textBox);

      //TextControl.SetText(textBox, textBox.TextValue);
      TextControl.CreateTextBoxHighlights(textBox);

      TextControl.SetIsInitialized(textBox, true);
    }

    private static void CreateTextBoxHighlights(System.Windows.Controls.RichTextBox textBox)
    {
      HighLightRangeCollection highlightRanges = TextControl.GetHighlightRanges(textBox);
      if (!highlightRanges.Any() || !TextControl.GetIsHighlightingEnabled(textBox) ||
          string.IsNullOrWhiteSpace(TextControl.GetText(textBox)))
      {
        return;
      }

      Brush highlightBackground = TextControl.GetHighlightBackground(textBox);
      var text = TextControl.GetText(textBox);
      if (textBox.Document == null)
      {
        textBox.Document = new FlowDocument();
      }
      else
      {
        textBox.Document.Blocks.Clear();
      }

      int highlightPosition = 0;
      List<HighlightRange> orderedHighlightRanges = highlightRanges.OrderBy(range => range.StartIndex).ToList();

      highlightPosition = TextControl.HandleLeadingNonHighlightText(textBox, orderedHighlightRanges, text);

      if (highlightPosition >= text.Length)
      {
        return;
      }

      foreach (HighlightRange highlightRange in orderedHighlightRanges.Where(
        highlightRange => highlightRange.StartIndex < text.Length))
      {
        var rangeLength = 0;
        if (highlightRange.StartIndex > highlightPosition)
        {
          highlightPosition = TextControl.HandleEnclosedNonHighlightText(
            textBox,
            highlightRange,
            highlightPosition,
            text);
        }

        rangeLength = Math.Min(highlightRange.EndIndex - highlightPosition + 1, text.Length);
        string textRange = text.Substring(highlightRange.StartIndex, rangeLength);
        textBox.Document.Blocks.Add(new Paragraph(new Run(textRange) { Background = highlightBackground }));
        highlightPosition += rangeLength;
      }

      TextControl.HandleTrailingNonHighlightText(textBox, text, highlightPosition);
    }

    private static void HandleTrailingNonHighlightText(System.Windows.Controls.RichTextBox textBox, string text, int highlightPosition)
    {
      if (highlightPosition < text.Length)
      {
        string textRange = text.Substring(highlightPosition);
        textBox.Document.Blocks.Add(new Paragraph(new Run(textRange)));
      }
    }

    private static int HandleLeadingNonHighlightText(
      System.Windows.Controls.RichTextBox textBox,
      IEnumerable<HighlightRange> orderedHighlightRanges,
      string text)
    {
      int rangeLength = Math.Min(orderedHighlightRanges.First().StartIndex, text.Length);
      if (rangeLength > 0)
      {
        string textRange = text.Substring(0, rangeLength);
        textBox.Document.Blocks.Add(new Paragraph(new Run(textRange)));
        return rangeLength;
      }

      return 0;
    }

    private static int HandleEnclosedNonHighlightText(
      System.Windows.Controls.RichTextBox textBox,
      HighlightRange highlightRange,
      int highlightPosition,
      string text)
    {
      int rangeLength = Math.Min(highlightRange.StartIndex - highlightPosition, text.Length);
      string textRange = text.Substring(highlightPosition, rangeLength);
      textBox.Document.Blocks.Add(new Paragraph(new Run(textRange)));
      highlightPosition += rangeLength;
      return highlightPosition;
    }

    private static void InitializeAttachedTextBlockOnLoaded(object sender, EventArgs e)
    {
      var textBlock = sender as TextBlock;
      TextControl.GetHighlightRanges(textBlock).CollectionChanged +=
        (s, e) => TextControl.CreateTextBlockHighlights(textBlock);

      BindingExpression bindingExpression = textBlock.GetBindingExpression(TextBlock.TextProperty);
      if (bindingExpression == null)
      {
        return;
      }

      TextControl.SetText(textBlock, textBlock.Text);
      textBlock.SetBinding(TextControl.TextProperty, bindingExpression.ParentBindingBase);
      TextControl.CreateTextBlockHighlights(textBlock);

      TextControl.SetIsInitialized(textBlock, true);
    }

    private static void OnTextChanged(
      DependencyObject attachingElement,
      DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      if (attachingElement is TextBlock textBlock)
      {
        if (TextControl.GetIsHighlightingEnabled(textBlock))
        {
          TextControl.CreateTextBlockHighlights(textBlock);
        }
        else
        {
          textBlock.Text = dependencyPropertyChangedEventArgs.NewValue as string;
        }
      }
      else if (attachingElement is System.Windows.Controls.RichTextBox textBox)
      {
        if (TextControl.GetIsHighlightingEnabled(textBox))
        {
          TextControl.CreateTextBoxHighlights(textBox);
        }
        else
        {
          TextControl.CreateRichTextBoxDefaultContent(dependencyPropertyChangedEventArgs.NewValue as string, textBox);
        }
      }
    }

    private static void CreateRichTextBoxDefaultContent(string text, System.Windows.Controls.RichTextBox textBox)
    {
      var paragraph = new Paragraph(new Run(text));
      var document = new FlowDocument(paragraph);
      textBox.Document = document;
    }

    private static void OnRangeAdded(
      DependencyObject attachingElement,
      DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      TextControl.InitializeHighlighting(attachingElement as FrameworkElement);
    }

    private static void CreateTextBlockHighlights(TextBlock textBlock)
    {
      BindingExpression textBinding = textBlock.GetBindingExpression(TextBlock.TextProperty);
      HighLightRangeCollection highlightRanges = TextControl.GetHighlightRanges(textBlock);
      if (!highlightRanges.Any() || !TextControl.GetIsHighlightingEnabled(textBlock) ||
          string.IsNullOrWhiteSpace(TextControl.GetText(textBlock)))
      {
        return;
      }

      Brush highlightBackground = TextControl.GetHighlightBackground(textBlock);
      var text = TextControl.GetText(textBlock);
      textBlock.Inlines.Clear();

      int highlightPosition = 0;
      List<HighlightRange> orderedHighlightRanges = highlightRanges.OrderBy(range => range.StartIndex).ToList();

      highlightPosition = TextControl.HandleLeadingNonHighlightText(textBlock, orderedHighlightRanges, text);

      if (highlightPosition >= text.Length)
      {
        return;
      }

      foreach (HighlightRange highlightRange in orderedHighlightRanges.Where(
        highlightRange => highlightRange.StartIndex < text.Length))
      {
        var rangeLength = 0;
        if (highlightRange.StartIndex > highlightPosition)
        {
          highlightPosition = TextControl.HandleEnclosedNonHighlightText(
            textBlock,
            highlightRange,
            highlightPosition,
            text);
        }

        rangeLength = Math.Min(highlightRange.EndIndex - highlightPosition + 1, text.Length);
        string textRange = text.Substring(highlightRange.StartIndex, rangeLength);
        textBlock.Inlines.Add(new Run(textRange) { Background = highlightBackground });
        highlightPosition += rangeLength;
      }

      TextControl.HandleTrailingNonHighlightText(textBlock, text, highlightPosition);
    }

    private static void HandleTrailingNonHighlightText(TextBlock textBlock, string text, int highlightPosition)
    {
      if (highlightPosition < text.Length)
      {
        string textRange = text.Substring(highlightPosition);
        textBlock.Inlines.Add(new Run(textRange));
      }
    }

    private static int HandleEnclosedNonHighlightText(
      TextBlock textBlock,
      HighlightRange highlightRange,
      int highlightPosition,
      string text)
    {
      int rangeLength = Math.Min(highlightRange.StartIndex - highlightPosition, text.Length);
      string textRange = text.Substring(highlightPosition, rangeLength);
      textBlock.Inlines.Add(new Run(textRange));
      highlightPosition += rangeLength;
      return highlightPosition;
    }

    private static int HandleLeadingNonHighlightText(
      TextBlock textBlock,
      IEnumerable<HighlightRange> orderedHighlightRanges,
      string text)
    {
      int rangeLength = Math.Min(orderedHighlightRanges.First().StartIndex, text.Length);
      if (rangeLength > 0)
      {
        string textRange = text.Substring(0, rangeLength);
        textBlock.Inlines.Add(new Run(textRange));
        return rangeLength;
      }

      return 0;
    }
  }
}
