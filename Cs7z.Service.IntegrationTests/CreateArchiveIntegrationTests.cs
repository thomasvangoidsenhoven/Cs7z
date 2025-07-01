using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cs7z.Core;
using Cs7z.Service.OmniPlatform;
using Xunit;

namespace Cs7z.Service.IntegrationTests;

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
}