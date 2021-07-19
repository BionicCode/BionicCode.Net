#region Info

// 2020/11/04  13:19
// Activitytracker

#endregion

#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using BionicCode.Utilities.Net.Framework.Wpf.Extensions;

#endregion

namespace BionicCode.Controls.Net.Framework.Wpf.BionicCalendar
{
  [TemplatePart(Name = "PART_ScrollHost", Type = typeof(Panel))]
  [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(CalendarEventItem))]
  public class Calendar : ItemsControl
  {
    private const string ScrollHostPartName = "PART_ScrollHost";

    public static readonly RoutedUICommand SelectNextMonthViewRoutedCommand = new RoutedUICommand("Select the next calendar month view.", nameof(Calendar.SelectNextMonthViewRoutedCommand), typeof(Calendar));

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
        default(DateTime),
        Calendar.OnDayChanged));

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


    //public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
    //  "ItemsSource",
    //  typeof(IEnumerable),
    //  typeof(Calendar),
    //  new PropertyMetadata(default(IEnumerable), Calendar.OnItemsSourceChanged));

    //public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
    //  "Items",
    //  typeof(CollectionView),
    //  typeof(Calendar),
    //  new PropertyMetadata(default(CollectionView)));

    //public static readonly DependencyProperty ItemContainerStyleProperty = DependencyProperty.Register(
    //  "ItemContainerStyle",
    //  typeof(Style),
    //  typeof(Calendar),
    //  new PropertyMetadata(default(Style)));

    //public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
    //  "ItemTemplate",
    //  typeof(DataTemplate),
    //  typeof(Calendar),
    //  new PropertyMetadata(default(DataTemplate)));

    //public static readonly DependencyProperty ItemTemplateSelectorProperty = DependencyProperty.Register(
    //  "ItemTemplateSelector",
    //  typeof(DataTemplateSelector),
    //  typeof(Calendar),
    //  new PropertyMetadata(default(DataTemplateSelector)));

    //public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
    //  "SelectedItem",
    //  typeof(object),
    //  typeof(Calendar),
    //  new PropertyMetadata(default, Calendar.OnSelectedItemChanged));

    //public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
    //  "SelectedIndex",
    //  typeof(int),
    //  typeof(Calendar),
    //  new PropertyMetadata(default(int), Calendar.OnSelectedIndexChanged));

    public static readonly DependencyProperty TodayItemIndexProperty = DependencyProperty.Register(
      "TodayItemIndex",
      typeof(int),
      typeof(Calendar),
      new PropertyMetadata(default(int)));

    public int TodayItemIndex
    {
      get => (int) GetValue(Calendar.TodayItemIndexProperty);
      set => SetValue(Calendar.TodayItemIndexProperty, value);
    }

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

    #region Dependency properties

    private static void OnDateColumnHeaderItemContainerStyleChanged(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
      var this_ = d as Calendar;
      foreach (FrameworkElement itemContainer in this_.ItemContainerToItemMap.Keys
        .OfType<CalendarDateColumnHeaderItem>()
      )
      {
        itemContainer.Style = e.NewValue as Style;
      }
    }

    private static void OnDayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var oldCalendarDate = (DateTime)e.OldValue;
      var newCalendarDate = (DateTime)e.NewValue;

      if (d is CalendarEventItem eventItemContainer)
      {
        Calendar.IsItemsHostLayoutDirty = true;
        //if (!Calendar.Instance.ItemContainerToItemMap.ContainsKey(eventItemContainer))
        //{
        //  Calendar.Instance.ItemContainerToItemMap.Add(eventItemContainer, eventItemContainer.Content);
        //  Calendar.Instance.PrepareContainerForEventItemOverride(eventItemContainer, eventItemContainer.Content);
        //}


        //if (Calendar.DateToDateItemContainerMap.TryGetValue(
        //  oldCalendarDate.Date,
        //  out UIElement oldCalendarDateItem) && oldCalendarDateItem is HeaderedItemsControl oldHeaderedItemsControl)
        //{
        //  oldHeaderedItemsControl.Items.Remove(eventItemContainer);
        //}
        //if (Calendar.DateToDateItemContainerMap.TryGetValue(
        //  newCalendarDate.Date,
        //  out UIElement calendarDateItem) && calendarDateItem is HeaderedItemsControl headeredItemsControl)
        //{
        //  if (!headeredItemsControl.Items.Contains(eventItemContainer))
        //  {
        //    headeredItemsControl.Items.Add(eventItemContainer);
        //    Calendar.IsItemsHostLayoutDirty = true;
        //  }
        //}
      }

      //if (d is CalendarDateItem dateItemContainer)
      //{
      //  if (Calendar.DateItemContainerToDateMap.TryGetValue(dateItemContainer, out DateTime currentCalendarDate))
      //  {
      //    if (newCalendarDate.Date.Equals(currentCalendarDate.Date))
      //    {
      //      return;
      //    }

      //    //Calendar.UnregisterDateItemContainer(dateItemContainer);
      //  }

      //  Calendar.Instance.HandleTodayChanged();
      //  //Calendar.RegisterDateItemContainer(itemContainer);
      //  Calendar.IsItemsHostLayoutDirty = true;
      //}
    }

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

    private bool canVerticallyScroll;
    private bool canHorizontallyScroll;
    private double extentWidth;
    private double extentHeight;
    private double viewportWidth;
    private double viewportHeight;
    private double horizontalOffset;
    private double verticalOffset;
    private ScrollViewer scrollOwner;

    static Calendar()
    {
      FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
        typeof(Calendar),
        new FrameworkPropertyMetadata(typeof(Calendar)));
      Calendar.DateToDateItemContainerMap = new Dictionary<DateTime, UIElement>();
      Calendar.DateItemContainerToDateMap = new Dictionary<UIElement, DateTime>();
    }

    public Calendar()
    {
      AddHandler(Calendar.SelectedRoutedEvent, new RoutedEventHandler(OnItemSelected));
      AddHandler(Calendar.SelectedRoutedEvent, new RoutedEventHandler(OnItemSelected));
      AddHandler(Calendar.SpanningRequestedRoutedEvent, new SpanningRequestedRoutedEventHandler(SpanEventItemOnSpanningRequested));
      AddHandler(CalendarPanel.CurrentViewChangedRoutedEvent, new CalendarViewChangedRoutedEventHandler<CalendarMonthView>(OnCalendarViewChanged));

      this.CommandBindings.Add(
        new CommandBinding(Calendar.SelectNextMonthViewRoutedCommand, ExecuteSelectNextMonthView));

      this.Loaded += OnLoaded;

      this.CalendarSource = new GregorianCalendar();
      this.ItemContainerToItemMap = new Dictionary<UIElement, object>();
      this.ItemToItemContainerMap = new Dictionary<object, UIElement>();
      this.DayChangeWatcher = new DayChangeWatcher();
      this.FirstDayOfWeek = DayOfWeek.Sunday;

      Initialize();
    }

    private void OnCalendarViewChanged(object sender, CalendarViewChangedEventArgs<CalendarMonthView> e)
    {
      this.CurrentMonthView = e.CurrentView;
    }

    private void ExecuteSelectNextMonthView(object sender, ExecutedRoutedEventArgs e) => this.ScrollHost.PageDown();

    private void SpanEventItemOnSpanningRequested(object sender, EventItemDragDropArgs e)
    {
      ;
    }

    private void OnItemSelected(object sender, RoutedEventArgs e)
    {
      if (e.OriginalSource is CalendarDateItem)
      {
        OnCalendarDateItemSelected(sender, e);
      }
      else if (e.OriginalSource is CalendarEventItem)
      {
        OnCalendarEventItemSelected(sender, e);
      }
    }

    #endregion
    

    #region Overrides of FrameworkElement

    protected virtual void Initialize()
    {
      // TODO::Remove
      int daysInMonth = this.CalendarSource.GetDaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
      var firstDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
      var events = new ObservableCollection<ICalendarEvent>();
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

      //this.ItemsSource = events;
      //this.ItemsHost.InvalidateMeasure();
      
      //this.DayChangeWatcher.CalendarDayChanged += OnTodayChanged;
      this.DayChangeWatcher.Start();
    }

    private void OnTodayChanged(object sender, EventArgs e)
    {
      HandleTodayChanged();
    }

    private void HandleTodayChanged()
    {
      //if (this.TodayItemIndex < 0)
      //{
      //  if (!Calendar.DateToDateItemContainerMap.TryGetValue(DateTime.Today, out UIElement todayCalendarDateItem))
      //  {
      //    return;
      //  }

      //  Calendar.SetIsToday(todayCalendarDateItem, true);

      //  if (this.ItemContainerToItemMap.TryGetValue(todayCalendarDateItem, out object todayItem))
      //  {
      //    this.TodayItemIndex = this.DateHeaderItems.IndexOf(todayItem);
      //  }

      //  return;
      //}

      //if (this.ItemsHost.TryGetDateItem(DateTime.Today.Subtract(TimeSpan.FromDays(1)), out CalendarDate yesterdayItem) 
      //    && this.ItemToItemContainerMap.TryGetValue(yesterdayItem, out UIElement yesterdayItemContainer))
      //{
      //  Calendar.SetIsToday(yesterdayItemContainer, false);
      //}

      //if (this.ItemsHost.TryGetDateItem(DateTime.Today, out CalendarDate todayItem)
      //    && this.ItemToItemContainerMap.TryGetValue(todayItem, out UIElement todayItemContainer))
      //{
      //  Calendar.SetIsToday(todayItemContainer, true);

      //  this.TodayItemIndex++;

      //  //// If this.DateHeaderItemsSource is sorted by date, then the next item is the today item
      //  //object todayItemByIndex = this.DateHeaderItems.GetItemAt(this.TodayItemIndex);
      //  //if (this.ItemContainerToItemMap.TryGetValue(todayItemContainer, out object todayItem))
      //  //{
      //  //  if (object.ReferenceEquals(todayItem, todayItemByIndex))
      //  //  {
      //  //    return;
      //  //  }

      //  //  this.TodayItemIndex = this.DateHeaderItems.IndexOf(todayItem);
      //  //  return;
      //  //}
        return;
      //}

      //this.TodayItemIndex = -1;
    }

    #endregion

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      this.ScrollHost = GetTemplateChild(Calendar.ScrollHostPartName) as ScrollViewer;
      if (this.ScrollHost == null)
      {
        throw new ArgumentNullException(Calendar.ScrollHostPartName,
          $"Template part '{Calendar.ScrollHostPartName}' of type '{nameof(ScrollViewer)}' not found in template.");
      }
      InitializeScrollHost();
    }

    private void InitializeScrollHost()
    {
      this.ScrollHost.CanContentScroll = true;
      this.ScrollHost.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
      this.ScrollHost.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
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

    private void OnCalendarDateItemSelected(object sender, RoutedEventArgs routedEventArgs)
    {
      //foreach (UIElement itemsHostChild in this.ItemsHost.GetDateItemContainers())
      //{
      //  if (!object.ReferenceEquals(itemsHostChild, routedEventArgs.OriginalSource)
      //      && Selector.GetIsSelected(itemsHostChild))
      //  {
      //    Selector.SetIsSelected(itemsHostChild, false);
      //    RaiseEvent(new RoutedEventArgs(Selector.UnselectedEvent, this));
      //  }
      //}

      RaiseEvent(new RoutedEventArgs(Selector.SelectedEvent, this));
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

    public UIElement GetContainerFromItem(object item) =>
      this.ItemToItemContainerMap.TryGetValue(item, out UIElement itemContainer) ? itemContainer : null;

    public object GetItemFromContainer(UIElement itemContainer) =>
      this.ItemContainerToItemMap.TryGetValue(itemContainer, out object item) ? item : null;

    #region Overrides of Control

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size arrangeBounds)
    {
      if (Calendar.IsItemsHostLayoutDirty)
      {
        //this.ItemsHost?.InvalidateArrange();
        Calendar.IsItemsHostLayoutDirty = false;
      }
      base.ArrangeOverride(arrangeBounds);

      return arrangeBounds;
    }

    #endregion

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      if (this.TryFindVisualChildElement(out CalendarPanel hostPanel))
      {
        ObserveItemsHost(hostPanel);
      }
    }

    private void ObserveItemsHost(CalendarPanel hostPanel)
    {
      hostPanel.AutogeneratingDate += OnItemsHostIsAutogeneratingDate;
      hostPanel.AutogeneratingDateColumnHeader += OnItemsHostIsAutogeneratingDateColumnHeader;
    }

    private void OnItemsHostIsAutogeneratingDateColumnHeader(object sender, DateColumnHeaderGeneratorArgs e) => OnAutogeneratingDateColumnHeader(e);

    private void OnItemsHostIsAutogeneratingDate(object sender, DateGeneratorArgs e) => OnAutogeneratingDate(e);

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
  }
}