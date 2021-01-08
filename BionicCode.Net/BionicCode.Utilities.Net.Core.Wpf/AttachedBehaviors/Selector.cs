using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using BionicCode.Utilities.Net.Core.Wpf.Extensions;

namespace BionicCode.Utilities.Net.Core.Wpf.AttachedBehaviors
{
  public class Selector : DependencyObject
  {
    #region IsInfiniteScrollEnabled attached property

    public static readonly DependencyProperty IsInfiniteScrollEnabledProperty = DependencyProperty.RegisterAttached(
      "IsInfiniteScrollEnabled",
      typeof(bool),
      typeof(Selector),
      new PropertyMetadata(default));

    public static void SetIsInfiniteScrollEnabled(DependencyObject attachingElement, bool value) => attachingElement.SetValue(Selector.IsInfiniteScrollEnabledProperty, value);

    public static bool GetIsInfiniteScrollEnabled(DependencyObject attachingElement) => (bool)attachingElement.GetValue(Selector.IsInfiniteScrollEnabledProperty);


    #endregion IsInfiniteScrollEnabled attached property

    #region IsSelectOnScrollEnabled attached property

    public static readonly DependencyProperty IsSelectOnScrollEnabledProperty = DependencyProperty.RegisterAttached(
      "IsSelectOnScrollEnabled",
      typeof(bool),
      typeof(Selector),
      new PropertyMetadata(default(bool), Selector.OnIsSelectOnScrollEnabledChanged));

    public static void SetIsSelectOnScrollEnabled(DependencyObject attachingElement, bool value) => attachingElement.SetValue(Selector.IsSelectOnScrollEnabledProperty, value);

    public static bool GetIsSelectOnScrollEnabled(DependencyObject attachingElement) => (bool)attachingElement.GetValue(Selector.IsSelectOnScrollEnabledProperty);


    #endregion IsSelectOnScrollEnabled attached property

    private static Dictionary<System.Windows.Controls.Primitives.Selector, ScrollViewer> ScrollViewerTable { get; }
    private static Dictionary<ScrollViewer, System.Windows.Controls.Primitives.Selector> ScrollViewerReverseTable { get; }

    static Selector()
    {
      Selector.ScrollViewerTable = new Dictionary<System.Windows.Controls.Primitives.Selector, ScrollViewer>();
      Selector.ScrollViewerReverseTable = new Dictionary<ScrollViewer, System.Windows.Controls.Primitives.Selector>();
    }

    private static void OnIsSelectOnScrollEnabledChanged(DependencyObject attachedElement, DependencyPropertyChangedEventArgs e)
    {
      if (!(attachedElement is System.Windows.Controls.Primitives.Selector selector))
      {
        throw new ArgumentException($"Attaching element must be of type '{typeof(System.Windows.Controls.Primitives.Selector).FullName}'", nameof(attachedElement));
      }
      if ((bool)e.NewValue)
      {
        Selector.EnableInfiniteScroll(selector);
      }
      else
      {
        Selector.DisableInfiniteScroll(selector);
      }
    }

    private static void EnableInfiniteScroll(System.Windows.Controls.Primitives.Selector scrollTargetElement)
    {
      if (!scrollTargetElement.IsLoaded)
      {
        scrollTargetElement.Loaded += Selector.EnableInfiniteScrollOnLoaded;
        return;
      }

      scrollTargetElement.ApplyTemplate();
      if (scrollTargetElement.TryFindVisualChildElement(out ScrollViewer scrollViewer))
      {
        Selector.ScrollViewerTable.Add(scrollTargetElement, scrollViewer);
        Selector.ScrollViewerReverseTable.Add(scrollViewer, scrollTargetElement);
      }
      scrollTargetElement.PreviewMouseWheel += Selector.HandleInfiniteLoopOnScrollChanged;
      scrollTargetElement.SelectionChanged += Selector.OnSelectionChanged;
      Selector.ScrollIntoView(scrollTargetElement);
    }

    private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      var selector = sender as System.Windows.Controls.Primitives.Selector;
      Selector.ScrollIntoView(selector);
    }

    private static void EnableInfiniteScrollOnLoaded(object sender, RoutedEventArgs e)
    {
      var selector = sender as System.Windows.Controls.Primitives.Selector;
      selector.Loaded -= Selector.EnableInfiniteScrollOnLoaded;
      Selector.EnableInfiniteScroll(selector);
    }

    private static void DisableInfiniteScroll(System.Windows.Controls.Primitives.Selector scrollTargetElement)
    {
      if (Selector.ScrollViewerTable.TryGetValue(scrollTargetElement, out ScrollViewer scrollViewer))
      {
        Selector.ScrollViewerTable.Remove(scrollTargetElement);
        Selector.ScrollViewerReverseTable.Remove(scrollViewer);
      }

      scrollTargetElement.PreviewMouseWheel -= Selector.HandleInfiniteLoopOnScrollChanged;
      scrollTargetElement.SelectionChanged -= Selector.OnSelectionChanged;
    }

    private static void HandleInfiniteLoopOnScrollChanged(object sender, MouseWheelEventArgs mouseWheelEventArgs)
    {
      var selector = sender as System.Windows.Controls.Primitives.Selector;
      switch (mouseWheelEventArgs.Delta)
      {
        case int delta when delta > 0:
          if (!selector.Items.MoveCurrentToPrevious())
          {
            if (Selector.GetIsInfiniteScrollEnabled(selector))
            {
              selector.Items.MoveCurrentToLast();
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
            if (Selector.GetIsInfiniteScrollEnabled(selector))
            {
              selector.Items.MoveCurrentToFirst();
            }
            else
            {
              return;
            }
          }
          break;
      }
      selector.SelectedIndex = selector.Items.CurrentPosition;
      Selector.ScrollIntoView(selector);
    }

    private static void ScrollIntoView(System.Windows.Controls.Primitives.Selector selector)
    {
      Selector.EnsureItemRealization(selector);
      Selector.ExecuteDeferredScroll(selector);
    }

    private static void ExecuteDeferredScroll(System.Windows.Controls.Primitives.Selector selector)
    {
      selector.Dispatcher.InvokeAsync(
        () =>
        {
          if (Selector.ScrollViewerTable.TryGetValue(selector, out ScrollViewer scrollViewer))
          {
            if (!scrollViewer.IsLoaded)
            {
              scrollViewer.Loaded += Selector.InitializeScrollPositionOnScrollViewerLoaded;
              return;
            }

            Selector.ScrollItemToCenter(scrollViewer, selector.SelectedIndex);
          }
          else
          {
            (selector.ItemContainerGenerator.ContainerFromIndex(selector.SelectedIndex) as FrameworkElement)
              ?.BringIntoView();
          }
        },
        DispatcherPriority.ContextIdle);
    }

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
      scrollViewer.Loaded -= Selector.InitializeScrollPositionOnScrollViewerLoaded;
      if (Selector.ScrollViewerReverseTable.TryGetValue(
        scrollViewer,
        out System.Windows.Controls.Primitives.Selector selector))
      {
        Selector.ScrollIntoView(selector);
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
        if (Selector.ScrollViewerReverseTable.TryGetValue(
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
}
