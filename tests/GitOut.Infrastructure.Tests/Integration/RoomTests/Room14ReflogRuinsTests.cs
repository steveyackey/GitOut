using FluentAssertions;
using GitOut.Infrastructure.Tests.Fixtures;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration.RoomTests;

/// <summary>
/// Integration tests for Room 14: The Reflog Ruins (git reflog)
/// </summary>
public class Room14ReflogRuinsTests : RoomIntegrationTestFixture
{
    [Fact]
    public async Task Room14_ReflogRuins_ShouldCompleteWhenLostCommitIsRecovered()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room14-test");
        var room = await RoomHelper.LoadRoomAsync("room-14");

        room.Name.Should().Be("The Reflog Ruins");

        // Act - Setup creates and "loses" a commit
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify notes.txt doesn't exist (commit was reset away)
        GitHelper.VerifyFileDoesNotExist(workingDirectory, "notes.txt");

        // Use reflog to find the lost commit
        var reflog = await GitExecutor.GetReflogAsync(workingDirectory, 20);
        reflog.Should().Contain("Important notes", "Reflog should contain the lost commit");

        // Parse reflog to find the commit hash
        var reflogLines = reflog.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        string? lostCommitHash = null;

        foreach (var line in reflogLines)
        {
            if (line.Contains("Important notes"))
            {
                lostCommitHash = line.Split(' ')[0]; // First part is commit hash
                break;
            }
        }

        lostCommitHash.Should().NotBeNull("Should find lost commit in reflog");

        // Recover the lost commit
        await GitHelper.ResetAsync(workingDirectory, lostCommitHash!, hard: true);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "recovered");

        // Verify notes.txt now exists
        GitHelper.VerifyFileExists(workingDirectory, "notes.txt");
    }
}
