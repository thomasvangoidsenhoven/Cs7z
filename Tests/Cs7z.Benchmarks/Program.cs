using BenchmarkDotNet.Running;
using Cs7z.Benchmarks;

namespace Cs7z.Benchmarks;

class Program
{
    static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<ArchiveCompressionBenchmarks>();
    }
}