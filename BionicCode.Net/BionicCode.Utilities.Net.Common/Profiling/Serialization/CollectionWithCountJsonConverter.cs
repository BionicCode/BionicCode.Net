namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  internal class CollectionWithCountJsonConverter : JsonConverter<ChartTableCollection>
  {
    public override ChartTableCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter writer, ChartTableCollection value, JsonSerializerOptions options)
    {
      writer.WriteStartObject();
        writer.WritePropertyName("tableCount");
        JsonSerializer.Serialize(writer, value.Count, options);
      writer.WritePropertyName("chartTables");
      JsonSerializer.Serialize(writer, value as IEnumerable<ChartTable>, options);

      writer.WriteEndObject();
    }
  }
}