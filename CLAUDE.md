# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

GitOut is a terminal-based dungeon crawler that teaches git commands through gameplay. Players navigate rooms, encounter challenges, and use **real git commands** to progress. The project is built with .NET 10, C# 14, and Spectre.Console for TUI, following clean architecture principles.

**Current Status**: Phase 4 complete (24 playable rooms, all 3 challenge types, save/load system fully integrated, factory-based room architecture). Covers basic through expert git concepts including init, add, commit, log, status, branch, merge, restore, conflict resolution, stash, cherry-pick, rebase, tag, reflog, remote repos, bisect, revert, worktree, blame, hooks, interactive staging, submodule, filter-branch, and a comprehensive final boss challenge.

## Essential Commands

### Build and Test
```bash
# Build the solution
dotnet build GitOut.sln

# Run all tests
dotnet test GitOut.sln

# Run tests with detailed output
dotnet test GitOut.sln --logger "console;verbosity=detailed"

# Run single test by filter
dotnet test GitOut.sln --filter "FullyQualifiedName~GitOut.Domain.Tests.Challenges.RepositoryChallengeTests.ValidateAsync_ShouldReturnSuccess_WhenGitInitRequired_AndInitialized"

# Run all tests in a specific class
dotnet test GitOut.sln --filter "FullyQualifiedName~GitCommandExecutorTests"

# Run the game
dotnet run --project src/GitOut.Console
```

### Test Organization
- **Unit tests**: `tests/GitOut.Domain.Tests/` - Test domain logic with mocked dependencies
- **Integration tests**: `tests/GitOut.Infrastructure.Tests/` - Test real git execution
- **E2E tests**: `tests/GitOut.Infrastructure.Tests/Integration/` - Complete game scenarios

## Clean Architecture Implementation

GitOut strictly enforces clean architecture with **zero dependency violations**. Understanding the layer boundaries is critical.

### Layer Dependencies (Enforced)

```
Domain (No dependencies)
   ↑
Application (Depends only on Domain)
   ↑
Infrastructure (Depends on Domain + Application)
   ↑
Console (Depends on all layers)
```

### Domain Layer (`src/GitOut.Domain/`)

**Core principle**: Zero external dependencies. Not even System.Text.Json or any NuGet packages.

**Key entities:**
- `Game` - Manages game state, current room, player progress
- `Room` - Contains id, narrative, challenge, exits (directions → room IDs)
- `Player` - Tracks completed rooms/challenges, move count
- `IChallenge` - Interface for all challenge types with `SetupAsync()` and `ValidateAsync()`

**Challenge types:**
- `RepositoryChallenge` - Validates git repo state (commit count, clean status, files exist, etc.)
- `QuizChallenge` - Multiple choice questions, stores player answer in memory
- `ScenarioChallenge` - Story-driven challenges with branch validation, scenario narrative

**Critical interface:**
- `IGitCommandExecutor` - Defined in Domain but **implemented in Infrastructure**. This is the key abstraction that allows Domain to define git operations without depending on Process.Start or any I/O.

### Application Layer (`src/GitOut.Application/`)

**Core principle**: Use cases and orchestration only. Depends on Domain interfaces, not implementations.

**Key services:**
- `GameEngine` - Main game loop coordinator. Processes commands (help, status, look, move, git, answer, hint, save)
- `StartGameUseCase` - Initializes new game from room repository
- `SaveProgressUseCase` / `LoadProgressUseCase` - Persist/restore game state

**Critical pattern**: Application defines interfaces (`IProgressRepository`, `IRoomRepository`) that Infrastructure implements. Application never imports Infrastructure namespace.

### Infrastructure Layer (`src/GitOut.Infrastructure/`)

**Core principle**: All external dependencies live here. Implements interfaces defined in Domain/Application.

**Git execution (`Git/GitCommandExecutor.cs`):**
```csharp
// Executes real git commands via Process.Start()
// Returns GitCommandResult with Success, Output, Error, ExitCode
await ExecuteAsync("init", workingDirectory)
await IsGitRepositoryAsync(directory)  // Checks .git folder + git rev-parse
await GetStatusAsync(workingDirectory)  // Parses git status output
await GetLogAsync(workingDirectory, maxCount)  // Gets oneline log, handles empty repos
```

