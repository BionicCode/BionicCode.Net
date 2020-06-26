#region Usings

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

#endregion

namespace BionicCode.Controls.Net.Core.Wpf
{
  /// <summary>
  /// Implement this interface to apply a custom highlight style for more complex scenarios e.g., code editors.
  /// </summary>
  public interface IMatchFormatter
  {
    /// <summary>
    /// Executes when the <see cref="HighlightRichTextBox"/> highlights found matches.
    /// </summary>
    /// <param name="matchInfo">A <see cref="MatchInfo"/> that contains the match to give access to the actual <see cref="TextRange"/>.</param>
    /// <param name="args">A <see cref="HighlightArgs"/> that holds the current configured highlight formatting attributes of the <see cref="HighlightRichTextBox"/> and a reference to the actual <see cref="HighlightRichTextBox"/>.</param>
    void FormatMatch(MatchInfo matchInfo, HighlightArgs args);
  }

  public class DefaultMatchFormatter : IMatchFormatter
  {
    #region Implementation of IMatchFormatter

    /// <inheritdoc />
    public void FormatMatch(MatchInfo matchInfo, HighlightArgs args)
    {
      matchInfo.TextRange.ApplyPropertyValue(TextElement.ForegroundProperty, args.Foreground);

      matchInfo.TextRange.ApplyPropertyValue(TextElement.BackgroundProperty, args.Background);

      matchInfo.TextRange.ApplyPropertyValue(TextElement.FontFamilyProperty, args.FontFamily);

      matchInfo.TextRange.ApplyPropertyValue(TextElement.FontSizeProperty, args.FontSize);

      matchInfo.TextRange.ApplyPropertyValue(TextElement.FontStretchProperty, args.FontStretch);

      matchInfo.TextRange.ApplyPropertyValue(TextElement.FontStyleProperty, args.FontStyle);

      matchInfo.TextRange.ApplyPropertyValue(TextElement.FontWeightProperty, args.FontWeight);
    }

    #endregion
  }

  /// <summary>
  /// Describes a pair of opening and closing delimiters that encloses text to highlight on match.
  /// </summary>
  public struct DelimiterPair : IEquatable<DelimiterPair>
  {
    public DelimiterPair(string start, string end)
    {
      this.Start = start;
      this.End = end;
    }

    public string Start { get; }
    public string End { get; }

    public override bool Equals(object obj) => obj is DelimiterPair delimiter && Equals(delimiter);

    public override int GetHashCode()
    {
      unchecked
      {
        var hash = (int) 2166136261;

        hash = (16777619 * hash) ^ this.Start.GetHashCode();
        hash = (16777619 * hash) ^ this.End.GetHashCode();
        return hash;
      }
    }

    public static bool operator ==(DelimiterPair left, DelimiterPair right) => left.Equals(right);

    public static bool operator !=(DelimiterPair left, DelimiterPair right) => !(left == right);

    public bool Equals(DelimiterPair other) =>
      other.Start.Equals(this.Start, StringComparison.Ordinal) &&
      other.End.Equals(this.End, StringComparison.Ordinal);
  }

  /// <summary>
  /// Holds formatting attributes that should be applied to the match results.
  /// </summary>
  public struct HighlightArgs : IEquatable<HighlightArgs>
  {
    public Brush Foreground { get; }
    public Brush Background { get; }
    public FontStretch FontStretch { get; }
    public FontStyle FontStyle { get; }
    public FontWeight FontWeight { get; }
    public FontFamily FontFamily { get; }
    public double FontSize { get; }
    public HighlightRichTextBox RichTextBox { get; }

    public HighlightArgs(
      Brush foreground,
      Brush background,
      FontStretch fontStretch,
      FontStyle fontStyle,
      FontWeight fontWeight,
      FontFamily fontFamily,
      double fontSize,
      HighlightRichTextBox richTextBox)
    {
      this.Foreground = foreground;
      this.Background = background;
      this.FontStretch = fontStretch;
      this.FontStyle = fontStyle;
      this.FontWeight = fontWeight;
      this.FontFamily = fontFamily;
      this.FontSize = fontSize;
      this.RichTextBox = richTextBox;
    }

    public override bool Equals(object obj) => obj is HighlightArgs highlightArgs && Equals(highlightArgs);

    public override int GetHashCode()
    {
      unchecked
      {
        var hash = (int) 2166136261;

        hash = (16777619 * hash) ^ this.FontFamily.GetHashCode();
        hash = (16777619 * hash) ^ this.FontStretch.GetHashCode();
        hash = (16777619 * hash) ^ this.FontWeight.GetHashCode();
        hash = (16777619 * hash) ^ this.FontStyle.GetHashCode();
        hash = (16777619 * hash) ^ this.FontSize.GetHashCode();
        hash = (16777619 * hash) ^ this.RichTextBox.GetHashCode();
        hash = (16777619 * hash) ^ this.Foreground.GetHashCode();
        hash = (16777619 * hash) ^ this.Background.GetHashCode();
        return hash;
      }
    }

    public static bool operator ==(HighlightArgs left, HighlightArgs right) => left.Equals(right);

    public static bool operator !=(HighlightArgs left, HighlightArgs right) => !(left == right);

