namespace BionicCode.Controls.Net.Wpf
{
  using System;
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using System.Threading.Tasks;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Controls.Primitives;
  using System.Windows.Input;
  using System.Windows.Media;
  using BionicCode.Utilities.Net;
  using Math = System.Math;

  /// <summary>
  /// </summary>
  public class HighlightTextBox : Control
  {

    #region Routed events
    public static readonly RoutedEvent TextChangedEvent = EventManager.RegisterRoutedEvent(
        name: "TextChanged",
        routingStrategy: RoutingStrategy.Bubble,
        handlerType: typeof(TextChangedRoutedEventHandler),
        ownerType: typeof(HighlightTextBox));

    public event RoutedEventHandler TextChanged
    {
      add => AddHandler(TextChangedEvent, value);
      remove => RemoveHandler(TextChangedEvent, value);
    }
    #endregion
    #region Dependency properties

    public string Text
    {
      get => (string)GetValue(TextProperty);
      set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
      "Text",
      typeof(string),
      typeof(HighlightTextBox),
      new FrameworkPropertyMetadata(
        string.Empty,
        FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
        OnTextChanged,
        CoerceText));

    public int CaretPosition
    {
      get => (int)GetValue(CaretPositionProperty);
      set => SetValue(CaretPositionProperty, value);
    }

    public static readonly DependencyProperty CaretPositionProperty = DependencyProperty.Register(
      "CaretPosition",
      typeof(int),
      typeof(HighlightTextBox),
      new PropertyMetadata(default(int), OnCaretPositionChanged, CoerceCaretPosition));

    public IEnumerable<TextRange> TextRangeItemsSource
    {
      get => (IEnumerable<TextRange>)GetValue(TextRangeItemsSourceProperty);
      set => SetValue(TextRangeItemsSourceProperty, value);
    }

    public static readonly DependencyProperty TextRangeItemsSourceProperty = DependencyProperty.Register(
      "TextRangeItemsSource",
      typeof(IEnumerable<TextRange>),
      typeof(HighlightTextBox),
      new PropertyMetadata(default(IEnumerable<TextRange>), OnTextRangeItemsSourceChanged));

    public string SelectedText
    {
      get => (string)GetValue(SelectedTextProperty);
      set => SetValue(SelectedTextProperty, value);
    }

    public static readonly DependencyProperty SelectedTextProperty = DependencyProperty.Register(
      "SelectedText",
      typeof(string),
      typeof(HighlightTextBox),
      new PropertyMetadata(default));

    public double LineHeight
    {
      get => (double)GetValue(LineHeightProperty);
      set => SetValue(LineHeightProperty, value);
    }

    public static readonly DependencyProperty LineHeightProperty = DependencyProperty.Register(
      "LineHeight",
      typeof(double),
      typeof(HighlightTextBox),
      new PropertyMetadata(16.0));

    public bool AcceptsTab
    {
      get => (bool)GetValue(AcceptsTabProperty);
      set => SetValue(AcceptsTabProperty, value);
    }

    public static readonly DependencyProperty AcceptsTabProperty = DependencyProperty.Register(
      "AcceptsTab",
      typeof(bool),
      typeof(HighlightTextBox),
      new PropertyMetadata(default));

    public bool AcceptsReturn
    {
      get => (bool)GetValue(AcceptsReturnProperty);
      set => SetValue(AcceptsReturnProperty, value);
    }

    public static readonly DependencyProperty AcceptsReturnProperty = DependencyProperty.Register(
      "AcceptsReturn",
      typeof(bool),
      typeof(HighlightTextBox),
      new PropertyMetadata(default));

    public int TabSize
    {
      get => (int)GetValue(TabSizeProperty);
      set => SetValue(TabSizeProperty, value);
    }

    public static readonly DependencyProperty TabSizeProperty = DependencyProperty.Register(
      "TabSize",
      typeof(int),
      typeof(HighlightTextBox),
      new PropertyMetadata(4, OnTabSizeChanged, CoerceTabSize));

    #endregion Dependency properties

    #region private properties
    private const string WhiteSpaceCharacterText = " ";
    //private KeyConverter KeyConverter { get; }
    private Cursor OldCursor { get; set; }
    private UIElement PART_TextSiteHost { get; set; }
    private TextEngine TextEngine { get; }
    private string TabCharacter { get; set; }

    #endregion private properties

    protected bool HasText => this.Text.Length > 0;