**Temp directory management (`FileSystem/TempDirectoryManager.cs`):**
- Creates isolated temp directories under `/tmp/GitOut/challenge/{guid}/`
- Tracks all created directories for cleanup
- Implements IDisposable - cleanup happens in Dispose() or finalizer
- Used in tests with `using` statements for automatic cleanup

**Save/load (`Persistence/JsonProgressRepository.cs`):**
- Saves to `~/.gitout/save.json` by default
- Stores: player name, current room ID, completed rooms/challenges, move count, timestamps
- Uses System.Text.Json with camelCase naming

**Room repository (`Persistence/RoomRepository.cs` + `Persistence/RoomFactories/`):**
- RoomRepository.cs (~95 lines) - Lightweight coordinator that instantiates factory classes
- 23 individual factory files in RoomFactories/ folder - Each factory creates one room
- Factory pattern: Each RoomXXFactory has a CreateAsync() method that returns a fully-configured Room
- Wires up IGitCommandExecutor to each challenge via factory constructors
- Returns Dictionary<string, Room> for game navigation

### Console Layer (`src/GitOut.Console/`)

**Entry point (`Program.cs`):**
- Sets up DI container with Microsoft.Extensions.DependencyInjection
- Registers all services as singletons
- Manages game loop and working directory lifecycle
- Handles save/load prompts before game start
- Creates temp directory via TempDirectoryManager
- Calls Challenge.SetupAsync() when entering rooms
- Cleanup in finally block

**Key components:**
- `GameRenderer` - Renders welcome screen, rooms, command results, errors
- `CommandHandler` - Parses user input, identifies exit commands

## Git Command Execution Strategy

**This is the most important architectural decision in the codebase.**

### How It Works

1. **Per-challenge isolation**: Each challenge gets its own temp directory via `TempDirectoryManager`
2. **Real git CLI**: Execute actual git commands via `Process.Start("git", args, workingDirectory)`
3. **Setup phase**: `Challenge.SetupAsync()` creates files, initializes repo with starting state
4. **Player interaction**: Player types real git commands in game (e.g., "git init", "git add README.md")
5. **Validation**: `Challenge.ValidateAsync()` parses git output to check if solution is correct
6. **State parsing**: Infrastructure parses `git status`, `git log`, `git branch` output using string matching

### Example Flow (Room 2: Staging Area)

```csharp
// 1. Setup (RepositoryChallenge.SetupAsync)
// Creates README.md in temp directory
await File.WriteAllTextAsync(Path.Combine(workingDir, "README.md"), "...");

// 2. Player commands (GameEngine.ProcessCommandAsync)
await _gitExecutor.ExecuteAsync("add README.md", workingDir);
await _gitExecutor.ExecuteAsync("commit -m 'Initial commit'", workingDir);

// 3. Validation (RepositoryChallenge.ValidateAsync)
var status = await _gitExecutor.GetStatusAsync(workingDir);
var log = await _gitExecutor.GetLogAsync(workingDir);
// Check: status contains "working tree clean" AND log has >= 1 commit
```

### Why This Approach

- **Authentic learning**: Players use real git, not simulated/fake commands
- **Testability**: Mocking IGitCommandExecutor allows unit tests without actual git
- **Isolation**: Each challenge in separate temp dir prevents cross-contamination
- **Validation flexibility**: Can check any git state (status, log, branches, reflog, etc.)

## Challenge System Architecture

### Strategy Pattern Implementation

All challenges implement `IChallenge`:
```csharp
interface IChallenge {
    string Id { get; }
    ChallengeType Type { get; }
    string Description { get; }
    Task SetupAsync(string workingDirectory);
    Task<ChallengeResult> ValidateAsync(string workingDirectory);
}
```

### RepositoryChallenge Extensibility

**Declarative validation** (common cases):
```csharp
new RepositoryChallenge(
    id: "challenge-1",
    description: "Initialize a repo and make a commit",
    gitExecutor: _gitExecutor,
    requireGitInit: true,        // .git directory must exist
    requireCleanStatus: true,    // Working tree must be clean
    requiredCommitCount: 1,      // At least 1 commit in log
    requiredFiles: ["README.md"], // File must exist
    setupFiles: ["README.md"]    // Create this file in SetupAsync
)
```

