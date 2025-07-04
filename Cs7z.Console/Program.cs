using Cs7z.Core;
using Cs7z.Core.Models;
using Cs7z.Service.OmniPlatform;

Console.WriteLine("Cs7z Compression Level Test");
Console.WriteLine("===========================");

var sourceLocation = new OmniPlatformSevenZipExecutableSource();
var archive = new SevenZipArchive(sourceLocation);

// Create test directory with compressible content
var testDir = "test_compression";
Directory.CreateDirectory(testDir);

// Create test files with repeated content that compresses well
var testContent = string.Join("\n", Enumerable.Repeat("This is a test line that should compress very well because it repeats.", 500));
await File.WriteAllTextAsync(Path.Combine(testDir, "file1.txt"), testContent);
await File.WriteAllTextAsync(Path.Combine(testDir, "file2.txt"), testContent);
await File.WriteAllTextAsync(Path.Combine(testDir, "file3.txt"), testContent);

Console.WriteLine($"\nCreated test files in '{testDir}' directory");
Console.WriteLine("Testing different compression levels...\n");

// Test each compression level
var levels = new[] 
{
    CompressionLevel.Store,
    CompressionLevel.Fastest,
    CompressionLevel.Fast,
    CompressionLevel.Normal,
    CompressionLevel.Maximum,
    CompressionLevel.Ultra
};

var results = new List<(CompressionLevel level, string archiveName, long size, TimeSpan duration)>();

foreach (var level in levels)
{
    var archiveName = $"test_{level}.7z";
    Console.Write($"Creating archive with {level} compression... ");
    
    var startTime = DateTime.Now;
    await archive.CreateArchive(archiveName, testDir, level);
    var duration = DateTime.Now - startTime;
    
    var fileInfo = new FileInfo(archiveName);
    results.Add((level, archiveName, fileInfo.Length, duration));
    
    Console.WriteLine($"Done! Size: {fileInfo.Length:N0} bytes, Time: {duration.TotalMilliseconds:F0}ms");
}

// Display summary
Console.WriteLine("\n=== Compression Results Summary ===");
Console.WriteLine($"{"Level",-10} {"Size (bytes)",-15} {"Time (ms)",-10} {"Compression %",-15}");
Console.WriteLine(new string('-', 50));

var originalSize = new DirectoryInfo(testDir).GetFiles().Sum(f => f.Length);
Console.WriteLine($"Original size: {originalSize:N0} bytes\n");

foreach (var (level, _, size, duration) in results)
{
    var compressionPercent = 100.0 * (1.0 - (double)size / originalSize);
    Console.WriteLine($"{level,-10} {size,-15:N0} {duration.TotalMilliseconds,-10:F0} {compressionPercent,-15:F2}%");
}

// Test extraction of one archive to verify it works
Console.WriteLine("\nVerifying archive integrity...");
var extractDir = "test_extract";
await archive.ExtractToDirectoryAsync("test_Normal.7z", extractDir);
Console.WriteLine($"Successfully extracted test_Normal.7z to '{extractDir}'");

// Cleanup
Console.WriteLine("\nCleaning up test files...");
foreach (var (_, archiveName, _, _) in results)
{
    File.Delete(archiveName);
}
Directory.Delete(testDir, true);
Directory.Delete(extractDir, true);

Console.WriteLine("\nAll tests completed successfully!");