namespace BionicCode.Utilities.Net.Wpf.AttachedBehaviors
{
  /// <summary>
  /// Represents a text range, described by a <see cref="StartIndex"/> and <see cref="EndIndex"/>.
  /// </summary>
  public struct HighlightRange
  {
    /// <summary>
    /// Creates a new instance of <see cref="HighlightRange"/>
    /// </summary>
    /// <param name="startIndex"></param>
    /// <param name="endIndex"></param>
    public HighlightRange(int startIndex, int endIndex)
    {
      this.StartIndex = startIndex;
      this.EndIndex = endIndex;
    }

    /// <summary>
    /// Holds the starting index of the text range.
    /// </summary>
    /// <value>An integer that describes the starting index of a index based text representation.</value>
    public int StartIndex { get; set; }
    /// <summary>
    /// Holds the end index of the text range.
    /// </summary>
    /// <value>An integer that describes the end index of a index based text representation.</value>
    public int EndIndex { get; set; }
  }
}
