#region Info
// //  
// BionicCode.BionicNuGetDeploy.Main
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using BionicCode.Utilities.Net.Wpf.Extensions;

namespace BionicCode.Utilities.Net.Wpf.Dialog
{
  /// <summary>
  /// Attached behavior. Displays a <see cref="Window"/> based on an implementation of <see cref="IDialogViewModel"/> assigned to the attached property <see cref="DialogDataContextProperty"/> and a custom <see cref="DataTemplate"/>.
  /// </summary>
  /// <remarks>
  /// This attached behavior will display a dialog <see cref="Window"/> using the View-model-first pattern.
  /// <para>The <see cref="IDialogViewModel"/> instance bound to the <see cref="DialogDataContextProperty"/> will be assigned to the <see cref="ContentControl.Content"/> of the <see cref="Window"/>. To layout the content requires to define an implicit <see cref="DataTemplate"/> that targets the type of the <see cref="IDialogViewModel"/> implementation. Alternatively set the template for the <see cref="ContentControl.ContentTemplate"/> via a <see cref="Style"/> that targets <see cref="Window"/> and is assigned to the <see cref="StyleProperty"/> attached property. For more complex scenarios it is possible to assign a <see cref="DataTemplateSelector"/> to the <see cref="DataTemplateSelectorProperty"/> attached property.</para>
  /// 
  /// <para>
  /// To define the attributes like title and icon of the dialog, set the corresponding values of the <see cref="IDialogViewModel"/> implementation. <see cref="Dialog"/> sets up a data binding to those properties, so that they can be dynamically changed by the view model.
  /// </para>
  /// <para>
  /// It is recommended to use and extend the abstract <see cref="DialogViewModel"/>, which provides the basic <see cref="IDialogViewModel"/> implementation and logic. It only needs to be extended to provide the required specific properties for the dialog's context.
  /// </para>
  /// 
  /// <para>
  /// To show a dialog, simply assign an instance of <see cref="IDialogViewModel"/> to the attached property <see cref="DialogDataContextProperty"/>. A change of that property will automatically display a new dialog window. To each <see cref="IDialogViewModel"/> maps an instance of <see cref="Window"/>. It is recommended to bind the attached <see cref="DialogDataContextProperty"/> to a property of a view model class that implements <see cref="System.ComponentModel.INotifyPropertyChanged"/>. This way it is very simple to display dialogs dynamically initiated by the view model i.e. the binding source of the attached <see cref="DialogDataContextProperty"/> property.
  /// </para>
  /// 
  /// <para>
  /// To close a dialog, raise the <see cref="IDialogViewModel.InteractionCompleted"/> event from the <see cref="IDialogViewModel"/> implementation e.g., by calling <see cref="DialogViewModel.OnInteractionCompleted"/> or by invoking the <see cref="DialogViewModel.SendResponseAsyncCommand"/> (in case you followed the recommendation to extend <see cref="DialogViewModel"/>).
  /// </para>
  /// </remarks>
  /// <seealso href="https://github.com/BionicCode/BionicCode.Net#mvvm-dialog-attached-behavior">See advanced example</seealso>
  public class Dialog : DependencyObject
  {
    #region DialogDataContext attached property

    /// <summary>
    /// Attached property designed to bind to a view model property of type <see cref="IDialogViewModel"/>. A change of this property will trigger the <see cref="Dialog"/> to show a <see cref="Window"/> with the <see cref="FrameworkElement.DataContext"/> set to the <see cref="IDialogViewModel"/> instance of this property.
    /// </summary>
    public static readonly DependencyProperty DialogDataContextProperty = DependencyProperty.RegisterAttached(
      "DialogDataContext", typeof(IDialogViewModel), typeof(Dialog), new PropertyMetadata(default(IDialogViewModel), Dialog.OnDialogDataContextChanged));

