namespace Cs7z.Core.Parsing;

internal static class SevenZipOutputValidator
{
    public static void ValidateOutput(string output, string error)
    {
        if (!string.IsNullOrEmpty(error))
        {
            if (error.Contains("ERROR") || error.Contains("FATAL"))
            {
                throw new InvalidOperationException($"7-Zip reported errors: {error}");
            }
        }

        if (string.IsNullOrWhiteSpace(output))
        {
            throw new InvalidOperationException("7-Zip produced no output, which may indicate a failure.");
        }

        var lowerOutput = output.ToLowerInvariant();
        
        if (lowerOutput.Contains("error") && lowerOutput.Contains("can not open"))
        {
            throw new FileNotFoundException("7-Zip could not open the archive file.");
        }
        
        if (lowerOutput.Contains("wrong password"))
        {
            throw new UnauthorizedAccessException("Archive requires a password or the provided password is incorrect.");
        }
        
        if (lowerOutput.Contains("unsupported method") || lowerOutput.Contains("unsupported archive"))
        {
            throw new NotSupportedException("Archive format or compression method is not supported.");
        }
        
        if (lowerOutput.Contains("crc failed") || lowerOutput.Contains("data error"))
        {
            throw new InvalidDataException("Archive data is corrupted.");
        }
        
        if (lowerOutput.Contains("disk full") || lowerOutput.Contains("not enough space"))
        {
            throw new IOException("Insufficient disk space to complete the operation.");
        }
    }
}