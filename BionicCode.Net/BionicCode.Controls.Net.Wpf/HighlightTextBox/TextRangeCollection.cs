namespace BionicCode.Controls.Net.Wpf
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Collections.Immutable;
  using System.Linq;
  using System.Threading.Tasks;
  using BionicCode.Utilities.Net;

  internal class TextRangeCollection : IEnumerable<TextRange>
  {
    public IEnumerator<TextRange> GetEnumerator() => this.UnsortedTextRanges.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => this.UnsortedTextRanges.GetEnumerator();
    public IImmutableList<TextRange> TextRanges => this.SortedTextRangeCreator.Value;
    private bool IsIndexingPending => this.TextRangeIndex.IsEmpty() && this.TextRanges.Any();
    private Lazy<IImmutableList<TextRange>> SortedTextRangeCreator { get; }
    private Dictionary<int, IImmutableList<TextRange>> TextRangeIndex { get; set; }
    private IEnumerable<TextRange> UnsortedTextRanges { get; }

    public TextRangeCollection(IEnumerable<TextRange> textRanges)
    {
      this.UnsortedTextRanges = new List<TextRange>(textRanges);
      this.UnsortedTextRanges = textRanges.ToImmutableList();
      this.TextRangeIndex = new Dictionary<int, IImmutableList<TextRange>>();
      this.SortedTextRangeCreator = new Lazy<IImmutableList<TextRange>>(CreateSortedTextRanges);
    }

    public IEnumerable<TextRange> AsSortedIEnumerable() => this.TextRanges;

    public async Task<IImmutableList<TextRange>> GetTextRangesFromIndexAsnyc(int index)
    {
      if (this.IsIndexingPending)
      {
        await IndexTextRangesAsync().ConfigureAwait(false);
      }

      return this.TextRangeIndex.TryGetValue(index, out IImmutableList<TextRange> results)
        ? results
        : throw new IndexOutOfRangeException(ExceptionMessages.GetIndexOutOfRangeExceptionMessage(index, nameof(this.TextRangeIndex)));
    }

    public async Task<IEnumerable<TextRange>> GetTextRangesFromRangeAsync(Range indexRange)
    {
      var acceptedTextRanges = new List<TextRange>();
      await Task.Run(() =>
      {
        foreach (TextRange textRangeInRange in this.TextRanges
         .SkipWhile(range => range.Begin < indexRange.Start.Value))
        {
          if (textRangeInRange.Begin < indexRange.End.Value)
          {
            TextRange acceptedTextRange = textRangeInRange;
            if (textRangeInRange.Begin + textRangeInRange.Length >= indexRange.End.Value)
            {
              int clippedLength = indexRange.End.Value - textRangeInRange.Begin;
              acceptedTextRange = textRangeInRange.ClipToBounds(textRangeInRange.Begin, clippedLength);
            }

            acceptedTextRanges.Add(acceptedTextRange);
          }
        }
      });

      return acceptedTextRanges;
    }

    private IImmutableList<TextRange> CreateSortedTextRanges()
      => this.UnsortedTextRanges
        .OrderBy(textRange => textRange.Begin)
        .ToImmutableList();

    private void IndexTextRanges()
    {
      foreach (TextRange textRange in this.UnsortedTextRanges)
      {
        // Map each index of a TextRange.
        // Multiple TextRange items can have the same index (overlap), for example when a range is bold and italic.
        for (int index = textRange.Begin; index < textRange.Begin + textRange.Length; index++)
        {
          if (!this.TextRangeIndex.TryGetValue(index, out IImmutableList<TextRange> bucket))
          {
            bucket = ImmutableList<TextRange>.Empty;
          }

          this.TextRangeIndex[index] = bucket.Add(textRange);
        }
      }
    }

    private Task IndexTextRangesAsync() => Task.Run(IndexTextRanges);
  }
}