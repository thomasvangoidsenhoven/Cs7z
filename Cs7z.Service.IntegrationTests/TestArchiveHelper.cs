using System.IO;
using System.IO.Compression;
using Cs7z.Core;
using Cs7z.Service.OmniPlatform;

namespace Cs7z.Service.IntegrationTests;

public static class TestArchiveHelper
{
    public static async Task<string> CreateTestZipArchive(string outputPath, Dictionary<string, string> files)
    {
        using (var archive = ZipFile.Open(outputPath, ZipArchiveMode.Create))
        {
            foreach (var file in files)
            {
                var entry = archive.CreateEntry(file.Key);
                using (var stream = entry.Open())
                using (var writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(file.Value);
                }
            }
        }
        
        return outputPath;
    }
    
    public static async Task<string> CreateTest7zArchive(string outputPath, Dictionary<string, string> files)
    {
        // First create a temporary directory with the files
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        try
        {
            foreach (var file in files)
            {
                var filePath = Path.Combine(tempDir, file.Key);
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                await File.WriteAllTextAsync(filePath, file.Value);
            }
            
            // Use SevenZipArchive to create the archive
            var executableSource = new OmniPlatformSevenZipExecutableSource();
            var sevenZip = new SevenZipArchive(executableSource);
            await sevenZip.CreateArchive(outputPath, tempDir);
            
            return outputPath;
        }
        finally
        {
            try
            {
                Directory.Delete(tempDir, true);
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }
    
    public static Dictionary<string, string> GetSimpleTestFiles()
    {
        return new Dictionary<string, string>
        {
            ["file1.txt"] = "This is test file 1",
            ["file2.txt"] = "This is test file 2",
            ["subfolder/file3.txt"] = "This is test file 3 in a subfolder"
        };
    }
    
    public static Dictionary<string, string> GetComplexTestFiles()
    {
        return new Dictionary<string, string>
        {
            ["readme.md"] = "# Test Archive\n\nThis is a test archive for integration testing.",
            ["src/main.cs"] = "namespace Test { class Program { static void Main() { } } }",
            ["src/helper.cs"] = "namespace Test { class Helper { } }",
            ["docs/guide.txt"] = "This is a guide document.",
            ["data/sample.json"] = "{ \"test\": true, \"value\": 42 }",
            ["data/nested/deep/file.txt"] = "Deeply nested file content"
        };
    }
    
    public static Dictionary<string, string> GetEmptyFolderTestFiles()
    {
        return new Dictionary<string, string>
        {
            ["folder1/.gitkeep"] = "",
            ["folder2/subfolder/.gitkeep"] = ""
        };
    }
    
    public static async Task<string> CreateCorruptedArchive(string outputPath)
    {
        // Create a file that looks like an archive but has corrupted data
        var randomBytes = new byte[1024];
        new Random().NextBytes(randomBytes);
        
        // Add some zip-like header bytes but corrupt content
        randomBytes[0] = 0x50; // P
        randomBytes[1] = 0x4B; // K
        randomBytes[2] = 0x03; // zip signature
        randomBytes[3] = 0x04;
        
        await File.WriteAllBytesAsync(outputPath, randomBytes);
        return outputPath;
    }
}