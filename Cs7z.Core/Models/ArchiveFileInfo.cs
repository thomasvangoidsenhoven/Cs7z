namespace Cs7z.Core.Models;

public class ArchiveFileInfo
{
    public required string Path { get; init; }
    public required long Size { get; init; }
    public required long CompressedSize { get; init; }
    public required DateTime Modified { get; init; }
    public required string Attributes { get; init; }
    public required bool IsDirectory { get; init; }
    public string? Crc { get; init; }
    public string? Method { get; init; }
}

public class ArchiveListResult
{
    public required IReadOnlyList<ArchiveFileInfo> Files { get; init; }
    public required int TotalFiles { get; init; }
    public required int TotalDirectories { get; init; }
    public required long TotalSize { get; init; }
    public required long TotalCompressedSize { get; init; }
}