#!/bin/bash

# Check if both version and API key are provided as arguments
if [ $# -ne 2 ]; then
    echo "Error: Please provide both version number and API key."
    echo "Usage: $0 <version> <api_key>"
    exit 1
fi

VERSION=$1
API_KEY=$2

# Set the GitHub NuGet source name (as configured in your NuGet.config)
GITHUB_SOURCE="https://nuget.pkg.github.com/utconnect/index.json"

# Array of project paths
PROJECT="./Coffer/Coffer.csproj"

echo "Pushing packages with version: $VERSION"

dotnet build "$PROJECT" -c Release
dotnet pack "$PROJECT" -c Release -p:PackageVersion=$VERSION -o ./nupkgs

PACKAGE_NAME=Utconnect.$(basename "$PROJECT" .csproj)
dotnet nuget push "./nupkgs/${PACKAGE_NAME}.${VERSION}.nupkg" --source "$GITHUB_SOURCE" --api-key "$API_KEY"

echo "Package has been pushed to GitHub NuGet"