**Custom validation** (complex scenarios):
```csharp
new RepositoryChallenge(
    id: "challenge-merge",
    description: "Merge feature branch into main",
    gitExecutor: _gitExecutor,
    customSetup: async (dir, git) => {
        // Initialize repo with two branches and commits
        await git.ExecuteAsync("init", dir);
        await git.ExecuteAsync("checkout -b feature", dir);
        // ... create complex starting state
    },
    customValidator: async (dir, git) => {
        var branches = await git.ExecuteAsync("branch", dir);
        var log = await git.GetLogAsync(dir);
        // Check for merge commit, both branches exist, etc.
        return new ChallengeResult(success, message, hint);
    }
)
```

**Key insight**: Challenges are immutable data + validation functions. Easy to test in isolation.

### QuizChallenge Pattern

- Stateful: Stores `_playerAnswer` in memory
- Player submits answer via `challenge.SubmitAnswer(index)` (called by GameEngine)
- Validation checks if `_playerAnswer == CorrectAnswerIndex`
- No git interaction required (SetupAsync is no-op)

### ScenarioChallenge Pattern

- Combines narrative storytelling with git validation
- Supports branch validation: `RequiredBranches`, `RequiredCurrentBranch`
- Uses same validation infrastructure as RepositoryChallenge
- Adds `Scenario` property for longer narrative text

## Testing Strategy

### Unit Tests (Domain/Application)

**Pattern**: Mock IGitCommandExecutor, test business logic
```csharp
[Fact]
public async Task ValidateAsync_ShouldReturnSuccess_WhenCommitCountSufficient()
{
    // Arrange
    _gitExecutorMock.Setup(x => x.GetLogAsync(dir, count))
        .ReturnsAsync("abc123 Commit 1\ndef456 Commit 2");

    var challenge = new RepositoryChallenge(..., requiredCommitCount: 2);

    // Act
    var result = await challenge.ValidateAsync(tempDir);

    // Assert
    result.IsSuccessful.Should().BeTrue();
}
```

### Integration Tests (Infrastructure)

**Pattern**: Use real temp directories and actual git commands
```csharp
[Fact]
public async Task ExecuteAsync_ShouldExecuteGitInit_Successfully()
{
    // Arrange
    var tempDir = Path.Combine(Path.GetTempPath(), "GitOutTests", Guid.NewGuid().ToString());
    Directory.CreateDirectory(tempDir);
    var executor = new GitCommandExecutor();

    // Act
    var result = await executor.ExecuteAsync("init", tempDir);

    // Assert
    result.Success.Should().BeTrue();
    Directory.Exists(Path.Combine(tempDir, ".git")).Should().BeTrue();

    // Cleanup
    Directory.Delete(tempDir, recursive: true);
}
```

**Critical**: Tests use `IDisposable` pattern to clean up temp directories in Dispose()

### E2E Tests (Full game flow)

**Pattern**: Test complete scenarios from StartGameUseCase through GameEngine
```csharp
[Fact]
public async Task CompleteGame_ShouldProgressThroughFirstTwoRooms()
{
    // Start game
    var startResult = await _startGameUseCase.ExecuteAsync("TestPlayer");
    _gameEngine.StartGame(startResult.Game!, _workingDirectory);

    // Complete Room 1: git init
    var result = await _gameEngine.ProcessCommandAsync("git init");
    result.Message.Should().Contain("Challenge completed");

    // Move to Room 2
    await _gameEngine.ProcessCommandAsync("forward");

    // Complete Room 2: git add + commit
    await _gitExecutor.ExecuteAsync("config user.email \"test@example.com\"", _workingDirectory);
    await _gameEngine.ProcessCommandAsync("git add README.md");
    await _gameEngine.ProcessCommandAsync("git commit -m 'Initial commit'");

    // Assert progress
    startResult.Game!.Player.CompletedChallenges.Should().HaveCount(2);
}
```

### Test Coverage Goals

- Domain: 95%+ (pure logic, highly testable)
- Application: 90%+
- Infrastructure: 80%+ (some OS/git dependencies)
- Overall: 85%+ (currently achieved with 184 tests: 61 Domain + 123 Infrastructure)

