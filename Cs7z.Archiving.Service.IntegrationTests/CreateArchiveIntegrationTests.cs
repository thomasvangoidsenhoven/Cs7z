using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cs7z.Archiving.Core;
using Cs7z.Archiving.Core.Models;
using Cs7z.Archiving.Service.OmniPlatform;
using Xunit;

namespace Cs7z.Archiving.Service.IntegrationTests;

public class CreateArchiveIntegrationTests : IntegrationTestBase
{
    private readonly ISevenZipArchive _sevenZip;
    
    public CreateArchiveIntegrationTests()
    {
        var executableSource = new OmniPlatformSevenZipExecutableSource();
        _sevenZip = new SevenZipArchive(executableSource);
    }
    
    [Fact]
    public async Task CreateArchive_FromFolderWithFiles_CreatesArchiveSuccessfully()
    {
        // Arrange
        var sourceFolder = GetTestFilePath("source");
        CreateTestFile("source/file1.txt", "Content of file 1");
        CreateTestFile("source/file2.txt", "Content of file 2");
        CreateTestFile("source/readme.md", "# Test Project");
        
        var archivePath = GetOutputPath("created.7z");
        
        // Act
        await _sevenZip.CreateArchive(archivePath, sourceFolder);
        
        // Assert
        Assert.True(File.Exists(archivePath));
        var fileInfo = new FileInfo(archivePath);
        Assert.True(fileInfo.Length > 0);
        
        // Verify by extracting
        var extractPath = GetOutputPath("verify");
        await _sevenZip.ExtractToDirectoryAsync(archivePath, extractPath);
        
        Assert.True(File.Exists(Path.Combine(extractPath, "file1.txt")));
        Assert.True(File.Exists(Path.Combine(extractPath, "file2.txt")));
        Assert.True(File.Exists(Path.Combine(extractPath, "readme.md")));
        
        var content1 = await File.ReadAllTextAsync(Path.Combine(extractPath, "file1.txt"));
        Assert.Equal("Content of file 1", content1);
    }
    
    [Fact]
    public async Task CreateArchive_FromFolderWithSubdirectories_PreservesStructure()
    {
        // Arrange
        var sourceFolder = GetTestFilePath("complex");
        CreateTestFile("complex/root.txt", "Root file");
        CreateTestFile("complex/src/main.cs", "Main source");
        CreateTestFile("complex/src/utils/helper.cs", "Helper class");
        CreateTestFile("complex/docs/readme.md", "Documentation");
        CreateTestDirectory("complex/empty");
        
        var archivePath = GetOutputPath("complex.7z");
        
        // Act
        await _sevenZip.CreateArchive(archivePath, sourceFolder);
        
        // Assert
        Assert.True(File.Exists(archivePath));
        
        // Verify structure by extracting
        var extractPath = GetOutputPath("verify_complex");
        await _sevenZip.ExtractToDirectoryAsync(archivePath, extractPath);
        
        Assert.True(File.Exists(Path.Combine(extractPath, "root.txt")));
        Assert.True(File.Exists(Path.Combine(extractPath, "src", "main.cs")));
        Assert.True(File.Exists(Path.Combine(extractPath, "src", "utils", "helper.cs")));
        Assert.True(File.Exists(Path.Combine(extractPath, "docs", "readme.md")));
        Assert.True(Directory.Exists(Path.Combine(extractPath, "empty")));
    }
    
    [Fact]
    public async Task CreateArchive_FromNonExistentFolder_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        var sourceFolder = GetTestFilePath("nonexistent");
        var archivePath = GetOutputPath("archive.7z");
        
