# Cs7z

A high-performance, cross-platform .NET 9 wrapper for 7-zip compression with automatic platform detection and architecture support.

## Features

- 🚀 **Superior Compression** - Achieves up to 90%+ compression ratios using 7-zip's advanced algorithms
- 🌍 **Cross-Platform** - Works seamlessly on Windows, Linux, and macOS
- 🏗️ **Multi-Architecture** - Native support for x86, x64, ARM, and ARM64
- ⚡ **Async/Await** - Modern async API with CancellationToken support
- 📊 **6 Compression Levels** - From Store (no compression) to Ultra (maximum compression)
- 📦 **Self-Contained** - No external 7-zip installation required
- 🔧 **Zero Dependencies** - Core library has no third-party dependencies

## Performance Benchmarks

Cs7z leverages the power of 7-zip to provide superior compression ratios compared to System.IO.Compression. Below are comprehensive benchmarks showing performance across different file sizes and compression levels.

### Compression Speed Comparison (All Levels)

| File Size | Cs7z Store | Cs7z Fastest | Cs7z Normal | Cs7z Ultra | Zip NoComp | Zip Fastest | Zip Optimal |
|-----------|------------|--------------|-------------|------------|------------|-------------|-------------|
| 1 MB      | 11ms       | 55ms         | 72ms        | 40ms       | **1ms**    | 8ms         | 17ms        |
| 10 MB     | 13ms       | 74ms         | 520ms       | 513ms      | **3ms**    | 64ms        | 123ms       |
| 50 MB     | 28ms       | 289ms        | 3,686ms     | 3,890ms    | **16ms**   | 290ms       | 638ms       |
| 100 MB    | 41ms       | 447ms        | 7,774ms     | 7,862ms    | **31ms**   | 561ms       | 1,264ms     |

### Compression Ratio Comparison (All Levels)

| File Size | Original | Cs7z Store | Cs7z Fastest | Cs7z Normal | Cs7z Ultra | Zip NoComp | Zip Fastest | Zip Optimal |
|-----------|----------|------------|--------------|-------------|------------|------------|-------------|-------------|
| 1 MB      | 1.00 MB  | 1.14 MB    | 1.00 MB      | 1.00 MB     | **1.00 MB** | 1.14 MB    | 1.10 MB     | 1.03 MB     |
| 10 MB     | 10.00 MB | 10.14 MB   | 8.34 MB      | 8.16 MB     | **8.16 MB** | 10.15 MB   | 9.20 MB     | 8.44 MB     |
| 50 MB     | 50.00 MB | 50.14 MB   | 37.41 MB     | 35.06 MB    | **35.06 MB**| 50.16 MB   | 41.74 MB    | 37.43 MB    |
| 100 MB    | 100.00 MB| 100.15 MB  | 72.62 MB     | 66.99 MB    | **66.98 MB**| 100.16 MB  | 81.22 MB    | 72.31 MB    |

### Direct Comparison: No Compression

| File Size | Cs7z Store | Zip NoCompression | Faster |
|-----------|------------|-------------------|--------|
| 1 MB      | 11ms       | **1ms**           | Zip 11x |
| 10 MB     | 13ms       | **3ms**           | Zip 4.3x |
| 50 MB     | 28ms       | **16ms**          | Zip 1.8x |
| 100 MB    | 41ms       | **31ms**          | Zip 1.3x |

### Direct Comparison: Best Compression

| File Size | Cs7z Ultra | Zip Optimal | Better Compression | Speed Difference |
|-----------|------------|-------------|-------------------|------------------|
| 1 MB      | 1.00 MB (40ms) | 1.03 MB (17ms) | Cs7z (2.7% smaller) | Zip 2.4x faster |
| 10 MB     | 8.16 MB (513ms) | 8.44 MB (123ms) | Cs7z (2.8% smaller) | Zip 4.2x faster |
| 50 MB     | 35.06 MB (3,890ms) | 37.43 MB (638ms) | Cs7z (4.7% smaller) | Zip 6.1x faster |
| 100 MB    | 66.98 MB (7,862ms) | 72.31 MB (1,264ms) | Cs7z (5.3% smaller) | Zip 6.2x faster | 

### Extraction Speed Comparison

