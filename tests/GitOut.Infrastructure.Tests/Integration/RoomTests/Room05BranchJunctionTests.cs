using FluentAssertions;
using GitOut.Infrastructure.Tests.Fixtures;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration.RoomTests;

/// <summary>
/// Integration tests for Room 5: The Branch Junction (git branch)
/// </summary>
public class Room05BranchJunctionTests : RoomIntegrationTestFixture
{
    [Fact]
    public async Task Room5_BranchJunction_ShouldCompleteWhenFeatureBranchIsCreated()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room5-test");
        var room = await RoomHelper.LoadRoomAsync("room-5");

        room.Name.Should().Be("The Branch Junction");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify repo was initialized
        await GitHelper.VerifyRepositoryInitializedAsync(workingDirectory);

        // Create the feature-branch and switch to it
        await GitHelper.CreateBranchAsync(workingDirectory, "feature-branch");

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "feature-branch");
    }

    [Fact]
    public async Task Room5_BranchJunction_ShouldFailWhenBranchNotCreated()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room5-fail-test");
        var room = await RoomHelper.LoadRoomWithChallengeAsync("room-5");

        // Act - Setup only, don't create branch
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifyFailure(validationResult, "not been created");
    }

    [Fact]
    public async Task Room5_BranchJunction_ShouldFailWhenBranchCreatedButNotSwitched()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room5-not-switched-test");
        var room = await RoomHelper.LoadRoomWithChallengeAsync("room-5");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Create the branch but DON'T switch to it
        await GitExecutor.ExecuteAsync("branch feature-branch", workingDirectory);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifyFailure(validationResult, "not currently on it");
        validationResult.Hint.Should().Contain("switch");
    }
}