        // Act & Assert
        await Assert.ThrowsAsync<DirectoryNotFoundException>(
            () => _sevenZip.CreateArchive(archivePath, sourceFolder));
    }
    
    [Fact]
    public async Task CreateArchive_WithInvalidSourcePath_ThrowsArgumentException()
    {
        // Arrange
        var archivePath = GetOutputPath("archive.7z");
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sevenZip.CreateArchive(archivePath, ""));
        
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sevenZip.CreateArchive(archivePath, null!));
    }
    
    [Fact]
    public async Task CreateArchive_WithInvalidArchivePath_ThrowsArgumentException()
    {
        // Arrange
        var sourceFolder = GetTestFilePath("source");
        CreateTestFile("source/file.txt", "content");
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sevenZip.CreateArchive("", sourceFolder));
        
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sevenZip.CreateArchive(null!, sourceFolder));
    }
    
    [Fact]
    public async Task CreateArchive_OverwriteExistingArchive_CreatesNewArchive()
    {
        // Arrange
        var sourceFolder1 = GetTestFilePath("source1");
        CreateTestFile("source1/file1.txt", "First version");
        
        var sourceFolder2 = GetTestFilePath("source2");
        CreateTestFile("source2/file2.txt", "Second version");
        
        var archivePath = GetOutputPath("overwrite.7z");
        
        // Act - Create first archive
        await _sevenZip.CreateArchive(archivePath, sourceFolder1);
        var firstSize = new FileInfo(archivePath).Length;
        
        // Act - Overwrite with second archive
        await _sevenZip.CreateArchive(archivePath, sourceFolder2);
        var secondSize = new FileInfo(archivePath).Length;
        
        // Assert
        Assert.True(File.Exists(archivePath));
        
        // Verify content is from second archive
        var extractPath = GetOutputPath("verify_overwrite");
        await _sevenZip.ExtractToDirectoryAsync(archivePath, extractPath);
        
        Assert.False(File.Exists(Path.Combine(extractPath, "file1.txt")));
        Assert.True(File.Exists(Path.Combine(extractPath, "file2.txt")));
    }
    
    [Fact]
    public async Task CreateArchive_WithCancellationToken_CanBeCancelled()
    {
        // Arrange
        var sourceFolder = GetTestFilePath("large");
        // Create many files to increase processing time
        for (int i = 0; i < 100; i++)
        {
            CreateTestFile($"large/file{i}.txt", $"Content of file {i}");
        }
        
        var archivePath = GetOutputPath("cancelled.7z");
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately
        
        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _sevenZip.CreateArchive(archivePath, sourceFolder, cts.Token));
    }
    
    [Fact]
    public async Task CreateArchive_EmptyFolder_CreatesValidArchive()
    {
        // Arrange
        var sourceFolder = GetTestFilePath("empty");
        CreateTestDirectory("empty");
        var archivePath = GetOutputPath("empty.7z");
        
        // Act
        await _sevenZip.CreateArchive(archivePath, sourceFolder);
        
        // Assert
        Assert.True(File.Exists(archivePath));
        var fileInfo = new FileInfo(archivePath);
        Assert.True(fileInfo.Length > 0); // Even empty archives have headers
    }
    
    [Fact]
    public async Task CreateArchive_WithSpecialCharactersInFileNames_HandlesCorrectly()
    {
        // Arrange
        var sourceFolder = GetTestFilePath("special");
        CreateTestFile("special/file with spaces.txt", "Content with spaces");
        CreateTestFile("special/file-with-dashes.txt", "Content with dashes");
        CreateTestFile("special/file_with_underscores.txt", "Content with underscores");
        CreateTestFile("special/文件.txt", "Unicode content");
        
        var archivePath = GetOutputPath("special.7z");
        
        // Act
        await _sevenZip.CreateArchive(archivePath, sourceFolder);
        
        // Assert
        Assert.True(File.Exists(archivePath));
        
        // Verify all files are preserved
        var extractPath = GetOutputPath("verify_special");
        await _sevenZip.ExtractToDirectoryAsync(archivePath, extractPath);
        
        Assert.True(File.Exists(Path.Combine(extractPath, "file with spaces.txt")));
        Assert.True(File.Exists(Path.Combine(extractPath, "file-with-dashes.txt")));
        Assert.True(File.Exists(Path.Combine(extractPath, "file_with_underscores.txt")));
        Assert.True(File.Exists(Path.Combine(extractPath, "文件.txt")));
    }
    
    [Fact]
    public async Task CreateArchive_WithDifferentCompressionLevels_CreatesArchivesWithDifferentSizes()
    {
        // Arrange
        var sourceFolder = GetTestFilePath("compression_test");
        
        // Create test files with compressible content
        var content = string.Join("\n", Enumerable.Repeat("This is a test line that should compress well.", 1000));
        CreateTestFile("compression_test/file1.txt", content);
        CreateTestFile("compression_test/file2.txt", content);
        CreateTestFile("compression_test/file3.txt", content);
        
        var storeArchive = GetOutputPath("store.7z");
        var normalArchive = GetOutputPath("normal.7z");
        var ultraArchive = GetOutputPath("ultra.7z");
        
        // Act
        await _sevenZip.CreateArchive(storeArchive, sourceFolder, CompressionLevel.Store);
        await _sevenZip.CreateArchive(normalArchive, sourceFolder, CompressionLevel.Normal);
        await _sevenZip.CreateArchive(ultraArchive, sourceFolder, CompressionLevel.Ultra);
        
        // Assert
        var storeSize = new FileInfo(storeArchive).Length;
        var normalSize = new FileInfo(normalArchive).Length;
        var ultraSize = new FileInfo(ultraArchive).Length;
        
        // Store should be larger than Normal
        Assert.True(storeSize > normalSize, 
            $"Store size ({storeSize}) should be larger than Normal size ({normalSize})");
        
        // Normal should be larger than Ultra
        Assert.True(normalSize > ultraSize, 
            $"Normal size ({normalSize}) should be larger than Ultra size ({ultraSize})");
        
        // Verify all archives extract correctly
        var extractStore = GetOutputPath("extract_store");
        var extractNormal = GetOutputPath("extract_normal");
        var extractUltra = GetOutputPath("extract_ultra");
        
        await _sevenZip.ExtractToDirectoryAsync(storeArchive, extractStore);
        await _sevenZip.ExtractToDirectoryAsync(normalArchive, extractNormal);
        await _sevenZip.ExtractToDirectoryAsync(ultraArchive, extractUltra);
        
        // Verify content is identical
        foreach (var file in new[] { "file1.txt", "file2.txt", "file3.txt" })
        {
            var storeContent = await File.ReadAllTextAsync(Path.Combine(extractStore, file));
            var normalContent = await File.ReadAllTextAsync(Path.Combine(extractNormal, file));
            var ultraContent = await File.ReadAllTextAsync(Path.Combine(extractUltra, file));
            
            Assert.Equal(content, storeContent);
            Assert.Equal(content, normalContent);
            Assert.Equal(content, ultraContent);
        }
    }
    
    [Theory]
    [InlineData(CompressionLevel.Store)]
    [InlineData(CompressionLevel.Fastest)]
    [InlineData(CompressionLevel.Fast)]
    [InlineData(CompressionLevel.Normal)]
    [InlineData(CompressionLevel.Maximum)]
    [InlineData(CompressionLevel.Ultra)]
    public async Task CreateArchive_WithEachCompressionLevel_CreatesValidArchive(CompressionLevel compressionLevel)
    {
        // Arrange
        var sourceFolder = GetTestFilePath($"level_{compressionLevel}");
        CreateTestFile($"level_{compressionLevel}/test.txt", $"Testing compression level {compressionLevel}");
        
        var archivePath = GetOutputPath($"level_{compressionLevel}.7z");
        
        // Act
        await _sevenZip.CreateArchive(archivePath, sourceFolder, compressionLevel);
        
        // Assert
        Assert.True(File.Exists(archivePath));
        Assert.True(new FileInfo(archivePath).Length > 0);
        
        // Verify extraction
        var extractPath = GetOutputPath($"extract_{compressionLevel}");
        await _sevenZip.ExtractToDirectoryAsync(archivePath, extractPath);
        
        var extractedContent = await File.ReadAllTextAsync(Path.Combine(extractPath, "test.txt"));
        Assert.Equal($"Testing compression level {compressionLevel}", extractedContent);
    }
    
    [Fact]
    public async Task CreateArchive_EmptyFolderWithDifferentCompressionLevels_CreatesConsistentArchives()
    {
        // Arrange
        var emptyFolder = GetTestFilePath("empty_compression");
        CreateTestDirectory("empty_compression");
        
        var storeArchive = GetOutputPath("empty_store.7z");
        var ultraArchive = GetOutputPath("empty_ultra.7z");
        
        // Act
        await _sevenZip.CreateArchive(storeArchive, emptyFolder, CompressionLevel.Store);
        await _sevenZip.CreateArchive(ultraArchive, emptyFolder, CompressionLevel.Ultra);
        
        // Assert - Empty archives should have similar sizes regardless of compression level
        var storeSize = new FileInfo(storeArchive).Length;
        var ultraSize = new FileInfo(ultraArchive).Length;
        
        Assert.True(storeSize > 0);
        Assert.True(ultraSize > 0);
        
        // The difference should be minimal for empty archives
        var sizeDifference = Math.Abs(storeSize - ultraSize);
        Assert.True(sizeDifference < 1000, 
            $"Size difference ({sizeDifference}) should be minimal for empty archives");
    }
}