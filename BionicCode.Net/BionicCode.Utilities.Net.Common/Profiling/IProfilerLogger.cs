namespace BionicCode.Utilities.Net
{
  using System;
  using System.CodeDom;
  using System.Collections.Generic;
  using System.Text;
  using System.Threading.Tasks;
  using System.Text.Json;
  using System.IO;
  using System.Reflection;
  using System.Runtime.Caching;
  using System.Diagnostics;
  using System.Linq;
  using System.Drawing;
  using static BionicCode.Utilities.Net.HtmlLogger;
  using System.Collections.Concurrent;
  using System.Collections;
  using System.Text.Json.Serialization;

  internal interface IProfilerLogger
  {
    Task LogAsync(IEnumerable<ProfilerBatchResult> batchResults, string message);
  }

  internal partial class HtmlLogger : IProfilerLogger
  {
    private const string GoolgeChartsJavaScriptSourceFileName = @"Plotter.js";
    private readonly TimeSpan FileContentCacheExpiration = TimeSpan.FromMinutes(5);

    public async Task LogAsync(IEnumerable<ProfilerBatchResult> batchResults, string message)
    {
      if (!batchResults.Any())
      {
        return;
      }

      var htmlDocumentBuilder = new StringBuilder();
      string htmlDocument = await CreateHtmlDocumentAsync(batchResults, htmlDocumentBuilder);
      string htmlFileName = $"profiler_result_{batchResults.First().TimeStamp.ToString("MM-dd-yyyy_hhmmss.fffffff")}.html";
      string fulFileName = Path.Combine(Path.GetTempPath(), htmlFileName);

      using (var streamWriter = new StreamWriter(fulFileName, false))
      {
        await streamWriter.WriteAsync(htmlDocument);
      }

      var startInfo = new ProcessStartInfo(fulFileName) { UseShellExecute = true };
      _ = Process.Start(startInfo);
    }

    private async Task<string> CreateHtmlDocumentAsync(IEnumerable<ProfilerBatchResult> batchResults, StringBuilder htmlDocumentBuilder)
    {
      CreateDocumentHead(batchResults, htmlDocumentBuilder);
      string scriptCode = await GetJavaScriptCodeTextAsync();
      string finalScriptCode = scriptCode;
      ConcurrentBag<NormalDistributionData<CartesianPoint>> normalDistributionValuesCollection = new ConcurrentBag<NormalDistributionData<CartesianPoint>>();
      foreach (ProfilerBatchResult batchResult in batchResults)
      {
        await CreateHtmlTableAsync(batchResult, htmlDocumentBuilder);
      }

      _ = Parallel.ForEach(batchResults, new ParallelOptions() { MaxDegreeOfParallelism = -1 }, async (batchResult) =>
      {

#if NET7_0_OR_GREATER
        IEnumerable<CartesianPoint> normalDistributionValues = Math.NormDist(batchResult.AverageDurationInMicroseconds, batchResult.StandardDeviationInMicroseconds, 0.01);
#else
        IEnumerable<CartesianPoint> normalDistributionValues = Math.NormDist(batchResult.AverageDuration.TotalMicroseconds(), batchResult.StandardDeviationInMicroseconds, 0.1);
#endif
        var result = new NormalDistributionData<CartesianPoint>(batchResult.Index, normalDistributionValues);
        normalDistributionValuesCollection.Add(result);
      });

      IEnumerable<IEnumerable<CartesianPoint>> res = normalDistributionValuesCollection
        .OrderBy(data => data.Index)
        .Select(data => data.Values);
        string jsonDataValues = await ConvertToJsonAsync(normalDistributionValuesCollection);

        finalScriptCode = string.Format(scriptCode, jsonDataValues);
      InlineScriptIntoHtmlDocument(htmlDocumentBuilder, finalScriptCode);

      return htmlDocumentBuilder.ToString();
    }

    private static void InlineScriptIntoHtmlDocument(StringBuilder htmlDocumentBuilder, string finalScriptCode) => _ = htmlDocumentBuilder.Append($@"
<div id=""chart"" style=""width:100%;  height:800px;""></div>
<script>{finalScriptCode}</script>
</body>
</html>");
    private static void CreateDocumentHead(IEnumerable<ProfilerBatchResult> batchResults, StringBuilder htmlDocumentBuilder)
    {
      _ = htmlDocumentBuilder.Append($@"<!DOCTYPE html>
<html>
<script src=""https://www.gstatic.com/charts/loader.js""></script>
<style>
tr:nth-child(even) {{
  background-color: #8fb2e6;
}}

.header {{
  background-color: #8d9cb26;
}}

.dataRow {{
  padding: 4px 8px 4px 8px;
}}

.labelSpan {{
  font-style: italic;
}}
</style>
<head>
<title>Page Title</title>
</head>
<body>
<div id=""document_start""></div>
<div id=""json_data""></div>");
      
      foreach (ProfilerBatchResult batchResult in  batchResults)
      {
        _ = htmlDocumentBuilder.Append($@"<a href=""#{batchResult.Index}"" style=""margin-right: 12px;"">{batchResult.Context.TargetType.ToDisplayStringValue()} method results</a>");
      }
    }

    private static async Task<string> ConvertToJsonAsync(IEnumerable normalDistributionValues)
    {
      using (var memStream = new MemoryStream())
      {
        await JsonSerializer.SerializeAsync(memStream, normalDistributionValues);
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
      EnvironmentInfo environmentInfo = await Environment.GetEnvironmentInfoAsync();
      _ = htmlDocumentBuilder.Append($@"<table id=""{batchResult.Index}"">
<thead>
<tr>
<th colspan=""10"" style=""font-weight: normal; text-align: left; border: 1px solid black; padding: 12px"">
  <span style=""font-weight: bold; font-size: 14pt"">Context</span><br/>
  	<span class=""labelSpan"">Timestamp: </span><span class=""valueSpan"">{batchResult.TimeStamp}</span><br />
  	<span class=""labelSpan"">Target framework: </span><span class=""valueSpan"">{batchResult.Context.RuntimeVersion}</span><br />
  	<span class=""labelSpan"">Target type: </span><span class=""valueSpan"">{batchResult.Context.TargetType.ToDisplayStringValue()}</span><br />
  	<span class=""labelSpan"">Target: </span><span class=""valueSpan"">{batchResult.Context.TargetName}</span><br />
  	<span class=""labelSpan"">Assembly: </span><span class=""valueSpan"">{batchResult.Context.AssemblyName}</span><br />
  	<span class=""labelSpan"">Source file: </span><span class=""valueSpan"">{batchResult.Context.SourceFileName}</span><br /> 
  	<span class=""labelSpan"">Line number: </span><span class=""valueSpan"">{batchResult.Context.LineNumber}</span><br />   
  	<span class=""labelSpan"">Processor: </span><span class=""valueSpan"">{environmentInfo.ProcessorName}</span><br />   
  	<span class=""labelSpan"">Processor physical cores: </span><span class=""valueSpan"">{environmentInfo.ProcessorCoreCount}</span><br />   
  	<span class=""labelSpan"">Processor logical cores: </span><span class=""valueSpan"">{environmentInfo.ProcessorLogicalCoreCount}</span><br />   
  	<span class=""labelSpan"">Processor threads: </span><span class=""valueSpan"">{environmentInfo.ThradCount}</span><br />   
  	<span class=""labelSpan"">Processor clock: </span><span class=""valueSpan"">{environmentInfo.ProcessorSpeed / 1000d} GHz</span><br />   
  	<span class=""labelSpan"">OS architecture: </span><span class=""valueSpan"">{(environmentInfo.Is64BitOperatingSystem ? 64 : 32)} bit</span><br />   
  	<span class=""labelSpan"">Process architecture: </span><span class=""valueSpan"">{(environmentInfo.Is64BitProcess ? 64 : 32)} bit</span><br />   
  </th>
</tr>
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
      foreach (ProfilerResult result in batchResult.Results)
      {
        _ = htmlDocumentBuilder.Append($@"<tr>
<td class=""dataRow"">{result.Iteration}</td>
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
<td colspan=""10"" style=""font-weight: bold;"">Totals:</td>
</tr>
<tr>
<td class=""dataRow"">{batchResult.IterationCount}</td>
<td class=""dataRow"">{batchResult.TotalDurationInMicroseconds}</td>
<td class=""dataRow"">{batchResult.AverageDurationInMicroseconds}</td>
<td class=""dataRow"">-</td>
<td class=""dataRow"">{batchResult.StandardDeviationInMicroseconds}</td>
<td class=""dataRow"">{batchResult.Variance}</td>
<td class=""dataRow"">Min (fastest): {batchResult.MinResult.ElapsedTimeInMicroseconds} µs (#{batchResult.MinResult.Iteration})</td>
<td class=""dataRow"">Max (slowest): {batchResult.MaxResult.ElapsedTimeInMicroseconds} µs (#{batchResult.MaxResult.Iteration})</td>
</tr>
</tfoot>
</table>")
        .Append($@"<a href=""#document_start"">Go to top</a>"); ;      
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