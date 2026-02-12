# Environment setup

Before running any build or tests, execute:

bash .codex/setup.sh

# Build instructions

Build the solution:

dotnet restore
dotnet build --configuration Release

# Test instructions

Run tests:

dotnet test
