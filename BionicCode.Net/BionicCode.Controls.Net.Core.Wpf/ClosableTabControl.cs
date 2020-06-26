using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BionicCode.Controls.Net.Core.Wpf
{
  /// <summary>
  /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
  ///
  /// Step 1a) Using this custom control in a XAML file that exists in the current project.
  /// Add this XmlNamespace attribute to the root element of the markup file where it is 
  /// to be used:
  ///
  ///     xmlns:MyNamespace="clr-namespace:WpfTestRange.Main"
  ///
  ///
  /// Step 1b) Using this custom control in a XAML file that exists in a different project.
  /// Add this XmlNamespace attribute to the root element of the markup file where it is 
  /// to be used:
  ///
  ///     xmlns:MyNamespace="clr-namespace:WpfTestRange.Main;assembly=WpfTestRange.Main"
  ///
  /// You will also need to add a project reference from the project where the XAML file lives
  /// to this project and Rebuild to avoid compilation errors:
  ///
  ///     Right click on the target project in the Solution Explorer and
  ///     "Add Reference"->"Projects"->[Browse to and select this project]
  ///
  ///
  /// Step 2)
  /// Go ahead and use your control in the XAML file.
  ///
  ///     <MyNamespace:ClosableTabControl/>
  ///
  /// </summary>
    public class ClosableTabControl : TabControl
    {
      public static readonly RoutedUICommand CloseTabRoutedCommand = new RoutedUICommand(
        "Close TabItem and remove item from ItemsSource",
        nameof(ClosableTabControl.CloseTabRoutedCommand),
        typeof(ClosableTabControl));

      static ClosableTabControl()
      {
        FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ClosableTabControl), new FrameworkPropertyMetadata(typeof(ClosableTabControl)));
      }

      public ClosableTabControl()
      {
        this.CommandBindings.Add(
          new CommandBinding(ClosableTabControl.CloseTabRoutedCommand, ExecuteRemoveTab, CanExecuteRemoveTab));
      }

      private void CanExecuteRemoveTab(object sender, CanExecuteRoutedEventArgs e)
      {
        e.CanExecute = e.OriginalSource is FrameworkElement frameworkElement
                       && this.Items.Contains(frameworkElement.DataContext)
                       || this.Items.Contains(e.Source);
      }

      private void ExecuteRemoveTab(object sender, ExecutedRoutedEventArgs e)
      {
      object tabItemToRemove = this.ItemsSource == null
        ? e.Source
        : (e.OriginalSource as FrameworkElement).DataContext;
      int lastItemIndex = this.Items.Count - 1;
      int nextItemIndex = this.Items.IndexOf(tabItemToRemove) + 1;
      this.SelectedIndex = Math.Min(lastItemIndex, nextItemIndex);
      (this.ItemContainerGenerator.ContainerFromItem(tabItemToRemove) as UIElement).Visibility = Visibility.Collapsed;
    }
    }
}
