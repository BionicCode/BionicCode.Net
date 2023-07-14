namespace BionicCode.Controls.Net.Wpf
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Linq;
  using System.Threading.Tasks;
  using System.Windows;
  using System.Windows.Media;
  using BionicCode.Utilities.Net;

  /// <summary>
  /// Manages the text content that is renderd by <see cref="TextSite"/>.
  /// <para>
  /// Primary purpose of this type is provide a text engine that is highly optimized for code editing and code highlighting. 
  /// <br/> Code editing means highlight support (for key words), text selection and general highlighting of text (e.g. text matching).
  /// <br/>This class is not intenden to support full (rich) text document editig like apllyintg different fonts or font sizes.
  /// </para>
  /// </summary>
  /// <remarks>Following assumptions (constraints) were made:
  /// <para>
  /// <list type="bullet">
  /// <item>Homogeneous font size</item>
  /// <item>Homogeneous font family</item>
  /// <item>Homogeneous font style</item>
  /// <item>homogeneous spacing (based on the font family or font</item>
  /// <item>No italics</item>
  /// <item>No text wrapping</item>
  /// </list>
  /// </para>
  /// </remarks>
  internal class TextEngine
  {
    public TextEngine()
    {
      this.TextSite = new TextSite();
      this.TextSite.TextClicked += OnTextClicked;
      this.PixelsPerDip = VisualTreeHelper.GetDpi(this.TextSite).PixelsPerDip;
      this.CaretWidth = SystemParameters.CaretWidth;
      //this.CaretPositionToIndexMap = new Dictionary<Point, int>();
      this.LineCaretPositionToIndexMapTable = new Dictionary<int, Dictionary<Point, int>>();
    }

    public async Task InitializeAsync(TextInfo textInfo) => await InitializeTextMetaInfoAsync(textInfo);

    public async Task ShowTextAsync(TextInfo textInfo, Point offset)
    {
      await InitializeTextMetaInfoAsync(textInfo);

      TextRenderInfo renderInfo = CreateTextRenderInfo(this.TextContainer.TextInfo.TextRanges);
      this.TextSite.RenderText(renderInfo, offset);
    }

    public async Task RemoveTextAsync(int lineIndex, Range characterRange)
    {
      TextLine currentLine = await this.TextContainer.GetTextLineAsync(lineIndex);

      currentLine.RemoveTextRange(characterRange);
      TextRenderInfo renderInfo = CreateTextRenderInfo(this.TextContainer.TextInfo.TextRanges);
      this.TextSite.RenderText(renderInfo, new Point());
    }

    public async Task RemoveTextAtCurrentCaretPositionAsync(int characterCount, TextDirecetion textDirecetion)
    {
      CaretInfo caretInfo = this.TextSite.CaretInfo;
      TextLine currentLine = await GetLineFromPointAsync(caretInfo.Position);
      FormattedText formattedTextOfLine = CreateFormattedTextFrom(currentLine.TextInfo);
      await ApplyTextRangesToFormattedTextAsync(formattedTextOfLine, currentLine.TextRanges);
      int currentCaretIndex = await GetCaretIndexFromPointAsync(currentLine, formattedTextOfLine, caretInfo.Position);
      int startIndex;
      int endIndex;
      if (textDirecetion == TextDirecetion.Right)
      {
        startIndex = currentCaretIndex;
        endIndex = currentCaretIndex + characterCount;
      }
      else
      {
        startIndex = currentCaretIndex - characterCount;
        endIndex = currentCaretIndex;
      }

      currentLine.RemoveTextRange(startIndex..endIndex);
      TextRenderInfo renderInfo = CreateTextRenderInfo(this.TextContainer.TextInfo.TextRanges);
      this.TextSite.RenderText(renderInfo, new Point());
    }

    public async Task MoveCaretToAsync(int index, Point offset)
    {
      CaretInfo caretInfo = await CreateCaretInfoFromIndexAsync(index, offset);
      this.TextSite.SetCaretPosition(caretInfo);
    }

    private void MoveCaretTo(Point caretPosition, Point offset)
    {
      CaretInfo caretInfo = CreateCaretInfoFromPoint(caretPosition, offset);
      this.TextSite.SetCaretPosition(caretInfo);
    }

    private async void OnTextClicked(object sender, TextClickedEventArgs e)
    {
      TextLine clickedLine = await GetLineFromPointAsync(e.ClickPosition);
      FormattedText formattedTextOfLine = CreateFormattedTextFrom(clickedLine.TextInfo);
      await ApplyTextRangesToFormattedTextAsync(formattedTextOfLine, clickedLine.TextRanges);
      Point alignedClickPosition = AlignClickPositionWithText(clickedLine.VerticalOffset, formattedTextOfLine, e.ClickPosition);
      MoveCaretTo(alignedClickPosition, new Point());
      int newCaretIndex = await GetCaretIndexFromPointAsync(clickedLine, formattedTextOfLine, alignedClickPosition);
      OnCaretIndexChanged(newCaretIndex);
    }

    private async Task<int> GetCaretIndexFromPointAsync(TextLine textLine, FormattedText formattedLineText, Point caretPosition)
    {
      var currentPosition = new Point();
      if (!this.LineCaretPositionToIndexMapTable.TryGetValue(textLine.Index, out Dictionary<Point, int> caretPositionToIndexMap))
      {
        caretPositionToIndexMap = new Dictionary<Point, int>();
        this.LineCaretPositionToIndexMapTable.Add(textLine.Index, caretPositionToIndexMap);
      }

      if (caretPositionToIndexMap.TryGetValue(caretPosition, out int index))
      {
        return index;
      }

      Point lastCachedCaretPositionBeforeCurrent = caretPositionToIndexMap.Keys
        .OrderBy(position => position.X)
        .LastOrDefault(point => point.X <= caretPosition.X);
      currentPosition = lastCachedCaretPositionBeforeCurrent;
      if (caretPositionToIndexMap.TryGetValue(lastCachedCaretPositionBeforeCurrent, out int lastCachedCaretIndexBeforeCurrent))
      {
        //string remainingLineTextSegment = formattedLineText.Text.Substring(lastCachedCaretIndexBeforeCurrent);
        //IEnumerable<TextRange> textRangesOfRemainingLineSegment = await textLine.TextRanges.GetTextRangesFromRangeAsync(lastCachedCaretIndexBeforeCurrent..);
        int rangeStartIndex = textLine.DoucmentIndex + lastCachedCaretIndexBeforeCurrent;
        TextInfo textInfoOfRemainingLineTextSegment = await CreateTextInfoFromDocumentRangeAsync(rangeStartIndex..textLine.RangeInDocument.End);
        formattedLineText = CreateFormattedTextFrom(textInfoOfRemainingLineTextSegment);
        await ApplyTextRangesToFormattedTextAsync(formattedLineText, textInfoOfRemainingLineTextSegment.TextRanges);
        index = lastCachedCaretIndexBeforeCurrent;
      }

      var hitTestGeometry = new RectangleGeometry(new Rect(currentPosition, new Size(0.1, formattedLineText.Height)));
      Geometry textGeometry = formattedLineText.BuildGeometry(currentPosition);

      IntersectionDetail intersectionDetail = textGeometry.FillContainsWithDetail(hitTestGeometry);
      currentPosition = SeekIntersectionDetail(IntersectionDetail.Intersects, currentPosition, caretPosition, textGeometry, FlowDirection.LeftToRight);

      while (currentPosition.X < caretPosition.X)
      {
        currentPosition = SeekIntersectionDetail(IntersectionDetail.Empty, currentPosition, caretPosition, textGeometry, FlowDirection.LeftToRight);
        index++;
        currentPosition = SeekIntersectionDetail(IntersectionDetail.Intersects, currentPosition, caretPosition, textGeometry, FlowDirection.LeftToRight);
      }

      caretPositionToIndexMap.Add(caretPosition, index);
      return index;
    }

    private async Task<TextInfo> CreateTextInfoFromDocumentRangeAsync(Range documentRange)
    {
      IEnumerable<TextRange> textRanges = await this.TextContainer.TextInfo.TextRanges.GetTextRangesFromRangeAsync(documentRange);
      string textInRange = this.TextContainer.TextInfo.Text[documentRange];
      return new TextInfo(textInRange, textRanges);
    }

    private static Point SeekIntersectionDetail(IntersectionDetail intersectionDetail, Point startPosition, Point stopPosition, Geometry geometryToTest, FlowDirection direction)
    {
      Func<bool> pointCondition = () => direction == FlowDirection.LeftToRight
        ? startPosition.X < stopPosition.X
        : startPosition.X > stopPosition.X;
      double incrementValue = direction == FlowDirection.LeftToRight ? 0.1 : -0.1;
      var hitTestGeometry = new RectangleGeometry(new Rect(startPosition, new Size(0.1, geometryToTest.Bounds.Height)));
      IntersectionDetail currentIntersectionDetail = geometryToTest.FillContainsWithDetail(hitTestGeometry);
      while (currentIntersectionDetail != intersectionDetail
        && pointCondition.Invoke())
      {
        startPosition.Offset(incrementValue, 0);
        hitTestGeometry = new RectangleGeometry(new Rect(startPosition, new Size(0.1, geometryToTest.Bounds.Height)));
        currentIntersectionDetail = geometryToTest.FillContainsWithDetail(hitTestGeometry);
      }

      return startPosition;
    }

    private async Task<TextLine> GetLineFromPointAsync(Point clickPosition)
    {
      int lineIndex = GetLineIndexFromPosition(clickPosition);
      TextLine clickedLine = await this.TextContainer.GetTextLineAsync(lineIndex);
      return clickedLine;
    }

    private int GetLineIndexFromPosition(Point clickPosition)
    {
      double lineHeight = GetLineHeight();
      return (int)Math.Truncate(clickPosition.Y / lineHeight);
    }

    private double GetLineHeight()
    {
      // change Text height based on line size
      FontFamily fontFamily = this.TextContainer.TextInfo.FallbackFontFamily;
      double fontSize = this.TextContainer.TextInfo.FallbackFontSize;

      // If Ps Task 25254 is completed (not likely in V1), LineStackingStrategy
      // won't be constant and we'll need to call some sort of CalcLineAdvance method.
      double lineHeight;

      //if (TextOptions.GetTextFormattingMode(this.TextContainer) == TextFormattingMode.Ideal)
      {
        lineHeight = fontFamily.LineSpacing * fontSize;
      }
      //else
      //{
      //  lineHeight = fontFamily.GetLineSpacingForDisplayMode(fontSize, GetDpi().DpiScaleY);
      //}

      return Math.Max(0.1, lineHeight);
    }

    private Point AlignClickPositionWithText(double clickedLineVerticalOffset, FormattedText formattedLineText, Point clickPosition)
    {
      var coercedClickPosition = new Point(clickPosition.X, clickedLineVerticalOffset);
      _ = this.TextSite.CaretInfo;
      var hitTestCursorGeometry = new RectangleGeometry(new Rect(coercedClickPosition, new Size(0.1, formattedLineText.Height)));
      Geometry textGeometry = formattedLineText.BuildGeometry(new Point());

      var clickPointGeometry = new EllipseGeometry(clickPosition, 4, 4);
      var textBox = new RectangleGeometry(formattedLineText.BuildGeometry(new Point()).Bounds);
      var gg = new GeometryGroup();
      gg.Children.Add(clickPointGeometry);
      gg.Children.Add(textBox);
      gg.Children.Add(textGeometry);
      gg.Children.Add(hitTestCursorGeometry);
      this.TextSite.Render(gg);

      coercedClickPosition = SeekIntersectionDetail(IntersectionDetail.Empty, coercedClickPosition, new Point(0, coercedClickPosition.Y), textGeometry, FlowDirection.RightToLeft);
      coercedClickPosition = SeekIntersectionDetail(IntersectionDetail.Intersects, coercedClickPosition, new Point(0, coercedClickPosition.Y), textGeometry, FlowDirection.RightToLeft);
      //IntersectionDetail intersectionDetailResult = textGeometry.FillContainsWithDetail(hitTestCursorGeometry);
      //Predicate<IntersectionDetail> intersectionPredicate = intersectionDetail => intersectionDetail != IntersectionDetail.Empty;
      //while (intersectionPredicate.Invoke(intersectionDetailResult))
      //{
      //  coercedClickPosition.Offset(-0.1, 0);
      //  if (coercedClickPosition.X < 0)
      //  {
      //    return new Point(0, coercedClickPosition.Y);
      //  }

      //  hitTestCursorGeometry = new RectangleGeometry(new Rect(coercedClickPosition, new Size(caretInfo.Width, caretInfo.Height)));
      //  intersectionDetailResult = textGeometry.FillContainsWithDetail(hitTestCursorGeometry);
      //  if (!intersectionPredicate.Invoke(intersectionDetailResult))
      //  {
      //    intersectionPredicate = intersectionDetail => intersectionDetail == IntersectionDetail.Empty;
      //  }
      //}

      coercedClickPosition.Offset(0.1, 0);
      return coercedClickPosition;
    }

    private async Task<CaretInfo> CreateCaretInfoFromIndexAsync(int textIndex, Point offset)
    {
      TextInfo textInfoBeforeCaretPosition = await CreateTextInfoFromDocumentRangeAsync(..textIndex);
      Point caretPosition = await GetCaretPositionAsync(offset, textInfoBeforeCaretPosition);
      Size caretSize = GetCaretSize();

      return new CaretInfo(caretPosition, caretSize.Width, caretSize.Height, offset);
    }

    private CaretInfo CreateCaretInfoFromPoint(Point caretPosition, Point offset)
    {
      Size caretSize = GetCaretSize();
      return new CaretInfo(caretPosition, caretSize.Width, caretSize.Height, offset);
    }

    private Size GetCaretSize()
    {
      //TextRange textRangeAtCaretPosition = textRangesBeforeCaretPosition.Last();
      //FormattedText formattedTextAtCaretPosition = CreateFormattedTextFrom(textRangeAtCaretPosition.Text);
      //ApplyTextRangeFormatting(formattedTextAtCaretPosition, textRangeAtCaretPosition);
      double caretHeight = Math.Round(this.FormattedText.Height, 0, MidpointRounding.AwayFromZero);
      return new Size(this.CaretWidth, caretHeight);
    }

    private async Task<Point> GetCaretPositionAsync(Point offset, TextInfo textInfoBeforeCaretPosition)
    {
      FormattedText formattedTextBeforeCaretPosition = CreateFormattedTextFrom(textInfoBeforeCaretPosition);
      await ApplyTextRangesToFormattedTextAsync(formattedTextBeforeCaretPosition, textInfoBeforeCaretPosition.TextRanges);

      return new Point(formattedTextBeforeCaretPosition.WidthIncludingTrailingWhitespace + formattedTextBeforeCaretPosition.OverhangTrailing + offset.X, offset.Y);
    }

    private Task ApplyTextRangesToFormattedTextAsync(FormattedText formattedTextBeforeCaretPosition, IEnumerable<TextRange> textRangesBeforeCaretPosition) => Task.Run(() =>
                                                                                                                                                             {
                                                                                                                                                               foreach (TextRange textRange in textRangesBeforeCaretPosition)
                                                                                                                                                               {
                                                                                                                                                                 ApplyTextRangeFormatting(formattedTextBeforeCaretPosition, textRange);
                                                                                                                                                               }
                                                                                                                                                             });

    private TextRenderInfo CreateTextRenderInfo(IEnumerable<TextRange> textRanges)
    {
      var highlightBackgroungInfos = new List<HighlightBackgroundInfo>();
      foreach (TextRange textRange in textRanges)
      {
        Geometry highlightGeometry = this.FormattedText.BuildHighlightGeometry(new Point(0, 0), textRange.Begin, textRange.Length);
        var highlightBackgroundInfo = new HighlightBackgroundInfo(textRange.BorderBrush, textRange.Background, highlightGeometry, textRange.BorderThickness);
        highlightBackgroungInfos.Add(highlightBackgroundInfo);
      }

      return new TextRenderInfo(highlightBackgroungInfos, this.FormattedText);
    }

    private async Task InitializeTextMetaInfoAsync(TextInfo textInfo)
    {
      this.FormattedText = CreateFormattedTextFrom(textInfo);
      await InitializeFormattedTextAsync(textInfo);
      this.TextContainer = new TextContainer(this.TextContainer.TextInfo, this.FormattedText.LineHeight);
    }

    private Task InitializeFormattedTextAsync(TextInfo textInfo) =>
      Task.Run(() =>
      {
        foreach (TextRange textRange in textInfo.TextRanges)
        {
          ApplyTextRangeFormatting(this.FormattedText, textRange);
        }
      });

    private void ClearAllHighlightRanges() => this.FormattedText = CreateFormattedTextFrom(this.TextContainer.TextInfo);

    private void ClearTextRangeFormatting(IEnumerable<TextRange> highlightRanges)
    {
      foreach (TextRange textRange in highlightRanges)
      {
        this.FormattedText.SetForegroundBrush(textRange.TextBrush, textRange.Begin, textRange.Length);
        this.FormattedText.SetFontStretch(textRange.FontStretch, textRange.Begin, textRange.Length);
        this.FormattedText.SetFontSize(textRange.FontSize, textRange.Begin, textRange.Length);
        this.FormattedText.SetFontFamily(textRange.FontFamily, textRange.Begin, textRange.Length);
        this.FormattedText.SetCulture(textRange.CultureInfo, textRange.Begin, textRange.Length);
        this.FormattedText.SetFontStyle(textRange.FontStyle, textRange.Begin, textRange.Length);
        this.FormattedText.SetFontWeight(textRange.FontWeight, textRange.Begin, textRange.Length);
        this.FormattedText.SetNumberSubstitution(textRange.NumberSubstitution, textRange.Begin, textRange.Length);
        this.FormattedText.SetTextDecorations(new TextDecorationCollection(), textRange.Begin, textRange.Length);
      }
    }

    private void ApplyTextRangeFormatting(FormattedText formattedText, TextRange textRange)
    {
      int textRangeLength = Math.Min(formattedText.Text.Length, textRange.Length);
      formattedText.SetCulture(textRange.CultureInfo, textRange.Begin, textRangeLength);
      formattedText.SetNumberSubstitution(textRange.NumberSubstitution, textRange.Begin, textRangeLength);
      formattedText.SetForegroundBrush(textRange.TextBrush, textRange.Begin, textRangeLength);
      formattedText.SetFontStretch(textRange.FontStretch, textRange.Begin, textRangeLength);
      formattedText.SetFontSize(textRange.FontSize, textRange.Begin, textRangeLength);
      formattedText.SetFontFamily(textRange.FontFamily, textRange.Begin, textRangeLength);
      formattedText.SetFontStyle(textRange.FontStyle, textRange.Begin, textRangeLength);
      formattedText.SetFontWeight(textRange.FontWeight, textRange.Begin, textRangeLength);
      formattedText.SetTextDecorations(textRange.TextDecorations, textRange.Begin, textRangeLength);
    }

    private FormattedText CreateFormattedTextFrom(TextInfo textInfo)
    {
      var typeface = new Typeface(
        SystemFonts.MessageFontFamily,
        SystemFonts.MessageFontStyle,
        SystemFonts.MessageFontWeight,
        FontStretches.Normal,
        textInfo.FallbackFontFamily);
      return new FormattedText(
        textInfo.Text,
        CultureInfo.CurrentCulture,
        textInfo.FlowDirection,
        typeface,
        SystemFonts.MessageFontSize,
        textInfo.TextBrush,
        null,
        this.PixelsPerDip)
      {
        MaxTextWidth = this.IsTextWrappingEnabled ? textInfo.MaxAllowedTextWidth : 0,
      };
    }

    protected virtual void OnCaretIndexChanged(int newIndex)
      => this.CaretIndexChanged?.Invoke(this, new ValueEventArgs<int>(newIndex));

    public event EventHandler<ValueEventArgs<int>> CaretIndexChanged;
    public double PixelsPerDip { get; set; }
    public double CaretWidth { get; set; }
    public bool IsTextWrappingEnabled { get; set; }
    public FormattedText FormattedText { get; private set; }
    public TextSite TextSite { get; }
    private TextContainer TextContainer { get; set; }
    //private Dictionary<Point, int> CaretPositionToIndexMap { get; }
    private Dictionary<int, Dictionary<Point, int>> LineCaretPositionToIndexMapTable { get; }
  }
}

