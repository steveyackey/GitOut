using FluentAssertions;
using GitOut.Infrastructure.Tests.Fixtures;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration.RoomTests;

/// <summary>
/// Integration tests for Room 17: The Revert Chamber (git revert)
/// Note: Room 17 was renamed from Worktree Workshop in some versions
/// </summary>
public class Room17RevertChamberTests : RoomIntegrationTestFixture
{
    [Fact]
    public async Task Room17_WorktreeWorkshop_ShouldCompleteWhenWorktreeCreated()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room17-test");
        var room = await RoomHelper.LoadRoomAsync("room-17");

        room.Name.Should().Be("The Worktree Workshop");

        // Act - Setup creates repo with initial commit
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify README.md was created
        var readmePath = Path.Combine(workingDirectory, "README.md");
        File.Exists(readmePath).Should().BeTrue("README.md should exist");

        // Verify guide was created
        var guidePath = Path.Combine(workingDirectory, "WORKTREE_GUIDE.txt");
        File.Exists(guidePath).Should().BeTrue("WORKTREE_GUIDE.txt should exist");

        // Player creates a worktree
        // Use a unique name based on the working directory to avoid conflicts
        var workingDirName = Path.GetFileName(workingDirectory);
        var featureWorkName = $"{workingDirName}-feature-work";
        var featureDir = Path.Combine(Path.GetDirectoryName(workingDirectory)!, featureWorkName);

        // Clean up any existing directory from previous test runs
        if (Directory.Exists(featureDir))
        {
            Directory.Delete(featureDir, true);
        }

        var worktreeResult = await GitExecutor.ExecuteAsync($"worktree add ../{featureWorkName} -b feature", workingDirectory);
        worktreeResult.Success.Should().BeTrue($"Worktree creation should succeed: {worktreeResult.Error}");

        // Verify worktree was created
        Directory.Exists(featureDir).Should().BeTrue("Feature worktree directory should exist");

        // Verify worktree list shows 2 worktrees
        var listResult = await GitExecutor.ExecuteAsync("worktree list", workingDirectory);
        listResult.Success.Should().BeTrue();
        var worktreeCount = listResult.Output?.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length ?? 0;
        worktreeCount.Should().BeGreaterThanOrEqualTo(2, "Should have at least 2 worktrees");

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "worktree");
    }

    [Fact]
    public async Task Room17_WorktreeWorkshop_ShouldFailWhenNoWorktreeCreated()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room17-fail-test");
        var room = await RoomHelper.LoadRoomAsync("room-17");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Don't create any worktree

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifyFailure(validationResult, "1 worktree");
    }
}
