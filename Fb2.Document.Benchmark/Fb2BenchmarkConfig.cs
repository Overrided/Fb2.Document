using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;

namespace Fb2.Document.Benchmark;

public class Fb2BenchmarkConfig : ManualConfig
{
    public Fb2BenchmarkConfig() =>
        AddJob(Job.Default
            .WithRuntime(CoreRuntime.Core90)
            .WithWarmupCount(5)
            .WithLaunchCount(10)
            .WithIterationCount(1000)
            .WithToolchain(InProcessNoEmitToolchain.Instance))
        .AddDiagnoser(MemoryDiagnoser.Default, ThreadingDiagnoser.Default, ExceptionDiagnoser.Default)
        .AddLogger(ConsoleLogger.Default)
        .AddAnalyser(OutliersAnalyser.Default, EnvironmentAnalyser.Default)
        .AddColumn(StatisticColumn.AllStatistics);
}
