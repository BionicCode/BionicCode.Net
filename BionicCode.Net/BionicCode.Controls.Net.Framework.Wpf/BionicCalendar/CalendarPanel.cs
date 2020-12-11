#region Info

// 2020/11/06  20:13
// Activitytracker

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using BionicCode.Utilities.Net.Framework.Wpf.Extensions;
using ColumnDefinition = System.Windows.Controls.ColumnDefinition;
using Grid = System.Windows.Controls.Grid;
using Panel = System.Windows.Controls.Panel;
using RowDefinition = System.Windows.Controls.RowDefinition;
using ScrollViewer = System.Windows.Controls.ScrollViewer;

#endregion

namespace BionicCode.Controls.Net.Framework.Wpf.BionicCalendar
{
  public class CalendarPanel : Grid, IScrollInfo
  {
    public static readonly DependencyProperty RowCountProperty = DependencyProperty.Register(
      "RowCount",
      typeof(int),
      typeof(CalendarPanel),
      new PropertyMetadata(5));

    public static readonly DependencyProperty ColumnCountProperty = DependencyProperty.Register(
      "ColumnCount",
      typeof(int),
      typeof(CalendarPanel),
      new PropertyMetadata(7));

    private bool canVerticallyScroll;
    private bool canHorizontallyScroll;
    private double extentWidth;
    private double extentHeight;
    private double viewportWidth;
    private double viewportHeight;
    private double horizontalOffset;
    private double verticalOffset;
    private ScrollViewer scrollOwner;

    //public static readonly DependencyProperty GridColorProperty = DependencyProperty.Register(
    //  "GridColor",
    //  typeof(Brush),
    //  typeof(CalendarPanel),
    //  new PropertyMetadata(Brushes.DimGray));


    public Brush GridColor
    {
      get => (Brush) GetValue(Calendar.GridColorProperty);
      set => SetValue(Calendar.GridColorProperty, value);
    }

    //public static readonly DependencyProperty GridThicknessProperty = DependencyProperty.Register(
    //  "GridThickness",
    //  typeof(double),
    //  typeof(Calendar),
    //  new PropertyMetadata(0.2));

    public double GridThickness
    {
      get => (double) GetValue(Calendar.GridThicknessProperty);
      set => SetValue(Calendar.GridThicknessProperty, value);
    }

    #region

    static CalendarPanel()
    {
      FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
        typeof(CalendarPanel),
        new FrameworkPropertyMetadata(typeof(CalendarPanel)));
    }

    public CalendarPanel()
    {
      AddHandler(Calendar.SpanningRequestedRoutedEvent, new SpanningRequestedRoutedEventHandler(SpanEventItemOnSpanningRequested));
      AddHandler(Calendar.SelectedRoutedEvent, new RoutedEventHandler(OnCalendarItemSelected));
      AddHandler(Calendar.SelectedRoutedEvent, new RoutedEventHandler(OnCalendarItemSelected));
      this.InternalHostPanels = new Dictionary<int, (Grid SpanningPanel, Panel DefaultPanel)>[this.RowCount - 1];
      this.InternalEventItems = new List<UIElement>();
      this.InternalDateHeaderItems = new List<UIElement>();
      this.InternalDateColumnHeaderItems = new List<UIElement>();
      this.InternalRowItems = new List<CalendarRowItem>();
      this.InternalDateHeaderItemLookupTable = new Dictionary<DateTime, UIElement>();
      this.CalendarViewTable = new Dictionary<DateTime, CalendarView>();

      this.CurrentCalendarView = new CalendarView(new List<DateTime>());
      CreateRootGrid();
    }

    #endregion

    /// <inheritdoc />
    protected override void OnRender(DrawingContext drawingContext)
    {
      base.OnRender(drawingContext);

      DrawGridLines(this, drawingContext);
    }

