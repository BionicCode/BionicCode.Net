namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Concurrent;
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using System.IO;
  using System.Text;
  using System.Text.Encodings.Web;
  using System.Text.Json;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Collections.Generic;
  using Microsoft.Extensions.Caching.Memory;
  using System.Web;

  internal class HtmlLogger : IProfilerLogger
  {
    private const string JavaScriptSourceFileName = @"HtmlLogger.js";
    private const string HtmlSourceFileName = @"HtmlLogger.html";
    private const int DoublePrecision = 3;
    private const double GraphIntervalResolution = 0.01;
    private readonly MemoryCache fileContentCache;
    private readonly TimeSpan FileContentCacheExpiration = TimeSpan.FromMinutes(5);

    public HtmlLogger()
    {
      var cacheOptions = new MemoryCacheOptions()
      {
        // Item based
        SizeLimit = 100,
      };
      this.fileContentCache = new MemoryCache(cacheOptions);
    }

    //public async Task LogAsync(ProfilerBatchResult batchResult, Types profiledType)
    //  => await LogAsync(new ProfilerBatchResultGroupCollection(new[] { new ProfilerBatchResultGroup(batchResult.Context.TargetType, new[] { batchResult }) }, profiledType));

    //public async Task LogAsync(ProfilerBatchResultGroup batchResultGroup, Types profiledType)
    //  => await LogAsync(new ProfilerBatchResultGroupCollection(new[] { batchResultGroup }, profiledType));
    //
    //public async Task LogAsync(ProfilerBatchResultGroupCollection batchResultGroups)
    //  => await LogAsync(new ProfiledTypeResultCollection(new[] { batchResultGroups }));

    public async Task LogAsync(ProfiledTypeResultCollection typeResults, CancellationToken cancellationToken)
    {
      StringBuilder htmlTypeNavigationIndexBuilder = StringBuilderFactory.GetOrCreate();
      var documentBuilderInfoMap = new Dictionary<ProfilerBatchResultGroupCollection, IEnumerable<HtmlDocumentBuilderInfo>>();
      foreach (ProfilerBatchResultGroupCollection batchResultGroups in typeResults)
      {
        cancellationToken.ThrowIfCancellationRequested();

        if (!batchResultGroups.Any())
        {
          continue;
        }

        IEnumerable<HtmlDocumentBuilderInfo> htmlDocumentBuilderInfos = await CreateHtmlDocumentsAsync(batchResultGroups, cancellationToken);
        if (htmlDocumentBuilderInfos.IsEmpty())
        {
          continue;
        }

        documentBuilderInfoMap.Add(batchResultGroups, htmlDocumentBuilderInfos);
        string indexPageNameOfCurrentType = htmlDocumentBuilderInfos.First().FileName;
        _ = htmlTypeNavigationIndexBuilder.AppendLine($@"<li><a class=""dropdown-item {{0}}"" {{1}} href=""{indexPageNameOfCurrentType}"">{batchResultGroups.ProfiledTypeData.ShortCompactSignature.ToHtmlEncodedString()}</a></li>");
      }

      string htmlTypeNavigationIndexTemplate = htmlTypeNavigationIndexBuilder.ToString();
      var htmlFilePaths = new List<string>();
      for (int typeResultGroupsIndex = 0; typeResultGroupsIndex < typeResults.Count; typeResultGroupsIndex++)
      {
        cancellationToken.ThrowIfCancellationRequested();

        ProfilerBatchResultGroupCollection batchResultGroups = typeResults[typeResultGroupsIndex];
        if (!documentBuilderInfoMap.TryGetValue(batchResultGroups, out IEnumerable<HtmlDocumentBuilderInfo> documentBuilderInfos))
        {
          continue;
        }

        _ = htmlTypeNavigationIndexBuilder.Clear();
        string globalNavigationIndexForCurrentType = await CreateGlobalNavigationIndexAsync(htmlTypeNavigationIndexBuilder, typeResults.Count, typeResultGroupsIndex, htmlTypeNavigationIndexTemplate, cancellationToken);

        foreach (HtmlDocumentBuilderInfo htmlDocumentBuilderInfo in documentBuilderInfos)
        {
          string htmlFilePath = Path.Combine(Path.GetTempPath(), htmlDocumentBuilderInfo.FileName);
          htmlFilePaths.Add(htmlFilePath);

          string encodedCurrentProfiledTypeSignature = $"{batchResultGroups.ProfiledTypeData.ShortDisplayName.ToHtmlEncodedString()} ({batchResultGroups.ProfiledTypeData.SymbolAttributes.ToDisplayTypeKind()})";
          string htmlDocument = string.Format(
            htmlDocumentBuilderInfo.DocumentTemplate,
            encodedCurrentProfiledTypeSignature,
            globalNavigationIndexForCurrentType,
            htmlDocumentBuilderInfo.MemberName,
            htmlDocumentBuilderInfo.ResultNavigationElements,
            htmlDocumentBuilderInfo.InPageNavigationElements,
            htmlDocumentBuilderInfo.DocumentTitle,
            htmlDocumentBuilderInfo.ChartSection,
            htmlDocumentBuilderInfo.ScriptCode,
            htmlDocumentBuilderInfo.DocumentFooterElements,
            htmlFilePath,
            htmlDocumentBuilderInfo.TargetSignature,
            htmlDocumentBuilderInfo.TargetNamespace,
            htmlDocumentBuilderInfo.TargetAssemblyName,
            htmlDocumentBuilderInfo.TargetSourceFileName,
            htmlDocumentBuilderInfo.TargetSourceFileLineNumber,
            htmlDocumentBuilderInfo.EnvironmentInfo);

#if NETCOREAPP || NET
          await using var streamWriter = new StreamWriter(htmlFilePath, false);
          await streamWriter.WriteAsync(htmlDocument);
#else
          using (var streamWriter = new StreamWriter(htmlFilePath, false))
          {
            await streamWriter.WriteAsync(htmlDocument);
          }
#endif
        }
      }

      if (htmlFilePaths.IsEmpty())
      {
        return;
      }

      string indexFilePath = htmlFilePaths.First();
      var startInfo = new ProcessStartInfo(indexFilePath) { UseShellExecute = true };
      _ = Process.Start(startInfo);

      StringBuilderFactory.Recycle(htmlTypeNavigationIndexBuilder);
    }

    private async Task<IEnumerable<HtmlDocumentBuilderInfo>> CreateHtmlDocumentsAsync(ProfilerBatchResultGroupCollection batchResultGroups, CancellationToken cancellationToken)
    {
      var filePaths = new List<string>();
      var runningTasks = new List<Task<ChartTableCollection>>();
      var htmlDocumentBuilderValues = new Dictionary<ProfilerBatchResultGroup, HtmlDocumentBuilderInfo>();
      StringBuilder htmlTypeMemberNavigationIndexBuilder = StringBuilderFactory.GetOrCreate();
      var chartDataConverter = new GoogleChartsDataConverter();

      string scriptCode = await GetEncodedJavaScriptCodeTextAsync();

      foreach (ProfilerBatchResultGroup batchResultGroup in batchResultGroups)
      {
        cancellationToken.ThrowIfCancellationRequested();

        var task = Task.Run(() => chartDataConverter.CreateChartAsync(batchResultGroup, HtmlLogger.GraphIntervalResolution, cancellationToken), cancellationToken);
        runningTasks.Add(task);
        string resultHtmlTable = await CreateHtmlTableAsync(batchResultGroup, cancellationToken);

        DateTime timeStamp = DateTime.Now;
        string htmlFileName = $"profiler_result_{timeStamp.ToString("MM-dd-yyyy_hhmmss.fffffff")}.html";
        _ = htmlTypeMemberNavigationIndexBuilder.AppendLine($@"<li><a class=""dropdown-item {{0}}"" style=""white-space: pre-wrap; "" {{1}} href=""{htmlFileName}"">{batchResultGroup.TargetShortCompactSignature.ToHtmlEncodedString()}</a></li>");
        string htmlSourceCodeTemplate = await GetEncodedHtmlCodeTextAsync();
        string pageTitle = $"{batchResultGroup.TargetName.ToWrappingHtml(WrapStyle.Casing, '.', '<', '>', ':', '(', '[')} {batchResultGroup.TargetType.ToDisplayStringValue(toUpperCase: true, toBaseType: true)}";
        string inPageNavigationHtmlElements = CreateHtmlInPageNavigationElements(batchResultGroup);
        //string pageFooterElements = CreateHtmlInPageFooterElements(batchResultGroup);

        EnvironmentInfo environmentInfo = await Environment.GetEnvironmentInfoAsync();
        var builderInfo = new HtmlDocumentBuilderInfo()
        {
          ChartSection = resultHtmlTable,
          DocumentTemplate = htmlSourceCodeTemplate,
          DocumentTitle = pageTitle,
          TargetSignature = batchResultGroup.TargetSignatureComponentInfo.ToHtml(),
          TargetNamespace = batchResultGroup.TargetNamespace,
          TargetAssemblyName = batchResultGroup.TargetAssemblyName,
          TargetSourceFileName = batchResultGroup.TargetSourceFileName,
          TargetSourceFileLineNumber = batchResultGroup.TargetSourceFileLineNumber,
          InPageNavigationElements = inPageNavigationHtmlElements,
          //DocumentFooterElements = pageFooterElements,
          FileName = htmlFileName,
          MemberName = $"{batchResultGroup.TargetShortName} ({batchResultGroup.TargetType.ToDisplayStringValue(toBaseType: true)})".ToHtmlEncodedString(),
          EnvironmentInfo = $"<div style=\"height: 100%; width: auto; border-left: 1px solid black; padding: 12px 0px 12px 12px;\">\r\n           <span class=\"label-span\">Computer: </span><span class=\"valueSpan\">{environmentInfo.MachineName}</span><br> <span class=\"label-span\">Timer: </span><span class=\"valueSpan\">{(environmentInfo.HasHighPrecisionTimer ? $"High precision counter" : "System timer (normal precision)")}</span><br>     \t\r\n          <span class=\"label-span\">Timer resolution: </span><span class=\"valueSpan\">{environmentInfo.NanosecondsPerTick} ns</span><br>     \t\r\n        \t<span class=\"label-span\">OS version: </span><span class=\"valueSpan\">{environmentInfo.OperatingSystemName}</span><br>   \t\r\n        \t<span class=\"label-span\">OS architecture: </span><span class=\"valueSpan\">{environmentInfo.OperatingSystemArchitecture}</span><br>   \r\n        \t   \r\n      \t  <span class=\"label-span\">Processor: </span><span class=\"valueSpan\">{environmentInfo.ProcessorName}</span><br>   \r\n        \t<span class=\"label-span\">Clock: </span><span class=\"valueSpan\">{environmentInfo.ProcessorSpeed / 1000d} GHz</span><br> \r\n      \t  <span class=\"label-span\">Physical cores: </span><span class=\"valueSpan\">{environmentInfo.ProcessorCoreCount}</span><br>   \r\n      \t  <span class=\"label-span\">Logical cores: </span><span class=\"valueSpan\">{environmentInfo.ProcessorLogicalCoreCount}</span><br>   \r\n        \t<span class=\"label-span\">Threads: </span><span class=\"valueSpan\">{environmentInfo.ThreadCount}</span><br>   \r\n        </div>"
        };

        htmlDocumentBuilderValues.Add(batchResultGroup, builderInfo);
      }

      ChartTableCollection[] scriptChartData = await Task.WhenAll(runningTasks);

      string htmlNavigationIndexTemplate = htmlTypeMemberNavigationIndexBuilder.ToString();

      for (int groupIndex = 0; groupIndex < batchResultGroups.Count; groupIndex++)
      {
        cancellationToken.ThrowIfCancellationRequested();

        ProfilerBatchResultGroup resultGroup = batchResultGroups[groupIndex];

        _ = htmlTypeMemberNavigationIndexBuilder.Clear();
        string globalNavigationIndexForCurrentResult = await CreateGlobalNavigationIndexAsync(htmlTypeMemberNavigationIndexBuilder, batchResultGroups.Count, groupIndex, htmlNavigationIndexTemplate, cancellationToken);
        HtmlDocumentBuilderInfo htmlDocumentBuilderInfo = htmlDocumentBuilderValues[resultGroup];
        htmlDocumentBuilderInfo.ResultNavigationElements = globalNavigationIndexForCurrentResult;

        ChartTableCollection chartTables = scriptChartData[groupIndex];
        string jsonDataValues = await ConvertToJsonAsync(chartTables);
        string finalScriptCode = string.Format(scriptCode, jsonDataValues);
        htmlDocumentBuilderInfo.ScriptCode = finalScriptCode;
      }

      StringBuilderFactory.Recycle(htmlTypeMemberNavigationIndexBuilder);

      return htmlDocumentBuilderValues.Values.ToList();
    }

    private async Task<string> CreateGlobalNavigationIndexAsync(StringBuilder htmlDocumentNavigationIndexBuilder, int totalResultCount, int currentResultIndex, string htmlTypeNavigationIndexTemplate, CancellationToken cancellationToken)
    {
      int lineIndex = 0;

      using (var templateReader = new StringReader(htmlTypeNavigationIndexTemplate))
      {
        string line = string.Empty;
        string rawLine = string.Empty;
        while (lineIndex++ < currentResultIndex)
        {
          cancellationToken.ThrowIfCancellationRequested();

          rawLine = await templateReader.ReadLineAsync();
          line = StringEncoder.EncodeFormatString(rawLine);
          _ = htmlDocumentNavigationIndexBuilder.AppendFormat(line, string.Empty, string.Empty);
        }

        rawLine = await templateReader.ReadLineAsync();
        line = StringEncoder.EncodeFormatString(rawLine);
        _ = htmlDocumentNavigationIndexBuilder.AppendFormat(line, "active", "aria-current=\"page\"");

        while (lineIndex++ < totalResultCount)
        {
          cancellationToken.ThrowIfCancellationRequested();

          rawLine = await templateReader.ReadLineAsync();
          line = StringEncoder.EncodeFormatString(rawLine);
          _ = htmlDocumentNavigationIndexBuilder.AppendFormat(line, string.Empty, string.Empty);
        }
      }

      return htmlDocumentNavigationIndexBuilder.ToString();
    }

    private string CreateHtmlInPageNavigationElements(ProfilerBatchResultGroup profilerBatchResultGroup)
    {
      if (profilerBatchResultGroup.Count < 2)
      {
        return string.Empty;
      }

      StringBuilder htmlDocumentBuilder = StringBuilderFactory.GetOrCreate();
      foreach (ProfilerBatchResult result in profilerBatchResultGroup)
      {
        _ = htmlDocumentBuilder
          .Append($@"<a class=""list-group-item list-group-item-action nav-link"" width=""20px"" href=""#{result.Index}"">'{result.Context.MethodInvokeInfo.ShortDisplayName.ToHtmlEncodedString()}' ({result.Context.MethodInvokeInfo.ProfiledTargetType.ToDisplayStringValue()})</a>");
      }

      string htmlDocumentContent = htmlDocumentBuilder.ToString();
      StringBuilderFactory.Recycle(htmlDocumentBuilder);

      return htmlDocumentContent;
    }

    private string CreateHtmlInPageFooterElements(ProfilerBatchResultGroup profilerBatchResultGroup)
    {
      StringBuilder htmlDocumentBuilder = StringBuilderFactory.GetOrCreate();
      foreach (ProfilerBatchResult result in profilerBatchResultGroup)
      {
        _ = htmlDocumentBuilder
          .Append($@"<a class=""list-group-item list-group-item-action nav-link"" href=""#{result.Index}"">'{result.Context.MethodInvokeInfo.ShortDisplayName.ToHtmlEncodedString()}' ({result.Context.MethodInvokeInfo.ProfiledTargetType.ToDisplayStringValue()}) results</a>");
      }

      string htmlDocumentContent = htmlDocumentBuilder.ToString();
      StringBuilderFactory.Recycle(htmlDocumentBuilder);

      return htmlDocumentContent;
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

    private async Task<string> CreateHtmlTableAsync(ProfilerBatchResultGroup batchResultGroup, CancellationToken cancellationToken)
    {
      StringBuilder htmlDocumentBuilder = StringBuilderFactory.GetOrCreate();
      EnvironmentInfo environmentInfo = await Environment.GetEnvironmentInfoAsync();

      foreach (ProfilerBatchResult batchResult in batchResultGroup)
      {
        cancellationToken.ThrowIfCancellationRequested();

        _ = htmlDocumentBuilder.Append($@"
    <article id=""{batchResult.Index}"" style=""padding-top: 48px;"">
      <div style=""margin: 12px 0px 24px 0px; width: 100%; display: grid; grid-template-columns: auto auto; overflow: auto;"">
        

        <div style=""height: 100%; width: auto; border-left: 1px solid black; padding: 12px 12px 12px 12px;"">
          
          <span style=""font-weight: bold; font-size: 14pt"">Target Kind</span><br>
      	  <span class=""valueSpan"">{batchResult.Context.MethodInvokeInfo.ProfiledTargetType.ToDisplayStringValue(toUpperCase: true).ToHtmlEncodedString()}</span><br><br>
          <span style=""font-weight: bold; font-size: 14pt"">Conditions</span><br>
      	  <span class=""label-span"">Timestamp: </span><span class=""valueSpan"">{batchResult.TimeStamp}</span><br> 
      	  <span class=""label-span"">Target framework: </span><span class=""valueSpan"">{environmentInfo.RuntimeVersion}</span><br> 	
          <span class=""label-span"">Process architecture: </span><span class=""valueSpan"">{environmentInfo.ProcessArchitecture}</span><br>
          <span class=""label-span"">Base unit: </span><span class=""valueSpan"">{batchResult.BaseUnit.ToDisplayStringValue()}</span><br>
      	  <span class=""label-span"">Warmup iterations: </span><span class=""valueSpan"">{batchResult.Context.WarmupCount} runs for each argument list</span><br>
      	  <span class=""label-span"">Iterations: </span><span class=""valueSpan"">{batchResult.IterationCount} runs for each argument list</span><br>
      	  <span class=""label-span"">Argument lists: </span><span class=""valueSpan"">{batchResult.ArgumentListCount}</span><br>
      	  <span class=""label-span"">Total iterations: </span><span class=""valueSpan"">{batchResult.TotalIterationCount} ({batchResult.IterationCount} runs for each of {batchResult.ArgumentListCount} argument {(batchResult.ArgumentListCount == 1 ? "list" : "lists")}) </span><br>
        </div>        
        </div>
      </div>

      <div>
        <div class=""table-scroll-host"">
          <table id=""result-table-{batchResult.Index}"">
            <thead>
              <tr>
                <th class=""header"">Iteration #</th>
                <th class=""header"">Elapsed time [{batchResult.BaseUnit.ToDisplayStringValue()}]</th>
                <th class=""header"">Mean µ [{batchResult.BaseUnit.ToDisplayStringValue()}]</th>
                <th class=""header"">Deviation [{batchResult.BaseUnit.ToDisplayStringValue()}]</th>
                <th class=""header"">Standard deviation σ [{batchResult.BaseUnit.ToDisplayStringValue()}]</th>
                <th class=""header"">Variance σ²</th>
              </tr>
            </thead>
            <tbody>");

        int resultIndex = 0;
        foreach (ProfilerResult result in batchResult.Results)
        {
          _ = htmlDocumentBuilder.Append($@"
              <tr class=""data-row"">
                <td class=""row-data"">{++resultIndex} (argument list {result.ArgumentListIndex})</td>
                <td class=""row-data"">{result.ElapsedTimeConverted}</td>
                <td class=""row-data"">{batchResult.AverageDurationConverted}</td>
                <td class=""row-data"">{result.DeviationConverted}</td>
                <td class=""row-data"">{batchResult.StandardDeviationConverted}</td>
                <td class=""row-data"">{batchResult.Variance}</td>
              </tr>");
        }

        _ = htmlDocumentBuilder.Append($@"
            </tbody>
            <tfoot>
              <tr>
                <th class=""data-row-summary"" style=""text-align: left;"" colspan=""10"">Summary</th>
              </tr>
              <tr>
                <td colspan=""3"" class=""data-row-summary"">Total runs: {batchResult.TotalIterationCount}</td>
                <td colspan=""3"" class=""data-row-summary"">Total time elapsed: {batchResult.TotalDurationConverted} {batchResult.BaseUnit.ToDisplayStringValue()}</td>
              </tr>
              <tr>
                <td colspan=""3"" class=""data-row-summary"">Min (fastest): {batchResult.MinResult.ElapsedTimeConverted} {batchResult.BaseUnit.ToDisplayStringValue()} (#{batchResult.MinResult.Iteration})</td>
                <td colspan=""3"" class=""data-row-summary"">Max (slowest): {batchResult.MaxResult.ElapsedTimeConverted} {batchResult.BaseUnit.ToDisplayStringValue()} (#{batchResult.MaxResult.Iteration})</td>
              </tr>
              <tr>
                <td colspan=""3"" class=""data-row-summary"">Mean: {batchResult.AverageDurationConverted} {batchResult.BaseUnit.ToDisplayStringValue()}</td>
                <td colspan=""3"" class=""data-row-summary"">Range: {batchResult.RangeConverted} {batchResult.BaseUnit.ToDisplayStringValue()}</td>
              </tr>
              <tr>
                <td colspan=""3"" class=""data-row-summary"">Standard deviation: {batchResult.StandardDeviationConverted} {batchResult.BaseUnit.ToDisplayStringValue()}</td>
                <td colspan=""3"" class=""data-row-summary"">Variance: {batchResult.Variance}</td>
              </tr>
            </tfoot>
          </table>
        </div>

        <div style=""width:50%; float: left;"">
          <div id=""chart-{batchResult.Index}"" class=""line-chart""></div>
        </div>")
        .Append($@"<a class=""navigation-link"" href=""#document_start"">Go to top 🡡</a>")
        .Append("</article>");
      }

      string htmlDocumentContent = htmlDocumentBuilder.ToString();
      StringBuilderFactory.Recycle(htmlDocumentBuilder);

      return htmlDocumentContent;
    }

    private async Task<string> GetEncodedJavaScriptCodeTextAsync()
    {
      if (!(this.fileContentCache.Get(HtmlLogger.JavaScriptSourceFileName) is string scriptCode))
      {
        var assembly = Assembly.GetAssembly(GetType());
        string resourceName = assembly.GetManifestResourceNames().FirstOrDefault(name => name.EndsWith(HtmlLogger.JavaScriptSourceFileName, StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(resourceName))
        {
          return string.Empty;
        }

        using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
        {
          using (var streamReader = new StreamReader(resourceStream))
          {
            string rawScriptCode = await streamReader.ReadToEndAsync();
            scriptCode = StringEncoder.EncodeFormatString(rawScriptCode);
            ICacheEntry entry = this.fileContentCache.CreateEntry(HtmlLogger.JavaScriptSourceFileName)
              .SetValue(scriptCode)
              .SetSize(1)
              .SetSlidingExpiration(this.FileContentCacheExpiration);
          }
        }
      }

      return scriptCode;
    }

    private async Task<string> GetEncodedHtmlCodeTextAsync()
    {
      if (!(this.fileContentCache.Get(HtmlLogger.HtmlSourceFileName) is string htmlCode))
      {
        var assembly = Assembly.GetAssembly(GetType());
        string resourceName = assembly.GetManifestResourceNames()
          .FirstOrDefault(name => name.EndsWith(HtmlLogger.HtmlSourceFileName, StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(resourceName))
        {
          return string.Empty;
        }

        using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
        {
          using (var streamReader = new StreamReader(resourceStream))
          {
            string rawHtmlCode = await streamReader.ReadToEndAsync();
            htmlCode = StringEncoder.EncodeFormatString(rawHtmlCode);
            ICacheEntry entry = this.fileContentCache.CreateEntry(HtmlLogger.HtmlSourceFileName)
              .SetValue(htmlCode)
              .SetSize(1)
              .SetSlidingExpiration(this.FileContentCacheExpiration);
          }
        }
      }

      return htmlCode;
    }
  }
}