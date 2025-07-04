using System.Diagnostics;
using System.Text;
using Cs7z.Core.Models;
using Cs7z.Core.Parsing;

namespace Cs7z.Core;

public class SevenZipArchive : ISevenZipArchive
{
    private readonly string _sevenZipPath;

    public SevenZipArchive(ISevenZipExecutableSource executableSource)
    {
        _sevenZipPath = executableSource.FindExecutable();
    }

    public async Task ExtractToDirectoryAsync(
        string archiveFilePath, 
        string destinationDirectoryName, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(archiveFilePath))
            throw new ArgumentException("Archive file path cannot be null or empty.", nameof(archiveFilePath));
        
        if (string.IsNullOrWhiteSpace(destinationDirectoryName))
            throw new ArgumentException("Destination directory cannot be null or empty.", nameof(destinationDirectoryName));

        if (!File.Exists(archiveFilePath))
            throw new FileNotFoundException($"Archive file not found: {archiveFilePath}");

        Directory.CreateDirectory(destinationDirectoryName);

        var arguments = $"x \"{archiveFilePath}\" -o\"{destinationDirectoryName}\" -y";
        await ExecuteSevenZipCommandAsync(arguments, cancellationToken);
    }

    public async Task CreateArchive(string archiveFilePath, string folder, CancellationToken cancellationToken = default)
    {
        await CreateArchive(archiveFilePath, folder, CompressionLevel.Normal, cancellationToken);
    }

    public async Task CreateArchive(string archiveFilePath, string folder, CompressionLevel compressionLevel, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(archiveFilePath))
            throw new ArgumentException("Archive file path cannot be null or empty.", nameof(archiveFilePath));
        
        if (string.IsNullOrWhiteSpace(folder))
            throw new ArgumentException("Folder path cannot be null or empty.", nameof(folder));

        if (!Directory.Exists(folder))
            throw new DirectoryNotFoundException($"Source folder not found: {folder}");

        var archiveDirectory = Path.GetDirectoryName(archiveFilePath);
        if (!string.IsNullOrEmpty(archiveDirectory))
        {
            Directory.CreateDirectory(archiveDirectory);
        }

        // Delete existing archive to ensure complete replacement
        if (File.Exists(archiveFilePath))
        {
            File.Delete(archiveFilePath);
        }

        var arguments = $"a \"{archiveFilePath}\" \"{Path.Combine(folder, "*")}\" -r -mx{(int)compressionLevel}";
        await ExecuteSevenZipCommandAsync(arguments, cancellationToken);
    }

    public async Task<ArchiveListResult> ListArchive(string archiveFilePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(archiveFilePath))
            throw new ArgumentException("Archive file path cannot be null or empty.", nameof(archiveFilePath));

        if (!File.Exists(archiveFilePath))
            throw new FileNotFoundException($"Archive file not found: {archiveFilePath}");

        var arguments = $"l \"{archiveFilePath}\"";
        var output = await ExecuteSevenZipCommandAsync(arguments, cancellationToken);
        return SevenZipOutputParser.ParseListOutput(output);
    }

    private async Task<string> ExecuteSevenZipCommandAsync(string arguments, CancellationToken cancellationToken)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = _sevenZipPath,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                outputBuilder.AppendLine(e.Data);
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                errorBuilder.AppendLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cancellationToken);

        var output = outputBuilder.ToString();
        var error = errorBuilder.ToString();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"7-Zip command failed with exit code {process.ExitCode}. " +
                $"Error: {error}. Output: {output}");
        }

        SevenZipOutputValidator.ValidateOutput(output, error);
        return output;
    }
}