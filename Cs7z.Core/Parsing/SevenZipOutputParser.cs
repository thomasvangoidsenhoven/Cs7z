using System.Globalization;
using System.Text.RegularExpressions;
using Cs7z.Core.Models;

namespace Cs7z.Core.Parsing;

internal static class SevenZipOutputParser
{
    /// <summary>
    /// Regex pattern to parse 7-Zip list output lines.
    /// 
    /// Pattern breakdown:
    /// ^(\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}) - Group 1: Date and time (e.g., "2023-12-25 14:30:45")
    /// \s+([D\.]) - Group 2: Directory indicator ("D" for directory, "." for file)
    /// ([A-Z\.]{4}) - Group 3: File attributes (4 chars, e.g., "RHA.", "....")
    /// \s+(\d+) - Group 4: File size in bytes (e.g., "1024")
    /// \s+(\d+)? - Group 5: Compressed size in bytes (optional, e.g., "512")
    /// \s*([A-F0-9]{8})? - Group 6: CRC32 checksum (optional, 8 hex chars, e.g., "A1B2C3D4")
    /// \s+(.+)$ - Group 7: File/directory path (e.g., "folder/file.txt")
    /// 
    /// Example lines this matches:
    /// "2023-12-25 14:30:45 ....A         1024      512  A1B2C3D4  folder/file.txt"
    /// "2023-12-25 14:30:45 D....            0        0           folder"
    /// "2023-12-25 14:30:46 .RH..         2048     1024  E5F6G7H8  hidden-file.dat"
    /// "2023-12-25 14:30:47 ....A          500      450           small-file.txt"
    /// </summary>
    private static readonly Regex FileLineRegex = new(
        @"^(\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2})\s+([D\.])([A-Z\.]{4})\s+(\d+)\s+(\d*)\s*(.+)$",
        RegexOptions.Compiled | RegexOptions.Multiline);

    /// <summary>
    /// Parses 7-Zip list output into structured data.
    /// 
    /// Expected 7-Zip output format:
    /// 
    /// 7-Zip 19.00 (x64) : Copyright (c) 1999-2018 Igor Pavlov : 2019-02-21
    /// 
    /// Scanning the drive for archives:
    /// 1 file, 12345 bytes (12 KiB)
    /// 
    /// Listing archive: example.zip
    /// 
    /// --
    /// Path = example.zip
    /// Type = zip
    /// Physical Size = 12345
    /// 
    ///    Date      Time    Attr         Size   Compressed  Name
    /// ------------------- ----- ------------ ------------  ------------------------
    /// 2023-12-25 14:30:45 ....A         1024          512  folder/file.txt
    /// 2023-12-25 14:30:45 D....            0            0  folder
    /// 2023-12-25 14:30:46 .RH..         2048         1024  hidden-file.dat
    /// ------------------- ----- ------------ ------------  ------------------------
    ///                                   3072         1536  2 files, 1 folders
    /// </summary>
    public static ArchiveListResult ParseListOutput(string output)
    {
        var files = new List<ArchiveFileInfo>();
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        bool inFilesSection = false;
        int totalFiles = 0;
        int totalDirectories = 0;
        long totalSize = 0;
        long totalCompressedSize = 0;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            // Look for the header line that starts the file listing section
            // Example: "   Date      Time    Attr         Size   Compressed  Name"
            if (trimmedLine.StartsWith("Date") && trimmedLine.Contains("Time") && trimmedLine.Contains("Attr"))
            {
                inFilesSection = true;
                continue;
            }
            
            // Skip separator lines like "------------------- ----- ------------ ------------  ------------------------"
            if (trimmedLine.StartsWith("---------------"))
            {
                if (inFilesSection)
                {
                    inFilesSection = false; // End of files section
                }
                continue;
            }
            
            // Only process lines within the files section
            if (!inFilesSection)
            {
                continue;
            }

            var match = FileLineRegex.Match(trimmedLine);
            if (match.Success)
            {
                var dateTimeStr = match.Groups[1].Value;           // "2023-12-25 14:30:45"
                var isDirectory = match.Groups[2].Value == "D";    // "D" or "."
                var attributes = match.Groups[3].Value;            // "RHA." or "...."
                var sizeStr = match.Groups[4].Value;               // "1024"
                var compressedSizeStr = match.Groups[5].Value.Trim(); // "512" (optional, may be spaces)
                var path = match.Groups[6].Value;                  // "folder/file.txt"

                if (DateTime.TryParseExact(dateTimeStr, "yyyy-MM-dd HH:mm:ss", 
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var modified) &&
                    long.TryParse(sizeStr, out var size))
                {
                    // Use original size if compressed size is not available
                    var compressedSize = string.IsNullOrEmpty(compressedSizeStr) ? size : 
                        long.TryParse(compressedSizeStr, out var cs) ? cs : size;

                    var fileInfo = new ArchiveFileInfo
                    {
                        Path = path,
                        Size = size,
                        CompressedSize = compressedSize,
                        Modified = modified,
                        Attributes = isDirectory ? "D" + attributes : attributes,
                        IsDirectory = isDirectory,
                        Crc = null // CRC is not always present in list output
                    };

                    files.Add(fileInfo);
                    
                    if (isDirectory)
                    {
                        totalDirectories++;
                    }
                    else
                    {
                        totalFiles++;
                        totalSize += size;
                        totalCompressedSize += compressedSize;
                    }
                }
            }
        }

        return new ArchiveListResult
        {
            Files = files.AsReadOnly(),
            TotalFiles = totalFiles,
            TotalDirectories = totalDirectories,
            TotalSize = totalSize,
            TotalCompressedSize = totalCompressedSize
        };
    }
}