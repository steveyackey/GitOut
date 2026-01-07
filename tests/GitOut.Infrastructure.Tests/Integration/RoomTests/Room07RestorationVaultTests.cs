using FluentAssertions;
using GitOut.Infrastructure.Tests.Fixtures;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration.RoomTests;

/// <summary>
/// Integration tests for Room 7: The Restoration Vault (git restore)
/// </summary>
public class Room07RestorationVaultTests : RoomIntegrationTestFixture
{
    [Fact]
    public async Task Room7_RestorationVault_ShouldCompleteWhenFileIsRestored()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room7-test");
        var room = await RoomHelper.LoadRoomAsync("room-7");

        room.Name.Should().Be("The Restoration Vault");

        // Configure git
        await GitHelper.ConfigureGitUserAsync(workingDirectory);

        // Act - Setup creates corrupted file
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify file is corrupted
        await GitHelper.VerifyFileContainsAsync(workingDirectory, "sacred-text.txt", "CORRUPTED");

        // Restore the file
        await GitExecutor.ExecuteAsync("restore sacred-text.txt", workingDirectory);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "restored");

        // Verify file content is restored
        await GitHelper.VerifyFileContainsAsync(workingDirectory, "sacred-text.txt", "Sacred Text");
    }
}
