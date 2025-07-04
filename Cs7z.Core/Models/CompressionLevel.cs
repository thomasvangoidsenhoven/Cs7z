namespace Cs7z.Core.Models;

/// <summary>
/// Represents the compression level for creating archives.
/// </summary>
public enum CompressionLevel
{
    /// <summary>
    /// Store mode - no compression. Files are stored without compression.
    /// Fastest but results in largest file size.
    /// </summary>
    Store = 0,
    
    /// <summary>
    /// Fastest compression. Minimal compression ratio but very fast.
    /// </summary>
    Fastest = 1,
    
    /// <summary>
    /// Fast compression. Better compression than Fastest but still prioritizes speed.
    /// </summary>
    Fast = 3,
    
    /// <summary>
    /// Normal compression. Default compression level with balanced speed and compression ratio.
    /// </summary>
    Normal = 5,
    
    /// <summary>
    /// Maximum compression. Good compression ratio with reasonable speed.
    /// </summary>
    Maximum = 7,
    
    /// <summary>
    /// Ultra compression. Best compression ratio but slowest compression speed.
    /// </summary>
    Ultra = 9
}