using System.Reflection;
using Cs7z.Core;

namespace Cs7z.Service.Linux;

public class LinuxSevenZipExecutableSource : ISevenZipExecutableSource
{
    public string FindExecutable()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
        
        if (string.IsNullOrEmpty(assemblyDirectory))
            throw new InvalidOperationException("Unable to determine assembly directory");

        var sevenZipPath = Path.Combine(assemblyDirectory, "Source", "Linux", "7zz");
        
        if (!File.Exists(sevenZipPath))
            throw new FileNotFoundException($"7zz executable not found at: {sevenZipPath}");

        return sevenZipPath;
    }
}