using System.Windows;
using System.Windows.Controls;

namespace BionicCode.Controls.Net.Wpf
{
  /// <summary>
  /// Container element that represents a selectable item in a <see cref="BionicSwipePageFrame"/>.
  /// </summary>
  /// <example>
  /// <code>
  /// &lt;BionicSwipePageFrame x:Name="PageFrame" Height="500" &gt;
  ///   &lt;BionicSwipePage&gt;First XAML created page&lt;/BionicSwipePage&gt;
  ///   &lt;BionicSwipePage&gt;Second XAML created page&lt;/BionicSwipePage&gt;
  ///   &lt;BionicSwipePage&gt;Third XAML created page&lt;/BionicSwipePage&gt;
  ///   &lt;BionicSwipePage&gt;Fourth XAML created page&lt;/BionicSwipePage&gt;
  /// &lt;/BionicSwipePageFrame&gt;
  /// </code>
  /// </example>
  public class BionicSwipePage : ContentControl
  {
    static BionicSwipePage()
    {
      FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(BionicSwipePage), new FrameworkPropertyMetadata(typeof(BionicSwipePage)));
    }
  }
}
