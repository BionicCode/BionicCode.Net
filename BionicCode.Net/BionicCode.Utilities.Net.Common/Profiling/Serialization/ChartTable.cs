namespace BionicCode.Utilities.Net
{
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  internal class ChartTable
  {
    public ChartTable()
    {
      this.Rows = new List<ChartTableRow>();
      this.Columns = new List<ChartTableColumn>();
    }

    public ChartTableRowBuilder CreateTableRowBuilder() => new ChartTableRowBuilder(this, this.ColumnCount);

    public void AddRow(ChartTableRow row) => this.Rows.Add(row);
    public void AddColumn(ChartTableColumn column) => this.Columns.Add(column);

    [JsonPropertyName("columns")]
    public IList<ChartTableColumn> Columns { get; set; }

    [JsonPropertyName("rows")]
    public IList<ChartTableRow> Rows { get; set; }

    [JsonPropertyName("options")]
    public ChartOptions Options { get; set; }

    [JsonIgnore]
    public int ColumnCount => this.Columns.Count;
    [JsonIgnore]
    public int RowCount => this.Rows.Count;
  }
}