    protected virtual void DrawGridLines(Panel panel, DrawingContext drawingContext)
    {
      if (!(panel is Grid gridPanel))
      {
        return;
      }

      var pen = new Pen(this.GridColor, this.GridThickness);
      var verticalLineStart = new Point(0, 0);
      var horizontalLineStart = new Point(0, 0);
      var verticalLineEnd = new Point(0, gridPanel.ActualHeight);
      var horizontalLineEnd = new Point(gridPanel.ActualWidth, 0);

      drawingContext.DrawLine(pen, horizontalLineStart, horizontalLineEnd);
      drawingContext.DrawLine(pen, verticalLineStart, verticalLineEnd);

      foreach (RowDefinition panelRowDefinition in gridPanel.RowDefinitions)
      {
        horizontalLineStart.Y = panelRowDefinition.Offset + panelRowDefinition.ActualHeight;
        horizontalLineEnd.Y = panelRowDefinition.Offset + panelRowDefinition.ActualHeight;


        drawingContext.DrawLine(pen, horizontalLineStart, horizontalLineEnd);
      }

      foreach (ColumnDefinition panelColumnDefinition in gridPanel.ColumnDefinitions)
      {
        verticalLineStart.X = panelColumnDefinition.Offset + panelColumnDefinition.ActualWidth;
        verticalLineEnd.X = panelColumnDefinition.Offset + panelColumnDefinition.ActualWidth;
        drawingContext.DrawLine(pen, verticalLineStart, verticalLineEnd);
      }
    }

    private void OnCalendarItemSelected(object sender, RoutedEventArgs e)
    {
      if (e.OriginalSource is UIElement dateItemContainer)
      {
        var rowItem = e.Source as CalendarRowItem;

        int calendarDay = GetCalendarDayOf(dateItemContainer);
        int calendarDayIndex = calendarDay - 1;
        int columnIndex = calendarDayIndex % this.ColumnCount;
        //Grid.SetColumn(rowItem.SelectionBorder, columnIndex);
        //Grid.SetRowSpan(rowItem.SelectionBorder, rowItem.ItemsHost.RowDefinitions.Count);
        //this.InternalRowItems.ForEach(item => item.IsSelected = false);
        //rowItem.IsSelected = true;
        //rowItem.SelectionBorder.UpdateLayout();
      }
    }

    private int GetRowIndexOf(UIElement itemContainer)
    {
      int calendarDay = GetCalendarDayOf(itemContainer);
      int calendarDayIndex = calendarDay - 1;
      int rowIndex = calendarDayIndex / this.ColumnCount;

      return rowIndex;
    }

    public void SpanEventItemOnSpanningRequested(object sender, SpanningRequestedRoutedEventArgs e)
    {
      //EventGeneratorArgs containerData = CreateEventGeneratorArgsFor(sender as UIElement);
      //if (containerData.ItemsHost.DefaultEventPanel.Children.Contains(containerData.ItemContainer))
      //{
      //}

      //if (!containerData.ItemsHost.SpanningPanel.Children.Contains(containerData.ItemContainer))
      //{
      //    Grid.SetRow(
      //        containerData.ItemContainer,
      //        containerData.ItemsHost.SpanningPanel.RowDefinitions.Count);
      //    containerData.ItemsHost.SpanningPanel.RowDefinitions.Add(
      //        new RowDefinition {Height = new GridLength(1, GridUnitType.Auto)});
      //    containerData.ItemsHost.SpanningPanel.Children.Add(containerData.ItemContainer);
      //}

      //int columnSpanDelta = e.SpanDirection == ExpandDirection.Right
      //    ? 1
      //    : -1;
      //int columnSpan = Math.Min(
      //    this.ColumnCount,
      //    Math.Max(1, Grid.GetColumnSpan(containerData.ItemContainer) + columnSpanDelta));
      //Grid.SetColumnSpan(containerData.ItemContainer, columnSpan);

      //int calendarDay = GetCalendarDayOf(containerData.ItemContainer);
      //var nextCalendarDay = 0;
      //double heightDelta = 0;


      //if (e.SpanDirection == ExpandDirection.Right)
      //{
      //    if (columnSpan == 1)
      //    {
      //        return;
      //    }

      //    nextCalendarDay = calendarDay + columnSpan - 1;
      //    heightDelta = (containerData.ItemContainer as FrameworkElement).ActualHeight;
      //}

      //else if (e.SpanDirection == ExpandDirection.Left)
      //{
      //    nextCalendarDay = calendarDay + columnSpan;
      //    heightDelta = -(containerData.ItemContainer as FrameworkElement).ActualHeight;
      //}

      //int nextCalendarDayIndex = nextCalendarDay - 1;
      //int nextDayRowIndex = nextCalendarDayIndex / this.ColumnCount;
      //if (nextDayRowIndex != containerData.RowIndex)
      //{
      //    return;
      //}

      //UIElement nextDayContainer =
      //    this.InternalDateHeaderItems.Find(
      //        itemContainer => GetCalendarDayOf(itemContainer) == nextCalendarDay);
      //if (nextDayContainer == null)
      //{
      //    return;
      //}

      //if (nextDayContainer.TryFindVisualChildElementByName("PART_SpanningEventItemsHost", out Grid grid))
      //{
      //    Thickness gridMargin = grid.Margin;
      //    gridMargin.Top += heightDelta;
      //    grid.Margin = gridMargin;
      //}
    }

