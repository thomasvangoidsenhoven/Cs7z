using System.Diagnostics;

namespace Cs7z.Core;

public class DefaultSevenZipExecutableSource : ISevenZipExecutableSource
{
    private static readonly string[] PossiblePaths = new[]
    {
        "7z",
        "7za",
        @"C:\Program Files\7-Zip\7z.exe",
        @"C:\Program Files (x86)\7-Zip\7z.exe",
        "/usr/bin/7z",
        "/usr/local/bin/7z",
        "/opt/homebrew/bin/7z"
    };

    public string FindExecutable()
    {
        foreach (var path in PossiblePaths)
        {
            if (IsExecutableAvailable(path))
                return path;
        }

        throw new FileNotFoundException(
            "7-Zip executable not found. Please install 7-Zip or provide the path to the executable.");
    }

    private static bool IsExecutableAvailable(string executablePath)
    {
        try
        {
            if (Path.IsPathRooted(executablePath))
                return File.Exists(executablePath);

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = "--help",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            return process?.WaitForExit(1000) == true;
        }
        catch
        {
            return false;
        }
    }
}