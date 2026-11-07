using BenchmarkDotNet.Running;

namespace Fb2.Document.Benchmark;

public class Program
{
    public static void Main(string[] args) => _ = BenchmarkRunner.Run<Fb2DocumentBenchmarks>();

    //public static void Main(string[] args) =>
    //    BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
}