using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
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
        AddJob(
            Job.Default
            .WithRuntime(CoreRuntime.Core80)
            .WithWarmupCount(5)
            .WithLaunchCount(10)
            .WithIterationCount(100)
            .WithToolchain(InProcessNoEmitToolchain.Instance))
        .AddDiagnoser(MemoryDiagnoser.Default);
}

[Config(typeof(AntiVirusFriendlyConfig))]
public class Fb2DocumentBenchMark
{
    public FileStream? fb2FileContentStream;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var sampleFilePath = GetSampleFilePath();
        fb2FileContentStream = new FileStream(sampleFilePath, FileMode.Open);
    }

    private static string GetSampleFilePath()
    {
        var currentFolder = Environment.CurrentDirectory;
        var pathChunks = currentFolder.Split(Path.DirectorySeparatorChar).ToList();
        var pathChunksTopIndex = pathChunks.Count - 1;
        for (int i = pathChunksTopIndex; i >= 0; i--)
        {
            var chunk = pathChunks[i];
            if (chunk.Equals("Fb2.Document"))
            {
                pathChunks.RemoveRange(i + 1, pathChunksTopIndex - i);
                break;
            }
        }

        pathChunks.Add("Sample");
        pathChunks.Add("_Test_1.fb2");

        var sampleFilePath = Path.Combine(pathChunks.ToArray());
        return sampleFilePath;
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