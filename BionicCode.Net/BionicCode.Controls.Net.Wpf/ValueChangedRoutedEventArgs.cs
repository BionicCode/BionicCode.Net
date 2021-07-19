#region Info
// //  
// BionicCode.BionicSwipePageFrame
#endregion

using System.Windows;

namespace BionicCode.Controls.Net.Wpf
{
  public delegate void ValueChangedRoutedEventHandler<TValue>(object sender, ValueChangedRoutedEventArgs<TValue> e);

  public class ValueChangedRoutedEventArgs<TValue> : RoutedEventArgs
  {
    public TValue OldValue { get; }
    public TValue NewValue { get; }

    public ValueChangedRoutedEventArgs(RoutedEvent routedEvent, object sender, TValue oldValue, TValue newValue) : base(routedEvent, sender)
    {
      this.OldValue = oldValue;
      this.NewValue = newValue;
    }
  }
}