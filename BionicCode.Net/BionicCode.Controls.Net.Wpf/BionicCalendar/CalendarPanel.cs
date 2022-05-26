
namespace BionicCode.Controls.Net.Wpf
{
  #region Info

  // 2020/11/06  20:13
  // Activitytracker

  #endregion

  #region Usings

  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Collections.Specialized;
  using System.Globalization;
  using System.Linq;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Controls.Primitives;
  using System.Windows.Media;
  using ColumnDefinition = System.Windows.Controls.ColumnDefinition;
  using Grid = System.Windows.Controls.Grid;
  using Panel = System.Windows.Controls.Panel;
  using RowDefinition = System.Windows.Controls.RowDefinition;
  using ScrollViewer = System.Windows.Controls.ScrollViewer;
  using BionicCode.Utilities.Net.Wpf.Extensions;

  #endregion

  public class CalendarPanel : VirtualizingPanel, IScrollInfo
  {
    public static string DefaultCalendarDateItemTemplateKeyId => nameof(CalendarPanel.DefaultCalendarDateItemTemplateKey);

    public static ComponentResourceKey DefaultCalendarDateItemTemplateKey { get; } = new ComponentResourceKey()
    {
      TypeInTargetAssembly = typeof(CalendarPanel), 
      ResourceId = CalendarPanel.DefaultCalendarDateItemTemplateKeyId
    }; 

    #region CurrentViewChangedRoutedEvent

    public static readonly RoutedEvent CurrentViewChangedRoutedEvent = EventManager.RegisterRoutedEvent("CurrentViewChanged",
      RoutingStrategy.Bubble, typeof(CalendarViewChangedRoutedEventHandler<CalendarMonthView>), typeof(CalendarPanel));

    public event RoutedEventHandler CurrentViewChanged
    {
      add { AddHandler(CalendarPanel.CurrentViewChangedRoutedEvent, value); }
      remove { RemoveHandler(CalendarPanel.CurrentViewChangedRoutedEvent, value); }
    }

    #endregion

    #region DatesRealizedRoutedEvent

    public static readonly RoutedEvent DatesRealizedRoutedEvent = EventManager.RegisterRoutedEvent("DatesRealized",
      RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CalendarPanel));

    public event RoutedEventHandler DatesRealized
    {
      add { AddHandler(CalendarPanel.DatesRealizedRoutedEvent, value); }
      remove { RemoveHandler(CalendarPanel.DatesRealizedRoutedEvent, value); }
    }

    #endregion