    /// <summary>
    /// The setter for the attached <see cref="DialogDataContextProperty"/> property.
    /// </summary>
    /// <param name="attachingElement">The attached <see cref="FrameworkElement"/>.</param>
    /// <param name="value">An instance of <see cref="IDialogViewModel"/>.</param>
    public static void SetDialogDataContext(DependencyObject attachingElement, IDialogViewModel value) => attachingElement.SetValue(Dialog.DialogDataContextProperty, value);

    /// <summary>
    /// The getter for the attached <see cref="DialogDataContextProperty"/> property.
    /// </summary>
    /// <param name="attachingElement">The attached <see cref="FrameworkElement"/>.</param>
    /// <returns>The current associated <see cref="IDialogViewModel"/>.</returns>
    public static IDialogViewModel GetDialogDataContext(DependencyObject attachingElement) => (IDialogViewModel)attachingElement.GetValue(Dialog.DialogDataContextProperty);

    #endregion

    #region DataTemplateSelector attached property

    /// <summary>
    /// Attached property to set a <see cref="DataTemplateSelector"/> to use for the displayed dialog <see cref="Window"/> to display the content.
    /// </summary>
    public static readonly DependencyProperty DataTemplateSelectorProperty = DependencyProperty.RegisterAttached(
      "DataTemplateSelector", typeof(DataTemplateSelector), typeof(Dialog), new PropertyMetadata(default(DataTemplateSelector)));

    /// <summary>
    /// Set method of the attached <see cref="DataTemplateSelectorProperty"/> property.
    /// </summary>
    /// <param name="attachingElement">The attached <see cref="FrameworkElement"/>.</param>
    /// <param name="value">A <see cref="DataTemplateSelector"/> implementation for the window's content.</param>
    public static void SetDataTemplateSelector(DependencyObject attachingElement, DataTemplateSelector value) => attachingElement.SetValue(Dialog.DataTemplateSelectorProperty, value);

    /// <summary>
    /// Returns the <see cref="DataTemplateSelector"/> registered with the <paramref name="attachingElement"/>.
    /// </summary>
    /// <param name="attachingElement">The attached <see cref="FrameworkElement"/>.</param>
    /// <returns>The <see cref="DataTemplateSelector"/> registered with the <paramref name="attachingElement"/>.</returns>
    public static DataTemplateSelector GetDataTemplateSelector(DependencyObject attachingElement) => (DataTemplateSelector)attachingElement.GetValue(Dialog.DataTemplateSelectorProperty);

    #endregion

    #region Style attached property
    /// <summary>
    /// The attached Style property which holds the <see cref="Style"/> which should apply to the dialog. <see cref="Style.TargetType"/> must be <see cref="Window"/>.
    /// </summary>
    public static readonly DependencyProperty StyleProperty = DependencyProperty.RegisterAttached(
      "Style", typeof(Style), typeof(Dialog), new PropertyMetadata(default(Style)));

    /// <summary>
    /// Set method of the attached property <see cref="StyleProperty"/>.
    /// </summary>
    /// <param name="attachingElement">The attached <see cref="FrameworkElement"/>.</param>
    /// <param name="value">The <see cref="Style"/> for the dialog <see cref="Window"/>.</param>
    public static void SetStyle(DependencyObject attachingElement, Style value) => attachingElement.SetValue(Dialog.StyleProperty, value);

    /// <summary>
    /// Get method of the attached property <see cref="StyleProperty"/>.
    /// </summary>
    /// <param name="attachingElement">The attached <see cref="FrameworkElement"/>.</param>
    /// <returns>The dialog's <see cref="Style"/> registered with the <paramref name="attachingElement"/>.</returns>
    public static Style GetStyle(DependencyObject attachingElement) => (Style)attachingElement.GetValue(Dialog.StyleProperty);

    #endregion

    #region IsClosable attached property

    /// <summary>
    /// Attached property that controls if the user is allowed to explicitly close the <see cref="Window"/>.
    /// </summary>
    public static readonly DependencyProperty IsClosableProperty = DependencyProperty.RegisterAttached(
      "IsClosable", typeof(bool), typeof(Dialog), new PropertyMetadata(default(bool)));