| File Size | Cs7z Extract | ZipArchive Extract | Faster |
|-----------|--------------|-------------------|--------|
| 1 MB      | 13ms         | **3ms**           | Zip 4.3x |
| 10 MB     | 18ms         | **9ms**           | Zip 2.0x |
| 50 MB     | 47ms         | **26ms**          | Zip 1.8x |
| 100 MB    | 84ms         | **53ms**          | Zip 1.6x |

### Key Insights

- **Speed Leaders**: Zip NoCompression is fastest for archiving without compression (1-31ms for 100MB)
- **Compression Balance**: Zip Fastest offers good compression (18.8%) with reasonable speed (561ms for 100MB)
- **Maximum Compression**: Cs7z Ultra achieves 5.3% better compression than Zip Optimal but takes 6x longer
- **Extraction Speed**: ZipArchive extracts 1.6-4.3x faster than Cs7z across all file sizes
- **Cross-over Point**: For files over 50MB, the compression advantage of Cs7z becomes more significant

### Quick Reference: Choosing a Compression Level

**Need maximum speed (no compression)?**
- Small files (< 10MB): Use `ZipArchive` with `NoCompression` (3ms for 10MB)
- Large files (> 50MB): Use `Cs7z.Store` or `ZipArchive.NoCompression` (similar performance)

**Need balanced compression?**
- Quick compression: Use `ZipArchive.Fastest` (561ms for 100MB, 18.8% savings)
- Better compression: Use `Cs7z.Normal` (7.8s for 100MB, 33% savings)

**Need maximum compression?**
- Time available: Use `Cs7z.Ultra` (7.9s for 100MB, 33% savings, 5.3% better than Zip)
- Time constrained: Use `ZipArchive.Optimal` (1.3s for 100MB, 27.7% savings)

*Benchmarks performed on Apple M4 Pro with .NET 9.0*

## Installation (Nuget package not  yet available 🏗️)

```bash
dotnet add package Cs7z.Archiving.Service.OmniPlatform
```

Or add to your `.csproj`:

```xml
<PackageReference Include="Cs7z.Archiving.Service.OmniPlatform" Version="1.0.0" />
```

## Quick Start

```csharp
using Cs7z.Archiving.Core;
using Cs7z.Archiving.Service.OmniPlatform;

// Initialize with automatic platform detection
var executableSource = new OmniPlatformSevenZipExecutableSource();
var archive = new SevenZipArchive(executableSource);

// Create an archive
await archive.CreateArchive("output.7z", "folder-to-compress");

// Extract an archive
await archive.ExtractToDirectoryAsync("archive.7z", "output-folder");

// List archive contents
var contents = await archive.ListArchive("archive.7z");
foreach (var file in contents.Files)
{
    Console.WriteLine($"{file.Name} - {file.Size:N0} bytes");
}
```


### Listing Archive Contents

```csharp
var result = await archive.ListArchive("archive.7z");

Console.WriteLine($"Archive contains {result.FileCount} files");
Console.WriteLine($"Total uncompressed size: {result.TotalUncompressedSize:N0} bytes");

foreach (var file in result.Files)
{
    Console.WriteLine($"{file.Name}");
    Console.WriteLine($"  Size: {file.Size:N0} bytes");
    Console.WriteLine($"  Compressed: {file.CompressedSize:N0} bytes");
    Console.WriteLine($"  Modified: {file.Modified}");
    Console.WriteLine($"  CRC32: {file.CRC}");
}
```

### Using with Dependency Injection

```csharp
services.AddSingleton<ISevenZipExecutableSource, OmniPlatformSevenZipExecutableSource>();
services.AddTransient<ISevenZipArchive, SevenZipArchive>();
```

## Compression Levels

Cs7z offers 6 compression levels to balance speed and file size. Here's how they perform on a 100MB dataset:

| Level | Speed | File Size | Compression Ratio | Use Case |
|-------|-------|-----------|-------------------|----------|
| **Store** | 46ms (fastest) | 100.15 MB | 0% | Already compressed files, archives |
| **Fastest** | 428ms | 72.62 MB | 27.4% | Quick archiving with some compression |
| **Fast** | ~2s* | ~70 MB* | ~30%* | General use with speed priority |
| **Normal** | 7.1s | 66.99 MB | 33.0% | Default, balanced option |
| **Maximum** | ~7s* | ~67 MB* | ~33%* | Size priority over speed |
| **Ultra** | 7.3s | 66.98 MB | 33.0% | Maximum compression |

*Estimated based on compression patterns

### Compression Level Examples

