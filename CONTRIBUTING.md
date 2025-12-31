# Contributing to GitOut

Thank you for your interest in contributing to GitOut!

## Development Setup

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Git

### Building

```bash
# Clone the repository
git clone https://github.com/YOUR_USERNAME/gitout.git
cd gitout

# Build the solution
dotnet build GitOut.sln

# Run the game
dotnet run --project src/GitOut.Console
```

### Testing

```bash
# Run all tests
dotnet test GitOut.sln

# Run with verbose output
dotnet test GitOut.sln --logger "console;verbosity=detailed"

# Run a specific test
dotnet test GitOut.sln --filter "FullyQualifiedName~TestName"

# Run tests for a specific class
dotnet test GitOut.sln --filter "FullyQualifiedName~ClassName"
```

## Architecture

GitOut follows **Clean Architecture** with strict layer dependencies:

```
Domain (No dependencies)
   ↑
Application (Domain only)
   ↑
Infrastructure (Domain + Application)
   ↑
Console (All layers)
```

### Layers

| Layer | Purpose | Key Files |
|-------|---------|-----------|
| **Domain** | Core entities, interfaces, challenge types | `Game.cs`, `Room.cs`, `IChallenge.cs` |
| **Application** | Use cases, game engine | `GameEngine.cs`, `StartGameUseCase.cs` |
| **Infrastructure** | Git execution, persistence, room factories | `GitCommandExecutor.cs`, `RoomRepository.cs` |
| **Console** | UI rendering, entry point | `Program.cs`, `GameRenderer.cs` |

For comprehensive architecture documentation, see [CLAUDE.md](CLAUDE.md).

## Adding New Rooms

1. **Create a factory** in `src/GitOut.Infrastructure/Persistence/RoomFactories/`:

```csharp
// RoomXXNameFactory.cs
public class RoomXXNameFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public RoomXXNameFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        var challenge = new RepositoryChallenge(
            id: "room-xx-challenge",
            description: "Challenge description",
            gitExecutor: _gitExecutor,
            // Configure challenge requirements...
        );

        var room = new Room(
            id: "room-xx",
            name: "Room Name",
            description: "Short description",
            narrative: "Full narrative text with [cyan]Spectre markup[/]...",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-next" } },
            isStartRoom: false,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}
```

2. **Register in RoomRepository** (`RoomRepository.cs`):

```csharp
var roomXXFactory = new RoomXXNameFactory(_gitExecutor);
_cachedRooms["room-xx"] = await roomXXFactory.CreateAsync();
```

3. **Add integration tests** in `tests/GitOut.Infrastructure.Tests/Integration/`:

```csharp
[Fact]
public async Task RoomXX_ShouldCompleteWhenConditionMet()
{
    // Arrange
    var room = await _roomRepository.GetRoomByIdAsync("room-xx");
    await room.Challenge!.SetupAsync(_workingDirectory);

    // Act - simulate player commands
    await _gitExecutor.ExecuteAsync("...", _workingDirectory);

    // Assert
    var result = await room.Challenge.ValidateAsync(_workingDirectory);
    result.IsSuccessful.Should().BeTrue();
}
```

## Commit Convention

This project uses **[Conventional Commits](https://conventionalcommits.org/)**:

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

### Types

| Type | Description | Version Bump |
|------|-------------|--------------|
| `feat` | New feature | MINOR |
| `fix` | Bug fix | PATCH |
| `docs` | Documentation only | - |
| `style` | Code style (formatting) | - |
| `refactor` | Code change (no bug fix or feature) | - |
| `perf` | Performance improvement | PATCH |
| `test` | Adding/updating tests | - |
| `build` | Build system or dependencies | - |
| `ci` | CI/CD configuration | - |
| `chore` | Other changes | - |

### Breaking Changes

Use `!` after type for breaking changes (triggers MAJOR version bump):

```
feat!: remove deprecated API
feat(room)!: change room ID format
```

### Examples

```
feat(room): add Room 24 for advanced worktree scenarios
fix(challenge): correct validation logic for merge conflicts
docs: update README with installation instructions
build: enable Native AOT compilation
ci: add GitHub Actions release workflow
```

### Scopes

- `room` - Room factories and content
- `challenge` - Challenge types and validation
- `engine` - GameEngine and command processing
- `ui` - Console rendering and input
- `save` - Progress persistence
- `git` - Git command execution

## Pull Request Process

1. **Fork** the repository
2. **Create a feature branch** from `main`
3. **Make your changes** following the conventions above
4. **Write/update tests** - aim for good coverage
5. **Ensure tests pass**: `dotnet test GitOut.sln`
6. **Submit PR** with a clear description

### PR Title Format

Use the same conventional commit format for PR titles:

```
feat(room): add Room 24 for worktree scenarios
```

## Testing Guidelines

- **Unit tests** (Domain/Application): Mock `IGitCommandExecutor`, test business logic
- **Integration tests** (Infrastructure): Use real git commands in temp directories
- **Always clean up** temp directories using `using` statements or `finally` blocks
- **Don't forget git config** in tests: commits require `user.email` and `user.name`

## Code Style

- Follow existing patterns in the codebase
- Use C# 14 features where appropriate
- Prefer immutability (records, readonly)
- Use meaningful names
- Keep methods focused and small

## Questions?

Open an issue for questions or discussions about contributing.
