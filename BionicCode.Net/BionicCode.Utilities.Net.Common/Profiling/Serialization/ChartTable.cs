namespace BionicCode.Utilities.Net
{
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  internal class ChartTable
  {
    public ChartTable(int dataSetCount, int dataColumnOffset)
    {
      this.Rows = new List<ChartTableRow>();
      this.Columns = new List<ChartTableColumn>();
      this.SeriesResultToRowIndexMap = new List<SeriesResultToRowIndexMap>();
      this.DataSetCount = dataSetCount;
      this.DataColumnOffset = dataColumnOffset;
    }

    public ChartTableRowBuilder CreateTableRowBuilder() => new ChartTableRowBuilder(this, this.ColumnCount);

    public void AddRow(ChartTableRow row) => this.Rows.Add(row);
    public void AddColumn(ChartTableColumn column) => this.Columns.Add(column);
    public SeriesResultToRowIndexMap AddResultMapperEntry(int originalProfilerResultCount)
    {
      var seriesMapper = new SeriesResultToRowIndexMap(originalProfilerResultCount);
      this.SeriesResultToRowIndexMap.Add(seriesMapper);

      return seriesMapper;
    }

    [JsonPropertyName("dataSetCount")]
    public int DataSetCount { get; private set; }

    [JsonPropertyName("dataColumnOffset")]
    public int DataColumnOffset { get; private set; }

    [JsonPropertyName("columns")]
    public IList<ChartTableColumn> Columns { get; set; }

    [JsonPropertyName("rows")]
    public IList<ChartTableRow> Rows { get; set; }

    [JsonPropertyName("seriesResultToRowIndexMap")]
    public IList<SeriesResultToRowIndexMap> SeriesResultToRowIndexMap { get; set; }

    [JsonPropertyName("options")]
    public ChartOptions Options { get; set; }

    [JsonIgnore]
    public int ColumnCount => this.Columns.Count;
    [JsonIgnore]
    public int RowCount => this.Rows.Count;
  }

  internal class SeriesResultToRowIndexMap
  {
    public SeriesResultToRowIndexMap(int capacity)
      => this.ResultToRowIndexMap = new ResultMapperEntry[capacity];

    public void AddResultMapperEntry(int resultIndex, int rowIndex) => this.ResultToRowIndexMap[resultIndex] = new ResultMapperEntry(resultIndex, rowIndex);

    [JsonPropertyName("resultToRowIndexMap")]
    public ResultMapperEntry[] ResultToRowIndexMap { get; }
  }

  internal readonly struct ResultMapperEntry
  {
    public ResultMapperEntry(int resultIndex, int tableRowIndex)
    {
      this.ResultIndex = resultIndex;
      this.TableRowIndex = tableRowIndex;
    }

    [JsonPropertyName("resultIndex")]
    public int ResultIndex { get; }
    [JsonPropertyName("tableRowIndex")]
    public int TableRowIndex { get; }
  }

  [JsonConverter(typeof(CollectionWithCountJsonConverter))]
  internal class ChartTableCollection : List<ChartTable>
  {
  }
}