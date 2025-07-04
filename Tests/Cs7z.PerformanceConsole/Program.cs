using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using Cs7z.Core;
using Cs7z.Service.OmniPlatform;

namespace Cs7z.PerformanceConsole;

class Program
{
    private static readonly int[] TestSizes = { 1, 10, 100, 500, 1000 };
    
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Cs7z Performance Test Console ===");
        Console.WriteLine();
        
        var results = new List<PerformanceResult>();
        
        foreach (var sizeMB in TestSizes)
        {
            Console.WriteLine($"Testing with {sizeMB} MB of data...");
            
            // Setup
            var tempPath = Path.Combine(Path.GetTempPath(), $"cs7z_perf_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempPath);
            
            try
            {
                var sourceDataPath = Path.Combine(tempPath, "source_data");
                Directory.CreateDirectory(sourceDataPath);
                
                Console.Write("  Generating test data... ");
                var sw = Stopwatch.StartNew();
                GenerateTestData(sourceDataPath, sizeMB);
                sw.Stop();
                Console.WriteLine($"Done ({sw.ElapsedMilliseconds} ms)");
                
                var result = new PerformanceResult { SizeMB = sizeMB };
                
                // Initialize SevenZipArchive
                var executableSource = new OmniPlatformSevenZipExecutableSource();
                var sevenZipArchive = new SevenZipArchive(executableSource);
                
                // Test SevenZip Create
                Console.Write("  SevenZip Create: ");
                var sevenZipPath = Path.Combine(tempPath, "test.7z");
                sw.Restart();
                await sevenZipArchive.CreateArchive(sevenZipPath, sourceDataPath);
                sw.Stop();
                result.SevenZipCreate = sw.ElapsedMilliseconds;
                result.SevenZipFileSize = new FileInfo(sevenZipPath).Length;
                Console.WriteLine($"{result.SevenZipCreate} ms ({FormatFileSize(result.SevenZipFileSize)})");
                
                // Test ZipArchive Create
                Console.Write("  ZipArchive Create: ");
                var zipPath = Path.Combine(tempPath, "test.zip");
                sw.Restart();
                ZipFile.CreateFromDirectory(sourceDataPath, zipPath, CompressionLevel.Optimal, false);
                sw.Stop();
                result.ZipArchiveCreate = sw.ElapsedMilliseconds;
                result.ZipArchiveFileSize = new FileInfo(zipPath).Length;
                Console.WriteLine($"{result.ZipArchiveCreate} ms ({FormatFileSize(result.ZipArchiveFileSize)})");
                
                // Test ZipArchive Stream Create
                Console.Write("  ZipArchive Stream Create: ");
                var zipStreamPath = Path.Combine(tempPath, "test_stream.zip");
                sw.Restart();
                await CreateZipWithStreams(sourceDataPath, zipStreamPath);
                sw.Stop();
                result.ZipArchiveStreamCreate = sw.ElapsedMilliseconds;
                result.ZipArchiveStreamFileSize = new FileInfo(zipStreamPath).Length;
                Console.WriteLine($"{result.ZipArchiveStreamCreate} ms ({FormatFileSize(result.ZipArchiveStreamFileSize)})");
                
                // Test SevenZip Extract
                Console.Write("  SevenZip Extract: ");
                var sevenZipExtractPath = Path.Combine(tempPath, "extract_7z");
                sw.Restart();
                await sevenZipArchive.ExtractToDirectoryAsync(sevenZipPath, sevenZipExtractPath);
                sw.Stop();
                result.SevenZipExtract = sw.ElapsedMilliseconds;
                Console.WriteLine($"{result.SevenZipExtract} ms");
                
                // Test ZipArchive Extract
                Console.Write("  ZipArchive Extract: ");
                var zipExtractPath = Path.Combine(tempPath, "extract_zip");
                sw.Restart();
                ZipFile.ExtractToDirectory(zipPath, zipExtractPath);
                sw.Stop();
                result.ZipArchiveExtract = sw.ElapsedMilliseconds;
                Console.WriteLine($"{result.ZipArchiveExtract} ms");
                
                // Test ZipArchive Stream Extract
                Console.Write("  ZipArchive Stream Extract: ");
                var zipStreamExtractPath = Path.Combine(tempPath, "extract_zip_stream");
                sw.Restart();
                await ExtractZipWithStreams(zipPath, zipStreamExtractPath);
                sw.Stop();
                result.ZipArchiveStreamExtract = sw.ElapsedMilliseconds;
                Console.WriteLine($"{result.ZipArchiveStreamExtract} ms");
                
                results.Add(result);
                Console.WriteLine();
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                }
            }
        }
        
        // Display summary table
        Console.WriteLine("Summary Table:");
        Console.WriteLine("\nTiming (milliseconds):");
        Console.WriteLine("Size | 7z Create | Zip Create | Zip Stream | 7z Extract | Zip Extract | Zip Stream");
        Console.WriteLine("-----|-----------|------------|------------|------------|-------------|------------");
        
        foreach (var result in results)
        {
            Console.WriteLine($"{result.SizeMB,4} | {result.SevenZipCreate,9} | {result.ZipArchiveCreate,10} | {result.ZipArchiveStreamCreate,10} | {result.SevenZipExtract,10} | {result.ZipArchiveExtract,11} | {result.ZipArchiveStreamExtract,10}");
        }
        
        Console.WriteLine("\nFile Sizes:");
        Console.WriteLine("Size | 7z Archive    | Zip Archive   | Zip Stream    | Compression Ratio");
        Console.WriteLine("-----|---------------|---------------|---------------|------------------");
        
