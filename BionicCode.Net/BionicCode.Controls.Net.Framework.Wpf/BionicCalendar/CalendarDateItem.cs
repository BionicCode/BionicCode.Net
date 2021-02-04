#region Info

// 2020/11/07  17:32
// Activitytracker

#endregion

#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

#endregion

namespace BionicCode.Controls.Net.Framework.Wpf.BionicCalendar
{
  public class CalendarDateItem : HeaderedItemsControl, ICommandSource
  {
    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
      "IsSelected",
      typeof(bool),
      typeof(CalendarDateItem),
      new PropertyMetadata(default(bool)));

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
      "Command",
      typeof(ICommand),
      typeof(CalendarDateItem),
      new PropertyMetadata(default(ICommand), CalendarDateItem.OnCommandChanged));

    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
      "CommandParameter",
      typeof(object),
      typeof(CalendarDateItem),
      new PropertyMetadata(default(object)));

    public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(
      "CommandTarget",
      typeof(IInputElement),
      typeof(CalendarDateItem),
      new PropertyMetadata(default(IInputElement)));

    public static readonly DependencyProperty DayOfWeekProperty = DependencyProperty.Register(
      "DayOfWeek",
      typeof(DayOfWeek),
      typeof(CalendarDateItem),
      new PropertyMetadata(default(DayOfWeek)));

    public static readonly DependencyProperty AnnotationProperty = DependencyProperty.Register(
      "Annotation",
      typeof(object),
      typeof(CalendarDateItem),
      new PropertyMetadata(default(object)));

    public static readonly DependencyProperty DayProperty = DependencyProperty.Register(
      "Day",
      typeof(DateTime),
      typeof(CalendarDateItem),
      new PropertyMetadata(default(DateTime)));

    public static readonly DependencyProperty IsHolidayProperty = DependencyProperty.Register(
      "IsHoliday",
      typeof(bool),
      typeof(CalendarDateItem),
      new PropertyMetadata(default(bool)));

    public static readonly DependencyProperty WeekOfYearProperty = DependencyProperty.Register(
      "WeekOfYear",
      typeof(int),
      typeof(CalendarDateItem),
      new PropertyMetadata(default(int)));

    public static readonly DependencyProperty IsTodayProperty = DependencyProperty.Register(
      "IsToday",
      typeof(bool),
      typeof(CalendarDateItem),
      new PropertyMetadata(default(bool)));

    public bool IsToday
    {
      get => (bool) GetValue(CalendarDateItem.IsTodayProperty);
      set => SetValue(CalendarDateItem.IsTodayProperty, value);
    }

    #region Dependency properties

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var this_ = d as CalendarDateItem;
      if (e.OldValue is ICommand oldCommand)
      {
        oldCommand.CanExecuteChanged -= this_.OnCanExecuteChanged;
      }

      if (e.NewValue is ICommand newCommand)
      {
        newCommand.CanExecuteChanged += this_.OnCanExecuteChanged;
      }
    }

    #endregion

    #region

    static CalendarDateItem()
    {
      FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
        typeof(CalendarDateItem),
        new FrameworkPropertyMetadata(typeof(CalendarDateItem)));
      Selector.IsSelectedProperty.OverrideMetadata(
        typeof(CalendarDateItem),
        new FrameworkPropertyMetadata(default(bool), CalendarDateItem.OnIsSelectedChanged));
    }

    public CalendarDateItem()
    {
      this.AllowDrop = true;
      AddHandler(Calendar.PreviewSelectedRoutedEvent, new RoutedEventHandler(OnEventItemSelected));
    }

    #endregion

    private void OnEventItemSelected(object sender, RoutedEventArgs e)
    {
      ;
    }
    #region

    /// <inheritdoc />
    public ICommand Command
    {
      get => (ICommand) GetValue(CalendarDateItem.CommandProperty);
      set => SetValue(CalendarDateItem.CommandProperty, value);
    }

    /// <inheritdoc />
    public object CommandParameter
    {
      get => GetValue(CalendarDateItem.CommandParameterProperty);
      set => SetValue(CalendarDateItem.CommandParameterProperty, value);
    }

    /// <inheritdoc />
    public IInputElement CommandTarget
    {
      get => (IInputElement) GetValue(CalendarDateItem.CommandTargetProperty);
      set => SetValue(CalendarDateItem.CommandTargetProperty, value);
    }

    #endregion

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var this_ = d as CalendarDateItem;
      this_.OnIsSelectedChanged((bool) e.OldValue, (bool) e.NewValue);
    }
    #region Overrides of UIElement

    private bool IsAcceptingDrag { get; set; }
    /// <inheritdoc />
    protected override void OnDragEnter(DragEventArgs e)
    {
      base.OnDragEnter(e);
      if (e.AllowedEffects.HasFlag(DragDropEffects.Move) || !this.IsAcceptingDrag)
      {
        return;
      }

      this.IsAcceptingDrag = false;
      var eventItemDragDropArgs = e.Data.GetData(DataFormats.Serializable) as EventItemDragDropArgs;

      if (eventItemDragDropArgs == null)
      {
        return;
      }
      RaiseEvent(new EventItemDragDropArgs(eventItemDragDropArgs, Calendar.SpanningRequestedRoutedEvent, this));
    }

    /// <inheritdoc />
    protected override void OnDragLeave(DragEventArgs e)
    {
      base.OnDragLeave(e);

      this.IsAcceptingDrag = true;
    }

    /// <inheritdoc />
    protected override void OnDrop(DragEventArgs e)
    {
      base.OnDrop(e);
      var eventItemDragDropArgs = e.Data.GetData(DataFormats.Serializable) as EventItemDragDropArgs;

      if (eventItemDragDropArgs == null || this.Items.Contains(eventItemDragDropArgs))
      {
        return;
      }

      if (e.AllowedEffects.HasFlag(DragDropEffects.Move))
      {
        Calendar.SetDay(eventItemDragDropArgs.ItemContainer, Calendar.GetDay(this));
        RaiseEvent(eventItemDragDropArgs);
      }
      if (e.AllowedEffects.HasFlag(DragDropEffects.Link))
      {
        //var newItem = new CalendarEventItem(eventItemDragDropArgs as CalendarEventItem){Tag = "qwertz"};
        //newItem.IsSpanningTarget = true;
        //this.Items.Insert(0, newItem);
        //DateTime tt = Calendar.GetDay(this);
        //if (Calendar.DateToDateItemContainerMap.TryGetValue(
        //  tt.Date,
        //  out UIElement calendarDateItem))
        //{
        //  var eq = ReferenceEquals(this, calendarDateItem);
        //  var eq1 = ReferenceEquals(this, newItem.Parent);
        //  var eq2 = ReferenceEquals(newItem.Parent, calendarDateItem);
        //}
        //Calendar.SetDay(newItem, tt);
      }
    }

    #endregion

    /// <inheritdoc />
    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
      base.OnPreviewMouseLeftButtonDown(e);
      HandleSelection();
      ExecuteClickCommand();
    }

    private void HandleSelection()
    {
      Selector.SetIsSelected(this, true);
    }

    private void ExecuteClickCommand()
    {
      if (this.Command?.CanExecute(this.CommandParameter) ?? false)
      {
        this.Command.Execute(this.CommandParameter);
      }
    }

    protected virtual void OnCanExecuteChanged(object sender, EventArgs e)
    {
    }

    protected virtual void OnIsSelectedChanged(bool oldValue, bool newValue)
    {
      this.IsSelected = newValue;

      if (newValue)
      {
        RaiseEvent(new RoutedEventArgs(Calendar.PreviewSelectedRoutedEvent, this));
        RaiseEvent(new RoutedEventArgs(Calendar.SelectedRoutedEvent, this));
        RaiseEvent(new RoutedEventArgs(Selector.SelectedEvent, this));
      }
      else
      {
        RaiseEvent(new RoutedEventArgs(Calendar.PreviewUnselectedRoutedEvent, this));
        RaiseEvent(new RoutedEventArgs(Calendar.UnselectedRoutedEvent, this));
        RaiseEvent(new RoutedEventArgs(Selector.UnselectedEvent, this));
      }
    }

    ///// <inheritdoc />
    //protected override DependencyObject GetContainerForItemOverride() => new CalendarEventItem();

    ///// <inheritdoc />
    //protected override bool IsItemItsOwnContainerOverride(object item) => item is CalendarEventItem;

    private ICommand _command;
    private object _commandParameter;
    private IInputElement _commandTarget;

    public DayOfWeek DayOfWeek
    {
      get => (DayOfWeek) GetValue(CalendarDateItem.DayOfWeekProperty);
      set => SetValue(CalendarDateItem.DayOfWeekProperty, value);
    }

    public object Annotation
    {
      get => GetValue(CalendarDateItem.AnnotationProperty);
      set => SetValue(CalendarDateItem.AnnotationProperty, value);
    }

    public DateTime Day
    {
      get => (DateTime) GetValue(CalendarDateItem.DayProperty);
      set => SetValue(CalendarDateItem.DayProperty, value);
    }

    public bool IsHoliday
    {
      get => (bool) GetValue(CalendarDateItem.IsHolidayProperty);
      set => SetValue(CalendarDateItem.IsHolidayProperty, value);
    }

    public int WeekOfYear
    {
      get => (int) GetValue(CalendarDateItem.WeekOfYearProperty);
      set => SetValue(CalendarDateItem.WeekOfYearProperty, value);
    }

    public bool IsSelected
    {
      get => (bool) GetValue(CalendarDateItem.IsSelectedProperty);
      set => SetValue(CalendarDateItem.IsSelectedProperty, value);
    }
  }
}