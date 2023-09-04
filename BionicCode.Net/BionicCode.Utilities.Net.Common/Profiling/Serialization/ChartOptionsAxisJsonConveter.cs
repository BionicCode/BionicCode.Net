namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  internal class ChartOptionsAxisJsonConveter : JsonConverter<IList<ChartAxis>>
  {
    public override IList<ChartAxis> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter writer, IList<ChartAxis> value, JsonSerializerOptions options)
    {
      writer.WriteStartObject();
      foreach (ChartAxis axis in value)
      {
        writer.WritePropertyName(axis.AxisIndex.ToString());
        JsonSerializer.Serialize(writer, axis, options);
      }

      writer.WriteEndObject();
    }
  }
}