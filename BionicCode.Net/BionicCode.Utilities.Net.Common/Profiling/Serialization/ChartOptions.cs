namespace BionicCode.Utilities.Net
{
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  internal class ChartOptions
  {
    public ChartOptions()
    {
      this.Series = new List<ChartSeries>();
      //this.HorizontalAxis = new List<ChartAxis>();
      this.VerticalAxis = new List<ChartAxis>();
    }
    public void AddSeries(ChartSeries chartSeries)
      => this.Series.Add(chartSeries);
    //public void AddHorizontalAxis(ChartAxis chartAxis)
    //  => this.HorizontalAxis.Add(chartAxis);
    public void AddHorizontalAxis(ChartAxis chartAxis)
      => this.HorizontalAxis = chartAxis;
    public void AddVerticalAxis(ChartAxis chartAxis)
      => this.VerticalAxis.Add(chartAxis);

    [JsonPropertyName("series")]
    [JsonConverter(typeof(ChartOptionsSeriesJsonConveter))]
    IList<ChartSeries> Series { get; }
    
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