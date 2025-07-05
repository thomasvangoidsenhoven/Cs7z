# GitHub Workflows Setup

This directory contains GitHub Actions workflows for the Cs7z.Archiving project.

## Workflows

### 1. CI (Continuous Integration)
- **File**: `ci.yml`
- **Triggers**: Push to main/develop branches, Pull requests
- **Purpose**: Build and test on Windows, Linux, and macOS
- **Artifacts**: Creates NuGet packages on every build (not published)

### 2. Publish NuGet Package
- **File**: `publish-nuget.yml`
- **Triggers**: 
  - GitHub Release creation
  - Manual workflow dispatch
- **Purpose**: Build, test, and publish the NuGet package to NuGet.org

## Setup Instructions

### 1. Configure NuGet API Key

1. Go to [NuGet.org](https://www.nuget.org) and sign in
2. Click on your username → API Keys
3. Create a new API Key with the following settings:
   - Key Name: `Cs7z.Archiving.Service.OmniPlatform`
   - Expiration: Choose appropriate expiration
   - Scope: Push new packages and package versions
   - Glob Pattern: `Cs7z.Archiving.Service.OmniPlatform`
4. Copy the generated API key

### 2. Add Secret to GitHub Repository

1. Go to your GitHub repository
2. Navigate to Settings → Secrets and variables → Actions
3. Click "New repository secret"
4. Add the following secret:
   - Name: `NUGET_API_KEY`
   - Value: [Paste your NuGet API key]

### 3. Update Package Metadata

Before publishing, update the following in `Cs7z.Archiving.Service.OmniPlatform.csproj`:
- `<Authors>`: Your name or organization
- `<Company>`: Your company name
- `<PackageProjectUrl>`: Your GitHub repository URL
- `<RepositoryUrl>`: Your GitHub repository URL

## Publishing a Release

### Option 1: Via GitHub Release
1. Create a new tag following semantic versioning (e.g., `v1.0.0`)
2. Create a GitHub Release from the tag
3. The workflow will automatically trigger and publish to NuGet.org

### Option 2: Manual Workflow Dispatch
1. Go to Actions → Publish NuGet Package
2. Click "Run workflow"
3. Enter the version number (e.g., `1.0.0`)
4. Click "Run workflow"

## Version Strategy

- Release versions: `1.0.0`, `1.1.0`, `2.0.0`
- Preview versions: `1.0.0-preview.1`, `1.0.0-beta.1`
- Manual dispatch without version: Creates timestamped preview (e.g., `1.0.0-preview-20240705123456`)