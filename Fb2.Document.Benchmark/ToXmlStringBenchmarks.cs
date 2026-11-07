using System;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using Microsoft.VSDiagnostics;

namespace Fb2.Document.Benchmark;
[CPUUsageDiagnoser]
public class ToXmlStringBenchmarks
{
    private Fb2Document _document = null!;
    [GlobalSetup]
    public void Setup()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames().Single(name => name.EndsWith("_Test_1.fb2", StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        _document = new Fb2Document();
        _document.Load(stream);
    }

    [Benchmark]
    public string? SerializeToXmlString() => _document.ToXmlString();
}