    private void CreateRootGrid()
    {
      this.RowDefinitions.Add(new RowDefinition {Height = new GridLength(28, GridUnitType.Pixel)});
      InitializeGrid(this, new GridLength(1, GridUnitType.Star), this.RowCount);
    }


    public void ClearEventChildren() => this.InternalEventItems.Clear();

    public void AddEventChildren(IEnumerable<UIElement> children)
    {
      this.InternalEventItems.AddRange(children);
      this.IsLayoutDirty = true;
      //InvalidateMeasure();
    }

    public void ClearDateHeaderChildren() => this.InternalDateHeaderItems.Clear();

    public void AddDateHeaderChildren(IEnumerable<UIElement> children)
    {
      this.InternalDateHeaderItems.AddRange(children);
      this.IsLayoutDirty = true;
      //InvalidateMeasure();
    }

    public void ClearDateColumnHeaderChildren() => this.InternalDateColumnHeaderItems.Clear();

    public void AddDateColumnHeaderChildren(IEnumerable<UIElement> children)
    {
      this.InternalDateColumnHeaderItems.AddRange(children);
      this.IsLayoutDirty = true;
      InvalidateMeasure();
    }

    public void RemoveEventChildren(IEnumerable<UIElement> childrenToRemove)
    {
      foreach (UIElement uiElement in childrenToRemove)
      {
        this.InternalEventItems.Remove(uiElement);
      }

      InvalidateMeasure();
    }

    public void RemoveDateHeaderChildren(IEnumerable<UIElement> childrenToRemove)
    {
      foreach (UIElement calendarDateItem in childrenToRemove)
      {
        this.InternalDateHeaderItems.Remove(calendarDateItem);
      }

      InvalidateMeasure();
    }

