namespace BionicCode.Utilities.Net.Reflection;

using System.Collections;

public interface ISymbolListView<TListView, TItem> : ICollection, IReadOnlyList<TItem>, IEquatable<TListView>
{
    new int Count { get; }
    bool HasItems { get; }
    bool IsEmpty { get; }
}