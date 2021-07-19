#region Info

// 2020/11/07  17:41
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
    public class CalendarDateColumnHeaderItem : Control, ICommandSource
    {
        public static readonly RoutedEvent SelectedRoutedEvent = EventManager.RegisterRoutedEvent(
            "Selected",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(CalendarDateColumnHeaderItem));

        public static readonly RoutedEvent PreviewSelectedRoutedEvent = EventManager.RegisterRoutedEvent(
            "PreviewSelected",
            RoutingStrategy.Tunnel,
            typeof(RoutedEventHandler),
            typeof(CalendarDateColumnHeaderItem));

        public static readonly RoutedEvent UnselectedRoutedEvent = EventManager.RegisterRoutedEvent(
            "Unselected",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(CalendarDateColumnHeaderItem));

        public static readonly RoutedEvent PreviewUnselectedRoutedEvent = EventManager.RegisterRoutedEvent(
            "PreviewUnselected",
            RoutingStrategy.Tunnel,
            typeof(RoutedEventHandler),
            typeof(CalendarDateColumnHeaderItem));

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected",
            typeof(bool),
            typeof(CalendarDateColumnHeaderItem),
            new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command",
            typeof(ICommand),
            typeof(CalendarDateColumnHeaderItem),
            new PropertyMetadata(default(ICommand), CalendarDateColumnHeaderItem.OnCommandChanged));

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            "CommandParameter",
            typeof(object),
            typeof(CalendarDateColumnHeaderItem),
            new PropertyMetadata(default(object)));

        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(
            "CommandTarget",
            typeof(IInputElement),
            typeof(CalendarDateColumnHeaderItem),
            new PropertyMetadata(default(IInputElement)));

        #region Dependency properties

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var this_ = d as CalendarDateColumnHeaderItem;
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

        static CalendarDateColumnHeaderItem()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CalendarDateColumnHeaderItem),
                new FrameworkPropertyMetadata(typeof(CalendarDateColumnHeaderItem)));
            Selector.IsSelectedProperty.OverrideMetadata(
                typeof(CalendarDateColumnHeaderItem),
                new FrameworkPropertyMetadata(default(bool), CalendarDateColumnHeaderItem.OnIsSelectedChanged));
        }

        #endregion

        #region

        /// <inheritdoc />
        public ICommand Command
        {
            get => (ICommand) GetValue(CalendarDateColumnHeaderItem.CommandProperty);
            set => SetValue(CalendarDateColumnHeaderItem.CommandProperty, value);
        }

        /// <inheritdoc />
        public object CommandParameter
        {
            get => GetValue(CalendarDateColumnHeaderItem.CommandParameterProperty);
            set => SetValue(CalendarDateColumnHeaderItem.CommandParameterProperty, value);
        }

        /// <inheritdoc />
        public IInputElement CommandTarget
        {
            get => (IInputElement) GetValue(CalendarDateColumnHeaderItem.CommandTargetProperty);
            set => SetValue(CalendarDateColumnHeaderItem.CommandTargetProperty, value);
        }

        #endregion

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var this_ = d as CalendarDateColumnHeaderItem;
            this_.OnIsSelectedChanged((bool) e.OldValue, (bool) e.NewValue);
        }

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
                RaiseEvent(new RoutedEventArgs(CalendarDateColumnHeaderItem.PreviewSelectedRoutedEvent, this));
                RaiseEvent(new RoutedEventArgs(CalendarDateColumnHeaderItem.SelectedRoutedEvent, this));
                RaiseEvent(new RoutedEventArgs(Selector.SelectedEvent, this));
            }
            else
            {
                RaiseEvent(new RoutedEventArgs(CalendarDateColumnHeaderItem.PreviewUnselectedRoutedEvent, this));
                RaiseEvent(new RoutedEventArgs(CalendarDateColumnHeaderItem.UnselectedRoutedEvent, this));
                RaiseEvent(new RoutedEventArgs(Selector.UnselectedEvent, this));
            }
        }

        public event RoutedEventHandler Selected
        {
            add => AddHandler(CalendarDateColumnHeaderItem.SelectedRoutedEvent, value);
            remove => RemoveHandler(CalendarDateColumnHeaderItem.SelectedRoutedEvent, value);
        }

        public event RoutedEventHandler PreviewSelected
        {
            add => AddHandler(CalendarDateColumnHeaderItem.PreviewSelectedRoutedEvent, value);
            remove => RemoveHandler(CalendarDateColumnHeaderItem.PreviewSelectedRoutedEvent, value);
        }

        public event RoutedEventHandler Unselected
        {
            add => AddHandler(CalendarDateColumnHeaderItem.UnselectedRoutedEvent, value);
            remove => RemoveHandler(CalendarDateColumnHeaderItem.UnselectedRoutedEvent, value);
        }

        public event RoutedEventHandler PreviewUnselected
        {
            add => AddHandler(CalendarDateColumnHeaderItem.PreviewUnselectedRoutedEvent, value);
            remove => RemoveHandler(CalendarDateColumnHeaderItem.PreviewUnselectedRoutedEvent, value);
        }

        public bool IsSelected
        {
            get => (bool) GetValue(CalendarDateColumnHeaderItem.IsSelectedProperty);
            set => SetValue(CalendarDateColumnHeaderItem.IsSelectedProperty, value);
        }
    }
}