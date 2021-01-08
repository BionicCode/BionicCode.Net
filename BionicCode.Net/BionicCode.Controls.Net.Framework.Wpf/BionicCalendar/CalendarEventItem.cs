#region Info

// 2020/11/06  16:08
// Activitytracker

#endregion

#region Usings

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;

#endregion

namespace BionicCode.Controls.Net.Framework.Wpf.BionicCalendar
{
  internal delegate void SpanningRequestedRoutedEventHandler(object sender, EventItemDragDropArgs e);

  public delegate void EventItemDroppedRoutedEventHandler(object sender, EventItemDragDropArgs e);

  public class CalendarEventItem : ContentControl, ICommandSource
  {
    #region EventItemDroppedRoutedEvent

    public static readonly RoutedEvent EventItemDroppedRoutedEvent = EventManager.RegisterRoutedEvent("EventItemDropped",
      RoutingStrategy.Bubble, typeof(EventItemDroppedRoutedEventHandler), typeof(CalendarEventItem));

    public event EventItemDroppedRoutedEventHandler EventItemDropped
    {
      add => AddHandler(CalendarEventItem.EventItemDroppedRoutedEvent, value);
      remove => RemoveHandler(CalendarEventItem.EventItemDroppedRoutedEvent, value);
    }

    #endregion

    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
      "IsSelected",
      typeof(bool),
      typeof(CalendarEventItem),
      new PropertyMetadata(default(bool)));

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
      "Command",
      typeof(ICommand),
      typeof(CalendarEventItem),
      new PropertyMetadata(default(ICommand), CalendarEventItem.OnCommandChanged));

    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
      "CommandParameter",
      typeof(object),
      typeof(CalendarEventItem),
      new PropertyMetadata(default(object)));

    public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(
      "CommandTarget",
      typeof(IInputElement),
      typeof(CalendarEventItem),
      new PropertyMetadata(default(IInputElement)));

    #region IsSpanningSource dependency property

    public static readonly DependencyProperty IsSpanningSourceProperty = DependencyProperty.Register(
      "IsSpanningSource",
      typeof(bool),
      typeof(CalendarEventItem),
      new PropertyMetadata(default));

    public bool IsSpanningSource
    {
      get => (bool) GetValue(CalendarEventItem.IsSpanningSourceProperty);
      set => SetValue(CalendarEventItem.IsSpanningSourceProperty, value);
    }

    #endregion IsSpanningSource dependency property

    #region IsSpanningTarget dependency property

    public static readonly DependencyProperty IsSpanningTargetProperty = DependencyProperty.Register(
      "IsSpanningTarget",
      typeof(bool),
      typeof(CalendarEventItem),
      new PropertyMetadata(default));

    public bool IsSpanningTarget
    {
      get => (bool) GetValue(CalendarEventItem.IsSpanningTargetProperty);
      set => SetValue(CalendarEventItem.IsSpanningTargetProperty, value);
    }

    #endregion IsSpanningTarget dependency property

    #region Dependency properties

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var this_ = d as CalendarEventItem;
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

    static CalendarEventItem()
    {
      FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
        typeof(CalendarEventItem),
        new FrameworkPropertyMetadata(typeof(CalendarEventItem)));
      Selector.IsSelectedProperty.OverrideMetadata(
        typeof(CalendarEventItem),
        new FrameworkPropertyMetadata(default(bool), CalendarEventItem.OnIsSelectedChanged));
    }

    public CalendarEventItem()
    {
      this.Loaded += OnLoaded;
    }

    public CalendarEventItem(CalendarEventItem source) : this()
    {
      this.IsSpanningSource = source.IsSpanningSource;
      this.IsSpanningTarget = source.IsSpanningTarget;
      this.Command = source.Command;
      this.CommandParameter = source.CommandParameter;
      this.CommandTarget = source.CommandTarget;
      this.IsSelected = source.IsSelected;
      this.Content = source.Content;
      //Calendar.SetDay(this, Calendar.GetDay(source));
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      var itemContainerAdornerLayer = AdornerLayer.GetAdornerLayer(this);
      var resizeAdorner = new EventResizeAdorner(this);
      itemContainerAdornerLayer.Add(resizeAdorner);
      resizeAdorner.Resizing += HandleAdornerResizing;
    }

    #endregion


    #region

    /// <inheritdoc />
    public ICommand Command
    {
      get => (ICommand) GetValue(CalendarEventItem.CommandProperty);
      set => SetValue(CalendarEventItem.CommandProperty, value);
    }

    /// <inheritdoc />
    public object CommandParameter
    {
      get => GetValue(CalendarEventItem.CommandParameterProperty);
      set => SetValue(CalendarEventItem.CommandParameterProperty, value);
    }

    /// <inheritdoc />
    public IInputElement CommandTarget
    {
      get => (IInputElement) GetValue(CalendarEventItem.CommandTargetProperty);
      set => SetValue(CalendarEventItem.CommandTargetProperty, value);
    }

    #endregion

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var this_ = d as CalendarEventItem;
      this_.OnIsSelectedChanged((bool) e.OldValue, (bool) e.NewValue);
    }

    public CalendarEventItem Clone() => MemberwiseClone() as CalendarEventItem;


    /// <inheritdoc />
    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
      base.OnPreviewMouseLeftButtonDown(e);
      HandleSelection();
      ExecuteClickCommand();
      
      StartDragDropOperation();
    }

    private void StartSpanOperation()
    {
      DragDrop.DoDragDrop(
        this,
        new DataObject(
          DataFormats.Serializable,
          new EventItemDragDropArgs(Calendar.GetDay(this), this, DragDropEffects.Link)),
        DragDropEffects.Link);
    }

    private void StartDragDropOperation()
    {
      DragDrop.DoDragDrop(
        this,
        new DataObject(
          DataFormats.Serializable,
          new EventItemDragDropArgs(Calendar.GetDay(this), this, DragDropEffects.Move)),
        DragDropEffects.Move);
    }

    private void HandleSelection()
    {
      Selector.SetIsSelected(this, true);
    }

    private void HandleAdornerResizing(object sender, EventArgs e)
    {
      this.IsSpanningSource = true;
      StartSpanOperation();
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

    public bool IsSelected
    {
      get => (bool) GetValue(CalendarEventItem.IsSelectedProperty);
      set => SetValue(CalendarEventItem.IsSelectedProperty, value);
    }
  }
}