using System.Reflection;
using System.Runtime.InteropServices;
using Cs7z.Archiving.Core;

namespace Cs7z.Archiving.Service.Windows;

public class WindowsSevenZipExecutableSource : ISevenZipExecutableSource
{
    public string FindExecutable()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
        
        if (string.IsNullOrEmpty(assemblyDirectory))
            throw new InvalidOperationException("Unable to determine assembly directory");

        var architecture = GetArchitectureFolder();
        var sevenZipPath = Path.Combine(assemblyDirectory, "Source", "Windows", architecture, "7za.exe");
        
        if (!File.Exists(sevenZipPath))
            throw new FileNotFoundException($"7za.exe executable not found at: {sevenZipPath}");

        return sevenZipPath;
    }

    private static string GetArchitectureFolder()
    {
        return RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X86 => "x86",
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            _ => throw new PlatformNotSupportedException($"Unsupported architecture: {RuntimeInformation.ProcessArchitecture}")
        };
    }
}