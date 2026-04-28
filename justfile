# Fable.Logging build commands
# Install just: https://github.com/casey/just

set dotenv-load

src_path := "src"

# Default recipe - show available commands
default:
    @just --list

# Clean build output
clean:
    rm -rf {{src_path}}/Fable.Logging/obj {{src_path}}/Fable.Logging/bin
    rm -rf {{src_path}}/Fable.Logging.Structlog/obj {{src_path}}/Fable.Logging.Structlog/bin
    rm -rf {{src_path}}/Fable.Logging.Beam/obj {{src_path}}/Fable.Logging.Beam/bin
    rm -rf .fable

# Build all projects
build:
    dotnet build {{src_path}}/Fable.Logging
    dotnet build {{src_path}}/Fable.Logging.Structlog
    dotnet build {{src_path}}/Fable.Logging.Beam
    dotnet build test

# Create NuGet packages with version from root changelog
pack:
    #!/usr/bin/env bash
    set -euo pipefail
    VERSION=$(grep -m1 '^## ' CHANGELOG.md | sed 's/^## \([^ ]*\).*/\1/')
    dotnet pack src/Fable.Logging -c Release -o ./nupkgs -p:PackageVersion=$VERSION -p:InformationalVersion=$VERSION
    dotnet pack src/Fable.Logging.Structlog -c Release -o ./nupkgs -p:PackageVersion=$VERSION -p:InformationalVersion=$VERSION
    dotnet pack src/Fable.Logging.Beam -c Release -o ./nupkgs -p:PackageVersion=$VERSION -p:InformationalVersion=$VERSION

# Pack and push all packages to NuGet (used in CI)
release: pack
    dotnet nuget push './nupkgs/*.nupkg' -s https://api.nuget.org/v3/index.json -k $NUGET_KEY --skip-duplicate

# Run .NET tests
test:
    dotnet build test
    dotnet run --project test

# Format code with Fantomas
format:
    dotnet fantomas {{src_path}}

# Check code formatting without making changes
format-check:
    dotnet fantomas {{src_path}} --check

# Install .NET tools (Fable, Fantomas, etc.)
setup:
    dotnet tool restore

# Restore all dependencies
restore:
    dotnet paket install
    dotnet restore {{src_path}}/Fable.Logging
    dotnet restore {{src_path}}/Fable.Logging.Structlog
    dotnet restore {{src_path}}/Fable.Logging.Beam
    dotnet restore test

# Run EasyBuild.ShipIt for release management
shipit *args:
    dotnet shipit {{args}}