## Save/Load System

### Architecture

**Persistence format**: JSON file at `~/.gitout/save.json`
```json
{
  "playerName": "Adventurer",
  "currentRoomId": "room-3",
  "completedRooms": ["room-1", "room-2"],
  "completedChallenges": ["challenge-1", "challenge-2"],
  "moveCount": 2,
  "savedAt": "2025-01-15T10:30:00Z",
  "gameStarted": "2025-01-15T10:00:00Z"
}
```

**Save flow**:
1. Player types "save" command
2. GameEngine calls `_saveProgressHandler(currentGame)` (injected callback)
3. SaveProgressUseCase extracts GameProgress record from Game entity
4. JsonProgressRepository serializes to file

**Load flow**:
1. Program.cs checks `progressRepository.HasSavedProgressAsync()` on startup
2. If exists, prompt user to load
3. LoadProgressUseCase reads JSON, reconstructs Game entity
4. RoomRepository loads all room definitions
5. Game.CurrentRoom set to saved room ID
6. Player progress restored (completed rooms/challenges, move count)

**Critical**: Challenge objects are NOT serialized. They are recreated from room definitions on load.

## Important Architectural Boundaries

### Dependency Rules (NEVER VIOLATE)

1. **Domain imports nothing** (not even System.Text.Json)
2. **Application imports only Domain** (never Infrastructure or Console)
3. **Infrastructure imports Domain + Application** (implements their interfaces)
4. **Console imports all layers** (composition root)

### Interface Placement

- **IGitCommandExecutor**: Defined in Domain, implemented in Infrastructure
- **IProgressRepository**: Defined in Application, implemented in Infrastructure
- **IRoomRepository**: Defined in Application, implemented in Infrastructure

**Why**: Dependency Inversion Principle. Core logic defines contracts; outer layers fulfill them.

### Testing Boundaries

- **Domain/Application tests**: Use Moq to mock interfaces, never import Infrastructure
- **Infrastructure tests**: Can import everything, use real file system and git
- **Console tests**: Not implemented yet (Phase 5) - will use Spectre.Console.Testing

## Key Design Patterns

1. **Strategy Pattern**: `IChallenge` with three implementations (Repository, Quiz, Scenario)
2. **Repository Pattern**: `IRoomRepository`, `IProgressRepository` abstract data access
3. **Dependency Injection**: Microsoft.Extensions.DependencyInjection wires everything in Program.cs
4. **Command Pattern**: GameEngine.ProcessCommandAsync routes to command handlers
5. **Template Method**: Challenge base validation (custom vs. declarative)
6. **Facade**: GameEngine hides complexity of command routing and challenge orchestration

## Phase Roadmap Context

### Phase 2 (Complete)
- 8 rooms: init, add/commit, log, status, branch, merge, restore, quiz
- All 3 challenge types implemented
- Save/load infrastructure ready
- 120+ passing tests
- Hint system functional

### Phase 3 (Complete)
- 16 rooms total (8 new rooms added)
- Conflict resolution challenges (Room 9)
- Intermediate branching: stash (Room 10), cherry-pick (Room 11), rebase (Room 12)
- Advanced commands: tag (Room 13), reflog (Room 14), remote (Room 15), bisect (Room 16)
- Factory pattern refactoring for better maintainability
- 159 tests passing

### Phase 4 (Complete)
- 24 rooms total (8 new rooms added)
- Safe undo: revert (Room 17) - safely undo pushed commits
- Advanced git workflows: worktree (Room 18), blame (Room 19), hooks (Room 20)
- Advanced techniques: interactive staging (Room 21), submodule (Room 22), filter-branch (Room 23)
- Epic final boss challenge (Room 24) - combining all git concepts learned
- Save/load system fully integrated and tested
- Cross-platform compatibility (Windows/Unix/Mac)
- 184 tests passing (61 Domain + 123 Infrastructure)
- 31 new Phase 4 integration tests

### Future Considerations

