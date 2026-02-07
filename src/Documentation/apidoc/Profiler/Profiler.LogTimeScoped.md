---
uid: BionicCode.Utilities.Net.Profiler.LogTimeScoped(BionicCode.Utilities.Net.ProfilerBatchResult@,BionicCode.Utilities.Net.TimeUnit,System.String,System.String,System.Int32)
example: [*content]
---
> [!Note]
> It is recommended to enclose the profiled code section in a `using` block. That clarifies the scope. 
> If you chose to avoid the `using` block you have to explictily call `Dispose()` on the `IDisposable` that was returned
> from the `Profiler.LogTimeScoped` call.

[!code-csharp[](../../../BionicCode.Utilities.Net.Common/Examples/ProfilerExamples/LogTimeScoped.cs#CodeWithoutNamespace)]
[!code-csharp[](../../../BionicCode.Utilities.Net.Common/Examples/ProfilerExamples/BenchmarkTargetNoAttributes.cs#CodeWithoutNamespace)]