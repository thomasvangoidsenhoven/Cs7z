using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cs7z.Core;
using Cs7z.Service.OmniPlatform;
using Xunit;

namespace Cs7z.Service.IntegrationTests;

public class ExtractToDirectoryAsyncIntegrationTests : IntegrationTestBase
{
    private readonly ISevenZipArchive _sevenZip;
    
    public ExtractToDirectoryAsyncIntegrationTests()
    {
        var executableSource = new OmniPlatformSevenZipExecutableSource();
        _sevenZip = new SevenZipArchive(executableSource);
    }
    
    [Fact]
    public async Task ExtractToDirectoryAsync_ValidZipArchive_ExtractsSuccessfully()
    {
        // Arrange
        var files = TestArchiveHelper.GetSimpleTestFiles();
        var archivePath = await TestArchiveHelper.CreateTestZipArchive(
            GetTestFilePath("test.zip"), files);
        var extractPath = GetOutputPath("extracted");
        
        // Act
        await _sevenZip.ExtractToDirectoryAsync(archivePath, extractPath);
        
        // Assert
        Assert.True(Directory.Exists(extractPath));
        foreach (var file in files)
        {
            var extractedFile = Path.Combine(extractPath, file.Key);
            Assert.True(File.Exists(extractedFile), $"File {file.Key} was not extracted");
            var content = await File.ReadAllTextAsync(extractedFile);
            Assert.Equal(file.Value, content);
        }
    }
    
    [Fact]
    public async Task ExtractToDirectoryAsync_Valid7zArchive_ExtractsSuccessfully()
    {
        // Arrange
        var files = TestArchiveHelper.GetComplexTestFiles();
        var archivePath = await TestArchiveHelper.CreateTest7zArchive(
            GetTestFilePath("test.7z"), files);
        var extractPath = GetOutputPath("extracted");
        
        // Act
        await _sevenZip.ExtractToDirectoryAsync(archivePath, extractPath);
        
        // Assert
        Assert.True(Directory.Exists(extractPath));
        foreach (var file in files)
        {
            var extractedFile = Path.Combine(extractPath, file.Key);
            Assert.True(File.Exists(extractedFile), $"File {file.Key} was not extracted");
            var content = await File.ReadAllTextAsync(extractedFile);
            Assert.Equal(file.Value, content);
        }
    }
    
    [Fact]
    public async Task ExtractToDirectoryAsync_ExistingDestinationDirectory_ExtractsSuccessfully()
    {
        // Arrange
        var files = TestArchiveHelper.GetSimpleTestFiles();
        var archivePath = await TestArchiveHelper.CreateTestZipArchive(
            GetTestFilePath("test.zip"), files);
        var extractPath = GetOutputPath("existing");
        Directory.CreateDirectory(extractPath);
        await File.WriteAllTextAsync(Path.Combine(extractPath, "existing.txt"), "Existing file");
        
        // Act
        await _sevenZip.ExtractToDirectoryAsync(archivePath, extractPath);
        
        // Assert
        Assert.True(File.Exists(Path.Combine(extractPath, "existing.txt")));
        foreach (var file in files)
        {
            var extractedFile = Path.Combine(extractPath, file.Key);
            Assert.True(File.Exists(extractedFile));
        }
    }
    
    [Fact]
    public async Task ExtractToDirectoryAsync_NonExistentArchive_ThrowsFileNotFoundException()
    {
        // Arrange
        var archivePath = GetTestFilePath("nonexistent.zip");
        var extractPath = GetOutputPath("extracted");
        
        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => _sevenZip.ExtractToDirectoryAsync(archivePath, extractPath));
    }
    
    [Fact]
    public async Task ExtractToDirectoryAsync_InvalidArchivePath_ThrowsArgumentException()
    {
        // Arrange
        var extractPath = GetOutputPath("extracted");
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sevenZip.ExtractToDirectoryAsync("", extractPath));
        
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _sevenZip.ExtractToDirectoryAsync(null!, extractPath));
    }
    
    [Fact]
    public async Task ExtractToDirectoryAsync_InvalidDestinationPath_ThrowsArgumentException()
    {
        // Arrange
        var files = TestArchiveHelper.GetSimpleTestFiles();
        var archivePath = await TestArchiveHelper.CreateTestZipArchive(
            GetTestFilePath("test.zip"), files);
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sevenZip.ExtractToDirectoryAsync(archivePath, ""));
        
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _sevenZip.ExtractToDirectoryAsync(archivePath, null!));
    }
    
    [Fact]
    public async Task ExtractToDirectoryAsync_CorruptedArchive_ThrowsException()
    {
        // Arrange
        var archivePath = await TestArchiveHelper.CreateCorruptedArchive(
            GetTestFilePath("corrupted.zip"));
        var extractPath = GetOutputPath("extracted");
        
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sevenZip.ExtractToDirectoryAsync(archivePath, extractPath));
    }
    
    [Fact]
    public async Task ExtractToDirectoryAsync_WithCancellationToken_CanBeCancelled()
    {
        // Arrange
        var files = TestArchiveHelper.GetComplexTestFiles();
        var archivePath = await TestArchiveHelper.CreateTest7zArchive(
            GetTestFilePath("test.7z"), files);
        var extractPath = GetOutputPath("extracted");
        
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately
        
        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _sevenZip.ExtractToDirectoryAsync(archivePath, extractPath, cts.Token));
    }
    
    [Fact]
    public async Task ExtractToDirectoryAsync_ArchiveWithEmptyFolders_PreservesDirectoryStructure()
    {
        // Arrange
        var files = TestArchiveHelper.GetEmptyFolderTestFiles();
        var archivePath = await TestArchiveHelper.CreateTestZipArchive(
            GetTestFilePath("test.zip"), files);
        var extractPath = GetOutputPath("extracted");
        
        // Act
        await _sevenZip.ExtractToDirectoryAsync(archivePath, extractPath);
        
        // Assert
        Assert.True(Directory.Exists(Path.Combine(extractPath, "folder1")));
        Assert.True(Directory.Exists(Path.Combine(extractPath, "folder2", "subfolder")));
    }
}