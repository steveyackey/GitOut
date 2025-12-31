# Contributing to GitOut

Thank you for your interest in contributing to GitOut! This document provides guidelines and instructions for developers.

## Prerequisites

- **.NET 10 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Git** - Must be installed and available on PATH
- A terminal that supports ANSI color codes (most modern terminals)

## Getting Started

### Clone and Build

```bash
# Clone the repository
git clone https://github.com/steveyackey/GitOut.git
cd GitOut

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test

# Run the game
dotnet run --project src/GitOut.Console
```

### Running Tests

```bash
# Run all tests
dotnet test GitOut.sln

# Run tests with detailed output
dotnet test GitOut.sln --logger "console;verbosity=detailed"

# Run specific test by filter
dotnet test GitOut.sln --filter "FullyQualifiedName~TestName"

# Run all tests in a specific class
dotnet test GitOut.sln --filter "FullyQualifiedName~ClassName"
```

## Repository Structure

```
GitOut/
├── src/
│   ├── GitOut.Domain/          # Core business logic (no dependencies)
│   ├── GitOut.Application/     # Use cases and orchestration
│   ├── GitOut.Infrastructure/  # Git execution, I/O, persistence
│   └── GitOut.Console/         # Spectre.Console TUI
├── tests/
│   ├── GitOut.Domain.Tests/    # Unit tests
│   └── GitOut.Infrastructure.Tests/  # Integration tests
├── plans/                      # Internal planning & architecture docs
├── eng/                        # Build and publishing scripts
├── README.md                   # User-facing documentation
├── CONTRIBUTING.md             # This file
├── LICENSE                     # MIT License
└── CLAUDE.md                   # AI agent guidance
```

## Architecture Overview

GitOut follows **Clean Architecture** principles with strict dependency rules:

- **Domain Layer** - Core entities and interfaces (zero external dependencies)
- **Application Layer** - Use cases (depends only on Domain)
- **Infrastructure Layer** - External concerns: git, file I/O (depends on Domain + Application)
- **Console Layer** - UI presentation (depends on all layers)

**Critical Rule**: Never violate layer dependencies. Infrastructure implements interfaces defined in Domain/Application.

## Adding Content

### Adding a New Room

Rooms are created using the factory pattern. Each room has its own factory class.

1. Create a new factory class in `src/GitOut.Infrastructure/Persistence/RoomFactories/`
2. Implement the factory to return a configured `Room` object
3. Register the factory in `RoomRepository.cs`

Example:

```csharp
public class Room24Factory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room24Factory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor;
    }

    public async Task<Room> CreateAsync()
    {
        var challenge = new RepositoryChallenge(_gitExecutor)
        {
            // Configure challenge...
        };
        
        return new Room
        {
            Id = "room_24",
            Title = "Room Title",
            Description = "Room description...",
            Challenge = challenge,
            Exits = new Dictionary<string, string>
            {
                { "north", "room_25" }
            }
        };
    }
}
```

### Challenge Types

- **RepositoryChallenge** - Validate git repository state (commits, branches, files, etc.)
- **QuizChallenge** - Multiple choice questions about git concepts
- **ScenarioChallenge** - Story-driven challenges with narrative

## Native AOT Publishing

### Build Native AOT Binaries

**Linux/macOS:**
```bash
./eng/publish.sh
```

**Windows:**
```powershell
.\eng\publish.ps1
```

These scripts build self-contained Native AOT binaries for all target platforms:
- Windows: x64, ARM64
- Linux: x64, ARM64
- macOS: x64, ARM64

Artifacts are placed in `artifacts/publish/<rid>/`

### Testing AOT Builds

After publishing, test the binaries:

```bash
# Test the native binary
./artifacts/publish/linux-x64/GitOut.Console

# Verify it runs and basic functionality works
```

## Docker

### Build Docker Image

```bash
docker build -t gitout:local .
```

### Run Docker Image

```bash
# Basic run
docker run -it --rm gitout:local

# With volume mount for git challenges
docker run -it --rm -v "$PWD:/work" -w /work gitout:local
```

## Pull Request Guidelines

- **Keep PRs small and focused** - One feature or fix per PR
- **Write tests** - Maintain 85%+ test coverage
- **Follow existing code style** - Match the patterns in the codebase
- **Make commits atomic** - Each commit should be a logical unit that compiles and passes tests
- **Write descriptive commit messages** - Explain the "why" not just the "what"
- **Keep git history bisect-friendly** - Every commit should build and pass tests

## Testing Guidelines

- **Unit tests** - Domain and Application logic with mocked dependencies
- **Integration tests** - Real git command execution in temporary directories
- **Use temp directories** - Tests must clean up after themselves (use `TempDirectoryManager`)
- **Test isolation** - Each test should be independent

## Code Style

- Follow standard C# conventions
- Use nullable reference types
- Prefer explicit types over `var` for clarity (except obvious cases)
- Keep methods small and focused
- Use meaningful names

## Planning & Architecture

For information about project planning, architecture decisions, and development phases, see the [plans/](plans/) directory.

## Questions?

If you have questions or need help, please:
1. Check the [plans/](plans/) directory for architecture and design docs
2. Review existing code for patterns and examples
3. Open an issue for discussion

## License

By contributing to GitOut, you agree that your contributions will be licensed under the MIT License.
