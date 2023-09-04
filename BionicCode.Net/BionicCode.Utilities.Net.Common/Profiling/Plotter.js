google.charts.load('current', {{ packages: ['corechart'] }});
google.charts.setOnLoadCallback(prepareData);

function prepareData() {{

  const chartTableJsonText = '{0}';
  const chartTableJson = JSON.parse(chartTableJsonText);
  var dataTable = new google.visualization.DataTable();

  const columns = chartTableJson['columns'];
  for (let columnIndex = 0; columnIndex < columns.length; columnIndex += 1) {{
    const column = columns[columnIndex];
    dataTable.addColumn(column);
  }}

  const rows = chartTableJson['rows'];
  for (let rowIndex = 0; rowIndex < rows.length; rowIndex += 1) {{
    const row = rows[rowIndex];
    const cells = row['cellValues'];
    dataTable.addRow(cells);
  }}

  drawChart(dataTable, chartTableJson['options']);
}}

function drawChart(dataTable, options) {{
    const chart = new google.visualization.LineChart(document.getElementById('chart'));
    chart.draw(dataTable, options);
}}  