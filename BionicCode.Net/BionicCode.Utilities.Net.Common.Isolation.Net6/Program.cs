using System.Diagnostics;
using System.Drawing.Text;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using BionicCode.Utilities.Net;
using BionicCode.Utilities.Net.Profiling.Ipc;
using BionicCode.Utilities.Net.Profiling;

/******************************************************************************/
internal class Program
{
  private const string errorLogFilePath = "logs/error.log";
  private const string debugLogFilePath = "logs/debug.log";
  private static StreamWriter fileLogger;

  private static async Task<int> Main(string[] args)
  {
    try
    {
      //if (!Debugger.IsAttached)
      //{
      //  Debugger.Launch();
      //}

      await using FileStream errorFile = File.Create(debugLogFilePath);
      using (Program.fileLogger = new StreamWriter(errorFile) { AutoFlush = true })
      {
        TargetFrameworkAttribute? targetFrameworkAttribute = Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>();
        if (targetFrameworkAttribute is not null)
        {
          string runtimeName = $"{targetFrameworkAttribute.FrameworkName} ({targetFrameworkAttribute.FrameworkDisplayName})";
          await LogDebugAsync($">>Profiler context {runtimeName} loaded");
        }

        Runtime runtime = Runtime.Net6_0;
        await LogDebugAsync($">>Runtime ID set to {runtime}");

        await LogDebugAsync($"Raw process args length: {args.Length}");
        if (args.Length == 0)
        {
          throw new ArgumentException("No process arguments found.");
        }

        string pipeArgs = args[0].Trim();
        await LogDebugAsync($"Pipe args: {pipeArgs}");
        var cancellationTokenSource = new CancellationTokenSource();

        IpcProcessArgument ipcProcessArgs = await IpcProcessArgument.CreateFromJsonAsync(pipeArgs);
        var connectionInfo = new PipeClientConnection(ipcProcessArgs.PipeId, ipcProcessArgs.ServerClientLinkId);

        await LogDebugAsync($"Trying to connect to pipe server with PipeID {connectionInfo.PipeId}");
        if (await connectionInfo.TryConnectAsync(cancellationTokenSource.Token))
        {
          await LogDebugAsync($"Successfully connected to pipe server with PipeID {connectionInfo.PipeId}");
          AttributeProfiler attributeProfiler;
          //var waitHandle = EventWaitHandle.OpenExisting(ipcProcessArgs.ServerClientLinkId.ToString());
          while (connectionInfo.IsConnected)
          {
            await LogDebugAsync($"Waiting for server message");
            IPipeMessage<ProfilerContextMessage> responseMessage = await connectionInfo.ReadFromPipeAsync<ProfilerContextMessage>().ConfigureAwait(false);

            await LogDebugAsync($"Server message received");
            if (responseMessage.IsValid && responseMessage.HasData)
            {
              await LogDebugAsync($"Received message is valid");
              IAttributeProfilerConfiguration profilerConfiguration = responseMessage.Data.ProfilerConfiguration;
              if (profilerConfiguration.Runtime != runtime)
              {
                await LogDebugAsync($"EXCEPTION Profiler configuration runtime and current runtime mismatch");
                throw new InvalidOperationException($"Wrong runtime of profiler configuration.{System.Environment.NewLine}Provided runtime: {profilerConfiguration.Runtime}. Expected runtime: {runtime}");
              }

              await LogDebugAsync($"Starting profiler");
              attributeProfiler = new AttributeProfiler(profilerConfiguration);
              ProfiledTypeResultCollection profilerResult = await attributeProfiler.StartAsync(cancellationTokenSource.Token).ConfigureAwait(false);

              await LogDebugAsync($"Profiler succesfully completed with {profilerResult[0].Count} results");
              var resultMessageData = new ProfilerResultMessage(profilerResult);

              await LogDebugAsync($"Creating response message");
              IPipeMessage<ProfilerResultMessage> resultMessage = connectionInfo.CreateMessageForConversation(responseMessage.Id, resultMessageData);

              await LogDebugAsync($"Sending result message with converstaionID {resultMessage.Id} to pipe server with PipeID {connectionInfo.PipeId}");
              await connectionInfo.WriteToPipeAsync(resultMessage).ConfigureAwait(false);

              await LogDebugAsync($"Successfully send reult message");
            }
          }

          connectionInfo.Close();
        }

        return 0;
      }
    }
    catch (Exception e)
    {
      await using FileStream errorFile = File.Create(errorLogFilePath);
      await using var sw = new StreamWriter(errorFile);

      string message = string.Empty;

      TargetFrameworkAttribute? targetFrameworkAttribute = Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>();
      if (targetFrameworkAttribute is not null)
      {
        message = $"Profiler context: {targetFrameworkAttribute.FrameworkName} ({targetFrameworkAttribute.FrameworkDisplayName}){System.Environment.NewLine}**************************************************************{System.Environment.NewLine}";
      }

      await sw.WriteLineAsync(message);
      await sw.WriteLineAsync($"{e}");
      await sw.FlushAsync();
      await sw.DisposeAsync();
      var showErrorLogProcessInfo = new ProcessStartInfo()
      {
        UseShellExecute = true,
        FileName = errorFile.Name
      };

      _ = Process.Start(showErrorLogProcessInfo);

      return 99;
    }
    finally
    {

#if DEBUG

      var showDebuLogProcessInfo = new ProcessStartInfo()
      {
        UseShellExecute = true,
        FileName = (fileLogger.BaseStream as FileStream).Name
      };

      _ = Process.Start(showDebuLogProcessInfo);
#endif
    }
  }

  private static async Task LogDebugAsync(string message)
  {
#if DEBUG
    string logMessage = $"{DateTime.UtcNow} {message}";
    await fileLogger.WriteLineAsync(logMessage);
#endif
  }
}