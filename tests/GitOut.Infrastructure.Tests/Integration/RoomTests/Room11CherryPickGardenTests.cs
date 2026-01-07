using FluentAssertions;
using GitOut.Infrastructure.Tests.Fixtures;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration.RoomTests;

/// <summary>
/// Integration tests for Room 11: The Cherry-Pick Garden (git cherry-pick)
/// </summary>
public class Room11CherryPickGardenTests : RoomIntegrationTestFixture
{
    [Fact]
    public async Task Room11_CherryPickGarden_ShouldCompleteWhenCorrectCommitIsPicked()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room11-test");
        var room = await RoomHelper.LoadRoomAsync("room-11");

        room.Name.Should().Be("The Cherry-Pick Garden");

        // Act - Setup creates branches with commits
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Read the reference file to get commit hash
        var refFilePath = Path.Combine(workingDirectory, "CHERRY_PICK_THIS.txt");
        var refContent = await File.ReadAllTextAsync(refFilePath);
        refContent.Should().Contain("Cherry-pick this commit");

        // Extract commit hash from file
        var lines = refContent.Split('\n');
        var hashLine = lines[0];
        var commitHash = hashLine.Split(':')[1].Trim();

        // Verify we're on main
        await GitHelper.VerifyCurrentBranchAsync(workingDirectory, "main");

        // Cherry-pick the lavender commit
        var cherryPickResult = await GitExecutor.ExecuteAsync($"cherry-pick {commitHash}", workingDirectory);
        cherryPickResult.Success.Should().BeTrue("Cherry-pick should succeed");

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "lavender");

        // Verify file content
        await GitHelper.VerifyFileContainsAsync(workingDirectory, "garden.txt", "Lavender");
        var gardenContent = await File.ReadAllTextAsync(Path.Combine(workingDirectory, "garden.txt"));
        gardenContent.Should().NotContain("PoisonIvy");
    }
}
