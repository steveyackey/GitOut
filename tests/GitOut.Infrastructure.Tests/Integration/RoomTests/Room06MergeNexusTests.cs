using FluentAssertions;
using GitOut.Infrastructure.Tests.Fixtures;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration.RoomTests;

/// <summary>
/// Integration tests for Room 6: The Merge Nexus (git merge)
/// </summary>
public class Room06MergeNexusTests : RoomIntegrationTestFixture
{
    [Fact]
    public async Task Room6_MergeNexus_ShouldCompleteWhenFeatureIsMerged()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room6-test");
        var room = await RoomHelper.LoadRoomAsync("room-6");

        room.Name.Should().Be("The Merge Nexus");

        // Act - Setup creates my-feature branch with spell files already created
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify we're on main and my-feature branch exists
        await GitHelper.VerifyCurrentBranchAsync(workingDirectory, "main");
        await GitHelper.VerifyBranchExistsAsync(workingDirectory, "my-feature");

        // Switch to my-feature branch
        await GitHelper.CheckoutBranchAsync(workingDirectory, "my-feature");

        // Verify the three spell files already exist (created by setup)
        GitHelper.VerifyFileExists(workingDirectory, "fireball.txt");
        GitHelper.VerifyFileExists(workingDirectory, "icebolt.txt");
        GitHelper.VerifyFileExists(workingDirectory, "lightning.txt");

        // Stage and commit all spell files (files are already created, just need to be staged)
        await GitHelper.StageAllFilesAsync(workingDirectory);
        await GitHelper.CommitAsync(workingDirectory, "Add three combat spells");

        // Switch back to main
        await GitHelper.CheckoutBranchAsync(workingDirectory, "main");

        // Merge my-feature into main
        await GitHelper.MergeBranchAsync(workingDirectory, "my-feature");

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "merged");
    }
}