When adding new rooms/challenges:
1. Create new factory file: `src/GitOut.Infrastructure/Persistence/RoomFactories/RoomXXNameFactory.cs`
2. Implement CreateAsync() method returning a fully-configured Room with its challenge
3. Add factory instantiation to RoomRepository.LoadRoomsAsync() (2 lines: instantiate + add to dictionary)
4. Add integration test in appropriate Phase test file (e.g., Phase3RoomsTests.cs)
5. Ensure challenge cleanup (TempDirectoryManager handles this)
6. Test both setup and validation paths
7. Provide meaningful hints in ChallengeResult

**Factory pattern benefits:**
- Each room is self-contained in its own file (~40-100 lines)
- Easy to find specific rooms (Room05BranchJunctionFactory.cs vs. line 234 of a 1,459-line file)
- Adding rooms requires minimal changes to RoomRepository.cs
- Git diffs are clean and focused on the specific room being changed
- No merge conflicts when multiple developers add rooms simultaneously
- Better IDE navigation and search

When extending validation:
1. Add new methods to IGitCommandExecutor if needed (e.g., `GetReflogAsync`, `GetDiffAsync`)
2. Implement in GitCommandExecutor
3. Update mocks in Domain tests
4. Add integration tests for new git command parsing

## Common Pitfalls to Avoid

1. **Don't import Infrastructure in Application**: Use interfaces only
2. **Don't forget git config in tests**: Commits require user.email and user.name
3. **Don't skip temp directory cleanup**: Use `using` statements or finally blocks
4. **Don't assume git output format**: It can vary slightly across versions
5. **Don't create stateful challenges**: Each challenge should be re-creatable from JSON
6. **Don't test private methods**: Test through public APIs and integration tests
7. **Don't use shell commands in room instructions**: Only git commands are supported (no cat, type, ls, dir, etc.). Use `git show HEAD:file` to view file contents, `git diff` to see changes

## Development Workflow Tips

1. **Run tests frequently**: `dotnet test` after every change
2. **Use integration tests for git validation**: Faster than manual play-testing
3. **Check E2E tests for complete flows**: EndToEndGameTests shows full game loop
4. **Test cleanup**: Always verify temp directories are deleted after tests
5. **Read existing challenge patterns**: RepositoryChallengeTests has comprehensive examples

## Critical Files for Understanding

- `src/GitOut.Domain/Challenges/IChallenge.cs` - Challenge contract
- `src/GitOut.Infrastructure/Git/GitCommandExecutor.cs` - Git execution
- `src/GitOut.Application/Services/GameEngine.cs` - Command routing
- `src/GitOut.Console/Program.cs` - DI setup and game loop
- `src/GitOut.Infrastructure/Persistence/RoomRepository.cs` - Room loading coordinator (~95 lines)
- `src/GitOut.Infrastructure/Persistence/RoomFactories/Room01InitializationChamberFactory.cs` - Example factory pattern
- `src/GitOut.Infrastructure/Persistence/RoomFactories/Room24FinalGauntletFactory.cs` - Epic boss challenge example
- `tests/GitOut.Infrastructure.Tests/Integration/EndToEndGameTests.cs` - E2E examples
- `tests/GitOut.Infrastructure.Tests/Integration/Phase4RoomsTests.cs` - Phase 4 room tests

## Room Factory Architecture

### Structure

```
src/GitOut.Infrastructure/Persistence/
├── RoomRepository.cs                                    # ~95 lines - coordinator
└── RoomFactories/
    ├── Room01InitializationChamberFactory.cs           # git init
    ├── Room02StagingAreaFactory.cs                     # git add/commit
    ├── Room03HistoryArchiveFactory.cs                  # git log
    ├── Room04StatusChamberFactory.cs                   # git status
    ├── Room05BranchJunctionFactory.cs                  # git branch
    ├── Room06MergeNexusFactory.cs                      # git merge
    ├── Room07RestorationVaultFactory.cs                # git restore
    ├── Room08QuizMastersHallFactory.cs                 # quiz challenge
    ├── Room09ConflictCatacombsFactory.cs               # merge conflicts
    ├── Room10StashSanctumFactory.cs                    # git stash
    ├── Room11CherryPickGardenFactory.cs                # git cherry-pick
    ├── Room12RebaseRidgeFactory.cs                     # git rebase
    ├── Room13TagTowerFactory.cs                        # git tag
    ├── Room14ReflogRuinsFactory.cs                     # git reflog
    ├── Room15RemoteRealmFactory.cs                     # remote repos
    ├── Room16BisectBattlefieldFactory.cs               # git bisect
    ├── Room17RevertChamberFactory.cs                   # git revert
    ├── Room18WorktreeWorkshopFactory.cs                # git worktree
    ├── Room19BlameChamberFactory.cs                    # git blame
    ├── Room20HookHollowFactory.cs                      # git hooks
    ├── Room21InteractiveStagingHallFactory.cs          # git add -p
    ├── Room22SubmoduleSanctumFactory.cs                # git submodule
    ├── Room23RewriteReliquaryFactory.cs                # git filter-branch
    └── Room24FinalGauntletFactory.cs                   # FINAL BOSS CHALLENGE
```

