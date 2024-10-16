google.charts.load('current', { packages: 'corechart' });
addEventListener('load', initialize);
window.addEventListener('resize', onWindowResized);

const popoverTriggerList = document.querySelectorAll('[data-bs-toggle="popover"]')
const popoverList = [...popoverTriggerList].map(popoverTriggerEl => new bootstrap.Popover(popoverTriggerEl));
for (let popoverIndex = 0; popoverIndex < popoverList.length; popoverIndex += 1) {
  let popover = popoverList[popoverIndex];
  popover.popover();
}

const charts = [];
const dataTables = [];
let chartTableCollectionJson;
const selectedColumnIndexTable = [];
const selectedXValueTable = [];
const selectedRowAnnotationBackup = [];
const selectedRowIndexTable = [];
const numberOfTableHeaderRows = 1;
const documentNavigationHostId = "in-document-navigation-host";
const display = document.getElementById('json_data');

function onWindowResized() {
  updateCharts();
}

function initialize() {
  const chartTablesJsonText = '{0}';
  chartTableCollectionJson = JSON.parse(chartTablesJsonText);
  const tableCount = chartTableCollectionJson.tableCount;

  if (tableCount < 2) {
    const documentNavigationHost = document.getElementById(documentNavigationHostId);
    documentNavigationHost.remove();
  }

  const chartDataTables = chartTableCollectionJson.chartTables;
  for (let tableIndex = 0; tableIndex < tableCount; tableIndex += 1) {
    let chartDataTable = chartDataTables[tableIndex];
    const tableElementId = `result-table-${tableIndex}`;
    let tableElement = document.getElementById(tableElementId);
    let dataRows = tableElement.getElementsByClassName('data-row');
    const dataSetIndex = 0;
    for (let rowIndex = 0; rowIndex < dataRows.length; rowIndex += 1) {
      let row = dataRows[rowIndex];
      row.addEventListener(
        'mouseover',
        e => {
          const rowIndexOffset = dataSetIndex * 1;
          const dataColumnOffset = dataSetIndex * chartDataTable.dataColumnOffset;
          const dataColumnIndex = 1 + dataColumnOffset;
          selectTableRow(tableIndex, rowIndex, rowIndexOffset, dataColumnIndex, e);
        }
      );
    }

    prepareData(tableIndex, chartDataTable);
  }
}

function selectTableRow(tableIndex, iterationIndex, rowIndexOffset, dataColumnIndex, eventArgs) {

  const chartDataTables = chartTableCollectionJson.chartTables;
  const selectedTableData = chartDataTables[tableIndex];
  const dataSetOfClickedTable = selectedTableData.seriesResultToRowIndexMap[tableIndex].resultToRowIndexMap;
  const clickedRow = eventArgs.currentTarget;
  const clickedRowIndex = clickedRow.rowIndex - numberOfTableHeaderRows;
  const dataSetCount = selectedTableData.dataSetCount;
  const selectedRowIndex = (dataSetOfClickedTable[clickedRowIndex].tableRowIndex * dataSetCount) + rowIndexOffset;

  const dataTable = dataTables[tableIndex];
  const oldSelectedXValue = selectedXValueTable[tableIndex];
  const selectedXValue = dataTable.getValue(selectedRowIndex, 0);
  selectedXValueTable[tableIndex] = selectedXValue;
  if (selectedXValue === oldSelectedXValue) {
    return;
  }

  const oldSelectedRowIndex = selectedRowIndexTable[tableIndex];
  selectedRowIndexTable[tableIndex] = selectedRowIndex;
  const oldSelectedColumnIndex = selectedColumnIndexTable[tableIndex];
  selectedColumnIndexTable[tableIndex] = dataColumnIndex;
  if (oldSelectedRowIndex != undefined) {
    const originalAnnotation = selectedRowAnnotationBackup[tableIndex];
    dataTable.setValue(oldSelectedRowIndex, oldSelectedColumnIndex + 1, originalAnnotation);
    updateChart(tableIndex);
  }

  selectedRowAnnotationBackup[tableIndex] = dataTable.getValue(selectedRowIndex, dataColumnIndex + 1);
  const selectedRowAnnotation = `Iteration #${iterationIndex + 1}`;
  dataTable.setValue(selectedRowIndex, dataColumnIndex + 1, selectedRowAnnotation);
  const selectedChart = charts[tableIndex];
  google.visualization.events.addListener(selectedChart, 'ready', function onChartReady() {
    google.visualization.events.removeListener(selectedChart, 'ready', onChartReady);
    selectedChart.setSelection([{ row: selectedRowIndex, column: dataColumnIndex }]);
  });

  updateChart(tableIndex);
}

function prepareData(tableIndex, chartTableData) {
  const dataTable = new google.visualization.DataTable();
  dataTables[tableIndex] = dataTable;
  const columns = chartTableData.columns;
  for (let columnIndex = 0; columnIndex < columns.length; columnIndex += 1) {
    const column = columns[columnIndex];
    dataTable.addColumn(column);
  }

  const rows = chartTableData.rows;
  for (let rowIndex = 0; rowIndex < rows.length; rowIndex += 1) {
    const row = rows[rowIndex];
    const cells = row.cellValues;
    dataTable.addRow(cells);
  }

  updateChart(tableIndex);
}

function updateChart(tableIndex) {
  const chartDataTables = chartTableCollectionJson.chartTables;
  const chartDataTable = chartDataTables[tableIndex];
  const chartOptions = chartDataTable.options;
  drawChart(tableIndex, chartOptions);
}

function updateCharts() {
  const tableCount = chartTableCollectionJson.tableCount;
  const chartDataTables = chartTableCollectionJson.chartTables;
  for (let tableIndex = 0; tableIndex < tableCount; tableIndex += 1) {
    const chartDataTable = chartDataTables[tableIndex];
    const chartOptions = chartDataTable.options;
    drawChart(tableIndex, chartOptions);
  }
}

function drawChart(chartIndex, options) {
  if (charts[chartIndex] === undefined) {
    const chartHostElementId = `chart-${chartIndex}`;
    const chartHostElement = document.getElementById(chartHostElementId);
    charts[chartIndex] = new google.visualization.LineChart(chartHostElement);
  }

  const dataTable = dataTables[chartIndex];
  charts[chartIndex].draw(dataTable, options);
}

function copyToClipboard() {
  const filePath = document.getElementById('report_file_path').innerHTML;
  navigator.clipboard.writeText(filePath);
}