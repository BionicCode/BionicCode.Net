namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  internal class ChartOptionsSeriesJsonConveter : JsonConverter<IList<ChartSeries>>
  {
    public override IList<ChartSeries> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter writer, IList<ChartSeries> value, JsonSerializerOptions options)
    {
      writer.WriteStartObject();
      foreach (ChartSeries series in value)
      {
        writer.WritePropertyName(series.SeriesIndex.ToString());
        JsonSerializer.Serialize(writer, series, options);
      }

      writer.WriteEndObject();
    }
  }
}