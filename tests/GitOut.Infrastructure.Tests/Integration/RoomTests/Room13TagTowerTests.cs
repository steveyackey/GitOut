using FluentAssertions;
using GitOut.Infrastructure.Tests.Fixtures;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration.RoomTests;

/// <summary>
/// Integration tests for Room 13: The Tag Tower (git tag)
/// </summary>
public class Room13TagTowerTests : RoomIntegrationTestFixture
{
    [Fact]
    public async Task Room13_TagTower_ShouldCompleteWhenRequiredTagsAreCreated()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room13-test");
        var room = await RoomHelper.LoadRoomAsync("room-13");

        room.Name.Should().Be("The Tag Tower");

        // Act - Setup creates version commits
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Get commit log
        var log = await GitExecutor.GetLogAsync(workingDirectory, 10);
        var logLines = log.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Find the commits for v1.0.0 and v2.0.0
        string? v1Commit = null;
        string? v2Commit = null;

        foreach (var line in logLines)
        {
            if (line.Contains("version 1.0.0"))
            {
                v1Commit = line.Split(' ')[0]; // First part is commit hash
            }
            else if (line.Contains("version 2.0.0"))
            {
                v2Commit = line.Split(' ')[0];
            }
        }

        v1Commit.Should().NotBeNull("Should find v1.0.0 commit");
        v2Commit.Should().NotBeNull("Should find v2.0.0 commit");

        // Create tags
        await GitHelper.CreateTagAsync(workingDirectory, "v1.0.0", v1Commit);
        await GitHelper.CreateTagAsync(workingDirectory, "v2.0.0", v2Commit);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "tagged");

        // Verify tags exist
        await GitHelper.VerifyTagExistsAsync(workingDirectory, "v1.0.0");
        await GitHelper.VerifyTagExistsAsync(workingDirectory, "v2.0.0");
    }
}