    /// <summary>
    /// Set method of the <see cref="IsClosableProperty"/> attached property.
    /// </summary>
    /// <param name="attachingElement">The attached <see cref="FrameworkElement"/>.</param>
    /// <param name="value"><c>true</c> to allow the user to close the <see cref="Window"/>, otherwise <c>false</c>. The default is <c>false</c>.</param>
    public static void SetIsClosable(DependencyObject attachingElement, bool value) => attachingElement.SetValue(Dialog.IsClosableProperty, value);

    /// <summary>
    /// Get method of the attached property <see cref="IsClosableProperty"/>.
    /// </summary>
    /// <param name="attachingElement">The attached <see cref="FrameworkElement"/>.</param>
    /// <returns><c>true</c> to allow the user to close the <see cref="Window"/>, otherwise <c>false</c>.</returns>
    public static bool GetIsClosable(DependencyObject attachingElement) => (bool) attachingElement.GetValue(Dialog.IsClosableProperty);

    #endregion

    #region IsModal attached property

    /// <summary>
    /// Attached property that controls if the <see cref="Window"/> is shown modal.
    /// </summary>
    public static readonly DependencyProperty IsModalProperty = DependencyProperty.RegisterAttached(
      "IsModal", typeof(bool), typeof(Dialog), new PropertyMetadata(default(bool)));

    /// <summary>
    /// Set method of the <see cref="IsModalProperty"/> attached property.
    /// </summary>
    /// <param name="attachingElement">The attached <see cref="FrameworkElement"/>.</param>
    /// <param name="value"><c>true</c> to show the <see cref="Window"/> modal, otherwise <c>false</c>. The default is <c>false</c>.</param>
    public static void SetIsModal(DependencyObject attachingElement, bool value) => attachingElement.SetValue(Dialog.IsModalProperty, value);

    /// <summary>
    /// Get method of the attached property <see cref="IsModalProperty"/>.
    /// </summary>
    /// <param name="attachingElement">The attached <see cref="FrameworkElement"/>.</param>
    /// <returns><c>true</c> to show the <see cref="Window"/> modal, otherwise <c>false</c>.</returns>
    public static bool GetIsModal(DependencyObject attachingElement) => (bool) attachingElement.GetValue(Dialog.IsModalProperty);

    #endregion

    private static Dictionary<IDialogViewModel, Window> ViewModelToDialogMap { get; }
    private static Dictionary<Window, DependencyObject> DialogToAttachingElementMap { get; }
    private static Dictionary<IDialogViewModel, bool> IsClosedByWindowsChromeCloseButton { get; }

    static Dialog()
    {
      Dialog.ViewModelToDialogMap = new Dictionary<IDialogViewModel, Window>();
      Dialog.DialogToAttachingElementMap = new Dictionary<Window, DependencyObject>();
      Dialog.IsClosedByWindowsChromeCloseButton = new Dictionary<IDialogViewModel, bool>();
    }

    /// <summary>
    /// Attempts to get the dialog <see cref="Window"/> of the associated <see cref="IDialogViewModel"/> instance.
    /// </summary>
    /// <param name="viewModel">The associated data context of the dialog.</param>
    /// <param name="dialog">The <see cref="Window"/> instance associated with the <paramref name="viewModel"/>.</param>
    /// <returns><c>true</c> when a <see cref="Window"/> that maps to the <paramref name="viewModel"/> exists, otherwise <c>false</c>.</returns>
    public static bool TryGetDialog(IDialogViewModel viewModel, out Window dialog) => Dialog.ViewModelToDialogMap.TryGetValue(viewModel, out dialog);

    private static void OnDialogDataContextChanged(DependencyObject attachingElement, DependencyPropertyChangedEventArgs e)
    {
      if (e.NewValue is IDialogViewModel newDialogViewModel && attachingElement is FrameworkElement frameworkElement)
      {
        if (frameworkElement.IsLoaded)
        {
          Dialog.Show(attachingElement, newDialogViewModel);
        }
        else
        {
          frameworkElement.Loaded += Dialog.ShowDialogOnAttachingElementLoaded;
        }
      }
    }

