﻿
namespace BionicCode.Utilities.Net
{
#if NET || NET461_OR_GREATER
  using System;
  using System.Collections.Generic;
  using System.Windows;
  using System.Windows.Input;
  using System.Windows.Controls;
  using System.Windows.Threading;

  /// <summary>
  /// An attached behavior that enhances the functionality of a <see cref="System.Windows.Controls.Primitives.Selector"/> for example by enabling infinite scrolling (carousel) or item selection on scroll.
  /// </summary>
  public class SelectorService : DependencyObject
  {
    #region IsInfiniteScrollEnabled attached property

    /// <summary>
    /// Enable carousel mode for a <see cref="System.Windows.Controls.Primitives.Selector"/>.
    /// </summary>
    public static readonly DependencyProperty IsInfiniteScrollEnabledProperty = DependencyProperty.RegisterAttached(
      "IsInfiniteScrollEnabled",
      typeof(bool),
      typeof(SelectorService),
      new PropertyMetadata(default));

    /// <summary>
    /// The set method for the attached property <see cref="IsInfiniteScrollEnabledProperty"/>.
    /// </summary>
    /// <param name="attachingElement">The attaching element.</param>
    /// <param name="value"><see langword="true"/> to enable the behavior or <see langword="false"/> to disable it.</param>
    public static void SetIsInfiniteScrollEnabled(DependencyObject attachingElement, bool value) => attachingElement.SetValue(SelectorService.IsInfiniteScrollEnabledProperty, value);

    /// <summary>
    /// The get method for the attached property <see cref="IsInfiniteScrollEnabledProperty"/>.
    /// </summary>
    /// <param name="attachingElement">The attaching element.</param>
    /// <returns><see langword="true"/> if the behavior is enabled or <see langword="false"/> if it is disabled.</returns>
    public static bool GetIsInfiniteScrollEnabled(DependencyObject attachingElement) => (bool)attachingElement.GetValue(SelectorService.IsInfiniteScrollEnabledProperty);

#endregion IsInfiniteScrollEnabled attached property

#region IsSelectOnScrollEnabled attached property

    /// <summary>
    /// Enable select on scroll behavior for a <see cref="System.Windows.Controls.Primitives.Selector"/>.
    /// </summary>
    public static readonly DependencyProperty IsSelectOnScrollEnabledProperty = DependencyProperty.RegisterAttached(
      "IsSelectOnScrollEnabled",
      typeof(bool),
      typeof(SelectorService),
      new PropertyMetadata(default(bool), SelectorService.OnIsSelectOnScrollEnabledChanged));

    /// <summary>
    /// The set method for the attached property <see cref="IsSelectOnScrollEnabledProperty"/>.
    /// </summary>
    /// <param name="attachingElement">The attaching element.</param>
    /// <param name="value"><see langword="true"/> to enable the behavior or <see langword="false"/> to disable it.</param>
    public static void SetIsSelectOnScrollEnabled(DependencyObject attachingElement, bool value) => attachingElement.SetValue(SelectorService.IsSelectOnScrollEnabledProperty, value);

    /// <summary>
    /// The get method for the attached property <see cref="IsSelectOnScrollEnabledProperty"/>.
    /// </summary>
    /// <param name="attachingElement">The attaching element.</param>
    /// <returns><see langword="true"/> if the behavior is enabled or <see langword="false"/> if it is disabled.</returns>
    public static bool GetIsSelectOnScrollEnabled(DependencyObject attachingElement) => (bool)attachingElement.GetValue(SelectorService.IsSelectOnScrollEnabledProperty);

#endregion IsSelectOnScrollEnabled attached property

    private static Dictionary<System.Windows.Controls.Primitives.Selector, ScrollViewer> ScrollViewerTable { get; }
    private static Dictionary<ScrollViewer, System.Windows.Controls.Primitives.Selector> ScrollViewerReverseTable { get; }

    static SelectorService()
    {
      SelectorService.ScrollViewerTable = new Dictionary<System.Windows.Controls.Primitives.Selector, ScrollViewer>();
      SelectorService.ScrollViewerReverseTable = new Dictionary<ScrollViewer, System.Windows.Controls.Primitives.Selector>();
    }

    private static void OnIsSelectOnScrollEnabledChanged(DependencyObject attachedElement, DependencyPropertyChangedEventArgs e)
    {
      if (!(attachedElement is System.Windows.Controls.Primitives.Selector selector))
      {
        throw new ArgumentException($"Attaching element must be of type '{typeof(System.Windows.Controls.Primitives.Selector).FullName}'", nameof(attachedElement));
      }

      if ((bool)e.NewValue)
      {
        SelectorService.EnableInfiniteScroll(selector);
      }
      else
      {
        SelectorService.DisableInfiniteScroll(selector);
      }
    }

