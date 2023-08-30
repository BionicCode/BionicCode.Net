namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using System.Text;
  using System.Threading.Tasks;
  using static System.Collections.Specialized.BitVector32;

  /// <summary>
  /// Helper methods to measure code execution time.
  /// </summary>
  public static class Profiler
  {
    internal const int WarmUpCount = 4;

    /// <summary>
    /// Measures the execution time of a method.
    /// </summary>
    /// <param name="action">The code to measure execution time.</param>
    /// <returns>The average execution time of all <paramref name="runCount"/> number of iterations as <see cref="TimeSpan"/>.</returns>
    /// <param name="runCount">Number of iterations the <paramref name="action"/> should be executed.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value of<paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    /// <remarks>Use the <see cref="LogTime(Action, int, ProfilerLoggerDelegate, string, int)"/> or <see cref="LogTimeAsync(Action, int, ProfilerLoggerAsyncDelegate, string, int)"/> overload and provide an instance of the <see cref="ProfilerLoggerDelegate"/> or <see cref="ProfilerLoggerAsyncDelegate"/> delegate for the <c>logger</c> parameter to control the output target or customize the formatting.</remarks>
    public static ProfilerBatchResult LogTime(Action action, int runCount, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
      => LogTimeInternal(action, Profiler.WarmUpCount, runCount, null, sourceFileName, lineNumber);

    /// <summary>
    /// Measures the execution time of a method.
    /// </summary>
    /// <param name="action">The code to measure execution time.</param>
    /// <returns>The average execution time of all <paramref name="runCount"/> number of iterations as <see cref="TimeSpan"/>.</returns>
    /// <param name="runCount">Number of iterations the <paramref name="action"/> should be executed.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value of<paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    /// <remarks>Use the <see cref="LogTime(Action, int, ProfilerLoggerDelegate, string, int)"/> or <see cref="LogTimeAsync(Action, int, ProfilerLoggerAsyncDelegate, string, int)"/> overload and provide an instance of the <see cref="ProfilerLoggerDelegate"/> or <see cref="ProfilerLoggerAsyncDelegate"/> delegate for the <c>logger</c> parameter to control the output target or customize the formatting.</remarks>
    public static ProfilerBatchResult LogTime(Action action, int warmUpCount, int runCount, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
      => LogTimeInternal(action, warmUpCount, runCount, null, sourceFileName, lineNumber);

    /// <summary>
    /// Measures the execution time of a method.
    /// </summary>
    /// <param name="action">The code to measure execution time.</param>
    /// <returns>The average execution time of all <paramref name="runCount"/> number of iterations as <see cref="TimeSpan"/>.</returns>
    /// <param name="runCount">Number of iterations the <paramref name="action"/> should be executed.</param>
    /// <param name="logger">A delegate of type <see cref="ProfilerLoggerDelegate"/> which can be used to automatically print the <see cref="ProfilerBatchResult" /> to a destination (e.g. file) in the following formatting:
    /// 
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
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value of<paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    /// <remarks>Provide an instance of the <see cref="ProfilerLoggerDelegate"/> delegate for the <paramref name="logger"/> parameter to control the output target or customize the formatting.</remarks>
    public static ProfilerBatchResult LogTime(Action action, int runCount, ProfilerLoggerDelegate logger, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
      => LogTimeInternal(action, Profiler.WarmUpCount, runCount, logger, sourceFileName, lineNumber);

    /// <summary>
    /// Measures the execution time of a method.
    /// </summary>
    /// <param name="action">The code to measure execution time.</param>
    /// <returns>The average execution time of all <paramref name="runCount"/> number of iterations as <see cref="TimeSpan"/>.</returns>
    /// <param name="runCount">Number of iterations the <paramref name="action"/> should be executed.</param>
    /// <param name="logger">A delegate of type <see cref="ProfilerLoggerDelegate"/> which can be used to automatically print the <see cref="ProfilerBatchResult" /> to a destination (e.g. file) in the following formatting:
    /// 
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
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value of<paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    /// <remarks>Provide an instance of the <see cref="ProfilerLoggerDelegate"/> delegate for the <paramref name="logger"/> parameter to control the output target or customize the formatting.</remarks>
    public static ProfilerBatchResult LogTime(Action action, int warmUpCount, int runCount, ProfilerLoggerDelegate logger, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
      => LogTimeInternal(action, warmUpCount, runCount, logger, sourceFileName, lineNumber);

    /// <summary>
    /// Measures the execution time of a method.
    /// </summary>
    /// <param name="action">The code to measure execution time.</param>
    /// <returns>The average execution time of all <paramref name="runCount"/> number of iterations as <see cref="TimeSpan"/>.</returns>
    /// <param name="runCount">Number of iterations the <paramref name="action"/> should be executed.</param>
    /// <param name="asyncLogger">A delegate of type <see cref="ProfilerLoggerAsyncDelegate"/> which can be used to automatically print the <see cref="ProfilerBatchResult"/> to a destination (e.g. file) in the following formatting:
    /// 
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
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value of<paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    /// <remarks>Provide an instance of the <see cref="ProfilerLoggerAsyncDelegate"/> delegate for the <paramref name="asyncLogger"/> parameter to control the output target or customize the formatting.</remarks>
    public static async Task<ProfilerBatchResult> LogTimeAsync(Action action, int runCount, ProfilerLoggerAsyncDelegate asyncLogger, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
      => await LogTimeAsyncInternal(null, null, action, Profiler.WarmUpCount, runCount, null, asyncLogger, sourceFileName, lineNumber);

    /// <summary>
    /// Measures the execution time of a method.
    /// </summary>
    /// <param name="action">The code to measure execution time.</param>
    /// <returns>The average execution time of all <paramref name="runCount"/> number of iterations as <see cref="TimeSpan"/>.</returns>
    /// <param name="runCount">Number of iterations the <paramref name="action"/> should be executed.</param>
    /// <param name="asyncLogger">A delegate of type <see cref="ProfilerLoggerAsyncDelegate"/> which can be used to automatically print the <see cref="ProfilerBatchResult"/> to a destination (e.g. file) in the following formatting:
    /// 
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
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value of<paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    /// <remarks>Provide an instance of the <see cref="ProfilerLoggerAsyncDelegate"/> delegate for the <paramref name="asyncLogger"/> parameter to control the output target or customize the formatting.</remarks>
    public static async Task<ProfilerBatchResult> LogTimeAsync(Action action, int warmUpCount, int runCount, ProfilerLoggerAsyncDelegate asyncLogger, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
      => await LogTimeAsyncInternal(null, null, action, warmUpCount, runCount, null, asyncLogger, sourceFileName, lineNumber);

    /// <summary>
    /// Measures the execution time of a method.
    /// </summary>
    /// <param name="asyncAction">A delegate that executes the asynchronous code to measure execution time.</param>
    /// <param name="runCount">Number of iterations the <paramref name="asyncAction"/> should be executed.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value of <paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    /// <returns>A <see cref="Task"/> holding the average execution time of all <paramref name="runCount"/> number of iterations as <see cref="TimeSpan"/>.</returns> 
    /// <remarks>
    /// <para>Cancelled tasks are ignored when calculating the result (although the cancelled runs are listed in the <see cref="ProfilerBatchResult.Summary"/>, but marked as cancelled).
    /// <br/>A cancelled task is a <see cref="Task"/> where the <see cref="Task.Status"/> returns either <see cref="TaskStatus.Canceled"/> or <see cref="TaskStatus.Faulted"/> or an <see cref="OperationCanceledException"/> exception was thrown.</para>
    /// Use the <see cref="LogTimeAsync(Func{Task}, int, ProfilerLoggerAsyncDelegate, string, int)"/> or <see cref="LogTimeAsync(Action, int, ProfilerLoggerAsyncDelegate, string, int)"/> overload and provide an instance of the <see cref="ProfilerLoggerAsyncDelegate"/> or <see cref="ProfilerLoggerAsyncDelegate"/> delegate for the <c>logger</c> parameter to control the output target or customize the formatting.</remarks>
    public async static Task<ProfilerBatchResult> LogTimeAsync(Func<Task> asyncAction, int runCount, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
      => await LogTimeAsyncInternal(asyncAction, null, null, Profiler.WarmUpCount, runCount, null, null, sourceFileName, lineNumber);

    /// <summary>
    /// Measures the execution time of a method.
    /// </summary>
    /// <param name="asyncAction">A delegate that executes the asynchronous code to measure execution time.</param>
    /// <param name="runCount">Number of iterations the <paramref name="asyncAction"/> should be executed.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value of <paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    /// <returns>A <see cref="Task"/> holding the average execution time of all <paramref name="runCount"/> number of iterations as <see cref="TimeSpan"/>.</returns> 
    /// <remarks>
    /// <para>Cancelled tasks are ignored when calculating the result (although the cancelled runs are listed in the <see cref="ProfilerBatchResult.Summary"/>, but marked as cancelled).
    /// <br/>A cancelled task is a <see cref="Task"/> where the <see cref="Task.Status"/> returns either <see cref="TaskStatus.Canceled"/> or <see cref="TaskStatus.Faulted"/> or an <see cref="OperationCanceledException"/> exception was thrown.</para>
    /// Use the <see cref="LogTimeAsync(Func{Task}, int, ProfilerLoggerAsyncDelegate, string, int)"/> or <see cref="LogTimeAsync(Action, int, ProfilerLoggerAsyncDelegate, string, int)"/> overload and provide an instance of the <see cref="ProfilerLoggerAsyncDelegate"/> or <see cref="ProfilerLoggerAsyncDelegate"/> delegate for the <c>logger</c> parameter to control the output target or customize the formatting.</remarks>
    public async static Task<ProfilerBatchResult> LogTimeAsync(Func<Task> asyncAction, int warmUpCount, int runCount, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
      => await LogTimeAsyncInternal(asyncAction, null, null, warmUpCount, runCount, null, null, sourceFileName, lineNumber);

    /// <summary>
    /// Measures the execution time of a method.
    /// </summary>
    /// <param name="asyncAction">A delegate that executes the asynchronous code to measure execution time.</param>
    /// <param name="runCount">Number of iterations the <paramref name="asyncAction"/> should be executed.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value of <paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    /// <returns>A <see cref="Task"/> holding the average execution time of all <paramref name="runCount"/> number of iterations as <see cref="TimeSpan"/>.</returns> 
    /// <remarks>
    /// <para>Cancelled tasks are ignored when calculating the result (although the cancelled runs are listed in the <see cref="ProfilerBatchResult.Summary"/>, but marked as cancelled).
    /// <br/>A cancelled task is a <see cref="Task"/> where the <see cref="Task.Status"/> returns either <see cref="TaskStatus.Canceled"/> or <see cref="TaskStatus.Faulted"/> or an <see cref="OperationCanceledException"/> exception was thrown.</para>
    /// Use the <see cref="LogTimeAsync(Func{Task}, int, ProfilerLoggerAsyncDelegate, string, int)"/> or <see cref="LogTimeAsync(Action, int, ProfilerLoggerAsyncDelegate, string, int)"/> overload and provide an instance of the <see cref="ProfilerLoggerAsyncDelegate"/> or <see cref="ProfilerLoggerAsyncDelegate"/> delegate for the <c>logger</c> parameter to control the output target or customize the formatting.</remarks>
    public async static Task<ProfilerBatchResult> LogTimeAsync(Func<ValueTask> asyncAction, int runCount, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
      => await LogTimeAsyncInternal(null, asyncAction, null, Profiler.WarmUpCount, runCount, null, null, sourceFileName, lineNumber);

    /// <summary>
    /// Measures the execution time of a method.
    /// </summary>
    /// <param name="asyncAction">A delegate that executes the asynchronous code to measure execution time.</param>
    /// <param name="runCount">Number of iterations the <paramref name="asyncAction"/> should be executed.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value of <paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    /// <returns>A <see cref="Task"/> holding the average execution time of all <paramref name="runCount"/> number of iterations as <see cref="TimeSpan"/>.</returns> 
    /// <remarks>
    /// <para>Cancelled tasks are ignored when calculating the result (although the cancelled runs are listed in the <see cref="ProfilerBatchResult.Summary"/>, but marked as cancelled).
    /// <br/>A cancelled task is a <see cref="Task"/> where the <see cref="Task.Status"/> returns either <see cref="TaskStatus.Canceled"/> or <see cref="TaskStatus.Faulted"/> or an <see cref="OperationCanceledException"/> exception was thrown.</para>
    /// Use the <see cref="LogTimeAsync(Func{Task}, int, ProfilerLoggerAsyncDelegate, string, int)"/> or <see cref="LogTimeAsync(Action, int, ProfilerLoggerAsyncDelegate, string, int)"/> overload and provide an instance of the <see cref="ProfilerLoggerAsyncDelegate"/> or <see cref="ProfilerLoggerAsyncDelegate"/> delegate for the <c>logger</c> parameter to control the output target or customize the formatting.</remarks>
    public async static Task<ProfilerBatchResult> LogTimeAsync(Func<ValueTask> asyncAction, int warmUpCount, int runCount, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
      => await LogTimeAsyncInternal(null, asyncAction, null, warmUpCount, runCount, null, null, sourceFileName, lineNumber);

    /// <summary>
    /// Measures the execution time of a method.
    /// </summary>
    /// <param name="asyncAction">A delegate that executes the asynchronous code to measure execution time.</param>
    /// <param name="runCount">Number of iterations the <paramref name="asyncAction"/> should be executed.</param>
    /// <param name="logger">A delegate of type <see cref="ProfilerLoggerDelegate"/> which can be used to automatically print the <see cref="ProfilerBatchResult"/> to a destination (e.g. file) in the following formatting:
    /// 
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
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value of <paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    /// <returns>A <see cref="Task"/> holding the <see cref="ProfilerBatchResult"/> result which contains neta data like average execution time or a formatted report (<see cref="ProfilerBatchResult.Summary"/>) of all <paramref name="runCount"/> number of iterations.</returns>
    /// <remarks>
    /// <para>Cancelled tasks are ignored when calculating the result (although the cancelled runs are listed in the <see cref="ProfilerBatchResult.Summary"/>, but marked as cancelled).
    /// <br/>A cancelled task is a <see cref="Task"/> where the <see cref="Task.Status"/> returns either <see cref="TaskStatus.Canceled"/> or <see cref="TaskStatus.Faulted"/> or an <see cref="OperationCanceledException"/> exception was thrown.</para>
    /// Provide an instance of the <see cref="ProfilerLoggerDelegate"/> delegate for the <paramref name="logger"/> parameter to control the output target or customize the formatting.</remarks>
    public static async Task<ProfilerBatchResult> LogTimeAsync(Func<Task> asyncAction, int runCount, ProfilerLoggerDelegate logger, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1) 
      => await LogTimeAsyncInternal(asyncAction, null, null, Profiler.WarmUpCount, runCount, logger, null, sourceFileName, lineNumber);

    /// <summary>
    /// Measures the execution time of a method.
    /// </summary>
    /// <param name="asyncAction">A delegate that executes the asynchronous code to measure execution time.</param>
    /// <param name="runCount">Number of iterations the <paramref name="asyncAction"/> should be executed.</param>
    /// <param name="logger">A delegate of type <see cref="ProfilerLoggerDelegate"/> which can be used to automatically print the <see cref="ProfilerBatchResult"/> to a destination (e.g. file) in the following formatting:
    /// 
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
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value of <paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    /// <returns>A <see cref="Task"/> holding the <see cref="ProfilerBatchResult"/> result which contains neta data like average execution time or a formatted report (<see cref="ProfilerBatchResult.Summary"/>) of all <paramref name="runCount"/> number of iterations.</returns>
    /// <remarks>
    /// <para>Cancelled tasks are ignored when calculating the result (although the cancelled runs are listed in the <see cref="ProfilerBatchResult.Summary"/>, but marked as cancelled).
    /// <br/>A cancelled task is a <see cref="Task"/> where the <see cref="Task.Status"/> returns either <see cref="TaskStatus.Canceled"/> or <see cref="TaskStatus.Faulted"/> or an <see cref="OperationCanceledException"/> exception was thrown.</para>
    /// Provide an instance of the <see cref="ProfilerLoggerDelegate"/> delegate for the <paramref name="logger"/> parameter to control the output target or customize the formatting.</remarks>
    public static async Task<ProfilerBatchResult> LogTimeAsync(Func<Task> asyncAction, int warmUpCount, int runCount, ProfilerLoggerDelegate logger, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
      => await LogTimeAsyncInternal(asyncAction, null, null, warmUpCount, runCount, logger, null, sourceFileName, lineNumber);

    /// <summary>
    /// Measures the execution time of a method.
    /// </summary>
    /// <param name="asyncAction">A delegate that executes the asynchronous code to measure execution time.</param>
    /// <param name="runCount">Number of iterations the <paramref name="asyncAction"/> should be executed.</param>
    /// <param name="logger">A delegate of type <see cref="ProfilerLoggerDelegate"/> which can be used to automatically print the <see cref="ProfilerBatchResult.Summary"/> to a destination (e.g. file) in the following formatting:
    /// 
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
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value of <paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    /// <returns>A <see cref="Task"/> holding the <see cref="ProfilerBatchResult"/> result which contains neta data like average execution time or a formatted report (<see cref="ProfilerBatchResult.Summary"/>) of all <paramref name="runCount"/> number of iterations.</returns>
    /// <remarks>
    /// <para>Cancelled tasks are ignored when calculating the result (although the cancelled runs are listed in the <see cref="ProfilerBatchResult.Summary"/>, but marked as cancelled).
    /// <br/>A cancelled task is a <see cref="Task"/> where the <see cref="Task.Status"/> returns either <see cref="TaskStatus.Canceled"/> or <see cref="TaskStatus.Faulted"/> or an <see cref="OperationCanceledException"/> exception was thrown.</para>
    /// Provide an instance of the <see cref="ProfilerLoggerDelegate"/> delegate for the <paramref name="logger"/> parameter to control the output target or customize the formatting.</remarks>
    public static async Task<ProfilerBatchResult> LogTimeAsync(Func<ValueTask> asyncAction, int runCount, ProfilerLoggerDelegate logger, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
      => await LogTimeAsyncInternal(null, asyncAction, null, Profiler.WarmUpCount, runCount, logger, null, sourceFileName, lineNumber);

    /// <summary>
    /// Measures the execution time of a method.
    /// </summary>
    /// <param name="asyncAction">A delegate that executes the asynchronous code to measure execution time.</param>
    /// <param name="runCount">Number of iterations the <paramref name="asyncAction"/> should be executed.</param>
    /// <param name="logger">A delegate of type <see cref="ProfilerLoggerDelegate"/> which can be used to automatically print the <see cref="ProfilerBatchResult.Summary"/> to a destination (e.g. file) in the following formatting:
    /// 
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
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value of <paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    /// <returns>A <see cref="Task"/> holding the <see cref="ProfilerBatchResult"/> result which contains neta data like average execution time or a formatted report (<see cref="ProfilerBatchResult.Summary"/>) of all <paramref name="runCount"/> number of iterations.</returns>
    /// <remarks>
    /// <para>Cancelled tasks are ignored when calculating the result (although the cancelled runs are listed in the <see cref="ProfilerBatchResult.Summary"/>, but marked as cancelled).
    /// <br/>A cancelled task is a <see cref="Task"/> where the <see cref="Task.Status"/> returns either <see cref="TaskStatus.Canceled"/> or <see cref="TaskStatus.Faulted"/> or an <see cref="OperationCanceledException"/> exception was thrown.</para>
    /// Provide an instance of the <see cref="ProfilerLoggerDelegate"/> delegate for the <paramref name="logger"/> parameter to control the output target or customize the formatting.</remarks>
    public static async Task<ProfilerBatchResult> LogTimeAsync(Func<ValueTask> asyncAction, int warmUpCount, int runCount, ProfilerLoggerDelegate logger, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
      => await LogTimeAsyncInternal(null, asyncAction, null, warmUpCount, runCount, logger, null, sourceFileName, lineNumber);

    /// <summary>
    /// Measures the execution time of a method.
    /// </summary>
    /// <param name="asyncAction">A delegate that executes the asynchronous code to measure execution time.</param>
    /// <param name="runCount">Number of iterations the <paramref name="asyncAction"/> should be executed.</param>
    /// <param name="asyncLogger">An asynchronous delegate of type <see cref="ProfilerLoggerAsyncDelegate"/> which can be used to automatically let the <see cref="LogTimeAsync(Func{Task}, int, ProfilerLoggerDelegate)"/> print the <see cref="ProfilerBatchResult.Summary"/> to a destination (e.g. file) in the following formatting:
    /// 
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
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value of<paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    /// <returns>A <see cref="Task"/> holding the <see cref="ProfilerBatchResult"/> result which contains neta data like average execution time or a formatted report (<see cref="ProfilerBatchResult.Summary"/>) of all <paramref name="runCount"/> number of iterations.</returns>
    /// <remarks>
    /// <para>Cancelled tasks are ignored when calculating the result (although the cancelled runs are listed in the <see cref="ProfilerBatchResult.Summary"/>, but marked as cancelled).
    /// <br/>A cancelled task is a <see cref="Task"/> where the <see cref="Task.Status"/> returns either <see cref="TaskStatus.Canceled"/> or <see cref="TaskStatus.Faulted"/> or an <see cref="OperationCanceledException"/> exception was thrown.</para>
    /// Provide an instance of the <see cref="ProfilerLoggerAsyncDelegate"/> delegate for the <paramref name="asyncLogger"/> parameter to control the output target or customize the formatting.</remarks>
    public static async Task<ProfilerBatchResult> LogTimeAsync(Func<Task> asyncAction, int runCount, ProfilerLoggerAsyncDelegate asyncLogger, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
      => await LogTimeAsyncInternal(asyncAction, null, null, Profiler.WarmUpCount, runCount, null, asyncLogger, sourceFileName, lineNumber);

    /// <summary>
    /// Measures the execution time of a method.
    /// </summary>
    /// <param name="asyncAction">A delegate that executes the asynchronous code to measure execution time.</param>
    /// <param name="runCount">Number of iterations the <paramref name="asyncAction"/> should be executed.</param>
    /// <param name="asyncLogger">An asynchronous delegate of type <see cref="ProfilerLoggerAsyncDelegate"/> which can be used to automatically let the <see cref="LogTimeAsync(Func{Task}, int, ProfilerLoggerDelegate)"/> print the <see cref="ProfilerBatchResult.Summary"/> to a destination (e.g. file) in the following formatting:
    /// 
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
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value of<paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    /// <returns>A <see cref="Task"/> holding the <see cref="ProfilerBatchResult"/> result which contains neta data like average execution time or a formatted report (<see cref="ProfilerBatchResult.Summary"/>) of all <paramref name="runCount"/> number of iterations.</returns>
    /// <remarks>
    /// <para>Cancelled tasks are ignored when calculating the result (although the cancelled runs are listed in the <see cref="ProfilerBatchResult.Summary"/>, but marked as cancelled).
    /// <br/>A cancelled task is a <see cref="Task"/> where the <see cref="Task.Status"/> returns either <see cref="TaskStatus.Canceled"/> or <see cref="TaskStatus.Faulted"/> or an <see cref="OperationCanceledException"/> exception was thrown.</para>
    /// Provide an instance of the <see cref="ProfilerLoggerAsyncDelegate"/> delegate for the <paramref name="asyncLogger"/> parameter to control the output target or customize the formatting.</remarks>
    public static async Task<ProfilerBatchResult> LogTimeAsync(Func<Task> asyncAction, int warmUpCount, int runCount, ProfilerLoggerAsyncDelegate asyncLogger, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
      => await LogTimeAsyncInternal(asyncAction, null, null, warmUpCount, runCount, null, asyncLogger, sourceFileName, lineNumber);

    /// <summary>
    /// Measures the execution time of a method.
    /// </summary>
    /// <param name="asyncAction">A delegate that executes the asynchronous code to measure execution time.</param>
    /// <param name="runCount">Number of iterations the <paramref name="asyncAction"/> should be executed.</param>
    /// <param name="asyncLogger">An asynchronous delegate of type <see cref="ProfilerLoggerAsyncDelegate"/> which can be used to automatically let the <see cref="LogTimeAsync(Func{Task}, int, ProfilerLoggerDelegate)"/> print the <see cref="ProfilerBatchResult.Summary"/> to a destination (e.g. file) in the following formatting:
    /// 
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
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value of<paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    /// <returns>A <see cref="Task"/> holding the <see cref="ProfilerBatchResult"/> result which contains neta data like average execution time or a formatted report (<see cref="ProfilerBatchResult.Summary"/>) of all <paramref name="runCount"/> number of iterations.</returns>
    /// <remarks>
    /// <para>Cancelled tasks are ignored when calculating the result (although the cancelled runs are listed in the <see cref="ProfilerBatchResult.Summary"/>, but marked as cancelled).
    /// <br/>A cancelled task is a <see cref="Task"/> where the <see cref="Task.Status"/> returns either <see cref="TaskStatus.Canceled"/> or <see cref="TaskStatus.Faulted"/> or an <see cref="OperationCanceledException"/> exception was thrown.</para>
    /// Provide an instance of the <see cref="ProfilerLoggerAsyncDelegate"/> delegate for the <paramref name="asyncLogger"/> parameter to control the output target or customize the formatting.</remarks>
    public static async Task<ProfilerBatchResult> LogTimeAsync(Func<ValueTask> asyncAction, int runCount, ProfilerLoggerAsyncDelegate asyncLogger, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
      => await LogTimeAsyncInternal(null, asyncAction, null, Profiler.WarmUpCount, runCount, null, asyncLogger, sourceFileName, lineNumber);

    /// <summary>
    /// Measures the execution time of a method.
    /// </summary>
    /// <param name="asyncAction">A delegate that executes the asynchronous code to measure execution time.</param>
    /// <param name="runCount">Number of iterations the <paramref name="asyncAction"/> should be executed.</param>
    /// <param name="asyncLogger">An asynchronous delegate of type <see cref="ProfilerLoggerAsyncDelegate"/> which can be used to automatically let the <see cref="LogTimeAsync(Func{Task}, int, ProfilerLoggerDelegate)"/> print the <see cref="ProfilerBatchResult.Summary"/> to a destination (e.g. file) in the following formatting:
    /// 
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
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value of<paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    /// <returns>A <see cref="Task"/> holding the <see cref="ProfilerBatchResult"/> result which contains neta data like average execution time or a formatted report (<see cref="ProfilerBatchResult.Summary"/>) of all <paramref name="runCount"/> number of iterations.</returns>
    /// <remarks>
    /// <para>Cancelled tasks are ignored when calculating the result (although the cancelled runs are listed in the <see cref="ProfilerBatchResult.Summary"/>, but marked as cancelled).
    /// <br/>A cancelled task is a <see cref="Task"/> where the <see cref="Task.Status"/> returns either <see cref="TaskStatus.Canceled"/> or <see cref="TaskStatus.Faulted"/> or an <see cref="OperationCanceledException"/> exception was thrown.</para>
    /// Provide an instance of the <see cref="ProfilerLoggerAsyncDelegate"/> delegate for the <paramref name="asyncLogger"/> parameter to control the output target or customize the formatting.</remarks>
    public static async Task<ProfilerBatchResult> LogTimeAsync(Func<ValueTask> asyncAction, int warmUpCount, int runCount, ProfilerLoggerAsyncDelegate asyncLogger, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
      => await LogTimeAsyncInternal(null, asyncAction, null, warmUpCount, runCount, null, asyncLogger, sourceFileName, lineNumber);

    private static async Task<ProfilerBatchResult> LogTimeAsyncInternal(Func<Task> asyncAction, Func<ValueTask> asyncValueTaskAction, Action action, int warmUpCount, int runCount, ProfilerLoggerDelegate logger, ProfilerLoggerAsyncDelegate asyncLogger, string sourceFileName, int lineNumber)
    {
      if (runCount < 0)
      {
        throw new ArgumentOutOfRangeException(ExceptionMessages.GetArgumentExceptionMessage_ProfilerRunCount(), nameof(runCount));
      }

      if (warmUpCount < 0)
      {
        throw new ArgumentOutOfRangeException(ExceptionMessages.GetArgumentExceptionMessage_ProfilerWarmUpCount(), nameof(warmUpCount));
      }

      if (runCount == 0)
      {
        return ProfilerBatchResult.Empty;
      }

      ProfilerBatchResult result = new ProfilerBatchResult(runCount, DateTime.Now);
      var assemblyOfTargetType = Assembly.GetCallingAssembly();
      string asseblyName = assemblyOfTargetType.GetName().Name;
      var context = new ProfilerContext(asseblyName, action?.GetType().ToDisplayName() ?? asyncAction?.GetType().ToDisplayName(), ProfiledTargetType.Delegate, sourceFileName, lineNumber);
      result.Context = context;

      await LogAverageTimeInternalAsync(asyncAction, asyncValueTaskAction, action, warmUpCount, runCount, result);
      logger?.Invoke(result);
      if (asyncLogger != null)
      {
        await asyncLogger.Invoke(result);
      }

      return result;
    }

    private static ProfilerBatchResult LogTimeInternal(Action action, int warmUpCount, int runCount, ProfilerLoggerDelegate logger, string sourceFileName, int lineNumber)
    {
      if (runCount < 0)
      {
        throw new ArgumentException(ExceptionMessages.GetArgumentExceptionMessage_ProfilerRunCount(), nameof(runCount));
      }

      if (warmUpCount < 0)
      {
        throw new ArgumentOutOfRangeException(ExceptionMessages.GetArgumentExceptionMessage_ProfilerWarmUpCount(), nameof(warmUpCount));
      }

      if (runCount == 0)
      {
        return ProfilerBatchResult.Empty;
      }

      var result = new ProfilerBatchResult(runCount, DateTime.Now);
      var assemblyOfTargetType = Assembly.GetCallingAssembly();
      string asseblyName = assemblyOfTargetType.GetName().Name;
      var context = new ProfilerContext(asseblyName, action?.GetType().ToDisplayName(), ProfiledTargetType.Delegate, sourceFileName, lineNumber);
      result.Context = context;

      LogAverageTimeInternal(action, warmUpCount, runCount, result);
      logger?.Invoke(result);

      return result;
    }

    private static async Task LogAverageTimeInternalAsync(Func<Task> asyncAction, Func<ValueTask> asyncValueTaskAction, Action action, int warmUpCount, int runCount, ProfilerBatchResult result)
    {
      var stopwatch = new Stopwatch();
      Task profiledTask = null;
      for (int iterationCounter = 1 - warmUpCount; iterationCounter <= runCount; iterationCounter++)
      {
        if (action != null)
        {
          stopwatch.Restart();
          action.Invoke();
          stopwatch.Stop();
        }
        else if (asyncAction != null)
        {
          try
          {
            stopwatch.Restart();
            profiledTask = asyncAction.Invoke();
            await profiledTask;
            stopwatch.Stop();
          }
          catch (OperationCanceledException)
          {
            stopwatch.Stop();
          }
        }
        else if (asyncValueTaskAction != null)
        {
          try
          {
            stopwatch.Restart();
            ValueTask profiledValueTask = asyncValueTaskAction.Invoke();
            await profiledValueTask;
            stopwatch.Stop();
            profiledTask = profiledValueTask.AsTask();
          }
          catch (OperationCanceledException)
          {
            stopwatch.Stop();
          }
        }

        if (iterationCounter < 1)
        {
          continue;
        }

        var iterationResult = new ProfilerResult(iterationCounter, profiledTask, stopwatch.Elapsed, result);
        result.AddResult(iterationResult);
      }
    }

    private static void LogAverageTimeInternal(Action action, int warmUpCount, int runCount, ProfilerBatchResult result)
    {
      var stopwatch = new Stopwatch();
      for (int iterationCounter = 1 - warmUpCount; iterationCounter <= runCount; iterationCounter++)
      {
        stopwatch.Restart();
        action.Invoke();
        stopwatch.Stop();
        var iterationResult = new ProfilerResult(iterationCounter, stopwatch.Elapsed, result);

        if (iterationCounter < 1)
        {
          continue;
        }

        result.AddResult(iterationResult);
      }
    }

    /// <summary>
    /// Measures the execution time of using block i.e. the scope of the <see cref="IDisposable"/>'s lifetime.
    /// </summary>
    /// <param name="logger">A delegate of type <see cref="ProfilerLoggerDelegate"/> which can be used to automatically let this method print the <see cref="ProfilerBatchResult.Summary"/> to a destination (e.g. file) in the following formatting:
    /// 
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
    /// <param name="result">A <see cref="Task"/> holding the <see cref="ProfilerBatchResult"/> result which contains meta data like average execution time or a formatted report (<see cref="ProfilerBatchResult.Summary"/>).</param>
    /// <returns>An <see cref="IDisposable"/> to control the scope of the profiling.</returns>
    /// <para>
    /// <see href="https://sampoh.de/github/docs/bioniccode.net/api/BionicCode.Utilities.Net.Profiler.html#BionicCode_Utilities_Net_Profiler_LogTimeScoped_BionicCode_Utilities_Net_ProfilerLoggerDelegate_BionicCode_Utilities_Net_ProfilerBatchResult__"/>
    /// </para>
    /// <remarks>
    /// Time is measured during the lifetime of the <see cref="IDisposable"/> instance: from time of creation to the final <see cref="IDisposable.Dispose"/> call.
    /// <br/>It's recommended to use the <see cref="IDisposable"/> managed by a using-statement or using-expression.</remarks>
    /// <example>
    /// <code>
    /// public static void Main(string[] args)
    /// {
    ///  ProfilerBatchResult result;
    ///  using (Profiler.LogTimeScoped(out result))
    ///  {
    ///    FibonacciSeries(0, 1, 1, 100);
    ///  }
    /// 
    ///  // Print a preformatted summary
    ///  Console.Write(result.Summary);
    /// 
    ///  // Print the average duration
    ///  Console.Write(result.AverageDuration);
    /// }
    /// 
    /// public static void FibonacciSeries(int firstNumber, int secondNumber, int counter, int number)
    /// {
    ///   Console.Write(firstNumber + " ");
    ///   if (counter &lt; number)
    ///   {
    ///     FibonacciSeries(secondNumber, firstNumber + secondNumber, counter + 1, number);
    ///   }
    /// }
    /// </code>
    /// 
    /// Output:
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
    /// </code>
    /// </example>
    public static IDisposable LogTimeScoped(ProfilerLoggerDelegate logger, out ProfilerBatchResult result, [CallerMemberName] string scopeName = "", [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
    {
            
      result = new ProfilerBatchResult(1,  DateTime.Now);
      var assemblyOfTargetType = Assembly.GetCallingAssembly();
      string asseblyName = assemblyOfTargetType.GetName().Name;
      var context = new ProfilerContext(asseblyName, scopeName, ProfiledTargetType.Delegate, sourceFileName, lineNumber);
      result.Context = context;
      var profilerScopeProvider = new ProfilerScopeProvider(logger, context);
      IDisposable profilerScope = profilerScopeProvider.StartProfiling(out result);

      return profilerScope;
    }

    /// <summary>
    /// Measures the execution time of using block i.e. the scope of the <see cref="IDisposable"/>'s lifetime.
    /// </summary>
    /// <param name="logger">An asynchronous delegate of type <see cref="ProfilerLoggerAsyncDelegate"/> which can be used to automatically let the <see cref="LogTimeAsync(Func{Task}, int, ProfilerLoggerDelegate)"/> print the <see cref="ProfilerBatchResult.Summary"/> to a destination (e.g. file) in the following formatting:
    /// 
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
    /// <param name="result">A <see cref="Task"/> holding the <see cref="ProfilerBatchResult"/> result which contains meta data like average execution time or a formatted report (<see cref="ProfilerBatchResult.Summary"/>).</param>
    /// <returns>An <see cref="IDisposable"/> to control the scope of the profiling.</returns>
    /// <example>
    /// <code>
    /// public static void Main(string[] args)
    /// {
    ///  ProfilerBatchResult result;
    ///  using (Profiler.LogTimeScoped(LogToFileAsync, out result))
    ///  {
    ///    FibonacciSeries(0, 1, 1, 100);
    ///  }
    /// 
    ///  // Print a preformatted summary
    ///  Console.Write(result.Summary);
    /// 
    ///  // Print the average duration
    ///  Console.Write(result.AverageDuration);
    /// }
    /// 
    /// private static async Task LogToFileAsync(ProfilerBatchResult result)
    /// {
    ///   await File.WriteAllTextAsync(summary, @"c:\profiler_log.txt");
    /// }
    /// 
    /// public static void FibonacciSeries(int firstNumber, int secondNumber, int counter, int number)
    /// {
    ///   Console.Write(firstNumber + " ");
    ///   if (counter &lt; number)
    ///   {
    ///     FibonacciSeries(secondNumber, firstNumber + secondNumber, counter + 1, number);
    ///   }
    /// }
    /// </code>
    /// 
    /// Output:
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
    /// </code>
    /// </example>
    /// <remarks>
    /// Time is measured during the lifetime of the <see cref="IDisposable"/> instance: from time of creation to the final <see cref="IDisposable.Dispose"/> call.
    /// <br/>It's recommended to use the <see cref="IDisposable"/> managed by a using-statement or using-expression.
    /// <para>
    /// API docs: <see href="https://sampoh.de/github/docs/bioniccode.net/api/BionicCode.Utilities.Net.Profiler.html#BionicCode_Utilities_Net_Profiler_LogTimeScoped_BionicCode_Utilities_Net_ProfilerLoggerAsyncDelegate_BionicCode_Utilities_Net_ProfilerBatchResult__">IDisposable LogTimeScoped(ProfilerLoggerAsyncDelegate, out ProfilerBatchResult)</see>
    /// </para>
    /// </remarks>
    public static IDisposable LogTimeScoped(ProfilerLoggerAsyncDelegate logger, out ProfilerBatchResult result, [CallerMemberName] string scopeName = "", [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
    {
      result = new ProfilerBatchResult(1,DateTime.Now);
      var assemblyOfTargetType = Assembly.GetCallingAssembly();
      string asseblyName = assemblyOfTargetType.GetName().Name;
      var context = new ProfilerContext(asseblyName, scopeName, ProfiledTargetType.Delegate, sourceFileName, lineNumber);
      result.Context = context;
      var profilerScopeProvider = new ProfilerScopeProvider(logger, context);
      IDisposable profilerScope = profilerScopeProvider.StartProfiling(out result);

      return profilerScope;
    }

    /// <summary>
    /// Measures the execution time of a using block i.e. the scope of the <see cref="IDisposable"/>'s lifetime.
    /// </summary>
    /// <param name="result">A <see cref="Task"/> holding the <see cref="ProfilerBatchResult"/> result which contains meta data like average execution time or a formatted report (<see cref="ProfilerBatchResult.Summary"/>).</param>
    /// <returns>An <see cref="IDisposable"/> to control the scope of the profiling.</returns>
    /// <remarks>
    /// Time is measured during the lifetime of the <see cref="IDisposable"/> instance: from time of creation to the final <see cref="IDisposable.Dispose"/> call.
    /// <br/>It's recommended to use the <see cref="IDisposable"/> managed by a using-statement or using-expression.</remarks>
    /// <example>
    /// <code>
    /// public static void Main(string[] args)
    /// {
    ///  ProfilerBatchResult result;
    ///  using (Profiler.LogTimeScoped(out result))
    ///  {
    ///    FibonacciSeries(0, 1, 1, 100);
    ///  }
    /// 
    ///  // Print a preformatted summary
    ///  Console.Write(result.Summary);
    /// 
    ///  // Print the average duration
    ///  Console.Write(result.AverageDuration);
    /// }
    /// 
    /// public static void FibonacciSeries(int firstNumber, int secondNumber, int counter, int number)
    /// {
    ///   Console.Write(firstNumber + " ");
    ///   if (counter &lt; number)
    ///   {
    ///     FibonacciSeries(secondNumber, firstNumber + secondNumber, counter + 1, number);
    ///   }
    /// }
    /// </code>
    /// 
    /// Output:
    /// <code>
    ///           ╭─────────────────────────────────────────────────────────────╮
    ///           │ Target:                                                     │
    ///           │───────────────┬────────────────────────────┬────────────────│
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
    /// </code>
    /// </example>
    public static IDisposable LogTimeScoped(out ProfilerBatchResult result, [CallerMemberName] string scopeName = "", [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
      => LogTimeScoped(null, out result, scopeName, sourceFileName, lineNumber);

    public static ProfilerBuilder LogType<TTarget>()
      => new ProfilerBuilder(typeof(TTarget));
    public static ProfilerBuilder LogType(Type targetType)
      => new ProfilerBuilder(targetType);

    internal static void BuildSummaryHeader(StringBuilder outputBuilder, string title, string callerName, string sourceFileName, int lineNumber)
    {
      sourceFileName = Path.GetFileName(sourceFileName);
      string headerCol1 = "Iteration #";
      string headerCol2 = "Duration [ms]";
      string headerCol3 = "Is cancelled";
      _ = outputBuilder
        .AppendLine($"{"╭",11}─────────────────────────────────────────────────────────────╮")
        .AppendLine($"{"│",11} {title,-59} │")
        .AppendLine($"{"│",11} Caller: {callerName,-59} │")
        .AppendLine($"{"│",11} File: {sourceFileName,-59} │")
        .AppendLine($"{"│",11} Line: {lineNumber,-59} │")
        .AppendLine($"{"├",11}───────────────┬────────────────────────────┬────────────────┤")
        .AppendLine($"{"│",11} {headerCol1,-13} │ {headerCol2,-26} │ {headerCol3,-14} │")
        .AppendLine($"{"┝",11}━━━━━━━━━━━━━━━┿━━━━━━━━━━━━━━━━━━━━━━━━━━━━┿━━━━━━━━━━━━━━━━┥");
    }

    internal static void BuildSummaryEntry(StringBuilder outputBuilder, ProfilerResult iterationResult) => outputBuilder.AppendLine($"{"│",11} {iterationResult.Iteration,-13:N0} │ {iterationResult.ElapsedTime.TotalMilliseconds,26:N6} │ {iterationResult.IsProfiledTaskCancelled + (iterationResult.IsProfiledTaskCancelled ? " (ignored)" : ""),-14} │");

    internal static void BuildSummaryFooter(StringBuilder outputBuilder, ProfilerBatchResult batchResult) => _ = outputBuilder
        .AppendLine($"╭═════════╧═══════════════╪════════════════════════════┼────────────────┤")
        .AppendLine($"{"│ Total:",-11} {"-",-13} │ {batchResult.TotalDuration.TotalMilliseconds,26:N6} │{"│",17}")
        .AppendLine($"{"│ Min:",-11} {(batchResult.MinResult.Iteration < 0 ? "-" : batchResult.MinResult.Iteration.ToString("N0")),-13:N0} │ {(batchResult.MinResult.Iteration < 0 ? "-" : batchResult.MinResult.ElapsedTime.TotalMilliseconds.ToString("N6")),26:N6} │{"│",17}")
        .AppendLine($"{"│ Max:",-11} {(batchResult.MaxResult.Iteration < 0 ? "-" : batchResult.MaxResult.Iteration.ToString("N0")),-13:N0} │ {(batchResult.MaxResult.Iteration < 0 ? "-" : batchResult.MaxResult.ElapsedTime.TotalMilliseconds.ToString("N6")),26:N6} │{"│",17}")
        .AppendLine($"{"│ Average:",-11} {"-",-13} │ {batchResult.AverageDuration.TotalMilliseconds,26:N6} │{"│",17}")
        .AppendLine($"╰─────────────────────────┴────────────────────────────┴────────────────╯");
  }
}
