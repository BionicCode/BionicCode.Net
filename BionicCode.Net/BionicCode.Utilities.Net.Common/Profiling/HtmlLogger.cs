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
  using System.Collections.Specialized;
  using System.Text.RegularExpressions;

  internal class HtmlLogger : IProfilerLogger
  {
    private const string JavaScriptSourceFileName = @"HtmlLogger.js";
    private const string HtmlSourceFileName = @"HtmlLogger.html";
    private const int DoublePrecision = 3;
    private const double GraphIntervalResolution = 0.01;
    private readonly TimeSpan FileContentCacheExpiration = TimeSpan.FromMinutes(5);
    internal static HtmlEncoder HtmlEncoder { get; } = HtmlEncoder.Create();

    //public async Task LogAsync(ProfilerBatchResult batchResult, Type profiledType)
    //  => await LogAsync(new ProfilerBatchResultGroupCollection(new[] { new ProfilerBatchResultGroup(batchResult.Context.TargetType, new[] { batchResult }) }, profiledType));

    //public async Task LogAsync(ProfilerBatchResultGroup batchResultGroup, Type profiledType)
    //  => await LogAsync(new ProfilerBatchResultGroupCollection(new[] { batchResultGroup }, profiledType));
    //
    //public async Task LogAsync(ProfilerBatchResultGroupCollection batchResultGroups)
    //  => await LogAsync(new ProfiledTypeResultCollection(new[] { batchResultGroups }));

    public async Task LogAsync(ProfiledTypeResultCollection typeResults)
    {
      var htmlTypeNavigationIndexBuilder = new StringBuilder();
      var documentBuilderInfosMap = new Dictionary<ProfilerBatchResultGroupCollection, IEnumerable<HtmlDocumentBuilderInfo>>();
      foreach (ProfilerBatchResultGroupCollection batchResultGroups in typeResults)
      {
        if (!batchResultGroups.Any())
        {
          continue;
        }

        IEnumerable<HtmlDocumentBuilderInfo> htmlDocumentBuilderInfos = await CreateHtmlDocumentsAsync(batchResultGroups);
        if (htmlDocumentBuilderInfos.IsEmpty())
        {
          continue;
        }

        documentBuilderInfosMap.Add(batchResultGroups, htmlDocumentBuilderInfos);
        string indexPageNameOfCurrentType = htmlDocumentBuilderInfos.First().FileName;
        _ = htmlTypeNavigationIndexBuilder.AppendLine($@"<li><a class=""dropdown-item {{0}}"" {{1}} href=""{indexPageNameOfCurrentType}"">{HtmlLogger.HtmlEncoder.Encode(batchResultGroups.ProfiledType.ToDisplaySignatureName())}</a></li>");
      }

      string htmlTypeNavigationIndexTemplate = htmlTypeNavigationIndexBuilder.ToString();
      var htmlFilePaths = new List<string>();
      for (int typeResultGroupsIndex = 0; typeResultGroupsIndex < typeResults.Count; typeResultGroupsIndex++)
      {
        ProfilerBatchResultGroupCollection batchResultGroups = typeResults[typeResultGroupsIndex];
        if (!documentBuilderInfosMap.TryGetValue(batchResultGroups, out IEnumerable<HtmlDocumentBuilderInfo> documentBuilderInfos))
        {
          continue;
        }

        _ = htmlTypeNavigationIndexBuilder.Clear();
        string globalNavigationIndexForCurrentType = await CreateGlobalNavigationIndexAsync(htmlTypeNavigationIndexBuilder, typeResults.Count, typeResultGroupsIndex, htmlTypeNavigationIndexTemplate);

        foreach (HtmlDocumentBuilderInfo htmlDocumentBuilderInfo in documentBuilderInfos)
        {
          string htmlFilePath = Path.Combine(Path.GetTempPath(), htmlDocumentBuilderInfo.FileName);
          htmlFilePaths.Add(htmlFilePath);

          string encodedCurrentProfiledTypeSignature = HtmlLogger.HtmlEncoder.Encode(batchResultGroups.ProfiledType.ToDisplaySignatureName());
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
            htmlFilePath);

          using (var streamWriter = new StreamWriter(htmlFilePath, false))
          {
            await streamWriter.WriteAsync(htmlDocument);
          }
        }
      }

      if (htmlFilePaths.IsEmpty())
      {
        return;
      }

      string indexFilePath = htmlFilePaths.First();
      var startInfo = new ProcessStartInfo(indexFilePath) { UseShellExecute = true };
      _ = Process.Start(startInfo);
    }    

    private async Task<IEnumerable<HtmlDocumentBuilderInfo>> CreateHtmlDocumentsAsync(ProfilerBatchResultGroupCollection batchResultGroups)
    {
      var filePaths = new List<string>();
      var runningTasks = new List<Task<ChartTableCollection>>();
      var htmlDocumentBuilderValues = new Dictionary<ProfilerBatchResultGroup, HtmlDocumentBuilderInfo>();
      var htmlTypeMemberNavigationIndexBuilder = new StringBuilder();
      var chartDataConverter = new GoogleChartsDataConverter();

      string scriptCode = await GetEncodedJavaScriptCodeTextAsync();

      foreach (ProfilerBatchResultGroup batchResultGroup in batchResultGroups)
      {
        Task<ChartTableCollection> task = Task.Run(() => chartDataConverter.CreateChartAsync(batchResultGroup, HtmlLogger.GraphIntervalResolution));
        runningTasks.Add(task);
        string resultHtmlTable = await CreateHtmlTableAsync(batchResultGroup);

        DateTime timeStamp = DateTime.Now;
        string htmlFileName = $"profiler_result_{timeStamp.ToString("MM-dd-yyyy_hhmmss.fffffff")}.html";
        _ = htmlTypeMemberNavigationIndexBuilder.AppendLine($@"<li><a class=""dropdown-item {{0}}"" {{1}} href=""{htmlFileName}"">{HtmlLogger.HtmlEncoder.Encode(batchResultGroup.ProfiledTargetSignatureMemberName)}</a></li>");
        string htmlSourceCodeTemplate = await GetEncodedHtmlCodeTextAsync();
        string pageTitel = HtmlLogger.HtmlEncoder.Encode(batchResultGroup.ProfiledTargetSignatureMemberName);
        string inPageNavigationHtmlElements = CreateHtmlInPageNavigationElements(batchResultGroup);
        //string pageFooterElements = CreateHtmlInPageFooterElements(batchResultGroup);

        var builderInfo = new HtmlDocumentBuilderInfo()
        {
          ChartSection = resultHtmlTable,
          DocumentTemplate = htmlSourceCodeTemplate,
          DocumentTitle = pageTitel,
          InPageNavigationElements = inPageNavigationHtmlElements,
          //DocumentFooterElements = pageFooterElements,
          FileName = htmlFileName,
          MemberName = HtmlLogger.HtmlEncoder.Encode($"{batchResultGroup.ProfiledTargetMemberShortName} ({batchResultGroup.ProfiledTargetType.ToDisplayStringValue()})")
        };

        htmlDocumentBuilderValues.Add(batchResultGroup, builderInfo);
      }

      ChartTableCollection[] scriptChartData = await Task.WhenAll(runningTasks);

      string htmlNavigationIndexTemplate = htmlTypeMemberNavigationIndexBuilder.ToString();

      for (int groupIndex = 0; groupIndex < batchResultGroups.Count; groupIndex++)
      {
        ProfilerBatchResultGroup resultGroup = batchResultGroups[groupIndex];

        _ = htmlTypeMemberNavigationIndexBuilder.Clear();
        string globalNavigationIndexForCurrentResult = await CreateGlobalNavigationIndexAsync(htmlTypeMemberNavigationIndexBuilder, batchResultGroups.Count, groupIndex, htmlNavigationIndexTemplate);

        HtmlDocumentBuilderInfo htmlDocumentBuilderInfo = htmlDocumentBuilderValues[resultGroup];
        htmlDocumentBuilderInfo.ResultNavigationElements = globalNavigationIndexForCurrentResult;

        ChartTableCollection chartTables = scriptChartData[groupIndex];
        string jsonDataValues = await ConvertToJsonAsync(chartTables);
        string finalScriptCode = string.Format(scriptCode, jsonDataValues);
        htmlDocumentBuilderInfo.ScriptCode = finalScriptCode;
      }

      return htmlDocumentBuilderValues.Values.ToList();
    }

    private async Task<string> CreateGlobalNavigationIndexAsync(StringBuilder htmlDocumentNavigationIndexBuilder, int totalResultCount, int currentResultIndex, string htmlNavigationIndexTemplate)
    {
      int lineIndex = 0;

      using (var templateReader = new StringReader(htmlNavigationIndexTemplate))
      {
        string line = string.Empty;
        string rawLine = string.Empty;
        while (lineIndex++ < currentResultIndex)
        {
          rawLine = await templateReader.ReadLineAsync();
          line = StringEncoder.EncodeFormatString(rawLine);
          _ = htmlDocumentNavigationIndexBuilder.AppendFormat(line, string.Empty, string.Empty);
        }

        rawLine = await templateReader.ReadLineAsync();
        line = StringEncoder.EncodeFormatString(rawLine);
        _ = htmlDocumentNavigationIndexBuilder.AppendFormat(line, "active", "aria-current=\"page\"");

        while (lineIndex++ < totalResultCount)
        {
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

      var htmlDocumentBuilder = new StringBuilder();
      foreach (ProfilerBatchResult result in profilerBatchResultGroup)
      {
        _ = htmlDocumentBuilder
          .Append($@"<a class=""list-group-item list-group-item-action"" href=""#{result.Index}"">'{HtmlLogger.HtmlEncoder.Encode(result.Context.TargetTypeInfo.ToDisplayName())}' {result.Context.TargetType.ToDisplayStringValue()}</a>");
      }

      return htmlDocumentBuilder.ToString();
    }

    private string CreateHtmlInPageFooterElements(ProfilerBatchResultGroup profilerBatchResultGroup)
    {
      var htmlDocumentBuilder = new StringBuilder();
      foreach (ProfilerBatchResult result in profilerBatchResultGroup)
      {
        _ = htmlDocumentBuilder
          .Append($@"<a class=""list-group-item list-group-item-action"" href=""#{result.Index}"">'{HtmlLogger.HtmlEncoder.Encode(result.Context.TargetTypeInfo.ToDisplayName())}' {result.Context.TargetType.ToDisplayStringValue()}
                            results</a>");
      }

      return htmlDocumentBuilder.ToString();
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

    private async Task<string> CreateHtmlTableAsync(ProfilerBatchResultGroup batchResultGroup)
    {
      TimeUnit timeUnit = batchResultGroup.CommonBaseUnit;
      StringBuilder htmlDocumentBuilder = new StringBuilder();
      EnvironmentInfo environmentInfo = await Environment.GetEnvironmentInfoAsync();

      foreach (ProfilerBatchResult batchResult in batchResultGroup)
      {
        _ = htmlDocumentBuilder.Append($@"
<article id=""{batchResult.Index}"">
  
  <div class=""scroll-host"" style=""font-weight: normal; text-align: left; border: 1px solid black; padding: 12px"">
    <div style=""margin: 0px 0px 12px 0px;"">
      <span style=""font-weight: bold; font-size: 18pt"">Profile Context</span><br/>
      <span style=""font-weight: bold; font-size: 14pt"">Code</span><br/>
  	  <span class=""label-span"">Target framework: </span><span class=""valueSpan"">{batchResult.Context.RuntimeVersion}</span><br />
  	  <span class=""label-span"">Target type: </span><span class=""valueSpan"">{HtmlLogger.HtmlEncoder.Encode(batchResult.Context.TargetType.ToDisplayStringValue())}</span><br />
  	  <span class=""label-span"">Target: </span><span class=""valueSpan"">{HtmlLogger.HtmlEncoder.Encode(batchResult.Context.TargetName)}</span><br />
  	  <span class=""label-span"">Line number: </span><span class=""valueSpan"">{batchResult.Context.LineNumber}</span><br />
  	  <span class=""label-span"">Source file: </span><span class=""valueSpan"">{batchResult.Context.SourceFileName}</span><br /> 
  	  <span class=""label-span"">Assembly: </span><span class=""valueSpan"">{batchResult.Context.AssemblyName}</span><br />
    </div>
    <div style=""margin: 0px 0px 12px 0px;"">
      <div style=""float: left; padding: 0px 12px 0px 0px;"">
        <span style=""font-weight: bold; font-size: 14pt"">Profiler</span><br/>
  	    <span class=""label-span"">Timestamp: </span><span class=""valueSpan"">{batchResult.TimeStamp}</span><br />  	
        <span class=""label-span"">Base unit: </span><span class=""valueSpan"">{timeUnit.ToDisplayStringValue()}</span><br />
  	    <span class=""label-span"">Warmup iterations: </span><span class=""valueSpan"">{batchResult.Context.WarmupCount} runs for each argument list</span><br />
  	    <span class=""label-span"">Iterations: </span><span class=""valueSpan"">{batchResult.IterationCount} runs for each argument list</span><br />
  	    <span class=""label-span"">Argument lists: </span><span class=""valueSpan"">{batchResult.ArgumentListCount}</span><br />
  	    <span class=""label-span"">Total iterations: </span><span class=""valueSpan"">{batchResult.TotalIterationCount} ({batchResult.IterationCount} runs for each of {batchResult.ArgumentListCount} argument {(batchResult.ArgumentListCount == 1 ? "list" : "lists")}) </span><br />
      </div>
      <div style=""float: left; border-left: 1px solid black; padding: 0px 0px 0px 12px;"">
        <span style=""font-weight: bold; font-size: 14pt"">Machine</span><br/>  
  	    <span class=""label-span"">Timer: </span><span class=""valueSpan"">{((await Environment.GetEnvironmentInfoAsync()).HasHighPrecisionTimer ? $"High precision counter" : "System timer (normal precision)")}</span><br />     	
        <span class=""label-span"">Timer resolution: </span><span class=""valueSpan"">{(await Environment.GetEnvironmentInfoAsync()).NanosecondsPerTick} ns</span><br />     	
    	  <span class=""label-span"">OS architecture: </span><span class=""valueSpan"">{(environmentInfo.Is64BitOperatingSystem ? 64 : 32)} bit</span><br />   
    	  <span class=""label-span"">Process architecture: </span><span class=""valueSpan"">{(environmentInfo.Is64BitProcess ? 64 : 32)} bit</span><br />   
  	    <span class=""label-span"">Processor: </span><span class=""valueSpan"">{environmentInfo.ProcessorName}</span><br />   
  	    <span class=""label-span"">Physical cores: </span><span class=""valueSpan"">{environmentInfo.ProcessorCoreCount}</span><br />   
  	    <span class=""label-span"">Logical cores: </span><span class=""valueSpan"">{environmentInfo.ProcessorLogicalCoreCount}</span><br />   
    	  <span class=""label-span"">Threads: </span><span class=""valueSpan"">{environmentInfo.ThradCount}</span><br />   
    	  <span class=""label-span"">Clock: </span><span class=""valueSpan"">{environmentInfo.ProcessorSpeed / 1000d} GHz</span><br /> 
      </div>
    </div>
  </div>

  <div>
    <div class=""table-scroll-host"">
      <table id=""result-table-{batchResult.Index}"">
        <thead>
          <tr>
            <th class=""header"">Iteration #</th>
            <th class=""header"">Elapsed time [{timeUnit.ToDisplayStringValue()}]</th>
            <th class=""header"">Mean µ [{timeUnit.ToDisplayStringValue()}]</th>
            <th class=""header"">Deviation [{timeUnit.ToDisplayStringValue()}]</th>
            <th class=""header"">Standard deviation σ [{timeUnit.ToDisplayStringValue()}]</th>
            <th class=""header"">Variance σ²</th>
          </tr>
        </thead>
        <tbody>");

        int resultIndex = 0;
        foreach (ProfilerResult result in batchResult.Results)
        {
          _ = htmlDocumentBuilder.Append($@"
          <tr class=""data-row"">
            <td class=""row-data"">{++resultIndex} (iteration {result.Iteration} /w argument list {result.ArgumentListIndex}</td>
            <td class=""row-data"">{TimeValueConverter.ConvertTo(timeUnit, result.ElapsedTime, true)}</td>
            <td class=""row-data"">{TimeValueConverter.ConvertTo(timeUnit, batchResult.AverageDuration, true)}</td>
            <td class=""row-data"">{TimeValueConverter.ConvertTo(timeUnit, result.Deviation, true)}</td>
            <td class=""row-data"">{TimeValueConverter.ConvertTo(timeUnit, batchResult.StandardDeviation, true)}</td>
            <td class=""row-data"">{batchResult.Variance}</td>
          </tr>");
        }

        _ = htmlDocumentBuilder.Append($@"
        </tbody>
        <tfoot>
          <tr>
            <th class=""data-row-summary"" colspan=""10"">Summary</th>
          </tr>
          <tr>
            <td class=""dataRow"">{batchResult.TotalIterationCount}</td>
            <td class=""dataRow"">{TimeValueConverter.ConvertTo(timeUnit, batchResult.TotalDuration, true)}</td>
            <td class=""dataRow"">{TimeValueConverter.ConvertTo(timeUnit, batchResult.AverageDuration, true)}</td>
            <td class=""dataRow"">-</td>
            <td class=""dataRow"">{TimeValueConverter.ConvertTo(timeUnit, batchResult.StandardDeviation, true)}</td>
            <td class=""dataRow"">{batchResult.Variance}</td>
          </tr>
          <tr>
          <td colspan=""6"" class=""data-row-summary"">Min (fastest): {batchResult.MinResult.ElapsedTimeConverted} {timeUnit.ToDisplayStringValue()} (#{batchResult.MinResult.Iteration})</td>
            </tr>
          <tr>
            <td colspan=""6"" class=""data-row-summary"">Max (slowest): {batchResult.MaxResult.ElapsedTimeConverted} {timeUnit.ToDisplayStringValue()} (#{batchResult.MaxResult.Iteration})</td>
          </tr>
          <tr>
            <td colspan=""6"" class=""data-row-summary"">Range: {batchResult.RangeConverted} {timeUnit.ToDisplayStringValue()}</td>
          </tr>
        </tfoot>
      </table>
    </div>
    
    <div style=""width:50%; float: left;"">
      <div id=""chart-{batchResult.Index}"" class=""line-chart""></div>
    </div>
  </div>")
        .Append($@"<a class=""navigation-link"" href=""#document_start"">Go to top 🡡</a>")
        .Append("</article>");
      }

      return htmlDocumentBuilder.ToString();
    }

    private async Task<string> GetEncodedJavaScriptCodeTextAsync()
    {
      MemoryCache cache = MemoryCache.Default;
      if (!(cache.Get(HtmlLogger.JavaScriptSourceFileName) is string scriptCode))
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
            var scriptCodeBuilder = new StringBuilder();
            string rawScriptCode = await streamReader.ReadToEndAsync();
            scriptCode = StringEncoder.EncodeFormatString(rawScriptCode);
            var cachePolicy = new CacheItemPolicy
            {
              SlidingExpiration = this.FileContentCacheExpiration
            };
            cache.Set(HtmlLogger.JavaScriptSourceFileName, scriptCode, cachePolicy);
          }
        }
      }

      return scriptCode;
    }

    private async Task<string> GetEncodedHtmlCodeTextAsync()
    {
      MemoryCache cache = MemoryCache.Default;
      if (!(cache.Get(HtmlLogger.HtmlSourceFileName) is string htmlCode))
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
            var htmlCodeBuilder = new StringBuilder();
            string rawHtmlCode = await streamReader.ReadToEndAsync();
            htmlCode = StringEncoder.EncodeFormatString(rawHtmlCode);
            var cachePolicy = new CacheItemPolicy
            {
              SlidingExpiration = this.FileContentCacheExpiration
            };
            cache.Set(HtmlLogger.HtmlSourceFileName, htmlCode, cachePolicy);
          }
        }
      }

      return htmlCode;
    }
  }

  internal class HtmlDocumentBuilderInfo
  {
    public HtmlDocumentBuilderInfo()
    {
      this.DocumentTitle = string.Empty;
      this.DocumentTemplate = string.Empty;
      this.InPageNavigationElements = string.Empty;
      this.DocumentFooterElements = string.Empty;
      this.ResultNavigationElements = string.Empty;
      this.ChartSection = string.Empty;
      this.FileName = string.Empty;
      this.MemberName = string.Empty;
      this.ScriptCode = string.Empty;
    }

    public string ScriptCode { get; set; }
    public string DocumentTitle { get; set; }
    public string DocumentTemplate { get; set; }
    public string InPageNavigationElements { get; set; }
    public string DocumentFooterElements { get; set; }
    public string ResultNavigationElements { get; set; }
    public string ChartSection { get; set; }
    public string FileName { get; set; }
    public string MemberName { get; set; }
  }

  internal class GoogleChartsDataConverter
  {
    public async Task<ChartTableCollection> CreateChartAsync(ProfilerBatchResultGroup batchResultGroup, double graphIntervalResolution)
    {
      var chartTables = new ChartTableCollection();
      foreach (ProfilerBatchResult batchResult in batchResultGroup)
      {
        if (batchResult.StandardDeviation == Microseconds.Zero)
        {
          continue;
        }

        var chartOptions = new ChartOptions()
        {
          Title = "Normal distribution",
          LegendOptions = new LegendOptions() { Position = LegendOptions.PositionTop },
          HorizontalAxis = new ChartAxis() { Title = $"Elapsed time [{batchResultGroup.CommonBaseUnit.ToDisplayStringValue()}]" }
        };

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

        ProfilerBatchResult[] resultPair = batchResult.ProfilerReferenceResult != null
          ? new[] { batchResult, batchResult.ProfilerReferenceResult }
          : new[] { batchResult };

        var calculationTasks = new List<Task<NormalDistributionData<CartesianPoint>>>();
        for (int resultIndex = 0; resultIndex < resultPair.Length; resultIndex++)
        {
          ProfilerBatchResult profilerBatchResult = resultPair[resultIndex];
          int dataSetIndex = resultIndex;
          var calculationTask = Task.Run(() =>
          {
#if NET7_0_OR_GREATER
            IEnumerable<CartesianPoint> normalDistributionValues = Math.NormDist(TimeValueConverter.ConvertTo(batchResultGroup.CommonBaseUnit, profilerBatchResult.AverageDuration, true), TimeValueConverter.ConvertTo(batchResultGroup.CommonBaseUnit, profilerBatchResult.StandardDeviation, true), graphIntervalResolution, profilerBatchResult.Results.Select(profilerResult => TimeValueConverter.ConvertTo(batchResultGroup.CommonBaseUnit, profilerResult.ElapsedTime, true)));
#else
            IEnumerable<CartesianPoint> normalDistributionValues = Math.NormDist(TimeValueConverter.ConvertTo(batchResultGroup.CommonBaseUnit, profilerBatchResult.AverageDuration, true), TimeValueConverter.ConvertTo(batchResultGroup.CommonBaseUnit, profilerBatchResult.StandardDeviation, true), graphIntervalResolution, profilerBatchResult.Results.Select(profilerResult => TimeValueConverter.ConvertTo(batchResultGroup.CommonBaseUnit, profilerResult.ElapsedTime, true)));

#endif

            var result = new NormalDistributionData<CartesianPoint>(dataSetIndex, normalDistributionValues, TimeValueConverter.ConvertTo(batchResultGroup.CommonBaseUnit, profilerBatchResult.AverageDuration, true), TimeValueConverter.ConvertTo(batchResultGroup.CommonBaseUnit, profilerBatchResult.StandardDeviation, true), batchResultGroup.CommonBaseUnit, profilerBatchResult.Results.Count)
            {
              Title = series.Title,
            };

            return result;
          });

          calculationTasks.Add(calculationTask);
        }

        NormalDistributionData<CartesianPoint>[] results = await Task.WhenAll(calculationTasks);

        int totalValueCount = results.Max(dataSet => dataSet.Count);
        int metaInfoCellCount = 3;
        int dataCellCount = results.Length;

        // Data column + meta info columns
        int totalCellCountPerEntry = 1 + metaInfoCellCount;

        var chartTable = new ChartTable(resultPair.Length, metaInfoCellCount + 1)
        {
          Options = chartOptions
        };

        // X-axis
        chartTable.AddColumn(new ChartTableColumn() { Label = $"Elapsed time [{batchResultGroup.CommonBaseUnit.ToDisplayStringValue()}]", Type = ColumnType.Number });

        var seriesResultToRowIndexMapIndex = new SeriesResultToRowIndexMap[results.Length];
        foreach (NormalDistributionData<CartesianPoint> dataSet in results)
        {
          // Y-axis
          chartTable.AddColumn(new ChartTableColumn() { Label = dataSet.Title, Type = ColumnType.Number });

          // Data annotations
          chartTable.AddColumn(new ChartTableColumn() { Role = ColumnRole.Annotation, Type = ColumnType.String });
          chartTable.AddColumn(new ChartTableColumn() { Role = ColumnRole.AnnotationText, Type = ColumnType.String });

          // Tooltip column
          chartTable.AddColumn(new ChartTableColumn() { Type = ColumnType.String, Role = ColumnRole.Tooltip });

          seriesResultToRowIndexMapIndex[dataSet.Index] = chartTable.AddResultMapperEntry(dataSet.OriginalProfilerResultCount);
        }

        ChartTableRowBuilder tableRowBuilder = chartTable.CreateTableRowBuilder();

        // In case the profiler resultsa contains multiple equal values ensure that the sigma annotations
        // are only added once (for each of the two pots).
        int plotHasSigma0Bit = BitVector32.CreateMask();
        int plotHasSigma1Bit = BitVector32.CreateMask(plotHasSigma0Bit);
        int plotHasSigma2Bit = BitVector32.CreateMask(plotHasSigma1Bit);
        int plotHasSigma3Bit = BitVector32.CreateMask(plotHasSigma2Bit);
        int plotHasSigma1NegativeBit = BitVector32.CreateMask(plotHasSigma3Bit);
        int plotHasSigma2NegativeBit = BitVector32.CreateMask(plotHasSigma1NegativeBit);
        int plotHasSigma3NegativeBit = BitVector32.CreateMask(plotHasSigma2NegativeBit);
        var plot0HasSigmaMap = new BitVector32(0);
        var plot1HasSigmaMap = new BitVector32(0);

        for (int index = 0; index < totalValueCount; index++)
        {
          foreach (NormalDistributionData<CartesianPoint> dataSet in results)
          {
            if (dataSet.Count <= index)
            {
              continue;
            }

            BitVector32 plotHasSigmaMap = dataSet.Index == 0 ? plot0HasSigmaMap : plot1HasSigmaMap;
            SeriesResultToRowIndexMap rowIndexMapper = seriesResultToRowIndexMapIndex[dataSet.Index];
            CartesianPoint dataPoint = dataSet[index];
            if (dataPoint.IsSpecialValue)
            {
              rowIndexMapper.AddResultMapperEntry(dataPoint.SpecialValueId, index);
            }

            //double roundedDataPointX = TimeValueConverter.TrimDecimalsTo(batchResults.CommonBaseUnit, dataPoint.X);
            ChartTableRow tableRow = tableRowBuilder.CreateRow();

            // Write x-axis column
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

              // Write y-axis column
              tableRow.AppendValue(dataPoint.Y);

              // Write annotation and annotation text column
              bool isDataPointMean = dataPoint.X == dataSet.Mean;
              if (!plotHasSigmaMap[plotHasSigma0Bit] && isDataPointMean)
              {
                plotHasSigmaMap[plotHasSigma0Bit] = true;
                tableRow.AppendValue($"µ = {dataPoint.X} {batchResultGroup.CommonBaseUnit.ToDisplayStringValue()}");
                tableRow.AppendValue($"Average elapsed time: P({dataPoint.X},{dataPoint.Y})".ToString(System.Globalization.CultureInfo.CurrentUICulture));
              }
              else if (!plotHasSigmaMap[plotHasSigma1NegativeBit] && dataPoint.X == dataSet.Mean - dataSet.StandardDeviation)
              {
                plotHasSigmaMap[plotHasSigma1NegativeBit] = true;
                tableRow.AppendValue("34.1 %");
                tableRow.AppendValue("-1 σ. Interval µ ± 1σ contains 68.3 % of all values.");
              }
              else if (!plotHasSigmaMap[plotHasSigma2NegativeBit] && dataPoint.X == dataSet.Mean - 2 * dataSet.StandardDeviation)
              {
                plotHasSigmaMap[plotHasSigma2NegativeBit] = true;
                tableRow.AppendValue("13.6 %");
                tableRow.AppendValue("-2 σ. Interval µ ± 2σ contains 95.4 % of all values.");
              }
              else if (!plotHasSigmaMap[plotHasSigma3NegativeBit] && dataPoint.X == dataSet.Mean - 3 * dataSet.StandardDeviation)
              {
                plotHasSigmaMap[plotHasSigma3NegativeBit] = true;
                tableRow.AppendValue("2.1 %");
                tableRow.AppendValue("-3 σ. Interval µ ± 3σ contains 99.7 % of all values.");
              }
              else if (!plotHasSigmaMap[plotHasSigma1Bit] && dataPoint.X == dataSet.Mean + dataSet.StandardDeviation)
              {
                plotHasSigmaMap[plotHasSigma1Bit] = true;
                tableRow.AppendValue("34.1 %");
                tableRow.AppendValue("1 σ. Interval µ ± 1σ contains 68.3 % of all values.");
              }
              else if (!plotHasSigmaMap[plotHasSigma2Bit] && dataPoint.X == dataSet.Mean + 2 * dataSet.StandardDeviation)
              {
                plotHasSigmaMap[plotHasSigma2Bit] = true;
                tableRow.AppendValue("13.6 %");
                tableRow.AppendValue("2 σ. Interval µ ± 2σ contains 95.4 % of all values.");
              }
              else if (!plotHasSigmaMap[plotHasSigma3Bit] && dataPoint.X == dataSet.Mean + 3 * dataSet.StandardDeviation)
              {
                plotHasSigmaMap[plotHasSigma3Bit] = true;
                tableRow.AppendValue("2.1 %");
                tableRow.AppendValue("3 σ. Interval µ ± 3σ contains 99.7 % of all values.");
              }
              else
              {
                tableRow.AppendValue(null);
                tableRow.AppendValue(null);
              }

              // Write tooltip column
              tableRow.AppendValue($"{dataSet.Title}: {(isDataPointMean ? "data mean" : string.Empty)} {dataPoint.X} {batchResultGroup.CommonBaseUnit.ToDisplayStringValue()} (density {dataPoint.Y})");
            }
          }
        }

        chartTables.Add(chartTable);
      }

      return chartTables;
    }
  }
}