    public bool Equals(HighlightArgs other) =>
      other.FontFamily.Equals(this.FontFamily) &&
      other.FontStretch.Equals(this.FontStretch) &&
      other.FontWeight.Equals(this.FontWeight) &&
      other.FontStyle.Equals(this.FontStyle) &&
      other.FontSize.Equals(this.FontSize) &&
      other.Foreground.Equals(this.Foreground) &&
      other.Background.Equals(this.Background) &&
      other.RichTextBox.Equals(this.RichTextBox);
  }
  
  /// <summary>
  /// Holds formatting attributes that should be applied to the match results.
  /// </summary>
  public sealed class MatchFoundEventArgs : EventArgs
  {
    internal MatchFoundEventArgs(MatchInfo matchInfo, HighlightArgs highlightArgs, HighlightRichTextBox target)
    {
      this.MatchInfo = matchInfo;
      this.FormattingAttributes = highlightArgs;
      this.Target = target;
    }

    public void CancelAutomaticHighlight() => this.Target.IsHighlightCanceled = true;
    public MatchInfo MatchInfo { get; }
    public HighlightArgs FormattingAttributes { get; }
    private HighlightRichTextBox Target { get; }
  }

  public struct MatchInfo : IEquatable<MatchInfo>
  {
    public MatchInfo(int startIndex, int length, string value, TextRange textRange)
    {
      this.StartIndex = startIndex;
      this.Length = length;
      this.Value = value;
      this.TextRange = textRange;
    }

    public int StartIndex { get; }
    public int Length { get; }
    public string Value { get; }
    public TextRange TextRange { get; }

    public override bool Equals(object obj) => obj is MatchInfo matchInfo && Equals(matchInfo);

    public override int GetHashCode()
    {
      unchecked
      {
        var hash = (int) 2166136261;

        hash = (16777619 * hash) ^ this.StartIndex.GetHashCode();
        hash = (16777619 * hash) ^ this.Length.GetHashCode();
        return hash;
      }
    }

    public static bool operator ==(MatchInfo left, MatchInfo right) => left.Equals(right);

    public static bool operator !=(MatchInfo left, MatchInfo right) => !(left == right);

    public bool Equals(MatchInfo other) =>
      other.StartIndex.Equals(this.StartIndex) &&
      other.Length.Equals(this.Length);
  }

  public class HighlightRichTextBox : System.Windows.Controls.RichTextBox
  {
    #region Dependency properties

    public static readonly DependencyProperty IsLiveSearchEnabledProperty = DependencyProperty.Register(
      "IsLiveSearchEnabled",
      typeof(bool),
      typeof(HighlightRichTextBox),
      new PropertyMetadata(default(bool)));

    public bool IsLiveSearchEnabled { get => (bool) GetValue(HighlightRichTextBox.IsLiveSearchEnabledProperty); set => SetValue(HighlightRichTextBox.IsLiveSearchEnabledProperty, value); }

    public static readonly DependencyProperty IsAutoHighlightingEnabledProperty = DependencyProperty.Register(
      "IsAutoHighlightingEnabled",
      typeof(bool),
      typeof(HighlightRichTextBox),
      new PropertyMetadata(default(bool)));

    public bool IsAutoHighlightingEnabled { get => (bool) GetValue(HighlightRichTextBox.IsAutoHighlightingEnabledProperty); set => SetValue(HighlightRichTextBox.IsAutoHighlightingEnabledProperty, value); }

    public static readonly DependencyProperty HighlightForegroundProperty = DependencyProperty.Register(
      "HighlightForeground",
      typeof(Brush),
      typeof(HighlightRichTextBox),
      new PropertyMetadata(default(Brush)));

    public Brush HighlightForeground
    {
      get => (Brush) GetValue(HighlightRichTextBox.HighlightForegroundProperty);
      set => SetValue(HighlightRichTextBox.HighlightForegroundProperty, value);
    }

    public static readonly DependencyProperty HighlightBackgroundProperty = DependencyProperty.Register(
      "HighlightBackground",
      typeof(Brush),
      typeof(HighlightRichTextBox),
      new PropertyMetadata(default(Brush)));

    public Brush HighlightBackground
    {
      get => (Brush) GetValue(HighlightRichTextBox.HighlightBackgroundProperty);
      set => SetValue(HighlightRichTextBox.HighlightBackgroundProperty, value);
    }

    public static readonly DependencyProperty HighlightFontFamilyProperty = DependencyProperty.Register(
      "HighlightFontFamily",
      typeof(FontFamily),
      typeof(HighlightRichTextBox),
      new PropertyMetadata(default(FontFamily)));

    public FontFamily HighlightFontFamily
    {
      get => (FontFamily) GetValue(HighlightRichTextBox.HighlightFontFamilyProperty);
      set => SetValue(HighlightRichTextBox.HighlightFontFamilyProperty, value);
    }

    public static readonly DependencyProperty HighlightFontSizeProperty = DependencyProperty.Register(
      "HighlightFontSize",
      typeof(double),
      typeof(HighlightRichTextBox),
      new PropertyMetadata(default(double)));

    public double HighlightFontSize
    {
      get => (double) GetValue(HighlightRichTextBox.HighlightFontSizeProperty);
      set => SetValue(HighlightRichTextBox.HighlightFontSizeProperty, value);
    }

