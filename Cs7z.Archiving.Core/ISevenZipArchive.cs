using Cs7z.Archiving.Core.Models;

namespace Cs7z.Archiving.Core;

public interface ISevenZipArchive
{
    Task ExtractToDirectoryAsync(
        string archiveFilePath, 
        string destinationDirectoryName, 
        CancellationToken cancellationToken = default);

    Task CreateArchive(string archiveFilePath, string folder, CancellationToken cancellationToken = default);
    Task CreateArchive(string archiveFilePath, string folder, CompressionLevel compressionLevel, CancellationToken cancellationToken = default);
    Task<ArchiveListResult> ListArchive(string archiveFilePath, CancellationToken cancellationToken = default);
}