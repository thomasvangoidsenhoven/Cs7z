using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Cs7z.Core;
using Cs7z.Service.OmniPlatform;
using System.IO.Compression;
using System.Text;

namespace Cs7z.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class ArchiveCompressionBenchmarks
{
    private string _tempBasePath = null!;
    private string _sourceDataPath = null!;
    private string _sevenZipArchivePath = null!;
    private string _zipArchivePath = null!;
    private ISevenZipArchive _sevenZipArchive = null!;
    
    [Params(1, 10, 100, 500, 1000)] // Different data sizes: 1MB, 10MB, 100MB
    public int DataSizeMB { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        // Create temp directory for benchmarks
        _tempBasePath = Path.Combine(Path.GetTempPath(), $"cs7z_benchmark_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempBasePath);
        
        _sourceDataPath = Path.Combine(_tempBasePath, "source_data");
        Directory.CreateDirectory(_sourceDataPath);
        
        // Generate test data
        GenerateTestData(_sourceDataPath, DataSizeMB);
        
        // Setup archive paths
        _sevenZipArchivePath = Path.Combine(_tempBasePath, "test.7z");
        _zipArchivePath = Path.Combine(_tempBasePath, "test.zip");
        
        // Initialize SevenZipArchive with OmniPlatformSevenZipExecutableSource
        var executableSource = new OmniPlatformSevenZipExecutableSource();
        _sevenZipArchive = new SevenZipArchive(executableSource);
        
        // Pre-create archives for extraction benchmarks
        CreateSevenZipArchiveForExtraction();
        CreateZipArchiveForExtraction();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_tempBasePath))
        {
            Directory.Delete(_tempBasePath, true);
        }
    }

    [Benchmark]
    public async Task CreateArchiveWithSevenZip()
    {
        var outputPath = Path.Combine(_tempBasePath, $"benchmark_create_{Guid.NewGuid()}.7z");
        await _sevenZipArchive.CreateArchive(outputPath, _sourceDataPath);
        File.Delete(outputPath);
    }

    [Benchmark]
    public void CreateArchiveWithZipArchive()
    {
        var outputPath = Path.Combine(_tempBasePath, $"benchmark_create_{Guid.NewGuid()}.zip");
        ZipFile.CreateFromDirectory(_sourceDataPath, outputPath, CompressionLevel.Optimal, false);
        File.Delete(outputPath);
    }

    [Benchmark]
    public async Task ExtractArchiveWithSevenZip()
    {
        var extractPath = Path.Combine(_tempBasePath, $"extract_7z_{Guid.NewGuid()}");
        await _sevenZipArchive.ExtractToDirectoryAsync(_sevenZipArchivePath, extractPath);
        Directory.Delete(extractPath, true);
    }

    [Benchmark]
    public void ExtractArchiveWithZipArchive()
    {
        var extractPath = Path.Combine(_tempBasePath, $"extract_zip_{Guid.NewGuid()}");
        ZipFile.ExtractToDirectory(_zipArchivePath, extractPath);
        Directory.Delete(extractPath, true);
    }

    [Benchmark]
    public void CreateArchiveWithZipArchiveStreams()
    {
        var outputPath = Path.Combine(_tempBasePath, $"benchmark_create_stream_{Guid.NewGuid()}.zip");
        
        using (var fileStream = new FileStream(outputPath, FileMode.Create))
        using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
        {
            // Add all files from source directory
            AddDirectoryToZipArchive(archive, _sourceDataPath, "");
        }
        
        File.Delete(outputPath);
    }

    [Benchmark]
    public void ExtractArchiveWithZipArchiveStreams()
    {
        var extractPath = Path.Combine(_tempBasePath, $"extract_zip_stream_{Guid.NewGuid()}");
        Directory.CreateDirectory(extractPath);
        
        using (var fileStream = new FileStream(_zipArchivePath, FileMode.Open, FileAccess.Read))
        using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Read))
        {
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
                    using (var entryStream = entry.Open())
                    using (var destinationStream = new FileStream(destinationPath, FileMode.Create))
                    {
                        entryStream.CopyTo(destinationStream);
                    }
                }
            }
        }
        
        Directory.Delete(extractPath, true);
    }

    private void GenerateTestData(string targetPath, int sizeMB)
    {
        var random = new Random(42); // Fixed seed for reproducibility
        var totalBytes = sizeMB * 1024 * 1024;
        var bytesWritten = 0;
        var fileIndex = 0;

        // Create a mix of text and binary files
        while (bytesWritten < totalBytes)
        {
            fileIndex++;
            var remainingBytes = totalBytes - bytesWritten;
            var fileSize = Math.Min(remainingBytes, random.Next(100_000, 1_000_000)); // Files between 100KB and 1MB
            
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

    private string GenerateRandomText(int approximateSize)
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

    private void CreateSevenZipArchiveForExtraction()
    {
        _sevenZipArchive.CreateArchive(_sevenZipArchivePath, _sourceDataPath).Wait();
    }

    private void CreateZipArchiveForExtraction()
    {
        ZipFile.CreateFromDirectory(_sourceDataPath, _zipArchivePath, CompressionLevel.Optimal, false);
    }

    private void AddDirectoryToZipArchive(ZipArchive archive, string sourceDir, string entryPrefix)
    {
        var files = Directory.GetFiles(sourceDir);
        foreach (var file in files)
        {
            var entryName = Path.Combine(entryPrefix, Path.GetFileName(file));
            var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
            
            using (var entryStream = entry.Open())
            using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                fileStream.CopyTo(entryStream);
            }
        }
        
        var directories = Directory.GetDirectories(sourceDir);
        foreach (var directory in directories)
        {
            var dirName = Path.GetFileName(directory);
            var newPrefix = string.IsNullOrEmpty(entryPrefix) ? dirName : Path.Combine(entryPrefix, dirName);
            AddDirectoryToZipArchive(archive, directory, newPrefix);
        }
    }
}