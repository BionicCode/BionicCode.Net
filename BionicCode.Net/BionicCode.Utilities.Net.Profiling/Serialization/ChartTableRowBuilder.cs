namespace BionicCode.Utilities.Net.Profiling
{
    using System.Text.Json.Serialization;

    internal class ChartTableRowBuilder
    {
        protected internal ChartTableRowBuilder(ChartTable chartTable, int numberOfCells)
        {
            ChartTable = chartTable;
            CellCount = numberOfCells;
        }

        public ChartTableRow CreateRow()
        {
            var newRow = ChartTableRow.NewRow(CellCount);
            ChartTable.AddRow(newRow);
            return newRow;
        }

        public ChartTableRow CreateRow(params object[] cellValues)
        {
            var newRow = ChartTableRow.NewRow(CellCount, cellValues);
            ChartTable.AddRow(newRow);
            return newRow;
        }

        [JsonIgnore]
        private ChartTable ChartTable { get; }
        [JsonIgnore]
        public int CellCount { get; }
    }
}
