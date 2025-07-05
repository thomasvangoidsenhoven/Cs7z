# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Cs7z is a .NET 9 C# library that provides a cross-platform wrapper around the 7-zip command-line executable. It enables seamless integration of 7-zip archive functionality into .NET applications with support for Windows, Linux, and macOS.

## Solution Structure

The solution file is `Cs7z.Archiving.sln` and contains 11 projects organized into core functionality, platform-specific services, and test projects:

### Core Projects
- **Cs7z.Archiving.Core**: Main library with `ISevenZipArchive` interface and `SevenZipArchive` implementation
- **Cs7z.Archiving.Console**: Console application demonstrating library usage

### Platform Services
- **Cs7z.Archiving.Service.Windows**: Windows-specific implementation with x86/x64/ARM64 7za.exe binaries
- **Cs7z.Archiving.Service.Linux**: Linux-specific executable handling with x86/x64/ARM/ARM64 7zz binaries
- **Cs7z.Archiving.Service.MacOsx**: macOS-specific executable handling with x64/ARM64 7zz binaries
- **Cs7z.Archiving.Service.OmniPlatform**: Cross-platform abstraction that auto-selects the appropriate platform

### Test Projects
- **Tests/Cs7z.Archiving.Core.Tests**: Unit tests with xUnit and Moq
- **Cs7z.Archiving.Service.IntegrationTests**: Integration tests for actual 7-zip operations
- **Tests/Cs7z.Archiving.Benchmarks**: BenchmarkDotNet performance tests
- **Tests/Cs7z.Archiving.PerformanceConsole**: Performance testing console app

## Common Commands

### Build and Run
```bash
# Build the solution
dotnet build

# Run console application
dotnet run --project Cs7z.Archiving.Console

# Build in Release mode
dotnet build --configuration Release

# Clean build artifacts
dotnet clean

# Restore NuGet packages
dotnet restore
```

### Testing
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test Tests/Cs7z.Archiving.Core.Tests/Cs7z.Archiving.Core.Tests.csproj
dotnet test Cs7z.Archiving.Service.IntegrationTests/Cs7z.Archiving.Service.IntegrationTests.csproj

# Run tests with verbose output
dotnet test -v n

# Run specific test by name filter
dotnet test --filter "FullyQualifiedName~ExtractToDirectoryAsync"
dotnet test --filter "FullyQualifiedName~SevenZipArchiveTests"

# Run tests with code coverage
dotnet test /p:CollectCoverage=true
```

### Benchmarking
```bash
# Run benchmarks (must be in Release mode)
dotnet run --project Tests/Cs7z.Archiving.Benchmarks/Cs7z.Archiving.Benchmarks.csproj -c Release

# Run performance console
dotnet run --project Tests/Cs7z.Archiving.PerformanceConsole/Cs7z.Archiving.PerformanceConsole.csproj
```

### Code Quality
```bash
# Format code
dotnet format

# Check code style without fixing
dotnet format --verify-no-changes

# Run code analysis
dotnet build -p:EnableNETAnalyzers=true
```

## Architecture Overview

### Core Design
The library follows a clean architecture pattern with:
- **Interfaces** in Cs7z.Archiving.Core defining contracts (`ISevenZipArchive`, `ISevenZipExecutableSource`)
- **Platform-specific implementations** in separate service projects
- **OmniPlatform service** that uses `RuntimeInformation` to automatically select the correct platform implementation

### Key Components
1. **SevenZipArchive**: Main class providing async methods for:
   - `ExtractToDirectoryAsync()` - Extract archives
   - `CreateArchive()` - Create archives with optional compression level
   - `ListArchive()` - List archive contents with detailed file info

2. **SevenZipExecutableSource**: Platform-specific classes that provide the path to the 7-zip executable

3. **OutputParsers**: Utilities for parsing 7-zip command-line output into structured data

### Process Handling
All operations use `System.Diagnostics.Process` with:
- Async/await patterns throughout
- CancellationToken support
- Proper output/error stream handling
- Exit code validation

## Development Notes

- **Target Framework**: .NET 9.0
- **Language Features**: Nullable reference types enabled, ImplicitUsings enabled
- **Test Frameworks**: xUnit 2.9.2, Moq 4.20.70, BenchmarkDotNet 0.14.0
- **No External Dependencies**: Core library has zero third-party dependencies
- **Self-Contained**: Each platform service bundles the required 7-zip executables

## Current Status

- Main branch for PRs: `main`
- All projects use the `Cs7z.Archiving` namespace
- Recent work includes multi-architecture support, compression levels, and performance benchmarks