    private static void EnableInfiniteScroll(System.Windows.Controls.Primitives.Selector scrollTargetElement)
    {
      if (!scrollTargetElement.IsLoaded)
      {
        scrollTargetElement.Loaded += SelectorService.EnableInfiniteScrollOnLoaded;
        return;
      }

      _ = scrollTargetElement.ApplyTemplate();
      if (scrollTargetElement.TryFindVisualChildElement(out ScrollViewer scrollViewer))
      {
        SelectorService.ScrollViewerTable.Add(scrollTargetElement, scrollViewer);
        SelectorService.ScrollViewerReverseTable.Add(scrollViewer, scrollTargetElement);
      }

      scrollTargetElement.PreviewMouseWheel += SelectorService.HandleInfiniteLoopOnScrollChanged;
      scrollTargetElement.SelectionChanged += SelectorService.OnSelectionChanged;
      SelectorService.ScrollIntoView(scrollTargetElement);
    }

    private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      var selector = sender as System.Windows.Controls.Primitives.Selector;
      SelectorService.ScrollIntoView(selector);
    }

    private static void EnableInfiniteScrollOnLoaded(object sender, RoutedEventArgs e)
    {
      var selector = sender as System.Windows.Controls.Primitives.Selector;
      selector.Loaded -= SelectorService.EnableInfiniteScrollOnLoaded;
      SelectorService.EnableInfiniteScroll(selector);
    }

    private static void DisableInfiniteScroll(System.Windows.Controls.Primitives.Selector scrollTargetElement)
    {
      if (SelectorService.ScrollViewerTable.TryGetValue(scrollTargetElement, out ScrollViewer scrollViewer))
      {
        _ = SelectorService.ScrollViewerTable.Remove(scrollTargetElement);
        _ = SelectorService.ScrollViewerReverseTable.Remove(scrollViewer);
      }

      scrollTargetElement.PreviewMouseWheel -= SelectorService.HandleInfiniteLoopOnScrollChanged;
      scrollTargetElement.SelectionChanged -= SelectorService.OnSelectionChanged;
    }

    private static void HandleInfiniteLoopOnScrollChanged(object sender, MouseWheelEventArgs mouseWheelEventArgs)
    {
      var selector = sender as System.Windows.Controls.Primitives.Selector;
      switch (mouseWheelEventArgs.Delta)
      {
        case int delta when delta > 0:
          if (!selector.Items.MoveCurrentToPrevious())
          {
            if (SelectorService.GetIsInfiniteScrollEnabled(selector))
            {
              _ = selector.Items.MoveCurrentToLast();
            }
            else
            {
              return;
            }
          }

          break;
        case int delta when delta < 0:
          if (!selector.Items.MoveCurrentToNext())
          {
            if (SelectorService.GetIsInfiniteScrollEnabled(selector))
            {
              _ = selector.Items.MoveCurrentToFirst();
            }
            else
            {
              return;
            }
          }

          break;
      }

      selector.SelectedIndex = selector.Items.CurrentPosition;
      SelectorService.ScrollIntoView(selector);
    }

    private static void ScrollIntoView(System.Windows.Controls.Primitives.Selector selector)
    {
      SelectorService.EnsureItemRealization(selector);
      SelectorService.ExecuteDeferredScroll(selector);
    }

    private static void ExecuteDeferredScroll(System.Windows.Controls.Primitives.Selector selector) => selector.Dispatcher.InvokeAsync(
        () =>
        {
          if (SelectorService.ScrollViewerTable.TryGetValue(selector, out ScrollViewer scrollViewer))
          {
            if (!scrollViewer.IsLoaded)
            {
              scrollViewer.Loaded += SelectorService.InitializeScrollPositionOnScrollViewerLoaded;
              return;
            }

            SelectorService.ScrollItemToCenter(scrollViewer, selector.SelectedIndex);
          }
          else
          {
            (selector.ItemContainerGenerator.ContainerFromIndex(selector.SelectedIndex) as FrameworkElement)
              ?.BringIntoView();
          }
        },
        DispatcherPriority.ContextIdle);

    private static void EnsureItemRealization(System.Windows.Controls.Primitives.Selector selector)
    {
      if (selector is ListBox listBox)
      {
        listBox.ScrollIntoView(selector.SelectedItem);
      }
    }

    private static void InitializeScrollPositionOnScrollViewerLoaded(object sender, RoutedEventArgs e)
    {
      var scrollViewer = sender as ScrollViewer;
      scrollViewer.Loaded -= SelectorService.InitializeScrollPositionOnScrollViewerLoaded;
      if (SelectorService.ScrollViewerReverseTable.TryGetValue(
        scrollViewer,
        out System.Windows.Controls.Primitives.Selector selector))
      {
        SelectorService.ScrollIntoView(selector);
      }
    }

    private static void ScrollItemToCenter(ScrollViewer scrollViewer, in int selectorSelectedIndex)
    {
      double verticalOffset = 0;
      if (scrollViewer.CanContentScroll)
      {
        verticalOffset = selectorSelectedIndex + 1;
      }
      else
      {
        if (SelectorService.ScrollViewerReverseTable.TryGetValue(
          scrollViewer,
          out System.Windows.Controls.Primitives.Selector selector))
        {
          var selectedItemContainer =
            selector.ItemContainerGenerator.ContainerFromIndex(0) as UIElement;
          verticalOffset = (selectedItemContainer?.DesiredSize.Height ?? 0) * selectorSelectedIndex;
        }
      }

      scrollViewer.ScrollToVerticalOffset(verticalOffset - scrollViewer.ViewportHeight / 2);
    }
  }
#endif
}
