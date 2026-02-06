namespace BionicCode.Utilities.Net.Profiling
{
    using System.Text.Json.Serialization;

    internal class ChartTableColumn
    {
        public ChartTableColumn()
        {
            Id = string.Empty;
            Label = string.Empty;
            Pattern = string.Empty;
            Type = ColumnType.None;
            Role = ColumnRole.None;
        }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("pattern")]
        public string Pattern { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }
    }
}
