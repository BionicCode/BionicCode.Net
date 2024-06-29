namespace BionicCode.Controls.Net.Wpf
{
  #region Info
  // //  
  // BionicCode.BionicSwipePageFrame
  #endregion

  using System.Windows;



  public class ValueChangedRoutedEventArgs<TValue> : RoutedEventArgs
  {
    public TValue OldValue { get; }
    public TValue NewValue { get; }

    public ValueChangedRoutedEventArgs(RoutedEvent routedEvent, object source, TValue oldValue, TValue newValue) : base(routedEvent, source)
    {
      this.OldValue = oldValue;
      this.NewValue = newValue;
    }

    public ValueChangedRoutedEventArgs()
    {
    }

    public ValueChangedRoutedEventArgs(RoutedEvent routedEvent) : base(routedEvent)
    {
    }

    public ValueChangedRoutedEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source)
    {
    }
  }
}