    protected bool IsCaretPositionAtEnd => this.CaretPosition == this.Text.Length;

    protected bool IsCaretPositionAtBegin => this.CaretPosition == 0;

    private ScrollViewer ScrollHost { get; set; }

    static HighlightTextBox() => DefaultStyleKeyProperty.OverrideMetadata(typeof(HighlightTextBox), new FrameworkPropertyMetadata(typeof(HighlightTextBox)));

    public HighlightTextBox()
    {
      //this.KeyConverter = new KeyConverter();
      this.FontFamily = SystemFonts.MessageFontFamily;
      this.FontSize = 48;
      this.FontWeight = SystemFonts.MessageFontWeight;
      this.AcceptsTab = true;
      this.TextEngine = new TextEngine();
      this.TextEngine.CaretIndexChanged += OnTextEngineCaretIndexChanged;
      //this.TextRangeItemsSource = new ObservableCollection<TextRange>
      //{
      //  new TextRange(0, this.Background, this.Foreground, Brushes.Transparent, 0, text)
      //  {
      //    FontSize = this.FontSize,
      //    FontFamily = this.FontFamily,
      //    FontStretch = this.FontStretch,
      //    FontStyle = this.FontStyle,
      //    FontWeight = this.FontWeight,
      //  }
      //};
    }

    public override async void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      this.PART_TextSiteHost = GetTemplateChild("PART_TextSiteHost") as UIElement;
      if (this.PART_TextSiteHost is not null)
      {
        this.PART_TextSiteHost.GotKeyboardFocus += OnGotKeyboardFocus;
      }
      //this.PART_TextSiteHost.LostKeyboardFocus += OnLostKeyboardFocus;
      this.ScrollHost = this.PART_TextSiteHost is ScrollViewer scrollHost
        || this.TryFindVisualChildElement(out scrollHost)
          ? scrollHost
          : null;

      if (this.ScrollHost is not null)
      {
        this.ScrollHost.SizeChanged += OnScrollHostSizeChanged;
      }

      InitializeTextSite();
      await InitializeAsync();
      await MoveCaretToStartOfLine();
    }

    protected override Size MeasureOverride(Size constraint)
    {
      _ = base.MeasureOverride(this.TextEngine.TextSite.DesiredSize);
      return this.ScrollHost?.DesiredSize ?? this.TextEngine.TextSite.DesiredSize;
    }

    private async Task InitializeAsync()
    {
      OnTabSizeChanged(this.TabSize, this.TabSize);
      TextInfo textInfo = CreateTextInfo(" ");
      await this.TextEngine.InitializeAsync(textInfo);
    }

    private void OnTextEngineCaretIndexChanged(object sender, ValueEventArgs<int> e)
      => this.CaretPosition = e.Value;

    private static void OnCaretPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      => (d as HighlightTextBox).OnCaretPositionChanged((int)e.OldValue, (int)e.NewValue);

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      => (d as HighlightTextBox).OnTextChanged(e.OldValue as string, e.NewValue as string);

    private static object CoerceText(DependencyObject d, object baseValue) => baseValue is null ? string.Empty : baseValue;

