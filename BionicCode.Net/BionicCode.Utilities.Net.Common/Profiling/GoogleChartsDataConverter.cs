namespace BionicCode.Utilities.Net
{
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;

  internal class GoogleChartsDataConverter
  {
    public async Task<ChartTableCollection> CreateChartAsync(ProfilerBatchResultGroup batchResultGroup, double graphIntervalResolution, CancellationToken cancellationToken)
    {
      var chartTables = new ChartTableCollection();
      foreach (ProfilerBatchResult batchResult in batchResultGroup)
      {
        cancellationToken.ThrowIfCancellationRequested();

        if (batchResult.StandardDeviation == Microseconds.Zero)
        {
          continue;
        }

        var chartOptions = new ChartOptions()
        {
          Title = "Normal distribution",
          LegendOptions = new LegendOptions() { Position = LegendOptions.PositionTop },
          HorizontalAxis = new ChartAxis() { Title = $"Elapsed time [{batchResult.BaseUnit.ToDisplayStringValue()}]" }
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
          Title = $"{batchResult.Context.MethodInvokeInfo.ShortDisplayName} ({batchResult.Context.MethodInvokeInfo.ProfiledTargetType.ToDisplayStringValue()})"
        };

        chartOptions.AddSeries(series);

        ProfilerBatchResult[] resultPair = batchResult.ProfilerReferenceResult != null
          ? new[] { batchResult, batchResult.ProfilerReferenceResult }
          : new[] { batchResult };

        var calculationTasks = new List<Task<NormalDistributionData<CartesianPoint>>>();
        for (int resultIndex = 0; resultIndex < resultPair.Length; resultIndex++)
        {
          cancellationToken.ThrowIfCancellationRequested();

          ProfilerBatchResult profilerBatchResult = resultPair[resultIndex];
          int dataSetIndex = resultIndex;
          var calculationTask = Task.Run(() =>
          {
            IEnumerable<CartesianPoint> normalDistributionValues = Math.NormDist(profilerBatchResult.AverageDurationConverted, profilerBatchResult.StandardDeviationConverted, graphIntervalResolution, profilerBatchResult.Results.Select(profilerResult => profilerResult.ElapsedTimeConverted));
            var result = new NormalDistributionData<CartesianPoint>(dataSetIndex, normalDistributionValues, profilerBatchResult.AverageDurationConverted, profilerBatchResult.StandardDeviationConverted, profilerBatchResult.BaseUnit, profilerBatchResult.Results.Count)
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
        chartTable.AddColumn(new ChartTableColumn() { Label = $"Elapsed time [{batchResult.BaseUnit.ToDisplayStringValue()}]", Type = ColumnType.Number });

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

        // In case the profiler results contains multiple equal values ensure that the sigma annotations
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
            cancellationToken.ThrowIfCancellationRequested();

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
            _ = tableRow.AppendValue(dataPoint.X);

            for (int dataCellIndex = 0; dataCellIndex < dataCellCount; dataCellIndex++)
            {
              // Skip cells (data and meta info) that belong to a different data set
              if (dataCellIndex != dataSet.Index)
              {
                for (int entryCellCount = 0; entryCellCount < totalCellCountPerEntry; entryCellCount++)
                {
                  _ = tableRow.AppendValue(null);
                }

                continue;
              }

              // Write y-axis column
              _ = tableRow.AppendValue(dataPoint.Y);

              // Write annotation and annotation text column
              bool isDataPointMean = dataPoint.X == dataSet.Mean;
              if (!plotHasSigmaMap[plotHasSigma0Bit] && isDataPointMean)
              {
                plotHasSigmaMap[plotHasSigma0Bit] = true;
                _ = tableRow.AppendValue($"µ = {dataPoint.X} {batchResult.BaseUnit.ToDisplayStringValue()}");
                _ = tableRow.AppendValue($"Average elapsed time: P({dataPoint.X},{dataPoint.Y})".ToString(System.Globalization.CultureInfo.CurrentUICulture));
              }
              else if (!plotHasSigmaMap[plotHasSigma1NegativeBit] && dataPoint.X == dataSet.Mean - dataSet.StandardDeviation)
              {
                plotHasSigmaMap[plotHasSigma1NegativeBit] = true;
                _ = tableRow.AppendValue("34.1 %");
                _ = tableRow.AppendValue("-1 σ. Interval µ ± 1σ contains 68.3 % of all values.");
              }
              else if (!plotHasSigmaMap[plotHasSigma2NegativeBit] && dataPoint.X == dataSet.Mean - (2 * dataSet.StandardDeviation))
              {
                plotHasSigmaMap[plotHasSigma2NegativeBit] = true;
                _ = tableRow.AppendValue("13.6 %");
                _ = tableRow.AppendValue("-2 σ. Interval µ ± 2σ contains 95.4 % of all values.");
              }
              else if (!plotHasSigmaMap[plotHasSigma3NegativeBit] && dataPoint.X == dataSet.Mean - (3 * dataSet.StandardDeviation))
              {
                plotHasSigmaMap[plotHasSigma3NegativeBit] = true;
                _ = tableRow.AppendValue("2.1 %");
                _ = tableRow.AppendValue("-3 σ. Interval µ ± 3σ contains 99.7 % of all values.");
              }
              else if (!plotHasSigmaMap[plotHasSigma1Bit] && dataPoint.X == dataSet.Mean + dataSet.StandardDeviation)
              {
                plotHasSigmaMap[plotHasSigma1Bit] = true;
                _ = tableRow.AppendValue("34.1 %");
                _ = tableRow.AppendValue("1 σ. Interval µ ± 1σ contains 68.3 % of all values.");
              }
              else if (!plotHasSigmaMap[plotHasSigma2Bit] && dataPoint.X == dataSet.Mean + (2 * dataSet.StandardDeviation))
              {
                plotHasSigmaMap[plotHasSigma2Bit] = true;
                _ = tableRow.AppendValue("13.6 %");
                _ = tableRow.AppendValue("2 σ. Interval µ ± 2σ contains 95.4 % of all values.");
              }
              else if (!plotHasSigmaMap[plotHasSigma3Bit] && dataPoint.X == dataSet.Mean + (3 * dataSet.StandardDeviation))
              {
                plotHasSigmaMap[plotHasSigma3Bit] = true;
                _ = tableRow.AppendValue("2.1 %");
                _ = tableRow.AppendValue("3 σ. Interval µ ± 3σ contains 99.7 % of all values.");
              }
              else
              {
                _ = tableRow.AppendValue(null);
                _ = tableRow.AppendValue(null);
              }

              // Write tooltip column
              _ = tableRow.AppendValue($"{dataSet.Title}: {(isDataPointMean ? "data mean" : string.Empty)} {dataPoint.X} {batchResult.BaseUnit.ToDisplayStringValue()} (density {dataPoint.Y})");
            }
          }
        }

        chartTables.Add(chartTable);
      }

      return chartTables;
    }
  }
}