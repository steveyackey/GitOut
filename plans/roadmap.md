# GitOut - Development Plan

## Project Vision

GitOut is a terminal-based dungeon crawler that teaches git commands through gameplay. Players navigate rooms, encounter challenges, and use **real git commands** to progress. The project follows clean architecture principles and uses .NET 10, C# 14, and Spectre.Console for TUI.

## Current Status: Phase 4 Complete ✅ 🎉

**23 playable rooms** covering basic through expert-level git commands:
- Phase 1-2: git init, add, commit, log, status, branch, merge, restore, quiz
- Phase 3: Conflict resolution, stash, cherry-pick, rebase, tag, reflog, remote concepts, bisect
- Phase 4: worktree, blame, hooks, interactive staging, submodule, filter-branch, **FINAL BOSS CHALLENGE**

**Architecture:** Clean architecture with factory pattern for room definitions.

**Test Coverage:** 184 tests passing (61 Domain + 123 Infrastructure), 85%+ coverage.

**Factory Pattern:** All 23 rooms follow individual factory pattern (40-550 lines per room).

**Save/Load System:** Fully implemented and integrated - players can save/load progress at any time.

## Architectural Layers

```
GitOut/
├── src/
│   ├── GitOut.Domain/          # Core entities, zero dependencies
│   ├── GitOut.Application/     # Use cases, depends only on Domain
│   ├── GitOut.Infrastructure/  # Git execution, file I/O, depends on Domain+Application
│   └── GitOut.Console/         # Spectre.Console TUI, depends on all layers
└── tests/
    ├── GitOut.Domain.Tests/           # Unit tests with mocks
    └── GitOut.Infrastructure.Tests/   # Integration tests with real git
```

### Key Design Patterns
1. **Strategy Pattern** - IChallenge with Repository/Quiz/Scenario implementations
2. **Repository Pattern** - IRoomRepository, IProgressRepository
3. **Factory Pattern** - Individual factory files for each room
4. **Dependency Injection** - Microsoft.Extensions.DependencyInjection
5. **Template Method** - Challenge base validation logic

### Git Execution Strategy
- Each challenge gets isolated temp directory
- Execute real git commands via Process.Start()
- Validate git state by parsing output (status, log, branches, etc.)
- Cleanup in finally blocks using TempDirectoryManager

## Phase Roadmap

### Phase 0: Foundation ✅ COMPLETE
- Project structure and architecture skeleton
- Core interfaces (IChallenge, IGitCommandExecutor, IRoomRepository)
- Clean architecture boundaries established
- Test projects configured with xUnit + FluentAssertions + Moq
- CI/CD pipeline (GitHub Actions)

### Phase 1: Minimal Prototype ✅ COMPLETE
**Goal:** Prove the concept works end-to-end

**Deliverables:**
- 2 playable rooms (Initialization Chamber, Staging Area)
- Real git command execution via GitCommandExecutor
- Basic game loop in GameEngine
- TempDirectoryManager for isolated challenge environments
- 70+ passing tests
- 80%+ test coverage

**Commands Taught:** git init, git add, git commit

### Phase 2: Core Git Commands ✅ COMPLETE
**Goal:** Cover fundamental git workflow

**Deliverables:**
- 6 additional rooms (8 total)
- All 3 challenge types (Repository, Quiz, Scenario)
- Enhanced UI with progress display
- Hint system
- Save/load progress infrastructure (ready but not wired to UI)
- 120+ passing tests
- 85%+ test coverage

**Commands Taught:** git log, git status, git branch, git merge, git restore

**Rooms:**
1. The Initialization Chamber (git init)
2. The Staging Area (git add/commit)
3. The History Archive (git log)
4. The Status Chamber (git status)
5. The Branch Junction (git branch)
6. The Merge Nexus (git merge)
7. The Restoration Vault (git restore)
8. The Quiz Master's Hall (quiz challenge)

### Phase 3: Intermediate Git ✅ COMPLETE
**Goal:** Branching, merging, conflicts, advanced commands

