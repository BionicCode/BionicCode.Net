namespace BionicCode.Utilities.Net.Profiling
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    internal class ChartTable
    {
        public ChartTable(int dataSetCount, int dataColumnOffset)
        {
            Rows = new List<ChartTableRow>();
            Columns = new List<ChartTableColumn>();
            SeriesResultToRowIndexMap = new List<SeriesResultToRowIndexMap>();
            DataSetCount = dataSetCount;
            DataColumnOffset = dataColumnOffset;
        }

        public ChartTableRowBuilder CreateTableRowBuilder() => new ChartTableRowBuilder(this, ColumnCount);

        public void AddRow(ChartTableRow row) => Rows.Add(row);
        public void AddColumn(ChartTableColumn column) => Columns.Add(column);
        public SeriesResultToRowIndexMap AddResultMapperEntry(int originalProfilerResultCount)
        {
            var seriesMapper = new SeriesResultToRowIndexMap(originalProfilerResultCount);
            SeriesResultToRowIndexMap.Add(seriesMapper);

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
        public int ColumnCount => Columns.Count;
        [JsonIgnore]
        public int RowCount => Rows.Count;
    }

    internal class SeriesResultToRowIndexMap
    {
        public SeriesResultToRowIndexMap(int capacity)
          => ResultToRowIndexMap = new ResultMapperEntry[capacity];

        public void AddResultMapperEntry(int resultIndex, int rowIndex) => ResultToRowIndexMap[resultIndex] = new ResultMapperEntry(resultIndex, rowIndex);

        [JsonPropertyName("resultToRowIndexMap")]
        public ResultMapperEntry[] ResultToRowIndexMap { get; }
    }

    internal readonly struct ResultMapperEntry
    {
        public ResultMapperEntry(int resultIndex, int tableRowIndex)
        {
            ResultIndex = resultIndex;
            TableRowIndex = tableRowIndex;
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
