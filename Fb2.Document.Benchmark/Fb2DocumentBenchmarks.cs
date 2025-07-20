using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Fb2.Document.LoadingOptions;

namespace Fb2.Document.Benchmark;

[Config(typeof(Fb2BenchmarkConfig))]
public class Fb2DocumentBenchmarks
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

    //[Benchmark]
    //public async Task<Fb2Document> LoadDocument_Full_FromStreamAsync()
    //{
    //    var doc = new Fb2Document();
    //    await doc.LoadAsync(fb2FileContentStream!);
    //    return doc;
    //}

    [Benchmark]
    public async Task<Fb2Document> LoadDocument_Optimized_FromStreamAsync()
    {
        var doc = new Fb2Document();
        await doc.Load(fb2FileContentStream!);
        return doc;
    }

    //[Benchmark]
    //public async Task<Fb2Document> LoadDocument_NoUnsafe_FromStreamAsync()
    //{
    //    var doc = new Fb2Document();
    //    await doc.LoadAsync(fb2FileContentStream!, new Fb2StreamLoadingOptions
    //    {
    //        LoadUnsafeElements = false
    //    });
    //    return doc;
    //}

    //[Benchmark]
    //public async Task<Fb2Document> LoadDocument_NoUnsafe_NoMetadata_FromStreamAsync()
    //{
    //    var doc = new Fb2Document();
    //    await doc.LoadAsync(fb2FileContentStream!, new Fb2StreamLoadingOptions
    //    {
    //        LoadNamespaceMetadata = false,
    //        LoadUnsafeElements = false
    //    });
    //    return doc;
    //}

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