# Cs7z Integration Tests

This project contains integration tests for the SevenZipArchive class using the OmniPlatform executable source.

## Test Structure

### Test Classes

1. **ExtractToDirectoryAsyncIntegrationTests**
   - Tests archive extraction functionality
   - Validates extraction of ZIP and 7z archives
   - Tests error handling and cancellation

2. **CreateArchiveIntegrationTests**
   - Tests archive creation functionality
   - Validates directory structure preservation
   - Tests special characters and empty folders

3. **ListArchiveIntegrationTests**
   - Tests archive listing functionality
   - Validates file information parsing
   - Tests various archive formats and structures

4. **PlatformSpecificTests**
   - Tests platform detection and executable selection
   - Validates cross-platform functionality
   - Uses conditional test execution based on OS

### Test Infrastructure

- **IntegrationTestBase**: Base class providing temporary directory management and cleanup
- **TestArchiveHelper**: Utility class for creating test archives with various content
- **Platform-specific skip attributes**: For conditional test execution

## Running Tests

### Run all tests
```bash
dotnet test
```

### Run specific test class
```bash
dotnet test --filter "FullyQualifiedName~ExtractToDirectoryAsync"
```

### Run with detailed output
```bash
dotnet test -v n
```

## Test Data

Tests create temporary files and archives in the system temp directory. All test data is automatically cleaned up after test execution.

## Requirements

- .NET 9.0 SDK
- 7-zip executable available (provided by platform-specific packages)
- Write access to system temp directory

## Test Coverage

The integration tests cover:
- Archive creation from directories
- Archive extraction to directories
- Archive content listing
- Error handling (corrupted files, missing files, invalid paths)
- Cancellation token support
- Cross-platform compatibility
- Special characters in file names
- Empty directories and complex folder structures