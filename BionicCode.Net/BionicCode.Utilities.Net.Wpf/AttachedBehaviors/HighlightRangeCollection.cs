#region Info

// 2020/06/15  16:03
// BionicCode.BionicUtilities.Net.Core.Wpf

#endregion

using System.Collections.ObjectModel;

namespace BionicCode.Utilities.Net.Wpf.AttachedBehaviors
{
  /// <summary>
  /// A collection of <see cref="HighlightRange"/> items. Can be used in XAML.
  /// </summary>
  /// <example>
  /// <code>
  /// &lt;HighlightRangeCollection&gt;
  ///   &lt;HighlightRange StartIndex="5" EndIndex="20" /&gt;
  /// &lt;/HighlightRangeCollection&gt;
  /// </code>
  /// </example>
  public class HighlightRangeCollection : ObservableCollection<HighlightRange>
  {

  }
}