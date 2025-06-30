using Cs7z.Core.Models;

namespace Cs7z.Core;

public interface ISevenZipArchive
{
    Task ExtractToDirectoryAsync(
        string archiveFilePath, 
        string destinationDirectoryName, 
        CancellationToken cancellationToken = default);

    Task CreateArchive(string archiveFilePath, string folder, CancellationToken cancellationToken = default);
    Task<ArchiveListResult> ListArchive(string archiveFilePath, CancellationToken cancellationToken = default);
}