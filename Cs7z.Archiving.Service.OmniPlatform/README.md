# Cs7z.Archiving.Service.OmniPlatform

A cross-platform .NET 9 wrapper for 7-zip command-line executable with automatic platform detection.

## Features

- **Cross-platform support**: Windows, Linux, and macOS
- **Multi-architecture support**: x86, x64, ARM, and ARM64
- **Automatic platform detection**: Selects the appropriate 7-zip executable based on the runtime environment
- **Zero external dependencies**: All 7-zip executables are bundled with the package
- **Async/await support**: All operations are asynchronous
- **Compression levels**: Support for all 7-zip compression levels (Store, Fastest, Fast, Normal, Maximum, Ultra)

## Installation

```bash
dotnet add package Cs7z.Archiving.Service.OmniPlatform
```

## Usage

```csharp
using Cs7z.Archiving.Core;
using Cs7z.Archiving.Service.OmniPlatform;

// Initialize with automatic platform detection
var executableSource = new OmniPlatformSevenZipExecutableSource();
var archive = new SevenZipArchive(executableSource);

// Extract an archive
await archive.ExtractToDirectoryAsync("archive.7z", "output_directory");

// Create an archive with default compression (Normal)
await archive.CreateArchive("output.7z", "source_directory");

// Create an archive with specific compression level
await archive.CreateArchive("output.7z", "source_directory", CompressionLevel.Ultra);

// List archive contents
var contents = await archive.ListArchive("archive.7z");
foreach (var file in contents.Files)
{
    Console.WriteLine($"{file.Name} - {file.Size} bytes");
}
```

## Supported Platforms

| Platform | Architectures |
|----------|--------------|
| Windows  | x86, x64, ARM64 |
| Linux    | x86, x64, ARM, ARM64 |
| macOS    | x64, ARM64 |

## License

This package is licensed under the MIT License. The bundled 7-zip executables are licensed under the GNU LGPL license.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.