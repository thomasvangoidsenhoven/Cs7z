using System.Globalization;
using Cs7z.Core.Models;

namespace Cs7z.Core.Parsing;

internal static class SevenZipOutputParser
{
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
            if (!inFilesSection && trimmedLine.StartsWith("Date") && trimmedLine.Contains("Time") && trimmedLine.Contains("Attr"))
            {
                inFilesSection = true;
                continue;
            }
            
            // Handle separator lines
            if (trimmedLine.StartsWith("---------------"))
            {
                // If we're in the files section and this is the second separator, we're done
                if (inFilesSection)
                {
                    // Don't immediately exit - there might be more content
                    // Just skip this separator line
                }
                continue;
            }
            
            // Only process lines within the files section
            if (!inFilesSection)
            {
                continue;
            }

            // Try to parse the file entry line
            var fileInfo = ParseFileLine(trimmedLine);
            if (fileInfo != null)
            {
                files.Add(fileInfo);
                
                if (fileInfo.IsDirectory)
                {
                    totalDirectories++;
                }
                else
                {
                    totalFiles++;
                    totalSize += fileInfo.Size;
                    totalCompressedSize += fileInfo.CompressedSize;
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

    /// <summary>
    /// Parses a single file entry line from 7-Zip output.
    /// Expected format: "2023-12-25 14:30:45 ....A         1024          512  folder/file.txt"
    /// </summary>
    private static ArchiveFileInfo? ParseFileLine(string line)
    {
        // Minimum length check - a valid line should have at least date, time, attributes, size, and name
        if (line.Length < 40)
            return null;

        // Skip summary lines - these have spaces where attributes should be
        // Summary line example: "2025-07-02 22:17:00                 72           96  3 files"
        // Real file/dir line:   "2023-12-25 14:30:45 D....            0            0  folder"
        // Check if position 20-24 (where attributes should be) contains only spaces
        if (line.Length > 24 && line.Substring(20, 5).Trim().Length == 0)
        {
            // This is likely a summary line
            return null;
        }

        try
        {
            // Parse date and time (first 19 characters: "2023-12-25 14:30:45")
            if (!TryParseDateTime(line, out var dateTime, out var nextPos))
                return null;

            // Skip whitespace after datetime
            nextPos = SkipWhitespace(line, nextPos);
            if (nextPos >= line.Length)
                return null;

            // Parse attributes (5 characters: "D...." or "....A")
            if (!TryParseAttributes(line, nextPos, out var isDirectory, out var attributes, out nextPos))
                return null;

            // Skip whitespace after attributes
            nextPos = SkipWhitespace(line, nextPos);
            if (nextPos >= line.Length)
                return null;

            // Parse size
            if (!TryParseNumber(line, nextPos, out var size, out nextPos))
                return null;

            // Skip whitespace after size
            nextPos = SkipWhitespace(line, nextPos);
            if (nextPos >= line.Length)
                return null;

            // Parse compressed size (optional - might be missing for some entries)
            long compressedSize = size; // Default to original size
            var savedPos = nextPos;
            
            // Try to parse compressed size, but if we can't, assume the rest is the filename
            if (TryParseNumber(line, nextPos, out var parsedCompressedSize, out var afterCompressedPos))
            {
                // Check if there's still content after (should be the filename)
                var afterWhitespace = SkipWhitespace(line, afterCompressedPos);
                if (afterWhitespace < line.Length)
                {
                    compressedSize = parsedCompressedSize;
                    nextPos = afterWhitespace;
                }
                else
                {
                    // No filename after, so this number is probably part of the filename
                    nextPos = savedPos;
                }
            }

            // The rest of the line is the file path (may contain spaces)
            var path = line.Substring(nextPos).Trim();
            if (string.IsNullOrEmpty(path))
                return null;

            return new ArchiveFileInfo
            {
                Path = path,
                Size = size,
                CompressedSize = compressedSize,
                Modified = dateTime,
                Attributes = attributes,
                IsDirectory = isDirectory,
                Crc = null // CRC is not always present in list output
            };
        }
        catch
        {
            // If any parsing error occurs, skip this line
            return null;
        }
    }

    /// <summary>
    /// Tries to parse the date and time from the beginning of the line.
    /// Expected format: "2023-12-25 14:30:45"
    /// </summary>
    private static bool TryParseDateTime(string line, out DateTime dateTime, out int nextPosition)
    {
        dateTime = default;
        nextPosition = 0;

        // Date time format is exactly 19 characters: "2023-12-25 14:30:45"
        if (line.Length < 19)
            return false;

        var dateTimeStr = line.Substring(0, 19);
        if (DateTime.TryParseExact(dateTimeStr, "yyyy-MM-dd HH:mm:ss", 
            CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
        {
            nextPosition = 19;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Tries to parse the attributes field.
    /// Expected format: "D...." for directories or "....A" for files (5 characters)
    /// </summary>
    private static bool TryParseAttributes(string line, int startPos, out bool isDirectory, 
        out string attributes, out int nextPosition)
    {
        isDirectory = false;
        attributes = string.Empty;
        nextPosition = startPos;

        // Need at least 5 characters for attributes
        if (startPos + 5 > line.Length)
            return false;

        var attrStr = line.Substring(startPos, 5);
        
        // First character indicates if it's a directory
        isDirectory = attrStr[0] == 'D';
        
        // The attributes field is exactly 5 characters
        attributes = attrStr;
        
        nextPosition = startPos + 5;
        return true;
    }

    /// <summary>
    /// Tries to parse a number (size or compressed size) from the line.
    /// </summary>
    private static bool TryParseNumber(string line, int startPos, out long number, out int nextPosition)
    {
        number = 0;
        nextPosition = startPos;

        // Find the end of the number
        int endPos = startPos;
        while (endPos < line.Length && char.IsDigit(line[endPos]))
        {
            endPos++;
        }

        // No digits found
        if (endPos == startPos)
            return false;

        var numberStr = line.Substring(startPos, endPos - startPos);
        if (long.TryParse(numberStr, out number))
        {
            nextPosition = endPos;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Skips whitespace characters and returns the position of the next non-whitespace character.
    /// </summary>
    private static int SkipWhitespace(string line, int startPos)
    {
        int pos = startPos;
        while (pos < line.Length && char.IsWhiteSpace(line[pos]))
        {
            pos++;
        }
        return pos;
    }
}