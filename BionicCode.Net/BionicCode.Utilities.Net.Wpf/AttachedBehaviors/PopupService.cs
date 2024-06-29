
namespace BionicCode.Utilities.Net
{
#if NET || NET461_OR_GREATER
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Windows;

  /// <summary>
  /// Set of attached behaviors for the <see cref="System.Windows.Controls.Primitives.Popup"/> control.
  /// </summary>
  /// <seealso href="https://github.com/BionicCode/BionicCode.Net#popup">See advanced example</seealso>
  public class PopupService : DependencyObject
  {
    #region IsFollowPlacementTargetPositionEnabled attached property

    /// <summary>
    /// When set to <c>true</c>, the <see cref="System.Windows.Controls.Primitives.Popup"/> is forced to stick to the current <see cref="System.Windows.Controls.Primitives.Popup.PlacementTarget"/>. The <see cref="System.Windows.Controls.Primitives.Popup"/> will follow the <see cref="System.Windows.Controls.Primitives.Popup.PlacementTarget"/> whenever it changes it's screen coordinates.
    /// </summary>
    public static readonly DependencyProperty IsStickyProperty =
      DependencyProperty.RegisterAttached(
        "IsSticky",
        typeof(bool),
        typeof(PopupService),
        new PropertyMetadata(default(bool), PopupService.OnIsStickyChanged));

    /// <summary>
    /// The set method of the attached <see cref="IsStickyProperty"/> property.
    /// </summary>
    /// <param name="attachingElement">The <see cref="System.Windows.Controls.Primitives.Popup"/> element.</param>
    /// <param name="value"><c>true</c> to enable the behavior or <c>false</c> to disable it.</param>
    public static void SetIsSticky(DependencyObject attachingElement, bool value) =>
      attachingElement.SetValue(PopupService.IsStickyProperty, value);

    /// <summary>
    /// Get method of the attachecd <see cref="IsStickyProperty"/> property.
    /// </summary>
    /// <param name="attachingElement">The <see cref="System.Windows.Controls.Primitives.Popup"/> element.</param>
    /// <returns><c>true</c> if the behavior is enabled or <c>false</c> if disabled.</returns>
    public static bool GetIsSticky(DependencyObject attachingElement) =>
      (bool)attachingElement.GetValue(PopupService.IsStickyProperty);

    #endregion

    private static Dictionary<Window, IList<System.Windows.Controls.Primitives.Popup>> WindowToPopupsMap { get; set; }

    static PopupService() => PopupService.WindowToPopupsMap = new Dictionary<Window, IList<System.Windows.Controls.Primitives.Popup>>();

    private static void OnIsStickyChanged(
      DependencyObject attachingElement,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(attachingElement is System.Windows.Controls.Primitives.Popup popup))
      {
        throw new ArgumentException("Attaching element must be of type 'System.Windows.Controls.Primitives.Popup'");
      }

      bool isEnabled = (bool)e.NewValue;

      if (isEnabled)
      {
        if (!popup.IsLoaded)
        {
          popup.Loaded += PopupService.EnableFollowTargetMovementOnPopupLoaded;
          return;
        }

        PopupService.EnableFollowTargetMovement(popup);
      }
      else
      {
        PopupService.DisableFollowTargetMovement(popup);
      }
    }

    private static void EnableFollowTargetMovementOnPopupLoaded(object sender, RoutedEventArgs e) => PopupService.EnableFollowTargetMovement(sender as System.Windows.Controls.Primitives.Popup);

    private static void EnableFollowTargetMovement(System.Windows.Controls.Primitives.Popup popup)
    {
      Window window = PopupService.GetParentWindow(popup);
      if (PopupService.WindowToPopupsMap.TryGetValue(window, out IList<System.Windows.Controls.Primitives.Popup> popups)
        && popups != null
        && !popups.Contains(popup))
      {
        popups.Add(popup);
      }
      else
      {
        PopupService.WindowToPopupsMap.Add(window, new List<System.Windows.Controls.Primitives.Popup>() { popup });
      }

      WeakEventManager<Window, EventArgs>.AddHandler(
        window,
        nameof(Window.LocationChanged),
        PopupService.UpdatePopup_OnParentWindowMoved);

      WeakEventManager<Window, SizeChangedEventArgs>.AddHandler(
        window,
        nameof(FrameworkElement.SizeChanged),
        PopupService.UpdatePopup_OnParentWindowSizeChanged);
    }

    private static void DisableFollowTargetMovement(System.Windows.Controls.Primitives.Popup popup)
    {
      Window window = PopupService.GetParentWindow(popup);
      if (PopupService.WindowToPopupsMap.TryGetValue(window, out IList<System.Windows.Controls.Primitives.Popup> popups)
          && !popups.Contains(popup))
      {
        _ = popups.Remove(popup);
        if (!popups.Any())
        {
          _ = PopupService.WindowToPopupsMap.Remove(window);
        }
      }

      WeakEventManager<Window, EventArgs>.RemoveHandler(
        window,
        nameof(Window.LocationChanged),
        PopupService.UpdatePopup_OnParentWindowMoved);

      WeakEventManager<Window, SizeChangedEventArgs>.RemoveHandler(
        window,
        nameof(FrameworkElement.SizeChanged),
        PopupService.UpdatePopup_OnParentWindowSizeChanged);
    }

    private static Window GetParentWindow(System.Windows.Controls.Primitives.Popup popup)
    {
      if (popup.PlacementTarget == null || !popup.PlacementTarget.TryFindVisualParentElement(out Window window))
      {
        window = Application.Current.MainWindow;
      }

      return window;
    }

    private static void UpdatePopup_OnParentWindowMoved(object sender, EventArgs e)
    {
      var window = sender as Window;
      PopupService.UpdatePopupPosition(window);
    }

    private static void UpdatePopup_OnParentWindowSizeChanged(object sender, SizeChangedEventArgs e)
    {
      var window = sender as Window;
      PopupService.UpdatePopupPosition(window);
    }

    private static void UpdatePopupPosition(Window window)
    {
      if (PopupService.WindowToPopupsMap.TryGetValue(window, out IList<System.Windows.Controls.Primitives.Popup> popups))
      {
        foreach (System.Windows.Controls.Primitives.Popup popup in popups)
        {
          popup.HorizontalOffset += 1;
          popup.HorizontalOffset -= 1;
        }
      }
    }
  }
#endif
}
