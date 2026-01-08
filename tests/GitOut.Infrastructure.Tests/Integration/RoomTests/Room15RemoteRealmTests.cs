using FluentAssertions;
using GitOut.Infrastructure.Tests.Fixtures;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration.RoomTests;

/// <summary>
/// Integration tests for Room 15: The Remote Realm (remote repos)
/// </summary>
public class Room15RemoteRealmTests : RoomIntegrationTestFixture
{
    [Fact]
    public async Task Room15_RemoteRealm_ShouldCompleteWhenRemoteIsAdded()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room15-test");
        var room = await RoomHelper.LoadRoomAsync("room-15");

        room.Name.Should().Be("The Remote Realm");

        // Act - Setup creates a remote repo
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Read the remote path
        var remotePathFile = Path.Combine(workingDirectory, "REMOTE_PATH.txt");
        var remotePathContent = await File.ReadAllTextAsync(remotePathFile);

        // Extract path from file - skip header line
        var lines = remotePathContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var remotePath = lines.Length > 1 ? lines[1] : lines[0]; // Second line or first if only one line

        // Add the remote
        await GitHelper.AddRemoteAsync(workingDirectory, "origin", remotePath);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "remote");

        // Verify remote exists
        await GitHelper.VerifyRemoteExistsAsync(workingDirectory, "origin");
    }
}
