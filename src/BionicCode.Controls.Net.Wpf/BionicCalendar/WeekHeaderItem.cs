namespace BionicCode.Controls.Net.Wpf
{
    #region Info

    // 2021/02/17  21:29
    // BionicCode.Controls.Net.Framework.Wpf

    #endregion

    using System.Windows;
    using System.Windows.Controls;

    public class WeekHeaderItem : ContentControl
    {
        static WeekHeaderItem() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(WeekHeaderItem), new FrameworkPropertyMetadata(typeof(WeekHeaderItem)));
    }
}
