#region Info

// 2020/11/12  18:31
// Activitytracker

#endregion

#region Usings

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

#endregion

namespace BionicCode.Controls.Net.Wpf
{
  public class CalendarRowItem : ContentControl, ICommandSource
  {
    public static readonly RoutedEvent SelectedRoutedEvent = EventManager.RegisterRoutedEvent(
        "Selected",
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(CalendarRowItem));

    public static readonly RoutedEvent PreviewSelectedRoutedEvent = EventManager.RegisterRoutedEvent(
        "PreviewSelected",
        RoutingStrategy.Tunnel,
        typeof(RoutedEventHandler),
        typeof(CalendarRowItem));

    public static readonly RoutedEvent UnselectedRoutedEvent = EventManager.RegisterRoutedEvent(
        "Unselected",
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(CalendarRowItem));

    public static readonly RoutedEvent PreviewUnselectedRoutedEvent = EventManager.RegisterRoutedEvent(
        "PreviewUnselected",
        RoutingStrategy.Tunnel,
        typeof(RoutedEventHandler),
        typeof(CalendarRowItem));

    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
        "IsSelected",
        typeof(bool),
        typeof(CalendarRowItem),
        new PropertyMetadata(default(bool)));

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        "Command",
        typeof(ICommand),
        typeof(CalendarRowItem),
        new PropertyMetadata(default(ICommand), CalendarRowItem.OnCommandChanged));

    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
        "CommandParameter",
        typeof(object),
        typeof(CalendarRowItem),
        new PropertyMetadata(default(object)));

    public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(
        "CommandTarget",
        typeof(IInputElement),
        typeof(CalendarRowItem),
        new PropertyMetadata(default(IInputElement)));

    public static readonly DependencyProperty DayOfWeekProperty = DependencyProperty.Register(
        "DayOfWeek",
        typeof(DayOfWeek),
        typeof(CalendarRowItem),
        new PropertyMetadata(default(DayOfWeek)));

    public static readonly DependencyProperty AnnotationProperty = DependencyProperty.Register(
        "Annotation",
        typeof(object),
        typeof(CalendarRowItem),
        new PropertyMetadata(default(object)));

    #region

    static CalendarRowItem()
    {
      FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
          typeof(CalendarRowItem),
          new FrameworkPropertyMetadata(typeof(CalendarRowItem)));
      Selector.IsSelectedProperty.OverrideMetadata(
          typeof(CalendarRowItem),
          new FrameworkPropertyMetadata(default(bool), CalendarRowItem.OnIsSelectedChanged));
    }

    #endregion

    #region Overrides of FrameworkElement

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      this.ItemsHost = GetTemplateChild("PART_RowItemsHost") as Grid;
      this.SelectionBorder = GetTemplateChild("PART_SelectionBorder") as UIElement;
    }

    #endregion

    public Grid ItemsHost { get; private set; }
    public UIElement SelectionBorder { get; private set; }

    #region

    /// <inheritdoc />
    public ICommand Command
    {
      get => (ICommand)GetValue(CalendarRowItem.CommandProperty);
      set => SetValue(CalendarRowItem.CommandProperty, value);
    }

    /// <inheritdoc />
    public object CommandParameter
    {
      get => GetValue(CalendarRowItem.CommandParameterProperty);
      set => SetValue(CalendarRowItem.CommandParameterProperty, value);
    }

    /// <inheritdoc />
    public IInputElement CommandTarget
    {
      get => (IInputElement)GetValue(CalendarRowItem.CommandTargetProperty);
      set => SetValue(CalendarRowItem.CommandTargetProperty, value);
    }

    #endregion

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var this_ = d as CalendarRowItem;
      if (e.OldValue is ICommand oldCommand)
      {
        oldCommand.CanExecuteChanged -= this_.OnCanExecuteChanged;
      }

      if (e.NewValue is ICommand newCommand)
      {
        newCommand.CanExecuteChanged += this_.OnCanExecuteChanged;
      }
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var this_ = d as CalendarRowItem;
      this_.OnIsSelectedChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    /// <inheritdoc />
    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
      base.OnPreviewMouseLeftButtonDown(e);
      HandleSelection();
      ExecuteClickCommand();
    }

    private void HandleSelection() => Selector.SetIsSelected(this, true);

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
        RaiseEvent(new RoutedEventArgs(CalendarRowItem.PreviewSelectedRoutedEvent, this));
        RaiseEvent(new RoutedEventArgs(CalendarRowItem.SelectedRoutedEvent, this));
        RaiseEvent(new RoutedEventArgs(Selector.SelectedEvent, this));
      }
      else
      {
        RaiseEvent(new RoutedEventArgs(CalendarRowItem.PreviewUnselectedRoutedEvent, this));
        RaiseEvent(new RoutedEventArgs(CalendarRowItem.UnselectedRoutedEvent, this));
        RaiseEvent(new RoutedEventArgs(Selector.UnselectedEvent, this));
      }
    }

    ///// <inheritdoc />
    //protected override DependencyObject GetContainerForItemOverride() => new CalendarEventItem();

    ///// <inheritdoc />
    //protected override bool IsItemItsOwnContainerOverride(object item) => item is CalendarEventItem;

    public event RoutedEventHandler Selected
    {
      add => AddHandler(CalendarRowItem.SelectedRoutedEvent, value);
      remove => RemoveHandler(CalendarRowItem.SelectedRoutedEvent, value);
    }

    public event RoutedEventHandler PreviewSelected
    {
      add => AddHandler(CalendarRowItem.PreviewSelectedRoutedEvent, value);
      remove => RemoveHandler(CalendarRowItem.PreviewSelectedRoutedEvent, value);
    }

    public event RoutedEventHandler Unselected
    {
      add => AddHandler(CalendarRowItem.UnselectedRoutedEvent, value);
      remove => RemoveHandler(CalendarRowItem.UnselectedRoutedEvent, value);
    }

    public event RoutedEventHandler PreviewUnselected
    {
      add => AddHandler(CalendarRowItem.PreviewUnselectedRoutedEvent, value);
      remove => RemoveHandler(CalendarRowItem.PreviewUnselectedRoutedEvent, value);
    }

    private ICommand _command;
    private object _commandParameter;
    private IInputElement _commandTarget;

    public DayOfWeek DayOfWeek
    {
      get => (DayOfWeek)GetValue(CalendarRowItem.DayOfWeekProperty);
      set => SetValue(CalendarRowItem.DayOfWeekProperty, value);
    }

    public object Annotation
    {
      get => GetValue(CalendarRowItem.AnnotationProperty);
      set => SetValue(CalendarRowItem.AnnotationProperty, value);
    }

    public bool IsSelected
    {
      get => (bool)GetValue(CalendarRowItem.IsSelectedProperty);
      set => SetValue(CalendarRowItem.IsSelectedProperty, value);
    }

    ///// <inheritdoc />
    //public ICommand Command { get => this._command; set => this._command = value; }

    ///// <inheritdoc />
    //public object CommandParameter { get => this._commandParameter; set => this._commandParameter = value; }

    ///// <inheritdoc />
    //public IInputElement CommandTarget { get => this._commandTarget; set => this._commandTarget = value; }
  }
}