    private static void OnTabSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as HighlightTextBox).OnTabSizeChanged((int)e.OldValue, (int)e.NewValue);

    private static object CoerceTabSize(DependencyObject d, object baseValue) => Math.Max(0, (int)baseValue);

    private static object CoerceCaretPosition(DependencyObject d, object baseValue) => System.Math.Max(0, (int)baseValue);

    private static void OnTextRangeItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as HighlightTextBox).OnTextRangeItemsSourceChanged(e.OldValue as IEnumerable<TextRange>, e.NewValue as IEnumerable<TextRange>);

    protected override async void OnPreviewTextInput(TextCompositionEventArgs e)
    {
      base.OnPreviewTextInput(e);

      await HandleTextInputAsync(e.Text);
      //MoveCaretToCurrentPosition();
    }

    protected override async void OnPreviewKeyDown(KeyEventArgs e)
    {
      base.OnPreviewKeyDown(e);

      bool isInputHandled = true;
      switch (e.Key)
      {
        case Key.Tab:
          if (this.AcceptsTab)
          {
            await SendTabInputAsync();
          }
          break;
        case Key.LineFeed:
          break;
        case Key.Clear:
          break;
        case Key.Enter or Key.Return:
          break;
        case Key.Escape:
          break;
        case Key.PageUp:
          break;
        case Key.Next or Key.PageDown:
          break;
        case Key.End:
          if (!this.IsCaretPositionAtEnd)
          {
            await MoveCaretToEndOfLine();
          }
          break;
        case Key.Home:
          if (!this.IsCaretPositionAtBegin)
          {
            await MoveCaretToStartOfLine();
          }
          break;
        case Key.Left:
          if (!this.IsCaretPositionAtBegin)
          {
            await MoveCaretToAsync(Math.Max(0, this.CaretPosition - 1));
          }
          break;
        case Key.Up:
          break;
        case Key.Right:
          if (!this.IsCaretPositionAtEnd)
          {
            await MoveCaretToAsync(this.CaretPosition + 1);
          }
          break;
        case Key.Down:
          break;
        case Key.Delete:
          if (this.HasText && !this.IsCaretPositionAtEnd)
          {
            await DeleteCharactersAtAsync(1, TextDirecetion.Right);
          }
          break;
        case Key.Back:
          if (this.HasText && !this.IsCaretPositionAtBegin)
          {
            await DeleteCharactersAtAsync(1, TextDirecetion.Left);
          }
          break;
        case Key.F1:
          break;
        case Key.F2:
          break;
        case Key.F3:
          break;
        case Key.F4:
          break;
        case Key.F5:
          break;
        case Key.F6:
          break;
        case Key.F7:
          break;
        case Key.F8:
          break;
        case Key.F9:
          break;
        case Key.F10:
          break;
        case Key.F11:
          break;
        case Key.F12:
          break;
        case Key.F13:
          break;
        case Key.F14:
          break;
        case Key.F15:
          break;
        case Key.F16:
          break;
        case Key.F17:
          break;
        case Key.F18:
          break;
        case Key.F19:
          break;
        case Key.F20:
          break;
        case Key.F21:
          break;
        case Key.F22:
          break;
        case Key.F23:
          break;
        case Key.F24:
          break;
        default:
          isInputHandled = false;
          break;
      }

      e.Handled = isInputHandled;
    }

    protected override async void OnInitialized(EventArgs e) => base.OnInitialized(e);

    private void OnScrollHostSizeChanged(object sender, SizeChangedEventArgs e) => InvalidateMeasure();

    protected void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
      ;
      //if (Keyboard.FocusedElement != this.TextEngine.TextContainer)
      //{
      //  this.TextEngine.TextContainer.StopListenToInput();
      //  //Keyboard.Focus(this.TextEngine.TextContainer);
      //}
      //e.Handled = true;
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
      base.OnMouseEnter(e);
      this.OldCursor = this.Cursor;
      this.Cursor = Cursors.IBeam;
      this.ForceCursor = true;
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
      base.OnMouseLeave(e);
      this.Cursor = this.OldCursor;
    }

    protected async void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
      if (Keyboard.FocusedElement != this.TextEngine.TextSite)
      {
        _ = Keyboard.Focus(this.TextEngine.TextSite);
        await MoveCaretToCurrentPositionAsync();
      }
    }

    protected virtual void OnTabSizeChanged(int oldValue, int newValue)
    {
      char[] tabCharacters = new char[newValue];
      Array.Fill(tabCharacters, ' ');
      this.TabCharacter = new string(tabCharacters);
    }

    protected virtual async void OnTextChanged(string oldText, string newText)
    {
      ;
      ;
      //MoveCaretToEnd();
    }

    protected virtual async void OnCaretPositionChanged(int oldValue, int newValue) => await MoveCaretToAsync(newValue);

    protected virtual void OnTextRangeItemsSourceChanged(IEnumerable<TextRange> oldTextRanges, IEnumerable<TextRange> newTextRanges)
    {
      if (oldTextRanges is INotifyCollectionChanged oldObservableCollection)
      {
        oldObservableCollection.CollectionChanged -= OnTextRangesChanged;
      }

      if (newTextRanges is INotifyCollectionChanged newObservableCollection)
      {
        newObservableCollection.CollectionChanged += OnTextRangesChanged;
      }
    }

    private void OnTextRangesChanged(object sender, NotifyCollectionChangedEventArgs e) => throw new NotImplementedException();

    protected async Task MoveCaretToCurrentPositionAsync() => await this.TextEngine.MoveCaretToAsync(this.CaretPosition, new Point(this.Padding.Left, this.Padding.Top));

    protected async Task MoveCaretToAsync(int position)
    {
      if (this.CaretPosition != position)
      {
        SetCaretPositionInternal(position);
      }

      await MoveCaretToCurrentPositionAsync();
    }

    protected async Task MoveCaretToStartOfLine()
    {
      if (this.CaretPosition != 0)
      {
        SetCaretPositionInternal(0);
      }

      await MoveCaretToCurrentPositionAsync();
    }

    protected async Task MoveCaretToEndOfLine()
    {
      if (this.CaretPosition != this.Text.Length)
      {
        SetCaretPositionInternal(this.Text.Length);
      }

      await MoveCaretToCurrentPositionAsync();
    }

    /// <summary>
    /// Sets the <seealso cref="Text"/> property and moves the caret to the end of the new text.
    /// </summary>
    /// <param name="text"></param>
    protected void SetTextInternal(string text) => SetTextInternal(text, text.Length);

    protected void SetTextInternal(string text, int caretPosition)
    {
      SetCurrentValue(TextProperty, text);
      if (this.CaretPosition != caretPosition)
      {
        SetCaretPositionInternal(caretPosition);
      }
    }

    protected void SetCaretPositionInternal(int caretPosition) => SetCurrentValue(HighlightTextBox.CaretPositionProperty, caretPosition);

    private async Task DeleteCharactersAtAsync(int characterCount, TextDirecetion textDirection) => await this.TextEngine.RemoveTextAtCurrentCaretPositionAsync(characterCount, textDirection);

    protected virtual async Task HandleTextInputAsync(string input)
    {
      int insertionIndex = this.CaretPosition;
      string oldText = this.Text;
      string newText = this.Text.Insert(insertionIndex, input);
      await WriteTextAsync(newText);
      SetTextInternal(newText, this.CaretPosition + input.Length);
      RaiseEvent(new TextChangedRoutedEventArgs(HighlightTextBox.TextChangedEvent, this, UndoAction.Create, oldText, newText, insertionIndex));
    }

    protected virtual async Task SendTabInputAsync() => await HandleTextInputAsync(this.TabCharacter);

    protected async Task WriteTextAsync(string text)
    {
      var textRanges = new List<TextRange>
      {
        new TextRange(0, this.Background, this.Foreground, Brushes.Transparent, 0, text)
        {
          FontSize = this.FontSize,
          FontFamily = this.FontFamily,
          FontStretch = this.FontStretch,
          FontStyle = this.FontStyle,
          FontWeight = this.FontWeight,
        }
      };

      TextInfo textInfo = TestCreateTextInfo(text, textRanges);
      await this.TextEngine.ShowTextAsync(textInfo, new Point(this.Padding.Left, this.Padding.Top));
    }

    private TextInfo TestCreateTextInfo(string text, IEnumerable<TextRange> textRanges)
    {
      var textInfo = new TextInfo(text, textRanges)
      {
        FallbackFontFamily = this.FontFamily,
        FallbackFontSize = this.FontSize,
        FlowDirection = this.FlowDirection,
        TextBrush = this.Foreground,
        MaxAllowedTextWidth = this.ActualWidth,
      };
      return textInfo;
    }

    private TextInfo CreateTextInfo(string text)
    {
      var textInfo = new TextInfo(text, this.TextRangeItemsSource
        ?? new[]
           {
             new TextRange(0, Brushes.Transparent, this.Foreground, text)
             {
               FontSize = this.FontSize,
               FontFamily = this.FontFamily,
               FontStretch = this.FontStretch,
               FontStyle = this.FontStyle,
               FontWeight = this.FontWeight,
             }
           })
      {
        FallbackFontFamily = this.FontFamily,
        FallbackFontSize = this.FontSize,
        FlowDirection = this.FlowDirection,
        TextBrush = this.Foreground,
        MaxAllowedTextWidth = this.ActualWidth,
      };
      return textInfo;
    }

    private void InitializeTextSite()
    {
      switch (this.PART_TextSiteHost)
      {
        case ScrollViewer scrollViewer:
          scrollViewer.Content = this.TextEngine.TextSite;
          break;
        case ContentControl contentControl:
          contentControl.Content = this.TextEngine.TextSite;
          break;
        case ContentPresenter contentPresenter:
          contentPresenter.Content = this.TextEngine.TextSite;
          break;
        case Border border:
          border.Child = this.TextEngine.TextSite;
          break;
        case Panel panel:
          _ = panel.Children.Add(this.TextEngine.TextSite);
          break;
        case Popup popup:
          popup.Child = this.TextEngine.TextSite;
          break;
      }
    }
  }
}