    public static readonly DependencyProperty HighlightFontStretchProperty = DependencyProperty.Register(
      "HighlightFontStretch",
      typeof(FontStretch),
      typeof(HighlightRichTextBox),
      new PropertyMetadata(default(FontStretch)));

    public FontStretch HighlightFontStretch
    {
      get => (FontStretch) GetValue(HighlightRichTextBox.HighlightFontStretchProperty);
      set => SetValue(HighlightRichTextBox.HighlightFontStretchProperty, value);
    }

    public static readonly DependencyProperty HighlightFontWeightProperty = DependencyProperty.Register(
      "HighlightFontWeight",
      typeof(FontWeight),
      typeof(HighlightRichTextBox),
      new PropertyMetadata(default(FontWeight)));

    public FontWeight HighlightFontWeight
    {
      get => (FontWeight) GetValue(HighlightRichTextBox.HighlightFontWeightProperty);
      set => SetValue(HighlightRichTextBox.HighlightFontWeightProperty, value);
    }

    public static readonly DependencyProperty HighlightFontStyleProperty = DependencyProperty.Register(
      "HighlightFontStyle",
      typeof(FontStyle),
      typeof(HighlightRichTextBox),
      new PropertyMetadata(default(FontStyle)));

    public FontStyle HighlightFontStyle
    {
      get => (FontStyle) GetValue(HighlightRichTextBox.HighlightFontStyleProperty);
      set => SetValue(HighlightRichTextBox.HighlightFontStyleProperty, value);
    }

    public static readonly DependencyProperty MatchFormatterProperty = DependencyProperty.Register(
      "MatchFormatter",
      typeof(IMatchFormatter),
      typeof(HighlightRichTextBox),
      new PropertyMetadata(default(IMatchFormatter)));

    public IMatchFormatter MatchFormatter
    {
      get => (IMatchFormatter) GetValue(HighlightRichTextBox.MatchFormatterProperty);
      set => SetValue(HighlightRichTextBox.MatchFormatterProperty, value);
    }

    public static readonly DependencyProperty TotalMatchCountProperty = DependencyProperty.Register(
      "TotalMatchCount",
      typeof(int),
      typeof(HighlightRichTextBox),
      new PropertyMetadata(default(int)));

    public int TotalMatchCount
    {
      get => (int) GetValue(HighlightRichTextBox.TotalMatchCountProperty);
      set => SetValue(HighlightRichTextBox.TotalMatchCountProperty, value);
    }

    protected static readonly DependencyPropertyKey MatchHistoryPropertyKey = DependencyProperty.RegisterReadOnly(
      "MatchHistory",
      typeof(ObservableCollection<MatchInfo>),
      typeof(HighlightRichTextBox),
      new PropertyMetadata(default(ObservableCollection<MatchInfo>)));

    public static readonly DependencyProperty MatchHistoryProperty =
      HighlightRichTextBox.MatchHistoryPropertyKey.DependencyProperty;

    public ObservableCollection<MatchInfo> MatchHistory
    {
      get => (ObservableCollection<MatchInfo>) GetValue(HighlightRichTextBox.MatchHistoryProperty);
      private set => SetValue(HighlightRichTextBox.MatchHistoryPropertyKey, value);
    }

    public static readonly DependencyProperty StringComparisonProperty = DependencyProperty.Register(
      "StringComparison",
      typeof(StringComparison),
      typeof(HighlightRichTextBox),
      new PropertyMetadata(default(StringComparison)));

    public StringComparison StringComparison
    {
      get => (StringComparison) GetValue(HighlightRichTextBox.StringComparisonProperty);
      set => SetValue(HighlightRichTextBox.StringComparisonProperty, value);
    }

    public static readonly DependencyProperty DelimiterPairsProperty = DependencyProperty.Register(
      "DelimiterPairs",
      typeof(IEnumerable<DelimiterPair>),
      typeof(HighlightRichTextBox),
      new PropertyMetadata(default(IEnumerable<DelimiterPair>), HighlightRichTextBox.OnDelimiterPairsChanged));

    public IEnumerable<DelimiterPair> DelimiterPairs
    {
      get => (IEnumerable<DelimiterPair>) GetValue(HighlightRichTextBox.DelimiterPairsProperty);
      set => SetValue(HighlightRichTextBox.DelimiterPairsProperty, value);
    }

    public static readonly DependencyProperty SearchKeysProperty = DependencyProperty.Register(
      "SearchKeys",
      typeof(IEnumerable<string>),
      typeof(HighlightRichTextBox),
      new PropertyMetadata(default(IEnumerable<string>)));

    public IEnumerable<string> SearchKeys
    {
      get => (IEnumerable<string>) GetValue(HighlightRichTextBox.SearchKeysProperty);
      set => SetValue(HighlightRichTextBox.SearchKeysProperty, value);
    }

    #endregion Dependency properties

