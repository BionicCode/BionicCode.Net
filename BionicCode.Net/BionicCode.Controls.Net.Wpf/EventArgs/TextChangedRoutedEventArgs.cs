namespace BionicCode.Controls.Net.Wpf
{
  using System.Windows;
  using System.Windows.Controls;

  public class TextChangedRoutedEventArgs : RoutedEventArgs
  {
    public TextChangedRoutedEventArgs()
    {
    }

    public TextChangedRoutedEventArgs(RoutedEvent routedEvent)
      : base(routedEvent)
    {
    }

    public TextChangedRoutedEventArgs(RoutedEvent routedEvent, object source)
      : base(routedEvent, source)
    {
    }

    public TextChangedRoutedEventArgs(RoutedEvent routedEvent, object source, UndoAction undoAction, string oldText, string newText, int changeIndex)
      : base(routedEvent, source)
    {
      this.UndoAction = undoAction;
      this.OldText = oldText;
      this.NewText = newText;
      this.ChangeIndex = changeIndex;
    }

    public UndoAction UndoAction { get; init; }
    public string OldText { get; init; }
    public string NewText { get; init; }
    public int ChangeIndex { get; init; }
  }
}
