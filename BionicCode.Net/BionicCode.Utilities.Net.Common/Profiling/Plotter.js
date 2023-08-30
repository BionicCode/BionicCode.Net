google.charts.load('current', {{ packages: ['corechart'] }});
google.charts.setOnLoadCallback(prepareData);

function prepareData() {{
    const dataValuesJsonText = '{0}';
    const dataValuesCollectionJson = JSON.parse(dataValuesJsonText);
    let dataValuesCollection = [];
    for (let dataValuesCollectionJsonIndex = 0; dataValuesCollectionJsonIndex < dataValuesCollectionJson.length; dataValuesCollectionJsonIndex += 1) {{
      let dataValuesJson = dataValuesCollectionJson[dataValuesCollectionJsonIndex]['Values'];
      let dataValues = [['Elapsed time', 'Probability density']];
      for (let dataValuesIndex = 0; dataValuesIndex < dataValuesJson.length; dataValuesIndex += 1) {{
        const value = [dataValuesJson[dataValuesIndex].X, dataValuesJson[dataValuesIndex].Y];
        dataValues.push(value);
      }}

      dataValuesCollection.push(dataValues);
  }}

  drawChart(dataValuesCollection);
  document.getElementById('json_data').innerHTML += "xyz:" + dataValuesCollection;
}}

function drawChart(dataValuesCollection) {{
  const options = {{
    title: 'Normal distribution',
    hAxis: {{ title: 'Elapsed time [µs]' }},
    vAxis: {{ title: 'Probability density' }},
    legend: 'none'
}};
  for (let dataValuesCollectionIndex = 0; dataValuesCollectionIndex < dataValuesCollection.length; dataValuesCollectionIndex += 1) {{
      
    let dataValues = dataValuesCollection[dataValuesCollectionIndex];
      const data = google.visualization.arrayToDataTable(dataValues);
      const chart = new google.visualization.LineChart(document.getElementById('chart'));
      chart.draw(data, options);
  }}
}}