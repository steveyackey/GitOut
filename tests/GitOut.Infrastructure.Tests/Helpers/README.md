# Test Helper Classes

This directory contains helper classes that eliminate duplication across the GitOut test suite.

## Overview

The test helpers provide a clean, reusable API for common git and room operations used in integration tests. They reduce boilerplate code, improve test readability, and ensure consistent test patterns.

## Classes

### GitTestHelper

Provides common git operations for tests.

**Key methods:**
- `ConfigureGitUserAsync(workingDir)` - Set user.email and user.name for commits
- `CreateAndCommitFileAsync(workingDir, filename, content, message)` - Create, stage, and commit a file in one operation
- `CreateBranchAsync(workingDir, branchName)` - Create and switch to a new branch
- `VerifyWorkingTreeCleanAsync(workingDir)` - Assert that working tree is clean
- `VerifyCommitCountAsync(workingDir, expectedCount)` - Verify exact commit count
- `VerifyFileExists(workingDir, filename)` - Assert file exists
- `VerifyBranchExistsAsync(workingDir, branchName)` - Verify branch exists
- `VerifyCurrentBranchAsync(workingDir, expectedBranch)` - Assert current branch

**Example usage:**
```csharp
// Before (verbose)
await _gitExecutor.ExecuteAsync("config user.email \"test@example.com\"", workingDir);
await _gitExecutor.ExecuteAsync("config user.name \"Test User\"", workingDir);
var testFile = Path.Combine(workingDir, "test.txt");
await File.WriteAllTextAsync(testFile, "content");
await _gitExecutor.ExecuteAsync("add test.txt", workingDir);
await _gitExecutor.ExecuteAsync("commit -m \"Initial commit\"", workingDir);

// After (concise)
await _gitHelper.ConfigureGitUserAsync(workingDir);
await _gitHelper.CreateAndCommitFileAsync(workingDir, "test.txt", "content", "Initial commit");
```

### RoomTestHelper

Provides common room operations for tests.

**Key methods:**
- `LoadRoomAsync(roomId)` - Load and verify room exists
- `VerifyRoomAsync(roomId, expectedName)` - Assert room has expected name
- `SetupChallengeAsync(roomId, workingDir)` - Setup a room's challenge
- `ValidateChallengeAsync(roomId, workingDir)` - Validate a room's challenge
- `VerifySuccess(result, expectedMessageContains)` - Assert challenge succeeded
- `VerifyFailure(result, expectedMessageContains)` - Assert challenge failed
- `VerifyChallengeType<T>(room)` - Assert challenge is of specific type
- `VerifyRoomChainAsync(roomNumbers...)` - Verify sequential room connections

**Example usage:**
```csharp
// Before (verbose)
var room = await _roomRepository.GetRoomByIdAsync("room-3");
room.Should().NotBeNull();
room!.Name.Should().Be("The History Archive");
room.Challenge.Should().NotBeNull();
await room.Challenge!.SetupAsync(workingDirectory);
var result = await room.Challenge.ValidateAsync(workingDirectory);
result.IsSuccessful.Should().BeTrue();
result.Message.Should().Contain("chronicles");

// After (concise)
var room = await _roomHelper.LoadRoomAsync("room-3");
await _roomHelper.VerifyRoomAsync("room-3", "The History Archive");
await _roomHelper.SetupChallengeAsync(room, workingDirectory);
var result = await _roomHelper.ValidateChallengeAsync(room, workingDirectory);
_roomHelper.VerifySuccess(result, "chronicles");
```

### RoomIntegrationTestFixture

Base class for room integration tests that provides common setup and helper utilities.

**Features:**
- Automatic initialization of all dependencies
- Automatic cleanup via IDisposable pattern
- Protected properties for all helpers and dependencies
- Convenient `CreateWorkingDirectory(testName)` method

**Example usage:**
```csharp
public class MyRoomTests : RoomIntegrationTestFixture
{
    [Fact]
    public async Task Room3_ShouldComplete()
    {
        // Arrange - Use inherited helpers
        var workingDir = CreateWorkingDirectory("room3-test");
        var room = await RoomHelper.LoadRoomAsync("room-3");

        // Act
        await RoomHelper.SetupChallengeAsync(room, workingDir);
        await GitHelper.ConfigureGitUserAsync(workingDir);

        // Assert
        var result = await RoomHelper.ValidateChallengeAsync(room, workingDir);
        RoomHelper.VerifySuccess(result);
    }

    // Automatic cleanup via base class Dispose()
}
```

