using System;
using System.Security;
using System.Windows;
using System.Windows.Input;

namespace BionicCode.Utilities.Net.Core.Wpf.AttachedBehaviors
{
  public class PasswordBox : DependencyObject
  {
    #region Command attached property

    public static readonly DependencyProperty CommandProperty =
      DependencyProperty.RegisterAttached(
        "Command",
        typeof(ICommand),
        typeof(PasswordBox),
        new PropertyMetadata(default(bool), PasswordBox.OnSendPasswordCommandChanged));

    public static void SetCommand(DependencyObject attachingElement, ICommand value) =>
      attachingElement.SetValue(PasswordBox.CommandProperty, value);

    public static ICommand GetCommand(DependencyObject attachingElement) =>
      (ICommand)attachingElement.GetValue(PasswordBox.CommandProperty);

    #endregion


    private static void OnSendPasswordCommandChanged(
      DependencyObject attachingElement,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(attachingElement is System.Windows.Controls.PasswordBox passwordBox))
      {
        throw new ArgumentException("Attaching element must be of type 'PasswordBox'");
      }

      if (e.OldValue != null)
      {
        return;
      }

      WeakEventManager<object, RoutedEventArgs>.AddHandler(
        passwordBox,
        nameof(System.Windows.Controls.PasswordBox.PasswordChanged),
        PasswordBox.SendPassword_OnPasswordChanged);
    }

    private static void SendPassword_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
      var attachedElement = sender as System.Windows.Controls.PasswordBox;
      SecureString commandParameter = attachedElement?.SecurePassword;
      if (commandParameter == null || commandParameter.Length < 1)
      {
        return;
      }

      ICommand sendCommand = PasswordBox.GetCommand(attachedElement);
      sendCommand?.Execute(commandParameter);
    }
  }
}
