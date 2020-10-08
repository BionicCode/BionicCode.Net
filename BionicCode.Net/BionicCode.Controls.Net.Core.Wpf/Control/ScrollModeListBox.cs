using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BionicCode.Controls.Net.Core.Wpf.Control
{
  public enum ItemsUpdatingScrollMode
  {
    Default = 0,
    KeepItemsInView, // Adjusts the scroll offset to keep the first visible item in the viewport when items are added to the ItemsSource.
    KeepLastItemInView, // Adjusts the scroll offset to keep the last visible item in the viewport when items are added to the ItemsSource.
    KeepScrollOffset // Maintains the scroll offset relative to the beginning of the list, forcing items in the viewport to move down when items are added to the ItemsSource.
  }

  public class ScrollModeListBox : System.Windows.Controls.ListBox
    {
      public static readonly DependencyProperty ItemsUpdatingScrollModeProperty = DependencyProperty.Register(
        "ItemsUpdatingScrollMode",
        typeof(ItemsUpdatingScrollMode),
        typeof(ScrollModeListBox),
        new PropertyMetadata(default(ItemsUpdatingScrollMode)));

      public ItemsUpdatingScrollMode ItemsUpdatingScrollMode
      {
        get => (ItemsUpdatingScrollMode) GetValue(ScrollModeListBox.ItemsUpdatingScrollModeProperty);
        set => SetValue(ScrollModeListBox.ItemsUpdatingScrollModeProperty, value);
      }

      private ScrollViewer ScrollViewer { get; set; }

      public ScrollModeListBox()
      {
        this.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;
      }

      #region Overrides of FrameworkElement

      /// <inheritdoc />
      public override void OnApplyTemplate()
      {
        base.OnApplyTemplate();
        if (TryFindVisualChildElement(this, out ScrollViewer scrollViewer))
        {
          this.ScrollViewer = scrollViewer;
        }
      }

      #endregion

      #region Overrides of ListView

      /// <inheritdoc />
      protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
      {
        base.OnItemsChanged(e);
        if (this.ScrollViewer == null)
        {
          return;
        }

        double verticalOffset;
        switch (this.ItemsUpdatingScrollMode)
        {
          case ItemsUpdatingScrollMode.Default:
          case ItemsUpdatingScrollMode.KeepScrollOffset:
            return;
          case ItemsUpdatingScrollMode.KeepItemsInView:
            switch (e.Action)
            {
              case NotifyCollectionChangedAction.Add when e.NewItems != null:

                // Check if insert or add
                verticalOffset = e.NewStartingIndex < this.ScrollViewer.VerticalOffset
                  ? this.ScrollViewer.VerticalOffset + e.NewItems.Count
                  : this.ScrollViewer.VerticalOffset;
                break;
              case NotifyCollectionChangedAction.Remove when e.OldItems != null:
                verticalOffset = this.ScrollViewer.VerticalOffset - e.OldItems.Count;
                break;
              default:
                verticalOffset = this.ScrollViewer.VerticalOffset;
                break;
            }

            break;
          case ItemsUpdatingScrollMode.KeepLastItemInView:
            verticalOffset = this.ScrollViewer.ScrollableHeight;
            break;
          default:
            return;
        }

        this.ScrollViewer?.ScrollToVerticalOffset(verticalOffset);
      }


      #endregion

    public bool TryFindVisualChildElement<TChild>(DependencyObject parent, out TChild childElement)
        where TChild : FrameworkElement
      {
        childElement = null;
        if (parent == null)
        {
          return false;
        }

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
          DependencyObject child = VisualTreeHelper.GetChild(parent, i);
          if (child is TChild resultElement)
          {
            childElement = resultElement;
            return true;
          }

          if (TryFindVisualChildElement(child, out childElement))
          {
            return true;
          }
        }

        return false;
      }
  }
}