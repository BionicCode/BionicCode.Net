namespace BionicCode.Utilities.Net
{
  using System.Text.Json.Serialization;

  internal class ChartTableRow
  {
    protected internal static ChartTableRow NewRow(int cellCount) => new ChartTableRow(cellCount);
    protected internal static ChartTableRow NewRow(int cellCount, params object[] cellValues) => new ChartTableRow(cellCount, cellValues);

    private ChartTableRow(int numberOfCells)
    {
      this.CellCount = numberOfCells;
      this.CellValues = new object[numberOfCells];
    }

    private ChartTableRow(int numberOfCells, params object[] cellValues)
    {
      this.CellCount = numberOfCells;
      this.CellValues = new object[numberOfCells];
      for (int index = 0; index < numberOfCells; index++)
      {
        object value = cellValues[index];
        _ = AppendValue(value);
      }
    }

    [JsonPropertyName("cellValues")]
    public object[] CellValues { get; private set; }

    public int AppendValue(object value)
    {
      this.CellValues[this.CurrentIndex] = value;
      return this.CurrentIndex++;
    }

    [JsonIgnore]
    public int CellCount { get; }

    [JsonIgnore]
    private int CurrentIndex { get; set; }
  }
}