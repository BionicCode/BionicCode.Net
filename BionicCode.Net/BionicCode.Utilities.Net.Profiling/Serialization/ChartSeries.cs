namespace BionicCode.Utilities.Net.Profiling
{
    using System.Text.Json.Serialization;

    internal class ChartSeries
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("targetAxisIndex")]
        public int TargetAxisIndex { get; set; }

        [JsonIgnore]
        public int SeriesIndex { get; set; }
    }
}
