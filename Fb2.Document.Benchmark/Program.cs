using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;

namespace Fb2.Document.Benchmark;

public class Program
{
    public static void Main(string[] args) => _ = BenchmarkRunner.Run<Fb2DocumentBenchMark>();
}

public class AntiVirusFriendlyConfig : ManualConfig
{
    public AntiVirusFriendlyConfig() =>
        AddJob(Job.Default
            .WithRuntime(CoreRuntime.Core90)
            .WithWarmupCount(5)
            .WithLaunchCount(10)
            .WithIterationCount(1000)
            .WithToolchain(InProcessNoEmitToolchain.Instance))
        .AddDiagnoser(MemoryDiagnoser.Default, ThreadingDiagnoser.Default, ExceptionDiagnoser.Default)
        .AddLogger(ConsoleLogger.Default)
        .AddColumn(StatisticColumn.AllStatistics);
}

[Config(typeof(AntiVirusFriendlyConfig))]
public class Fb2DocumentBenchMark
{
    public Stream? fb2FileContentStream;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var x = Assembly.GetExecutingAssembly();
        var names = x.GetManifestResourceNames();
        fb2FileContentStream = x.GetManifestResourceStream(names[0]); // only one resource
    }

    [IterationSetup]
    public void IterationSetup()
    {
        if (fb2FileContentStream!.Position != 0 && fb2FileContentStream!.CanSeek)
            fb2FileContentStream!.Seek(0, SeekOrigin.Begin);
    }

    [Benchmark]
    public async Task<Fb2Document> LoadDocumentFromStreamAsync()
    {
        var doc = new Fb2Document();
        await doc.LoadAsync(fb2FileContentStream!);
        return doc;
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        if (fb2FileContentStream != null)
        {
            fb2FileContentStream.Close();
            fb2FileContentStream.Dispose();
        }
    }
}