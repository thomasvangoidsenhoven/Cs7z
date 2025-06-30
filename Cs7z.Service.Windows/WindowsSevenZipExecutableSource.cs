using System.Reflection;
using Cs7z.Core;

namespace Cs7z.Service.Windows;

public class WindowsSevenZipExecutableSource : ISevenZipExecutableSource
{
    public string FindExecutable()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
        
        if (string.IsNullOrEmpty(assemblyDirectory))
            throw new InvalidOperationException("Unable to determine assembly directory");

        var sevenZipPath = Path.Combine(assemblyDirectory, "Source", "Windows", "x86", "7za.exe");
        
        if (!File.Exists(sevenZipPath))
            throw new FileNotFoundException($"7za.exe executable not found at: {sevenZipPath}");

        return sevenZipPath;
    }
}