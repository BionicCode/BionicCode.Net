namespace BionicCode.Utilities.Net
{
  /// <summary>
  /// A delegate to print the results of the <see cref="Profiler"/> to a output (e.g. file).
  /// </summary>
  /// <param name="results">A <see cref="ProfilerBatchResult"/> object that holds the results of the benchmark run.</param>
  /// <param name="preformattedOutput">The print-ready output of the bencmark results.
  /// <br/>Returns a default table of the following format:
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
  /// </code></param>
  public delegate void ProfilerLogger(ProfilerBatchResult results, string preformattedOutput);
}
