namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Threading.Tasks;
  using System.Text.Json;
  using System.IO;
  using System.Reflection;
  using System.Runtime.Caching;
  using System.Diagnostics;
  using System.Linq;
  using System.Collections.Concurrent;
  using System.Data;
  using System.Text.Encodings.Web;

  internal class HtmlLogger : IProfilerLogger
  {
    private const string GoolgeChartsJavaScriptSourceFileName = @"Plotter.js";
    private const int DoublePrecision = 3;
    private readonly TimeSpan FileContentCacheExpiration = TimeSpan.FromMinutes(5);

    public async Task LogAsync(ProfilerBatchResultCollection batchResults)
    {
      if (!batchResults.Any())
      {
        return;
      }

      var htmlDocumentBuilder = new StringBuilder();
      string htmlFileName = $"profiler_result_{batchResults.First().TimeStamp.ToString("MM-dd-yyyy_hhmmss.fffffff")}.html";
      string htmlFilePath = Path.Combine(Path.GetTempPath(), htmlFileName);
      string htmlDocument = await CreateHtmlDocumentAsync(batchResults, htmlDocumentBuilder, htmlFilePath);

      using (var streamWriter = new StreamWriter(htmlFilePath, false))
      {
        await streamWriter.WriteAsync(htmlDocument);
      }

      var startInfo = new ProcessStartInfo(htmlFilePath) { UseShellExecute = true };
      _ = Process.Start(startInfo);
    }

    private async Task<string> CreateHtmlDocumentAsync(ProfilerBatchResultCollection batchResults, StringBuilder htmlDocumentBuilder, string htmlFilePath)
    {
      CreateDocumentHead(batchResults, htmlDocumentBuilder, htmlFilePath);
      string scriptCode = await GetJavaScriptCodeTextAsync();
      string finalScriptCode = scriptCode;
      var tasks = new List<Task<NormalDistributionData<CartesianPoint>>>();
      var chartOptions = new ChartOptions()
      {
        Title = $"Normal distribution for {batchResults.ProfiledTargetType.ToDisplayStringValue().ToLowerInvariant()} '{batchResults.ProfiledTargetMemberName}'",
        LegendOptions = new LegendOptions() { Position = LegendOptions.PositionTop },
        Width = 1600,
        Height = 800,
        HorizontalAxis = new ChartAxis() { Title = "Elapsed time [µs]" }
      };

      foreach (ProfilerBatchResult batchResult in batchResults)
      {
        await CreateHtmlTableAsync(batchResult, htmlDocumentBuilder);
        var verticalChartAxis = new ChartAxis()
        {
          Title = $"Probability density ",
          AxisIndex = batchResult.Index
        };
        chartOptions.AddVerticalAxis(verticalChartAxis);

        var series = new ChartSeries()
        {
          SeriesIndex = batchResult.Index,
          TargetAxisIndex = verticalChartAxis.AxisIndex,
          Title = $"{batchResult.Context.TargetType.ToDisplayStringValue()}"
        };
        chartOptions.AddSeries(series);

        if (batchResult.StandardDeviationInMicroseconds == 0)
        {
          continue;
        }

        Task<NormalDistributionData<CartesianPoint>> task = Task.Run(() =>
        {

#if NET7_0_OR_GREATER
        IEnumerable<CartesianPoint> normalDistributionValues = Math.NormDist(batchResult.AverageDurationInMicroseconds, batchResult.StandardDeviationInMicroseconds, 0.01);
#else
          IEnumerable<CartesianPoint> normalDistributionValues = Math.NormDist(batchResult.AverageDuration.TotalMicroseconds(), batchResult.StandardDeviationInMicroseconds, 0.01);

#endif

          var result = new NormalDistributionData<CartesianPoint>(batchResult.Index, normalDistributionValues, batchResult.AverageDurationInMicroseconds, batchResult.StandardDeviationInMicroseconds)
          {
            Title = series.Title
          };
          return result;
        });

        tasks.Add(task);
      }

      NormalDistributionData<CartesianPoint>[] results = await Task.WhenAll(tasks);

      int totalValueCount = results.Max(dataSet => dataSet.Count);
      int metaInfoCellCount = 3;
      int dataCellCount = results.Count();

      // Data column + meta info columns
      int totalCellCountPerEntry = 1 + metaInfoCellCount;

      var chartTable = new ChartTable
      {
        Options = chartOptions
      };

      // X-axis
      chartTable.AddColumn(new ChartTableColumn() { Label = "Elapsed time [µ]", Type = ColumnType.Number });

      foreach (NormalDistributionData<CartesianPoint> dataSet in results)
      {
        // Y-axis
        chartTable.AddColumn(new ChartTableColumn() { Label = dataSet.Title, Type = ColumnType.Number });

        // Data annotations
        chartTable.AddColumn(new ChartTableColumn() { Role = ColumnRole.Annotation, Type = ColumnType.String });
        chartTable.AddColumn(new ChartTableColumn() { Role = ColumnRole.AnnotationText, Type = ColumnType.String });

        // Tooltip column
        chartTable.AddColumn(new ChartTableColumn() { Type = ColumnType.String, Role = ColumnRole.Tooltip });
      }

      ChartTableRowBuilder tableRowBuilder = chartTable.CreateTableRowBuilder();
      for (int index = 0; index < totalValueCount; index++)
      {
        foreach (NormalDistributionData<CartesianPoint> dataSet in results)
        {
          if (dataSet.Count <= index)
          {
            continue;
          }

          CartesianPoint dataPoint = dataSet[index];
          ChartTableRow tableRow = tableRowBuilder.CreateRow();
          tableRow.AppendValue(dataPoint.X);

          for (int dataCellIndex = 0; dataCellIndex < dataCellCount; dataCellIndex++)
          {
            // Skip cells (data and meta info) that belong to a different data set
            if (dataCellIndex != dataSet.Index)
            {
              for (int entryCellCount = 0; entryCellCount < totalCellCountPerEntry; entryCellCount++)
              {
                tableRow.AppendValue(null);
              }

              continue;
            }

            tableRow.AppendValue(dataPoint.Y);

            // Write annotation and annotation text column
            if (dataPoint.X == dataSet.Mean)
            {
              tableRow.AppendValue($"µ = {dataPoint.X} µs");
              tableRow.AppendValue($"Average elapsed time: P({dataPoint.X},{dataPoint.Y})".ToString(System.Globalization.CultureInfo.CurrentUICulture));
            }
            else if (dataPoint.X == dataSet.Mean - dataSet.StandardDeviation)
            {
              tableRow.AppendValue("34.1 %");
              tableRow.AppendValue("-1 σ. Interval µ ± 1σ contains 68.3 % of all values.");
            }
            else if (dataPoint.X == dataSet.Mean - 2d * dataSet.StandardDeviation)
            {
              tableRow.AppendValue("13.6 %");
              tableRow.AppendValue("-2 σ. Interval µ ± 2σ contains 95.4 % of all values.");
            }
            else if (dataPoint.X == dataSet.Mean - 3d * dataSet.StandardDeviation)
            {
              tableRow.AppendValue("2.1 %");
              tableRow.AppendValue("-3 σ. Interval µ ± 3σ contains 99.7 % of all values.");
            }
            else if (dataPoint.X == dataSet.Mean + dataSet.StandardDeviation)
            {
              tableRow.AppendValue("34.1 %");
              tableRow.AppendValue("1 σ. Interval µ ± 1σ contains 68.3 % of all values.");
            }
            else if (dataPoint.X == dataSet.Mean + 2d * dataSet.StandardDeviation)
            {
              tableRow.AppendValue("13.6 %");
              tableRow.AppendValue("2 σ. Interval µ ± 2σ contains 95.4 % of all values.");
            }
            else if (dataPoint.X == dataSet.Mean + 3d * dataSet.StandardDeviation)
            {
              tableRow.AppendValue("2.1 %");
              tableRow.AppendValue("3 σ. Interval µ ± 3σ contains 99.7 % of all values.");
            }
            else
            {
              tableRow.AppendValue(null);
              tableRow.AppendValue(null);
            }

            tableRow.AppendValue($"{dataSet.Title}: {dataPoint.X} µs (density {dataPoint.Y})");
          }
        }
      }

      string jsonDataValues = await ConvertToJsonAsync(chartTable);
      finalScriptCode = string.Format(scriptCode, jsonDataValues);
      InlineScriptIntoHtmlDocument(htmlDocumentBuilder, finalScriptCode, batchResults);

      return htmlDocumentBuilder.ToString();
    }

    private static void InlineScriptIntoHtmlDocument(StringBuilder htmlDocumentBuilder, string finalScriptCode, ProfilerBatchResultCollection batchResults)
    {
      _ = htmlDocumentBuilder.Append($@"<div id=""chart"" style=""width:100%; height:800px;""></div>");

      foreach (ProfilerBatchResult batchResult in batchResults)
      {
        _ = htmlDocumentBuilder.Append($@"<a class=""navigation-link"" href=""#{batchResult.Index}"">{batchResult.Context.TargetType.ToDisplayStringValue()} method results</a>");
      }

      _ = htmlDocumentBuilder
        .Append($@"<a class=""navigation-link"" href=""#document_start"">Go to top 🡡</a>")
        .Append($@"
<script>{finalScriptCode}</script>
</body>
</html>");
    }

    private static void CreateDocumentHead(IEnumerable<ProfilerBatchResult> batchResults, StringBuilder htmlDocumentBuilder, string htmlFilePath)
    {
      _ = htmlDocumentBuilder.Append($@"<!DOCTYPE html>
<html lang=""en"">
<script src=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.1/dist/js/bootstrap.bundle.min.js"" integrity=""sha384-HwwvtgBNo3bZJJLYd8oVXjrBZt8cqVSpeBNS5n7C8IVInixGAoxmnlMuBnhbgrkm"" crossorigin=""anonymous""></script>
<script src=""https://www.gstatic.com/charts/loader.js""></script>
<style>
th, td {{
  padding: 8px 12px 8px 12px;}}
tr:nth-child(even) {{
  background-color: #8fb2e6;
}}
thead  {{position: sticky; top: 0; 
  background-color: #4073a5;
  color: white;
text-align: left;}}

tfoot{{position: sticky;
   bottom: 0;
   background-color: #4073a5;
border-collapse: collapse;
  padding: 4px 8px 4px 8px;

}}

tfoot tr:nth-child(1) {{
  color: white;
  font-weight: bold;
}}

table {{
  border-spacing: 0;
}}


.source-file-span {{
  font-style: italic;
  font-size: 10pt;
}}
.label-span {{
  font-style: italic;
  font-size: 10pt;
}}

.table-host {{
  overflow:auto; 
  max-height: 480px;
}}

.default-icon {{
    width: 16px;
    height: 16px;
    margin: 0px;
}}

.tool-button {{
  padding: 4px;
}}

.navigation-link {{
  margin-right: 24px;
}}
</style>
<head>
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <meta charset=""utf-8"">
  <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.1/dist/css/bootstrap.min.css"" rel=""stylesheet"" integrity=""sha384-4bw+/aepP/YC94hEpVNVgiZdgIC5+VKNBQNGCHeKRQN+PtmoHDEXuppvnDJzQIu9"" crossorigin=""anonymous"">
<title>Page Title</title>
</head>
<body style=""padding: 12px;"">
<div id=""document_start""></div>
<div class=""source-file-span""><p>Report location: <span id=""report_file_path"">{htmlFilePath}</span>
<button type=""button"" class=""btn btn-outline-light btn-sm"" onclick=""copyToClipboard()"" title=""Copy file path to clipboard""><img class=""default-icon"" src=""https://sampoh.de/globalWebResources/icons/svg/clipboard-svgrepo-com.svg"" alt=""copy file path""></img></button></p></div>
<div id=""json_data""></div>");

      foreach (ProfilerBatchResult batchResult in batchResults)
      {
        string memberDisplayName = batchResult.Context?.TargetTypeInfo?.ToDisplayName() ?? string.Empty;
        _ = htmlDocumentBuilder.Append($@"<a class=""navigation-link"" href=""#{batchResult.Index}"">'{(memberDisplayName.Equals(HelperExtensionsCommon.IndexerName, StringComparison.Ordinal) ? "Indexer" : memberDisplayName)}' {batchResult.Context.TargetType.ToDisplayStringValue()} results</a>");
      }

      _ = htmlDocumentBuilder.Append($@"<a class=""navigation-link"" href=""#chart"">Go to plot</a></div>");
    }

    private static async Task<string> ConvertToJsonAsync<TData>(TData chartTable)
    {
      using (var memStream = new MemoryStream())
      {
        await JsonSerializer.SerializeAsync(memStream, chartTable);
        _ = memStream.Seek(0L, SeekOrigin.Begin);
        using (var streamReader = new StreamReader(memStream))
        {
          string jsonDataValues = await streamReader.ReadToEndAsync();
          return jsonDataValues;
        }
      }
    }

    private async Task CreateHtmlTableAsync(ProfilerBatchResult batchResult, StringBuilder htmlDocumentBuilder)
    {
      var htmlEncoder = HtmlEncoder.Create();
      EnvironmentInfo environmentInfo = await Environment.GetEnvironmentInfoAsync();
      _ = htmlDocumentBuilder.Append($@"<div id=""{batchResult.Index}"" style=""font-weight: normal; text-align: left; border: 1px solid black; padding: 12px"">
  <span style=""font-weight: bold; font-size: 18pt"">Profile Context</span><br/>
  <span style=""font-weight: bold; font-size: 14pt"">Code</span><br/>
  	<span class=""label-span"">Target framework: </span><span class=""valueSpan"">{batchResult.Context.RuntimeVersion}</span><br />
  	<span class=""label-span"">Target type: </span><span class=""valueSpan"">{batchResult.Context.TargetType.ToDisplayStringValue()}</span><br />
  	<span class=""label-span"">Target: </span><span class=""valueSpan"">{htmlEncoder.Encode(batchResult.Context.TargetName)}</span><br />
  	<span class=""label-span"">Assembly: </span><span class=""valueSpan"">{batchResult.Context.AssemblyName}</span><br />
  	<span class=""label-span"">Source file: </span><span class=""valueSpan"">{batchResult.Context.SourceFileName}</span><br /> 
  	<span class=""label-span"">Line number: </span><span class=""valueSpan"">{batchResult.Context.LineNumber}</span><br />
  <span style=""font-weight: bold; font-size: 14pt"">Profiler</span><br/>
  	<span class=""label-span"">Timestamp: </span><span class=""valueSpan"">{batchResult.TimeStamp}</span><br />
  	<span class=""label-span"">Iterations: </span><span class=""valueSpan"">{batchResult.IterationCount} runs for each argument list</span><br />
  	<span class=""label-span"">Argument lists: </span><span class=""valueSpan"">{batchResult.ArgumentListCount}</span><br />
  	<span class=""label-span"">Total iterations: </span><span class=""valueSpan"">{batchResult.TotalIterationCount} ({batchResult.IterationCount} runs for each of {batchResult.ArgumentListCount} argument {(batchResult.ArgumentListCount == 1 ? "list" : "lists")}) </span><br />
  <span style=""font-weight: bold; font-size: 14pt"">Machine</span><br/>  
  	<span class=""label-span"">OS architecture: </span><span class=""valueSpan"">{(environmentInfo.Is64BitOperatingSystem ? 64 : 32)} bit</span><br />   
  	<span class=""label-span"">Process architecture: </span><span class=""valueSpan"">{(environmentInfo.Is64BitProcess ? 64 : 32)} bit</span><br />   
  	<span class=""label-span"">Processor: </span><span class=""valueSpan"">{environmentInfo.ProcessorName}</span><br />   
  	<span class=""label-span"">Physical cores: </span><span class=""valueSpan"">{environmentInfo.ProcessorCoreCount}</span><br />   
  	<span class=""label-span"">Logical cores: </span><span class=""valueSpan"">{environmentInfo.ProcessorLogicalCoreCount}</span><br />   
  	<span class=""label-span"">Threads: </span><span class=""valueSpan"">{environmentInfo.ThradCount}</span><br />   
  	<span class=""label-span"">Clock: </span><span class=""valueSpan"">{environmentInfo.ProcessorSpeed / 1000d} GHz</span><br /> 
  </div>
<div class=""table-host""><table>
<thead>

<tr>
<th class=""header"">Iteration #</th>
<th class=""header"">Elapsed time [µs]</th>
<th class=""header"">Mean µ [µs]</th>
<th class=""header"">Deviation [µs]</th>
<th class=""header"">Standard deviation σ [µs]</th>
<th class=""header"">Variance σ²</th>
</tr>
</thead>
<tbody>");

      int resultIndex = 1;
      foreach (ProfilerResult result in batchResult.Results)
      {
        _ = htmlDocumentBuilder.Append($@"<tr>
<td class=""dataRow"">{resultIndex++}</td>
<td class=""dataRow"">{result.ElapsedTimeInMicroseconds}</td>
<td class=""dataRow"">{batchResult.AverageDurationInMicroseconds}</td>
<td class=""dataRow"">{result.DeviationInMicroseconds}</td>
<td class=""dataRow"">{batchResult.StandardDeviationInMicroseconds}</td>
<td class=""dataRow"">{batchResult.Variance}</td>
</tr>");
      }

      _ = htmlDocumentBuilder.Append($@"
</tbody>
<tfoot style=""border-style: solid solid solid solid; border-color: red"">
<tr>
<td colspan=""10"">Totals</td>
</tr>
<tr>
<td class=""dataRow"">{batchResult.TotalIterationCount}</td>
<td class=""dataRow"">{batchResult.TotalDurationInMicroseconds}</td>
<td class=""dataRow"">{batchResult.AverageDurationInMicroseconds}</td>
<td class=""dataRow"">-</td>
<td class=""dataRow"">{batchResult.StandardDeviationInMicroseconds}</td>
<td class=""dataRow"">{batchResult.Variance}</td>
<td class=""dataRow"">Min (fastest): {batchResult.MinResult.ElapsedTimeInMicroseconds} µs (#{batchResult.MinResult.Iteration})</td>
<td class=""dataRow"">Max (slowest): {batchResult.MaxResult.ElapsedTimeInMicroseconds} µs (#{batchResult.MaxResult.Iteration})</td>
</tr>
</tfoot>
</table></div>")
        .Append($@"<a class=""navigation-link"" href=""#chart"">Go to plot</a>")
        .Append($@"<a class=""navigation-link"" href=""#document_start"">Go to top 🡡</a>");
    }

    private async Task<string> GetJavaScriptCodeTextAsync()
    {
      MemoryCache cache = MemoryCache.Default;
      if (!(cache.Get(HtmlLogger.GoolgeChartsJavaScriptSourceFileName) is string scriptCode))
      {
        var assembly = Assembly.GetAssembly(GetType());
        string resourceName = assembly.GetManifestResourceNames().FirstOrDefault(name => name.EndsWith(HtmlLogger.GoolgeChartsJavaScriptSourceFileName, StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(resourceName))
        {
          return string.Empty;
        }

        using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
        {
          using (var streamReader = new StreamReader(resourceStream))
          {
            var scriptCodeBuilder = new StringBuilder();
            while (!streamReader.EndOfStream)
            {
              _ = scriptCodeBuilder.Append(await streamReader.ReadLineAsync());
            }

            scriptCode = scriptCodeBuilder.ToString();
            var cachePolicy = new CacheItemPolicy
            {
              SlidingExpiration = this.FileContentCacheExpiration
            };
            cache.Set(HtmlLogger.GoolgeChartsJavaScriptSourceFileName, scriptCode, cachePolicy);
          }
        }
      }

      return scriptCode;
    }
  }
}