using FluentAssertions;
using GitOut.Infrastructure.Tests.Fixtures;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration.RoomTests;

/// <summary>
/// Integration tests for Room 20: The Interactive Staging Hall (git add -p)
/// </summary>
public class Room20InteractiveStagingHallTests : RoomIntegrationTestFixture
{
    [Fact]
    public async Task Room20_InteractiveStagingHall_ShouldCompleteWhenPartialStaging()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room20-test");
        var room = await RoomHelper.LoadRoomAsync("room-20");

        room.Name.Should().Be("The Interactive Staging Hall");

        // Act - Setup creates utils.js with initial commit, then modifies file with unstaged changes
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify the file has unstaged changes after setup
        var statusAfterSetup = await GitExecutor.GetStatusAsync(workingDirectory);
        statusAfterSetup.Should().Contain("utils.js", "File should have unstaged changes after setup");

        // Simulate selective staging by creating a focused commit
        // First, let's manually create the partial content (greet with validation, but no multiply/subtract)
        var utilsPath = Path.Combine(workingDirectory, "utils.js");

        // Save the full content (has all changes)
        var fullContent = await File.ReadAllTextAsync(utilsPath);
        fullContent.Should().Contain("multiply", "Full content should have multiply function");
        fullContent.Should().Contain("subtract", "Full content should have subtract function");
        fullContent.Should().Contain("if (!name)", "Full content should have validation");

        // Create partial content with ONLY the greet validation change
        var partialContent = @"// Utility Functions

function greet(name) {
    if (!name) throw new Error('Name is required');
    return `Hello, ${name}!`;
}

function add(a, b) {
    return a + b;
}
";

        // Replace file with partial content, commit it, then restore full content
        await File.WriteAllTextAsync(utilsPath, partialContent);

        // Check status before add
        var statusBeforeAdd = await GitExecutor.GetStatusAsync(workingDirectory);

        var addResult = await GitExecutor.ExecuteAsync("add utils.js", workingDirectory);
        addResult.Success.Should().BeTrue($"Add should succeed: {addResult.Error}");

        // Check staged changes
        var diffCached = await GitExecutor.ExecuteAsync("diff --cached", workingDirectory);

        var commitResult = await GitExecutor.ExecuteAsync("commit -m \"Add validation to greet\"", workingDirectory);
        if (!commitResult.Success)
        {
            throw new Exception($"Commit failed!\nError: {commitResult.Error}\nOutput: {commitResult.Output}\n\nStatus before add:\n{statusBeforeAdd}\n\nDiff cached:\n{diffCached.Output}");
        }

        // Restore full content to make multiply/subtract unstaged again
        await File.WriteAllTextAsync(utilsPath, fullContent);

        // Verify working directory now has unstaged changes
        var statusAfterCommit = await GitExecutor.GetStatusAsync(workingDirectory);
        statusAfterCommit.Should().Contain("utils.js", "utils.js should have unstaged changes");

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        if (!validationResult.IsSuccessful)
        {
            // Debug: check what's in HEAD
            var headContent = await GitExecutor.ExecuteAsync("show HEAD:utils.js", workingDirectory);
            throw new Exception($"Validation failed: {validationResult.Message}\nHint: {validationResult.Hint}\n\nHEAD content:\n{headContent.Output}");
        }
        RoomHelper.VerifySuccess(validationResult, "selective");
    }

    [Fact]
    public async Task Room20_InteractiveStagingHall_ShouldFailWhenWorkingTreeClean()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room20-clean-test");
        var room = await RoomHelper.LoadRoomAsync("room-20");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Stage and commit ALL changes (wrong approach)
        await GitExecutor.ExecuteAsync("add utils.js", workingDirectory);
        await GitExecutor.ExecuteAsync("commit -m \"Add validation to greet\"", workingDirectory);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifyFailure(validationResult, "unstaged changes");
    }

    [Fact]
    public async Task Room20_InteractiveStagingHall_ShouldFailWhenCommitIncludesTooMuch()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room20-toomuch-test");
        var room = await RoomHelper.LoadRoomAsync("room-20");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Stage all, commit, but create a fake unstaged change
        await GitExecutor.ExecuteAsync("add utils.js", workingDirectory);
        await GitExecutor.ExecuteAsync("commit -m \"Add validation to greet\"", workingDirectory);

        // Add a different file to make working tree dirty
        await File.WriteAllTextAsync(Path.Combine(workingDirectory, "other.txt"), "other content");

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert - Should fail because commit includes multiply/subtract (working tree clean for utils.js)
        RoomHelper.VerifyFailure(validationResult, "unstaged changes");
    }
}
