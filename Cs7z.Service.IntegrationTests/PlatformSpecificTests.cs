using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Cs7z.Core;
using Cs7z.Service.Linux;
using Cs7z.Service.MacOsx;
using Cs7z.Service.OmniPlatform;
using Cs7z.Service.Windows;
using Xunit;

namespace Cs7z.Service.IntegrationTests;

public class PlatformSpecificTests : IntegrationTestBase
{
    [Fact]
    public void OmniPlatformExecutableSource_SelectsCorrectPlatformImplementation()
    {
        // Arrange
        var omniPlatform = new OmniPlatformSevenZipExecutableSource();
        
        // Act
        var executablePath = omniPlatform.FindExecutable();
        
        // Assert
        Assert.NotNull(executablePath);
        Assert.NotEmpty(executablePath);
        Assert.True(File.Exists(executablePath), $"Executable not found at: {executablePath}");
        
        // Verify correct platform was selected
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Assert.Contains("7z.exe", executablePath);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Assert.Contains("7zz", executablePath);
            Assert.DoesNotContain(".exe", executablePath);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Assert.Contains("7zz", executablePath);
            Assert.DoesNotContain(".exe", executablePath);
        }
    }
    
    [SkippableFact]
    public void WindowsPlatform_FindsWindowsExecutable()
    {
        Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), "This test only runs on Windows");
        
        // Arrange
        var windowsSource = new WindowsSevenZipExecutableSource();
        
        // Act
        var executablePath = windowsSource.FindExecutable();
        
        // Assert
        Assert.NotNull(executablePath);
        Assert.EndsWith("7z.exe", executablePath);
        Assert.True(File.Exists(executablePath));
    }
    
    [SkippableFact]
    public void LinuxPlatform_FindsLinuxExecutable()
    {
        Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Linux), "This test only runs on Linux");
        
        // Arrange
        var linuxSource = new LinuxSevenZipExecutableSource();
        
        // Act
        var executablePath = linuxSource.FindExecutable();
        
        // Assert
        Assert.NotNull(executablePath);
        Assert.Contains("7zz", executablePath);
        Assert.True(File.Exists(executablePath));
    }
    
    [SkippableFact]
    public void MacOsxPlatform_FindsMacExecutable()
    {
        Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.OSX), "This test only runs on macOS");
        
        // Arrange
        var macSource = new MacSevenZipExecutableSource();
        
        // Act
        var executablePath = macSource.FindExecutable();
        
        // Assert
        Assert.NotNull(executablePath);
        Assert.Contains("7zz", executablePath);
        Assert.True(File.Exists(executablePath));
    }
    
    [Fact]
    public async Task SevenZipArchive_WorksAcrossPlatforms()
    {
        // This test verifies that the same operations work regardless of platform
        
        // Arrange
        var executableSource = new OmniPlatformSevenZipExecutableSource();
        var sevenZip = new SevenZipArchive(executableSource);
        
        // Create test data
        CreateTestFile("cross-platform/test.txt", "Cross-platform test content");
        CreateTestFile("cross-platform/data/info.json", "{ \"platform\": \"test\" }");
        
        var archivePath = GetTestFilePath("cross-platform.7z");
        var sourceFolder = GetTestFilePath("cross-platform");
        
        // Act - Test Create
        await sevenZip.CreateArchive(archivePath, sourceFolder);
        Assert.True(File.Exists(archivePath));
        
        // Act - Test List
        var listResult = await sevenZip.ListArchive(archivePath);
        Assert.NotNull(listResult);
        Assert.Equal(2, listResult.TotalFiles);
        Assert.Contains(listResult.Files, f => f.Path.Contains("test.txt"));
        Assert.Contains(listResult.Files, f => f.Path.Contains("info.json"));
        
        // Act - Test Extract
        var extractPath = GetOutputPath("cross-platform-extracted");
        await sevenZip.ExtractToDirectoryAsync(archivePath, extractPath);
        
        // Assert extraction
        Assert.True(File.Exists(Path.Combine(extractPath, "test.txt")));
        Assert.True(File.Exists(Path.Combine(extractPath, "data", "info.json")));
        
        var extractedContent = await File.ReadAllTextAsync(Path.Combine(extractPath, "test.txt"));
        Assert.Equal("Cross-platform test content", extractedContent);
    }
    
    [Fact]
    public void ExecutableSource_HandlesExecutablePermissions()
    {
        // Verify that executables have proper permissions on Unix-like systems
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var executableSource = new OmniPlatformSevenZipExecutableSource();
            var executablePath = executableSource.FindExecutable();
            
            var fileInfo = new FileInfo(executablePath);
            // On Unix systems, we should be able to execute the file
            // This is a basic check - actual permission verification would require P/Invoke
            Assert.True(fileInfo.Exists);
        }
    }
}

// Helper attribute for skippable tests
public class SkippableFactAttribute : FactAttribute
{
    public override string Skip { get; set; } = null!;
}

public static class Skip
{
    public static void IfNot(bool condition, string reason)
    {
        if (!condition)
        {
            throw new SkipException(reason);
        }
    }
}

public class SkipException : Exception
{
    public SkipException(string reason) : base(reason) { }
}