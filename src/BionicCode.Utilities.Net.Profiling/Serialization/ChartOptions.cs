namespace BionicCode.Utilities.Net.Profiling
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    internal class ChartOptions
    {
        public ChartOptions()
        {
            Series = new List<ChartSeries>();
            //HorizontalAxis = new List<ChartAxis>();
            VerticalAxis = new List<ChartAxis>();
        }
        public void AddSeries(ChartSeries chartSeries)
          => Series.Add(chartSeries);
        //public void AddHorizontalAxis(ChartAxis chartAxis)
        //  => HorizontalAxis.Add(chartAxis);
        public void AddHorizontalAxis(ChartAxis chartAxis)
          => HorizontalAxis = chartAxis;
        public void AddVerticalAxis(ChartAxis chartAxis)
          => VerticalAxis.Add(chartAxis);

        [JsonPropertyName("series")]
        [JsonConverter(typeof(ChartOptionsSeriesJsonConveter))]
        private IList<ChartSeries> Series { get; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("legend")]
        public LegendOptions LegendOptions { get; set; }

        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonPropertyName("height")]
        public double Height { get; set; }

        //[JsonPropertyName("hAxis")]
        //[JsonConverter(typeof(ChartOptionsAxisJsonConveter))]
        //public IList<ChartAxis> HorizontalAxis { get; set; }

        [JsonPropertyName("hAxis")]
        public ChartAxis HorizontalAxis { get; set; }

        [JsonPropertyName("vAxes")]
        [JsonConverter(typeof(ChartOptionsAxisJsonConveter))]
        public IList<ChartAxis> VerticalAxis { get; set; }
    }
}
