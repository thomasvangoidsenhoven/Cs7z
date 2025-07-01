# Use the official .NET 9 runtime as the base image
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

# Use the .NET 9 SDK for building
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file and project files
COPY ["Cs7z.sln", "./"]
COPY ["Cs7z.Console/Cs7z.Console.csproj", "Cs7z.Console/"]
COPY ["Cs7z.Core/Cs7z.Core.csproj", "Cs7z.Core/"]
COPY ["Cs7z.Service.MacOsx/Cs7z.Service.MacOsx.csproj", "Cs7z.Service.MacOsx/"]
COPY ["Cs7z.Service.Linux/Cs7z.Service.Linux.csproj", "Cs7z.Service.Linux/"]
COPY ["Cs7z.Service.Windows/Cs7z.Service.Windows.csproj", "Cs7z.Service.Windows/"]
COPY ["Cs7z.Service.OmniPlatform/Cs7z.Service.OmniPlatform.csproj", "Cs7z.Service.OmniPlatform/"]

# Restore dependencies
RUN dotnet restore "Cs7z.Console/Cs7z.Console.csproj"

# Copy the source code
COPY . .

# Build the application
WORKDIR "/src/Cs7z.Console"
RUN dotnet build "Cs7z.Console.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Cs7z.Console.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Ensure all Linux 7zz executables have execute permissions for all architectures
RUN find /app/Source/Linux -name "7zz" -type f -exec chmod +x {} \; 2>/dev/null || true

ENTRYPOINT ["dotnet", "Cs7z.Console.dll"]