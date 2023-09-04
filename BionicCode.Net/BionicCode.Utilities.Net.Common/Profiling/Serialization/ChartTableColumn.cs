namespace BionicCode.Utilities.Net
{
  using System.Text.Json.Serialization;

  internal class ChartTableColumn
  {
    public ChartTableColumn()
    {
      this.Id = string.Empty;
      this.Label = string.Empty;
      this.Pattern = string.Empty;
      this.Type = ColumnType.None;
      this.Role = ColumnRole.None;
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