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
using System.Windows.Media;

#endregion

namespace BionicCode.Controls.Net.Framework.Wpf.BionicCalendar
{
  [TemplatePart(Name = "PART_ItemsHost", Type = typeof(Panel))]
  [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(CalendarEventItem))]
  public class Calendar : Control
  {
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
        FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
        Calendar.OnDayChanged));

    #region IsToday attached property

    public static readonly DependencyProperty IsTodayProperty = DependencyProperty.RegisterAttached(
      "IsToday", typeof(bool), typeof(Calendar), new PropertyMetadata(default(bool)));

    public static void SetIsToday(DependencyObject attachingElement, bool value) =>   attachingElement.SetValue(Calendar.IsTodayProperty, value);

    public static bool GetIsToday(DependencyObject attachingElement) => (bool) attachingElement.GetValue(Calendar.IsTodayProperty);

    #endregion

    private static void OnDayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    { 
      var oldCalendarDate = (DateTime) e.OldValue;
      var newCalendarDate = (DateTime) e.NewValue;

      if (d is CalendarEventItem eventItemContainer)
      {
        //Calendar.IsItemsHostLayoutDirty = true;
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

      if (d is CalendarDateItem dateItemContainer)
      {
        if (Calendar.DateItemContainerToDateMap.TryGetValue(dateItemContainer, out DateTime currentCalendarDate))
        {
          if (newCalendarDate.Date.Equals(currentCalendarDate.Date))
          {
            return;
          }

          Calendar.UnregisterDateItemContainer(dateItemContainer);
        }
        
        Calendar.Instance.HandleTodayChanged();
        Calendar.RegisterDateItemContainer(dateItemContainer);
        Calendar.IsItemsHostLayoutDirty = true;
      }
    }


    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
      "ItemsSource",
      typeof(IEnumerable),
      typeof(Calendar),
      new PropertyMetadata(default(IEnumerable), Calendar.OnItemsSourceChanged));

    public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
      "Items",
      typeof(CollectionView),
      typeof(Calendar),
      new PropertyMetadata(default(CollectionView)));

    public static readonly DependencyProperty ItemContainerStyleProperty = DependencyProperty.Register(
      "ItemContainerStyle",
      typeof(Style),
      typeof(Calendar),
      new PropertyMetadata(default(Style)));

    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
      "ItemTemplate",
      typeof(DataTemplate),
      typeof(Calendar),
      new PropertyMetadata(default(DataTemplate)));

    public static readonly DependencyProperty ItemTemplateSelectorProperty = DependencyProperty.Register(
      "ItemTemplateSelector",
      typeof(DataTemplateSelector),
      typeof(Calendar),
      new PropertyMetadata(default(DataTemplateSelector)));

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
      "SelectedItem",
      typeof(object),
      typeof(Calendar),
      new PropertyMetadata(default, Calendar.OnSelectedItemChanged));

    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
      "SelectedIndex",
      typeof(int),
      typeof(Calendar),
      new PropertyMetadata(default(int), Calendar.OnSelectedIndexChanged));

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

    public static readonly DependencyProperty DateHeaderItemsSourceProperty = DependencyProperty.Register(
      "DateHeaderItemsSource",
      typeof(IEnumerable),
      typeof(Calendar),
      new PropertyMetadata(default(IEnumerable), Calendar.OnDateHeaderItemsSourceChanged));

    public static readonly DependencyProperty DateHeaderItemsProperty = DependencyProperty.Register(
      "DateHeaderItems",
      typeof(CollectionView),
      typeof(Calendar),
      new PropertyMetadata(default(CollectionView)));

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
      new PropertyMetadata(default));

    public DayOfWeek FirstDayOfWeek { get => (DayOfWeek) GetValue(Calendar.FirstDayOfWeekProperty); set => SetValue(Calendar.FirstDayOfWeekProperty, value); }

    #endregion FirstDayOfWeek dependency property

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

    public static DateTime GetDay(DependencyObject attachingElement) =>
      (DateTime) attachingElement.GetValue(Calendar.DayProperty);

    public static void SetDay(DependencyObject attachingElement, DateTime value) =>
      attachingElement.SetValue(Calendar.DayProperty, value);

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as Calendar).OnItemsSourceChanged(e.OldValue as IEnumerable, e.NewValue as IEnumerable);
    }

    private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as Calendar).OnSelectedIndexChanged((int) e.OldValue, (int) e.NewValue);
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as Calendar).OnSelectedItemChanged(e.OldValue, e.NewValue);
    }

    #endregion

    #region

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
      Calendar.Instance = this;
      AddHandler(Calendar.SelectedRoutedEvent, new RoutedEventHandler(OnItemSelected));
      AddHandler(Calendar.SelectedRoutedEvent, new RoutedEventHandler(OnItemSelected));
      AddHandler(Calendar.SpanningRequestedRoutedEvent, new SpanningRequestedRoutedEventHandler(SpanEventItemOnSpanningRequested));
      this.Loaded += OnLoaded;

      this.CalendarSource = new GregorianCalendar();
      this.ItemContainerToItemMap = new Dictionary<UIElement, object>();
      this.ItemToItemContainerMap = new Dictionary<object, UIElement>();
      this.DayChangeWatcher = new DayChangeWatcher();
    }

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
      CreateDateColumnHeaderItems();

      if (this.DateHeaderItemsSource != null)
      {
        return;
      }
      var dates = new List<ICalendarDate>();
      int daysInMonth = this.CalendarSource.GetDaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
      var lastDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, daysInMonth);
      var firstDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
      while (firstDate.DayOfWeek != this.FirstDayOfWeek)
      {
        firstDate = firstDate.Subtract(TimeSpan.FromDays(1));
      }

      while (lastDate.AddDays(1).DayOfWeek != this.FirstDayOfWeek)
      {
        lastDate = lastDate.AddDays(1);
      }

      int daysInCalendarView = lastDate.Subtract(firstDate).Days + 1;
      this.ItemsHost.RowCount = (int) Math.Ceiling(daysInCalendarView / (double) this.ItemsHost.ColumnCount);
      for (var dayIndex = 0; dayIndex < this.ItemsHost.RowCount * this.ItemsHost.ColumnCount; dayIndex++)
      {
        DateTime currentDay = firstDate.AddDays(dayIndex);
        var calendarDateItem = new CalendarDate
        {
          Day = currentDay,
          IsHoliday = currentDay.Day % 2 == 0,
          DayOfWeek = this.CalendarSource.GetDayOfWeek(currentDay),
          WeekOfYear = this.CalendarSource.GetWeekOfYear(
            currentDay,
            CalendarWeekRule.FirstDay,
            this.FirstDayOfWeek),
          Annotation = "Special Date Special Date"
        };
        dates.Add(calendarDateItem);
      }

      this.DateHeaderItemsSource = dates;


      // TODO::Remove
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

      this.ItemsSource = events;
      //this.ItemsHost.InvalidateMeasure();
      this.DayChangeWatcher.CalendarDayChanged += OnTodayChanged;
      this.DayChangeWatcher.Start();
    }

    private void OnTodayChanged(object sender, EventArgs e)
    {
      HandleTodayChanged();
    }

    private void HandleTodayChanged()
    {
      if (this.TodayItemIndex < 0)
      {
        if (!Calendar.DateToDateItemContainerMap.TryGetValue(DateTime.Today, out UIElement todayCalendarDateItem))
        {
          return;
        }

        Calendar.SetIsToday(todayCalendarDateItem, true);

        if (this.ItemContainerToItemMap.TryGetValue(todayCalendarDateItem, out object todayItem))
        {
          this.TodayItemIndex = this.DateHeaderItems.IndexOf(todayItem);
        }

        return;
      }

      object yesterdayItem = this.DateHeaderItems.GetItemAt(this.TodayItemIndex);
      if (this.ItemToItemContainerMap.TryGetValue(yesterdayItem, out UIElement yesterdayItemContainer))
      {
        Calendar.SetIsToday(yesterdayItemContainer, false);
      }

      if (Calendar.DateToDateItemContainerMap.TryGetValue(DateTime.Today, out UIElement todayItemContainer))
      {
        Calendar.SetIsToday(todayItemContainer, true);

        this.TodayItemIndex++;

        // If this.DateHeaderItemsSource is sorted by date, then the next item is the today item
        object todayItemByIndex = this.DateHeaderItems.GetItemAt(this.TodayItemIndex);
        if (this.ItemContainerToItemMap.TryGetValue(todayItemContainer, out object todayItem))
        {
          if (object.ReferenceEquals(todayItem, todayItemByIndex))
          {
            return;
          }

          this.TodayItemIndex = this.DateHeaderItems.IndexOf(todayItem);
          return;
        }
      }

      this.TodayItemIndex = -1;
    }

    #endregion

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      this.ItemsHost = GetTemplateChild("PART_ItemsHost") as CalendarPanel;
      ObserveItemsHost();
      Initialize();
      this.ItemsHost.Initialize(this);
    }

    protected virtual void OnSelectedIndexChanged(int oldIndex, int newIndex)
    {
      if (newIndex < 0)
      {
        this.SelectedItem = null;
      }
      else if (newIndex > this.Items.Count)
      {
        throw new IndexOutOfRangeException();
      }
      else
      {
        this.SelectedItem = this.Items.GetItemAt(newIndex);
      }
    }

    protected virtual void OnSelectedItemChanged(object oldValue, object newValue)
    {
      if (newValue != oldValue)
      {
        this.SelectedIndex = this.Items.IndexOf(newValue);
      }
    }

    private static void OnDateHeaderItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as Calendar).OnDateHeaderItemsSourceChanged(e.OldValue as IEnumerable, e.NewValue as IEnumerable);
    }

    protected virtual void OnDateHeaderItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
      if (oldValue is INotifyCollectionChanged oldObservableCollection)
      {
        oldObservableCollection.CollectionChanged -= OnDateHeaderItemsSourceCollectionChanged;
      }

      if (oldValue != null)
      {
        RemoveOldDateItems(oldValue);
      }

      this.ItemsHost.ClearDateHeaderChildren();

      if (newValue == null)
      {
        return;
      }

      this.DateHeaderItems = CollectionViewSource.GetDefaultView(newValue) as CollectionView;

      if (newValue is INotifyCollectionChanged newObservableCollection)
      {
        newObservableCollection.CollectionChanged += OnDateHeaderItemsSourceCollectionChanged;
      }

      InitializeNewDateItems(this.DateHeaderItems);
    }

    private void OnDateHeaderItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          InitializeNewDateItems(e.NewItems);
          break;
        case NotifyCollectionChangedAction.Remove:
          RemoveOldDateItems(e.OldItems);
          break;
        case NotifyCollectionChangedAction.Reset:
          RemoveOldDateItems(e.OldItems);

          Calendar.DateToDateItemContainerMap.Clear();
          Calendar.DateItemContainerToDateMap.Clear();
          this.ItemsHost.ClearDateHeaderChildren();
          break;
      }
    }

    protected virtual void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
      if (oldValue is INotifyCollectionChanged oldObservableCollection)
      {
        oldObservableCollection.CollectionChanged -= OnItemsSourceCollectionChanged;
      }

      if (oldValue != null)
      {
        RemoveOlEventItems(oldValue);
      }

      this.ItemsHost.ClearEventChildren();

      if (newValue == null)
      {
        return;
      }

      this.Items = CollectionViewSource.GetDefaultView(newValue) as CollectionView;

      if (newValue is INotifyCollectionChanged newObservableCollection)
      {
        newObservableCollection.CollectionChanged += OnItemsSourceCollectionChanged;
      }

     InitializeNewEventItems(this.Items);
    }

    private void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          InitializeNewEventItems(e.NewItems);
          break;
        case NotifyCollectionChangedAction.Remove:
          RemoveOlEventItems(e.OldItems);
          break;
        case NotifyCollectionChangedAction.Reset:
          RemoveOlEventItems(e.OldItems);

          this.ItemsHost.ClearEventChildren();
          break;
      }
    }

    private void InitializeNewDateItems(IEnumerable newItems)
    {
      var itemContainers = new List<UIElement>();
      foreach (object item in newItems)
      {
        var dateItemContainer = GetContainerForDateItem() as FrameworkElement;
        PrepareContainerForCalendarDateItemOverride(dateItemContainer, item);
        RegisterItemContainer(dateItemContainer, item);
        itemContainers.Add(dateItemContainer);
      }

      this.ItemsHost.AddDateHeaderChildren(itemContainers);
    }

    private void RemoveOldDateItems(IEnumerable newItems)
    {
      var itemContainers = new List<UIElement>();
      foreach (object item in newItems)
      {
        if (this.ItemToItemContainerMap.TryGetValue(item, out UIElement dateItemContainer))
        {
          UnregisterItemContainer(dateItemContainer, item);
          Calendar.UnregisterDateItemContainer(dateItemContainer);
          itemContainers.Add(dateItemContainer);
        }
      }

      this.ItemsHost.RemoveDateHeaderChildren(itemContainers);
    }

    private void InitializeNewEventItems(IEnumerable newItems)
    {
      var itemContainers = new List<FrameworkElement>();
      foreach (object item in newItems)
      {
        var eventItemContainer = GetContainerForEventItemOverride();
        PrepareContainerForEventItemOverride(eventItemContainer, item);
        RegisterItemContainer(eventItemContainer, item);
        itemContainers.Add(eventItemContainer);
      }

      this.ItemsHost.AddEventChildren(itemContainers);
    }

    private void RemoveOlEventItems(IEnumerable newItems)
    {
      var itemContainers = new List<FrameworkElement>();
      foreach (object item in newItems)
      {
        if (this.ItemToItemContainerMap.TryGetValue(item, out UIElement dateItemContainer))
        {
          UnregisterItemContainer(dateItemContainer, item);
          Calendar.UnregisterDateItemContainer(dateItemContainer);
          itemContainers.Add(dateItemContainer as FrameworkElement);
        }
      }

      this.ItemsHost.RemoveEventChildren(itemContainers);
    }

    private static void UnregisterDateItemContainer(UIElement dateItemContainer)
    {
      Calendar.DateToDateItemContainerMap.Remove(Calendar.GetDay(dateItemContainer));
      Calendar.DateItemContainerToDateMap.Remove(dateItemContainer);
    }

    private static void RegisterDateItemContainer(UIElement dateItemContainer)
    {
      Calendar.DateToDateItemContainerMap.Add(Calendar.GetDay(dateItemContainer), dateItemContainer);
      Calendar.DateItemContainerToDateMap.Add(dateItemContainer, Calendar.GetDay(dateItemContainer));
    }

    private void UnregisterItemContainer(UIElement dateItemContainer, object item)
    {
      this.ItemContainerToItemMap.Remove(dateItemContainer);
      this.ItemToItemContainerMap.Remove(item);
    }

    private void RegisterItemContainer(UIElement dateItemContainer, object item)
    {
      this.ItemContainerToItemMap.Add(dateItemContainer, item);
      this.ItemToItemContainerMap.Add(item, dateItemContainer);
    }

    private void OnCalendarEventItemSelected(object sender, RoutedEventArgs routedEventArgs)
    {
      foreach (UIElement itemsHostChild in this.ItemsHost.GetEventItems())
      {
        if (!object.ReferenceEquals(itemsHostChild, routedEventArgs.OriginalSource)
            && Selector.GetIsSelected(itemsHostChild))
        {
          Selector.SetIsSelected(itemsHostChild, false);
          RaiseEvent(new RoutedEventArgs(Selector.UnselectedEvent, this));
        }
      }

      //this.ItemsHost.get
      switch (routedEventArgs.OriginalSource)
      {
        case HeaderedItemsControl headeredItemsControl:
          this.SelectedItem = headeredItemsControl.Header;
          break;
        case ContentControl contentControl:
          this.SelectedItem = contentControl.Content;
          break;
        case FrameworkElement frameworkElement:
          this.SelectedItem = frameworkElement.DataContext;
          break;
      }


      RaiseEvent(new RoutedEventArgs(Selector.SelectedEvent, this));
    }

    private void OnCalendarDateItemSelected(object sender, RoutedEventArgs routedEventArgs)
    {
      foreach (UIElement itemsHostChild in this.ItemsHost.GetDateItems())
      {
        if (!object.ReferenceEquals(itemsHostChild, routedEventArgs.OriginalSource)
            && Selector.GetIsSelected(itemsHostChild))
        {
          Selector.SetIsSelected(itemsHostChild, false);
          RaiseEvent(new RoutedEventArgs(Selector.UnselectedEvent, this));
        }
      }

      RaiseEvent(new RoutedEventArgs(Selector.SelectedEvent, this));
    }

    protected virtual FrameworkElement GetContainerForEventItemOverride() => new CalendarEventItem();
    protected DependencyObject GetContainerForDateItem() => new CalendarDateItem();
    protected DependencyObject GetContainerForDateColumnHeaderItem() => new CalendarDateColumnHeaderItem();

    protected virtual bool IsEventItemItsOwnContainerOverride(object item) => item is CalendarEventItem;

    protected virtual void PrepareContainerForEventItemOverride(DependencyObject element, object item)
    {
      switch (element)
      {
        case HeaderedItemsControl headeredItemsControl:
          headeredItemsControl.ItemContainerStyle = this.ItemContainerStyle;
          headeredItemsControl.ItemTemplate = this.ItemTemplate;
          headeredItemsControl.ItemTemplateSelector = this.ItemTemplateSelector;
          headeredItemsControl.Header = item;
          headeredItemsControl.DataContext = item;
          break;
        case ContentControl contentControl:
          contentControl.Style = this.ItemContainerStyle;
          contentControl.ContentTemplate = this.ItemTemplate;
          contentControl.ContentTemplateSelector = this.ItemTemplateSelector;
          contentControl.Content = item;
          contentControl.DataContext = item;
          break;
        case Control control:
          control.Style = this.ItemContainerStyle;
          control.DataContext = item;
          break;
      }
    }

    protected virtual void PrepareContainerForCalendarDateItemOverride(DependencyObject element, object item)
    {
      switch (element)
      {
        case HeaderedItemsControl headeredItemsControl:
          headeredItemsControl.ItemContainerStyle = this.ItemContainerStyle;
          headeredItemsControl.ItemTemplate = this.ItemTemplate;
          headeredItemsControl.ItemTemplateSelector = this.ItemTemplateSelector;
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

    protected virtual void PrepareContainerForCalendarDateColumnHeaderItemOverride(
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
      base.ArrangeOverride(arrangeBounds);
      if (Calendar.IsItemsHostLayoutDirty)
      {
        this.ItemsHost?.UpdateCalendarLayout();
        Calendar.IsItemsHostLayoutDirty = false;
      }

      return arrangeBounds;
    }

    #endregion

    ///// <inheritdoc />
    //protected override void OnRender(DrawingContext drawingContext)
    //{
    //    base.OnRender(drawingContext);
    //    if (!(this.ItemsHost is Panel panel))
    //    {
    //        return;
    //    }

    //    DrawGridLines(panel, drawingContext);
    //}

    //protected virtual void DrawGridLines(Panel panel, DrawingContext drawingContext)
    //{
    //    if (!(panel is Grid gridPanel))
    //    {
    //        return;
    //    }

    //    var pen = new Pen(this.GridColor, this.GridThickness);
    //    var verticalLineStart = new Point(0, 0);
    //    var horizontalLineStart = new Point(0, 0);
    //    var verticalLineEnd = new Point(0, gridPanel.ActualHeight);
    //    var horizontalLineEnd = new Point(gridPanel.ActualWidth, 0);

    //    drawingContext.DrawLine(pen, horizontalLineStart, horizontalLineEnd);
    //    drawingContext.DrawLine(pen, verticalLineStart, verticalLineEnd);

    //    foreach (RowDefinition panelRowDefinition in gridPanel.RowDefinitions)
    //    {
    //        horizontalLineStart.Y = panelRowDefinition.Offset + panelRowDefinition.ActualHeight;
    //        horizontalLineEnd.Y = panelRowDefinition.Offset + panelRowDefinition.ActualHeight;


    //        drawingContext.DrawLine(pen, horizontalLineStart, horizontalLineEnd);
    //    }

    //    foreach (ColumnDefinition panelColumnDefinition in gridPanel.ColumnDefinitions)
    //    {
    //        verticalLineStart.X = panelColumnDefinition.Offset + panelColumnDefinition.ActualWidth;
    //        verticalLineEnd.X = panelColumnDefinition.Offset + panelColumnDefinition.ActualWidth;
    //        drawingContext.DrawLine(pen, verticalLineStart, verticalLineEnd);
    //    }
    //}

    protected virtual void OnCanExecuteChanged(object sender, EventArgs e)
    {
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      //    this.ItemsHost = GetItemsHost();
      if (this.ItemsHost != null)
      {
        InvalidateVisual();
      }
    }

    private void ObserveItemsHost()
    {
      //if (this.ItemsHost is CalendarPanel oldCalendarPanel)
      //{
      //    oldCalendarPanel.AutogeneratingDate -= OnItemsHostIsAutogeneratingDate;
      //    oldCalendarPanel.AutogeneratingDateColumnHeader -= OnItemsHostIsAutogeneratingDateColumnHeader;
      //}

      //if (this.ItemsHost is CalendarPanel newCalendarPanel)
      //{
      //    newCalendarPanel.AutogeneratingDate += OnItemsHostIsAutogeneratingDate;
      //    newCalendarPanel.AutogeneratingDateColumnHeader += OnItemsHostIsAutogeneratingDateColumnHeader;
      //}

      this.ItemsHost.AutogeneratingDate += OnItemsHostIsAutogeneratingDate;
      this.ItemsHost.AutogeneratingDateColumnHeader += OnItemsHostIsAutogeneratingDateColumnHeader;
    }

    private void OnItemsHostIsAutogeneratingDateColumnHeader(object sender, DateColumnHeaderGeneratorArgs e)
    {
      OnAutogeneratingDateColumnHeader(e);
      //switch (this.ItemsHost)
      //{
      //  case CalendarPanel _:
      //    return;
      //  case Grid _:
      //    OnGeneratingDateColumnHeader(e);
      //    break;
      //}
    }

    private void OnItemsHostIsAutogeneratingDate(object sender, DateGeneratorArgs e)
    {
      OnAutogeneratingDate(e);
      //switch (this.ItemsHost)
      //{
      //  case CalendarPanel _:
      //    return;
      //  case Grid _:
      //    OnGeneratingDate(e);
      //    break;
      //}
    }

    private void CreateDateColumnHeaderItems()
    {
      var dateColumnHeaderItemContainers = new List<UIElement>();
      List<int> dayOfWeekValues = Enum.GetValues(typeof(DayOfWeek)).Cast<int>().ToList();
      List<int> daysOfWeekToWrap = dayOfWeekValues.TakeWhile(dayValue => dayValue < (int) this.FirstDayOfWeek).ToList();
      dayOfWeekValues.RemoveRange(0, daysOfWeekToWrap.Count);
      dayOfWeekValues.AddRange(daysOfWeekToWrap);
      IEnumerable<string> daysOfWeek = dayOfWeekValues.Select(dayOfWeekValue => ((DayOfWeek) dayOfWeekValue).ToString());
      foreach (string dayOfWeekName in daysOfWeek)
      {
        var dateContainer = GetContainerForDateColumnHeaderItem() as UIElement;
        PrepareContainerForCalendarDateColumnHeaderItemOverride(dateContainer, dayOfWeekName);

        dateColumnHeaderItemContainers.Add(dateContainer);
      }

      this.ItemsHost.AddDateColumnHeaderChildren(dateColumnHeaderItemContainers);
    }

    //private Size ArrangeDateItems(Size arrangeBounds)
    //{
    //    for (var index = 0; index < this.ItemContainerToItemMap.Count; index++)
    //    {
    //        DependencyObject itemContainer = this.ItemContainerToItemMap.Keys.ElementAt(index);
    //        object item = this.ItemContainerToItemMap[itemContainer];
    //        int columnIndex = index % 7;
    //        int rowIndex = (int) Math.Floor(index / 7.0) + 1;
    //        var dateGeneratorArgs = new DateGeneratorArgs(
    //            itemContainer as UIElement,
    //            this.ItemsHost,
    //            item,
    //            columnIndex % 7,
    //            rowIndex);

    //        OnAutogeneratingDate(dateGeneratorArgs);
    //        OnGeneratingDate(dateGeneratorArgs);
    //    }

    //    return arrangeBounds;
    //}

    //private Size ArrangeDateColumnHeaderItems(Size arrangeBounds)
    //{
    //    for (var columnIndex = 0; columnIndex < 7; columnIndex++)
    //    {
    //        CalendarDateColumnHeaderItem dateContainer = this.DateColumnHeaderItems[columnIndex];
    //        var dateColumnHeaderGeneratorArgs = new DateColumnHeaderGeneratorArgs(
    //            dateContainer,
    //            this.ItemsHost,
    //            dateContainer.Header,
    //            columnIndex,
    //            0);

    //        OnAutogeneratingDateColumnHeader(dateColumnHeaderGeneratorArgs);
    //        OnGeneratingDateColumnHeader(dateColumnHeaderGeneratorArgs);
    //    }

    //    return arrangeBounds;
    //}

    //protected virtual void OnGeneratingDateColumnHeader(DateColumnHeaderGeneratorArgs dateColumnHeaderGeneratorArgs)
    //{
    //  if (dateColumnHeaderGeneratorArgs.IsCanceled)
    //  {
    //    return;
    //  }

    //  Grid.SetRow(dateColumnHeaderGeneratorArgs.ItemContainer, 0);
    //  Grid.SetColumn(dateColumnHeaderGeneratorArgs.ItemContainer, dateColumnHeaderGeneratorArgs.ColumnIndex);
    //  //this.ItemsHost.Children.Add(dateColumnHeaderGeneratorArgs.ItemContainer);
    //}

    //protected virtual void OnGeneratingDate(DateGeneratorArgs dateGeneratorArgs)
    //{
    //  if (dateGeneratorArgs.IsCanceled)
    //  {
    //    return;
    //  }

    //  Grid.SetRow(dateGeneratorArgs.ItemContainer, dateGeneratorArgs.RowIndex);
    //  Grid.SetColumn(dateGeneratorArgs.ItemContainer, dateGeneratorArgs.ColumnIndex);
    //  //this.ItemsHost.Children.Add(dateGeneratorArgs.ItemContainer);
    //}

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

    public IEnumerable ItemsSource
    {
      get => (IEnumerable) GetValue(Calendar.ItemsSourceProperty);
      set => SetValue(Calendar.ItemsSourceProperty, value);
    }

    public CollectionView Items
    {
      get => (CollectionView) GetValue(Calendar.ItemsProperty);
      set => SetValue(Calendar.ItemsProperty, value);
    }

    public CollectionView DateHeaderItems
    {
      get => (CollectionView) GetValue(Calendar.DateHeaderItemsProperty);
      set => SetValue(Calendar.DateHeaderItemsProperty, value);
    }

    public Style ItemContainerStyle
    {
      get => (Style) GetValue(Calendar.ItemContainerStyleProperty);
      set => SetValue(Calendar.ItemContainerStyleProperty, value);
    }

    public DataTemplate ItemTemplate
    {
      get => (DataTemplate) GetValue(Calendar.ItemTemplateProperty);
      set => SetValue(Calendar.ItemTemplateProperty, value);
    }

    public DataTemplateSelector ItemTemplateSelector
    {
      get => (DataTemplateSelector) GetValue(Calendar.ItemTemplateSelectorProperty);
      set => SetValue(Calendar.ItemTemplateSelectorProperty, value);
    }

    public object SelectedItem
    {
      get => GetValue(Calendar.SelectedItemProperty);
      set => SetValue(Calendar.SelectedItemProperty, value);
    }

    public int SelectedIndex
    {
      get => (int) GetValue(Calendar.SelectedIndexProperty);
      set => SetValue(Calendar.SelectedIndexProperty, value);
    }

    public IEnumerable DateHeaderItemsSource
    {
      get => (IEnumerable) GetValue(Calendar.DateHeaderItemsSourceProperty);
      set => SetValue(Calendar.DateHeaderItemsSourceProperty, value);
    }

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

    private CalendarPanel ItemsHost { get; set; }

    private static bool IsItemsHostLayoutDirty { get; set; }

    private DayChangeWatcher DayChangeWatcher { get; }

    private static Calendar Instance { get; set; }

    //public static readonly DependencyProperty DateColumnHeaderItemTemplateProperty = DependencyProperty.Register(
    //    "DateColumnHeaderItemTemplate",
    //    typeof(DataTemplate),
    //    typeof(Calendar),
    //    new PropertyMetadata(default(DataTemplate), Calendar.OnDateColumnHeaderItemTemplateChanged));

    //public static readonly DependencyProperty DateColumnHeaderDataTemplateSelectorProperty =
    //    DependencyProperty.Register(
    //        "DateColumnHeaderDataTemplateSelector",
    //        typeof(DataTemplateSelector),
    //        typeof(Calendar),
    //        new PropertyMetadata(
    //            default(DataTemplateSelector),
    //            Calendar.OnDateColumnHeaderItemTemplateSelectorChanged));

    /// <inheritdoc />
    //protected override Size MeasureOverride(Size constraint)
    //{
    //    base.MeasureOverride(constraint);

    //    CreateDateColumnHeaderItems();
    //    this.DateColumnHeaderItems.ForEach(itemContainer => itemContainer.Measure(constraint));
    //    return constraint;
    //}

    //private static void OnDateColumnHeaderItemTemplateChanged(
    //    DependencyObject d,
    //    DependencyPropertyChangedEventArgs e)
    //{
    //    var this_ = d as Calendar;
    //    this_.DateColumnHeaderItems.ForEach(
    //        itemContainer => itemContainer.ContentTemplate = e.NewValue as DataTemplate);
    //}

    //private Size CreateDateItems(Size constraint)
    //{
    //    //this.DateItems.Clear();
    //    var requiredSize = new Size(0, 0);
    //    foreach (object item in this.Items)
    //    {
    //        DependencyObject itemContainer = GetContainerForItemOverride();
    //        PrepareContainerForItemOverride(itemContainer, item);
    //        //{
    //        //    DataContext = item,
    //        //    Header = item,
    //        //    HeaderTemplate = this.DateItemTemplate,
    //        //    HeaderTemplateSelector = this.DateItemTemplateSelector,
    //        //    Style = this.DateItemContainerStyle,
    //        //    ItemContainerStyle = this.DateHeaderItemContainerStyle,
    //        //    ItemTemplate = this.DateHeaderItemTemplate,
    //        //    ItemTemplateSelector = this.CalendarEventItemTemplateSelector
    //        //};
    //        //this.DateItems.Add(itemContainer);

    //        if (!this.ItemContainerToItemMap.ContainsKey(itemContainer))
    //        {
    //            this.ItemContainerToItemMap.Add(itemContainer, item);
    //        }

    //        (itemContainer as UIElement)?.Measure(constraint);
    //        requiredSize.Height = Math.Max(
    //            requiredSize.Height,
    //            (itemContainer as UIElement)?.DesiredSize.Height ?? 0);
    //        requiredSize.Width += (itemContainer as UIElement)?.DesiredSize.Width ?? 0;
    //    }

    //    return requiredSize;
    //}
  }
}