        foreach (var result in results)
        {
            var originalSize = result.SizeMB * 1024L * 1024L;
            var sevenZipRatio = (1.0 - (double)result.SevenZipFileSize / originalSize) * 100;
            var zipRatio = (1.0 - (double)result.ZipArchiveFileSize / originalSize) * 100;
            
            Console.WriteLine($"{result.SizeMB,4} | {FormatFileSize(result.SevenZipFileSize),13} | {FormatFileSize(result.ZipArchiveFileSize),13} | {FormatFileSize(result.ZipArchiveStreamFileSize),13} | 7z: {sevenZipRatio:F1}% Zip: {zipRatio:F1}%");
        }
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
    
    private static string FormatFileSize(long bytes)
    {
        const long GB = 1024L * 1024L * 1024L;
        const long MB = 1024L * 1024L;
        const long KB = 1024L;
        
        if (bytes >= GB)
            return $"{bytes / (double)GB:F2} GB";
        if (bytes >= MB)
            return $"{bytes / (double)MB:F2} MB";
        if (bytes >= KB)
            return $"{bytes / (double)KB:F2} KB";
        
        return $"{bytes} bytes";
    }
    
    private static void GenerateTestData(string targetPath, int sizeMB)
    {
        var random = new Random(42); // Fixed seed for consistency
        var totalBytes = sizeMB * 1024L * 1024L;
        var bytesWritten = 0L;
        var fileIndex = 0;
        
        // Create a mix of text and binary files
        while (bytesWritten < totalBytes)
        {
            fileIndex++;
            var remainingBytes = totalBytes - bytesWritten;
            var fileSize = (int)Math.Min(remainingBytes, random.Next(100_000, 1_000_000));
            
            if (fileIndex % 3 == 0)
            {
                // Create text file (more compressible)
                var fileName = Path.Combine(targetPath, $"text_file_{fileIndex}.txt");
                var content = GenerateRandomText(fileSize);
                File.WriteAllText(fileName, content);
                bytesWritten += Encoding.UTF8.GetByteCount(content);
            }
            else
            {
                // Create binary file (less compressible)
                var fileName = Path.Combine(targetPath, $"binary_file_{fileIndex}.bin");
                var data = new byte[fileSize];
                random.NextBytes(data);
                File.WriteAllBytes(fileName, data);
                bytesWritten += fileSize;
            }
        }
        
        // Create some subdirectories with files
        for (int i = 0; i < 3; i++)
        {
            var subDir = Path.Combine(targetPath, $"subdir_{i}");
            Directory.CreateDirectory(subDir);
            
            for (int j = 0; j < 5; j++)
            {
                var fileName = Path.Combine(subDir, $"file_{j}.txt");
                File.WriteAllText(fileName, GenerateRandomText(10_000));
            }
        }
    }
    
    private static string GenerateRandomText(int approximateSize)
    {
        var words = new[] { "lorem", "ipsum", "dolor", "sit", "amet", "consectetur", "adipiscing", "elit", "sed", "do", 
                           "eiusmod", "tempor", "incididunt", "ut", "labore", "et", "dolore", "magna", "aliqua" };
        var random = new Random(42);
        var sb = new StringBuilder();
        
        while (sb.Length < approximateSize)
        {
            sb.Append(words[random.Next(words.Length)]);
            sb.Append(' ');
            
            if (random.Next(10) == 0)
            {
                sb.AppendLine();
            }
        }
        
        return sb.ToString();
    }
    
    private static async Task CreateZipWithStreams(string sourceDir, string outputPath)
    {
        using var fileStream = new FileStream(outputPath, FileMode.Create);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Create);
        
        await AddDirectoryToZipArchive(archive, sourceDir, "");
    }
    
    private static async Task AddDirectoryToZipArchive(ZipArchive archive, string sourceDir, string entryPrefix)
    {
        var files = Directory.GetFiles(sourceDir);
        foreach (var file in files)
        {
            var entryName = Path.Combine(entryPrefix, Path.GetFileName(file));
            var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
            
            using var entryStream = entry.Open();
            using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            await fileStream.CopyToAsync(entryStream);
        }
        
        var directories = Directory.GetDirectories(sourceDir);
        foreach (var directory in directories)
        {
            var dirName = Path.GetFileName(directory);
            var newPrefix = string.IsNullOrEmpty(entryPrefix) ? dirName : Path.Combine(entryPrefix, dirName);
            await AddDirectoryToZipArchive(archive, directory, newPrefix);
        }
    }
    
    private static async Task ExtractZipWithStreams(string archivePath, string extractPath)
    {
        Directory.CreateDirectory(extractPath);
        
        using var fileStream = new FileStream(archivePath, FileMode.Open, FileAccess.Read);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
        
        foreach (var entry in archive.Entries)
        {
            var destinationPath = Path.Combine(extractPath, entry.FullName);
            var destinationDir = Path.GetDirectoryName(destinationPath);
            
            if (!string.IsNullOrEmpty(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }
            
            if (!entry.FullName.EndsWith("/"))
            {
                using var entryStream = entry.Open();
                using var destinationStream = new FileStream(destinationPath, FileMode.Create);
                await entryStream.CopyToAsync(destinationStream);
            }
        }
    }
}

class PerformanceResult
{
    public int SizeMB { get; set; }
    public long SevenZipCreate { get; set; }
    public long ZipArchiveCreate { get; set; }
    public long ZipArchiveStreamCreate { get; set; }
    public long SevenZipExtract { get; set; }
    public long ZipArchiveExtract { get; set; }
    public long ZipArchiveStreamExtract { get; set; }
    public long SevenZipFileSize { get; set; }
    public long ZipArchiveFileSize { get; set; }
    public long ZipArchiveStreamFileSize { get; set; }
}