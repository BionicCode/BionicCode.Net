//#if NET5_0_OR_GREATER
//using JetBrains.Annotations;
//#endif

namespace BionicCode.Controls.Net.Wpf
{
  #region Usings

  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.ComponentModel;
  using System.Globalization;
  using System.Linq;
  using System.Runtime.CompilerServices;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Controls.Primitives;
  using System.Windows.Input;
  using System.Windows.Media;
  using BionicCode.Utilities.Net.Wpf.Extensions;

  #endregion

  [TemplatePart(Name = "PART_ScrollHost", Type = typeof(Panel))]
  [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(CalendarEventItem))]
  [TemplateVisualState(GroupName = "CalendarView", Name = "ReplacingView")]
  [TemplateVisualState(GroupName = "CalendarView", Name = "ReplacedView")]
  [TemplateVisualState(GroupName = "CalendarView", Name = "Normal")]
  public class Calendar : ItemsControl, INotifyPropertyChanged
  {
    private const string ScrollHostPartName = "PART_ScrollHost";

    public static readonly RoutedUICommand SelectNextMonthViewRoutedCommand = new RoutedUICommand("Select the next calendar month view.", nameof(Calendar.SelectNextMonthViewRoutedCommand), typeof(Calendar));
    public static readonly RoutedUICommand SelectPreviousMonthViewRoutedCommand = new RoutedUICommand("Select the previous calendar month view.", nameof(Calendar.SelectPreviousMonthViewRoutedCommand), typeof(Calendar));
    public static readonly RoutedUICommand SelectTodayMonthViewRoutedCommand = new RoutedUICommand("Select the calendar month view that contains today.", nameof(Calendar.SelectPreviousMonthViewRoutedCommand), typeof(Calendar));

#region SpanningRequestedRoutedEvent

    public static readonly RoutedEvent SpanningRequestedRoutedEvent = EventManager.RegisterRoutedEvent(
      "SpanningRequested",
      RoutingStrategy.Bubble,
      typeof(SpanningRequestedRoutedEventHandler),
      typeof(Calendar));

    public event RoutedEventHandler SpanningRequested
    {
      add => AddHandler(Calendar.SpanningRequestedRoutedEvent, value);
      remove => RemoveHandler(Calendar.SpanningRequestedRoutedEvent, value);
    }

#endregion

    public static readonly RoutedEvent SelectedRoutedEvent = EventManager.RegisterRoutedEvent(
      "Selected",
      RoutingStrategy.Bubble,
      typeof(RoutedEventHandler),
      typeof(Calendar));

    public static readonly RoutedEvent PreviewSelectedRoutedEvent = EventManager.RegisterRoutedEvent(
      "PreviewSelected",
      RoutingStrategy.Tunnel,
      typeof(RoutedEventHandler),
      typeof(Calendar));

    public static readonly RoutedEvent UnselectedRoutedEvent = EventManager.RegisterRoutedEvent(
      "Unselected",
      RoutingStrategy.Bubble,
      typeof(RoutedEventHandler),
      typeof(Calendar));

    public static readonly RoutedEvent PreviewUnselectedRoutedEvent = EventManager.RegisterRoutedEvent(
      "PreviewUnselected",
      RoutingStrategy.Tunnel,
      typeof(RoutedEventHandler),
      typeof(Calendar));

    public static readonly DependencyProperty DayProperty = DependencyProperty.RegisterAttached(
      "Day",
      typeof(DateTime),
      typeof(Calendar),
      new FrameworkPropertyMetadata(
        default(DateTime)));

    public static DateTime GetDay(DependencyObject attachingElement) =>
      (DateTime)attachingElement.GetValue(Calendar.DayProperty);

    public static void SetDay(DependencyObject attachingElement, DateTime value) =>
      attachingElement.SetValue(Calendar.DayProperty, value);

#region IsToday attached property

    public static readonly DependencyProperty IsTodayProperty = DependencyProperty.RegisterAttached(
      "IsToday", typeof(bool), typeof(Calendar), new PropertyMetadata(default(bool)));

    public static void SetIsToday(DependencyObject attachingElement, bool value) =>   attachingElement.SetValue(Calendar.IsTodayProperty, value);

    public static bool GetIsToday(DependencyObject attachingElement) => (bool) attachingElement.GetValue(Calendar.IsTodayProperty);

#endregion

    public static readonly DependencyProperty DateColumnHeaderItemContainerStyleProperty =
      DependencyProperty.Register(
        "DateColumnHeaderItemContainerStyle",
        typeof(Style),
        typeof(Calendar),
        new PropertyMetadata(default(Style), Calendar.OnDateColumnHeaderItemContainerStyleChanged));

    public static readonly DependencyProperty DateHeaderItemTemplateProperty = DependencyProperty.Register(
      "DateHeaderItemTemplate",
      typeof(DataTemplate),
      typeof(Calendar),
      new PropertyMetadata(default(DataTemplate)));

    public static readonly DependencyProperty DateHeaderItemContainerStyleProperty = DependencyProperty.Register(
      "DateHeaderItemContainerStyle",
      typeof(Style),
      typeof(Calendar),
      new PropertyMetadata(default(Style)));

    public static readonly DependencyProperty DateHeaderItemTemplateSelectorProperty =
      DependencyProperty.Register(
        "DateHeaderItemTemplateSelector",
        typeof(DataTemplateSelector),
        typeof(Calendar),
        new PropertyMetadata(default(DataTemplateSelector)));

    public static readonly DependencyProperty CalendarSourceProperty = DependencyProperty.Register(
      "CalendarSource",
      typeof(System.Globalization.Calendar),
      typeof(Calendar),
      new PropertyMetadata(default(System.Globalization.Calendar)));

    public static readonly DependencyProperty GridColorProperty = DependencyProperty.Register(
      "GridColor",
      typeof(Brush),
      typeof(Calendar),
      new PropertyMetadata(Brushes.DimGray));

    public static readonly DependencyProperty GridThicknessProperty = DependencyProperty.Register(
      "GridThickness",
      typeof(double),
      typeof(Calendar),
      new PropertyMetadata(0.2));

#region FirstDayOfWeek dependency property

    public static readonly DependencyProperty FirstDayOfWeekProperty = DependencyProperty.Register(
      "FirstDayOfWeek",
      typeof(DayOfWeek),
      typeof(Calendar),
      new FrameworkPropertyMetadata(DayOfWeek.Sunday, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    public DayOfWeek FirstDayOfWeek { get => (DayOfWeek) GetValue(Calendar.FirstDayOfWeekProperty); set => SetValue(Calendar.FirstDayOfWeekProperty, value); }

#endregion FirstDayOfWeek dependency property

#region CurrentMonthView read-only dependency property
    protected static readonly DependencyPropertyKey CurrentMonthViewPropertyKey = DependencyProperty.RegisterReadOnly(
      "CurrentMonthView",
      typeof(CalendarMonthView),
      typeof(Calendar),
      new PropertyMetadata(default(CalendarMonthView)));

    public static readonly DependencyProperty CurrentMonthViewProperty = Calendar.CurrentMonthViewPropertyKey.DependencyProperty;

    public CalendarMonthView CurrentMonthView
    {
      get => (CalendarMonthView) GetValue(Calendar.CurrentMonthViewProperty);
      private set => SetValue(Calendar.CurrentMonthViewPropertyKey, value);
    }

#endregion CurrentMonthView read-only dependency property

#region IsNavigationBarVisible dependency property

    public static readonly DependencyProperty IsNavigationBarVisibleProperty = DependencyProperty.Register(
      "IsNavigationBarVisible",
      typeof(bool),
      typeof(Calendar),
      new PropertyMetadata(default));

    public bool IsNavigationBarVisible { get => (bool) GetValue(Calendar.IsNavigationBarVisibleProperty); set => SetValue(Calendar.IsNavigationBarVisibleProperty, value); }

#endregion IsNavigationBarVisible dependency property

#region IsTitleVisible dependency property

    public static readonly DependencyProperty IsTitleVisibleProperty = DependencyProperty.Register(
      "IsTitleVisible",
      typeof(bool),
      typeof(Calendar),
      new PropertyMetadata(default));

    public bool IsTitleVisible { get => (bool) GetValue(Calendar.IsTitleVisibleProperty); set => SetValue(Calendar.IsTitleVisibleProperty, value); }

#endregion IsTitleVisible dependency property

#region Today read-only dependency property
    protected static readonly DependencyPropertyKey TodayPropertyKey = DependencyProperty.RegisterReadOnly(
      "Today",
      typeof(DateTime),
      typeof(Calendar),
      new PropertyMetadata(DateTime.Today));

    public static readonly DependencyProperty TodayProperty = Calendar.TodayPropertyKey.DependencyProperty;

    public DateTime Today
    {
      get => (DateTime) GetValue(Calendar.TodayProperty);
      private set => SetValue(Calendar.TodayPropertyKey, value);
    }

#endregion Today read-only dependency property

#region Dependency properties

    private static void OnDateColumnHeaderItemContainerStyleChanged(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
      //var this_ = d as Calendar;
      //foreach (FrameworkElement itemContainer in this_.ItemContainerToItemMap.Keys
      //  .OfType<CalendarDateColumnHeaderItem>()
      //)
      //{
      //  itemContainer.Style = e.NewValue as Style;
      //}
    }

    //private static void OnDayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //{
    //  var oldCalendarDate = (DateTime)e.OldValue;
    //  var newCalendarDate = (DateTime)e.NewValue;

    //  if (d is CalendarEventItem eventItemContainer)
    //  {
    //    Calendar.IsItemsHostLayoutDirty = true;
    //    //if (!Calendar.Instance.ItemContainerToItemMap.ContainsKey(eventItemContainer))
    //    //{
    //    //  Calendar.Instance.ItemContainerToItemMap.Add(eventItemContainer, eventItemContainer.Content);
    //    //  Calendar.Instance.PrepareContainerForEventItemOverride(eventItemContainer, eventItemContainer.Content);
    //    //}


    //    //if (Calendar.DateToDateItemContainerMap.TryGetValue(
    //    //  oldCalendarDate.Date,
    //    //  out UIElement oldCalendarDateItem) && oldCalendarDateItem is HeaderedItemsControl oldHeaderedItemsControl)
    //    //{
    //    //  oldHeaderedItemsControl.Items.Remove(eventItemContainer);
    //    //}
    //    //if (Calendar.DateToDateItemContainerMap.TryGetValue(
    //    //  newCalendarDate.Date,
    //    //  out UIElement calendarDateItem) && calendarDateItem is HeaderedItemsControl headeredItemsControl)
    //    //{
    //    //  if (!headeredItemsControl.Items.Contains(eventItemContainer))
    //    //  {
    //    //    headeredItemsControl.Items.Add(eventItemContainer);
    //    //    Calendar.IsItemsHostLayoutDirty = true;
    //    //  }
    //    //}
    //  }

    //  //if (d is CalendarDateItem dateItemContainer)
    //  //{
    //  //  if (Calendar.DateItemContainerToDateMap.TryGetValue(dateItemContainer, out DateTime currentCalendarDate))
    //  //  {
    //  //    if (newCalendarDate.Date.Equals(currentCalendarDate.Date))
    //  //    {
    //  //      return;
    //  //    }

    //  //    //Calendar.UnregisterDateItemContainer(dateItemContainer);
    //  //  }

    //  //  Calendar.Instance.HandleTodayChanged();
    //  //  //Calendar.RegisterDateItemContainer(itemContainer);
    //  //  Calendar.IsItemsHostLayoutDirty = true;
    //  //}
    //}

    //private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //{
    //  (d as Calendar).OnItemsSourceChanged(e.OldValue as IEnumerable, e.NewValue as IEnumerable);
    //}

    //private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //{
    //  (d as Calendar).OnSelectedIndexChanged((int) e.OldValue, (int) e.NewValue);
    //}

    //private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //{
    //  (d as Calendar).OnSelectedItemChanged(e.OldValue, e.NewValue);
    //}

#endregion

#region

    private static double _transitionAnimationStart;

    public static double TransitionAnimationStart
    {
      get => Calendar._transitionAnimationStart;
      private set
      {
        Calendar._transitionAnimationStart = value;
        Calendar.OnGlobalPropertyChanged();
      }
    }

    private static double _transitionAnimationStop;

    public static double TransitionAnimationStop
    {
      get => Calendar._transitionAnimationStop;
      private set
      {
        Calendar._transitionAnimationStop = value;
        Calendar.OnGlobalPropertyChanged("TransitionAnimationStop");
      }
    }

    private static double TransitionAnimationLength { get; set; }
    private Action CompleteReplaceViewTransitioning { get; set; }
    private DependencyObject SelectedCalendarDate { get; set; }

    static Calendar()
    {
      FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
        typeof(Calendar),
        new FrameworkPropertyMetadata(typeof(Calendar)));
      Calendar.DateToDateItemContainerMap = new Dictionary<DateTime, UIElement>();
      Calendar.DateItemContainerToDateMap = new Dictionary<UIElement, DateTime>();
      Calendar.TransitionAnimationLength = 0;
      TransitionAnimationStop = 0;
      Calendar.TransitionAnimationStart = Calendar.TransitionAnimationLength;
    }

    public Calendar()
    {
      AddHandler(CalendarPanel.DatesRealizedRoutedEvent, new RoutedEventHandler(OnDatesRealized));
      AddHandler(CalendarPanel.CurrentViewChangedRoutedEvent, new CalendarViewChangedRoutedEventHandler<CalendarMonthView>(OnCalendarViewChanged));
      AddHandler(Calendar.SelectedRoutedEvent, new RoutedEventHandler(OnCalendarItemSelected));

      this.CommandBindings.Add(
        new CommandBinding(Calendar.SelectNextMonthViewRoutedCommand, ExecuteSelectNextMonthView));
      this.CommandBindings.Add(
        new CommandBinding(Calendar.SelectPreviousMonthViewRoutedCommand, ExecuteSelectPreviousMonthView));
      this.CommandBindings.Add(
        new CommandBinding(Calendar.SelectTodayMonthViewRoutedCommand, ExecuteSelectTodayMonthView));

      this.Loaded += OnLoaded;

      this.CalendarSource = new GregorianCalendar();
      this.ItemContainerToItemMap = new Dictionary<UIElement, object>();
      this.ItemToItemContainerMap = new Dictionary<object, UIElement>();
      this.DayChangeWatcher = new DayChangeWatcher();
      this.FirstDayOfWeek = DayOfWeek.Sunday;

      Initialize();
    }

    private void OnDatesRealized(object sender, RoutedEventArgs e) => HandleTodayChanged();

    private void OnCalendarItemSelected(object sender, RoutedEventArgs e)
    {
      if (e.OriginalSource is UIElement sourceItemContainer)
      {
        if (this.SelectedCalendarDate != null)
        {
          Selector.SetIsSelected(this.SelectedCalendarDate, false);
          RaiseEvent(new RoutedEventArgs(Selector.UnselectedEvent, this));
        }
        this.SelectedCalendarDate = sourceItemContainer;
      }
    }

    private void OnCalendarViewChanged(object sender, CalendarViewChangedRoutedEventArgs<CalendarMonthView> e)
    {
      this.CurrentMonthView = e.CurrentView;
      this.CompleteReplaceViewTransitioning?.Invoke();
    }

    private void ExecuteSelectTodayMonthView(object sender, ExecutedRoutedEventArgs e)
    {
      TransitionToReplacingWithNextViewState();
      this.CompleteReplaceViewTransitioning = TransitionToReplacedWithNextViewState;
      this.ItemsHost.ScrollDateIntoView(this.Today);
    }

    private void ExecuteSelectNextMonthView(object sender, ExecutedRoutedEventArgs e)
    {
      TransitionToReplacingWithNextViewState();
      this.CompleteReplaceViewTransitioning = TransitionToReplacedWithNextViewState;
      this.ScrollHost?.PageDown();
    }

    private void ExecuteSelectPreviousMonthView(object sender, ExecutedRoutedEventArgs e)
    {
      TransitionToReplacingWithPreviousViewState();
      this.CompleteReplaceViewTransitioning = TransitionToReplacedWithPreviousViewState;
      this.ScrollHost?.PageUp();
    }

    private void TransitionToReplacingWithNextViewState()
    {
      Calendar.TransitionAnimationLength = this.ActualWidth;
      Calendar.TransitionAnimationStart = 0;
      Calendar.TransitionAnimationStop = -Calendar.TransitionAnimationLength;
      VisualStateManager.GoToState(this, "ReplacingView", false);
    }

    private void TransitionToReplacedWithNextViewState()
    {
      Calendar.TransitionAnimationLength += 9000;
      Calendar.TransitionAnimationStart = Calendar.TransitionAnimationLength;
      Calendar.TransitionAnimationStop = 0;
      VisualStateManager.GoToState(this, "ReplacedView", false);
    }

    private void TransitionToReplacingWithPreviousViewState()
    {
      Calendar.TransitionAnimationLength = this.ActualWidth;
      Calendar.TransitionAnimationStart = 0;
      Calendar.TransitionAnimationStop = Calendar.TransitionAnimationLength;
      VisualStateManager.GoToState(this, "ReplacingView", false);
    }

    private void TransitionToReplacedWithPreviousViewState()
    {
      Calendar.TransitionAnimationStart = 0;
      Calendar.TransitionAnimationStop = Calendar.TransitionAnimationLength;
      VisualStateManager.GoToState(this, "ReplacedView", false);
    }

#endregion
    

#region Overrides of FrameworkElement

    private void Initialize()
    {
      // TODO::Remove
      int daysInMonth = this.CalendarSource.GetDaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
      var firstDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
      var events = new ObservableCollection<CalendarEvent>();
      for (var day = 0; day < daysInMonth; day++)
      {
        DateTime currentDay = firstDate.AddDays(day);

        for (var count = 0; count < 2; count++)
        {
          var calendarEvent = new CalendarEvent
          {
            Start = new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, 14, 30 + count, 0),
            Stop = new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, 15, count, 0),
            Summary = $"{currentDay.ToShortDateString()}/#{count} This is some event data"
          };
          events.Add(calendarEvent);
        }
      }

      this.ItemsSource = events;

      this.DayChangeWatcher.CalendarDayChanged += OnTodayChanged;
      this.DayChangeWatcher.Start();
    }

    private void OnTodayChanged(object sender, EventArgs e) => HandleTodayChanged();

    private void HandleTodayChanged()
    {
      this.Today = DateTime.Today;

      if (this.ItemsHost == null)
      {
        return;
      }

      if (this.ItemsHost.TryGetDateItem(DateTime.Today.Subtract(TimeSpan.FromDays(1)), out CalendarDate yesterdayCalendarDateItem))
      {
        yesterdayCalendarDateItem.IsToday = false;
      }

      if (this.ItemsHost.TryGetDateItem(DateTime.Today, out CalendarDate calendarDateItem))
      {
        calendarDateItem.IsToday = true;
      }
    }

