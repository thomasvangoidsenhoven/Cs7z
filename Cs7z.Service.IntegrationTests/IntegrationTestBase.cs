using System.IO;
using Xunit;

namespace Cs7z.Service.IntegrationTests;

public abstract class IntegrationTestBase : IDisposable
{
    protected readonly string TestDirectory;
    protected readonly string OutputDirectory;
    
    protected IntegrationTestBase()
    {
        var baseDirectory = Path.Combine(Path.GetTempPath(), "Cs7zIntegrationTests", Guid.NewGuid().ToString());
        TestDirectory = Path.Combine(baseDirectory, "TestData");
        OutputDirectory = Path.Combine(baseDirectory, "Output");
        
        Directory.CreateDirectory(TestDirectory);
        Directory.CreateDirectory(OutputDirectory);
    }
    
    protected string GetTestFilePath(string fileName)
    {
        return Path.Combine(TestDirectory, fileName);
    }
    
    protected string GetOutputPath(string fileName)
    {
        return Path.Combine(OutputDirectory, fileName);
    }
    
    protected void CreateTestFile(string fileName, string content)
    {
        var filePath = GetTestFilePath(fileName);
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllText(filePath, content);
    }
    
    protected void CreateTestDirectory(string directoryName)
    {
        Directory.CreateDirectory(GetTestFilePath(directoryName));
    }
    
    protected bool FileExists(string fileName)
    {
        return File.Exists(GetOutputPath(fileName));
    }
    
    protected bool DirectoryExists(string directoryName)
    {
        return Directory.Exists(GetOutputPath(directoryName));
    }
    
    protected string ReadFile(string fileName)
    {
        return File.ReadAllText(GetOutputPath(fileName));
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                if (Directory.Exists(Path.GetDirectoryName(TestDirectory)))
                {
                    Directory.Delete(Path.GetDirectoryName(TestDirectory)!, true);
                }
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }
}