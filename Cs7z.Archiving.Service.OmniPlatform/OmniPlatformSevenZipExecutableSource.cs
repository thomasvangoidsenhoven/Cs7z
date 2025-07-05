using System.Runtime.InteropServices;
using Cs7z.Archiving.Core;
using Cs7z.Archiving.Service.Windows;
using Cs7z.Archiving.Service.Linux;
using Cs7z.Archiving.Service.MacOsx;

namespace Cs7z.Archiving.Service.OmniPlatform;

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