#endregion

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      this.ScrollHost = GetTemplateChild(Calendar.ScrollHostPartName) as ScrollViewer;
      if (this.ScrollHost != null)
      {
        InitializeScrollHost();
      }
    }

    private void InitializeScrollHost()
    {
      this.ScrollHost.CanContentScroll = true;
      this.ScrollHost.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
      this.ScrollHost.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
      this.ScrollHost.RenderTransform = new TranslateTransform();
    }

    //protected virtual void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    //{
    //  if (oldValue is INotifyCollectionChanged oldObservableCollection)
    //  {
    //    oldObservableCollection.CollectionChanged -= OnItemsSourceCollectionChanged;
    //  }

    //  //if (oldValue != null)
    //  //{
    //  //  RemoveOlEventItems(oldValue);
    //  //}

    //  this.ItemsHost.ClearEventChildren();

    //  if (newValue == null)
    //  {
    //    return;
    //  }

    //  this.Items = CollectionViewSource.GetDefaultView(newValue) as CollectionView;

    //  if (newValue is INotifyCollectionChanged newObservableCollection)
    //  {
    //    newObservableCollection.CollectionChanged += OnItemsSourceCollectionChanged;
    //  }


    //  this.ItemsHost.AddEventChildren(newValue.Cast<object>());
    //}

    public void ClearItemContainer(UIElement itemContainer)
    {
      switch (itemContainer)
      {
        case HeaderedItemsControl headeredItemsControl:
          headeredItemsControl.Header = null;
          headeredItemsControl.DataContext = null;
          break;
        case ContentControl contentControl:
          contentControl.Content = null;
          contentControl.DataContext = null;
          break;
        case Control control:
          control.DataContext = null;
          break;
      }
      //UnregisterItemContainer(itemContainer);
    }

    //private void UnregisterItemContainer(UIElement itemContainer, object item = null)
    //{
    //  if (item == null)
    //  {
    //    this.ItemContainerToItemMap.TryGetValue(itemContainer, out item);
    //  }
    //  this.ItemContainerToItemMap.Remove(itemContainer);
    //  this.ItemToItemContainerMap.Remove(item);
    //}

    //private void RegisterItemContainer(UIElement itemContainer, object item)
    //{
    //  this.ItemContainerToItemMap.Add(itemContainer, item);
    //  this.ItemToItemContainerMap.Add(item, itemContainer);
    //}

    private void OnCalendarEventItemSelected(object sender, RoutedEventArgs routedEventArgs)
    {
      //foreach (UIElement itemsHostChild in this.ItemsHost.GetEventItemContainers())
      //{
      //  if (!object.ReferenceEquals(itemsHostChild, routedEventArgs.OriginalSource)
      //      && Selector.GetIsSelected(itemsHostChild))
      //  {
      //    Selector.SetIsSelected(itemsHostChild, false);
      //    RaiseEvent(new RoutedEventArgs(Selector.UnselectedEvent, this));
      //  }
      //}

      ////this.ItemsHost.get
      //switch (routedEventArgs.OriginalSource)
      //{
      //  case HeaderedItemsControl headeredItemsControl:
      //    this.SelectedItem = headeredItemsControl.Header;
      //    break;
      //  case ContentControl contentControl:
      //    this.SelectedItem = contentControl.Content;
      //    break;
      //  case FrameworkElement frameworkElement:
      //    this.SelectedItem = frameworkElement.DataContext;
      //    break;
      //}


      //RaiseEvent(new RoutedEventArgs(Selector.SelectedEvent, this));
    }
    
    public virtual UIElement GetContainerForDateItem() => new CalendarDateItem();
    public virtual UIElement GetContainerForWeekHeaderItem() => new WeekHeaderItem();
    public virtual UIElement GetContainerForDateColumnHeaderItem() => new CalendarDateColumnHeaderItem();

