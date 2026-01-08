using FluentAssertions;
using GitOut.Infrastructure.Tests.Fixtures;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration.RoomTests;

/// <summary>
/// Integration tests for Room 12: The Rebase Ridge (git rebase)
/// </summary>
public class Room12RebaseRidgeTests : RoomIntegrationTestFixture
{
    [Fact]
    public async Task Room12_RebaseRidge_ShouldCompleteWhenFeatureBranchIsRebased()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room12-test");
        var room = await RoomHelper.LoadRoomAsync("room-12");

        room.Name.Should().Be("The Rebase Ridge");

        // Act - Setup creates diverged branches
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify we're on feature-timeline
        await GitHelper.VerifyCurrentBranchAsync(workingDirectory, "feature-timeline");

        // Rebase feature branch onto main
        // Note: This might create conflicts in real scenarios, but our setup creates non-conflicting changes
        var rebaseResult = await GitExecutor.ExecuteAsync("rebase main", workingDirectory);

        // If there are conflicts, we need to resolve them
        if (!rebaseResult.Success)
        {
            // Check if it's a conflict
            var hasConflicts = await GitExecutor.HasConflictsAsync(workingDirectory);
            if (hasConflicts)
            {
                // Resolve by writing the expected combined version
                var timelinePath = Path.Combine(workingDirectory, "timeline.txt");
                await File.WriteAllTextAsync(timelinePath,
                    "Event 1: Beginning\nEvent A: Main progress\nEvent B: More main work\nEvent 2: Feature work");

                await GitExecutor.ExecuteAsync("add timeline.txt", workingDirectory);
                var continueResult = await GitExecutor.ExecuteAsync("rebase --continue", workingDirectory);
                continueResult.Success.Should().BeTrue($"rebase --continue should succeed. Error: {continueResult.Error}");
            }
        }

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "realigned");

        // Verify log contains commits from both branches
        await GitHelper.VerifyLogContainsAsync(workingDirectory, "Event A");
        await GitHelper.VerifyLogContainsAsync(workingDirectory, "Event B");
        await GitHelper.VerifyLogContainsAsync(workingDirectory, "Event 2");
    }
}
