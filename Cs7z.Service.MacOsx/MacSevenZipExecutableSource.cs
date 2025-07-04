using System.Reflection;
using System.Runtime.InteropServices;
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

        var architecture = GetArchitectureFolder();
        var sevenZipPath = Path.Combine(assemblyDirectory, "Source", "osx", architecture, "7zz");
        
        if (!File.Exists(sevenZipPath))
            throw new FileNotFoundException($"7zz executable not found at: {sevenZipPath}");

        return sevenZipPath;
    }

    private static string GetArchitectureFolder()
    {
        return RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            _ => throw new PlatformNotSupportedException($"Unsupported architecture: {RuntimeInformation.ProcessArchitecture}")
        };
    }
}