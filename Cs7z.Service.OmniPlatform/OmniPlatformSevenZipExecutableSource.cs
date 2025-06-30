using System.Runtime.InteropServices;
using Cs7z.Core;
using Cs7z.Service.Windows;
using Cs7z.Service.Linux;
using Cs7z.Service.MacOsx;

namespace Cs7z.Service.OmniPlatform;

public class OmniPlatformSevenZipExecutableSource : ISevenZipExecutableSource
{
    public string FindExecutable()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var windowsSource = new WindowsSevenZipExecutableSource();
            return windowsSource.FindExecutable();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var linuxSource = new LinuxSevenZipExecutableSource();
            return linuxSource.FindExecutable();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var macSource = new MacSevenZipExecutableSource();
            return macSource.FindExecutable();
        }
        else
        {
            throw new PlatformNotSupportedException($"Platform not supported: {RuntimeInformation.OSDescription}");
        }
    }
}