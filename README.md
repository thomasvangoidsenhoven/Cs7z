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

## Performance

Cs7z leverages the power of 7-zip to provide superior compression ratios compared to System.IO.Compression, especially for larger files:

### Compression Performance (1GB Dataset)
| Method | Time | File Size | Compression Ratio |
|--------|------|-----------|------------------|
| **Cs7z (Normal)** | 27.2s | 341 MB | 66.7% |
| System.IO.Compression | 12.2s | 512 MB | 50.0% |

### Extraction Performance (1GB Dataset)
| Method | Time |
|--------|------|
| **Cs7z** | 548ms |
| System.IO.Compression | 602ms |

*Benchmarks performed on Apple M4 Pro with .NET 9.0*

## Installation

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

## Compression Levels

Cs7z offers 6 compression levels to balance speed and file size:

| Level | Speed | Compression | Use Case |
|-------|-------|-------------|----------|
| **Store** | Fastest | None (0%) | Already compressed files |
| **Fastest** | Very Fast | Low (~40%) | Quick archiving |
| **Fast** | Fast | Medium (~50%) | General use with speed priority |
| **Normal** | Balanced | Good (~65%) | Default, balanced option |
| **Maximum** | Slow | High (~70%) | Size priority |
| **Ultra** | Slowest | Best (~75%+) | Maximum compression |

Example with compression level:

```csharp
await archive.CreateArchive("output.7z", "folder", CompressionLevel.Ultra);
```

## Platform Support

| Platform | x86 | x64 | ARM | ARM64 |
|----------|-----|-----|-----|-------|
| Windows  | ✅  | ✅  | ❌  | ✅    |
| Linux    | ✅  | ✅  | ✅  | ✅    |
| macOS    | ❌  | ✅  | ❌  | ✅    |

The OmniPlatform package automatically detects your runtime and uses the appropriate 7-zip binary.

## Advanced Usage

### Error Handling

```csharp
try
{
    await archive.ExtractToDirectoryAsync("archive.7z", "output");
}
catch (InvalidOperationException ex)
{
    // Handle extraction errors (corrupt archive, wrong password, etc.)
    Console.WriteLine($"Extraction failed: {ex.Message}");
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

## Deployment Considerations

### Binary Locations

The 7-zip executables are embedded in the platform-specific assemblies and extracted to a temporary directory at runtime. The executables are:

- **Windows**: `7za.exe` (console version)
- **Linux/macOS**: `7zz` (full version with codec support)

### Security Notes

- The library extracts executables to the system's temp directory
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
- **Maximum compression** - 7-zip typically achieves 20-50% better compression than ZIP
- **Large file handling** - Excellent performance with multi-gigabyte archives
- **Cross-platform support** - Same compression format across all platforms
- **Advanced features** - Support for various archive formats beyond ZIP

### Use System.IO.Compression when you need:
- **Maximum compatibility** - ZIP format is universally supported
- **Simpler deployment** - No external executables required
- **Smaller datasets** - Built-in compression is faster for small files
- **Stream-based processing** - Direct stream manipulation without temp files

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