### Factory Pattern Example

```csharp
// Each factory follows this pattern:
public class Room01InitializationChamberFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room01InitializationChamberFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        // 1. Create the challenge with all configuration
        var challenge = new RepositoryChallenge(
            id: "init-chamber-challenge",
            description: "Initialize a git repository by running 'git init'",
            gitExecutor: _gitExecutor,
            requireGitInit: true
        );

        // 2. Create the room with narrative, exits, challenge
        var room = new Room(
            id: "room-1",
            name: "The Initialization Chamber",
            description: "A barren chamber with ancient walls",
            narrative: "You've entered a barren chamber...",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-2" } },
            isStartRoom: true,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}
```

### RoomRepository Coordinator

```csharp
public async Task<Dictionary<string, Room>> LoadRoomsAsync()
{
    if (_cachedRooms != null)
    {
        return _cachedRooms;
    }

    _cachedRooms = new Dictionary<string, Room>();

    // Instantiate all factories
    var room01Factory = new Room01InitializationChamberFactory(_gitExecutor);
    var room02Factory = new Room02StagingAreaFactory(_gitExecutor);
    // ... 21 more factories

    // Create rooms from factories
    _cachedRooms["room-1"] = await room01Factory.CreateAsync();
    _cachedRooms["room-2"] = await room02Factory.CreateAsync();
    // ... 21 more rooms

    return _cachedRooms;
}
```

### Why This Architecture

**Before (Monolithic):**
- Single RoomRepository.cs file with 1,459 lines
- All room definitions inline in LoadRoomsAsync()
- Hard to find specific rooms
- Difficult to review changes in PRs
- Merge conflicts when adding rooms in parallel

**After (Factory Pattern):**
- RoomRepository.cs reduced to ~95 lines (from 1,459)
- Each room in its own file (~40-550 lines, most 40-120)
- Easy to locate: "Room05BranchJunctionFactory.cs"
- Clean git diffs focused on one room
- No merge conflicts - different files
- Better IDE navigation and searchability
- Room 24 (Final Boss) is largest at ~550 lines due to complex multi-step validation

## Commit Message Convention

This project uses **Conventional Commits** for all commit messages. Format:

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

### Types
- `feat`: New feature (MINOR version bump)
- `fix`: Bug fix (PATCH version bump)
- `docs`: Documentation only changes
- `style`: Code style (formatting, semicolons, etc.)
- `refactor`: Code change that neither fixes a bug nor adds a feature
- `perf`: Performance improvement
- `test`: Adding or updating tests
- `build`: Build system or dependencies
- `ci`: CI/CD configuration
- `chore`: Other changes (version bumps, etc.)

### Breaking Changes
Use `!` after type/scope for breaking changes (MAJOR version bump):
```
feat!: remove deprecated filter-branch support
feat(room)!: change room ID format
```

Or include `BREAKING CHANGE:` in footer.

### Scope Suggestions
- `room` - Room factories and content
- `challenge` - Challenge types and validation
- `engine` - GameEngine and command processing
- `ui` - Console rendering and input
- `save` - Progress persistence
- `git` - Git command execution

### Examples
```
feat(room): add Room 24 for git worktree advanced scenarios
fix(challenge): correct validation logic for merge conflicts
docs: update README with installation instructions
build: enable Native AOT compilation
ci: add GitHub Actions release workflow
```