#region Overrides of ItemsControl

    /// <inheritdoc />
    protected override DependencyObject GetContainerForItemOverride() => new CalendarEventItem();

    /// <inheritdoc />
    protected override bool IsItemItsOwnContainerOverride(object item) => item is CalendarEventItem;

#endregion

    protected virtual bool IsDateItemItsOwnContainerOverride(object item) => item is CalendarDateItem;
    protected virtual bool IsWeekHeaderItemItsOwnContainerOverride(object item) => item is WeekHeaderItem;

    public virtual void PrepareContainerForWeekHeaderItemOverride(DependencyObject element, object item)
    {
      switch (element)
      {
        case HeaderedItemsControl headeredItemsControl:
          headeredItemsControl.Header = item;
          headeredItemsControl.DataContext = item;
          break;
        case ContentControl contentControl:
          contentControl.Content = item;
          contentControl.DataContext = item;
          break;
        case Control control:
          control.DataContext = item;
          break;
      }
    }

    public virtual void PrepareContainerForCalendarDateItemOverride(DependencyObject element, object item)
    {
      switch (element)
      {
        case HeaderedItemsControl headeredItemsControl:
          headeredItemsControl.ItemContainerStyle = this.DateHeaderItemContainerStyle;
          headeredItemsControl.ItemTemplate = this.DateHeaderItemTemplate;
          headeredItemsControl.ItemTemplateSelector = this.DateHeaderItemTemplateSelector;
          headeredItemsControl.Style = this.DateHeaderItemContainerStyle;
          headeredItemsControl.HeaderTemplate = this.DateHeaderItemTemplate;
          headeredItemsControl.HeaderTemplateSelector = this.DateHeaderItemTemplateSelector;
          headeredItemsControl.Header = item;
          headeredItemsControl.DataContext = item;
          break;
        case ContentControl contentControl:
          contentControl.Style = this.DateHeaderItemContainerStyle;
          contentControl.ContentTemplate = this.DateHeaderItemTemplate;
          contentControl.ContentTemplateSelector = this.DateHeaderItemTemplateSelector;
          contentControl.Content = item;
          contentControl.DataContext = item;
          break;
        case Control control:
          control.Style = this.DateHeaderItemContainerStyle;
          control.DataContext = item;
          break;
      }
    }

    public virtual void PrepareContainerForCalendarDateColumnHeaderItemOverride(
      DependencyObject element,
      object item)
    {
      switch (element)
      {
        case HeaderedItemsControl headeredItemsControl:
          headeredItemsControl.Style = this.DateColumnHeaderItemContainerStyle;
          headeredItemsControl.Header = item;
          headeredItemsControl.DataContext = item;
          break;
        case ContentControl contentControl:
          contentControl.Style = this.DateColumnHeaderItemContainerStyle;
          contentControl.Content = item;
          contentControl.DataContext = item;
          break;
        case Control control:
          control.Style = this.DateColumnHeaderItemContainerStyle;
          control.DataContext = item;
          break;
      }
    }

   private void OnLoaded(object sender, RoutedEventArgs e)
    {
      VisualStateManager.GoToState(this, "Normal", false);
      
      if (this.TryFindVisualChildElement(out CalendarPanel hostPanel))
      {
        this.ItemsHost = hostPanel;
        ObserveItemsHost(hostPanel);
      }

      HandleTodayChanged();
    }

    private void ObserveItemsHost(CalendarPanel hostPanel)
    {
      hostPanel.AutoGeneratingDate += OnItemsHostIsAutoGeneratingDate;
      hostPanel.AutoGeneratingDateColumnHeader += OnItemsHostIsAutoGeneratingDateColumnHeader;
    }

    private void OnItemsHostIsAutoGeneratingDateColumnHeader(object sender, DateColumnHeaderGeneratorArgs e) => OnAutogeneratingDateColumnHeader(e);

    private void OnItemsHostIsAutoGeneratingDate(object sender, DateGeneratorArgs e) => OnAutogeneratingDate(e);

    protected virtual void OnAutogeneratingDate(DateGeneratorArgs e) => this.AutogeneratingDate?.Invoke(this, e);

    protected virtual void OnAutogeneratingDateColumnHeader(DateColumnHeaderGeneratorArgs e) =>
      this.AutogeneratingDateColumnHeader?.Invoke(this, e);

    public event EventHandler<DateGeneratorArgs> AutogeneratingDate;
    public event EventHandler<DateColumnHeaderGeneratorArgs> AutogeneratingDateColumnHeader;

    public event RoutedEventHandler Selected
    {
      add => AddHandler(Calendar.SelectedRoutedEvent, value);
      remove => RemoveHandler(Calendar.SelectedRoutedEvent, value);
    }

    public event RoutedEventHandler PreviewSelected
    {
      add => AddHandler(Calendar.PreviewSelectedRoutedEvent, value);
      remove => RemoveHandler(Calendar.PreviewSelectedRoutedEvent, value);
    }

    public event RoutedEventHandler Unselected
    {
      add => AddHandler(Calendar.UnselectedRoutedEvent, value);
      remove => RemoveHandler(Calendar.UnselectedRoutedEvent, value);
    }

    public event RoutedEventHandler PreviewUnselected
    {
      add => AddHandler(Calendar.PreviewUnselectedRoutedEvent, value);
      remove => RemoveHandler(Calendar.PreviewUnselectedRoutedEvent, value);
    }

    //public IEnumerable ItemsSource
    //{
    //  get => (IEnumerable) GetValue(Calendar.ItemsSourceProperty);
    //  set => SetValue(Calendar.ItemsSourceProperty, value);
    //}

    //public CollectionView Items
    //{
    //  get => (CollectionView) GetValue(Calendar.ItemsProperty);
    //  set => SetValue(Calendar.ItemsProperty, value);
    //}

    //public Style ItemContainerStyle
    //{
    //  get => (Style) GetValue(Calendar.ItemContainerStyleProperty);
    //  set => SetValue(Calendar.ItemContainerStyleProperty, value);
    //}

    //public DataTemplate ItemTemplate
    //{
    //  get => (DataTemplate) GetValue(Calendar.ItemTemplateProperty);
    //  set => SetValue(Calendar.ItemTemplateProperty, value);
    //}

    //public DataTemplateSelector ItemTemplateSelector
    //{
    //  get => (DataTemplateSelector) GetValue(Calendar.ItemTemplateSelectorProperty);
    //  set => SetValue(Calendar.ItemTemplateSelectorProperty, value);
    //}

    //public object SelectedItem
    //{
    //  get => GetValue(Calendar.SelectedItemProperty);
    //  set => SetValue(Calendar.SelectedItemProperty, value);
    //}

    //public int SelectedIndex
    //{
    //  get => (int) GetValue(Calendar.SelectedIndexProperty);
    //  set => SetValue(Calendar.SelectedIndexProperty, value);
    //}

    public Brush GridColor
    {
      get => (Brush) GetValue(Calendar.GridColorProperty);
      set => SetValue(Calendar.GridColorProperty, value);
    }

    public double GridThickness
    {
      get => (double) GetValue(Calendar.GridThicknessProperty);
      set => SetValue(Calendar.GridThicknessProperty, value);
    }

    public DataTemplate DateHeaderItemTemplate
    {
      get => (DataTemplate) GetValue(Calendar.DateHeaderItemTemplateProperty);
      set => SetValue(Calendar.DateHeaderItemTemplateProperty, value);
    }

    public Style DateHeaderItemContainerStyle
    {
      get => (Style) GetValue(Calendar.DateHeaderItemContainerStyleProperty);
      set => SetValue(Calendar.DateHeaderItemContainerStyleProperty, value);
    }

    public DataTemplateSelector DateHeaderItemTemplateSelector
    {
      get => (DataTemplateSelector) GetValue(Calendar.DateHeaderItemTemplateSelectorProperty);
      set => SetValue(Calendar.DateHeaderItemTemplateSelectorProperty, value);
    }

    public System.Globalization.Calendar CalendarSource
    {
      get => (System.Globalization.Calendar) GetValue(Calendar.CalendarSourceProperty);
      set => SetValue(Calendar.CalendarSourceProperty, value);
    }

    //public DataTemplateSelector DateColumnHeaderDataTemplateSelector
    //{
    //    get => (DataTemplateSelector) GetValue(Calendar.DateColumnHeaderDataTemplateSelectorProperty);
    //    set => SetValue(Calendar.DateColumnHeaderDataTemplateSelectorProperty, value);
    //}

    public Style DateColumnHeaderItemContainerStyle
    {
      get => (Style) GetValue(Calendar.DateColumnHeaderItemContainerStyleProperty);
      set => SetValue(Calendar.DateColumnHeaderItemContainerStyleProperty, value);
    }

    //public DataTemplate DateColumnHeaderItemTemplate
    //{
    //    get => (DataTemplate) GetValue(Calendar.DateColumnHeaderItemTemplateProperty);
    //    set => SetValue(Calendar.DateColumnHeaderItemTemplateProperty, value);
    //}


    private Dictionary<UIElement, object> ItemContainerToItemMap { get; }
    private Dictionary<object, UIElement> ItemToItemContainerMap { get; }
    public static Dictionary<DateTime, UIElement> DateToDateItemContainerMap { get; }
    private static Dictionary<UIElement, DateTime> DateItemContainerToDateMap { get; }

    private static bool IsItemsHostLayoutDirty { get; set; }

    private DayChangeWatcher DayChangeWatcher { get; }
    private ScrollViewer ScrollHost { get; set; }
    private CalendarPanel ItemsHost { get; set; }
    public static event PropertyChangedEventHandler GlobalPropertyChanged;
    public  event PropertyChangedEventHandler PropertyChanged;

//#if NET5_0_OR_GREATER
//    [NotifyPropertyChangedInvocator]
//#endif
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected static void OnGlobalPropertyChanged([CallerMemberName] string propertyName = null)
    {
      Calendar.GlobalPropertyChanged?.Invoke(typeof(Calendar), new PropertyChangedEventArgs(propertyName));
    }
  }
}