namespace BionicCode.Utilities.Net
{
  using System;
  using System.Security;
  using System.Windows;
  using System.Windows.Input;
  /// <summary>
  /// Attached behavior for <see cref="System.Windows.Controls.PasswordBox"/> that will invoke an <see cref="ICommand"/> to send the <see cref="SecureString"/> of the <see cref="System.Windows.Controls.PasswordBox.SecurePassword"/> property to a command target e.g., view model using a registered <see cref="ICommand"/> registered with the <see cref="CommandProperty"/> attached property.
  /// </summary>
  /// <remarks>The attached behavior does at no point unwrap the <see cref="SecureString"/> returned from the <see cref="System.Windows.Controls.PasswordBox.SecurePassword"/> property, nor does it access the security critical <see cref="System.Windows.Controls.PasswordBox.Password"/> property. The <see cref="System.Windows.Controls.PasswordBox.SecurePassword"/> value is simply forwarded to the command target of the registered <see cref="CommandProperty"/> attached property.</remarks>
  /// <seealso href="https://github.com/BionicCode/BionicCode.Net#passwordbox">See advanced example</seealso>
  public class PasswordBoxService : DependencyObject
  {
    #region Command attached property

    /// <summary>
    /// Holds the <see cref="ICommand"/> which will be invoked with the <see cref="System.Windows.Controls.PasswordBox.SecurePassword"/> of type <see cref="SecureString"/> as command parameter. 
    /// </summary>
    /// <value>An <see cref="ICommand"/> implementation.</value>
    public static readonly DependencyProperty CommandProperty =
      DependencyProperty.RegisterAttached(
        "Command",
        typeof(ICommand),
        typeof(PasswordBoxService),
        new PropertyMetadata(default(bool), PasswordBoxService.OnSendPasswordCommandChanged));

    /// <summary>
    /// The set method of the attached <see cref="System.Windows.Controls.PasswordBox"/> property.
    /// </summary>
    /// <param name="attachingElement">The <see cref="System.Windows.Controls.PasswordBox"/> element.</param>
    /// <param name="value">An <see cref="ICommand"/> implementation.</param>
    public static void SetCommand(DependencyObject attachingElement, ICommand value) 
      => attachingElement.SetValue(PasswordBoxService.CommandProperty, value);

    /// <summary>
    /// The set method of the attached <see cref="System.Windows.Controls.PasswordBox"/> property.
    /// </summary>
    /// <param name="attachingElement">The <see cref="System.Windows.Controls.PasswordBox"/> element.</param>
    /// <returns>The <see cref="ICommand"/> implementation registered with the <paramref name="attachingElement"/>.</returns>
    public static ICommand GetCommand(DependencyObject attachingElement) 
      => (ICommand)attachingElement.GetValue(PasswordBoxService.CommandProperty);

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

      System.Windows.WeakEventManager<object, RoutedEventArgs>.AddHandler(
        passwordBox,
        nameof(System.Windows.Controls.PasswordBox.PasswordChanged),
        PasswordBoxService.SendPassword_OnPasswordChanged);
    }

    private static void SendPassword_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
      var attachedElement = sender as System.Windows.Controls.PasswordBox;
      SecureString commandParameter = attachedElement?.SecurePassword;
      if (commandParameter == null || commandParameter.Length < 1)
      {
        return;
      }

      ICommand sendCommand = PasswordBoxService.GetCommand(attachedElement);
      sendCommand?.Execute(commandParameter);
    }
  }
}
