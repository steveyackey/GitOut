using FluentAssertions;
using GitOut.Infrastructure.Tests.Fixtures;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration.RoomTests;

/// <summary>
/// Integration tests for Room 3: The History Archive (git log)
/// </summary>
public class Room03HistoryArchiveTests : RoomIntegrationTestFixture
{
    [Fact]
    public async Task Room3_HistoryArchive_ShouldCompleteWhenLogIsViewed()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room3-test");
        var room = await RoomHelper.LoadRoomAsync("room-3");

        room.Name.Should().Be("The History Archive");
        room.Challenge.Should().NotBeNull();

        // Act - Setup creates commits
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify repo was initialized and has commits
        await GitHelper.VerifyRepositoryInitializedAsync(workingDirectory);
        await GitHelper.VerifyMinimumCommitCountAsync(workingDirectory, 1);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "chronicles");
    }
}
