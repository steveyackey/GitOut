using FluentAssertions;
using GitOut.Infrastructure.Tests.Fixtures;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration.RoomTests;

/// <summary>
/// Integration tests for Room 4: The Status Chamber (git status)
/// </summary>
public class Room04StatusChamberTests : RoomIntegrationTestFixture
{
    [Fact]
    public async Task Room4_StatusChamber_ShouldCompleteWhenStatusIsChecked()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room4-test");
        var room = await RoomHelper.LoadRoomAsync("room-4");

        room.Name.Should().Be("The Status Chamber");

        // Act - Setup creates files in various states
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify setup created expected files
        GitHelper.VerifyFileExists(workingDirectory, "tracked.txt");
        GitHelper.VerifyFileExists(workingDirectory, "untracked.txt");
        GitHelper.VerifyFileExists(workingDirectory, "staged.txt");

        // Act - Validate (challenge auto-completes since repo has state)
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult);
    }
}
