# GitOut

A terminal-based dungeon crawler that teaches git commands through gameplay.

## Overview

GitOut is an educational TUI (Text User Interface) game where players navigate through dungeon rooms, encounter challenges, and use **real git commands** to progress. Each room presents a unique git scenario that players must solve using their command-line git skills.

## Tech Stack

- **.NET 10** (LTS) with C# 14
- **Spectre.Console** 0.54.0 for beautiful terminal UI
- **xUnit + FluentAssertions + Moq** for testing
- **Clean Architecture** principles

## Architecture

The project follows clean architecture with clear separation of concerns:

```
GitOut/
├── src/
│   ├── GitOut.Domain/          # Core business logic (entities, interfaces)
│   ├── GitOut.Application/     # Use cases and game orchestration
│   ├── GitOut.Infrastructure/  # Git execution, file I/O, persistence
│   └── GitOut.Console/         # Spectre.Console TUI
└── tests/
    ├── GitOut.Domain.Tests/
    └── GitOut.Infrastructure.Tests/
```

### Layer Responsibilities

**Domain Layer**
- Core entities: `Game`, `Room`, `Player`
- Challenge interfaces and types
- Zero dependencies

**Application Layer**
- Game engine and use cases
- Depends only on Domain

**Infrastructure Layer**
- Git command execution via CLI
- File system operations
- JSON persistence
- Depends on Domain and Application

**Console Layer**
- Spectre.Console UI rendering
- User input handling
- Depends on all layers

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Git (installed and available in PATH)

### Building

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run the game
dotnet run --project src/GitOut.Console
```

## How It Works

1. **Rooms**: Navigate through dungeon rooms with descriptive narratives
2. **Challenges**: Each room may contain a git challenge
3. **Real Git**: Execute actual git commands in isolated temporary directories
4. **Validation**: The game checks the git repository state to verify solutions
5. **Progress**: Track your completion and learn git concepts progressively

## Challenge Types

- **Repository Challenges**: Manipulate a git repository to reach a target state
- **Quiz Challenges**: Answer questions about git commands
- **Scenario Challenges**: Story-driven problems that require git solutions

## Development Roadmap

### Phase 0: Foundation ✅ COMPLETE
- [x] Project structure and architecture
- [x] Core interfaces and entities
- [x] Initial test suite

### Phase 1: Minimal Prototype ✅ COMPLETE
- [x] Basic game loop
- [x] 2 playable rooms with git challenges
- [x] Git command execution infrastructure
- [x] 85%+ test coverage
- [x] 70+ passing tests

### Phase 2: Core Git Commands ✅ COMPLETE
- [x] 6 additional rooms (8 total)
- [x] All 3 challenge types implemented (Repository, Quiz, Scenario)
- [x] Enhanced UI with progress display
- [x] Hint system
- [x] 120+ passing tests
- [x] Save/load progress infrastructure ready

### Phase 3: Intermediate Git ✅ COMPLETE
- [x] 8 additional rooms (16 total)
- [x] Conflict resolution challenges
- [x] git rebase, git stash, git cherry-pick
- [x] Advanced commands: git tag, git reflog, git bisect
- [x] Remote repository concepts
- [x] Factory pattern refactoring for better maintainability

### Phase 4: Advanced Git & Polish ✅ COMPLETE
- [x] 7 additional rooms (23 total)
- [x] Advanced git topics: worktree, blame, hooks, interactive staging, submodule, filter-branch
- [x] Epic final boss challenge combining all concepts
- [x] Save/load system fully integrated
- [x] 184 tests passing (61 Domain + 123 Infrastructure)
- [x] Cross-platform compatibility (Windows/Unix/Mac)

**Current Status:** 23 playable rooms covering:
1. git init
2. git add/commit
3. git log
4. git status
5. git branch
6. git merge
7. git restore
8. Git quiz challenge
9. Merge conflict resolution
10. git stash
11. git cherry-pick
12. git rebase
13. git tag
14. git reflog
15. Remote repositories
16. git bisect
17. git worktree
18. git blame
19. Git hooks
20. git add -p (interactive staging)
21. git submodule
22. git filter-branch
23. Final Boss Challenge (combining all skills)

### Phase 5: Community (Planned)
- [ ] Plugin system for custom challenges
- [ ] Challenge creation SDK
- [ ] Community contributions

## Testing Strategy

- **Unit Tests**: Domain and Application logic with mocked dependencies
- **Integration Tests**: Real git command execution in temp directories
- **E2E Tests**: Complete game scenarios
- **Current Coverage**: 184 tests passing (61 Domain + 123 Infrastructure), 85%+ overall

## Contributing

This project is in early development. Contributions will be welcomed once Phase 1 is complete!

## License

TBD

## Acknowledgments

Built with:
- [Spectre.Console](https://spectreconsole.net/) for beautiful terminal UI
- [.NET 10](https://dotnet.microsoft.com/download/dotnet/10.0) for modern C# features
- Git for teaching one of the most important developer tools
