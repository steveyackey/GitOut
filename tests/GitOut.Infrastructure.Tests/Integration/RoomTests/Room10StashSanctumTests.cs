using FluentAssertions;
using GitOut.Infrastructure.Tests.Fixtures;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration.RoomTests;

/// <summary>
/// Integration tests for Room 10: The Stash Sanctum (git stash)
/// </summary>
public class Room10StashSanctumTests : RoomIntegrationTestFixture
{
    [Fact]
    public async Task Room10_StashSanctum_ShouldCompleteWhenWorkIsStashed()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room10-test");
        var room = await RoomHelper.LoadRoomAsync("room-10");

        room.Name.Should().Be("The Stash Sanctum");

        // Act - Setup creates work in progress
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify there are changes
        var statusBefore = await GitExecutor.GetStatusAsync(workingDirectory);
        statusBefore.Should().Contain("modified", "Setup should create modified files");

        // Stash the work (including untracked files)
        await GitHelper.StashAsync(workingDirectory, includeUntracked: true);

        // Verify working directory is clean
        await GitHelper.VerifyWorkingTreeCleanAsync(workingDirectory);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "stash");
    }

    [Fact]
    public async Task Room10_StashSanctum_ShouldFailWhenStashingWithoutUntrackedFiles()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room10-fail-test");
        var room = await RoomHelper.LoadRoomAsync("room-10");

        room.Name.Should().Be("The Stash Sanctum");

        // Act - Setup creates work in progress (both modified and untracked files)
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Stash without -u flag (does not include untracked files)
        await GitHelper.StashAsync(workingDirectory, includeUntracked: false);

        // Verify stash was created
        await GitHelper.VerifyStashExistsAsync(workingDirectory);

        // Verify working directory still has untracked files
        var statusAfter = await GitExecutor.GetStatusAsync(workingDirectory);
        statusAfter.Should().Contain("notes.txt", "Untracked file should remain");
        statusAfter.Should().NotContain("working tree clean", "Working directory should not be clean");

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        validationResult.IsSuccessful.Should().BeFalse("Should fail when untracked files remain");
        validationResult.Message.Should().Contain("clean");
        validationResult.Hint.Should().Contain("-u");
    }
}