    // TODO::Add alternative Regex search
    // TODO::Add cancellation to public Search
    // TODO::Add search RoutedCommand handling (find next/find all search)
    // TODO::Add result navigation
    public HighlightRichTextBox()
    {
      this.MatchHistory = new ObservableCollection<MatchInfo>();
      DataObject.AddPastingHandler(this, AnalyzeClipBoardOnPaste);
      this.HighlightBackground = Brushes.Gray;
      this.HighlightForeground = Brushes.Black;
      this.StringComparison = StringComparison.OrdinalIgnoreCase;
      this.LastLiveInputMatchPosition = -1;
      this.FallBackMatchFormatter = new DefaultMatchFormatter();
      this.IsLiveSearchEnabled = true;
      this.IsAutoHighlightingEnabled = true;
    }

    public void Search()
    {
      _ = EnumerateSearch().ToList();
    }

    public void Search(int documentOffset)
    {
      _ = EnumerateSearch(documentOffset).ToList();
    }

    public void Search(int documentOffset, IEnumerable<DelimiterPair> delimiters)
    {
      _ = EnumerateSearch(documentOffset, delimiters).ToList();
    }

    public void Search(int documentOffset, IEnumerable<string> searchKeys)
    {
      _ = EnumerateSearch(documentOffset, searchKeys).ToList();
    }

    public IEnumerable<MatchInfo> EnumerateSearch()
    {
      foreach (MatchInfo matchInfo in EnumerateSearch(0))
      {
        yield return matchInfo;
      }
    }

    public IEnumerable<MatchInfo> EnumerateSearch(int documentOffset)
    {
      string content = new TextRange(this.Document.ContentStart, this.Document.ContentEnd).Text;
      if (documentOffset < 0 || documentOffset > content.Length)
      {
        throw new ArgumentException("Invalid offset", nameof(documentOffset));
      }

      try
      {
        this.IsInputHandled = true;
        this.TotalMatchCount = 0;
        this.MatchHistory.Clear();
        var contentIndex = CreateLookupIndex(content, documentOffset);
        foreach (MatchInfo matchInfo in FindMatchesInContent(content, contentIndex, this.DelimiterPairs, this.SearchKeys))
        {
          yield return matchInfo;
        }
      }
      finally
      {
        this.IsInputHandled = false;
      }
    }

    public IEnumerable<MatchInfo> EnumerateSearch(int documentOffset, IEnumerable<DelimiterPair> delimiters)
    {
      string content = new TextRange(this.Document.ContentStart, this.Document.ContentEnd).Text;
      if (documentOffset < 0 || documentOffset > content.Length)
      {
        throw new ArgumentException("Invalid offset", nameof(documentOffset));
      }

      if (delimiters == null)
      {
        throw new ArgumentNullException(nameof(delimiters));
      }

      try
      {
        this.IsInputHandled = true;
        this.TotalMatchCount = 0;
        this.MatchHistory.Clear();
        var contentIndex = CreateLookupIndex(content, documentOffset);
        foreach (MatchInfo matchInfo in FindDelimitedToken(content, contentIndex, delimiters))
        {
          HighlightMatch(matchInfo);
          UpdateMatchHistory(matchInfo);
          this.TotalMatchCount++;
          yield return matchInfo;
        }
      }
      finally
      {
        this.IsInputHandled = false;
      }
    }

    public IEnumerable<MatchInfo> EnumerateSearch(int documentOffset, IEnumerable<string> searchKeys)
    {
      string content = new TextRange(this.Document.ContentStart, this.Document.ContentEnd).Text;
      if (documentOffset < 0 || documentOffset > content.Length)
      {
        throw new ArgumentException("Invalid offset", nameof(documentOffset));
      }

      if (searchKeys == null)
      {
        throw new ArgumentNullException(nameof(searchKeys));
      }

      try
      {
        this.IsInputHandled = true;
        this.TotalMatchCount = 0;
        this.MatchHistory.Clear();
        var contentIndex = CreateLookupIndex(content, documentOffset);
        foreach (MatchInfo matchInfo in FindSearchKeys(content, contentIndex, searchKeys))
        {
          HighlightMatch(matchInfo);
          UpdateMatchHistory(matchInfo);
          this.TotalMatchCount++;
          yield return matchInfo;
        }
      }
      finally
      {
        this.IsInputHandled = false;
      }
    }

    #region Overrides of TextBoxBase

    protected override void OnTextChanged(TextChangedEventArgs e)
    {
      base.OnTextChanged(e);
      if (this.IsInputHandled || !this.IsLiveSearchEnabled)
      {
        return;
      }
      
      if (this.IsPasteActive)
      {
        HandlePasteInput(e);
      }
      else
      {
        HandleLiveInput(e);
      }
    }

    #endregion Overrides of TextBoxBase