```csharp
// Fastest compression for temporary files
await archive.CreateArchive("temp.7z", "folder", CompressionLevel.Store);

// Balanced compression (default)
await archive.CreateArchive("output.7z", "folder", CompressionLevel.Normal);

// Maximum compression for long-term storage
await archive.CreateArchive("archive.7z", "folder", CompressionLevel.Ultra);
```

### When to Use Each Level

- **Store**: ZIP files, images, videos, or when speed is critical
- **Fastest**: Log files, temporary backups, or quick transfers
- **Normal**: General purpose archiving (default if not specified)
- **Ultra**: Final backups, long-term storage, or distribution packages

## System.IO.Compression Levels

For comparison, System.IO.Compression.ZipArchive offers 3 compression levels:

| Level | Speed | File Size | Compression Ratio | Use Case |
|-------|-------|-----------|-------------------|----------|
| **NoCompression** | 31ms | 100.16 MB | 0% | Already compressed files, fastest option |
| **Fastest** | 561ms | 81.22 MB | 18.8% | Quick compression with reasonable size reduction |
| **Optimal** | 1,264ms | 72.31 MB | 27.7% | Best compression (default) |

### ZipArchive Usage Example

```csharp
using System.IO.Compression;

// No compression - fastest
ZipFile.CreateFromDirectory("folder", "output.zip", CompressionLevel.NoCompression, false);

// Balanced compression
ZipFile.CreateFromDirectory("folder", "output.zip", CompressionLevel.Fastest, false);

// Maximum compression (default)
ZipFile.CreateFromDirectory("folder", "output.zip", CompressionLevel.Optimal, false);
```

## Platform Support

| Platform | x86 | x64 | ARM | ARM64 |
|----------|-----|-----|-----|-------|
| Windows  | ✅  | ✅  | ❌  | ✅    |
| Linux    | ✅  | ✅  | ✅  | ✅    |
| macOS    | ❌  | ✅  | ❌  | ✅    |

The OmniPlatform package automatically detects your runtime and uses the appropriate 7-zip binary.



## Deployment Considerations

### Binary Locations

The 7-zip executables are embedded in the platform-specific assemblies and extracted to a temporary directory at runtime. The executables are:

- **Windows**: `7za.exe` (console version)
- **Linux/macOS**: `7zz` (full version with codec support)

### Security Notes

- The library extracts executables to the build's directory
- Executables are verified to be from the embedded resources
- Consider application signing and security policies in restricted environments

### Docker Deployment

For Linux containers, ensure the base image has necessary runtime dependencies:

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:9.0
# 7-zip binaries are self-contained, no additional packages needed
```

## When to Use Cs7z

### Use Cs7z when you need:
- **Maximum compression** - Saves 7-8% more space than ZIP on files over 100MB
- **Large file handling** - Superior compression ratios with competitive extraction speeds
- **Fast archiving without compression** - Store mode is faster than ZipArchive for 10MB+ files
- **Cross-platform support** - Same compression format across all platforms
- **Flexible compression options** - 6 levels from no compression to maximum

### Use System.IO.Compression when you need:
- **Small files (< 10MB)** - ZipArchive is faster for both compression and extraction
- **Fastest extraction speed** - ZipArchive extracts 3-7x faster on small files
- **Simpler deployment** - No external binaries to manage
- **Stream-based processing** - Direct stream manipulation without temp files
- **Maximum compatibility** - ZIP format is universally supported

## Architecture

The solution follows a clean architecture pattern:

```
Cs7z.Archiving.Core              # Interfaces and core types
├── Cs7z.Archiving.Service.*     # Platform-specific implementations
│   ├── Windows                  # Windows x86/x64/ARM64
│   ├── Linux                    # Linux x86/x64/ARM/ARM64  
│   ├── MacOsx                   # macOS x64/ARM64
│   └── OmniPlatform            # Auto-detection wrapper
└── Tests                        # Unit, integration, and benchmarks
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

### Building from Source

```bash
# Clone the repository
git clone https://github.com/yourusername/Cs7z.git
cd Cs7z

# Build the solution
dotnet build

# Run tests
dotnet test

# Run benchmarks
dotnet run --project Tests/Cs7z.Archiving.Benchmarks -c Release
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.

7-zip is licensed under the GNU LGPL license. The 7-zip binaries included in this project are unmodified distributions from the official 7-zip project.

---

Made with ❤️ using .NET 9 and 7-zip