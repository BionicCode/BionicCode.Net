using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace BionicCode.Utilities.Net.Wpf.AttachedBehaviors
{
  /// <summary>
  /// Attached behavior that supports dynamic text highlighting for controls derived from <see cref="TextBlock"/> or <see cref="RichTextBox"/>.
  /// </summary>
  /// <seealso href="https://github.com/BionicCode/BionicCode.Net#textcontrol">See advanced example</seealso>
  public class TextControl : DependencyObject
  {
    #region TextValue attached property

    /// <summary>
    /// Attached property to serve as alternative text property for the <see cref="RichTextBox"/> (instead of using <see cref="RichTextBox.Document"/>). Optional property to use with <see cref="TextBlock"/> (instead of <see cref="TextBlock.Text"/>). The defined <see cref="HighlightRange"/> items contained in the attached property <see cref="HighlightRangesProperty"/> collection will always be applied to <see cref="TextBlock.Text"/> and the <see cref="TextProperty"/> values. 
    /// </summary>
    /// <remarks>In case of the <see cref="TextProperty"/> being attached to a <see cref="RichTextBox"/>, the string value will be converted to a <see cref="FlowDocument"/> and assigned to the <see cref="RichTextBox.Document"/> property.</remarks>
    public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
      "TextValue",
      typeof(string),
      typeof(TextControl),
      new PropertyMetadata(string.Empty, TextControl.OnTextChanged));

    /// <summary>
    /// Set method of attached property <see cref="TextProperty"/>.
    /// </summary>
    /// <param name="attachingElement">The attaching <see cref="TextBlock"/> or <see cref="RichTextBox"/>.</param>
    /// <param name="value">The text to display.</param>
    public static void SetText(DependencyObject attachingElement, string value) => attachingElement.SetValue(TextControl.TextProperty, value);

    /// <summary>
    /// Get method of the attached property <see cref="TextProperty"/>.
    /// </summary>
    /// <param name="attachingElement">The attaching <see cref="TextBlock"/> or <see cref="RichTextBox"/>.</param>
    /// <returns>The current text value.</returns>
    public static string GetText(DependencyObject attachingElement) => (string)attachingElement.GetValue(TextControl.TextProperty);

    #endregion

    #region HighlightBackgroundColor attached property
    /// <summary>
    /// Attached property to define the background <see cref="Brush"/> for the highlight text, which is defined by <see cref="HighlightRange"/> items contained in the <see cref="HighlightRangesProperty"/> attached property. 
    /// </summary>
    public static readonly DependencyProperty HighlightBackgroundProperty = DependencyProperty.RegisterAttached(
      "HighlightBackground",
      typeof(Brush),
      typeof(TextControl),
      new PropertyMetadata(Brushes.DarkRed, TextControl.OnHighlightColorsChanged));

    /// <summary>
    /// Set method of attached property <see cref="HighlightBackgroundProperty"/>.
    /// </summary>
    /// <param name="attachingElement">The attaching <see cref="TextBlock"/> or <see cref="RichTextBox"/>.</param>
    /// <param name="value">The <see cref="Brush"/> for the background of the highlight text ranges.</param>
    public static void SetHighlightBackground(DependencyObject attachingElement, Brush value) => attachingElement.SetValue(TextControl.HighlightBackgroundProperty, value);

    /// <summary>
    /// Get method of the attached property <see cref="HighlightBackgroundProperty"/>.
    /// </summary>
    /// <param name="attachingElement">The attaching <see cref="TextBlock"/> or <see cref="RichTextBox"/>.</param>
    /// <returns>The <see cref="Brush"/> for the background of the highlight text ranges.</returns>
    public static Brush GetHighlightBackground(DependencyObject attachingElement) => (Brush)attachingElement.GetValue(TextControl.HighlightBackgroundProperty);

    #endregion

    #region HighlightForeground attached property

    /// <summary>
    /// Attached property to define the foreground <see cref="Brush"/> for the highlight text, which is defined by <see cref="HighlightRange"/> items contained in the <see cref="HighlightRangesProperty"/> attached property. 
    /// </summary>
    public static readonly DependencyProperty HighlightForegroundProperty = DependencyProperty.RegisterAttached(
      "HighlightForeground",
      typeof(Brush),
      typeof(TextControl),
      new PropertyMetadata(default(Brush), TextControl.OnHighlightColorsChanged));

    /// <summary>
    /// Set method of attached property <see cref="HighlightForegroundProperty"/>.
    /// </summary>
    /// <param name="attachingElement">The attaching <see cref="TextBlock"/> or <see cref="RichTextBox"/>.</param>
    /// <param name="value">The <see cref="Brush"/> for the foreground of the highlight text ranges.</param>
    public static void SetHighlightForeground(DependencyObject attachingElement, Brush value) => attachingElement.SetValue(TextControl.HighlightForegroundProperty, value);

    /// <summary>
    /// Get method of the attached property <see cref="HighlightForegroundProperty"/>.
    /// </summary>
    /// <param name="attachingElement">The attaching <see cref="TextBlock"/> or <see cref="RichTextBox"/>.</param>
    /// <returns>The <see cref="Brush"/> for the foreground of the highlight text ranges.</returns>
    public static Brush GetHighlightForeground(DependencyObject attachingElement) => (Brush)attachingElement.GetValue(TextControl.HighlightForegroundProperty);

    #endregion

    #region IsEnabled attached property

    /// <summary>
    /// Attached property to enable or disable the highlight attached behavior <see cref="TextControl"/>. cref="HighlightRangesProperty"/> attached property. 
    /// </summary>
    public static readonly DependencyProperty IsHighlightingEnabledProperty = DependencyProperty.RegisterAttached(
      "IsAutoHighlightingEnabled",
      typeof(bool),
      typeof(TextControl),
      new PropertyMetadata(default(bool), TextControl.OnIsHighlightingEnabledChanged));

    /// <summary>
    /// Set method of attached property <see cref="IsHighlightingEnabledProperty"/>.
    /// </summary>
    /// <param name="attachingElement">The attaching <see cref="TextBlock"/> or <see cref="RichTextBox"/>.</param>
    /// <param name="value"><c>true</c> to enable the attached <see cref="TextControl"/> behavior or <c>false</c> to disable it.</param>
    public static void SetIsHighlightingEnabled(DependencyObject attachingElement, bool value) => attachingElement.SetValue(TextControl.IsHighlightingEnabledProperty, value);

    /// <summary>
    /// Get method of the attached property <see cref="HighlightForegroundProperty"/>.
    /// </summary>
    /// <param name="attachingElement">The attaching <see cref="TextBlock"/> or <see cref="RichTextBox"/>.</param>
    /// <returns><c>true</c> if the the attached <see cref="TextControl"/> behavior is enabled or <c>false</c> if it is disabled.</returns>
    public static bool GetIsHighlightingEnabled(DependencyObject attachingElement) => (bool)attachingElement.GetValue(TextControl.IsHighlightingEnabledProperty);

    #endregion

    #region HighlightRanges attached property

    /// <summary>
    /// Attached property to define a <see cref="HighlightRangeCollection"/> of <see cref="HighlightRange"/> items. cref="HighlightRangesProperty"/> attached property. 
    /// </summary>
    /// <remarks>This collection implements <see cref="INotifyCollectionChanged"/>.</remarks>
    public static readonly DependencyProperty HighlightRangesProperty = DependencyProperty.RegisterAttached(
      "HighlightRanges",
      typeof(HighlightRangeCollection),
      typeof(TextControl),
      new PropertyMetadata(new HighlightRangeCollection(), TextControl.OnRangeAdded));

    /// <summary>
    /// Set method of attached property <see cref="HighlightRangesProperty"/>.
    /// </summary>
    /// <param name="attachingElement">The attaching <see cref="TextBlock"/> or <see cref="RichTextBox"/>.</param>
    /// <param name="value">A <see cref="HighlightRangeCollection"/>.</param>
    public static void SetHighlightRanges(DependencyObject attachingElement, HighlightRangeCollection value) => attachingElement.SetValue(TextControl.HighlightRangesProperty, value);

    /// <summary>
    /// Get method of the attached property <see cref="HighlightRangesProperty"/>.
    /// </summary>
    /// <param name="attachingElement">The attaching <see cref="TextBlock"/> or <see cref="RichTextBox"/>.</param>
    /// <returns>A <see cref="HighlightRangeCollection"/>.</returns>
    public static HighlightRangeCollection GetHighlightRanges(DependencyObject attachingElement) => (HighlightRangeCollection)attachingElement.GetValue(TextControl.HighlightRangesProperty);

    #endregion

    #region IsInitialized attached property

    private static readonly DependencyProperty IsInitializedProperty = DependencyProperty.RegisterAttached(
      "IsInitialized",
      typeof(bool),
      typeof(TextControl),
      new PropertyMetadata(default(bool)));

    private static void SetIsInitialized(DependencyObject attachingElement, bool value) => attachingElement.SetValue(TextControl.IsInitializedProperty, value);

    private static bool GetIsInitialized(DependencyObject attachingElement) => (bool)attachingElement.GetValue(TextControl.IsInitializedProperty);

    #endregion

    private static Dictionary<INotifyCollectionChanged, DependencyObject> INotifyCollectionToAttachedElementMap { get; }

    static TextControl()
    {
      TextControl.INotifyCollectionToAttachedElementMap = new Dictionary<INotifyCollectionChanged, DependencyObject>();
    }

    private static void OnIsHighlightingEnabledChanged(DependencyObject attachingElement, DependencyPropertyChangedEventArgs e)
    {
      if ((bool)e.OldValue && (bool)e.NewValue || TextControl.GetIsInitialized(attachingElement))
      {
        return;
      }

      if ((bool)e.NewValue)
      {
        TextControl.InitializeHighlighting(attachingElement as FrameworkElement);
      }
      else // Remove highlighting
      {
        HighlightRangeCollection highlightRangeCollection = TextControl.GetHighlightRanges(attachingElement);
        if (highlightRangeCollection != null)
        {
          highlightRangeCollection.CollectionChanged -= TextControl.CreateTextHighlightsOnHighLightRangeCollectionChanged;
        }
        string text = TextControl.GetText(attachingElement);
        TextControl.OnTextChanged(attachingElement, new DependencyPropertyChangedEventArgs(TextControl.TextProperty, text, text));
      }
    }

    private static void InitializeHighlighting(FrameworkElement frameworkElement)
    {
      if (!frameworkElement?.IsLoaded ?? false)
      {
        switch (frameworkElement)
        {
          case TextBlock _:
            frameworkElement.Loaded += TextControl.InitializeAttachedTextBlockOnLoaded;
            break;
          case System.Windows.Controls.RichTextBox _:
            frameworkElement.Loaded += TextControl.InitializeAttachedTextBoxOnLoaded;
            break;
          default: return;
        }
        return;
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
      textBox.Loaded -= TextControl.InitializeAttachedTextBoxOnLoaded;
      HighlightRangeCollection highlightRangeCollection = TextControl.GetHighlightRanges(textBox);
      highlightRangeCollection.CollectionChanged += TextControl.CreateTextHighlightsOnHighLightRangeCollectionChanged;
      TextControl.INotifyCollectionToAttachedElementMap.Add(highlightRangeCollection, textBox);

      TextControl.CreateTextBoxHighlights(textBox);
      TextControl.SetIsInitialized(textBox, true);
    }

    private static void CreateTextHighlightsOnHighLightRangeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (!TextControl.INotifyCollectionToAttachedElementMap.TryGetValue(sender as INotifyCollectionChanged, out DependencyObject attachedElement))
      {
        return;
      }

      switch (attachedElement)
      {
        case RichTextBox richTextBox:
          TextControl.CreateTextBoxHighlights(richTextBox);
          break;
        case TextBlock textBlock:
          TextControl.CreateTextBlockHighlights(textBlock);
          break;
      }
    }

    private static void CreateTextBoxHighlights(System.Windows.Controls.RichTextBox textBox)
    {
      HighlightRangeCollection highlightRanges = TextControl.GetHighlightRanges(textBox);
      if (!highlightRanges.Any() || !TextControl.GetIsHighlightingEnabled(textBox) ||
          string.IsNullOrWhiteSpace(TextControl.GetText(textBox)))
      {
        return;
      }

      Brush highlightBackground = TextControl.GetHighlightBackground(textBox);
      Brush highlightForeground = TextControl.GetHighlightForeground(textBox);
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
        var range = new TextRange(textBox.Document.ContentEnd, textBox.Document.ContentEnd) { Text = textRange };
        range.ApplyPropertyValue(TextElement.BackgroundProperty, highlightBackground);
        range.ApplyPropertyValue(TextElement.ForegroundProperty, highlightForeground);
        highlightPosition += rangeLength;
      }

      TextControl.HandleTrailingNonHighlightText(textBox, text, highlightPosition);
    }

    private static void HandleTrailingNonHighlightText(System.Windows.Controls.RichTextBox textBox, string text, int highlightPosition)
    {
      if (highlightPosition < text.Length)
      {
        string textRange = text.Substring(highlightPosition);
        var range = new TextRange(textBox.Document.ContentEnd, textBox.Document.ContentEnd) { Text = textRange };
        range.ApplyPropertyValue(TextElement.BackgroundProperty, textBox.Background);
        range.ApplyPropertyValue(TextElement.ForegroundProperty, textBox.Foreground);
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
        var range = new TextRange(textBox.Document.ContentEnd, textBox.Document.ContentEnd) { Text = textRange };
        range.ApplyPropertyValue(TextElement.BackgroundProperty, textBox.Background);
        range.ApplyPropertyValue(TextElement.ForegroundProperty, textBox.Foreground);
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
      HighlightRangeCollection highlightRangeCollection = TextControl.GetHighlightRanges(textBlock);
      highlightRangeCollection.CollectionChanged += TextControl.CreateTextHighlightsOnHighLightRangeCollectionChanged;
      TextControl.INotifyCollectionToAttachedElementMap.Add(highlightRangeCollection, textBlock);

      BindingExpression bindingExpression = textBlock.GetBindingExpression(TextBlock.TextProperty);
      if (bindingExpression != null)
      {
        TextControl.SetText(textBlock, textBlock.Text);
        textBlock.SetBinding(TextControl.TextProperty, bindingExpression.ParentBindingBase);
      }

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

    private static void OnHighlightColorsChanged(DependencyObject attachingElement, DependencyPropertyChangedEventArgs e)
    {

      if ((attachingElement as FrameworkElement).IsLoaded)
      {
        string text = TextControl.GetText(attachingElement);
        TextControl.OnTextChanged(attachingElement, new DependencyPropertyChangedEventArgs(TextControl.TextProperty, text, text));
      }
    }

    private static void OnRangeAdded(
      DependencyObject attachingElement,
      DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      if (dependencyPropertyChangedEventArgs.OldValue is INotifyCollectionChanged oldHighlightRanges)
      {
        oldHighlightRanges.CollectionChanged -= TextControl.CreateTextHighlightsOnHighLightRangeCollectionChanged;
        TextControl.INotifyCollectionToAttachedElementMap.Remove(oldHighlightRanges);
      }

      if ((attachingElement as FrameworkElement).IsLoaded)
      {
        TextControl.InitializeHighlighting(attachingElement as FrameworkElement);
      }
    }

    private static void CreateTextBlockHighlights(TextBlock textBlock)
    {
      HighlightRangeCollection highlightRanges = TextControl.GetHighlightRanges(textBlock);
      if (!highlightRanges.Any() || !TextControl.GetIsHighlightingEnabled(textBlock) ||
          string.IsNullOrWhiteSpace(TextControl.GetText(textBlock)))
      {
        return;
      }

      Brush highlightBackground = TextControl.GetHighlightBackground(textBlock);
      Brush highlightForeground = TextControl.GetHighlightForeground(textBlock);
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
        textBlock.Inlines.Add(new Run(textRange) { Background = highlightBackground, Foreground = highlightForeground });
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
