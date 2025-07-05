using BenchmarkDotNet.Running;

namespace Cs7z.Archiving.Benchmarks;

class Program
{
    static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<ArchiveCompressionBenchmarks>();
    }
}