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

  /// <summary>
  /// Helper methods to measure code execution time.
  /// </summary>
  public static class Profiler
  {
    /// <summary>
    /// The default value used by the <see cref="Profiler"/> to execute warmup iterations (to initilaize the JIT compiler).
    /// </summary>
    /// <remarks>Use the appropriate overloads of the profiler methods or define a <see cref="ProfilerOptions"/> object to change the default behavior.</remarks>
    public const int DefaultWarmupCount = 10;

    /// <summary>
    /// The default base unit that all results are transformed to.
    /// </summary>
    /// <remarks>
    /// Internally time is recorded in microseconds and then converted to the desired base unit (e.g., ms). The usual timer resolution on most modern machines is 100 ns (see <see cref="TimeSpan.Ticks"/> to learn more. Call the static <see cref="Stopwatch.IsHighResolution"/> field to know if the executing machine supports that high resolution.)
    /// <para>
    /// Use the appropriate overloads of the profiler methods or define a <see cref="ProfilerOptions"/> object to change the default behavior.
    /// </para>
    /// </remarks>
    public const TimeUnit DefaultBaseUnit = TimeUnit.Milliseconds;

    /// <summary>
    /// The default number of iterations to execute the profiled code.
    /// </summary>
    /// <remarks>
    /// Use the appropriate overloads of the profiler methods or define a <see cref="ProfilerOptions"/> object to change the default behavior.
    /// </remarks>
    public const int DefaultIterationsCount = 1000;

    //// Shipped
    ///// <summary>
    ///// Measures the execution time of a method using a default warmup iteration count of '4' to warmup the JIT compiler.
    ///// </summary>
    ///// <param name="action">The code to measure execution time.</param>
    ///// <returns>The average execution time of all <paramref name="runCount"/> number of iterations as <see cref="TimeSpan"/>.</returns>
    ///// <param name="runCount">Number of iterations the <paramref name="action"/> should be executed.</param>
    ///// <param name="sourceFileName">The source file path of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    ///// <param name="lineNumber">The line in the source file of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    ///// <exception cref="ArgumentOutOfRangeException">Thrown when the value of<paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    ///// <remarks>
    ///// <para>
    ///// Use the <see cref="LogTime(Action, int, ProfilerLoggerDelegate, TimeUnit, string, int)"/> and provide an instance of the <see cref="ProfilerLoggerDelegate"/>  to enable logging.
    ///// <br/>Or use the <see cref="LogTimeAsync(Action, int, ProfilerLoggerAsyncDelegate, string, int)"/> overload and provide a <see cref="ProfilerLoggerAsyncDelegate"/> delegate to enable asynchronous logging (for example, use asnchronous API to write the results to a file).
    ///// </para>
    ///// <para>
    ///// The results are calculated using the default time base defined by <see cref="DefaultBaseUnit"/>.
    ///// Use the <see cref="LogTime(Action, ProfilerOptions, string, int)"/> overload to customize the behavior, e.g. time base or the number of warmup iterations.
    ///// </para>
    ///// </remarks>
    //public static ProfilerBatchResult LogTime(Action action, int runCount, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
    //  => LogTimeInternal(action, Profiler.DefaultWarmupCount, runCount, -1, null, Profiler.DefaultBaseUnit, sourceFileName, lineNumber);

    ///// <summary>
    ///// Measures the execution time of a method using a default warmup iteration count of '4' to warmup the JIT compiler.
    ///// </summary>
    ///// <param name="action">The code to measure execution time.</param>
    ///// <returns>The average execution time of all <paramref name="runCount"/> number of iterations as <see cref="TimeSpan"/>.</returns>
    ///// <param name="runCount">Number of iterations the <paramref name="action"/> should be executed.</param>
    ///// <param name="baseUnit">The optional time unit that theresults should be converted to. The default is <see cref="TimeUnit.Milliseconds"/>. </param>
    ///// <param name="sourceFileName">The source file path of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    ///// <param name="lineNumber">The line in the source file of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    ///// <param name="logger">A delegate of type <see cref="ProfilerLoggerDelegate"/> which can be used to automatically print the <see cref="ProfilerBatchResult.Summary" /> to a destination (e.g. file) in the following formatting:
    ///// 
    ///// <br/>
    ///// <code>
    /////           ╭───────────────┬────────────────────────────┬────────────────╮
    /////           | Iteration #   │ Duration [ms]              │ Is cancelled   |
    /////           ┝━━━━━━━━━━━━━━━┿━━━━━━━━━━━━━━━━━━━━━━━━━━━━┿━━━━━━━━━━━━━━━━┥
    /////           │ 1             │                     1.0512 │ False          |
    /////           │ 2             │                     1.0020 │ False          │
    /////           │ 3             │                     0.8732 │ False          │
    /////           │ 4             │                     0.0258 │ True (ignored) │
    /////           │ 5             │                     0.9943 │ False          │
    ///// ╭═════════╧═══════════════╪════════════════════════════┼────────────────┤
    ///// │ Total:    -             │                     3.9207 │                │
    ///// │ Min:      3             │                     0.8732 │                │
    ///// │ Max:      1             │                     1.0512 │                │
    ///// │ Average:  -             │                     0.9802 │                │
    ///// ╰─────────────────────────┴────────────────────────────┴────────────────╯
    ///// </code></param>
    ///// <exception cref="ArgumentOutOfRangeException">Thrown when the value of<paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    ///// <remarks>
    ///// <para>
    ///// Provide an instance of the <see cref="ProfilerLoggerDelegate"/> delegate for the <paramref name="logger"/> parameter to control the output invocationTarget or customize the formatting.
    ///// <br/> Use the <see cref="LogTimeAsync(Action, AsyncProfilerOptions, string, int)"/> or <see cref="LogTimeAsync(Action, int, ProfilerLoggerAsyncDelegate, string, int)"/> overload to enable asynchronous logging.
    ///// </para>
    ///// <para>
    ///// The results are calculated using the default time base defined by <see cref="DefaultBaseUnit"/>.
    ///// Use the <see cref="LogTime(Action, ProfilerOptions, string, int)"/> overload to customize the behavior, e.g. time base or the number of warmup iterations.
    ///// </para>
    ///// </remarks>
    //public static ProfilerBatchResult LogTime(Action action, int runCount, ProfilerLoggerDelegate logger, TimeUnit baseUnit = Profiler.DefaultBaseUnit, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
    //  => LogTimeInternal(action, Profiler.DefaultWarmupCount, runCount, -1, logger, baseUnit, sourceFileName, lineNumber);

    ///// <summary>
    ///// Measures the execution time of a method using a default warmup iteration count of '4' to warmup the JIT compiler.
    ///// </summary>
    ///// <param name="asyncAction">A delegate that executes the asynchronous code to measure execution time.</param>
    ///// <param name="runCount">Number of iterations the <paramref name="asyncAction"/> should be executed.</param>
    ///// <param name="sourceFileName">The source file path of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    ///// <param name="lineNumber">The line in the source file of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    ///// <exception cref="ArgumentOutOfRangeException">Thrown when the value of <paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    ///// <returns>A <see cref="Task"/> holding the average execution time of all <paramref name="runCount"/> number of iterations as <see cref="TimeSpan"/>.</returns> 
    ///// <remarks>
    ///// <para>Cancelled tasks are ignored when calculating the result (although the cancelled runs are listed in the <see cref="ProfilerBatchResult.Summary"/>, but marked as cancelled).
    ///// <br/>A cancelled task is a <see cref="Task"/> where the <see cref="Task.Status"/> returns either <see cref="TaskStatus.Canceled"/> or <see cref="TaskStatus.Faulted"/> or an <see cref="OperationCanceledException"/> exception was thrown.</para>
    ///// Use the <see cref="LogTimeAsync(Func{Task}, int, ProfilerLoggerAsyncDelegate, string, int)"/> or <see cref="LogTimeAsync(Func{Task}, AsyncProfilerOptions, string, int)"/> overload and provide an instance of the <see cref="ProfilerLoggerAsyncDelegate"/> to enable asynchronous logging.
    ///// <para>
    ///// The results are calculated using the default time base defined by <see cref="DefaultBaseUnit"/>.
    ///// Use the <see cref="LogTimeAsync(Func{Task}, AsyncProfilerOptions, string, int)"/> overload to customize the behavior, e.g. time base or the number of warmup iterations.
    ///// </para>
    ///// </remarks>
    //public static async Task<ProfilerBatchResult> LogTimeAsync(Func<Task> asyncAction, int runCount, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
    //  => await LogTimeAsyncInternal(asyncAction, null, null, Profiler.DefaultWarmupCount, runCount, -1, null, null, Profiler.DefaultBaseUnit, sourceFileName, lineNumber);

    ///// <summary>
    ///// Measures the execution time of a method using a default warmup iteration count of '4' to warmup the JIT compiler.
    ///// </summary>
    ///// <param name="asyncAction">A delegate that executes the asynchronous code to measure execution time.</param>
    ///// <param name="runCount">Number of iterations the <paramref name="asyncAction"/> should be executed.</param>
    ///// <param name="sourceFileName">The source file path of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    ///// <param name="lineNumber">The line in the source file of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    ///// <param name="logger">A delegate of type <see cref="ProfilerLoggerDelegate"/> which can be used to automatically print the <see cref="ProfilerBatchResult.Summary"/> to a destination (e.g. file) in the following formatting:
    ///// 
    ///// <br/>
    ///// <code>
    /////           ╭───────────────┬────────────────────────────┬────────────────╮
    /////           | Iteration #   │ Duration [ms]              │ Is cancelled   |
    /////           ┝━━━━━━━━━━━━━━━┿━━━━━━━━━━━━━━━━━━━━━━━━━━━━┿━━━━━━━━━━━━━━━━┥
    /////           │ 1             │                     1.0512 │ False          |
    /////           │ 2             │                     1.0020 │ False          │
    /////           │ 3             │                     0.8732 │ False          │
    /////           │ 4             │                     0.0258 │ True (ignored) │
    /////           │ 5             │                     0.9943 │ False          │
    ///// ╭═════════╧═══════════════╪════════════════════════════┼────────────────┤
    ///// │ Total:    -             │                     3.9207 │                │
    ///// │ Min:      3             │                     0.8732 │                │
    ///// │ Max:      1             │                     1.0512 │                │
    ///// │ Average:  -             │                     0.9802 │                │
    ///// ╰─────────────────────────┴────────────────────────────┴────────────────╯
    ///// </code></param>
    ///// <exception cref="ArgumentOutOfRangeException">Thrown when the value of <paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    ///// <returns>A <see cref="Task"/> holding the <see cref="ProfilerBatchResult"/> result which contains neta data like average execution time or a formatted report (<see cref="ProfilerBatchResult.Summary"/>) of all <paramref name="runCount"/> number of iterations.</returns>
    ///// <remarks>
    ///// <para>
    ///// Cancelled tasks are ignored when calculating the result (although the cancelled runs are listed in the <see cref="ProfilerBatchResult.Summary"/>, but marked as cancelled).
    ///// <br/>A cancelled task is a <see cref="Task"/> where the <see cref="Task.Status"/> returns either <see cref="TaskStatus.Canceled"/> or <see cref="TaskStatus.Faulted"/> or an <see cref="OperationCanceledException"/> exception was thrown.
    ///// </para>
    ///// <para>
    ///// The results are calculated using the default time base defined by <see cref="DefaultBaseUnit"/>.
    ///// Use the <see cref="LogTimeAsync(Func{Task}, AsyncProfilerOptions, string, int)"/> overload to customize the behavior, e.g. time base or the number of warmup iterations.
    ///// </para>
    ///// </remarks>
    //public static async Task<ProfilerBatchResult> LogTimeAsync(Func<Task> asyncAction, int runCount, ProfilerLoggerDelegate logger, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
    //  => await LogTimeAsyncInternal(asyncAction, null, null, Profiler.DefaultWarmupCount, runCount, -1, logger, null, Profiler.DefaultBaseUnit, sourceFileName, lineNumber);

    ///// <summary>
    ///// Measures the execution time of a method using a default warmup iteration count of '4' to warmup the JIT compiler.
    ///// </summary>
    ///// <param name="action">The code to measure execution time.</param>
    ///// <param name="options">The options object to customize the behavior of the <see cref="Profiler"/>.</param>
    ///// <returns>The result containing the average execution time of all iterations as <see cref="TimeSpan"/>.</returns>
    ///// <param name="sourceFileName">The source file path of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    ///// <param name="lineNumber">The line in the source file of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    ///// <exception cref="ArgumentOutOfRangeException">Thrown when the value of <see cref="ProfilerOptions.Iterations"/> or <see cref="ProfilerOptions.WarmupIterations"/> is not between '0' and 'ulong.MaxValue'.</exception>
    ///// <remarks>
    ///// The results are calculated using the default time base defined by <see cref="DefaultBaseUnit"/>.
    ///// <para>
    ///// Use the <see cref="LogTime(Action, int, ProfilerLoggerDelegate, TimeUnit, string, int)"/> and provide an instance of the <see cref="ProfilerLoggerDelegate"/>  to enable logging.
    ///// <br/>Or use the <see cref="LogTimeAsync(Action, int, ProfilerLoggerAsyncDelegate, string, int)"/> overload and provide a <see cref="ProfilerLoggerAsyncDelegate"/> delegate to enable asynchronous logging (for example, use asnchronous API to write the results to a file).
    ///// </para>
    ///// </remarks>
    //public static ProfilerBatchResult LogTime(Action action, ProfilerOptions options, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
    //  => LogTimeInternal(action, options.WarmupIterations, options.Iterations, -1, options.Logger, options.BaseUnit, sourceFileName, lineNumber);

    ///// <summary>
    ///// Measures the execution time of a method using a default warump iteration cvount of '4' to warmup the JIT compiler.
    ///// </summary>
    ///// <param name="action">The code to measure execution time.</param>
    ///// <param name="options">The options object to customize the behavior of the <see cref="Profiler"/>.</param>
    ///// <returns>The profiling result.</returns>
    ///// <param name="sourceFileName">The source file path of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    ///// <param name="lineNumber">The line in the source file of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    ///// 
    ///// <exception cref="ArgumentOutOfRangeException">Thrown when the value of <see cref="ProfilerOptions.Iterations"/> or <see cref="ProfilerOptions.WarmupIterations"/> is not between '0' and 'ulong.MaxValue'.</exception>
    //public static async Task<ProfilerBatchResult> LogTimeAsync(Action action, AsyncProfilerOptions options, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
    //  => await LogTimeAsyncInternal(null, null, action, options.WarmupIterations, options.Iterations, -1, options.Logger, options.AsyncLogger, options.BaseUnit, sourceFileName, lineNumber);

    ///// <summary>
    ///// Measures the execution time of a method using a default warump iteration cvount of '4' to warmup the JIT compiler.
    ///// </summary>
    ///// <param name="action">The code to measure execution time.</param>
    ///// <returns>The average execution time of all <paramref name="runCount"/> number of iterations as <see cref="TimeSpan"/>.</returns>
    ///// <param name="runCount">Number of iterations the <paramref name="action"/> should be executed.</param>
    ///// <param name="sourceFileName">The source file path of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    ///// <param name="lineNumber">The line in the source file of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    ///// <param name="asyncLogger">A delegate of type <see cref="ProfilerLoggerAsyncDelegate"/> which can be used to automatically print the <see cref="ProfilerBatchResult.Summary"/> to a destination (e.g. file) in the following formatting:
    ///// 
    ///// <br/>
    ///// <code>
    /////           ╭───────────────┬────────────────────────────┬────────────────╮
    /////           | Iteration #   │ Duration [ms]              │ Is cancelled   |
    /////           ┝━━━━━━━━━━━━━━━┿━━━━━━━━━━━━━━━━━━━━━━━━━━━━┿━━━━━━━━━━━━━━━━┥
    /////           │ 1             │                     1.0512 │ False          |
    /////           │ 2             │                     1.0020 │ False          │
    /////           │ 3             │                     0.8732 │ False          │
    /////           │ 4             │                     0.0258 │ True (ignored) │
    /////           │ 5             │                     0.9943 │ False          │
    ///// ╭═════════╧═══════════════╪════════════════════════════┼────────────────┤
    ///// │ Total:    -             │                     3.9207 │                │
    ///// │ Min:      3             │                     0.8732 │                │
    ///// │ Max:      1             │                     1.0512 │                │
    ///// │ Average:  -             │                     0.9802 │                │
    ///// ╰─────────────────────────┴────────────────────────────┴────────────────╯
    ///// </code></param>
    ///// <exception cref="ArgumentOutOfRangeException">Thrown when the value of<paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    ///// <remarks>
    ///// Provide an instance of the <see cref="ProfilerLoggerAsyncDelegate"/> delegate for the <paramref name="asyncLogger"/> parameter to control the output invocationTarget or customize the formatting.   
    ///// <para>
    ///// The results are calculated using the default time base defined by <see cref="DefaultBaseUnit"/>.
    ///// Use the <see cref="LogTimeAsync(Action, AsyncProfilerOptions, string, int)"/> overload to customize the behavior, e.g. time base or the number of warmup iterations.
    ///// </para>
    ///// </remarks>
    //public static async Task<ProfilerBatchResult> LogTimeAsync(Action action, int runCount, ProfilerLoggerAsyncDelegate asyncLogger, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
    //  => await LogTimeAsyncInternal(null, null, action, Profiler.DefaultWarmupCount, runCount, -1, null, asyncLogger, Profiler.DefaultBaseUnit, sourceFileName, lineNumber);

    ///// <summary>
    ///// Measures the execution time of a method using a default warmup iteration count of '4' to warmup the JIT compiler.
    ///// </summary>
    ///// <param name="options">The options object to customize the behavior of the profiler.</param>
    ///// <param name="asyncAction">A delegate that executes the asynchronous code to measure execution time.</param>
    ///// <param name="sourceFileName">The source file path of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    ///// <param name="lineNumber">The line in the source file of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    ///// <exception cref="ArgumentOutOfRangeException">Thrown when the value of <see cref="ProfilerOptions.Iterations"/> or <see cref="ProfilerOptions.WarmupIterations"/> is not between '0' and 'ulong.MaxValue'.</exception>
    ///// <returns>The profiling result.</returns> 
    ///// <remarks>
    ///// <para>Cancelled tasks are ignored when calculating the result (although the cancelled runs are listed in the <see cref="ProfilerBatchResult.Summary"/>, but marked as cancelled).
    ///// <br/>A cancelled task is a <see cref="Task"/> where the <see cref="Task.Status"/> returns either <see cref="TaskStatus.Canceled"/> or <see cref="TaskStatus.Faulted"/> or an <see cref="OperationCanceledException"/> exception was thrown.</para>
    ///// </remarks>
    //public static async Task<ProfilerBatchResult> LogTimeAsync(Func<Task> asyncAction, AsyncProfilerOptions options, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
    //  => await LogTimeAsyncInternal(asyncAction, null, null, options.WarmupIterations, options.Iterations, -1, options.Logger, options.AsyncLogger, options.BaseUnit, sourceFileName, lineNumber);

    ///// <summary>
    ///// Measures the execution time of a method using a default warmup iteration count of '4' to warmup the JIT compiler.
    ///// </summary>
    ///// <param name="asyncAction">A delegate that executes the asynchronous code to measure execution time.</param>
    ///// <param name="options">The options object to customize the behavior of the profiler.</param>
    ///// <param name="sourceFileName">The source file path of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    ///// <param name="lineNumber">The line in the source file of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    ///// <exception cref="ArgumentOutOfRangeException">Thrown when the value of <see cref="ProfilerOptions.Iterations"/> or <see cref="ProfilerOptions.WarmupIterations"/> is not between '0' and 'ulong.MaxValue'.</exception>
    ///// <returns>The profiling result.</returns> 
    ///// <remarks>
    ///// <para>Cancelled tasks are ignored when calculating the result (although the cancelled runs are listed in the <see cref="ProfilerBatchResult.Summary"/>, but marked as cancelled).
    ///// <br/>A cancelled task is a <see cref="Task"/> where the <see cref="Task.Status"/> returns either <see cref="TaskStatus.Canceled"/> or <see cref="TaskStatus.Faulted"/> or an <see cref="OperationCanceledException"/> exception was thrown.</para>
    ///// </remarks>
    //public static async Task<ProfilerBatchResult> LogTimeAsync(Func<ValueTask> asyncAction, AsyncProfilerOptions options, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
    //  => await LogTimeAsyncInternal(null, asyncAction, null, options.WarmupIterations, options.Iterations, -1, options.Logger, options.AsyncLogger, options.BaseUnit, sourceFileName, lineNumber);

    ///// <summary>
    ///// Measures the execution time of a method using a default warmup iteration count of '4' to warmup the JIT compiler.
    ///// </summary>
    ///// <param name="asyncAction">A delegate that executes the asynchronous code to measure execution time.</param>
    ///// <param name="runCount">Number of iterations the <paramref name="asyncAction"/> should be executed.</param>
    ///// <param name="sourceFileName">The source file path of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    ///// <param name="lineNumber">The line in the source file of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    ///// <exception cref="ArgumentOutOfRangeException">Thrown when the value of <paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    ///// <returns>A <see cref="Task"/> holding the average execution time of all <paramref name="runCount"/> number of iterations as <see cref="TimeSpan"/>.</returns> 
    ///// <remarks>
    ///// <para>
    ///// Cancelled tasks are ignored when calculating the result (although the cancelled runs are listed in the <see cref="ProfilerBatchResult.Summary"/>, but marked as cancelled).
    ///// <br/>A cancelled task is a <see cref="Task"/> where the <see cref="Task.Status"/> returns either <see cref="TaskStatus.Canceled"/> or <see cref="TaskStatus.Faulted"/> or an <see cref="OperationCanceledException"/> exception was thrown.
    ///// </para>
    ///// Use the <see cref="LogTimeAsync(Func{ValueTask}, int, ProfilerLoggerAsyncDelegate, string, int)"/> or <see cref="LogTimeAsync(Func{ValueTask}, AsyncProfilerOptions, string, int)"/> overload and provide an instance of the <see cref="ProfilerLoggerAsyncDelegate"/> delegate to enable logging.
    ///// <para>
    ///// The results are calculated using the default time base defined by <see cref="DefaultBaseUnit"/>.
    ///// Use the <see cref="LogTimeAsync(Func{ValueTask}, AsyncProfilerOptions, string, int)"/> overload to customize the behavior, e.g. time base or the number of warmup iterations.
    ///// </para>
    ///// </remarks>
    //public static async Task<ProfilerBatchResult> LogTimeAsync(Func<ValueTask> asyncAction, int runCount, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
    //  => await LogTimeAsyncInternal(null, asyncAction, null, Profiler.DefaultWarmupCount, runCount, -1, null, null, Profiler.DefaultBaseUnit, sourceFileName, lineNumber);

    ///// <summary>
    ///// Measures the execution time of a method using a default warmup iteration count of '4' to warmup the JIT compiler.
    ///// </summary>
    ///// <param name="asyncAction">A delegate that executes the asynchronous code to measure execution time.</param>
    ///// <param name="runCount">Number of iterations the <paramref name="asyncAction"/> should be executed.</param>
    ///// <param name="sourceFileName">The source file path of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    ///// <param name="lineNumber">The line in the source file of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    ///// <param name="logger">A delegate of type <see cref="ProfilerLoggerDelegate"/> which can be used to automatically print the <see cref="ProfilerBatchResult.Summary"/> to a destination (e.g. file) in the following formatting:
    ///// 
    ///// <br/>
    ///// <code>
    /////           ╭───────────────┬────────────────────────────┬────────────────╮
    /////           | Iteration #   │ Duration [ms]              │ Is cancelled   |
    /////           ┝━━━━━━━━━━━━━━━┿━━━━━━━━━━━━━━━━━━━━━━━━━━━━┿━━━━━━━━━━━━━━━━┥
    /////           │ 1             │                     1.0512 │ False          |
    /////           │ 2             │                     1.0020 │ False          │
    /////           │ 3             │                     0.8732 │ False          │
    /////           │ 4             │                     0.0258 │ True (ignored) │
    /////           │ 5             │                     0.9943 │ False          │
    ///// ╭═════════╧═══════════════╪════════════════════════════┼────────────────┤
    ///// │ Total:    -             │                     3.9207 │                │
    ///// │ Min:      3             │                     0.8732 │                │
    ///// │ Max:      1             │                     1.0512 │                │
    ///// │ Average:  -             │                     0.9802 │                │
    ///// ╰─────────────────────────┴────────────────────────────┴────────────────╯
    ///// </code></param>
    ///// <exception cref="ArgumentOutOfRangeException">Thrown when the value of <paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    ///// <returns>A <see cref="Task"/> holding the <see cref="ProfilerBatchResult"/> result which contains neta data like average execution time or a formatted report (<see cref="ProfilerBatchResult.Summary"/>) of all <paramref name="runCount"/> number of iterations.</returns>
    ///// <remarks>
    ///// <para>
    ///// Cancelled tasks are ignored when calculating the result (although the cancelled runs are listed in the <see cref="ProfilerBatchResult.Summary"/>, but marked as cancelled).
    ///// <br/>A cancelled task is a <see cref="Task"/> where the <see cref="Task.Status"/> returns either <see cref="TaskStatus.Canceled"/> or <see cref="TaskStatus.Faulted"/> or an <see cref="OperationCanceledException"/> exception was thrown.
    ///// </para>
    ///// <para>
    ///// The results are calculated using the default time base defined by <see cref="DefaultBaseUnit"/>.
    ///// Use the <see cref="LogTimeAsync(Func{ValueTask}, AsyncProfilerOptions, string, int)"/> overload to customize the behavior, e.g. time base or the number of warmup iterations.
    ///// </para>
    ///// </remarks>
    //public static async Task<ProfilerBatchResult> LogTimeAsync(Func<ValueTask> asyncAction, int runCount, ProfilerLoggerDelegate logger, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
    //  => await LogTimeAsyncInternal(null, asyncAction, null, Profiler.DefaultWarmupCount, runCount, -1, logger, null, Profiler.DefaultBaseUnit, sourceFileName, lineNumber);

    ///// <summary>
    ///// Measures the execution time of a method using a default warmup iteation count of '4' to wramup the JIT compiler.
    ///// </summary>
    ///// <param name="asyncAction">A delegate that executes the asynchronous code to measure execution time.</param>
    ///// <param name="runCount">Number of iterations the <paramref name="asyncAction"/> should be executed.</param>
    ///// <param name="sourceFileName">The source file path of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    ///// <param name="lineNumber">The line in the source file of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    ///// <param name="asyncLogger">An asynchronous delegate of type <see cref="ProfilerLoggerAsyncDelegate"/> which can be used to automatically let the <see cref="LogTimeAsync(Func{Task}, int, int, ProfilerLoggerAsyncDelegate, TimeUnit, string, int)"/> print the <see cref="ProfilerBatchResult.Summary"/> to a destination (e.g. file) in the following formatting:
    ///// 
    ///// <br/>
    ///// <code>
    /////           ╭───────────────┬────────────────────────────┬────────────────╮
    /////           | Iteration #   │ Duration [ms]              │ Is cancelled   |
    /////           ┝━━━━━━━━━━━━━━━┿━━━━━━━━━━━━━━━━━━━━━━━━━━━━┿━━━━━━━━━━━━━━━━┥
    /////           │ 1             │                     1.0512 │ False          |
    /////           │ 2             │                     1.0020 │ False          │
    /////           │ 3             │                     0.8732 │ False          │
    /////           │ 4             │                     0.0258 │ True (ignored) │
    /////           │ 5             │                     0.9943 │ False          │
    ///// ╭═════════╧═══════════════╪════════════════════════════┼────────────────┤
    ///// │ Total:    -             │                     3.9207 │                │
    ///// │ Min:      3             │                     0.8732 │                │
    ///// │ Max:      1             │                     1.0512 │                │
    ///// │ Average:  -             │                     0.9802 │                │
    ///// ╰─────────────────────────┴────────────────────────────┴────────────────╯
    ///// </code></param>
    ///// <exception cref="ArgumentOutOfRangeException">Thrown when the value of<paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    ///// <returns>A <see cref="Task"/> holding the <see cref="ProfilerBatchResult"/> result which contains neta data like average execution time or a formatted report (<see cref="ProfilerBatchResult.Summary"/>) of all <paramref name="runCount"/> number of iterations.</returns>
    ///// <remarks>
    ///// <para>
    ///// Cancelled tasks are ignored when calculating the result (although the cancelled runs are listed in the <see cref="ProfilerBatchResult.Summary"/>, but marked as cancelled).
    ///// <br/>A cancelled task is a <see cref="Task"/> where the <see cref="Task.Status"/> returns either <see cref="TaskStatus.Canceled"/> or <see cref="TaskStatus.Faulted"/> or an <see cref="OperationCanceledException"/> exception was thrown.
    ///// </para>
    ///// <para>
    ///// The results are calculated using the default time base defined by <see cref="DefaultBaseUnit"/>.
    ///// Use the <see cref="LogTimeAsync(Func{Task}, AsyncProfilerOptions, string, int)"/> overload to customize the behavior, e.g. time base or the number of warmup iterations.
    ///// </para>
    ///// </remarks>
    //public static async Task<ProfilerBatchResult> LogTimeAsync(Func<Task> asyncAction, int runCount, ProfilerLoggerAsyncDelegate asyncLogger, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
    //  => await LogTimeAsyncInternal(asyncAction, null, null, Profiler.DefaultWarmupCount, runCount, -1, null, asyncLogger, Profiler.DefaultBaseUnit, sourceFileName, lineNumber);

    ///// <summary>
    ///// Measures the execution time of a method.
    ///// </summary>
    ///// <param name="asyncAction">A delegate that executes the asynchronous code to measure execution time.</param>
    ///// <param name="runCount">Number of iterations the <paramref name="asyncAction"/> should be executed.</param>
    ///// <param name="sourceFileName">The source file path of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    ///// <param name="lineNumber">The line in the source file of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    ///// <param name="asyncLogger">An asynchronous delegate of type <see cref="ProfilerLoggerAsyncDelegate"/> which can be used to automatically let the <see cref="LogTimeAsync(Func{Task}, int, ProfilerLoggerAsyncDelegate, TimeUnit, string, int)"/> print the <see cref="ProfilerBatchResult.Summary"/> to a destination (e.g. file) in the following formatting:
    ///// 
    ///// <br/>
    ///// <code>
    /////           ╭───────────────┬────────────────────────────┬────────────────╮
    /////           | Iteration #   │ Duration [ms]              │ Is cancelled   |
    /////           ┝━━━━━━━━━━━━━━━┿━━━━━━━━━━━━━━━━━━━━━━━━━━━━┿━━━━━━━━━━━━━━━━┥
    /////           │ 1             │                     1.0512 │ False          |
    /////           │ 2             │                     1.0020 │ False          │
    /////           │ 3             │                     0.8732 │ False          │
    /////           │ 4             │                     0.0258 │ True (ignored) │
    /////           │ 5             │                     0.9943 │ False          │
    ///// ╭═════════╧═══════════════╪════════════════════════════┼────────────────┤
    ///// │ Total:    -             │                     3.9207 │                │
    ///// │ Min:      3             │                     0.8732 │                │
    ///// │ Max:      1             │                     1.0512 │                │
    ///// │ Average:  -             │                     0.9802 │                │
    ///// ╰─────────────────────────┴────────────────────────────┴────────────────╯
    ///// </code></param>
    ///// <exception cref="ArgumentOutOfRangeException">Thrown when the value of<paramref name="runCount"/> is not between '0' and 'ulong.MaxValue'.</exception>
    ///// <returns>A <see cref="Task"/> holding the <see cref="ProfilerBatchResult"/> result which contains neta data like average execution time or a formatted report (<see cref="ProfilerBatchResult.Summary"/>) of all <paramref name="runCount"/> number of iterations.</returns>
    ///// <remarks>
    ///// <para>
    ///// Cancelled tasks are ignored when calculating the result (although the cancelled runs are listed in the <see cref="ProfilerBatchResult.Summary"/>, but marked as cancelled).
    ///// <br/>A cancelled task is a <see cref="Task"/> where the <see cref="Task.Status"/> returns either <see cref="TaskStatus.Canceled"/> or <see cref="TaskStatus.Faulted"/> or an <see cref="OperationCanceledException"/> exception was thrown.
    ///// </para>
    ///// <para>
    ///// The results are calculated using the default time base defined by <see cref="DefaultBaseUnit"/>.
    ///// Use the <see cref="LogTimeAsync(Func{ValueTask}, AsyncProfilerOptions, string, int)"/> overload to customize the behavior, e.g. time base or the number of warmup iterations.
    ///// </para>
    ///// </remarks>
    //public static async Task<ProfilerBatchResult> LogTimeAsync(Func<ValueTask> asyncAction, int runCount, ProfilerLoggerAsyncDelegate asyncLogger, [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
    //  => await LogTimeAsyncInternal(null, asyncAction, null, Profiler.DefaultWarmupCount, runCount, -1, null, asyncLogger, Profiler.DefaultBaseUnit, sourceFileName, lineNumber);

    //internal static async Task<ProfilerBatchResult> LogTimeAsyncInternal(Func<Task> asyncAction, Func<ValueTask> asyncValueTaskAction, Action action, int warmupCount, int runCount, int argumentListIndex, ProfilerLoggerDelegate logger, ProfilerLoggerAsyncDelegate asyncLogger, TimeUnit baseUnit, string sourceFileName, int lineNumber)
    //  => LogTimeAsyncInternal(new ProfilerTargetInvokeInfo())
    //internal static async Task<ProfilerBatchResult> LogTimeAsyncInternal<TResult>(Func<ValueTask<TResult>> asyncValueTaskAction, Action action, int warmupCount, int runCount, int argumentListIndex, ProfilerLoggerDelegate logger, ProfilerLoggerAsyncDelegate asyncLogger, TimeUnit baseUnit, string sourceFileName, int lineNumber)
    //  => LogTimeAsyncInternal(new ProfilerTargetInvokeInfo())

    internal static async Task<ProfilerBatchResult> LogTimeInternalAsync(ProfilerContext context)
    {      
      if (context.IterationCount < 1)
      {
        return ProfilerBatchResult.Empty;
      }

      ProfilerBatchResult result = await LogAverageTimeInternalAsync(context);
      context.Logger?.Invoke(result, result.Summary);
      if (context.AsyncLogger != null)
      {
        await context.AsyncLogger.Invoke(result, result.Summary);
      }

      return result;
    }

    private static async Task<ProfilerBatchResult> LogAverageTimeInternalAsync(ProfilerContext context)
    {
      ProfilerBatchResult result = null;
      ProfiledTargetType profiledTargetType = context.MethodInvokeInfo.ProfiledTargetType;
      if (profiledTargetType is ProfiledTargetType.Method)
      {
        result = await LogMethodAsync(context);
      }
      else if (profiledTargetType is ProfiledTargetType.Constructor)
      {
        result = LogConstructor(context);
      }
      else if (profiledTargetType.HasFlag(ProfiledTargetType.PropertyGet))
      {
        result = LogPropertyGet(context);
      }
      else if (profiledTargetType.HasFlag(ProfiledTargetType.PropertySet))
      {
        result = LogPropertySet(context);
      }

      return result;
    }

    private static async Task<ProfilerBatchResult> LogMethodAsync(ProfilerContext context)
    {
      ProfilerBatchResult result = new ProfilerBatchResult(DateTime.Now, context);
      var stopwatch = new Stopwatch();
      object invocationTarget = context.MethodInvokeInfo.Target;
      object[] arguments = context.MethodInvokeInfo.MethodArgument.Arguments.ToArray();
      bool isTaskCancelled = false;
      for (int iterationCounter = 1 - context.WarmupCount; iterationCounter <= context.IterationCount; iterationCounter++)
      {
        if (context.MethodInvokeInfo.AsynchronousTaskMethodInvocator != null)
        {
          try
          {
            stopwatch.Restart();
            await context.MethodInvokeInfo.AsynchronousTaskMethodInvocator.Invoke(invocationTarget, arguments);
            stopwatch.Stop();
          }
          catch (OperationCanceledException)
          {
            stopwatch.Stop();
            isTaskCancelled = true;
          }
        }
        else if (context.MethodInvokeInfo.AsynchronousValueTaskMethodInvocator != null)
        {
          try
          {
            stopwatch.Restart();
            await context.MethodInvokeInfo.AsynchronousValueTaskMethodInvocator.Invoke(invocationTarget, arguments);
            stopwatch.Stop();
          }
          catch (OperationCanceledException)
          {
            stopwatch.Stop();
            isTaskCancelled = true;
          }
        }
        else if (context.MethodInvokeInfo.AsynchronousGenericValueTaskMethodInvocator != null)
        {
          try
          {
            stopwatch.Restart();
            dynamic profiledValueTask = context.MethodInvokeInfo.AsynchronousGenericValueTaskMethodInvocator.Invoke(invocationTarget, arguments);
            await profiledValueTask;
            stopwatch.Stop();
          }
          catch (OperationCanceledException)
          {
            stopwatch.Stop();
            isTaskCancelled = true;
          }
        }

        if (iterationCounter < 1)
        {
          // Still warming up
          continue;
        }

        var iterationResult = new ProfilerResult(iterationCounter, isTaskCancelled, stopwatch.Elapsed, context.BaseUnit, result, context.MethodInvokeInfo.MethodArgument.ArgumentListIndex);
        result.AddResult(iterationResult);
      }

      return result;
    }

    private static ProfilerBatchResult LogConstructor(ProfilerContext context)
    {
      ProfilerBatchResult result = new ProfilerBatchResult(DateTime.Now, context);
      var stopwatch = new Stopwatch();
      object[] arguments = context.MethodInvokeInfo.MethodArgument.Arguments.ToArray();
      for (int iterationCounter = 1 - context.WarmupCount; iterationCounter <= context.IterationCount; iterationCounter++)
      {
        stopwatch.Restart();
        _ = context.MethodInvokeInfo.ConstructorInvocator.Invoke(arguments);
        stopwatch.Stop();

        if (iterationCounter < 1)
        {
          // Still warming up
          continue;
        }

        var iterationResult = new ProfilerResult(iterationCounter, stopwatch.Elapsed, context.BaseUnit, result, context.MethodInvokeInfo.MethodArgument.ArgumentListIndex);
        result.AddResult(iterationResult);
      }

      return result;
    }

    private static ProfilerBatchResult LogPropertySet(ProfilerContext context)
    {
      ProfilerBatchResult result = new ProfilerBatchResult(DateTime.Now, context);
      var stopwatch = new Stopwatch();
      object[] indexArguments = context.MethodInvokeInfo.ProfiledTargetType.HasFlag(ProfiledTargetType.Indexer) 
        ? context.MethodInvokeInfo.PropertyArgument.Index 
        : null;
      object value = context.MethodInvokeInfo.PropertyArgument.Value;
      object invocationTarget = context.MethodInvokeInfo.Target;
      for (int iterationCounter = 1 - context.WarmupCount; iterationCounter <= context.IterationCount; iterationCounter++)
      {
        stopwatch.Restart();
        context.MethodInvokeInfo.PropertySetInvocator.Invoke(invocationTarget, value, indexArguments);
        stopwatch.Stop();

        if (iterationCounter < 1)
        {
          // Still warming up
          continue;
        }

        var iterationResult = new ProfilerResult(iterationCounter, stopwatch.Elapsed, context.BaseUnit, result, context.MethodInvokeInfo.PropertyArgument.ArgumentListIndex);
        result.AddResult(iterationResult);
      }

      return result;
    }

    private static ProfilerBatchResult LogPropertyGet(ProfilerContext context)
    {
      ProfilerBatchResult result = new ProfilerBatchResult(DateTime.Now, context);
      var stopwatch = new Stopwatch();
      object[] indexArguments = context.MethodInvokeInfo.ProfiledTargetType.HasFlag(ProfiledTargetType.Indexer)
        ? context.MethodInvokeInfo.PropertyArgument.Index
        : null;
      object invocationTarget = context.MethodInvokeInfo.Target;
      for (int iterationCounter = 1 - context.WarmupCount; iterationCounter <= context.IterationCount; iterationCounter++)
      {
        stopwatch.Restart();
        _ = context.MethodInvokeInfo.PropertyGetInvocator.Invoke(invocationTarget, indexArguments);
        stopwatch.Stop();

        if (iterationCounter < 1)
        {
          // Still warming up
          continue;
        }

        var iterationResult = new ProfilerResult(iterationCounter, stopwatch.Elapsed, context.BaseUnit, result, context.MethodInvokeInfo.PropertyArgument.ArgumentListIndex);
        result.AddResult(iterationResult);
      }

      return result;
    }

    /// <summary>
    /// Measures the execution time of a using block i.e. the scope of the <see cref="IDisposable"/>'s lifetime.
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
    /// <param name="result">An <see langword="out"/> parameter holding the <see cref="ProfilerBatchResult"/> result which contains meta data like average execution time or a formatted report (<see cref="ProfilerBatchResult.Summary"/>).</param>
    /// <param name="baseUnit">The optional time unit that theresults should be converted to. The default is <see cref="TimeUnit.Milliseconds"/>. </param>
    /// <param name="scopeName">The name of the scope. This value is automatically captured and set to the caller's member name. Therfore this optional parameter doesn't require an explicit value.</param>
    /// <param name="sourceFileName">The source file path of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    /// <param name="lineNumber">The line in the source file of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    /// <returns>An <see cref="IDisposable"/> to control the scope of the profiling.</returns>
    /// <remarks>
    /// Time is measured during the lifetime of the <see cref="IDisposable"/> instance: from time of creation to the final <see cref="IDisposable.Dispose"/> call.
    /// <br/>It's recommended to use the <see cref="IDisposable"/> managed by a using-statement or using-expression.
    /// <para>
    /// API docs: <see href="https://sampoh.de/github/docs/bioniccode.net/api/BionicCode.Utilities.Net.Profiler.html#BionicCode_Utilities_Net_Profiler_LogTimeScoped_BionicCode_Utilities_Net_ProfilerLoggerDelegate_BionicCode_Utilities_Net_ProfilerBatchResult__">IDisposable LogTimeScoped(ProfilerLoggerDelegate logger, out ProfilerBatchResult result)</see>
    /// </para>
    /// </remarks>
    public static IDisposable LogTimeScoped(Action<ProfilerBatchResult, string> logger, out ProfilerBatchResult result, TimeUnit baseUnit = Profiler.DefaultBaseUnit, [CallerMemberName] string scopeName = "Scoped Profiling", [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
    {
      var assemblyOfTargetType = Assembly.GetCallingAssembly();
      string assemblyName = assemblyOfTargetType.GetName().Name;
      var profilerTargetInfo = new ProfilerTargetInvokeInfo(scopeName, scopeName, scopeName, scopeName, string.Empty, assemblyName);
      var context = new ProfilerContext(profilerTargetInfo, sourceFileName, lineNumber, -1, -1, Runtime.Current, baseUnit, logger, null);
      var profilerScopeProvider = new ProfilerScopeProvider(logger, context);
      IDisposable profilerScope = profilerScopeProvider.StartProfiling(out result);

      return profilerScope;
    }

    /// <summary>
    /// Measures the execution time of a using block i.e. the scope of the <see cref="IDisposable"/>'s lifetime.
    /// </summary>
    /// <param name="asyncLogger">An asynchronous delegate of type <see cref="ProfilerLoggerAsyncDelegate"/> which can be used to automatically print the <see cref="ProfilerBatchResult.Summary"/> to a destination (e.g. file) in the following formatting:
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
    /// <param name="result">An <see langword="out"/> parameter holding the <see cref="ProfilerBatchResult"/> result which contains meta data like average execution time or a formatted report (<see cref="ProfilerBatchResult.Summary"/>).</param>
    /// <param name="baseUnit">The optional time unit that the results should be converted to. The default is <see cref="TimeUnit.Milliseconds"/>. </param>
    /// <param name="scopeName">The name of the scope. This value is automatically captured and set to the caller's member name. Therefore, this optional parameter doesn't require an explicit value.</param>
    /// <param name="sourceFileName">The source file path of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    /// <param name="lineNumber">The line in the source file of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    /// <returns>An <see cref="IAsyncDisposable"/> to control the scope of the profiling and that executes the async logger delegate.</returns>
    /// <remarks>
    /// Time is measured during the lifetime of the <see cref="IAsyncDisposable"/> instance: from time of creation to the final <see cref="IDisposable.Dispose"/> call.
    /// <br/>It's recommended to use the <see cref="IAsyncDisposable"/> managed by a using-statement or using-expression.
    /// <para>
    /// API docs: <see href="https://sampoh.de/github/docs/bioniccode.net/api/BionicCode.Utilities.Net.Profiler.html#BionicCode_Utilities_Net_Profiler_LogTimeScoped_BionicCode_Utilities_Net_ProfilerLoggerAsync_BionicCode_Utilities_Net_ProfilerBatchResult__">IDisposable LogTimeScoped(ProfilerLoggerAsyncDelegate, out ProfilerBatchResult)</see>
    /// </para>
    /// </remarks>
    public static IAsyncDisposable LogTimeScopedAsync(Func<ProfilerBatchResult, string, Task> asyncLogger, out ProfilerBatchResult result, TimeUnit baseUnit = Profiler.DefaultBaseUnit, [CallerMemberName] string scopeName = "Scoped Profiling", [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
    {
      var assemblyOfTargetType = Assembly.GetCallingAssembly();
      string assemblyName = assemblyOfTargetType.GetName().Name;
      var profilerTargetInfo = new ProfilerTargetInvokeInfo(scopeName, scopeName, scopeName, scopeName, string.Empty, assemblyName);
      var context = new ProfilerContext(profilerTargetInfo, sourceFileName, lineNumber, -1, -1, Runtime.Current, baseUnit, null, asyncLogger);
      var profilerScopeProvider = new ProfilerScopeProvider(asyncLogger, context);
      IAsyncDisposable profilerScope = profilerScopeProvider.StartProfilingAsync(out result);

      return profilerScope;
    }

    /// <summary>
    /// Measures the execution time of a using block i.e. the scope of the <see cref="IDisposable"/>'s lifetime.
    /// </summary>
    /// <param name="result">An <see langword="out"/> parameter holding the <see cref="ProfilerBatchResult"/> result which contains meta data like average execution time or a formatted report (<see cref="ProfilerBatchResult.Summary"/>).</param>
    /// <param name="baseUnit">The optional time unit that the results should be converted to. The default is <see cref="TimeUnit.Milliseconds"/>. </param>
    /// <param name="scopeName">The name of the scope. This value is automatically captured and set to the caller's member name. Therefore this optional parameter doesn't require an explicit value.</param>
    /// <param name="sourceFileName">The source file path of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    /// <param name="lineNumber">The line in the source file of the profiled code. This value is automatically captured and therefore doesn't require an explicit value.</param>
    /// <returns>An <see cref="IDisposable"/> to control the scope of the profiling.</returns>
    /// <example></example>
    /// <remarks>
    /// Time is measured during the lifetime of the <see cref="IDisposable"/> instance: from time of creation to the final <see cref="IDisposable.Dispose"/> call.
    /// <br/>It's recommended to use the <see cref="IDisposable"/> managed by a using-statement or using-expression.</remarks>
    public static IDisposable LogTimeScoped(out ProfilerBatchResult result, TimeUnit baseUnit = Profiler.DefaultBaseUnit, [CallerMemberName] string scopeName = "Scoped Profiling", [CallerFilePath] string sourceFileName = "", [CallerLineNumber] int lineNumber = -1)
      => LogTimeScoped(null, out result, baseUnit, scopeName, sourceFileName, lineNumber);

    /// <summary>
    /// Creates the builder object which configures and starts the attribute based profiling only targeting auto discoverable types.
    /// </summary>
    /// <returns>
    /// The builder instance which configures and starts the attribute based profiling.</returns>
    /// <remarks>
    /// To make a type auto discoverable it must b e decorated with the <see cref="ProfilerAutoDiscoverAttribute"/> attribute. 
    /// <para></para>
    /// 
    /// The target member of the profiled type must be decorated with the <see cref="ProfileAttribute"/>. These members don't have to be <see langword="public"/>.
    /// <br/>Use the <see cref="ProfilerMethodArgumentAttribute"/> to define the argument list which is used to invoke the member. The member can have multiple argument lists. Each argument list is invoked for the number of iterations that are set using the <see cref="ProfilerBuilder"/>.
    /// </remarks>
    public static ProfilerBuilder CreateProfilerBuilder() 
      => new ProfilerBuilder().SetIncludeAutoDiscoverableTypes(true);

    /// <summary>
    /// Creates the builder object which configures and starts the attribute based profiling.
    /// </summary>
    /// <typeparam name="TTarget">The type to profile.</typeparam>
    /// <returns>
    /// The builder instance which configures and starts the attribute based profiling.</returns>
    /// <remarks>
    /// The target member of the profiled type must be decorated with the <see cref="ProfileAttribute"/>. These members don't have to be <see langword="public"/>.
    /// <br/>Use the <see cref="ProfilerMethodArgumentAttribute"/> to define the argument list which is used to invoke the member. The member can have multiple argument lists. Each argument list is invoked for the number of iterations that are set using the <see cref="ProfilerBuilder"/>.
    /// </remarks>
    public static ProfilerBuilder CreateProfilerBuilder<TTarget>()
    {
      TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(typeof(TTarget));
      return new ProfilerBuilder(typeData);
    }

    /// <summary>
    /// Creates the builder object which configures and starts the attribute based profiling.
    /// </summary>
    /// <param name="targetType">The type to profile.</param>
    /// <returns>
    /// The builder instance which configures and starts the attribute based profiling.
    /// </returns>
    /// <remarks>
    /// The target member of the profiled type must be decorated with the <see cref="ProfileAttribute"/>. These members don't have to be <see langword="public"/>.
    /// <br/>Use the <see cref="ProfilerMethodArgumentAttribute"/> to define the argument list which is used to invoke the member. The member can have multiple argument lists. Each argument list is invoked for the number of iterations that are set using the <see cref="ProfilerBuilder"/>.
    /// </remarks>
    public static ProfilerBuilder CreateProfilerBuilder(Type targetType)
    {
      if (targetType is null)
      {
        throw new ArgumentNullException(nameof(targetType));
      }

      TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(targetType);
      return new ProfilerBuilder(typeData);
    }

    /// <summary>
    /// Creates the builder object which configures and starts the attribute based profiling.
    /// </summary>
    /// <param name="targetTypes">A list of types to profile in a batch.</param>
    /// <returns>
    /// The builder instance which configures and starts the attribute based profiling.</returns>
    /// <remarks>
    /// The member of the profiled type must be decorated with the <see cref="ProfileAttribute"/>. These members don't have to be <see langword="public"/>.
    /// <br/>Use the <see cref="ProfilerMethodArgumentAttribute"/> to define the argument list which is used to invoke the member. The member can have multiple argument lists. Each argument list is invoked for the number of iterations that are set using the <see cref="ProfilerBuilder"/>.
    /// </remarks>
    public static ProfilerBuilder CreateProfilerBuilder(params Type[] targetTypes)
      => CreateProfilerBuilder((IEnumerable<Type> )targetTypes);

    /// <summary>
    /// Creates the builder object which configures and starts the attribute based profiling.
    /// </summary>
    /// <param name="targetTypes">A list of types to profile in a batch.</param>
    /// <returns>
    /// The builder instance which configures and starts the attribute based profiling.</returns>
    /// <remarks>
    /// The target member of the profiled type must be decorated with the <see cref="ProfileAttribute"/>. These members don't have to be <see langword="public"/>.
    /// <br/>Use the <see cref="ProfilerMethodArgumentAttribute"/> to define the argument list which is used to invoke the member. The member can have multiple argument lists. Each argument list is invoked for the number of iterations that are set using the <see cref="ProfilerBuilder"/>.
    /// </remarks>
    public static ProfilerBuilder CreateProfilerBuilder(IEnumerable<Type> targetTypes)
    {
      var typeData = new List<TypeData>();
      foreach (Type targetType in targetTypes)
      {
        if (targetType is null)
        {
          throw new ArgumentNullException(nameof(targetTypes), "One or more type parameters are NULL.");
        }

        typeData.Add(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(targetType));
      }

      return new ProfilerBuilder(typeData);
    }

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

    internal static void BuildSummaryEntry(StringBuilder outputBuilder, ProfilerResult iterationResult) => outputBuilder.AppendLine($"{"│",11} {iterationResult.Iteration,-13:N0} │ {iterationResult.ElapsedTime,26:N6} │ {iterationResult.IsProfiledTaskCancelled + (iterationResult.IsProfiledTaskCancelled ? " (ignored)" : ""),-14} │");

    internal static void BuildSummaryFooter(StringBuilder outputBuilder, ProfilerBatchResult batchResult) => _ = outputBuilder
        .AppendLine($"╭═════════╧═══════════════╪════════════════════════════┼────────────────┤")
        .AppendLine($"{"│ Total:",-11} {"-",-13} │ {batchResult.TotalDuration,26:N6} │{"│",17}")
        .AppendLine($"{"│ Min:",-11} {(batchResult.MinResult.Iteration < 0 ? "-" : batchResult.MinResult.Iteration.ToString("N0")),-13:N0} │ {(batchResult.MinResult.Iteration < 0 ? "-" : batchResult.MinResult.ElapsedTime.Value.ToString("N6")),26:N6} │{"│",17}")
        .AppendLine($"{"│ Max:",-11} {(batchResult.MaxResult.Iteration < 0 ? "-" : batchResult.MaxResult.Iteration.ToString("N0")),-13:N0} │ {(batchResult.MaxResult.Iteration < 0 ? "-" : batchResult.MaxResult.ElapsedTime.Value.ToString("N6")),26:N6} │{"│",17}")
        .AppendLine($"{"│ Average:",-11} {"-",-13} │ {batchResult.AverageDuration,26:N6} │{"│",17}")
        .AppendLine($"╰─────────────────────────┴────────────────────────────┴────────────────╯");
  }
}
