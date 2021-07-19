using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace BionicCode.Controls.Net.Wpf
{
  /// <summary>
  /// Customizable page frame that displays a collection of arbitrary data model items according to a provided <see cref="ItemsControl.ItemTemplate"/>. The items are selectable.
  /// </summary>
  /// <remarks>
  /// <see cref="BionicSwipePageFrame"/> derives from <see cref="Selector"/> which is a <see cref="ItemsControl"/>. The <see cref="ItemsControl.ItemsPanel"/> property is ignored.<br/><br/>
  ///
  /// 
  /// If the <see cref="ItemsControl.ItemsSource"/> contains different data types use the <see cref="ItemsControl.ItemTemplateSelector"/> property to assign a <see cref="DataTemplateSelector"/> that maps the appropriate <see cref="DataTemplate"/> to the items. <br/>
  /// The <see cref="BionicSwipePageFrame"/> uses <see cref="BionicSwipePage"/> as container for the data items.<br/>
  /// The control uses semi-virtualization which means item containers are lazy generated once they are requested for display, but never destroyed. Full virtualization support is not implemented yet. <br/><br/>
  /// When <see cref="IsLoopingPagesEnabled"/> is set to <c>True</c>, index values assigned to <see cref="Selector.SelectedIndex"/> property or to the <see cref="ICommandSource.CommandParameter"/> of <see cref="LoadPageFromIndexRoutedCommand"/> are treated special. An index greater than the count of the <see cref="ItemsControl.ItemsSource"/> is coerced to point to the first item in the collection. Similarly an index value lesser than zero will point to the last page item (wrapping). <br/>
  /// When <see cref="IsLoopingPagesEnabled"/> is set to <c>False</c>, index values assigned to <see cref="Selector.SelectedIndex"/> property or to the <see cref="ICommandSource.CommandParameter"/> of <see cref="LoadPageFromIndexRoutedCommand"/> are coerced to valid index values. An index greater than the count of the <see cref="ItemsControl.ItemsSource"/> is coerced to point to the last item in the collection. An index value lesser than zero will point to the first page item (zero index). <br/>
  /// </remarks>
  /// <example>
  /// <code>
  /// &lt;BionicSwipePageFrame x:Name="PageFrame" Height="500" &gt;
  ///   &lt;BionicSwipePage&gt;First XAML created page&lt;/BionicSwipePage&gt;
  ///   &lt;BionicSwipePage&gt;Second XAML created page&lt;/BionicSwipePage&gt;
  ///   &lt;BionicSwipePage&gt;Third XAML created page&lt;/BionicSwipePage&gt;
  ///   &lt;BionicSwipePage&gt;Fourth XAML created page&lt;/BionicSwipePage&gt;
  /// &lt;/BionicSwipePageFrame&gt;
  /// </code>
  /// </example>
  [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(BionicSwipePage))]
  [TemplatePart(Name = "PART_SelectedPageHost", Type = typeof(ContentPresenter))]
  [TemplatePart(Name = "PART_PageHeader", Type = typeof(BionicSwipePageFrameHeader))]
  [TemplatePart(Name = "PART_PageHostPanel", Type = typeof(Panel))]
  public class BionicSwipePageFrame : Selector
  {
    #region RoutedCommands

    /// <summary>
    /// Load the next page. Command is parameterless.
    /// </summary>
    public static readonly RoutedUICommand LoadNextPageRoutedCommand = new RoutedUICommand("Load the next page", nameof(BionicSwipePageFrame.LoadNextPageRoutedCommand), typeof(BionicSwipePageFrame));

    /// <summary>
    /// Load the previous page. This Command is parameterless.
    /// </summary>
    public static readonly RoutedUICommand LoadPreviousPageRoutedCommand = new RoutedUICommand("Load the previous page", nameof(BionicSwipePageFrame.LoadPreviousPageRoutedCommand), typeof(BionicSwipePageFrame));

    /// <summary>
    /// Load a specific page by passing in the page index as CommandParameter. <br/>
    /// <see cref="ICommandSource.CommandParameter"/> is the item's index in the <see cref="ItemsControl.ItemsSource"/>.
    /// </summary>
    public static readonly RoutedUICommand LoadPageFromIndexRoutedCommand = new RoutedUICommand("Load a specific page by passing in the corresponding page index as CommandParameter", nameof(BionicSwipePageFrame.LoadPageFromIndexRoutedCommand), typeof(BionicSwipePageFrame));

    /// <summary>
    /// Load a specific page by passing in the item data model as CommandParameter. <br/>
    /// <see cref="ICommandSource.CommandParameter"/> is the item data model from the <see cref="ItemsControl.ItemsSource"/>.
    /// </summary>
    public static readonly RoutedUICommand LoadPageFromItemRoutedCommand = new RoutedUICommand("Load a specific page by passing in the item data model as CommandParameter", nameof(BionicSwipePageFrame.LoadPageFromItemRoutedCommand), typeof(BionicSwipePageFrame));

    #endregion

    #region RoutedEvents


    #region PageChangedRoutedEvents

    /// <summary>
    /// The <see cref="RoutedEvent"/> iof the <see cref="PageChanged"/> event.
    /// </summary>
    public static readonly RoutedEvent PreviewPageChangedRoutedEvent = EventManager.RegisterRoutedEvent("PreviewPageChanged",
      RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(BionicSwipePageFrame));

    /// <summary>
    /// The event is raised when the <see cref="SelectedPage"/> and the <see cref="PreviousPage"/> have changed.
    /// </summary>
    public event RoutedEventHandler PreviewPageChanged
    {
      add { AddHandler(BionicSwipePageFrame.PreviewPageChangedRoutedEvent, value); }
      remove { RemoveHandler(BionicSwipePageFrame.PreviewPageChangedRoutedEvent, value); }
    }

    /// <summary>
    /// The <see cref="RoutedEvent"/> iof the <see cref="PageChanged"/> event.
    /// </summary>
    public static readonly RoutedEvent PageChangedRoutedEvent = EventManager.RegisterRoutedEvent("PageChanged",
      RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BionicSwipePageFrame));

    /// <summary>
    /// The event is raised when the <see cref="SelectedPage"/> and the <see cref="PreviousPage"/> have changed.
    /// </summary>
    public event RoutedEventHandler PageChanged
    {
      add => AddHandler(BionicSwipePageFrame.PageChangedRoutedEvent, value);
      remove => RemoveHandler(BionicSwipePageFrame.PageChangedRoutedEvent, value);
    }

    #endregion

    #region SelectedItemChangingRoutedEvent

    public static readonly RoutedEvent SelectedItemChangingRoutedEvent = EventManager.RegisterRoutedEvent(
      "SelectedItemChanging",
      RoutingStrategy.Bubble,
      typeof(ValueChangedRoutedEventHandler<object>),
      typeof(BionicSwipePageFrame));

    public event ValueChangedRoutedEventHandler<object> SelectedItemChanging
    {
      add => AddHandler(BionicSwipePageFrame.SelectedItemChangingRoutedEvent, value);
      remove => RemoveHandler(BionicSwipePageFrame.SelectedItemChangingRoutedEvent, value);
    }

    #endregion

    #region PreviewSelectedItemChangingRoutedEvent

    public static readonly RoutedEvent PreviewSelectedItemChangingRoutedEvent = EventManager.RegisterRoutedEvent(
      "PreviewSelectedItemChanging",
      RoutingStrategy.Tunnel,
      typeof(ValueChangedRoutedEventHandler<object>),
      typeof(BionicSwipePageFrame));

    public event ValueChangedRoutedEventHandler<object> PreviewSelectedItemChanging
    {
      add => AddHandler(BionicSwipePageFrame.PreviewSelectedItemChangingRoutedEvent, value);
      remove => RemoveHandler(BionicSwipePageFrame.PreviewSelectedItemChangingRoutedEvent, value);
    }

    #endregion

    #region SelectedItemChangedRoutedEvent

    public static readonly RoutedEvent SelectedItemChangedRoutedEvent = EventManager.RegisterRoutedEvent(
      "SelectedItemChanged",
      RoutingStrategy.Bubble,
      typeof(ValueChangedRoutedEventHandler<object>),
      typeof(BionicSwipePageFrame));

    public event ValueChangedRoutedEventHandler<object> SelectedItemChanged
    {
      add => AddHandler(BionicSwipePageFrame.SelectedItemChangedRoutedEvent, value);
      remove => RemoveHandler(BionicSwipePageFrame.SelectedItemChangedRoutedEvent, value);
    }

    #endregion

    #region PreviewSelectedItemChangedRoutedEvent

    public static readonly RoutedEvent PreviewSelectedItemChangedRoutedEvent = EventManager.RegisterRoutedEvent(
      "PreviewSelectedItemChanged",
      RoutingStrategy.Tunnel,
      typeof(ValueChangedRoutedEventHandler<object>),
      typeof(BionicSwipePageFrame));

    public event ValueChangedRoutedEventHandler<object> PreviewSelectedItemChanged
    {
      add => AddHandler(BionicSwipePageFrame.PreviewSelectedItemChangedRoutedEvent, value);
      remove => RemoveHandler(BionicSwipePageFrame.PreviewSelectedItemChangedRoutedEvent, value);
    }

    #region SelectedIndexChangingRoutedEvent

    public static readonly RoutedEvent SelectedIndexChangingRoutedEvent = EventManager.RegisterRoutedEvent(
      "SelectedIndexChanging",
      RoutingStrategy.Bubble,
      typeof(ValueChangedRoutedEventHandler<int>),
      typeof(BionicSwipePageFrame));

    public event ValueChangedRoutedEventHandler<int> SelectedIndexChanging
    {
      add => AddHandler(BionicSwipePageFrame.SelectedIndexChangingRoutedEvent, value);
      remove => RemoveHandler(BionicSwipePageFrame.SelectedIndexChangingRoutedEvent, value);
    }

    #endregion

    #region PreviewSelectedIndexChangingRoutedEvent

    public static readonly RoutedEvent PreviewSelectedIndexChangingRoutedEvent = EventManager.RegisterRoutedEvent(
      "PreviewSelectedIndexChanging",
      RoutingStrategy.Tunnel,
      typeof(ValueChangedRoutedEventHandler<int>),
      typeof(BionicSwipePageFrame));

    public event ValueChangedRoutedEventHandler<int> PreviewSelectedIndexChanging
    {
      add => AddHandler(BionicSwipePageFrame.PreviewSelectedIndexChangingRoutedEvent, value);
      remove => RemoveHandler(BionicSwipePageFrame.PreviewSelectedIndexChangingRoutedEvent, value);
    }

    #endregion

    #region SelectedIndexChangedRoutedEvent

    public static readonly RoutedEvent SelectedIndexChangedRoutedEvent = EventManager.RegisterRoutedEvent(
      "SelectedIndexChanged",
      RoutingStrategy.Bubble,
      typeof(ValueChangedRoutedEventHandler<int>),
      typeof(BionicSwipePageFrame));

    public event ValueChangedRoutedEventHandler<int> SelectedIndexChanged
    {
      add => AddHandler(BionicSwipePageFrame.SelectedIndexChangedRoutedEvent, value);
      remove => RemoveHandler(BionicSwipePageFrame.SelectedIndexChangedRoutedEvent, value);
    }

    #endregion

    #region PreviewSelectedIndexChangedRoutedEvent

    public static readonly RoutedEvent PreviewSelectedIndexChangedRoutedEvent = EventManager.RegisterRoutedEvent(
      "PreviewSelectedIndexChanged",
      RoutingStrategy.Tunnel,
      typeof(ValueChangedRoutedEventHandler<int>),
      typeof(BionicSwipePageFrame));

    public event ValueChangedRoutedEventHandler<int> PreviewSelectedIndexChanged
    {
      add => AddHandler(BionicSwipePageFrame.PreviewSelectedIndexChangedRoutedEvent, value);
      remove => RemoveHandler(BionicSwipePageFrame.PreviewSelectedIndexChangedRoutedEvent, value);
    }

    #endregion

    #endregion

    #endregion

    #region DependencyProperties

    /// <summary>
    /// The DependencyProperty for the <see cref="SelectedPage"/> property.
    /// </summary>
    public static readonly DependencyProperty SelectedPageProperty = DependencyProperty.Register(
      "SelectedPage",
      typeof(BionicSwipePage),
      typeof(BionicSwipePageFrame),
      new PropertyMetadata(default(BionicSwipePage), BionicSwipePageFrame.OnSelectedPageChanged));

    /// <summary>
    /// The currently displaying <see cref="BionicSwipePage"/> instance. 
    /// </summary>
    public BionicSwipePage SelectedPage
    {
      get => (BionicSwipePage) GetValue(BionicSwipePageFrame.SelectedPageProperty);
      set => SetValue(BionicSwipePageFrame.SelectedPageProperty, value);
    }

    public static readonly DependencyProperty PreviousSelectedIndexProperty = DependencyProperty.Register(
      "PreviousSelectedIndex",
      typeof(int),
      typeof(BionicSwipePageFrame),
      new PropertyMetadata(default(int)));

    public int PreviousSelectedIndex
    {
      get => (int) GetValue(BionicSwipePageFrame.PreviousSelectedIndexProperty);
      set => SetValue(BionicSwipePageFrame.PreviousSelectedIndexProperty, value);
    }

    public static readonly DependencyProperty PreviousPageProperty = DependencyProperty.Register(
      "PreviousPage",
      typeof(BionicSwipePage),
      typeof(BionicSwipePageFrame),
      new PropertyMetadata(default(BionicSwipePage)));

    public BionicSwipePage PreviousPage { get { return (BionicSwipePage) GetValue(BionicSwipePageFrame.PreviousPageProperty); } set { SetValue(BionicSwipePageFrame.PreviousPageProperty, value); } }

    public static readonly DependencyProperty IsHeaderVisibleProperty = DependencyProperty.Register(
      "IsHeaderVisible",
      typeof(bool),
      typeof(BionicSwipePageFrame),
      new PropertyMetadata(true, BionicSwipePageFrame.OnIsHeaderVisibleChanged));

    public bool IsHeaderVisible
    {
      get => (bool) GetValue(BionicSwipePageFrame.IsHeaderVisibleProperty);
      set => SetValue(BionicSwipePageFrame.IsHeaderVisibleProperty, value);
    }

    public static readonly DependencyProperty FrameHeaderStyleProperty = DependencyProperty.Register(
      "FrameHeaderStyle",
      typeof(Style),
      typeof(BionicSwipePageFrame),
      new PropertyMetadata(default(Style), BionicSwipePageFrame.OnFrameHeaderStyleChanged));

    public Style FrameHeaderStyle
    {
      get => (Style) GetValue(BionicSwipePageFrame.FrameHeaderStyleProperty);
      set => SetValue(BionicSwipePageFrame.FrameHeaderStyleProperty, value);
    }

    public static readonly DependencyProperty NavigationDirectionProperty = DependencyProperty.Register(
      "NavigationDirection",
      typeof(PageNavigationDirection),
      typeof(BionicSwipePageFrame),
      new PropertyMetadata(PageNavigationDirection.Undefined, BionicSwipePageFrame.OnNavigationDirectionChanged));

    public PageNavigationDirection NavigationDirection
    {
      get => (PageNavigationDirection) GetValue(BionicSwipePageFrame.NavigationDirectionProperty);
      set => SetValue(BionicSwipePageFrame.NavigationDirectionProperty, value);
    }

    /// <summary>
    /// DependencyProperty of the <see cref="IsLoopingPagesEnabled"/> property
    /// </summary>
    public static readonly DependencyProperty IsLoopingPagesEnabledProperty = DependencyProperty.Register(
      "IsLoopingPagesEnabled",
      typeof(bool),
      typeof(BionicSwipePageFrame),
      new PropertyMetadata(true));

    /// <summary>
    /// When enabled the <see cref="LoadNextPageRoutedCommand"/> and <see cref="LoadPreviousPageRoutedCommand"/> will loop throug the <see cref="ItemsControl.ItemsSource"/>.
    /// </summary>
    public bool IsLoopingPagesEnabled
    {
      get => (bool) GetValue(BionicSwipePageFrame.IsLoopingPagesEnabledProperty);
      set => SetValue(BionicSwipePageFrame.IsLoopingPagesEnabledProperty, value);
    }

    private static readonly DependencyProperty PreviousPageTranslationStartPositionProperty =
      DependencyProperty.Register(
        "PreviousPageTranslationStartPosition",
        typeof(double),
        typeof(BionicSwipePageFrame),
        new PropertyMetadata(default(double)));

    private double PreviousPageTranslationStartPosition
    {
      get => (double) GetValue(BionicSwipePageFrame.PreviousPageTranslationStartPositionProperty);
      set => SetValue(BionicSwipePageFrame.PreviousPageTranslationStartPositionProperty, value);
    }

    private static readonly DependencyProperty PreviousPageTranslationEndPositionProperty = DependencyProperty.Register(
      "PreviousPageTranslationEndPosition",
      typeof(double),
      typeof(BionicSwipePageFrame),
      new PropertyMetadata(default(double)));

    private double PreviousPageTranslationEndPosition
    {
      get => (double) GetValue(BionicSwipePageFrame.PreviousPageTranslationEndPositionProperty);
      set => SetValue(BionicSwipePageFrame.PreviousPageTranslationEndPositionProperty, value);
    }

    private static readonly DependencyProperty SelectedPageTranslationStartPositionProperty =
      DependencyProperty.Register(
        "SelectedPageTranslationStartPosition",
        typeof(double),
        typeof(BionicSwipePageFrame),
        new PropertyMetadata(default(double)));

    private double SelectedPageTranslationStartPosition
    {
      get => (double) GetValue(BionicSwipePageFrame.SelectedPageTranslationStartPositionProperty);
      set => SetValue(BionicSwipePageFrame.SelectedPageTranslationStartPositionProperty, value);
    }

    private static readonly DependencyProperty SelectedPageTranslationEndPositionProperty = DependencyProperty.Register(
      "SelectedPageTranslationEndPosition",
      typeof(double),
      typeof(BionicSwipePageFrame),
      new PropertyMetadata(default(double)));

    private double SelectedPageTranslationEndPosition
    {
      get => (double) GetValue(BionicSwipePageFrame.SelectedPageTranslationEndPositionProperty);
      set => SetValue(BionicSwipePageFrame.SelectedPageTranslationEndPositionProperty, value);
    }

    /// <summary>
    /// The DependencyProperty for <see cref="TitleMemberPath"/>.
    /// </summary>
    public static readonly DependencyProperty TitleMemberPathProperty = DependencyProperty.Register(
      "TitleMemberPath",
      typeof(string),
      typeof(BionicSwipePageFrame),
      new PropertyMetadata(default(string), BionicSwipePageFrame.OnTitleMemberPathChanged));
    
    /// <summary>
    /// The property path to the property that holds the page title <c>String</c>. The property path is relative to the data model type. <br/><br/>
    /// E.g. The property path to the <c>PageTitle</c> property of a data model class called ExampleClass would be <c>"PageTitle"</c>. <br/>The property can be nested (e.g. <c>"NestedType.PageTitle"</c>) or reference a collection member (e.g. <c>"NestedType.Items[1].PageTitle"</c>) 
    /// </summary>
    public string TitleMemberPath
    {
      get => (string) GetValue(BionicSwipePageFrame.TitleMemberPathProperty);
      set => SetValue(BionicSwipePageFrame.TitleMemberPathProperty, value);
    }

    public static readonly DependencyProperty TitleTemplateProperty = DependencyProperty.Register(
      "TitleTemplate",
      typeof(DataTemplate),
      typeof(BionicSwipePageFrame),
      new PropertyMetadata(default(DataTemplate), BionicSwipePageFrame.OnTitleTemplateChanged));

    public DataTemplate TitleTemplate
    {
      get => (DataTemplate) GetValue(BionicSwipePageFrame.TitleTemplateProperty);
      set => SetValue(BionicSwipePageFrame.TitleTemplateProperty, value);
    }

    public static readonly DependencyProperty TitleDataTemplateSelectorProperty = DependencyProperty.Register(
      "TitleDataTemplateSelector",
      typeof(DataTemplateSelector),
      typeof(BionicSwipePageFrame),
      new PropertyMetadata(default(DataTemplateSelector), BionicSwipePageFrame.OnTitleDataTemplateSelectorChanged));

    public DataTemplateSelector TitleDataTemplateSelector
    {
      get => (DataTemplateSelector) GetValue(BionicSwipePageFrame.TitleDataTemplateSelectorProperty);
      set => SetValue(BionicSwipePageFrame.TitleDataTemplateSelectorProperty, value);
    }

    #endregion

    static BionicSwipePageFrame()
    {
      FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
        typeof(BionicSwipePageFrame),
        new FrameworkPropertyMetadata(typeof(BionicSwipePageFrame)));
      KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(
        typeof(BionicSwipePageFrame),
        new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));
      Selector.SelectedIndexProperty.OverrideMetadata(typeof(BionicSwipePageFrame), new FrameworkPropertyMetadata(0, BionicSwipePageFrame.OnSelectedIndexChanged, BionicSwipePageFrame.CoerceSelectedIndex));
      Selector.SelectedItemProperty.OverrideMetadata(typeof(BionicSwipePageFrame), new FrameworkPropertyMetadata(null, BionicSwipePageFrame.OnSelectedItemChanged, BionicSwipePageFrame.CoerceSelectedItem));
    }

    public BionicSwipePageFrame()
    {
      this.CommandBindings.Add(
        new CommandBinding(
          BionicSwipePageFrame.LoadNextPageRoutedCommand,
          ((sender, args) => this.SelectedIndex += 1)));
      this.CommandBindings.Add(
        new CommandBinding(
          BionicSwipePageFrame.LoadPreviousPageRoutedCommand,
          ((sender, args) => this.SelectedIndex -= 1)));
      this.CommandBindings.Add(
        new CommandBinding(
          BionicSwipePageFrame.LoadPageFromItemRoutedCommand,
          (sender, args) => this.SelectedItem = args.Parameter, (sender, args) => args.CanExecute = args.Parameter != null && this.Items.Contains(args.Parameter)));
      this.CommandBindings.Add(
        new CommandBinding(
          BionicSwipePageFrame.LoadPageFromIndexRoutedCommand,
          (sender, args) => this.SelectedIndex = args.Parameter is int newSelectedIndex
            ? newSelectedIndex
            : int.Parse((string) args.Parameter),
          (sender, args) => args.CanExecute = args.Parameter is int || int.Parse(args.Parameter as string) is int));

      this.PageChanged += OnPageChanged;
      this.SelectedItemChanging += OnSelectedItemChanging;
      this.SelectedItemChanged += OnSelectedItemChanged;
      this.SelectedIndexChanging += OnSelectedIndexChanging;
      this.SelectedIndexChanged += OnSelectedIndexChanged;

      this.Loaded += Initialize;
    }

    private void Initialize(object sender, RoutedEventArgs e)
    {
      InitializePageHeaderPart();
      InitializeAnimatedElements();
      var pageContentBinding = new Binding()
      {
        Source = this,
        Path = new PropertyPath(nameof(this.SelectedPage))
      };
      this.PART_SelectedPageHost.SetBinding(ContentPresenter.ContentProperty, pageContentBinding);
      this.IsInitialized = true;
      this.SelectedIndex = 0;
      UpdateSelectedPage();

    }

    private static void OnTitleMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as BionicSwipePageFrame).UpdateFrameTitleFromTitleMemberPath();
    }
    
    /// <summary>
    /// Called when the <see cref="PageChanged"/> event is received.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected virtual void OnPageChanged(object sender, RoutedEventArgs e)
    {
    }

    private static void OnIsHeaderVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var _this = d as BionicSwipePageFrame;
      if (_this.PART_PageHeader == null)
      {
        return;
      }

      _this.PART_PageHeader.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
    }

    private static void OnTitleDataTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var _this = (d as BionicSwipePageFrame);
      if (_this.PART_PageHeader != null)
      {
        _this.PART_PageHeader.TitleDataTemplateSelector = e.NewValue as DataTemplateSelector;
      }
    }

    private static void OnTitleTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var _this = (d as BionicSwipePageFrame);
      if (_this.PART_PageHeader != null)
      {
        _this.PART_PageHeader.TitleTemplate = e.NewValue as DataTemplate;
      }
    }

    private static object CoerceSelectedIndex(DependencyObject d, object basevalue)
    {
      var _this = d as BionicSwipePageFrame;
      if (!_this.IsInitialized)
      {
        return basevalue;
      }
      
      var originalValue = (int) basevalue;
      int coercedValue = originalValue;

      _this.NavigationDirection = originalValue > _this.SelectedIndex
        ? PageNavigationDirection.Next
        : PageNavigationDirection.Previous;

      if (_this.Items.Count == 0)
      {
        _this.NavigationDirection = PageNavigationDirection.Undefined;
        coercedValue = -1;
      }
      else if (_this.IsLoopingPagesEnabled)
      {
        if (originalValue < 0)
        {
          coercedValue = _this.Items.Count - 1;
        }
        else if (originalValue >= _this.Items.Count)
        {
          coercedValue = 0;
        }
      }
      else 
      {
        if (_this.NavigationDirection == PageNavigationDirection.Previous)
        {
          coercedValue = Math.Max(originalValue, 0);
        }
        else if (_this.NavigationDirection == PageNavigationDirection.Next)
        {
          coercedValue = Math.Min(originalValue, _this.Items.Count - 1);
        }
      }

      _this.RaiseEvent(new ValueChangedRoutedEventArgs<int>(BionicSwipePageFrame.SelectedIndexChangingRoutedEvent, _this, _this.SelectedIndex, coercedValue));

      return coercedValue;
    }

    private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var _this = (d as BionicSwipePageFrame);
      _this.RaiseEvent(new ValueChangedRoutedEventArgs<int>(BionicSwipePageFrame.SelectedIndexChangedRoutedEvent, _this, (int) e.OldValue, (int) e.NewValue));
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var _this = (d as BionicSwipePageFrame);
      _this.RaiseEvent(new ValueChangedRoutedEventArgs<object>(BionicSwipePageFrame.SelectedItemChangedRoutedEvent, _this, e.OldValue, e.NewValue));
    }

    private static object CoerceSelectedItem(DependencyObject d, object basevalue)
    {
      var _this = d as BionicSwipePageFrame;

     _this.RaiseEvent(new ValueChangedRoutedEventArgs<object>(BionicSwipePageFrame.SelectedItemChangingRoutedEvent, _this, _this.SelectedItem, basevalue));

      return basevalue;
    }

    protected virtual void OnSelectedIndexChanging(object sender, ValueChangedRoutedEventArgs<int> routedEventArgs)
    {
      if (!this.IsInitialized)
      {
        return;
      }

      this.PreviousSelectedIndex = routedEventArgs.OldValue;
    }

    /// <summary>
    /// Called when the <see cref="SelectedIndexChanged"/> event is received. <br/>
    /// Executes the swipe animation.
    /// </summary>
    /// <param name="sender">The event source instance.</param>
    /// <param name="routedEventArgs">Event args holding the old <see cref="Selector.SelectedIndex"/> and the <see cref="Selector.SelectedIndex"/> </param>
    protected virtual void OnSelectedIndexChanged(object sender, ValueChangedRoutedEventArgs<int> routedEventArgs)
    {
    }
    
    protected virtual void OnSelectedItemChanged(object sender, ValueChangedRoutedEventArgs<object> routedEventArgs)
    {
      if (!this.IsInitialized)
      {
        return;
      }

      UpdateSelectedPage();

      // new behavior (DDVSO 208019) - change SelectedContent and focus
      // before raising SelectionChanged.
      bool isKeyboardFocusWithin = this.IsKeyboardFocusWithin;
      if (isKeyboardFocusWithin)
      {
        // If keyboard focus is within the control, make sure it is going to the correct place
        BionicSwipePage item = GetPageContainerAt(this.SelectedIndex);
        item?.Focus();
      }

      InitializeSwipeTranslateAnimations();
      HandleTranslateSwipeAnimation();
      RaiseEvent(new RoutedEventArgs(BionicSwipePageFrame.PreviewPageChangedRoutedEvent, this));
      RaiseEvent(new RoutedEventArgs(BionicSwipePageFrame.PageChangedRoutedEvent, this));
    }
    
    protected virtual void OnSelectedItemChanging(object sender, ValueChangedRoutedEventArgs<object> routedEventArgs)
    {
      if (!this.IsInitialized)
      {
        return;
      }

      UpdateLeavingPage();
      InitializeSwipeVisibilityAnimations();
      HandleVisibilitySwipeAnimation();
    }

    private static void OnSelectedPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as BionicSwipePageFrame).OnSelectedPageChanged((BionicSwipePage) e.OldValue, (BionicSwipePage) e.NewValue);
    }

    /// <summary>
    /// Called when the <see cref="SelectedPage"/> property changes.
    /// </summary>
    /// <param name="oldValue">Old value of the <see cref="SelectedPage"/> property.</param>
    /// <param name="newValue">New value of the <see cref="SelectedPage"/> property.</param>
    protected virtual void OnSelectedPageChanged(BionicSwipePage oldValue, BionicSwipePage newValue)
    {
    }

    private static void OnPreviousPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as BionicSwipePageFrame).OnPreviousPageChanged((BionicSwipePage) e.OldValue, (BionicSwipePage) e.NewValue);
    }

    /// <summary>
    /// Called when the <see cref="PreviousPage"/> property changes.
    /// </summary>
    /// <param name="oldValue">Old value of the <see cref="PreviousPage"/> property.</param>
    /// <param name="newValue">New value of the <see cref="PreviousPage"/> property.</param>
    protected virtual  void OnPreviousPageChanged(BionicSwipePage oldValue, BionicSwipePage newValue)
    {
    }
    
    private static void OnFrameHeaderStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var _this = (d as BionicSwipePageFrame);
      var style = (Style) e.NewValue;
      if (style == null || _this.PART_PageHeader == null)
      {
        return;
      }
      _this.PART_PageHeader.Style = style;
    }

    private static void OnNavigationDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var _this = (d as BionicSwipePageFrame);
      _this.OnNavigationDirectionChanged((PageNavigationDirection) e.OldValue, (PageNavigationDirection) e.NewValue);
    }

    /// <summary> 
    /// Called when the <see cref="NavigationDirection"/> property changes.
    /// </summary>
    /// <param name="oldNavigationDirection">The last value of the <see cref="NavigationDirection"/> property.</param>
    /// <param name="newNavigationDirection">The new value of the <see cref="NavigationDirection"/> property.</param>
    protected virtual void OnNavigationDirectionChanged(PageNavigationDirection oldNavigationDirection, PageNavigationDirection newNavigationDirection)
    {
      if (newNavigationDirection == PageNavigationDirection.Undefined)
      {
        return;
      }

      this.PreviousPageTranslationEndPosition = newNavigationDirection == PageNavigationDirection.Next
        ? this.ActualWidth * -1
        : this.ActualWidth;
      this.SelectedPageTranslationStartPosition = newNavigationDirection == PageNavigationDirection.Next
        ? this.ActualWidth
        : this.ActualWidth * -1;
    }

    #region Overrides of FrameworkElement

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      this.PART_PageHeader = GetTemplateChild("PART_PageHeader") as BionicSwipePageFrameHeader;
      this.PART_PageHostPanel = GetTemplateChild("PART_PageHostPanel") as Panel;
      this.PART_SelectedPageHost = GetTemplateChild("PART_SelectedPageHost") as ContentPresenter;
      this.PART_SelectedPageHost.Visibility = Visibility.Visible;
    }

    #endregion

    private void InitializeAnimatedElements()
    {
      InitializePageFrameHeader();
      InitializeAnimatedPageParts();
    }

    private void InitializePageFrameHeader()
    {
      this.AnimatedPreviousPageHeaderTitle = new Rectangle { Visibility = Visibility.Collapsed, Fill = new VisualBrush() };
      this.PART_PageHeader.PART_PageHeaderHostPanel.Children.Add(this.AnimatedPreviousPageHeaderTitle);
      Panel.SetZIndex(this.AnimatedPreviousPageHeaderTitle, 100);
      this.AnimatedPreviousPageHeaderTitle.RenderTransform = new TranslateTransform();

      this.PART_PageHeader.PART_PageHeaderHostPanel.ClipToBounds = true;
    }
    
    private void InitializeAnimatedPageParts()
    {
      this.AnimatedSelectedPageHost = new Rectangle {Visibility = Visibility.Collapsed, Fill = new VisualBrush()};
      this.PART_PageHostPanel.Children.Add(this.AnimatedSelectedPageHost);
      var selectedPageBinding = new Binding
      {
        Source = this,
        Path = new PropertyPath(nameof(this.SelectedPage))
      };

      BindingOperations.SetBinding(this.AnimatedSelectedPageHost.Fill as VisualBrush, VisualBrush.VisualProperty, selectedPageBinding);
      Panel.SetZIndex(this.AnimatedSelectedPageHost, 100);
      this.AnimatedSelectedPageHost.RenderTransform = new TranslateTransform();
      var selectedPageTranslationStartPositionBinding = new Binding
      {
        Source = this,
        Path = new PropertyPath(nameof(this.SelectedPageTranslationStartPosition))
      };
      this.AnimatedSelectedPageHost.SetBinding(TranslateTransform.XProperty, selectedPageTranslationStartPositionBinding);

      this.AnimatedPreviousPageHost = new Rectangle { Visibility = Visibility.Collapsed, Fill = new VisualBrush() };
      this.PART_PageHostPanel.Children.Add(this.AnimatedPreviousPageHost);
      var previousPageBinding = new Binding
      {
        Source = this,
        Path = new PropertyPath(nameof(this.PreviousPage))
      };
      BindingOperations.SetBinding(this.AnimatedPreviousPageHost.Fill as VisualBrush, VisualBrush.VisualProperty, previousPageBinding);
      Panel.SetZIndex(this.AnimatedPreviousPageHost, 100);
      this.AnimatedPreviousPageHost.RenderTransform = new TranslateTransform();
      var previousPageTranslationEndPositionBinding = new Binding
      {
        Source = this,
        Path = new PropertyPath(nameof(this.PreviousPageTranslationEndPosition))
      };
      this.AnimatedPreviousPageHost.SetBinding(TranslateTransform.XProperty, previousPageTranslationEndPositionBinding);

      this.PART_PageHostPanel.ClipToBounds = true;
    }

    private void InitializePageHeaderPart()
    {
      if (this.PART_PageHeader.Style == null && this.FrameHeaderStyle != null)
      {
        this.PART_PageHeader.Style = this.FrameHeaderStyle;
      }

      if (this.PART_PageHeader.TitleDataTemplateSelector == null)
      {
        this.PART_PageHeader.TitleDataTemplateSelector = this.TitleDataTemplateSelector;
      }

      if (this.PART_PageHeader.TitleTemplate == null && this.TitleTemplate != null)
      {
        this.PART_PageHeader.TitleTemplate = this.TitleTemplate;
      }

      UpdateFrameTitleFromTitleMemberPath();
      this.PART_PageHeader.Visibility = this.IsHeaderVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    private void InitializeSwipeVisibilityAnimations()
    {
      var selectedPageVisibilityAnimation = new ObjectAnimationUsingKeyFrames { Duration = new Duration(TimeSpan.FromSeconds(0.3)) };
      selectedPageVisibilityAnimation.KeyFrames.Add(
        new DiscreteObjectKeyFrame(
          Visibility.Hidden,
          KeyTime.FromTimeSpan(TimeSpan.Zero)));

      var selectedAnimationPageVisibilityAnimation =
        new ObjectAnimationUsingKeyFrames { Duration = new Duration(TimeSpan.FromSeconds(0.3)) };
      selectedAnimationPageVisibilityAnimation.KeyFrames.Add(
        new DiscreteObjectKeyFrame(
          Visibility.Visible,
          KeyTime.FromTimeSpan(TimeSpan.Zero)));

      var previousAnimatedPageVisibilityAnimation =
        new ObjectAnimationUsingKeyFrames { Duration = new Duration(TimeSpan.FromSeconds(0.3)) };
      previousAnimatedPageVisibilityAnimation.KeyFrames.Add(
        new DiscreteObjectKeyFrame(
          Visibility.Visible,
          KeyTime.FromTimeSpan(TimeSpan.Zero)));

      var previousAnimatedPageHeaderTitleVisibilityAnimation =
        new ObjectAnimationUsingKeyFrames { Duration = new Duration(TimeSpan.FromSeconds(0.3))};
      previousAnimatedPageHeaderTitleVisibilityAnimation.KeyFrames.Add(
        new DiscreteObjectKeyFrame(
          Visibility.Visible,
          KeyTime.FromTimeSpan(TimeSpan.Zero)));

      var selectedAnimatedPageHeaderTitleVisibilityAnimation =
        new ObjectAnimationUsingKeyFrames { Duration = new Duration(TimeSpan.FromSeconds(0.3))};
      selectedAnimatedPageHeaderTitleVisibilityAnimation.KeyFrames.Add(
        new DiscreteObjectKeyFrame(
          Visibility.Hidden,
          KeyTime.FromTimeSpan(TimeSpan.Zero)));


      Storyboard.SetTarget(selectedPageVisibilityAnimation, this.PART_SelectedPageHost);
      Storyboard.SetTargetProperty(selectedPageVisibilityAnimation, new PropertyPath("Visibility"));
      Storyboard.SetTarget(previousAnimatedPageVisibilityAnimation, this.AnimatedPreviousPageHost);
      Storyboard.SetTargetProperty(previousAnimatedPageVisibilityAnimation, new PropertyPath("Visibility"));
      Storyboard.SetTarget(selectedAnimationPageVisibilityAnimation, this.AnimatedSelectedPageHost);
      Storyboard.SetTargetProperty(selectedAnimationPageVisibilityAnimation, new PropertyPath("Visibility"));
      Storyboard.SetTarget(previousAnimatedPageHeaderTitleVisibilityAnimation, this.AnimatedPreviousPageHeaderTitle);
      Storyboard.SetTargetProperty(previousAnimatedPageHeaderTitleVisibilityAnimation, new PropertyPath("Visibility"));
      Storyboard.SetTarget(selectedAnimatedPageHeaderTitleVisibilityAnimation, this.PART_PageHeader.PART_Title);
      Storyboard.SetTargetProperty(selectedAnimatedPageHeaderTitleVisibilityAnimation, new PropertyPath("Visibility"));
      this.VisibilityStoryboard = new Storyboard { FillBehavior = FillBehavior.Stop };
      this.VisibilityStoryboard.Children.Add(previousAnimatedPageVisibilityAnimation);
      this.VisibilityStoryboard.Children.Add(selectedAnimationPageVisibilityAnimation);
      this.VisibilityStoryboard.Children.Add(selectedPageVisibilityAnimation);
      this.VisibilityStoryboard.Children.Add(previousAnimatedPageHeaderTitleVisibilityAnimation);
      this.VisibilityStoryboard.Children.Add(selectedAnimatedPageHeaderTitleVisibilityAnimation);
      this.VisibilityStoryboard.Freeze();
    }

    private void InitializeSwipeTranslateAnimations()
    {
      var selectedAnimatedPageTranslateAnimation = new DoubleAnimation(
        this.SelectedPageTranslationStartPosition,
        this.SelectedPageTranslationEndPosition,
        new Duration(TimeSpan.FromSeconds(0.3)))
      {
        BeginTime = TimeSpan.Zero,
        EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseInOut, Exponent = 6 }
      };

      var previousAnimatedPageTranslateAnimation = new DoubleAnimation(
        this.PreviousPageTranslationStartPosition,
        this.PreviousPageTranslationEndPosition,
        new Duration(TimeSpan.FromSeconds(0.3)))
      {
        BeginTime = TimeSpan.Zero,
        EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseInOut, Exponent = 6 }
      };

      var pageTitleTranslateAnimation = new DoubleAnimation(0, 
        this.NavigationDirection == PageNavigationDirection.Next ? this.PART_PageHeader.PART_PageHeaderHostPanel.ActualWidth * -1 : this.PART_PageHeader.PART_PageHeaderHostPanel.ActualWidth,
        new Duration(TimeSpan.FromSeconds(0.3)))
      {
        BeginTime = TimeSpan.Zero,
        EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseInOut, Exponent = 6 }
      };

      var pageTitleOpacityFadeOutAnimation = new DoubleAnimation(
        1,
        0,
        new Duration(TimeSpan.FromSeconds(0.2)))
      {
        BeginTime = TimeSpan.Zero,
        EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseInOut, Exponent = 6 }
      };
      var pageTitleOpacityFadeInAnimation = new DoubleAnimation(
        0,
        1,
        new Duration(TimeSpan.FromSeconds(0.4)))
      {
        BeginTime = TimeSpan.Zero,
        EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseInOut, Exponent = 6 }
      };


      Storyboard.SetTarget(selectedAnimatedPageTranslateAnimation, this.AnimatedSelectedPageHost);
      Storyboard.SetTargetProperty(
        selectedAnimatedPageTranslateAnimation,
        new PropertyPath("RenderTransform.(TranslateTransform.X)"));
      Storyboard.SetTarget(previousAnimatedPageTranslateAnimation, this.AnimatedPreviousPageHost);
      Storyboard.SetTargetProperty(
        previousAnimatedPageTranslateAnimation,
        new PropertyPath("RenderTransform.(TranslateTransform.X)"));

      Storyboard.SetTarget(pageTitleTranslateAnimation, this.AnimatedPreviousPageHeaderTitle);
      Storyboard.SetTargetProperty(
        pageTitleTranslateAnimation,
        new PropertyPath("RenderTransform.(TranslateTransform.X)"));

      Storyboard.SetTarget(pageTitleOpacityFadeOutAnimation, this.AnimatedPreviousPageHeaderTitle);
      Storyboard.SetTargetProperty(
        pageTitleOpacityFadeOutAnimation,
        new PropertyPath("Opacity"));

      Storyboard.SetTarget(pageTitleOpacityFadeInAnimation, this.PART_PageHeader.PART_Title);
      Storyboard.SetTargetProperty(
        pageTitleOpacityFadeInAnimation,
        new PropertyPath("Opacity"));

      this.TranslateStoryboard = new Storyboard { FillBehavior = FillBehavior.Stop };
      this.TranslateStoryboard.Children.Add(selectedAnimatedPageTranslateAnimation);
      this.TranslateStoryboard.Children.Add(previousAnimatedPageTranslateAnimation);
      this.TranslateStoryboard.Children.Add(pageTitleTranslateAnimation);
      this.TranslateStoryboard.Children.Add(pageTitleOpacityFadeOutAnimation);
      this.TranslateStoryboard.Children.Add(pageTitleOpacityFadeInAnimation);
      var beginStoryboard = new BeginStoryboard { Storyboard = this.TranslateStoryboard };
      //this.TranslateStoryboard.Completed += (sender, args) => this.TranslateStoryboard?.Stop();
      this.TranslateStoryboard.Freeze();
    }

    private void HandleTranslateSwipeAnimation()
    {
      this.TranslateStoryboard?.Begin();
    }

    private void HandleVisibilitySwipeAnimation()
    {
      this.VisibilityStoryboard.Begin();
    }


    #region Overrides of ItemsControl

    /// <inheritdoc />
    protected override DependencyObject GetContainerForItemOverride() => new BionicSwipePage();

    /// <inheritdoc />
    protected override bool IsItemItsOwnContainerOverride(object item) => item is BionicSwipePage;

    #endregion

    private void UpdateFrameTitleFromTitleMemberPath()
    {
      if (this.PART_PageHeader == null)
      {
        return;
      }

      if (string.IsNullOrWhiteSpace(this.TitleMemberPath))
      {
        this.PART_PageHeader.PART_Title.Visibility = Visibility.Collapsed;
        return;
      }

      var binding = new Binding
      {
        Source = this,
        Path = new PropertyPath(nameof(this.SelectedItem) + "." + this.TitleMemberPath)
      };
      this.PART_PageHeader.PART_Title.SetBinding(ContentControl.ContentProperty, binding);
      this.PART_PageHeader.PART_Title.Visibility = Visibility.Visible;
    }

    private void GenerateContainerForItemAt(int pageIndex)
    {
      if (pageIndex < 0)
      {
        return;
      }

      var generator = this.ItemContainerGenerator as IItemContainerGenerator;
      GeneratorPosition startPosition = generator.GeneratorPositionFromIndex(pageIndex);
      using (generator.StartAt(startPosition, GeneratorDirection.Forward, true))
      {
        DependencyObject itemContainer = generator.GenerateNext(out bool isNewlyGenerated);
        if (isNewlyGenerated)
        {
          generator.PrepareItemContainer(itemContainer);
        }
      }
    }

    private void GenerateContainersForItems()
    {
      if (this.Items.Count < 1)
      {
        return;
      }

      var generator = this.ItemContainerGenerator as IItemContainerGenerator;

      GeneratorPosition startPosition = generator.GeneratorPositionFromIndex(0);
      using (generator.StartAt(startPosition, GeneratorDirection.Forward, true))
      {
        for (int i = 0; i < this.Items.Count; i++)
        {
          DependencyObject itemContainer = generator.GenerateNext(out bool isNewlyGenerated);
          if (isNewlyGenerated)
          {
            generator.PrepareItemContainer(itemContainer);
          }
        }
      }
    }

    private void UpdateLeavingPage()
    {
      GenerateContainerForItemAt(this.PreviousSelectedIndex);
      this.PreviousPage = GetPageContainerAt(this.PreviousSelectedIndex);

      //Take a snapshot of the current frame header title element
      this.AnimatedPreviousPageHeaderTitle.Fill = new VisualBrush(this.PART_PageHeader.PART_PageHeaderHostPanel);
      //this.AnimatedPreviousPageHeaderTitle.Fill = Brushes.IndianRed;
    }

    private void UpdateSelectedPage()
    {
      GenerateContainerForItemAt(this.SelectedIndex);
      this.SelectedPage = GetPageContainerAt(this.SelectedIndex);
      this.PART_SelectedPageHost?.ContentTemplate?.LoadContent();
    }

    private BionicSwipePage GetPageContainerAt(int pageIndex)
    {
      // Check if the selected item is a BionicSwipePage (when added through XAML)
      var pageContainer = this.SelectedItem as BionicSwipePage;
      if (pageContainer == null)
      {
        // It is a data item, get its TabItem container
        pageContainer = this.ItemContainerGenerator.ContainerFromIndex(pageIndex) as BionicSwipePage;

        // Due to event leapfrogging, we may have the wrong container.
        // If so, re-fetch the right container using a more expensive method.
        // (BTW, the previous line will cause a debug assert in this case)  [Dev10 452711]
        if (pageContainer == null ||
            !ItemsControl.Equals(this.SelectedItem, this.ItemContainerGenerator.ItemFromContainer(pageContainer)))
        {
          pageContainer = this.ItemContainerGenerator.ContainerFromItem(this.SelectedItem) as BionicSwipePage;
        }
      }

      return pageContainer;
    }

    private BionicSwipePageFrameHeader PART_PageHeader { get; set; }
    private Shape AnimatedPreviousPageHost { get; set; }
    private Shape AnimatedSelectedPageHost { get; set; }

    private Shape AnimatedPreviousPageHeaderTitle { get; set; }

    //public static readonly DependencyProperty AnimatedPreviousPageHeaderTitleProperty = DependencyProperty.Register(
    //  "AnimatedPreviousPageHeaderTitle",
    //  typeof(Shape),
    //  typeof(BionicSwipePageFrame),
    //  new PropertyMetadata(default(Shape)));

    //public Shape AnimatedPreviousPageHeaderTitle { get { return (Shape)GetValue(BionicSwipePageFrame.AnimatedPreviousPageHeaderTitleProperty); } set { SetValue(BionicSwipePageFrame.AnimatedPreviousPageHeaderTitleProperty, value); } }

    private Panel PART_PageHostPanel { get; set; }

    private ContentPresenter PART_SelectedPageHost { get; set; }
    private new bool IsInitialized { get; set; }
    private Storyboard TranslateStoryboard { get; set; }
    private Storyboard VisibilityStoryboard { get; set; }
  }
}