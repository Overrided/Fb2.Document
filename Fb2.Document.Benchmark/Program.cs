using System;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace Fb2.Document.Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Fb2DocumentBenchMark>();
        }
    }

    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.Net50)]
    [SimpleJob(RuntimeMoniker.Net60)]
    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class Fb2DocumentBenchMark
    {
        public FileStream? fb2FileContentStream;

        [GlobalSetup]
        public void GlobalSetup()
        {
            fb2FileContentStream = new FileStream("D:\\Projects\\Fb2.Document.Bootstrap\\Samples\\Small\\_Test_1.fb2", FileMode.Open);
        }

        [IterationSetup]
        public void IterationSetup()
        {
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
}