**Deliverables:**
- 8 additional rooms (16 total)
- Conflict resolution challenges
- Multi-branch scenarios
- Advanced git commands
- Factory pattern refactoring (1,459 line file → 16 factory files of 40-100 lines each)
- Test helper refactoring (GitTestHelper, RoomTestHelper, RoomIntegrationTestFixture)
- 159+ passing tests

**Commands Taught:** Merge conflicts, git stash, git cherry-pick, git rebase, git tag, git reflog, remote concepts, git bisect

**Rooms 9-16:**
9. The Conflict Catacombs (conflict resolution)
10. The Stash Sanctum (git stash)
11. The Cherry-Pick Garden (git cherry-pick)
12. The Rebase Ridge (git rebase)
13. The Tag Tower (git tag)
14. The Reflog Ruins (git reflog)
15. The Remote Realm (remote concepts)
16. The Bisect Battlefield (git bisect)

### Phase 4: Polish & Advanced Features ✅ COMPLETE
**Goal:** Advanced topics, UI polish, content expansion

**Delivered:**
- ✅ 7 additional rooms (23 total - EXCEEDED 20+ target)
- ✅ Save/load fully integrated (already complete from Phase 3)
- ✅ Advanced git topics coverage
- ✅ Epic final boss challenge
- ✅ Comprehensive testing (184 total tests, 31 Phase 4 tests)
- ✅ Factory pattern maintained across all rooms
- ✅ Cross-platform compatibility (Windows/Unix/Mac)

**Advanced Git Topics Implemented:**
- ✅ git worktree (manage multiple working trees)
- ✅ git submodule (nested repositories)
- ✅ git filter-branch (rewrite history)
- ✅ git blame (track line changes)
- ✅ Git hooks introduction
- ✅ Interactive staging (git add -p)
- ✅ Combined mastery (final boss with stash, reflog, cherry-pick, merge, tag, conflicts)

**Rooms 17-23:**
17. The Worktree Workshop (git worktree)
18. The Blame Chamber (git blame)
19. The Hook Hollow (git hooks)
20. The Interactive Staging Hall (git add -p)
21. The Submodule Sanctum (git submodule)
22. The Rewrite Reliquary (git filter-branch)
23. The Final Gauntlet (BOSS CHALLENGE - combining all concepts)

**Deferred to Phase 5:**
- Achievements/leaderboard system
- Enhanced UI (map visualization, animations, progress tracker)
- Sound effects (terminal beeps)
- Hint system improvements
- Difficulty levels/tutorial mode
- Console UI tests with Spectre.Console.Testing
- Performance benchmarks
- Test file refactoring

### Phase 5: Community & Extensibility (FUTURE)
**Goal:** Make it easy for others to contribute

**Features:**
- Plugin system for custom challenges
- External room definition files (YAML/JSON)
- Workshop/community challenges
- Localization support
- Documentation for creating challenges
- NuGet package for challenge SDK

## Testing Strategy

### Current State
- **Total Tests:** 184 passing (0 failed)
- **Domain Tests:** 61 (unit tests with mocks)
- **Infrastructure Tests:** 123 (integration tests with real git)
- **Coverage:** 85%+ overall (95%+ Domain, 90%+ Application, 80%+ Infrastructure)

### Test Organization

**Unit Tests (Domain/Application):**
- Mock IGitCommandExecutor to avoid real git execution
- Test business logic in isolation
- FluentAssertions for readable assertions
- Example: RepositoryChallengeTests, QuizChallengeTests

**Integration Tests (Infrastructure):**
- Use real temp directories and actual git commands
- Test git command parsing and validation
- RoomIntegrationTestFixture base class for common setup
- GitTestHelper (40+ methods) and RoomTestHelper (25+ methods) for reusable operations
- Example: Phase2RoomsTests, Phase3RoomsTests, EndToEndGameTests

**Test Helpers:**
- **GitTestHelper** - Reusable git operations (ConfigureGitUserAsync, CreateAndCommitFileAsync, VerifyWorkingTreeCleanAsync, etc.)
- **RoomTestHelper** - Reusable room operations (LoadRoomAsync, SetupChallengeAsync, ValidateChallengeAsync, VerifySuccess, etc.)
- **RoomIntegrationTestFixture** - Base class with common dependencies and setup