    private static void OnDelimiterPairsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as HighlightRichTextBox).OnDelimiterPairsChanged(e.NewValue as IEnumerable<DelimiterPair>, e.OldValue as IEnumerable<DelimiterPair>);
    }

    protected virtual void OnDelimiterPairsChanged(IEnumerable<DelimiterPair> newValue, IEnumerable<DelimiterPair> oldValue)
    {
      ListenToCollectionChanged(newValue, oldValue);
      if (this.IsLiveSearchEnabled)
      {
        Search();
      }
    }

    private void ListenToCollectionChanged(IEnumerable<DelimiterPair> newValue, IEnumerable<DelimiterPair> oldValue)
    {
      if (oldValue is INotifyCollectionChanged oldObservableCollection)
      {
        oldObservableCollection.CollectionChanged -= HandleSearchKeyCollectionsChanged;
      }
      if (newValue is INotifyCollectionChanged newObservableCollection)
      {
        newObservableCollection.CollectionChanged += HandleSearchKeyCollectionsChanged;
      }
    }

    private void HandleSearchKeyCollectionsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (this.IsLiveSearchEnabled && e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
      {
        Search(0, e.NewItems.OfType<DelimiterPair>());
        Search(0, e.NewItems.OfType<string>());
      }
      if (this.IsLiveSearchEnabled && e.Action == NotifyCollectionChangedAction.Remove)
      {
        Search();
      }
    }

    private void AnalyzeClipBoardOnPaste(object sender, DataObjectPastingEventArgs e)
    {
      // If clipboard content is not of type string, don't handle it
      this.IsPasteActive = e.DataObject.GetDataPresent(typeof(string));
    }

    private void HandlePasteInput(TextChangedEventArgs e)
    {
      try
      {
        this.IsInputHandled = true;
        TextChange changeInfo = e.Changes.ToList().OrderBy(change => change.AddedLength).Last();

        string pastedInputWithoutLineFeed = new TextRange(
          this.Document.ContentStart.GetPositionAtOffset(changeInfo.Offset, LogicalDirection.Forward)
            .GetInsertionPosition(LogicalDirection.Forward),
          this.Document.ContentStart.GetPositionAtOffset(
            changeInfo.Offset + changeInfo.AddedLength,
            LogicalDirection.Forward)).Text.Replace(Environment.NewLine, "");
        Dictionary<int, (TextPointer RunPointer, string RunText)> pastedContentIndex = CreateLookupIndex(
          pastedInputWithoutLineFeed,
          changeInfo.Offset);
        if (!pastedContentIndex.Any())
        {
          return;
        }

        _ = FindMatchesInContent(
          pastedInputWithoutLineFeed,
          pastedContentIndex,
          this.DelimiterPairs ?? new List<DelimiterPair>(),
          this.SearchKeys ?? new List<string>()).ToList();
      }
      finally
      {
        this.IsPasteActive = false;
        this.IsInputHandled = false;
      }
    }

    private Dictionary<int, (TextPointer RunPointer, string RunText)> CreateLookupIndex(
      string input,
      int offset)
    {
      var pastedContentDocumentRunMap = new Dictionary<int, (TextPointer RunPointer, string RunText)>();

      TextPointer currentDocumentPosition =
        this.Document.ContentStart.GetPositionAtOffset(offset, LogicalDirection.Forward);
      TextPointerContext currentDocumentPositionContext =
        currentDocumentPosition.GetPointerContext(LogicalDirection.Forward);

      var currentInputIndex = 0;
      while (currentInputIndex < input.Length)
      {
        while (currentDocumentPositionContext != TextPointerContext.Text)
        {
          MoveToNextDocumentContextPosition(ref currentDocumentPosition, out currentDocumentPositionContext,
            LogicalDirection.Forward);
          if (currentDocumentPosition == null)
          {
            return pastedContentDocumentRunMap;
          }
        }

        CreateIndexForDocumentRun(input, currentDocumentPosition, pastedContentDocumentRunMap, ref currentInputIndex);

        MoveToNextDocumentContextPosition(ref currentDocumentPosition, out currentDocumentPositionContext);
        if (currentDocumentPosition == null)
        {
          return pastedContentDocumentRunMap;
        }
      }

      return pastedContentDocumentRunMap;
    }

    private Dictionary<int, (TextPointer RunPointer, string RunText)> CreateReverseLookupIndex(
      int requestedTextLength,
      int documentOffset)
    {
      var pastedContentDocumentRunMap = new Dictionary<int, (TextPointer RunPointer, string RunText)>();

      TextPointer currentDocumentPosition =
        this.Document.ContentStart.GetPositionAtOffset(documentOffset).GetInsertionPosition(LogicalDirection.Forward);
      TextPointerContext currentDocumentPositionContext =
        currentDocumentPosition.GetPointerContext(LogicalDirection.Forward);

      while (requestedTextLength > 0)
      {
        while (currentDocumentPositionContext != TextPointerContext.Text)
        {
          MoveToNextDocumentContextPosition(ref currentDocumentPosition, out currentDocumentPositionContext,
            LogicalDirection.Backward);
          if (currentDocumentPosition == null)
          {
            return pastedContentDocumentRunMap;
          }
        }

        CreateReverseIndexForDocumentRun(currentDocumentPosition, pastedContentDocumentRunMap, ref requestedTextLength);

        MoveToNextDocumentContextPosition(ref currentDocumentPosition, out currentDocumentPositionContext, LogicalDirection.Backward);
        if (currentDocumentPosition == null)
        {
          return pastedContentDocumentRunMap;
        }
      }

      return pastedContentDocumentRunMap;
    }

    private void CreateIndexForDocumentRun(
      string input,
      TextPointer currentDocumentPosition,
      Dictionary<int, (TextPointer RunPointer, string RunText)> pastedContentDocumentRunMap,
      ref int currentInputIndex)
    {
      string textInRun = currentDocumentPosition.GetTextInRun(LogicalDirection.Forward);
      currentInputIndex = input.IndexOf(textInRun, currentInputIndex, StringComparison.Ordinal);
      if (currentInputIndex > -1)
      {
        pastedContentDocumentRunMap.Add(currentInputIndex, (currentDocumentPosition, textInRun));
        currentInputIndex += textInRun.Length;
      }
    }

    private void CreateReverseIndexForDocumentRun(
      TextPointer currentDocumentPosition,
      Dictionary<int, (TextPointer RunPointer, string RunText)> pastedContentDocumentRunMap,
      ref int requestedRunLength)
    {
      string textInRun = currentDocumentPosition.GetTextInRun(LogicalDirection.Forward);
      pastedContentDocumentRunMap.Add(pastedContentDocumentRunMap.Count, (currentDocumentPosition, textInRun));
      requestedRunLength -= textInRun.Length;
    }

    private void MoveToNextDocumentContextPosition(
      ref TextPointer documentPosition,
      out TextPointerContext documentPositionContext,
      LogicalDirection logicalDirection = LogicalDirection.Forward)
    {
      documentPosition = documentPosition.GetNextContextPosition(logicalDirection);

      documentPositionContext =
        documentPosition?.GetPointerContext(logicalDirection) ?? TextPointerContext.None;
    }

    private IEnumerable<MatchInfo> FindMatchesInContent(
      string content,
      Dictionary<int, (TextPointer RunPointer, string RunText)> contentIndex, IEnumerable<DelimiterPair> delimiters, IEnumerable<string> searchKeys)
    {
      BackupOriginalFormatting();
      foreach (MatchInfo matchInfo in FindDelimitedToken(content, contentIndex, delimiters))
      {
        HighlightMatch(matchInfo);
        UpdateMatchHistory(matchInfo);
        this.TotalMatchCount++;
        yield return matchInfo;
      }
      foreach (MatchInfo matchInfo in FindSearchKeys(content, contentIndex, searchKeys))
      {
        HighlightMatch(matchInfo);
        UpdateMatchHistory(matchInfo);
        this.TotalMatchCount++;
        yield return matchInfo;
      }
    }

    private IEnumerable<MatchInfo> FindDelimitedToken(
      string input,
      Dictionary<int, (TextPointer RunPointer, string RunText)> pastedContentIndex,
      IEnumerable<DelimiterPair> delimiterPairs)
    {
      foreach (DelimiterPair delimiterPair in delimiterPairs)
      {
        var inputIndex = 0;
        TextPointer matchStart = null;
        while (inputIndex < input.Length)
        {
          int matchIndex;
          if (matchStart == null)
          {
            matchIndex = input.IndexOf(delimiterPair.Start, inputIndex, this.StringComparison);
            if (matchIndex < 0)
            {
              break;
            }

            matchStart = CreateMatchStartPointer(pastedContentIndex, matchIndex, delimiterPair.Start);
            inputIndex = matchIndex + delimiterPair.Start.Length;
            continue;
          }

          matchIndex = input.IndexOf(delimiterPair.End, inputIndex, this.StringComparison);
          if (matchIndex < 0)
          {
            break;
          }

          TextPointer matchEnd = CreateMatchEndPointer(pastedContentIndex, matchIndex, delimiterPair.End);

          var match = new TextRange(matchStart, matchEnd);
          yield return new MatchInfo(matchIndex - (match.Text.Length - 1), match.Text.Length, match.Text, match);
          inputIndex = matchIndex + delimiterPair.End.Length;
          matchStart = null;
        }
      }
    }

    private IEnumerable<MatchInfo> FindSearchKeys(
      string input,
      Dictionary<int, (TextPointer RunPointer, string RunText)> pastedContentIndex,
      IEnumerable<string> searchKeys)
    {
      foreach (string searchKey in searchKeys)
      {
        var inputIndex = 0;
        while (inputIndex < input.Length)
        {
          int
            matchIndex = input.IndexOf(searchKey, inputIndex, this.StringComparison);
          if (matchIndex < 0)
          {
            break;
          }

          TextRange match = CreateFullMatch(pastedContentIndex, matchIndex, searchKey);
            yield return new MatchInfo(matchIndex - (match.Text.Length - 1), match.Text.Length, match.Text, match);
            inputIndex = matchIndex + searchKey.Length;
        }
      }
    }

    private TextRange CreateFullMatch
    (
      Dictionary<int, (TextPointer RunPointer, string RunText)> pastedContentIndex,
      int inputIndex,
      string searchKey)
    {
      KeyValuePair<int, (TextPointer RunPointer, string RunText)> startEntry =
        pastedContentIndex.Last(kvp => kvp.Key <= inputIndex);
      int runStartIndex = startEntry.Key;
      int relativeRunIndex = inputIndex - runStartIndex;
      KeyValuePair<int, (TextPointer RunPointer, string RunText)> endEntry = startEntry;
      int remainingSearchKeyLength = searchKey.Length - (startEntry.Value.RunText.Length - relativeRunIndex);
      while (remainingSearchKeyLength > 0)
      {
        endEntry =
          pastedContentIndex.First(kvp => kvp.Key > endEntry.Key);
        if (remainingSearchKeyLength - endEntry.Value.RunText.Length < 0)
        {
          break;
        }
        remainingSearchKeyLength -= endEntry.Value.RunText.Length;
      }

      int matchEndOffset = remainingSearchKeyLength <= 0
        ? relativeRunIndex + searchKey.Length
        : remainingSearchKeyLength;

      TextPointer matchStart = startEntry.Value.RunPointer.GetPositionAtOffset(relativeRunIndex,
        LogicalDirection.Forward);

      TextPointer matchEnd = endEntry.Value.RunPointer.GetPositionAtOffset(matchEndOffset, LogicalDirection.Forward);

      return new TextRange(matchStart, matchEnd);
    }

    private TextPointer CreateMatchStartPointer(
      Dictionary<int, (TextPointer RunPointer, string RunText)> pastedContentIndex,
      int inputIndex,
      string openingDelimiter)
    {
      KeyValuePair<int, (TextPointer RunPointer, string RunText)> entry =
        pastedContentIndex.Last(kvp => kvp.Key <= inputIndex);
      int runStartIndex = entry.Key;
      int relativeRunIndex = inputIndex - runStartIndex;
      TextPointer matchStart = entry.Value.RunPointer.GetPositionAtOffset(relativeRunIndex,
        LogicalDirection.Forward);
      return matchStart;
    }

    private TextPointer CreateMatchEndPointer(
      Dictionary<int, (TextPointer RunPointer, string RunText)> pastedContentIndex,
      int inputIndex,
      string closingDelimiter)
    {
      KeyValuePair<int, (TextPointer RunPointer, string RunText)> entry =
        pastedContentIndex.Last(kvp => kvp.Key <= inputIndex);
      int runStartIndex = entry.Key;
      int relativeRunIndex = inputIndex - runStartIndex;
      TextPointer matchEnd = entry.Value.RunPointer
        .GetPositionAtOffset(relativeRunIndex,
          LogicalDirection.Forward)
        .GetNextInsertionPosition(LogicalDirection.Forward);
      return matchEnd;
    }

    private void HandleLiveInput(TextChangedEventArgs e)
    {
      this.IsInputHandled = true;
      int changeOffset = e.Changes.ToList().Last().Offset;
      string input = this.Document.ContentStart.GetPositionAtOffset(changeOffset, LogicalDirection.Forward)
        .GetInsertionPosition(LogicalDirection.Forward).GetTextInRun(LogicalDirection.Forward);
      if (TryFindMatch(input, changeOffset, out List<MatchInfo> matchInfos))
      {
        foreach (MatchInfo matchInfo in matchInfos)
        {
          BackupOriginalFormatting();
          HighlightMatch(matchInfo);
          UpdateMatchHistory(matchInfo);
          this.TotalMatchCount++;
        }
      }

      this.IsInputHandled = false;
    }

    // TODO::Step back for delimiter length (changeOffset - delimiterPair.Start.Length) to match input
    public Dictionary<string, int> StartMatchOffsets { get; set; } = new Dictionary<string, int>();
    private bool TryFindMatch(string input, int changeOffset, out List<MatchInfo> matchInfos)
    {
      matchInfos = new List<MatchInfo>();
      if (string.IsNullOrWhiteSpace(input))
      {
        return false;
      }
      DelimiterPair maxStartLengthDelimiter = this.DelimiterPairs.Aggregate((longestStartDelimiter, nextDelimiterPair) => nextDelimiterPair.Start.Length > longestStartDelimiter.Start.Length ? nextDelimiterPair : longestStartDelimiter);

      DelimiterPair maxEndLengthDelimiter = this.DelimiterPairs.Aggregate((longestStartDelimiter, nextDelimiterPair) => nextDelimiterPair.Start.Length > longestStartDelimiter.Start.Length ? nextDelimiterPair : longestStartDelimiter);

      int maxDelimiterLength = Math.Max(maxStartLengthDelimiter.Start.Length, maxEndLengthDelimiter.End.Length);
      
      var docIndex = CreateReverseLookupIndex(maxDelimiterLength, changeOffset);
      
      foreach (DelimiterPair delimiterPair in this.DelimiterPairs)
      {
        var requiredRuns = docIndex.Select(entry => entry.Value.RunText).Aggregate((result, nextRunText) => result.Length < maxDelimiterLength ? result + nextRunText : result);

        if (this.StartMatchOffsets.TryGetValue(delimiterPair.Start, out int delimiterStartMatchIndex))
        {
          if (requiredRuns.Length - delimiterPair.End.Length < 0)
          {
            continue;
          }
          int endDelimiterMatchIndex = requiredRuns.LastIndexOf(
            delimiterPair.End,
            requiredRuns.Length - delimiterPair.End.Length,
            this.StringComparison);
          if (endDelimiterMatchIndex < 0)
          {
            continue;
          }
          var match = new TextRange(
            this.Document.ContentStart.GetPositionAtOffset(delimiterStartMatchIndex)
              .GetInsertionPosition(LogicalDirection.Forward),
            this.Document.ContentStart.GetPositionAtOffset(changeOffset, LogicalDirection.Forward)
              .GetNextInsertionPosition(LogicalDirection.Forward));
          matchInfos.Add(new MatchInfo(this.LastLiveInputMatchPosition, match.Text.Length, match.Text, match));
          this.StartMatchOffsets.Remove(delimiterPair.Start);
          continue;
        }

        if (requiredRuns.Length - delimiterPair.Start.Length < 0)
        {
          continue;
        }
        int matchIndex = requiredRuns.LastIndexOf(
          delimiterPair.Start,
          requiredRuns.Length - delimiterPair.Start.Length,
          this.StringComparison);
        if (matchIndex < 0)
        {
          continue;
        }
        this.StartMatchOffsets.Add(delimiterPair.Start, matchIndex);
      }
      //if (this.LastLiveInputMatchPosition < 0 && this.DelimiterPairs.Any(
      //  delimiterPair => input.StartsWith(delimiterPair.Start, this.StringComparison)))
      //{
      //  this.LastLiveInputMatchPosition = changeOffset;
      //  return false;
      //}

      //if (this.LastLiveInputMatchPosition > -1 && this.DelimiterPairs.Any(
      //  delimiterPair => input.StartsWith(delimiterPair.End, this.StringComparison)))
      //{
      //  var match = new TextRange(
      //    this.Document.ContentStart.GetPositionAtOffset(this.LastLiveInputMatchPosition)
      //      .GetInsertionPosition(LogicalDirection.Forward),
      //    this.Document.ContentStart.GetPositionAtOffset(changeOffset, LogicalDirection.Forward)
      //      .GetNextInsertionPosition(LogicalDirection.Forward));
      //  matchInfo = new MatchInfo(this.LastLiveInputMatchPosition, match.TextValue.Length, match.TextValue, match);
      //  this.LastLiveInputMatchPosition = -1;
      //  return true;
      //}

      return matchInfos.Any();
    }

    private void UpdateMatchHistory(MatchInfo matchInfo)
    {
      this.MatchHistory.Add(matchInfo);
    }

    private void HighlightMatch(MatchInfo matchInfo)
    {
      var highlightArgs = new HighlightArgs(this.HighlightForeground ?? this.Foreground, this.HighlightBackground ?? this.Background, this.HighlightFontStretch, this.HighlightFontStyle, this.HighlightFontWeight, this.HighlightFontFamily ?? this.FontFamily, this.HighlightFontSize.Equals(0) ? this.FontSize : this.HighlightFontSize, this);

      OnMatchFound(matchInfo, highlightArgs);

      if (this.IsHighlightCanceled || !this.IsAutoHighlightingEnabled)
      {
        RestoreOriginalFormattingForNonMatchingText();
        return;
      }

      if (this.MatchFormatter != null)
      {
        this.MatchFormatter.FormatMatch(matchInfo, highlightArgs);
      }
      else
      {
        this.FallBackMatchFormatter.FormatMatch(matchInfo, highlightArgs);
      }
      RestoreOriginalFormattingForNonMatchingText();
    }

    private void BackupOriginalFormatting()
    {
      this.OriginalTextBackground = this.Background;
      this.OriginalTextForeground = this.Foreground;
      this.OriginalFontStyle = this.FontStyle;
      this.OriginalFontSize = this.FontSize;
      this.OriginalFontWeight = this.FontWeight;
      this.OriginalFontFamily = this.FontFamily;
      this.OriginalFontStretch = this.FontStretch;
    }

    private void RestoreOriginalFormattingForNonMatchingText()
    {
      var newRun = new Run(string.Empty, this.CaretPosition)
      {
        Foreground = this.OriginalTextForeground,
        Background = this.OriginalTextBackground,
        FontStyle = this.OriginalFontStyle,
        FontSize = this.OriginalFontSize,
        FontWeight = this.OriginalFontWeight,
        FontFamily = this.OriginalFontFamily,
        FontStretch = this.OriginalFontStretch
      };
      this.CaretPosition.Paragraph.Inlines.Add(newRun);
      this.CaretPosition = newRun.ContentEnd;
      //if (this.Selection.IsEmpty)
      //{
      //  var newRun = new Run(string.Empty, this.CaretPosition)
      //    {Foreground = this.OriginalTextForeground, Background = this.OriginalTextBackground};
      //  this.CaretPosition.Paragraph.Inlines.Add(newRun);
      //  this.CaretPosition = newRun.ContentEnd;
      //}
      //else
      //{
      //  this.Selection.ApplyPropertyValue(Control.ForegroundProperty, this.OriginalTextForeground);
      //  this.Selection.ApplyPropertyValue(Control.BackgroundProperty, this.OriginalTextBackground);
      //}
    }

    protected virtual void OnMatchFound(MatchInfo matchInfo, HighlightArgs highlightArgs)
    {
      this.MatchFound?.Invoke(this, new MatchFoundEventArgs(matchInfo, highlightArgs, this));
    }

    public event EventHandler<MatchFoundEventArgs> MatchFound;
    internal bool IsHighlightCanceled { get; set; }
    private IMatchFormatter FallBackMatchFormatter { get; }
    private int LastLiveInputMatchPosition { get; set; }
    private bool IsPasteActive { get; set; }
    private bool IsInputHandled { get; set; }
    private Brush OriginalTextForeground { get; set; }
    private Brush OriginalTextBackground { get; set; }
    private FontStretch OriginalFontStretch { get; set; }
    private FontFamily OriginalFontFamily { get; set; }
    private FontWeight OriginalFontWeight { get; set; }
    private double OriginalFontSize { get; set; }
    private FontStyle OriginalFontStyle { get; set; }
  }
}