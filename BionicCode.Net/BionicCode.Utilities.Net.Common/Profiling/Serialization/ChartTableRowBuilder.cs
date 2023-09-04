namespace BionicCode.Utilities.Net
{
  using System.Text.Json.Serialization;

  internal class ChartTableRowBuilder
  {
    internal protected ChartTableRowBuilder(ChartTable chartTable, int numberOfCells)
    {
      this.ChartTable = chartTable;
      this.CellCount = numberOfCells;
    }

    public ChartTableRow CreateRow()
    {
      var newRow = ChartTableRow.NewRow(this.CellCount);
      this.ChartTable.AddRow(newRow);
      return newRow;
    }

    public ChartTableRow CreateRow(params object[] cellValues)
    {
      var newRow = ChartTableRow.NewRow(this.CellCount, cellValues);
      this.ChartTable.AddRow(newRow);
      return newRow;
    }

    [JsonIgnore]
    private ChartTable ChartTable { get; }
    [JsonIgnore]
    public int CellCount { get; }
  }
}