    public static readonly DependencyProperty RowCountProperty = DependencyProperty.Register(
      "RowCount",
      typeof(int),
      typeof(CalendarPanel),
      new FrameworkPropertyMetadata(5, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    public static readonly DependencyProperty ColumnCountProperty = DependencyProperty.Register(
      "ColumnCount",
      typeof(int),
      typeof(CalendarPanel),
      new PropertyMetadata(7));

    public static readonly DependencyProperty GridColorProperty = DependencyProperty.Register(
      "GridColor",
      typeof(Brush),
      typeof(CalendarPanel),
      new FrameworkPropertyMetadata(Brushes.DimGray, FrameworkPropertyMetadataOptions.AffectsRender));


    public Brush GridColor
    {
      get => (Brush) GetValue(CalendarPanel.GridColorProperty);
      set => SetValue(CalendarPanel.GridColorProperty, value);
    }

    public static readonly DependencyProperty GridThicknessProperty = DependencyProperty.Register(
      "GridThickness",
      typeof(double),
      typeof(CalendarPanel),
      new FrameworkPropertyMetadata(0.2, FrameworkPropertyMetadataOptions.AffectsRender));

    public double GridThickness
    {
      get => (double) GetValue(CalendarPanel.GridThicknessProperty);
      set => SetValue(CalendarPanel.GridThicknessProperty, value);
    }

    #region IsShowingCalendarWeek dependency property

    public static readonly DependencyProperty IsShowingCalendarWeekProperty = DependencyProperty.Register(
      "IsShowingCalendarWeek",
      typeof(bool),
      typeof(CalendarPanel),
      new PropertyMetadata(true, CalendarPanel.OnIsShowingCalendarWeekChanged));

    public bool IsShowingCalendarWeek { get => (bool) GetValue(CalendarPanel.IsShowingCalendarWeekProperty); set => SetValue(CalendarPanel.IsShowingCalendarWeekProperty, value); }

    #endregion IsShowingCalendarWeek dependency property

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
      AddHandler(CalendarEventItem.EventItemDroppedRoutedEvent, new EventItemDroppedRoutedEventHandler(OnEventItemDropped));
      this.InternalHostPanels = new Dictionary<UIElement, Panel>();
      this.InternalEventItems = new List<UIElement>();
      this.InternalRecycledDateItemContainerLookupTable = new Stack<UIElement>();
      this.InternalDateItemLookupTable = new Dictionary<DateTime, CalendarDate>();
      this.InternalDateColumnHeaderItems = new List<UIElement>();
      this.ItemsHost = new Grid() {Background = Brushes.Transparent};
      this.InternalDateItemContainerLookupTable = new Dictionary<DateTime, UIElement>();
      this.InternalDateToEventItemContainersSnapshotTable = new Dictionary<DateTime, List<UIElement>>();
      this.InternalEventItemToEventItemContainerSnapshotTable = new Dictionary<object, UIElement>();
      this.InternalDateItemContainerToCalendarIndexTable = new Dictionary<UIElement, int>();
      this.InternalCalendarIndexToWeekNumberTable = new Dictionary<int, int>();
      this.InternalDateItemContainerToDateItemTable = new Dictionary<UIElement, CalendarDate>();
      this.InternalRowIndexToWeekNumberItemTable = new Dictionary<int, UIElement>();
      this.PreviousScrollDirection = ScrollDirection.Bottom;
      this.ContentRowOffset = 1;
      this.ContentColumnOffset = this.IsShowingCalendarWeek ? 1 : 0;

      CreateRootGrid();
    }

    private void OnEventItemDropped(object sender, EventItemDragDropArgs e)
    {

      var newDateItemContainer = e.Source as UIElement;
      CalendarDate newDateItem = this.InternalDateItemContainerToDateItemTable[newDateItemContainer];
      DateTime dropDate = newDateItem.Day.Date;

      UnhookEventItemContainer(e, dropDate);
      if (!this.InternalDateToEventItemContainersSnapshotTable.TryGetValue(dropDate, out List<UIElement> eventItemContainersOfNewDateItemContainer))
      {
        eventItemContainersOfNewDateItemContainer = new List<UIElement>() {e.ItemContainer};
        this.InternalDateToEventItemContainersSnapshotTable.Add(dropDate, eventItemContainersOfNewDateItemContainer);
      }
      UIElement equallySpanningItem = eventItemContainersOfNewDateItemContainer.Find(
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
        double verticalContainerOffset = (this.DateColumnItemOffsetTable.TryGetValue(dropDate, out int precedingItemCount) ? precedingItemCount : 0) * e.ItemContainer.DesiredSize.Height;
        ArrangeMovedCalendarEventItem(e.ItemContainer, droppedEventItemContainerHostSiblings, newDateItemContainer, verticalContainerOffset);
    }

    private void UnhookEventItemContainer(EventItemDragDropArgs e, DateTime date)
    {
      if (this.InternalDateItemContainerLookupTable.TryGetValue(e.OriginalDay.Date, out UIElement oldDateItemContainer))
      {
        if (this.InternalDateToEventItemContainersSnapshotTable.TryGetValue(
          date.Date,
          out List<UIElement> eventItemContainersOfOldDateItemContainer))
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

    private void ArrangeMovedCalendarEventItem(FrameworkElement movedEventItem, IEnumerable<Panel> movedEventItemHostSiblings, UIElement newDateItemContainer, double verticalContainerOffset = 1)
    {
      if (!this.InternalDateItemContainerToCalendarIndexTable.TryGetValue(newDateItemContainer, out int calendarIndex))
      {
        return;
      }

      if (!this.InternalHostPanels.TryGetValue(movedEventItem, out Panel movedEventItemHost))
      {
        return;
      }
      newDateItemContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
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

      int rowIndex = calendarIndex / this.ColumnCount + this.ContentRowOffset;
      int columnIndex = calendarIndex % this.ColumnCount;
      CalendarDate currentCalendarDate = this.InternalDateItemContainerToDateItemTable[newDateItemContainer];
      var eventGeneratorArgs = new EventGeneratorArgs(
        movedEventItemHost,
        this,
        this.InternalDateToEventItemContainersSnapshotTable[currentCalendarDate.Day.Date],
        columnIndex,
        rowIndex,
        currentCalendarDate.Day);

      OnAutoGeneratingEvent(eventGeneratorArgs);
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

      DrawGridLines(this.ItemsHost, drawingContext);
    }

    protected virtual void DrawGridLines(Panel panel, DrawingContext drawingContext)
    {
#if NET5_0_OR_GREATER      
      if (panel is not Grid gridPanel)
      {
        return;
      }
#else    
      if (!(panel is Grid gridPanel))
      {
        return;
      }
#endif

      var pen = new Pen(this.GridColor, this.GridThickness);
      double horizontalStartOffset = this.IsShowingCalendarWeek ? gridPanel.ColumnDefinitions[0].ActualWidth / 2 : 0;
      double verticalStartOffset = gridPanel.RowDefinitions[0].ActualHeight / 2;
      var verticalLineStart = new Point(0, verticalStartOffset);
      var horizontalLineStart = new Point(horizontalStartOffset, 0);
      var verticalLineEnd = new Point(0, gridPanel.ActualHeight);
      var horizontalLineEnd = new Point(gridPanel.ActualWidth, 0);

      //drawingContext.DrawLine(pen, horizontalLineStart, horizontalLineEnd);
      //drawingContext.DrawLine(pen, verticalLineStart, verticalLineEnd);

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


    private void SpanEventItemOnSpanningRequested(object sender, EventItemDragDropArgs eventItemDragDropArgs)
    {
      var currentDateContainer = eventItemDragDropArgs.Source as UIElement;
      CalendarDate currentDateItem = this.InternalDateItemContainerToDateItemTable[currentDateContainer];
      
      DateTime newTargetCalendarDate = currentDateItem.Day.Date;
      int newDateSpan = newTargetCalendarDate.Date.Subtract(eventItemDragDropArgs.OriginalDay.Date).Days + 1;
      bool isNextDateRequireCleanup =
        this.InternalHostPanels.TryGetValue(eventItemDragDropArgs.ItemContainer, out Panel currentEventItemHost) 
        && Grid.GetColumnSpan(currentEventItemHost) > newDateSpan;
      bool isIncreasingSpan = !isNextDateRequireCleanup && !(this.InternalHostPanels.TryGetValue(eventItemDragDropArgs.ItemContainer, out Panel eventItemHost) && this.PreviouslySpannedPanelsTable.TryGetValue(eventItemHost, out DateTime oldTargetDate) && oldTargetDate.Equals(newTargetCalendarDate));

      if (this.InternalDateItemContainerLookupTable.TryGetValue(
        eventItemDragDropArgs.OriginalDay.Date,
        out UIElement originalDateItemContainer))
      {
        if (this.InternalDateToEventItemContainersSnapshotTable.TryGetValue(
          newTargetCalendarDate,
          out List<UIElement> originalDateEventItems))
        {
          Panel matchingSpanningHost = originalDateEventItems.Select(
            eventItemContainer => this.InternalHostPanels.TryGetValue(eventItemContainer, out Panel eventHost)
              ? eventHost
              : new StackPanel())
            .FirstOrDefault(eventHost => Grid.GetColumnSpan(eventHost).Equals(newDateSpan));

          Panel eventItemContainerHost = null;
          if (matchingSpanningHost != null)
          {
            eventItemContainerHost = matchingSpanningHost;
            if (this.InternalHostPanels.TryGetValue(eventItemDragDropArgs.ItemContainer, out Panel originalHost))
            {
              originalHost.Children.Remove(eventItemDragDropArgs.ItemContainer);
              this.InternalHostPanels[eventItemDragDropArgs.ItemContainer] = eventItemContainerHost;
            }

            List<UIElement> panelChildren = eventItemContainerHost.Children.Cast<UIElement>().ToList();
            panelChildren.Add(eventItemDragDropArgs.ItemContainer);
            eventItemContainerHost.Children.Clear();
            panelChildren.Sort((element1, element2) => Calendar.GetDay(element1).CompareTo(Calendar.GetDay(element2)));
            panelChildren.ForEach(item => eventItemContainerHost.Children.Add(item));
          }
          else
          {
            if (this.InternalHostPanels.TryGetValue(eventItemDragDropArgs.ItemContainer, out Panel originalHost))
            {
              if (originalHost.Children.Count == 1)
              {
                Grid.SetColumnSpan(originalHost, newDateSpan);
                eventItemContainerHost = originalHost;
              }
              else
              {
                originalHost.Children.Remove(eventItemDragDropArgs.ItemContainer);
                if (originalHost.Children.Count == 0)
                {
                  this.Children.Remove(originalHost);
                }
                
                eventItemContainerHost = new StackPanel() { VerticalAlignment = VerticalAlignment.Top};
                eventItemContainerHost.Children.Add(eventItemDragDropArgs.ItemContainer);
                eventItemContainerHost.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Grid.SetColumnSpan(eventItemContainerHost, newDateSpan);
                this.InternalHostPanels[eventItemDragDropArgs.ItemContainer] = eventItemContainerHost;

                //for (int dayOffset = 1; dayOffset < dateSpan; dayOffset++)
                //{0
                //  DateTime key = eventItemDragDropArgs.OriginalDay.Date.AddDays(dayOffset);
                //  this.DateColumnItemOffsetTable[eventItemDragDropArgs.OriginalDay.Date] += 1;
                //}
              }
            }
          }

          this.PreviouslySpannedPanelsTable[eventItemContainerHost] = newTargetCalendarDate.Date;

          if (isIncreasingSpan)
          {
            if (!this.DateColumnItemOffsetTable.ContainsKey(newTargetCalendarDate))
            {
              this.DateColumnItemOffsetTable.Add(newTargetCalendarDate, 0);
            }

            this.DateColumnItemOffsetTable[newTargetCalendarDate] += 1;
          }

          List<Panel> originalDateEventItemHosts = originalDateEventItems
            .Select(itemContainer => this.InternalHostPanels[itemContainer])
            .Distinct()
            .OrderByDescending(Grid.GetColumnSpan)
            .ToList();
          int precedingItemsOffset = this.DateColumnItemOffsetTable.TryGetValue(
            eventItemDragDropArgs.OriginalDay.Date,
            out int itemOffset)
            ? itemOffset
            : 0;

          ArrangeMovedCalendarEventItem(eventItemDragDropArgs.ItemContainer, originalDateEventItemHosts, originalDateItemContainer, precedingItemsOffset * eventItemDragDropArgs.ItemContainer.DesiredSize.Height);

          if (!object.ReferenceEquals(currentDateContainer, originalDateItemContainer) 
              && this.InternalDateToEventItemContainersSnapshotTable.TryGetValue(
            newTargetCalendarDate,
            out List<UIElement> currentDateEventItems) && currentDateEventItems.Any())
          {
            IEnumerable<Panel> currentDateEventItemHosts = currentDateEventItems
              .Select(itemContainer => this.InternalHostPanels[itemContainer])
              .Distinct()
              .OrderByDescending(Grid.GetColumnSpan);
            ArrangeCalendarEventItemHostsOfDateItem(currentDateEventItemHosts,
              currentDateContainer, this.DateColumnItemOffsetTable[newTargetCalendarDate] * eventItemDragDropArgs.ItemContainer.DesiredSize.Height);
          }

          if (isNextDateRequireCleanup)
          {
            DateTime nextDate = newTargetCalendarDate.AddDays(1).Date;
            if (this.InternalDateItemContainerLookupTable.TryGetValue(
              nextDate,
              out UIElement nextDateItemContainer))
            {
              this.DateColumnItemOffsetTable[nextDate] -= 1;
              if (this.InternalDateToEventItemContainersSnapshotTable.TryGetValue(
                nextDate,
                out List<UIElement> nextDateEventItems) && nextDateEventItems.Any())
              {
                double offset = this.DateColumnItemOffsetTable[nextDate] *
                                eventItemDragDropArgs.ItemContainer.DesiredSize.Height;
                IEnumerable<Panel> nextDateEventItemHosts = nextDateEventItems
                  .Select(itemContainer => this.InternalHostPanels[itemContainer])
                  .Distinct()
                  .OrderByDescending(panel => Grid.GetColumnSpan(panel));
                ArrangeCalendarEventItemHostsOfDateItem(
                  nextDateEventItemHosts,
                  nextDateItemContainer,
                  offset);
              }
            }
          }
        }
      }
    }

    private void CreateRootGrid()
    {
      this.ItemsHost.RowDefinitions.Clear();
      this.ItemsHost.ColumnDefinitions.Clear();
      this.ItemsHost.RowDefinitions.Add(new RowDefinition {Height = new GridLength(32, GridUnitType.Pixel)});
      if (this.IsShowingCalendarWeek)
      {
        this.ItemsHost.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(32, GridUnitType.Pixel)});
      }
      InitializeGrid(this.ItemsHost, new GridLength(1, GridUnitType.Star), this.RowCount);
    }

    private void InitializeGrid(Grid grid, GridLength rowHeight, int rowCount, int rowStartIndex = 0)
    {
      for (int rowIndex = rowStartIndex; rowIndex < rowCount; rowIndex++)
      {
        grid.RowDefinitions.Add(new RowDefinition {MinHeight = 48, Height = rowHeight});
      }

      for (var columnIndex = 0; columnIndex < this.ColumnCount; columnIndex++)
      {
        grid.ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)});
      }
    }

