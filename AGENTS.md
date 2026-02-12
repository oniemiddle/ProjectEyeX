# Environment setup
Run environment bootstrap first:

bash .codex/setup.sh

# Solution location
The .NET solution is located under the src directory.

# Restore
cd src
dotnet restore

# Build
dotnet build --configuration Release --no-restore

# Test
dotnet test --no-build --configuration Release