    private void InitializeGrid(Grid grid, GridLength rowHeight, int rowCount, int rowStartIndex = 0)
    {
      for (int rowIndex = rowStartIndex; rowIndex < rowCount; rowIndex++)
      {
        grid.RowDefinitions.Add(new RowDefinition {Height = rowHeight});
      }

      for (var columnIndex = 0; columnIndex < this.ColumnCount; columnIndex++)
      {
        grid.ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)});
      }
    }

    public void UpdateCalendarLayout()
    {
      this.IsLayoutDirty = true;
      InvalidateMeasure();
    }

    public void Initialize(Calendar owner)
    {
      this.Owner = owner ?? throw new ArgumentNullException(nameof(owner));

      int startYear = DateTime.Today.Year - 5;
      for (int yearIndex = 0; yearIndex < 11; yearIndex++)
      {
        for (int monthIndex = 0;
          monthIndex < this.Owner.CalendarSource.GetMonthsInYear(startYear + yearIndex);
          monthIndex++)
        {
          CalendarView monthView = CreateCalendarViewOfMonth(startYear + yearIndex, monthIndex + 1);
          this.CalendarViewTable.Add(monthView.Index, monthView);
        }
      }

      this.CurrentCalendarView =
        this.CalendarViewTable[new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)];

      this.IsCalendarPanelInitialized = true;
    }


    ///// <inheritdoc />
    //protected override Size MeasureOverride(Size constraint)
    //{
    //    base.MeasureOverride(constraint);

    //    ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);
    //    UIElementCollection items = this.InternalChildren;
    //    if (!(itemsControl is Calendar calendar) || calendar.DateColumnHeaderItems.Count < 7)
    //    {
    //        return constraint;
    //    }

    //    this.InternalDateColumnHeaderItems = calendar.DateColumnHeaderItems;
    //    this.InternalDateColumnHeaderItems.ForEach(itemContainer => itemContainer.Measure(constraint));
    //    return constraint;
    //}

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size arrangeSize)
    {
      base.ArrangeOverride(arrangeSize);
      if (!this.IsCalendarPanelInitialized)
      {
        throw new InvalidOperationException(
          "'CalendarPanel' instance not initialized. Make sure to call 'CalendarPanel.Initialize()' before the panel is entering the layout passing.");
      }

      if (!this.IsLayoutDirty)
      {
        return arrangeSize;
      }

      this.Children.Clear();
      ArrangeDateColumnHeaderItems(arrangeSize);
      ArrangeDateItems(arrangeSize);
      //ArrangeCalendarEventItems(arrangeSize);

      this.IsLayoutDirty = false;
      return arrangeSize;
    }

    private Size ArrangeCalendarEventItems(Size arrangeSize)
    {
      foreach (UIElement itemContainer in this.InternalEventItems)
      {
        //EventGeneratorArgs eventGeneratorArgs = CreateEventGeneratorArgsFor(itemContainer);

        //OnAutogeneratingEvent(eventGeneratorArgs);
        //OnGeneratingCalendarEvents(eventGeneratorArgs);
        var itemContainerAdornerLayer = AdornerLayer.GetAdornerLayer(itemContainer);
        itemContainerAdornerLayer.Add(new EventResizeAdorner(itemContainer));
      }

      return arrangeSize;
    }

    private EventGeneratorArgs CreateEventGeneratorArgsFor(UIElement itemContainer)
    {
      int calendarDay = GetCalendarDayOf(itemContainer);

      int calendarIndex = calendarDay - 1;
      int calendarRowIndex = calendarIndex / this.ColumnCount;
      int columnIndex = calendarIndex % this.ColumnCount;
      //(Grid SpanningPanel, Panel DefaultPanel) itemsHost = this.InternalHostPanels[rowIndex][columnIndex];
      Panel itemsHost = this.InternalRowItems[calendarRowIndex].ItemsHost;
      int rowIndex = itemsHost.Children
        .Cast<UIElement>()
        .Count(eventItemContainer => Grid.GetColumn(eventItemContainer) == columnIndex);
      object item = this.Owner.GetItemFromContainer(itemContainer);
      return new EventGeneratorArgs(
        itemContainer,
        itemsHost,
        item,
        columnIndex,
        rowIndex);
    }


    private Size ArrangeDateItems(Size arrangeBounds)
    {
      int calendarIndex = 0;
      foreach (DateTime calendarViewDate in this.CurrentCalendarView.Dates)
      {
        if (!this.InternalDateHeaderItemLookupTable.TryGetValue(calendarViewDate, out UIElement dateContainer))
        {
          dateContainer = this.InternalDateHeaderItems.FirstOrDefault(
            dateItemContainer => calendarViewDate.Date.Equals(Calendar.GetDay(dateItemContainer).Date));
          if (dateContainer == null)
          {
            continue;
          }

          this.InternalDateHeaderItemLookupTable.Add(calendarViewDate, dateContainer);
        }

        int rowIndex = calendarIndex / this.ColumnCount + CalendarPanel.CalendarDateAreaRowOffset;
        int columnIndex = calendarIndex % this.ColumnCount;
        object item = this.Owner.GetItemFromContainer(dateContainer);
        var dateGeneratorArgs = new DateGeneratorArgs(
          dateContainer,
          this,
          item,
          columnIndex,
          rowIndex);

        OnAutogeneratingDate(dateGeneratorArgs);
        OnGeneratingDate(dateGeneratorArgs);
        calendarIndex++;
      }
      //foreach (UIElement itemContainer in this.InternalDateHeaderItems)
      //{
      //    int calendarDay = GetCalendarDayOf(itemContainer);

      //    int calendarIndex = calendarDay - 1;
      //    int rowIndex = calendarIndex / this.ColumnCount + 1;
      //    int columnIndex = calendarIndex % this.ColumnCount;

      //    //if (this.InternalHostPanels[rowIndex] == null)
      //    //{
      //    //    this.InternalHostPanels[rowIndex] = new Dictionary<int, (Grid SpanningPanel, Panel DefaultPanel)>();
      //    //}

      //    //Dictionary<int, (Grid SpanningPanel, Panel defaultPanel)> itemsHostMap =
      //    //    this.InternalHostPanels[rowIndex];
      //    //if (!itemsHostMap.ContainsKey(columnIndex))
      //    //{
      //    //    if (!itemContainer.IsLoaded)
      //    //    {
      //    //        itemContainer.ApplyTemplate();
      //    //    }

      //    //    if (itemContainer.TryFindVisualChildElementByName("RootGrid", out Grid grid) &&
      //    //        grid.ColumnDefinitions.Count < this.ColumnCount - columnIndex)
      //    //    {
      //    //        grid.ColumnDefinitions.Clear();
      //    //        for (var columnDefinitionIndex = 0;
      //    //            columnDefinitionIndex < this.ColumnCount - columnIndex;
      //    //            columnDefinitionIndex++)
      //    //        {
      //    //            grid.ColumnDefinitions.Add(new ColumnDefinition());
      //    //        }

      //    //        if (grid.TryFindVisualChildElementByName(
      //    //            "PART_SpanningEventItemsHost",
      //    //            out Grid spanningEventGrid))
      //    //        {
      //    //            Grid.SetColumnSpan(spanningEventGrid, this.ColumnCount - columnIndex);
      //    //            spanningEventGrid.ColumnDefinitions.Clear();
      //    //            for (var columnDefinitionIndex = 0;
      //    //                columnDefinitionIndex < this.ColumnCount - columnIndex;
      //    //                columnDefinitionIndex++)
      //    //            {
      //    //                spanningEventGrid.ColumnDefinitions.Add(new ColumnDefinition());
      //    //            }
      //    //        }

      //    //        if (grid.TryFindVisualChildElementByName("PART_EventItemsHost", out Panel eventPanel))
      //    //        {
      //    //        }

      //    //        if (!itemsHostMap.TryGetValue(columnIndex, out _))
      //    //        {
      //    //            itemsHostMap.Add(columnIndex, (spanningEventGrid, eventPanel));
      //    //        }
      //    //    }
      //    //}

      //    //Panel itemsHost = this.InternalRowItems[rowIndex].ItemsHost;
      //    object item = this.Owner.GetItemFromContainer(itemContainer);
      //    var dateGeneratorArgs = new DateGeneratorArgs(
      //        itemContainer,
      //        this,
      //        item,
      //        columnIndex,
      //        rowIndex);

      //    OnAutogeneratingDate(dateGeneratorArgs);
      //    OnGeneratingDate(dateGeneratorArgs);
      //}

      return arrangeBounds;
    }

    private CalendarView CreateCalendarViewOfMonth(int year, int month)
    {
      var datesOfCurrentView = new List<DateTime>(this.RowCount * this.ColumnCount);
      var calendarView = new CalendarView(datesOfCurrentView)
        {Index = new DateTime(year, month, 1, this.Owner.CalendarSource)};
      var firstDateOfCurrentMonth = new DateTime(year, month, 1);
      int daysInCalendarViewOverflow = 0;
      while (firstDateOfCurrentMonth.DayOfWeek != this.Owner.FirstDayOfWeek)
      {
        firstDateOfCurrentMonth = firstDateOfCurrentMonth.AddDays(-1);
        daysInCalendarViewOverflow++;
      }

      int daysInMonth = this.Owner.CalendarSource.GetDaysInMonth(year, month);
      daysInCalendarViewOverflow += this.RowCount * this.ColumnCount -
                                    daysInMonth -
                                    daysInCalendarViewOverflow;

      DateTime currentCalendarDate = firstDateOfCurrentMonth;
      for (int dayIndex = 0;
        dayIndex < daysInMonth +
        daysInCalendarViewOverflow;
        dayIndex++)
      {
        datesOfCurrentView.Add(currentCalendarDate.AddDays(dayIndex));
      }

      return calendarView;
    }

    private Size ArrangeDateColumnHeaderItems(Size arrangeBounds)
    {
      if (this.InternalDateColumnHeaderItems.Count < this.ColumnCount)
      {
        return arrangeBounds;
      }

      for (var columnIndex = 0; columnIndex < this.ColumnCount; columnIndex++)
      {
        UIElement itemContainer = this.InternalDateColumnHeaderItems[columnIndex];
        object item = this.Owner.GetItemFromContainer(itemContainer);
        var dateColumnHeaderGeneratorArgs = new DateColumnHeaderGeneratorArgs(
          itemContainer,
          this,
          item,
          columnIndex,
          0);

        OnAutogeneratingDateColumnHeader(dateColumnHeaderGeneratorArgs);
        OnGeneratingDateColumnHeader(dateColumnHeaderGeneratorArgs);
      }

      return arrangeBounds;
    }

    protected virtual void OnGeneratingDate(DateGeneratorArgs dateGeneratorArgs)
    {
      if (dateGeneratorArgs.IsCanceled)
      {
        return;
      }

      Grid.SetRow(dateGeneratorArgs.ItemContainer, dateGeneratorArgs.RowIndex);
      Grid.SetColumn(dateGeneratorArgs.ItemContainer, dateGeneratorArgs.ColumnIndex);
      //Grid.SetColumnSpan(dateGeneratorArgs.ItemContainer, this.ColumnCount - dateGeneratorArgs.ColumnIndex);
      //Panel.SetZIndex(eventGeneratorArgs.ItemContainer, 0);

      //if (dateGeneratorArgs.ItemContainer.TryFindVisualChildElementByName(
      //    "PART_SelectionBorder",
      //    out FrameworkElement selectionBorder))
      //{
      //    Grid.GetRowSpan(selectionBorder);
      //}


      if (!dateGeneratorArgs.ItemsHost.Children.Contains(dateGeneratorArgs.ItemContainer))
      {
        (dateGeneratorArgs.ItemsHost as Grid).RowDefinitions.Add(
          new RowDefinition {Height = GridLength.Auto});
        dateGeneratorArgs.ItemsHost.Children.Add(dateGeneratorArgs.ItemContainer);
      }
    }

    private void OnGeneratingCalendarEvents(EventGeneratorArgs eventGeneratorArgs)
    {
      if (eventGeneratorArgs.IsCanceled)
      {
        return;
      }

      Panel targetHost = eventGeneratorArgs.ItemsHost;
      //Panel targetHost = Grid.GetColumnSpan(eventGeneratorArgs.ItemContainer) > 1
      //    ? eventGeneratorArgs.ItemsHost.SpanningPanel
      //    : eventGeneratorArgs.ItemsHost.DefaultEventPanel;
      Grid.SetRow(eventGeneratorArgs.ItemContainer, eventGeneratorArgs.RowIndex);
      Grid.SetColumn(eventGeneratorArgs.ItemContainer, eventGeneratorArgs.ColumnIndex);
      //if (!targetHost.Children.Contains(eventGeneratorArgs.ItemContainer))
      //{
      //    targetHost.Children.Add(eventGeneratorArgs.ItemContainer);
      //}

      if (!targetHost.Children.Contains(eventGeneratorArgs.ItemContainer))
      {
        (targetHost as Grid).RowDefinitions.Add(new RowDefinition {Height = GridLength.Auto});
        targetHost.Children.Add(eventGeneratorArgs.ItemContainer);
      }
    }

    protected virtual void OnGeneratingDateColumnHeader(DateColumnHeaderGeneratorArgs dateColumnHeaderGeneratorArgs)
    {
      if (dateColumnHeaderGeneratorArgs.IsCanceled ||
          dateColumnHeaderGeneratorArgs.ItemsHost.Children.Contains(dateColumnHeaderGeneratorArgs.ItemContainer))
      {
        return;
      }

      Grid.SetRow(dateColumnHeaderGeneratorArgs.ItemContainer, 0);
      Grid.SetColumn(dateColumnHeaderGeneratorArgs.ItemContainer, dateColumnHeaderGeneratorArgs.ColumnIndex);

      if (!dateColumnHeaderGeneratorArgs.ItemsHost.Children.Contains(dateColumnHeaderGeneratorArgs.ItemContainer))
      {
        dateColumnHeaderGeneratorArgs.ItemsHost.Children.Add(dateColumnHeaderGeneratorArgs.ItemContainer);
      }
    }

    private int GetCalendarDayOf(UIElement itemContainer)
    {
      object dayValue = Calendar.GetDay(itemContainer);
      if (dayValue == null)
      {
        return 1;
        throw new NullReferenceException(
          "Attached property 'Calendar.Date' must be set. Use data binding in 'Calendar.ItemContainerStyle' or 'Calendar.SetDay' attached property setter method.");
      }

      int calendarDay;
      switch (dayValue)
      {
        case string dayString:
          calendarDay = int.Parse(dayString);
          break;
        case int intDay:
          calendarDay = intDay;
          break;
        case double doubleDay:
          calendarDay = (int) doubleDay;
          break;
        case DateTime dateTime:
          calendarDay = dateTime.Day;
          break;
        default:
          throw new ArgumentException(
            "Invalid type of 'Calendar.Date'. Type must be 'int'  or 'DateTime'.");
      }

      return calendarDay;
    }

    public IReadOnlyCollection<UIElement> GetDateItems() =>
      new ReadOnlyCollection<UIElement>(this.InternalDateHeaderItems);

    public IReadOnlyCollection<UIElement> GetEventItems() =>
      new ReadOnlyCollection<UIElement>(this.InternalEventItems);

    public IReadOnlyCollection<UIElement> GetDateColumnHeaderItems() =>
      new ReadOnlyCollection<UIElement>(this.InternalDateColumnHeaderItems);


    protected virtual void OnAutogeneratingEvent(EventGeneratorArgs e) => this.AutogeneratingEvent?.Invoke(this, e);
    protected virtual void OnAutogeneratingDate(DateGeneratorArgs e) => this.AutogeneratingDate?.Invoke(this, e);

    protected virtual void OnAutogeneratingDateColumnHeader(DateColumnHeaderGeneratorArgs e) =>
      this.AutogeneratingDateColumnHeader?.Invoke(this, e);

    public event EventHandler<EventGeneratorArgs> AutogeneratingEvent;
    public event EventHandler<DateGeneratorArgs> AutogeneratingDate;
    public event EventHandler<DateColumnHeaderGeneratorArgs> AutogeneratingDateColumnHeader;

    public List<CalendarRowItem> InternalRowItems { get; set; }

    public int RowCount
    {
      get => (int) GetValue(CalendarPanel.RowCountProperty);
      set => SetValue(CalendarPanel.RowCountProperty, value);
    }

    public int ColumnCount
    {
      get => (int) GetValue(CalendarPanel.ColumnCountProperty);
      set => SetValue(CalendarPanel.ColumnCountProperty, value);
    }

    public Calendar Owner { get; set; }
    private const int CalendarDateAreaRowOffset = 1;

    private Dictionary<int, (Grid SpanningPanel, Panel DefaultPanel)>[] InternalHostPanels { get; }
    private Dictionary<DateTime, UIElement> InternalDateHeaderItemLookupTable { get; }
    private List<UIElement> InternalDateColumnHeaderItems { get; }
    private List<UIElement> InternalDateHeaderItems { get; }
    private List<UIElement> InternalEventItems { get; }
    private bool IsLayoutDirty { get; set; }
    private Dictionary<DateTime, CalendarView> CalendarViewTable { get; }
    private CalendarView CurrentCalendarView { get; set; }

    public bool IsCalendarPanelInitialized { get; private set; }

    #region Implementation of IScrollInfo

    /// <inheritdoc />
    public void LineUp()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void LineDown()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void LineLeft()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void LineRight()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void PageUp()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void PageDown()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void PageLeft()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void PageRight()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void MouseWheelUp()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void MouseWheelDown()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void MouseWheelLeft()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void MouseWheelRight()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void SetHorizontalOffset(double offset)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void SetVerticalOffset(double offset)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Rect MakeVisible(Visual visual, Rect rectangle) => throw new NotImplementedException();

    /// <inheritdoc />
    public bool CanVerticallyScroll { get => this.canVerticallyScroll; set => this.canVerticallyScroll = value; }

    /// <inheritdoc />
    public bool CanHorizontallyScroll { get => this.canHorizontallyScroll; set => this.canHorizontallyScroll = value; }

    /// <inheritdoc />
    public double ExtentWidth { get => this.extentWidth; set => this.extentWidth = value; }

    /// <inheritdoc />
    public double ExtentHeight { get => this.extentHeight; set => this.extentHeight = value; }

    /// <inheritdoc />
    public double ViewportWidth { get => this.viewportWidth; set => this.viewportWidth = value; }

    /// <inheritdoc />
    public double ViewportHeight { get => this.viewportHeight; set => this.viewportHeight = value; }

    /// <inheritdoc />
    public double HorizontalOffset { get => this.horizontalOffset; set => this.horizontalOffset = value; }

    /// <inheritdoc />
    public double VerticalOffset { get => this.verticalOffset; set => this.verticalOffset = value; }

    /// <inheritdoc />
    public ScrollViewer ScrollOwner { get => this.scrollOwner; set => this.scrollOwner = value; }

    #endregion
  }

  internal class CalendarView
  {
    public CalendarView(List<DateTime> dates)
    {
      this.Dates = dates;
    }

    public List<DateTime> Dates { get; }
    public DateTime Index { get; set; }
  }
}