### Test File Refactoring Plan (Phase 4)
**Current Issue:** Tests are organized by "Phase2RoomsTests" and "Phase3RoomsTests", but the application doesn't have a concept of phases. This creates confusion and makes it harder to find tests for specific rooms.

**Proposed Refactoring:**
- Split Phase2RoomsTests.cs into individual test files or logical groupings
- Split Phase3RoomsTests.cs into individual test files or logical groupings
- Organize by git concept area or room functionality instead of development phase

**Example New Structure:**
```
tests/GitOut.Infrastructure.Tests/Integration/
├── BasicGitCommandsTests.cs        # Rooms 1-2 (init, add/commit)
├── GitHistoryTests.cs              # Rooms 3-4 (log, status)
├── BranchingAndMergingTests.cs     # Rooms 5-6 (branch, merge)
├── FileManagementTests.cs          # Room 7 (restore)
├── QuizChallengeTests.cs           # Room 8 (quiz)
├── ConflictResolutionTests.cs      # Room 9 (conflicts)
├── WorkInProgressTests.cs          # Room 10 (stash)
├── AdvancedBranchingTests.cs       # Rooms 11-12 (cherry-pick, rebase)
├── TaggingAndReferencesTests.cs    # Rooms 13-14 (tag, reflog)
├── RemoteWorkflowTests.cs          # Room 15 (remote)
├── DebuggingTests.cs               # Room 16 (bisect)
└── EndToEndGameTests.cs            # Full game scenarios
```

**Benefits:**
- Tests organized by git concept, not arbitrary development phase
- Easier to find tests for specific functionality
- Better aligned with how users think about git commands
- Each file focused on related functionality
- Continues to use helper classes for maximum code reuse

## Save/Load System

**Current Status:** Infrastructure complete, not wired to UI yet.

**Architecture:**
- Save format: JSON at `~/.gitout/save.json`
- Persists: player name, current room, completed rooms/challenges, move count, timestamps
- IProgressRepository interface defined in Application
- JsonProgressRepository implementation in Infrastructure
- SaveProgressUseCase and LoadProgressUseCase ready

**Phase 4 Integration:**
- Add save/load prompts in Program.cs
- Add "save" command to GameEngine
- Add auto-save on exit
- Multiple save slots (optional)

## Critical Files Reference

**Domain Layer:**
- `src/GitOut.Domain/Entities/Game.cs` - Game state
- `src/GitOut.Domain/Entities/Room.cs` - Room definition
- `src/GitOut.Domain/Challenges/IChallenge.cs` - Challenge interface
- `src/GitOut.Domain/Challenges/RepositoryChallenge.cs` - Git state validation

**Application Layer:**
- `src/GitOut.Application/Services/GameEngine.cs` - Main game loop
- `src/GitOut.Application/UseCases/StartGameUseCase.cs` - Game initialization
- `src/GitOut.Application/UseCases/SaveProgressUseCase.cs` - Persistence

**Infrastructure Layer:**
- `src/GitOut.Infrastructure/Git/GitCommandExecutor.cs` - Git execution
- `src/GitOut.Infrastructure/FileSystem/TempDirectoryManager.cs` - Temp dir management
- `src/GitOut.Infrastructure/Persistence/RoomRepository.cs` - Room coordinator (81 lines)
- `src/GitOut.Infrastructure/Persistence/RoomFactories/*.cs` - 16 room factory files

**Console Layer:**
- `src/GitOut.Console/Program.cs` - Entry point, DI setup
- `src/GitOut.Console/UI/GameRenderer.cs` - Spectre.Console rendering

**Test Helpers:**
- `tests/GitOut.Infrastructure.Tests/Helpers/GitTestHelper.cs` - Git operations
- `tests/GitOut.Infrastructure.Tests/Helpers/RoomTestHelper.cs` - Room operations
- `tests/GitOut.Infrastructure.Tests/Fixtures/RoomIntegrationTestFixture.cs` - Base class

## How to Add a New Room

1. **Create factory file:** `src/GitOut.Infrastructure/Persistence/RoomFactories/Room17NameFactory.cs`
2. **Implement CreateAsync()** with room configuration and challenge
3. **Update RoomRepository.cs:**
   - Add factory instantiation: `var room17Factory = new Room17NameFactory(_gitExecutor);`
   - Add to dictionary: `_cachedRooms["room-17"] = await room17Factory.CreateAsync();`