    private static void ShowDialogOnAttachingElementLoaded(object sender, RoutedEventArgs e)
    {
      if (sender is Window window
          && window.DataContext is IDialogViewModel dialogViewModel)
      {
        window.ContentTemplate = window.TryFindResource(dialogViewModel.GetType()) as DataTemplate;
      }
    }

    private static void Show(DependencyObject attachingElement, IDialogViewModel newDialogViewModel)
    {
      newDialogViewModel.InteractionCompleted += Dialog.CloseDialogOnInteractionCompleted;
      Window window = Dialog.Prepare(attachingElement, newDialogViewModel);
      window.Closed += Dialog.CleanUpOnDialogClosed;
      window.Closing += Dialog.PreventCloseDialogOnClosing;
      Dialog.ViewModelToDialogMap.Add(newDialogViewModel, window);
      Dialog.DialogToAttachingElementMap.Add(window, attachingElement);
      Dialog.IsClosedByWindowsChromeCloseButton.Add(newDialogViewModel, true);

      if (Dialog.GetIsModal(attachingElement))
      {
        window.ShowDialog();
      }
      else
      {
        window.Show();
      }
    }

    private static Window Prepare(DependencyObject attachingElement, IDialogViewModel newDialogViewModel)
    {
      var window = new Window
      {
        SizeToContent = SizeToContent.WidthAndHeight,
        WindowStartupLocation = WindowStartupLocation.CenterOwner,
        Topmost = true,
        DataContext = newDialogViewModel,
        Content = newDialogViewModel,
        ContentTemplateSelector = Dialog.GetDataTemplateSelector(attachingElement),
        Style = Dialog.GetStyle(attachingElement)
      };

      var titleBinding = new Binding(nameof(IDialogViewModel.Title)) { Source = newDialogViewModel };
      window.SetBinding(Window.TitleProperty, titleBinding);
      var iconBinding = new Binding(nameof(IDialogViewModel.TitleBarIcon)) { Source = newDialogViewModel };
      window.SetBinding(Window.IconProperty, iconBinding);

      if (attachingElement is Window parentWindow
          || attachingElement.TryFindVisualParentElement(out parentWindow))
      {
        window.Owner = parentWindow;
      }
      return window;
    }

    private static void PreventCloseDialogOnClosing(object sender, CancelEventArgs e)
    {
      var dialog = sender as Window;
      if (Dialog.DialogToAttachingElementMap.TryGetValue(dialog, out DependencyObject attachedElement) 
          && !Dialog.GetIsClosable(attachedElement))
      {
        e.Cancel = true;
      }
    }

    private static async void CleanUpOnDialogClosed(object sender, EventArgs e)
    {
      var dialog = sender as Window;
      var dialogViewModel = dialog?.DataContext as IDialogViewModel;
      Dialog.ViewModelToDialogMap.Remove(dialogViewModel);
      Dialog.DialogToAttachingElementMap.Remove(dialog);
      dialog.Closed -= Dialog.CleanUpOnDialogClosed;
      dialog.Closing -= Dialog.PreventCloseDialogOnClosing;
      dialogViewModel.InteractionCompleted -= Dialog.CloseDialogOnInteractionCompleted;

      if (Dialog.IsClosedByWindowsChromeCloseButton.TryGetValue(dialogViewModel, out bool isClosedByWindowChromeCloseButton) && isClosedByWindowChromeCloseButton)
      {
        await dialogViewModel.SendResponseAsyncCommand.ExecuteAsync(DialogResult.Aborted);
      }

      Dialog.IsClosedByWindowsChromeCloseButton.Remove(dialogViewModel);
    }

    private static void CloseDialogOnInteractionCompleted(object sender, EventArgs e)
    {
      var dialogViewModel = sender as IDialogViewModel;
      if (Dialog.ViewModelToDialogMap.TryGetValue(dialogViewModel, out Window dialog))
      {
        dialog.Closing -= Dialog.PreventCloseDialogOnClosing;
        dialog.Close();
      }
    }
  }
}