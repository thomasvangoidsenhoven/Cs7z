using System.Reflection;
using Cs7z.Core;

namespace Cs7z.Service.MacOsx;

public class MacSevenZipExecutableSource : ISevenZipExecutableSource
{
    public string FindExecutable()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
        
        if (string.IsNullOrEmpty(assemblyDirectory))
            throw new InvalidOperationException("Unable to determine assembly directory");

        var sevenZipPath = Path.Combine(assemblyDirectory, "Source", "osx", "7zz");
        
        if (!File.Exists(sevenZipPath))
            throw new FileNotFoundException($"7zz executable not found at: {sevenZipPath}");

        return sevenZipPath;
    }
}