namespace BionicCode.Utilities.Net.Profiling.Ipc
{
  using System;
  using System.IO;
  using System.Text.Json;
  using System.Threading.Tasks;

  internal class IpcProcessArgument
  {
    public IpcProcessArgument(Guid pipeId, Guid serverClientLinkId)
    {
      this.PipeId = pipeId;
      this.ServerClientLinkId = serverClientLinkId;
    }

    public static async Task<IpcProcessArgument> CreateFromJsonAsync(string jsonText)
    {
      using (var memoryStream = new MemoryStream())
      {
        using (var streamWriter = new StreamWriter(memoryStream))
        {
          await streamWriter.WriteAsync(jsonText).ConfigureAwait(false);
          await streamWriter.FlushAsync();
          _ = memoryStream.Seek(0, SeekOrigin.Begin);
          var serializerOptions = new JsonSerializerOptions()
          {
            WriteIndented = false,
            IgnoreReadOnlyProperties = false
          };
          IpcProcessArgument ipcProcessArgs = await JsonSerializer.DeserializeAsync<IpcProcessArgument>(memoryStream, serializerOptions).ConfigureAwait(false);

          return ipcProcessArgs;
        }
      }
    }

    public async Task<string> ToJsonAsync()
    {
      using (var memoryStream = new MemoryStream())
      {
        using (var streamReader = new StreamReader(memoryStream))
        {
          var serializerOptions = new JsonSerializerOptions()
          {
            WriteIndented = false,
            IgnoreReadOnlyProperties = false,
          };
          await JsonSerializer.SerializeAsync(memoryStream, this, serializerOptions).ConfigureAwait(false);
          _ = memoryStream.Seek(0, SeekOrigin.Begin);
          string jsonText = await streamReader.ReadToEndAsync().ConfigureAwait(false);

          return jsonText;
        }
      }
    }

    public Guid PipeId { get; }
    public Guid ServerClientLinkId { get; }
  }
}