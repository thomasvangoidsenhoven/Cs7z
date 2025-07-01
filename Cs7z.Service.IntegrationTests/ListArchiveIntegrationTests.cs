using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cs7z.Core;
using Cs7z.Service.OmniPlatform;
using Xunit;

namespace Cs7z.Service.IntegrationTests;

public class ListArchiveIntegrationTests : IntegrationTestBase
{
    private readonly ISevenZipArchive _sevenZip;
    
    public ListArchiveIntegrationTests()
    {
        var executableSource = new OmniPlatformSevenZipExecutableSource();
        _sevenZip = new SevenZipArchive(executableSource);
    }
    
    [Fact]
    public async Task ListArchive_ValidZipArchive_ReturnsCorrectFileList()
    {
        // Arrange
        var files = TestArchiveHelper.GetSimpleTestFiles();
        var archivePath = await TestArchiveHelper.CreateTestZipArchive(
            GetTestFilePath("test.zip"), files);
        
        // Act
        var result = await _sevenZip.ListArchive(archivePath);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(files.Count, result.TotalFiles);
        
        // Verify all files are listed
        foreach (var file in files.Keys)
        {
            Assert.Contains(result.Files, f => f.Path.Contains(file));
        }
    }
    
    [Fact]
    public async Task ListArchive_Valid7zArchive_ReturnsCorrectFileList()
    {
        // Arrange
        var files = TestArchiveHelper.GetComplexTestFiles();
        var archivePath = await TestArchiveHelper.CreateTest7zArchive(
            GetTestFilePath("test.7z"), files);
        
        // Act
        var result = await _sevenZip.ListArchive(archivePath);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(files.Count, result.TotalFiles);
        
        // Verify all files are listed
        foreach (var file in files.Keys)
        {
            Assert.Contains(result.Files, f => f.Path.Contains(file));
        }
    }
    
    [Fact]
    public async Task ListArchive_EmptyArchive_ReturnsMinimalOutput()
    {
        // Arrange
        var sourceFolder = GetTestFilePath("empty");
        CreateTestDirectory("empty");
        var archivePath = GetTestFilePath("empty.7z");
        await _sevenZip.CreateArchive(archivePath, sourceFolder);
        
        // Act
        var result = await _sevenZip.ListArchive(archivePath);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalFiles);
        Assert.Empty(result.Files);
    }
    
    [Fact]
    public async Task ListArchive_ArchiveWithDirectories_ShowsDirectoryStructure()
    {
        // Arrange
        var files = new Dictionary<string, string>
        {
            ["root.txt"] = "Root file",
            ["folder1/file1.txt"] = "File in folder1",
            ["folder1/subfolder/file2.txt"] = "File in subfolder",
            ["folder2/file3.txt"] = "File in folder2"
        };
        var archivePath = await TestArchiveHelper.CreateTest7zArchive(
            GetTestFilePath("dirs.7z"), files);
        
        // Act
        var result = await _sevenZip.ListArchive(archivePath);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(files.Count, result.TotalFiles);
        
        // Should list all files with their paths
        Assert.Contains(result.Files, f => f.Path.Contains("root.txt"));
        Assert.Contains(result.Files, f => f.Path.Contains("folder1") && f.Path.Contains("file1.txt"));
        Assert.Contains(result.Files, f => f.Path.Contains("folder1") && f.Path.Contains("subfolder") && f.Path.Contains("file2.txt"));
        Assert.Contains(result.Files, f => f.Path.Contains("folder2") && f.Path.Contains("file3.txt"));
    }
    
    [Fact]
    public async Task ListArchive_NonExistentArchive_ThrowsFileNotFoundException()
    {
        // Arrange
        var archivePath = GetTestFilePath("nonexistent.zip");
        
        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => _sevenZip.ListArchive(archivePath));
    }
    
    [Fact]
    public async Task ListArchive_InvalidArchivePath_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sevenZip.ListArchive(""));
        
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _sevenZip.ListArchive(null!));
    }
    
    [Fact]
    public async Task ListArchive_CorruptedArchive_ThrowsException()
    {
        // Arrange
        var archivePath = await TestArchiveHelper.CreateCorruptedArchive(
            GetTestFilePath("corrupted.zip"));
        
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sevenZip.ListArchive(archivePath));
    }
    
    [Fact]
    public async Task ListArchive_WithCancellationToken_CanBeCancelled()
    {
        // Arrange
        var files = TestArchiveHelper.GetComplexTestFiles();
        var archivePath = await TestArchiveHelper.CreateTest7zArchive(
            GetTestFilePath("test.7z"), files);
        
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately
        
        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _sevenZip.ListArchive(archivePath, cts.Token));
    }
    
    [Fact]
    public async Task ListArchive_ShowsFileSizes()
    {
        // Arrange
        var files = new Dictionary<string, string>
        {
            ["small.txt"] = "Small",
            ["medium.txt"] = new string('M', 1000),
            ["large.txt"] = new string('L', 10000)
        };
        var archivePath = await TestArchiveHelper.CreateTest7zArchive(
            GetTestFilePath("sizes.7z"), files);
        
        // Act
        var result = await _sevenZip.ListArchive(archivePath);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(files.Count, result.TotalFiles);
        
        // Verify files and their sizes
        var smallFile = result.Files.Single(f => f.Path.Contains("small.txt"));
        var mediumFile = result.Files.Single(f => f.Path.Contains("medium.txt"));
        var largeFile = result.Files.Single(f => f.Path.Contains("large.txt"));
        
        // Verify size ordering
        Assert.True(smallFile.Size < mediumFile.Size);
        Assert.True(mediumFile.Size < largeFile.Size);
    }
    
    [Fact]
    public async Task ListArchive_WithSpecialCharactersInFileNames_ListsCorrectly()
    {
        // Arrange
        var files = new Dictionary<string, string>
        {
            ["file with spaces.txt"] = "Spaces",
            ["file-with-dashes.txt"] = "Dashes",
            ["file_with_underscores.txt"] = "Underscores",
            ["文件.txt"] = "Unicode"
        };
        var archivePath = await TestArchiveHelper.CreateTestZipArchive(
            GetTestFilePath("special.zip"), files);
        
        // Act
        var result = await _sevenZip.ListArchive(archivePath);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(files.Count, result.TotalFiles);
        
        // Verify all files with special characters are listed
        Assert.Contains(result.Files, f => f.Path.Contains("file with spaces.txt"));
        Assert.Contains(result.Files, f => f.Path.Contains("file-with-dashes.txt"));
        Assert.Contains(result.Files, f => f.Path.Contains("file_with_underscores.txt"));
        Assert.Contains(result.Files, f => f.Path.Contains("文件.txt"));
    }
}