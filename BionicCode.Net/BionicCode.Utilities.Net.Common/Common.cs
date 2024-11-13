#region Info

// 2020/09/19  17:17
// BionicCode.Utilities.Net

#endregion

namespace BionicCode.Utilities.Net
{
  using System.Collections;
  using System.ComponentModel;

  /// <summary>
  /// Describes lifetime scope of object instances
  /// </summary>
  public static class Common
  {
    /// <summary>
    /// The default CLR indexer property name <c>"Item[]"</c>.
    /// </summary>
    public const string IndexerName = "Item[]";

    /// <summary>
    /// <see cref="PropertyChangedEventArgs"/> for the <see cref="ICollection.Count"/> property.
    /// </summary>
    /// <remarks>Uses the property name <c>"Count"</c> for the <see cref="ICollection.Count"/> property.</remarks>
    public static readonly PropertyChangedEventArgs CountPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(ICollection.Count));

    /// <summary>
    /// <see cref="PropertyChangedEventArgs"/> for an indexer property.
    /// </summary>
    /// <remarks>Uses the default property name provided by <see cref="IndexerName"/>. You should not use this event args object if you have overridden the default CLR indexer property name (e.g. by decorating the indexer property with the IndexerNameAttribute.</remarks>
    public static readonly PropertyChangedEventArgs IndexerPropertyChangedEventArgs = new PropertyChangedEventArgs(Common.IndexerName);
  }
}