namespace BionicCode.Utilities.Net
{
  using System.Threading.Tasks;

  /// <summary>
  /// A delegate to print the results of the <see cref="Profiler"/> to a output (e.g. file).
  /// </summary>
  /// <param name="results">A <see cref="ProfilerBatchResult"/> object that holds the results of the benchmark run.</param>
  /// <remarks>
  /// <para>Read the <see cref="ProfilerBatchResult.Summary"/> property to obtain a print-ready output of the bencmark results.
  /// <br/>This property returns a default table of the following format:
  /// <br/>
  /// <code>
  ///           ╭───────────────┬────────────────────────────┬────────────────╮
  ///           | Iteration #   │ Duration [ms]              │ Is cancelled   |
  ///           ┝━━━━━━━━━━━━━━━┿━━━━━━━━━━━━━━━━━━━━━━━━━━━━┿━━━━━━━━━━━━━━━━┥
  ///           │ 1             │                     1.0512 │ False          |
  ///           │ 2             │                     1.0020 │ False          │
  ///           │ 3             │                     0.8732 │ False          │
  ///           │ 4             │                     0.0258 │ True (ignored) │
  ///           │ 5             │                     0.9943 │ False          │
  /// ╭═════════╧═══════════════╪════════════════════════════┼────────────────┤
  /// │ Total:    -             │                     3.9207 │                │
  /// │ Min:      3             │                     0.8732 │                │
  /// │ Max:      1             │                     1.0512 │                │
  /// │ Average:  -             │                     0.9802 │                │
  /// ╰─────────────────────────┴────────────────────────────┴────────────────╯
  /// </code></para>
  /// API docs: <see href="https://sampoh.de/github/docs/bioniccode.net/api/BionicCode.Utilities.Net.ProfilerLoggerDelegate.html">void ProfilerLoggerDelegate(ProfilerBatchResult results, string preformattedOutput)</see>
  /// </remarks>
  public delegate void ProfilerLoggerDelegate(ProfilerBatchResult results, string preformattedOutput);
  /// <summary>
  /// An asynchronous delegate to print the results of the <see cref="Profiler"/> to a output (e.g. file).
  /// </summary>
  /// <param name="results">A <see cref="ProfilerBatchResult"/> object that holds the results of the benchmark run.</param>
  /// <remarks>
  /// <para>Read the <see cref="ProfilerBatchResult.Summary"/> property to obtain a print-ready output of the bencmark results.
  /// <br/>This property returns a default table of the following format:
  /// <br/>
  /// <code>
  ///           ╭───────────────┬────────────────────────────┬────────────────╮
  ///           | Iteration #   │ Duration [ms]              │ Is cancelled   |
  ///           ┝━━━━━━━━━━━━━━━┿━━━━━━━━━━━━━━━━━━━━━━━━━━━━┿━━━━━━━━━━━━━━━━┥
  ///           │ 1             │                     1.0512 │ False          |
  ///           │ 2             │                     1.0020 │ False          │
  ///           │ 3             │                     0.8732 │ False          │
  ///           │ 4             │                     0.0258 │ True (ignored) │
  ///           │ 5             │                     0.9943 │ False          │
  /// ╭═════════╧═══════════════╪════════════════════════════┼────────────────┤
  /// │ Total:    -             │                     3.9207 │                │
  /// │ Min:      3             │                     0.8732 │                │
  /// │ Max:      1             │                     1.0512 │                │
  /// │ Average:  -             │                     0.9802 │                │
  /// ╰─────────────────────────┴────────────────────────────┴────────────────╯
  /// </code></para>
  /// API docs: <see href="https://sampoh.de/github/docs/bioniccode.net/api/BionicCode.Utilities.Net.ProfilerLoggerAsyncDelegate.html">Task ProfilerLoggerAsyncDelegate(ProfilerBatchResult results, string preformattedOutput)</see>
  /// </remarks>
  public delegate Task ProfilerLoggerAsyncDelegate(ProfilerBatchResult results, string preformattedOutput);
}