## Benefits

1. **Less Boilerplate**: Reduce 5-10 lines of setup code to 1-2 lines
2. **Better Readability**: Test intent is clearer when implementation details are hidden
3. **Consistency**: All tests use the same patterns and assertions
4. **Maintainability**: Changes to common operations happen in one place
5. **Type Safety**: Helpers use FluentAssertions for clear, readable assertions
6. **Error Messages**: Helpers provide descriptive error messages when assertions fail

## Migration Guide

Existing tests can gradually adopt these helpers:

1. **New tests**: Inherit from `RoomIntegrationTestFixture` and use helpers from the start
2. **Existing tests**: Keep current implementation but gradually refactor to use helpers
3. **No breaking changes**: All existing tests continue to work without modification

## Example: Complete Test Refactor

**Before (Phase2RoomsTests.cs - Room 7 test):**
```csharp
[Fact]
public async Task Room7_RestorationVault_ShouldCompleteWhenFileIsRestored()
{
    // Arrange
    var workingDirectory = _tempDirManager.CreateTempDirectory("room7-test");
    var room = await _roomRepository.GetRoomByIdAsync("room-7");

    room.Should().NotBeNull();
    room!.Name.Should().Be("The Restoration Vault");

    // Configure git
    await _gitExecutor.ExecuteAsync("config user.email \"test@example.com\"", workingDirectory);
    await _gitExecutor.ExecuteAsync("config user.name \"Test User\"", workingDirectory);

    // Act - Setup creates corrupted file
    await room.Challenge!.SetupAsync(workingDirectory);

    // Verify file is corrupted
    var corruptedContent = await File.ReadAllTextAsync(Path.Combine(workingDirectory, "sacred-text.txt"));
    corruptedContent.Should().Contain("CORRUPTED");

    // Restore the file
    await _gitExecutor.ExecuteAsync("restore sacred-text.txt", workingDirectory);

    // Act - Validate
    var validationResult = await room.Challenge.ValidateAsync(workingDirectory);

    // Assert
    validationResult.IsSuccessful.Should().BeTrue();
    validationResult.Message.Should().Contain("restored");

    // Verify file content is restored
    var restoredContent = await File.ReadAllTextAsync(Path.Combine(workingDirectory, "sacred-text.txt"));
    restoredContent.Should().Contain("Sacred Text");
}
```

**After (using helpers):**
```csharp
[Fact]
public async Task Room7_RestorationVault_ShouldCompleteWhenFileIsRestored()
{
    // Arrange
    var workingDir = CreateWorkingDirectory("room7-test");
    var room = await RoomHelper.LoadRoomAsync("room-7");
    await RoomHelper.VerifyRoomAsync("room-7", "The Restoration Vault");
    await GitHelper.ConfigureGitUserAsync(workingDir);

    // Act - Setup and verify corruption
    await RoomHelper.SetupChallengeAsync(room, workingDir);
    await GitHelper.VerifyFileContainsAsync(workingDir, "sacred-text.txt", "CORRUPTED");

    // Restore the file
    await GitExecutor.ExecuteAsync("restore sacred-text.txt", workingDir);

    // Assert
    var result = await RoomHelper.ValidateChallengeAsync(room, workingDir);
    RoomHelper.VerifySuccess(result, "restored");
    await GitHelper.VerifyFileContainsAsync(workingDir, "sacred-text.txt", "Sacred Text");
}
```

## Architecture

The helpers follow GitOut's clean architecture principles:

- **Domain Layer**: Helpers use `IGitCommandExecutor` from Domain
- **Application Layer**: Helpers use `IRoomRepository` from Application
- **Infrastructure Layer**: Helpers live in Infrastructure.Tests (test-only)
- **Dependency Injection**: Helpers receive dependencies via constructor

This ensures helpers respect architectural boundaries and can be used across all test types (unit, integration, E2E).
