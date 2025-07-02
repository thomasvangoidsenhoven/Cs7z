using System;
using System.IO;
using System.Threading.Tasks;
using Cs7z.Core;
using Moq;
using Xunit;

namespace Cs7z.Core.Tests;

public class SevenZipArchiveTests
{
    private readonly Mock<ISevenZipExecutableSource> _mockExecutableSource;
    private readonly string _testExecutablePath = "/usr/bin/7z";
    
    public SevenZipArchiveTests()
    {
        _mockExecutableSource = new Mock<ISevenZipExecutableSource>();
        _mockExecutableSource.Setup(x => x.FindExecutable()).Returns(_testExecutablePath);
    }
    
    [Fact]
    public void Constructor_WithValidExecutableSource_InitializesSuccessfully()
    {
        var archive = new SevenZipArchive(_mockExecutableSource.Object);
        
        Assert.NotNull(archive);
        _mockExecutableSource.Verify(x => x.FindExecutable(), Times.Once);
    }
    
    [Fact]
    public async Task ExtractToDirectoryAsync_WithNullArchivePath_ThrowsArgumentException()
    {
        var archive = new SevenZipArchive(_mockExecutableSource.Object);
        
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => 
            await archive.ExtractToDirectoryAsync(null!, "destination"));
        
        Assert.Contains("Archive file path", exception.Message);
        Assert.Equal("archiveFilePath", exception.ParamName);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExtractToDirectoryAsync_WithInvalidArchivePath_ThrowsArgumentException(string archivePath)
    {
        var archive = new SevenZipArchive(_mockExecutableSource.Object);
        
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => 
            await archive.ExtractToDirectoryAsync(archivePath, "destination"));
        
        Assert.Contains("Archive file path", exception.Message);
        Assert.Equal("archiveFilePath", exception.ParamName);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExtractToDirectoryAsync_WithInvalidDestination_ThrowsArgumentException(string destination)
    {
        var archive = new SevenZipArchive(_mockExecutableSource.Object);
        
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => 
            await archive.ExtractToDirectoryAsync("archive.zip", destination));
        
        Assert.Contains("Destination directory", exception.Message);
        Assert.Equal("destinationDirectoryName", exception.ParamName);
    }
    
    [Fact]
    public async Task ExtractToDirectoryAsync_WithNonExistentArchive_ThrowsFileNotFoundException()
    {
        var archive = new SevenZipArchive(_mockExecutableSource.Object);
        var nonExistentPath = "/path/to/nonexistent/archive.zip";
        
        var exception = await Assert.ThrowsAsync<FileNotFoundException>(async () => 
            await archive.ExtractToDirectoryAsync(nonExistentPath, "destination"));
        
        Assert.Contains(nonExistentPath, exception.Message);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateArchive_WithInvalidArchivePath_ThrowsArgumentException(string archivePath)
    {
        var archive = new SevenZipArchive(_mockExecutableSource.Object);
        
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => 
            await archive.CreateArchive(archivePath, "folder"));
        
        Assert.Contains("Archive file path", exception.Message);
        Assert.Equal("archiveFilePath", exception.ParamName);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateArchive_WithInvalidFolderPath_ThrowsArgumentException(string folderPath)
    {
        var archive = new SevenZipArchive(_mockExecutableSource.Object);
        
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => 
            await archive.CreateArchive("archive.zip", folderPath));
        
        Assert.Contains("Folder path", exception.Message);
        Assert.Equal("folder", exception.ParamName);
    }
    
    [Fact]
    public async Task CreateArchive_WithNonExistentFolder_ThrowsDirectoryNotFoundException()
    {
        var archive = new SevenZipArchive(_mockExecutableSource.Object);
        var nonExistentPath = "/path/to/nonexistent/folder";
        
        var exception = await Assert.ThrowsAsync<DirectoryNotFoundException>(async () => 
            await archive.CreateArchive("archive.zip", nonExistentPath));
        
        Assert.Contains(nonExistentPath, exception.Message);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ListArchive_WithInvalidArchivePath_ThrowsArgumentException(string archivePath)
    {
        var archive = new SevenZipArchive(_mockExecutableSource.Object);
        
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => 
            await archive.ListArchive(archivePath));
        
        Assert.Contains("Archive file path", exception.Message);
        Assert.Equal("archiveFilePath", exception.ParamName);
    }
    
    [Fact]
    public async Task ListArchive_WithNonExistentArchive_ThrowsFileNotFoundException()
    {
        var archive = new SevenZipArchive(_mockExecutableSource.Object);
        var nonExistentPath = "/path/to/nonexistent/archive.zip";
        
        var exception = await Assert.ThrowsAsync<FileNotFoundException>(async () => 
            await archive.ListArchive(nonExistentPath));
        
        Assert.Contains(nonExistentPath, exception.Message);
    }
}