    /// <summary>
    /// The get method of <see cref="Panel.InternalChildren"/> actually creates and connects the <see cref="ItemContainerGenerator"/>. Invoking this method ensures that this will happen. Otherwise the generator would be <c>null</c> until internals access the <see cref="Panel.InternalChildren"/> property. Info was deducted from source code review.
    /// </summary>
    protected void EnsureGenerator() => _ = this.InternalChildren;

    // TODO::Last stop
    //public UIElement GetDateItemContainerOfEventItem(FrameworkElement eventItemContainer)

#region Overrides of FrameworkElement

    /// <inheritdoc />
    protected override void OnInitialized(EventArgs e)
    {
      base.OnInitialized(e);
      Initialize(null);
    }

#endregion

    public void Initialize(Calendar owner)
    {
      EnsureGenerator();

      if (owner == null && this.TryFindVisualParentElement(out owner))
      {
      }

      this.Owner = owner;

      CalendarMonthView monthView = CreateCalendarViewOfMonth(DateTime.Now.Year, DateTime.Now.Month);
      this.CurrentCalendarView = monthView;
      this.NextCalendarView = this.CurrentCalendarView.GetNext();
      this.IsCalendarPanelInitialized = true;

      CreateRootGrid();
    }

#region Overrides of Grid

    /// <inheritdoc />
    protected override Size MeasureOverride(Size constraint)
    {
      if (!this.InternalChildren.Contains(this.ItemsHost))
      {
        AddInternalChild(this.ItemsHost);
      }
      constraint = base.MeasureOverride(constraint);
      this.ItemsHost.Measure(constraint);

      if (this.PreviousScrollDirection == ScrollDirection.Top)
      {
        int weekCountInCurrentView = this.CurrentCalendarView.WeekCount - this.WeekRealizationOffset;
        int weekCountInNextView = this.RowCount - weekCountInCurrentView;
        int unusedWeekCountInNextView = this.NextCalendarView.WeekCount - weekCountInNextView - (IsCurrentMonthViewOverflowing() ? 1 : 0);
        RecycleUnusedDateContainers(0, unusedWeekCountInNextView, this.PreviousScrollDirection);
      }
      else
      { 
        RecycleUnusedDateContainers(0, this.WeekRealizationOffset, this.PreviousScrollDirection);
      }

      this.ItemsHost.Children.Clear();
      this.InternalDateItemContainerToCalendarIndexTable.Clear();
      this.InternalCalendarIndexToWeekNumberTable.Clear();
      int calendarIndex = 0;
      int weekIndex = -1;
      RealizeDatesOfCurrentView(ref weekIndex, ref calendarIndex, this.DesiredSize);

      RealizeDatesOfNextView(ref weekIndex, ref calendarIndex, this.DesiredSize);
      RaiseEvent(new RoutedEventArgs(CalendarPanel.DatesRealizedRoutedEvent, this));

      if (this.IsShowingCalendarWeek)
      {
        List<int> weekNumbers = this.InternalCalendarIndexToWeekNumberTable.Values.Distinct().ToList();
        for (int rowIndex = 0; rowIndex < weekNumbers.Count; rowIndex++)
        {
          if (!this.InternalRowIndexToWeekNumberItemTable.TryGetValue(rowIndex + this.ContentRowOffset, out UIElement weekHeaderItemContainer))
          {
            weekHeaderItemContainer = this.Owner.GetContainerForWeekHeaderItem();
            this.InternalRowIndexToWeekNumberItemTable.Add(rowIndex + this.ContentRowOffset, weekHeaderItemContainer);
          }
          this.Owner.PrepareContainerForWeekHeaderItemOverride(weekHeaderItemContainer, weekNumbers[rowIndex]);
          this.ItemsHost.Children.Add(weekHeaderItemContainer);
        }
      }


      if (!this.InternalDateColumnHeaderItems.Any())
      {
        for (int columnIndex = 0; columnIndex < this.ColumnCount; columnIndex++)
        {
          UIElement columnHeaderItem = this.Owner.GetContainerForDateColumnHeaderItem();
          this.Owner.PrepareContainerForCalendarDateColumnHeaderItemOverride(columnHeaderItem, (DayOfWeek) (((int) this.Owner.FirstDayOfWeek + columnIndex) % 7));
          this.InternalDateColumnHeaderItems.Add(columnHeaderItem);
          this.ItemsHost.Children.Add(columnHeaderItem);
        }

        UIElement dummyColumnHeaderItem = this.Owner.GetContainerForDateColumnHeaderItem();
        this.Owner.PrepareContainerForCalendarDateColumnHeaderItemOverride(dummyColumnHeaderItem, string.Empty);
        this.InternalDateColumnHeaderItems.Add(dummyColumnHeaderItem);
        this.ItemsHost.Children.Add(dummyColumnHeaderItem);
      }
      else
      {
        for (var columnIndex = 0; columnIndex < this.ColumnCount; columnIndex++)
        {
          UIElement dateColumnHeaderItem = this.InternalDateColumnHeaderItems[columnIndex];
          this.Owner.PrepareContainerForCalendarDateColumnHeaderItemOverride(dateColumnHeaderItem, (DayOfWeek)(((int)this.Owner.FirstDayOfWeek + columnIndex) % 7));
          this.ItemsHost.Children.Add(dateColumnHeaderItem);
        }
        UIElement dummyColumnHeaderItem = this.Owner.GetContainerForDateColumnHeaderItem();
        this.ItemsHost.Children.Add(dummyColumnHeaderItem);
      }



      this.InternalEventItems.Clear();
      this.InternalDateToEventItemContainersSnapshotTable.Clear();
      this.InternalEventItemToEventItemContainerSnapshotTable.Clear();
      var generator = this.ItemContainerGenerator as ItemContainerGenerator;
      using (generator.GenerateBatches())
      {
        int generatorIndex = 0;
        using (this.ItemContainerGenerator.StartAt(new GeneratorPosition(-1, 1), GeneratorDirection.Forward, true))
        {
          for (int itemIndex = 0; itemIndex < generator.Items.Count; itemIndex++)
          {
            object eventItem = generator.Items[itemIndex];
            var eventItemContainer = this.ItemContainerGenerator.GenerateNext(out bool isNewlyRealized) as UIElement;
            this.ItemContainerGenerator.PrepareItemContainer(eventItemContainer);
            eventItemContainer.Measure(constraint);
            this.ItemsHost.Children.Add(eventItemContainer);
            DateTime eventDate = Calendar.GetDay(eventItemContainer);

            if (!this.CurrentCalendarView.ContainsDate(eventDate.Date))
            {
              this.ItemsHost.Children.Remove(eventItemContainer);
              (generator as IRecyclingItemContainerGenerator)?.Recycle(
                this.ItemContainerGenerator.GeneratorPositionFromIndex(generatorIndex), 1);
            }
            else
            {
              generatorIndex++;
              this.InternalEventItems.Add(eventItemContainer);
              this.InternalEventItemToEventItemContainerSnapshotTable.Add(eventItem, eventItemContainer);

              if (eventDate.Equals(default))
              {
                continue;
              }

              if (!this.InternalDateToEventItemContainersSnapshotTable.TryGetValue(eventDate.Date, out List<UIElement> eventContainers))
              {
                eventContainers = new List<UIElement>();
                this.InternalDateToEventItemContainersSnapshotTable.Add(eventDate.Date, eventContainers);
              }
              eventContainers.Add(eventItemContainer);
            }
          }
        }
      }

      return constraint;
    }

#endregion

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size arrangeSize)
    {
      this.ItemsHost.Arrange(new Rect(arrangeSize));
      base.ArrangeOverride(arrangeSize);
      if (!this.IsCalendarPanelInitialized)
      {
        throw new InvalidOperationException(
          "'CalendarPanel' instance not initialized. Make sure to call 'CalendarPanel.Initialize()' before the panel is entering the layout passing.");
      }

      if (!this.IsLayoutDirty)
      {
        //return arrangeSize;
      }

      ArrangeDateItems(arrangeSize);
      ArrangeDateColumnHeaderItems(arrangeSize);
      ArrangeCalendarEventItems(arrangeSize);

      this.IsLayoutDirty = false;
      //UpdateLayout();
      return arrangeSize;
    }

    private Size ArrangeCalendarEventItems(Size arrangeSize)
    {
      this.InternalHostPanels.Clear();
      
      List<UIElement> eventItems = this.InternalEventItems.ToList();
      var dateToEventItemContainerCacheTable = new Dictionary<DateTime, List<UIElement>>();

      foreach (DateTime calendarViewDate in this.CurrentCalendarView.Dates.Union(this.CurrentCalendarView.GetNext().Dates))
      {
        if (!IsDateRealized(calendarViewDate))
        {
          continue;
        }

        if (!this.InternalDateToEventItemContainersSnapshotTable.TryGetValue(calendarViewDate.Date, out List<UIElement> eventItemContainersOfCurrentDate))
        {
          continue;
        }

        if (!eventItemContainersOfCurrentDate.Any())
        {
          continue;
        }

        var hostPanel = new StackPanel() {VerticalAlignment = VerticalAlignment.Top};
        
        var items = new List<object>();
        foreach (UIElement itemContainer in eventItemContainersOfCurrentDate)
        {
          ((itemContainer as FrameworkElement)?.Parent as Panel)?.Children.Remove(itemContainer);
          hostPanel.Children.Add(itemContainer);
          this.InternalHostPanels.Add(itemContainer, hostPanel);
          object item = (this.ItemContainerGenerator as ItemContainerGenerator).ItemFromContainer(itemContainer);
          items.Add(item);
        }

        Thickness itemMargin = hostPanel.Margin;

        UIElement dateContainer = this.InternalDateItemContainerLookupTable[calendarViewDate.Date];
        int calendarIndex = this.InternalDateItemContainerToCalendarIndexTable[dateContainer];
        int rowIndex = calendarIndex / this.ColumnCount + this.ContentRowOffset;
        int columnIndex = calendarIndex % this.ColumnCount + this.ContentColumnOffset;
        if (!dateContainer.IsMeasureValid)
        {
          dateContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        }
        itemMargin.Top = dateContainer.DesiredSize.Height;
        hostPanel.Margin = itemMargin;

        var eventGeneratorArgs = new EventGeneratorArgs(
            hostPanel,
            this.ItemsHost,
            items,
            columnIndex,
            rowIndex,
            calendarViewDate.Date);

          OnAutoGeneratingEvent(eventGeneratorArgs);
          OnGeneratingCalendarEvents(eventGeneratorArgs);
      }

      return arrangeSize;
    }

    private Size ArrangeDateItems(Size arrangeBounds)
    {
      foreach ((UIElement key, int value) in this.InternalDateItemContainerToCalendarIndexTable)
      {
        DateGeneratorArgs dateGeneratorArgs = CreateDateGeneratorArgs(value, key);

        OnAutoGeneratingDate(dateGeneratorArgs);
        OnGeneratingDate(dateGeneratorArgs);
      }
    
      return arrangeBounds;
    }

    private void RecycleUnusedDateContainers(int recycleWeekOffset = 0, int recycleWeekCount = -1, ScrollDirection scrollDirection = ScrollDirection.Undefined)
    {
      if (!this.InternalDateItemContainerLookupTable.Any())
      {
        return;
      }

      if (recycleWeekCount == -1)
      {
        for (var index = this.InternalDateItemContainerLookupTable.Count - 1; index >= 0; index--)
        {
          DateTime recycleDate = this.InternalDateItemContainerLookupTable.Keys.ElementAt(index);
          this.InternalDateItemContainerLookupTable.Remove(recycleDate, out UIElement itemContainerToRecycle);
          this.InternalDateItemContainerToDateItemTable.Remove(itemContainerToRecycle);
          this.InternalDateItemLookupTable.Remove(recycleDate);
          this.Owner.ClearItemContainer(itemContainerToRecycle);
          //Selector.SetIsSelected(itemContainerToRecycle, false);
          this.InternalRecycledDateItemContainerLookupTable.Push(itemContainerToRecycle);
        }
        return;
      }

      switch (scrollDirection)
      {
        case ScrollDirection.Bottom:
        {
          RecycleLeadingContainers(recycleWeekOffset, recycleWeekCount);

          int remainingWeekCountOfCurrentView = this.CurrentCalendarView.WeekCount - recycleWeekCount;
          if (remainingWeekCountOfCurrentView < this.RowCount)
          {
            int missingWeeksCount = this.RowCount - remainingWeekCountOfCurrentView;
            recycleWeekCount = this.NextCalendarView.WeekCount - missingWeeksCount;
            RecycleTrailingContainers(0, recycleWeekCount);
          }
          break;
        }
        case ScrollDirection.Top:
        {
          RecycleTrailingContainers(recycleWeekOffset, recycleWeekCount);

          break;
        }
      }
    }

    private void RecycleLeadingContainers(int recycleWeekOffset, int recycleWeekCount)
    {
      foreach (DateTime dateTime in this.CurrentCalendarView
        .Skip(recycleWeekOffset)
        .Take(recycleWeekCount)
        .SelectMany(calendarWeekView => calendarWeekView.Dates))
      {
        if (this.InternalDateItemContainerLookupTable.Remove(dateTime, out UIElement recycleReadyContainer))
        {
          this.InternalDateItemContainerToDateItemTable.Remove(recycleReadyContainer);
          this.Owner.ClearItemContainer(recycleReadyContainer);
          this.InternalDateItemLookupTable.Remove(dateTime);
          this.InternalRecycledDateItemContainerLookupTable.Push(recycleReadyContainer);
        }
      }
    }

    private void RecycleTrailingContainers(int recycleWeekOffset, int recycleWeekCount)
    {
      foreach (DateTime dateTime in this.NextCalendarView
        .Reverse()
        .Skip(recycleWeekOffset)
        .Take(recycleWeekCount)
        .SelectMany(calendarWeekView => calendarWeekView.Dates))
      {
        if (this.InternalDateItemContainerLookupTable.Remove(dateTime, out UIElement recycleReadyContainer))
        {
          this.InternalDateItemContainerToDateItemTable.Remove(recycleReadyContainer);
          this.Owner.ClearItemContainer(recycleReadyContainer);
          this.InternalDateItemLookupTable.Remove(dateTime);
          this.InternalRecycledDateItemContainerLookupTable.Push(recycleReadyContainer);
        }
      }
    }

    private void RealizeDatesOfNextView(ref int weekIndex, ref int calendarIndex, Size constraint)
    {
      this.NextCalendarView.Reset();
      if (++weekIndex < this.RowCount)
      {
        for (; weekIndex < this.RowCount; weekIndex++)
        {
          if (!this.NextCalendarView.MoveNext())
          {
            break;
          }

          CalendarWeekView appendingWeekView = this.NextCalendarView.Current;

          foreach (UIElement dateItemContainer in EnumerateNewItemContainer(appendingWeekView.Dates))
          {
            this.InternalDateItemContainerToCalendarIndexTable.Add(dateItemContainer, calendarIndex);
            this.InternalCalendarIndexToWeekNumberTable.Add(calendarIndex, appendingWeekView.WeekNumber);
            dateItemContainer.Measure(constraint);
            this.ItemsHost.Children.Add(dateItemContainer);
            calendarIndex++;
          }
        }
      }
    }

    private void RealizeDatesOfCurrentView(ref int weekIndex, ref int calendarIndex, Size constraint)
    {
      foreach (CalendarWeekView weekView in this.CurrentCalendarView.Skip(this.WeekRealizationOffset))
      {
        if (++weekIndex >= Math.Min(this.RowCount, this.CurrentCalendarView.WeekCount))
        {
          break;
        }

        if (this.WeekRealizationOffset > 0
            //&& !IsRemainingWeeksOfViewCompletePage()
            && IsCurrentMonthViewOverflowing()
            && weekIndex + this.WeekRealizationOffset >= this.CurrentCalendarView.WeekCount - 1)
        {
          --weekIndex;
          break;
        }

        foreach (UIElement dateItemContainer in EnumerateNewItemContainer(weekView.Dates))
        {
          this.InternalDateItemContainerToCalendarIndexTable.Add(dateItemContainer, calendarIndex);
          this.InternalCalendarIndexToWeekNumberTable.Add(calendarIndex, weekView.WeekNumber);
          dateItemContainer.Measure(constraint);
          this.ItemsHost.Children.Add(dateItemContainer);
          calendarIndex++;
        }
      }
    }

    private bool IsRemainingWeeksOfViewCompletePage() => this.CurrentCalendarView.WeekCount - this.WeekRealizationOffset >= this.RowCount;

    private DateGeneratorArgs CreateDateGeneratorArgs(int calendarIndex, UIElement dateItemContainer)
    {
      int weekNumber = this.InternalCalendarIndexToWeekNumberTable[calendarIndex];
      int rowIndex = calendarIndex / this.ColumnCount + this.ContentRowOffset;
      int columnIndex = calendarIndex % this.ColumnCount + this.ContentColumnOffset;
      object item = GetItemFromContainer(dateItemContainer);
      var dateGeneratorArgs = new DateGeneratorArgs(
        dateItemContainer,
        this.ItemsHost,
        item,
        columnIndex,
        rowIndex,
        weekNumber);
      return dateGeneratorArgs;
    }

    private IEnumerable<UIElement> EnumerateNewItemContainer(IEnumerable<DateTime> dates)
    {
      foreach (DateTime calendarViewDate in dates)
      {
        if (!this.InternalDateItemContainerLookupTable.TryGetValue(calendarViewDate, out UIElement dateContainer))
        {
          if (!this.InternalDateItemLookupTable.TryGetValue(calendarViewDate, out CalendarDate dateItem))
          {
            dateItem = new CalendarDate()
            {
              Day = calendarViewDate,
              DayOfWeek = this.Owner.CalendarSource.GetDayOfWeek(calendarViewDate),
              WeekOfYear = this.Owner.CalendarSource.GetWeekOfYear(
              calendarViewDate,
              CalendarWeekRule.FirstDay,
              this.Owner.FirstDayOfWeek)
            };
            this.InternalDateItemLookupTable.Add(calendarViewDate.Date, dateItem);
          }

          if (this.InternalRecycledDateItemContainerLookupTable.Any())
          {
            dateContainer = this.InternalRecycledDateItemContainerLookupTable.Pop();
          }
          else
          {
            dateContainer = this.Owner.GetContainerForDateItem();
          }
          this.Owner.PrepareContainerForCalendarDateItemOverride(dateContainer, dateItem);

          this.InternalDateItemContainerLookupTable.Add(calendarViewDate, dateContainer);
          this.InternalDateItemContainerToDateItemTable.Add(dateContainer, dateItem);
        }
       
        yield return dateContainer;
      }
    }

    private CalendarMonthView CreateCalendarViewOfMonth(DateTime monthStart)
    {
      var calendarView = new CalendarMonthView(monthStart, this.Owner.CalendarSource, this.Owner.FirstDayOfWeek);
      return calendarView;
    }

    private CalendarMonthView CreateCalendarViewOfMonth(int year, int month) =>
      CreateCalendarViewOfMonth(new DateTime(year, month, 1));

    private Size ArrangeDateColumnHeaderItems(Size arrangeBounds)
    {
      if (this.InternalDateColumnHeaderItems.Count < this.ColumnCount)
      {
        return arrangeBounds;
      }

      for (var columnIndex = 0; columnIndex < this.ColumnCount; columnIndex++)
      {
        UIElement itemContainer = this.InternalDateColumnHeaderItems[columnIndex];
        object item = GetItemFromContainer(itemContainer);
        var dateColumnHeaderGeneratorArgs = new DateColumnHeaderGeneratorArgs(
          itemContainer,
          this,
          item,
          columnIndex + this.ContentColumnOffset,
          0);

        OnAutoGeneratingDateColumnHeader(dateColumnHeaderGeneratorArgs);
        OnGeneratingDateColumnHeader(dateColumnHeaderGeneratorArgs);
      }

      if (this.InternalDateColumnHeaderItems.Count == this.ColumnCount + 1)
      {
        UIElement dummyItemContainer = this.InternalDateColumnHeaderItems.Last();
        object item = GetItemFromContainer(dummyItemContainer);
        var dateColumnHeaderGeneratorArgs = new DateColumnHeaderGeneratorArgs(
          dummyItemContainer,
          this,
          item,
          0,
          0);

        OnAutoGeneratingDateColumnHeader(dateColumnHeaderGeneratorArgs);
        OnGeneratingDateColumnHeader(dateColumnHeaderGeneratorArgs);
      }

      return arrangeBounds;
    }

    public object GetItemFromContainer(UIElement itemContainer)
    {
      return itemContainer switch
      {
        HeaderedItemsControl headeredItemsControl => headeredItemsControl.Header,
        ContentControl contentControl => contentControl.Content,
        _ => (itemContainer as FrameworkElement)?.DataContext
      };
    }

    protected virtual void OnGeneratingDate(DateGeneratorArgs dateGeneratorArgs)
    {
      if (dateGeneratorArgs.IsCanceled)
      {
        return;
      }

      Grid.SetRow(dateGeneratorArgs.ItemContainer, dateGeneratorArgs.RowIndex);
      Grid.SetColumn(dateGeneratorArgs.ItemContainer, dateGeneratorArgs.ColumnIndex);

      if (dateGeneratorArgs.ColumnIndex == this.ContentColumnOffset && this.InternalRowIndexToWeekNumberItemTable.TryGetValue(
        dateGeneratorArgs.RowIndex,
        out UIElement weekHeaderItemContainer))
      {
        this.Owner.PrepareContainerForWeekHeaderItemOverride(weekHeaderItemContainer, $"Week {dateGeneratorArgs.WeekNumber}");
        Grid.SetColumn(weekHeaderItemContainer, 0);
        Grid.SetRow(weekHeaderItemContainer, dateGeneratorArgs.RowIndex);
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
      if (dateColumnHeaderGeneratorArgs.IsCanceled)
      {
        return;
      }

      Grid.SetRow(dateColumnHeaderGeneratorArgs.ItemContainer, dateColumnHeaderGeneratorArgs.RowIndex);
      Grid.SetColumn(dateColumnHeaderGeneratorArgs.ItemContainer, dateColumnHeaderGeneratorArgs.ColumnIndex);
    }

    public void ScrollDateIntoView(DateTime date)
    {
      if (this.CurrentCalendarView.ContainsDate(date))
      {
        return;
      }

      CalendarMonthView calendarView = CreateCalendarViewOfMonth(date.Year, date.Month);
      this.CurrentCalendarView = calendarView;
      this.NextCalendarView = calendarView.GetNext();
      this.PreviousScrollDirection = ScrollDirection.Bottom;
      this.WeekRealizationOffset = 0;
      InvalidateMeasure();
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

    public bool TryGetDateItem(DateTime calendarDate, out CalendarDate calendarDateItem) =>
      this.InternalDateItemLookupTable.TryGetValue(calendarDate, out calendarDateItem);

    public IReadOnlyCollection<UIElement> GetDateItemContainers() =>
      new ReadOnlyCollection<UIElement>(this.InternalDateItemContainerLookupTable.Values.ToList());

    public IReadOnlyCollection<UIElement> GetEventItemContainers() =>
      new ReadOnlyCollection<UIElement>(this.InternalDateToEventItemContainersSnapshotTable.Values.SelectMany(eventContainers => eventContainers).ToList());

    public IReadOnlyCollection<UIElement> GetDateColumnHeaderItemContainers() =>
      new ReadOnlyCollection<UIElement>(this.InternalDateColumnHeaderItems);

    private bool IsDateRealized(DateTime date) => this.InternalDateItemContainerLookupTable.ContainsKey(date.Date);

    private int WeekRealizationOffset { get; set; }
    private CalendarMonthView NextCalendarView { get; set; }

#region OnIsShowingCalendarWeekChanged dependency property changed handler

    private static void OnIsShowingCalendarWeekChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
      (d as CalendarPanel).OnIsShowingCalendarWeekChanged((bool) e.OldValue, (bool) e.NewValue);

    protected virtual void OnIsShowingCalendarWeekChanged(bool oldValue, bool newValue)
    {
      this.ContentColumnOffset = newValue ? 1 : 0;
      CreateRootGrid();
      InvalidateVisual();
    }

#endregion OnIsShowingCalendarWeekChanged dependency property changed handler


    protected virtual void OnAutoGeneratingEvent(EventGeneratorArgs e) => this.AutoGeneratingEvent?.Invoke(this, e);
    protected virtual void OnAutoGeneratingDate(DateGeneratorArgs e) => this.AutoGeneratingDate?.Invoke(this, e);

    protected virtual void OnAutoGeneratingDateColumnHeader(DateColumnHeaderGeneratorArgs e) =>
      this.AutoGeneratingDateColumnHeader?.Invoke(this, e);

    public event EventHandler<EventGeneratorArgs> AutoGeneratingEvent;
    public event EventHandler<DateGeneratorArgs> AutoGeneratingDate;
    public event EventHandler<DateColumnHeaderGeneratorArgs> AutoGeneratingDateColumnHeader;


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

    protected Calendar Owner { get; set; }
    protected Grid ItemsHost { get; }

    private Dictionary<DateTime, int> DateColumnItemOffsetTable { get; } = new Dictionary<DateTime, int>();
    private Dictionary<Panel, DateTime> PreviouslySpannedPanelsTable { get; } = new Dictionary<Panel, DateTime>();
    private Dictionary<UIElement, Panel> InternalHostPanels { get; }
    private Dictionary<DateTime, UIElement> InternalDateItemContainerLookupTable { get; }
    private Dictionary<UIElement, int> InternalDateItemContainerToCalendarIndexTable { get; }
    private Dictionary<int, int> InternalCalendarIndexToWeekNumberTable { get; }
    private Dictionary<int, UIElement> InternalRowIndexToWeekNumberItemTable { get; }
    private Dictionary<UIElement, CalendarDate> InternalDateItemContainerToDateItemTable { get; }
    private Dictionary<object, UIElement> InternalEventItemToEventItemContainerSnapshotTable { get; }
    private Dictionary<DateTime, List<UIElement>> InternalDateToEventItemContainersSnapshotTable { get; }
    private List<UIElement> InternalDateColumnHeaderItems { get; }
    private Stack<UIElement> InternalRecycledDateItemContainerLookupTable { get; }
    private Dictionary<DateTime, CalendarDate> InternalDateItemLookupTable { get; }
    private List<UIElement> InternalEventItems { get; }
    private bool IsLayoutDirty { get; set; }

    private CalendarMonthView currentCalendarView;
    private CalendarMonthView CurrentCalendarView
    {
      get => this.currentCalendarView;
      set
      {
        this.currentCalendarView = value;
        RaiseEvent(new CalendarViewChangedRoutedEventArgs<CalendarMonthView>(CalendarPanel.CurrentViewChangedRoutedEvent, this, this.CurrentCalendarView));
      }
    }

    private bool IsCalendarPanelInitialized { get; set; }

    private int ContentRowOffset { get; set; }
    private int ContentColumnOffset { get; set; }

#region Implementation of IScrollInfo

    /// <inheritdoc />
    public void LineUp() => SetVerticalOffset(this.VerticalOffset - 1);

    /// <inheritdoc />
    public void LineDown() => SetVerticalOffset(this.VerticalOffset + 1);

    /// <inheritdoc />
    public void LineLeft()
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc />
    public void LineRight() => throw new NotSupportedException();

    /// <inheritdoc />
    public void PageUp()
    {
      this.VerticalOffset = this.VerticalOffset - this.WeekRealizationOffset;
      LoadPreviousCalendarView();
      this.WeekRealizationOffset = 0;
      this.ScrollOwner.InvalidateScrollInfo();
      InvalidateMeasure();
    }

    /// <inheritdoc />
    public void PageDown()
    {
      this.VerticalOffset = this.VerticalOffset + this.CurrentCalendarView.WeekCount - this.WeekRealizationOffset;
      LoadNextCalendarView();
      this.WeekRealizationOffset = 0;
      this.ScrollOwner.InvalidateScrollInfo();
      InvalidateMeasure();
    }

    /// <inheritdoc />
    public void PageLeft() => throw new NotSupportedException();

    /// <inheritdoc />
    public void PageRight() => throw new NotSupportedException();

    /// <inheritdoc />
    public void MouseWheelUp() => LineUp();

    /// <inheritdoc />
    public void MouseWheelDown() => LineDown();

    /// <inheritdoc />
    public void MouseWheelLeft() => throw new NotSupportedException();

    /// <inheritdoc />
    public void MouseWheelRight() => throw new NotSupportedException();

    /// <inheritdoc />
    public void SetHorizontalOffset(double offset) => throw new NotSupportedException();

    private ScrollDirection PreviousScrollDirection { get; set; }

    /// <inheritdoc />
    public void SetVerticalOffset(double offset)
    {
      if (offset.Equals(this.VerticalOffset) || this.InternalChildren.Count == 0)
      {
        return;
      }

      int offsetDelta = (int)Math.Ceiling(offset - this.VerticalOffset);
      ScrollDirection scrollDirection = offset < this.VerticalOffset && this.WeekRealizationOffset + offsetDelta < 0 ? ScrollDirection.Top : offset > this.VerticalOffset && this.WeekRealizationOffset + offsetDelta > 0 ? ScrollDirection.Bottom : this.PreviousScrollDirection;
      this.WeekRealizationOffset += offsetDelta;
      this.VerticalOffset = offset;
      int availableWeeks = IsCurrentMonthViewOverflowing()
        ? this.CurrentCalendarView.WeekCount - 1
        : this.CurrentCalendarView.WeekCount;

      if (this.PreviousScrollDirection == ScrollDirection.Bottom && scrollDirection == ScrollDirection.Top && this.WeekRealizationOffset < 0 
          || scrollDirection == ScrollDirection.Top && this.WeekRealizationOffset < 0)
      {
        LoadPreviousCalendarView();
      }
      else if (this.PreviousScrollDirection == ScrollDirection.Top && scrollDirection == ScrollDirection.Bottom && this.WeekRealizationOffset >= availableWeeks 
               || scrollDirection == ScrollDirection.Bottom && this.WeekRealizationOffset >= availableWeeks)
      {
        LoadNextCalendarView();
      }

      this.PreviousScrollDirection = scrollDirection;
      //if (scrollDirection == ScrollDirection.Bottom && this.WeekRealizationOffset >= this.CurrentCalendarView.WeekCount)
      //{
      //  this.WeekRealizationOffset = IsCurrentMonthViewOverflowing() ? 1 : 0;
      //  this.CurrentCalendarView = this.CurrentCalendarView.GetNext();
      //  this.NextCalendarView = this.CurrentCalendarView.GetNext();
      //  if (!this.NextCalendarView.MoveNext())
      //  {
      //    return;
      //  }

      //}
      //else if(scrollDirection == ScrollDirection.Top && this.WeekRealizationOffset < 0)
      //{
      //  this.NextCalendarView = this.CurrentCalendarView;
      //  this.CurrentCalendarView = this.NextCalendarView.GetPrevious();
      //  this.WeekRealizationOffset = this.CurrentCalendarView.WeekCount - (IsCurrentMonthViewOverflowing() ? 2: 1);
      //}

      this.IsLayoutDirty = true;
      InvalidateMeasure();
      this.ScrollOwner.InvalidateScrollInfo();
    }

    private void LoadNextCalendarView()
    {
      RecycleUnusedDateContainers(0, -1);
      this.WeekRealizationOffset = 0; 
      this.CurrentCalendarView = this.NextCalendarView;
      this.NextCalendarView = this.NextCalendarView.GetNext();
    }

    private void LoadPreviousCalendarView()
    {
      RecycleUnusedDateContainers(0, -1);
      this.NextCalendarView = this.CurrentCalendarView;
      this.CurrentCalendarView = this.CurrentCalendarView.GetPrevious();
      this.WeekRealizationOffset = this.CurrentCalendarView.WeekCount - (IsCurrentMonthViewOverflowing() ? 2 : 1);
    }

    private bool IsCurrentMonthViewOverflowing() => this.PreviousScrollDirection == ScrollDirection.Bottom
        ? this.CurrentCalendarView.GetLastDate().Month != this.CurrentCalendarView.Index.Month
        : this.CurrentCalendarView.GetLastDate().Month != this.CurrentCalendarView.Index.Month;

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

    private bool canVerticallyScroll;
    private bool canHorizontallyScroll;
    private double extentWidth;
    private double extentHeight;
    private double viewportWidth;
    private double viewportHeight;
    private double horizontalOffset;
    private double verticalOffset;
    private ScrollViewer scrollOwner;

#endregion
  }
}