﻿// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Windows.UI.Xaml;
//
// namespace BionicCode.Utilities.Net.Uwp.AttachedBehaviors
// {
//   /// <summary>
//   /// Set of attached behaviors for the <see cref="System.Windows.Controls.Primitives.Popup"/> control.
//   /// </summary>
//   /// <seealso href="https://github.com/BionicCode/BionicCode.Net#popup">See advanced example</seealso>
//   public class Popup : DependencyObject
//   {
//     #region IsFollowPlacementTargetPositionEnabled attached property
//
//     /// <summary>
//     /// When set to <c>true</c>, the <see cref="System.Windows.Controls.Primitives.Popup"/> is forced to stick to the current <see cref="System.Windows.Controls.Primitives.Popup.PlacementTarget"/>. The <see cref="System.Windows.Controls.Primitives.Popup"/> will follow the <see cref="System.Windows.Controls.Primitives.Popup.PlacementTarget"/> whenever it changes it's screen coordinates.
//     /// </summary>
//     public static readonly DependencyProperty IsStickyProperty =
//       DependencyProperty.RegisterAttached(
//         "IsSticky",
//         typeof(bool),
//         typeof(Popup),
//         new PropertyMetadata(default(bool), Popup.OnIsStickyChanged));
//
//     /// <summary>
//     /// The set method of the attached <see cref="IsStickyProperty"/> property.
//     /// </summary>
//     /// <param name="attachingElement">The <see cref="System.Windows.Controls.Primitives.Popup"/> element.</param>
//     /// <param name="value"><c>true</c> to enable the behavior or <c>false</c> to disable it.</param>
//     public static void SetIsSticky(DependencyObject attachingElement, bool value) =>
//       attachingElement.SetValue(Popup.IsStickyProperty, value);
//
//     /// <summary>
//     /// Get method of the attachecd <see cref="IsStickyProperty"/> property.
//     /// </summary>
//     /// <param name="attachingElement">The <see cref="System.Windows.Controls.Primitives.Popup"/> element.</param>
//     /// <returns><c>true</c> if the behavior is enabled or <c>false</c> if disabled.</returns>
//     public static bool GetIsSticky(DependencyObject attachingElement) =>
//       (bool)attachingElement.GetValue(Popup.IsStickyProperty);
//
//     #endregion
//
//     private static Dictionary<Window, IList<System.Windows.Controls.Primitives.Popup>> WindowToPopupsMap { get; set; }
//
//     static Popup() => Popup.WindowToPopupsMap = new Dictionary<Window, IList<System.Windows.Controls.Primitives.Popup>>();
//
//
//     private static void OnIsStickyChanged(
//       DependencyObject attachingElement,
//       DependencyPropertyChangedEventArgs e)
//     {
//       if (!(attachingElement is System.Windows.Controls.Primitives.Popup popup))
//       {
//         throw new ArgumentException("Attaching element must be of type 'System.Windows.Controls.Primitives.Popup'");
//       }
//
//       bool isEnabled = (bool)e.NewValue;
//
//       if (isEnabled)
//       {
//         if (!popup.IsLoaded)
//         {
//           popup.Loaded += Popup.EnableFollowTargetMovementOnPopupLoaded;
//           return;
//         }
//
//         Popup.EnableFollowTargetMovement(popup);
//       }
//       else
//       {
//         Popup.DisableFollowTargetMovement(popup);
//       }
//     }
//
//     private static void EnableFollowTargetMovementOnPopupLoaded(object sender, RoutedEventArgs e)
//     {
//       Popup.EnableFollowTargetMovement(sender as System.Windows.Controls.Primitives.Popup);
//     }
//
//     private static void EnableFollowTargetMovement(System.Windows.Controls.Primitives.Popup popup)
//     {
//       Window window = Popup.GetParentWindow(popup);
//       if (Popup.WindowToPopupsMap.TryGetValue(window, out IList<System.Windows.Controls.Primitives.Popup> popups)
//           && !popups.Contains(popup))
//       {
//         popups.Add(popup);
//       }
//       else
//       {
//         Popup.WindowToPopupsMap.Add(window, new List<System.Windows.Controls.Primitives.Popup>() { popup });
//       }
//
//       WeakEventManager<Window, EventArgs>.AddHandler(
//         window,
//         nameof(Window.LocationChanged),
//         Popup.UpdatePopup_OnParentWindowMoved);
//
//       WeakEventManager<Window, SizeChangedEventArgs>.AddHandler(
//         window,
//         nameof(FrameworkElement.SizeChanged),
//         Popup.UpdatePopup_OnParentWindowSizeChanged);
//     }
//
//     private static void DisableFollowTargetMovement(System.Windows.Controls.Primitives.Popup popup)
//     {
//       Window window = Popup.GetParentWindow(popup);
//       if (Popup.WindowToPopupsMap.TryGetValue(window, out IList<System.Windows.Controls.Primitives.Popup> popups)
//           && !popups.Contains(popup))
//       {
//         popups.Remove(popup);
//         if (!popups.Any())
//         {
//           Popup.WindowToPopupsMap.Remove(window);
//         }
//       }
//
//       WeakEventManager<Window, EventArgs>.RemoveHandler(
//         window,
//         nameof(Window.LocationChanged),
//         Popup.UpdatePopup_OnParentWindowMoved);
//
//       WeakEventManager<Window, SizeChangedEventArgs>.RemoveHandler(
//         window,
//         nameof(FrameworkElement.SizeChanged),
//         Popup.UpdatePopup_OnParentWindowSizeChanged);
//     }
//
//     private static Window GetParentWindow(System.Windows.Controls.Primitives.Popup popup)
//     {
//       if (popup.PlacementTarget == null || !popup.PlacementTarget.TryFindVisualParentElement(out Window window))
//       {
//         window = Application.Current.MainWindow;
//       }
//
//       return window;
//     }
//
//     private static void UpdatePopup_OnParentWindowMoved(object sender, EventArgs e)
//     {
//       var window = sender as Window;
//       Popup.UpdatePopupPosition(window);
//     }
//
//     private static void UpdatePopup_OnParentWindowSizeChanged(object sender, SizeChangedEventArgs e)
//     {
//       var window = sender as Window;
//       Popup.UpdatePopupPosition(window);
//     }
//
//     private static void UpdatePopupPosition(Window window)
//     {
//       if (Popup.WindowToPopupsMap.TryGetValue(window, out IList<System.Windows.Controls.Primitives.Popup> popups))
//       {
//         foreach (var popup in popups)
//         {
//           popup.HorizontalOffset += 1;
//           popup.HorizontalOffset -= 1;
//         }
//       }
//     }
//   }
// }
