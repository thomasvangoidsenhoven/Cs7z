# CLAUDE.md
This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.
This project aims to integrate the popular 7-zip library in C# using a console wrapper

## Project Structure

This is a .NET 9 C# solution with two projects:
- **Cs7z.Core**: Class library containing the core functionality
- **Cs7z.Console**: Console application that serves as the entry point

The solution follows standard .NET project conventions with nullable reference types enabled.

## Common Commands

### Build
```bash
dotnet build
```

### Run Console Application
```bash
dotnet run --project Cs7z.Console
```

### Build in Release Mode
```bash
dotnet build --configuration Release
```

### Clean Build Artifacts
```bash
dotnet clean
```

### Restore NuGet Packages
```bash
dotnet restore
```

## Development Notes

- Target framework: .NET 9.0
- Nullable reference types are enabled across all projects
- ImplicitUsings are enabled, so common System namespaces are automatically imported
- The project appears to be in early development stages with minimal implementation