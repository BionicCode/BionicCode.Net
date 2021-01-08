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
using System.Windows.Controls;
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
      new PropertyMetadata(5, OnRowCountChanged));

    private static void OnRowCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var this_ = d as CalendarPanel;
      this_.CreateRootGrid();
      this_.UpdateCalendarLayout();
    }

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
      AddHandler(CalendarEventItem.EventItemDroppedRoutedEvent, new EventItemDroppedRoutedEventHandler(OnEventItemDropped));
      this.InternalHostPanels = new Dictionary<FrameworkElement, Panel>();
      this.InternalEventItems = new List<FrameworkElement>();
      this.InternalDateHeaderItems = new List<UIElement>();
      this.InternalDateColumnHeaderItems = new List<UIElement>();
      this.InternalRowItems = new List<CalendarRowItem>();
      this.InternalDateHeaderItemLookupTable = new Dictionary<DateTime, UIElement>();
      this.InternalDateItemToEventItemLookupTable = new Dictionary<UIElement, List<FrameworkElement>>();
      this.InternalDateHeaderToCalendarIndexTable = new Dictionary<UIElement, int>();
      this.CalendarViewTable = new Dictionary<DateTime, CalendarView>();

      this.CurrentCalendarView = new CalendarView(new List<DateTime>());
      CreateRootGrid();
    }

    private void OnEventItemDropped(object sender, EventItemDragDropArgs e)
    {
      UnhookEventItemContainer(e);

      var newDateItemContainer = e.Source as UIElement;
      if (!this.InternalDateItemToEventItemLookupTable.TryGetValue(newDateItemContainer, out List<FrameworkElement> eventItemContainersOfNewDateItemContainer))
      {
        eventItemContainersOfNewDateItemContainer = new List<FrameworkElement>() {e.ItemContainer};
        this.InternalDateItemToEventItemLookupTable.Add(newDateItemContainer, eventItemContainersOfNewDateItemContainer);
      }
      FrameworkElement equallySpanningItem = eventItemContainersOfNewDateItemContainer.Find(
        itemContainer => itemContainer != e.ItemContainer && Grid.GetColumnSpan(itemContainer) == Grid.GetColumnSpan(e.ItemContainer));

      if (!eventItemContainersOfNewDateItemContainer.Contains(e.ItemContainer))
      {
        eventItemContainersOfNewDateItemContainer.Add(e.ItemContainer);
      }

      eventItemContainersOfNewDateItemContainer.Sort((item1, item2) => Calendar.GetDay(item1).CompareTo(Calendar.GetDay(item2)));

      Panel droppedEventItemContainerHost;
        if (equallySpanningItem == null)
        {
          droppedEventItemContainerHost = new StackPanel();
          droppedEventItemContainerHost.Children.Add(e.ItemContainer);
          this.InternalHostPanels.Add(e.ItemContainer, droppedEventItemContainerHost);
        }
        else if (this.InternalHostPanels.TryGetValue(equallySpanningItem, out droppedEventItemContainerHost))
        {
          this.InternalHostPanels.Add(e.ItemContainer, droppedEventItemContainerHost);
          droppedEventItemContainerHost.Children.Add(e.ItemContainer);
          List<FrameworkElement> unsortedEventItemContainers = droppedEventItemContainerHost.Children
            .Cast<FrameworkElement>()
            .ToList();
            unsortedEventItemContainers.Sort((item1, item2) => Calendar.GetDay(item1).CompareTo(Calendar.GetDay(item2)));
            droppedEventItemContainerHost.Children.Clear();
            unsortedEventItemContainers.ForEach(item => droppedEventItemContainerHost.Children.Add(item));
      }

        IEnumerable<Panel> droppedEventItemContainerHostSiblings = eventItemContainersOfNewDateItemContainer
          .Select(itemContainer => this.InternalHostPanels[itemContainer])
          .Distinct()
          .OrderByDescending(Grid.GetColumnSpan);
        ArrangeMovedCalendarEventItem(e.ItemContainer, droppedEventItemContainerHostSiblings, newDateItemContainer);
    }

    private void UnhookEventItemContainer(EventItemDragDropArgs e)
    {
      if (this.InternalDateHeaderItemLookupTable.TryGetValue(e.OriginalDay.Date, out UIElement oldDateItemContainer))
      {
        if (this.InternalDateItemToEventItemLookupTable.TryGetValue(
          oldDateItemContainer,
          out List<FrameworkElement> eventItemContainersOfOldDateItemContainer))
        {
          eventItemContainersOfOldDateItemContainer.Remove(e.ItemContainer);
        }
      }

      if (this.InternalHostPanels.TryGetValue(e.ItemContainer, out Panel oldHostPanel))
      {
        oldHostPanel.Children.Remove(e.ItemContainer);
        this.InternalHostPanels.Remove(e.ItemContainer);
      }
    }

    private void ArrangeMovedCalendarEventItem(FrameworkElement movedEventItem, IEnumerable<Panel> movedEventItemHostSiblings, UIElement newDateItemContainer, double verticalContainerOffset = 0)
    {
      if (!this.InternalDateHeaderToCalendarIndexTable.TryGetValue(newDateItemContainer, out int calendarIndex))
      {
        return;
      }

      if (!this.InternalHostPanels.TryGetValue(movedEventItem, out Panel movedEventItemHost))
      {
        return;
      }

      double heightOffset = newDateItemContainer.DesiredSize.Height + verticalContainerOffset;
      List<Panel> siblingHosts = movedEventItemHostSiblings.ToList();
      for (var index = 0; index < siblingHosts.Count; index++)
      {
        Panel siblingHost = siblingHosts[index];
        if (siblingHost == movedEventItemHost)
        {
          var itemHostMargin = new Thickness {Top = heightOffset};
          movedEventItemHost.Margin = itemHostMargin;
          movedEventItemHost.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
          heightOffset = movedEventItemHost.DesiredSize.Height;

          for (int remainingSiblingsIndex = ++index;
            remainingSiblingsIndex < siblingHosts.Count;
            remainingSiblingsIndex++)
          {
            siblingHost = siblingHosts[index];
            var siblingHostMargin = new Thickness {Top = heightOffset};
            siblingHost.Margin = siblingHostMargin;
            heightOffset = siblingHost.DesiredSize.Height;
          }

          break;
        }

        heightOffset += siblingHost.ActualHeight;
      }

      int rowIndex = calendarIndex / this.ColumnCount + CalendarPanel.CalendarDateAreaRowOffset;
      int columnIndex = calendarIndex % this.ColumnCount;
      var eventGeneratorArgs = new EventGeneratorArgs(
        movedEventItemHost,
        this,
        movedEventItem,
        columnIndex,
        rowIndex);

      OnAutogeneratingEvent(eventGeneratorArgs);
      OnGeneratingCalendarEvents(eventGeneratorArgs);
    }

    private void ArrangeCalendarEventItemHostsOfDateItem(
      IEnumerable<Panel> eventItemContainerHostsOfDateItem,
      UIElement dateItemContainer,
      double verticalContainerOffset = 0)
    {
      double heightOffset = dateItemContainer.DesiredSize.Height + verticalContainerOffset;
      List<Panel> siblingHosts = eventItemContainerHostsOfDateItem.ToList();
      foreach (Panel siblingHost in siblingHosts)
      {
        Thickness siblingHostMargin = siblingHost.Margin;
        siblingHostMargin.Top = heightOffset;
        siblingHost.Margin = siblingHostMargin;
        siblingHost.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        heightOffset = siblingHost.DesiredSize.Height;
      }
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
        //var rowItem = e.Source as CalendarRowItem;

        //int calendarDay = GetCalendarDayOf(dateItemContainer);
        //int calendarDayIndex = calendarDay - 1;
        //int columnIndex = calendarDayIndex % this.ColumnCount;
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

    private void SpanEventItemOnSpanningRequested(object sender, EventItemDragDropArgs eventItemDragDropArgs)
    {
      var currentDateContainer = eventItemDragDropArgs.Source as UIElement;
      DateTime targetCalendarDate = Calendar.GetDay(currentDateContainer);
      int dateSpan = targetCalendarDate.Date.Subtract(eventItemDragDropArgs.OriginalDay.Date).Days + 1;
      bool isNextDateRequireCleanup =
        this.InternalHostPanels.TryGetValue(eventItemDragDropArgs.ItemContainer, out Panel currentEventItemHost) 
        && Grid.GetColumnSpan(currentEventItemHost) > dateSpan;
      if (this.InternalDateHeaderItemLookupTable.TryGetValue(
        eventItemDragDropArgs.OriginalDay.Date,
        out UIElement originalDateItemContainer))
      {
        if (this.InternalDateItemToEventItemLookupTable.TryGetValue(
          originalDateItemContainer,
          out List<FrameworkElement> originalDateEventItems))
        {
          Panel matchingSpanningHost = originalDateEventItems.Select(
            eventItemContainer => this.InternalHostPanels.TryGetValue(eventItemContainer, out Panel eventHost)
              ? eventHost
              : new StackPanel())
            .FirstOrDefault(eventHost => Grid.GetColumnSpan(eventHost).Equals(dateSpan));

          Panel eventItemContainerHost = null;
          if (matchingSpanningHost != null)
          {
            eventItemContainerHost = matchingSpanningHost;
            if (this.InternalHostPanels.TryGetValue(eventItemDragDropArgs.ItemContainer, out Panel originalHost))
            {
              originalHost.Children.Remove(eventItemDragDropArgs.ItemContainer);
              this.InternalHostPanels[eventItemDragDropArgs.ItemContainer] = matchingSpanningHost;
            }

            List<UIElement> panelChildren = matchingSpanningHost.Children.Cast<UIElement>().ToList();
            panelChildren.Add(eventItemDragDropArgs.ItemContainer);
            matchingSpanningHost.Children.Clear();
            panelChildren.Sort((element1, element2) => Calendar.GetDay(element1).CompareTo(Calendar.GetDay(element2)));
            panelChildren.ForEach(item => matchingSpanningHost.Children.Add(item));
          }
          else
          {
            if (this.InternalHostPanels.TryGetValue(eventItemDragDropArgs.ItemContainer, out Panel originalHost))
            {
              if (originalHost.Children.Count == 1)
              {
                Grid.SetColumnSpan(originalHost, dateSpan);
                eventItemContainerHost = originalHost;
              }
              else
              {
                originalHost.Children.Remove(eventItemDragDropArgs.ItemContainer);
                if (originalHost.Children.Count == 0)
                {
                  this.Children.Remove(originalHost);
                }
                
                eventItemContainerHost = new StackPanel() { VerticalAlignment = VerticalAlignment.Top };
                eventItemContainerHost.Children.Add(eventItemDragDropArgs.ItemContainer);
                eventItemContainerHost.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Grid.SetColumnSpan(eventItemContainerHost, dateSpan);
                InternalHostPanels[eventItemDragDropArgs.ItemContainer] = eventItemContainerHost;
              }
            }
          }


          List<Panel> originalDateEventItemHosts = originalDateEventItems
            .Select(itemContainer => this.InternalHostPanels[itemContainer])
            .Distinct()
            .OrderByDescending(Grid.GetColumnSpan)
            .ToList();
          int eventItemPositionInOriginalDateItem = originalDateEventItemHosts.IndexOf(eventItemContainerHost) + 1;

          ArrangeMovedCalendarEventItem(eventItemDragDropArgs.ItemContainer, originalDateEventItemHosts, originalDateItemContainer);

          if (!object.ReferenceEquals(currentDateContainer, originalDateItemContainer) 
              && this.InternalDateItemToEventItemLookupTable.TryGetValue(
            currentDateContainer,
            out List<FrameworkElement> currentDateEventItems))
          {
            IEnumerable<Panel> currentDateEventItemHosts = currentDateEventItems
              .Select(itemContainer => this.InternalHostPanels[itemContainer])
              .Distinct()
              .OrderByDescending(Grid.GetColumnSpan);
            ArrangeCalendarEventItemHostsOfDateItem(currentDateEventItemHosts,
              currentDateContainer, eventItemDragDropArgs.ItemContainer.DesiredSize.Height * eventItemPositionInOriginalDateItem);
          }

          if (isNextDateRequireCleanup)
          {
            if (this.InternalDateHeaderItemLookupTable.TryGetValue(
              targetCalendarDate.AddDays(1).Date,
              out UIElement nextDateItemContainer))
            {
              if (this.InternalDateItemToEventItemLookupTable.TryGetValue(
                nextDateItemContainer,
                out List<FrameworkElement> nextDateEventItems))
              {
                int offset = originalDateEventItemHosts.Count(panel => Grid.GetColumnSpan(panel) > dateSpan);
                IEnumerable<Panel> nextDateEventItemHosts = nextDateEventItems
                  .Select(itemContainer => this.InternalHostPanels[itemContainer])
                  .Distinct()
                  .OrderByDescending(panel => Grid.GetColumnSpan(panel));
                ArrangeCalendarEventItemHostsOfDateItem(
                  nextDateEventItemHosts,
                  nextDateItemContainer,
                  eventItemDragDropArgs.ItemContainer.DesiredSize.Height * offset);
              }
            }
          }
        }
      }
    }

    private void CreateRootGrid()
    {
      this.RowDefinitions.Clear();
      this.ColumnDefinitions.Clear();
      this.RowDefinitions.Add(new RowDefinition {Height = new GridLength(28, GridUnitType.Pixel)});
      InitializeGrid(this, new GridLength(1, GridUnitType.Star), this.RowCount);
    }


    public void ClearEventChildren()
    {
      this.InternalEventItems.Clear();
      this.InternalDateItemToEventItemLookupTable.Clear();
    }

    public void AddEventChildren(IEnumerable<FrameworkElement> children)
    {
      this.InternalEventItems.AddRange(children);
      this.IsLayoutDirty = true;
      //InvalidateMeasure();
    }

    public void ClearDateHeaderChildren()
    {
      this.InternalDateHeaderItems.Clear();
      this.InternalDateItemToEventItemLookupTable.Clear();
    }

    public void AddDateHeaderChildren(IEnumerable<UIElement> children)
    {
      this.InternalDateHeaderItems.AddRange(children);
      this.IsLayoutDirty = true;
      InvalidateMeasure();
    }

    public void ClearDateColumnHeaderChildren() => this.InternalDateColumnHeaderItems.Clear();

    public void AddDateColumnHeaderChildren(IEnumerable<UIElement> children)
    {
      this.InternalDateColumnHeaderItems.AddRange(children);
      this.IsLayoutDirty = true;
      InvalidateMeasure();
    }

    public void RemoveEventChildren(IEnumerable<FrameworkElement> childrenToRemove)
    {
      foreach (FrameworkElement uiElement in childrenToRemove)
      {
        this.InternalEventItems.Remove(uiElement);
        foreach (List<FrameworkElement> eventItemsOfDate in this.InternalDateItemToEventItemLookupTable.Select(entry => entry.Value))
        {
          eventItemsOfDate.Remove(uiElement);
        }
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

    // TODO::Last stop
    //public UIElement GetDateItemContainerOfEventItem(FrameworkElement eventItemContainer)

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
      ArrangeCalendarEventItems(arrangeSize);

      this.IsLayoutDirty = false;
      return arrangeSize;
    }

    private Size ArrangeCalendarEventItems(Size arrangeSize)
    {
      int calendarIndex = 0;
      this.InternalHostPanels.Clear();
      foreach (DateTime calendarViewDate in this.CurrentCalendarView.Dates)
      {
        if (!this.InternalDateHeaderItemLookupTable.TryGetValue(calendarViewDate, out UIElement dateContainer))
        {
          continue;
        }

        if (!this.InternalDateItemToEventItemLookupTable.TryGetValue(dateContainer, out List<FrameworkElement> eventContainers))
        {
          eventContainers = new List<FrameworkElement>(
            this.InternalEventItems.Where(
              eventItemContainer => calendarViewDate.Date.Equals(Calendar.GetDay(eventItemContainer).Date)));
          if (!eventContainers.Any())
          {
            continue;
          }

          this.InternalDateItemToEventItemLookupTable.Add(dateContainer, eventContainers);
        }

        int rowIndex = calendarIndex / this.ColumnCount + CalendarPanel.CalendarDateAreaRowOffset;
        int columnIndex = calendarIndex % this.ColumnCount;
        var hostPanel = new StackPanel() {VerticalAlignment = VerticalAlignment.Top};
        
        for (var index = 0; index < eventContainers.Count; index++)
        {
          FrameworkElement itemContainer = eventContainers[index];
          hostPanel.Children.Add(itemContainer);
          this.InternalHostPanels.Add(itemContainer, hostPanel);
        }
        Thickness itemMargin = hostPanel.Margin;
        dateContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        itemMargin.Top = dateContainer.DesiredSize.Height;
        hostPanel.Margin = itemMargin;
        var eventGeneratorArgs = new EventGeneratorArgs(
            hostPanel,
            this,
            null,
            columnIndex,
            rowIndex);

          OnAutogeneratingEvent(eventGeneratorArgs);
          OnGeneratingCalendarEvents(eventGeneratorArgs);

        calendarIndex++;
      }

      return arrangeSize;
    }

    private Size ArrangeDateItems(Size arrangeBounds)
    {
      this.InternalDateHeaderToCalendarIndexTable.Clear();
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

        this.InternalDateHeaderToCalendarIndexTable.Add(dateContainer, calendarIndex);

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
        //(dateGeneratorArgs.ItemsHost as Grid).RowDefinitions.Add(
        //  new RowDefinition {Height = GridLength.Auto});
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
        //(targetHost as Grid).RowDefinitions.Add(new RowDefinition {Height = GridLength.Auto});
        //var itemHost = new DockPanel();
        //itemHost.Children.Add(eventGeneratorArgs.ItemContainer);
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

    public IReadOnlyCollection<FrameworkElement> GetEventItems() =>
      new ReadOnlyCollection<FrameworkElement>(this.InternalEventItems);

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

    private Dictionary<FrameworkElement, Panel> InternalHostPanels { get; }
    private Dictionary<DateTime, UIElement> InternalDateHeaderItemLookupTable { get; }
    private Dictionary<UIElement, int> InternalDateHeaderToCalendarIndexTable { get; }
    private Dictionary<UIElement, List<FrameworkElement>> InternalDateItemToEventItemLookupTable { get; }
    private List<UIElement> InternalDateColumnHeaderItems { get; }
    private List<UIElement> InternalDateHeaderItems { get; }
    private List<FrameworkElement> InternalEventItems { get; }
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