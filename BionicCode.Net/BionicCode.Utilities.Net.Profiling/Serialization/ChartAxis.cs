namespace BionicCode.Utilities.Net.Profiling
{
    using System.Text.Json.Serialization;

    internal class ChartAxis
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonIgnore]
        public int AxisIndex { get; set; }
    }
}