4. **Create integration test** using helper classes
5. **Update room navigation** (exits in previous/next rooms)
6. **Test end-to-end** by playing through the game

## Quality Standards

**Architecture:**
- Zero dependency violations (Domain → Application → Infrastructure → Console)
- Interfaces defined in inner layers, implemented in outer layers
- No Infrastructure imports in Application or Domain

**Testing:**
- All new rooms require integration tests
- Use helper classes to minimize duplication
- Verify both setup and validation paths
- 85%+ overall coverage maintained

**Code Quality:**
- No compiler warnings
- Meaningful variable names
- XML documentation on public APIs
- Consistent naming conventions

## Risk Mitigation

**Git command execution security:**
- Whitelist allowed git commands
- Validate inputs
- Run in isolated temp directories

**Cross-platform compatibility:**
- Test on Windows/Mac/Linux
- Use consistent git version checks

**Temp directory cleanup:**
- Use `using` statements with TempDirectoryManager
- Cleanup in finally blocks
- AppDomain exit handlers

**Complex git state validation:**
- Start simple (status checks)
- Incrementally add parsing
- Extensive integration tests

**Scope creep:**
- Stick to phased plan
- MVP approach for each phase
- Defer advanced features to later phases

## Success Metrics

### Phase 3 (COMPLETE)
- ✅ 16 playable rooms
- ✅ Advanced git commands (conflict resolution, rebase, stash, cherry-pick, tag, reflog, bisect)
- ✅ Factory pattern refactoring complete
- ✅ Test helper refactoring complete
- ✅ 159+ tests passing
- ✅ 85%+ test coverage

### Phase 4 (Current - COMPLETE) ✅
- ✅ 23 total rooms (EXCEEDED 20+ target)
- ✅ Save/load fully integrated into UI (already complete)
- ✅ All advanced git topics covered
- ✅ Epic final boss challenge
- ✅ Cross-platform compatibility (Windows/Unix/Mac)
- ✅ 184 tests passing (31 new Phase 4 tests)
- ✅ 85%+ test coverage maintained
- ✅ Factory pattern consistency across all 23 rooms

### Long-term (Phase 5+)
- [ ] Plugin system for custom challenges
- [ ] Community contribution framework
- [ ] Localization support
- [ ] 30+ rooms covering all git concepts

## Documentation Files

- **README.md** - User-facing documentation
- **CLAUDE.md** - AI assistant instructions for this codebase
- **Plan.md** - This file (development roadmap)
- **QUICKSTART.md** - Quick start guide
- **PLAYING.md** - Gameplay instructions
- **ARCHITECTURE_REFACTORING_SUMMARY.md** - Factory pattern refactoring details
- **PHASE1_SUMMARY.md** - Phase 1 completion notes
- **PHASE2_SUMMARY.md** - Phase 2 completion notes
- **PHASE2_COMPLETION.md** - Phase 2 detailed completion report
- **PHASE3_COMPLETION.md** - Phase 3 detailed completion report
- **PHASE4_COMPLETION.md** - Phase 4 detailed completion report (NEW)
- **tests/GitOut.Infrastructure.Tests/Helpers/README.md** - Test helper documentation

## Next Immediate Steps

1. **Manual Testing** - Verify save/load system works across all 23 rooms
2. **Play-test** - Complete full playthrough to validate game balance
3. **Create PHASE4_COMPLETION.md** - Detailed completion report
4. **Update CLAUDE.md** - Add Phase 4 room details and patterns
5. **Begin Phase 5 planning** - Community features, UI enhancements, achievements

---

**Last Updated:** 2025-12-31
**Current Phase:** Phase 4 Complete ✅ 🎉
**Total Rooms:** 23 (16 from Phases 1-3 + 7 new in Phase 4)
**Total Tests:** 184 passing (61 Domain + 123 Infrastructure)
**Architecture:** Clean Architecture + Factory Pattern
**Game Status:** FEATURE COMPLETE - Ready for release!
