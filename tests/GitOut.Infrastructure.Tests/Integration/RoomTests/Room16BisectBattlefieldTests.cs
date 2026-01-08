using FluentAssertions;
using GitOut.Infrastructure.Tests.Fixtures;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration.RoomTests;

/// <summary>
/// Integration tests for Room 16: The Bisect Battlefield (git bisect)
/// </summary>
public class Room16BisectBattlefieldTests : RoomIntegrationTestFixture
{
    [Fact]
    public async Task Room16_BisectBattlefield_ShouldCompleteWhenBugIsFoundAndReset()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room16-test");
        var room = await RoomHelper.LoadRoomAsync("room-16");

        room.Name.Should().Be("The Bisect Battlefield");
        // Room 16 is no longer an end room in Phase 4 - it connects to room-17
        room.IsEndRoom.Should().BeFalse();
        room.Exits.Should().ContainKey("forward").WhoseValue.Should().Be("room-17");

        // Act - Setup creates commits with a bug
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Simulate the bisect process to find the bug
        // Start bisect
        await GitExecutor.ExecuteAsync("bisect start", workingDirectory);

        // Mark current (HEAD) as bad
        await GitExecutor.ExecuteAsync("bisect bad", workingDirectory);

        // Mark the first commit (Version 1) as good - HEAD~10 gets us to Version 1
        var bisectGoodResult = await GitExecutor.ExecuteAsync("bisect good HEAD~10", workingDirectory);

        // Now we need to complete the bisect by marking commits
        // Git will check out commits and we mark them until we find the bug
        // Version 6 is the first bad commit
        // We'll loop through the bisect process until git announces the first bad commit
        for (int i = 0; i < 10; i++)  // Safety limit
        {
            // Check the current commit message to see which version we're on
            var logResult = await GitExecutor.ExecuteAsync("log -1 --format=%s", workingDirectory);
            var commitMessage = logResult.Output?.Trim() ?? string.Empty;

            // Determine if this version is good or bad
            // Versions 1-5 are good, Versions 6-10 are bad
            if (commitMessage.StartsWith("Version "))
            {
                var versionStr = commitMessage.Replace("Version ", "");
                if (int.TryParse(versionStr, out var version))
                {
                    var markResult = version >= 6
                        ? await GitExecutor.ExecuteAsync("bisect bad", workingDirectory)
                        : await GitExecutor.ExecuteAsync("bisect good", workingDirectory);

                    // Check if bisect output says "is the first bad commit"
                    if (markResult.Output?.Contains("is the first bad commit") == true)
                    {
                        break;
                    }
                }
                else
                {
                    // This is the instructions commit - it's at the end so it's bad
                    await GitExecutor.ExecuteAsync("bisect bad", workingDirectory);
                }
            }
            else
            {
                // Unknown commit (probably instructions), mark as bad
                await GitExecutor.ExecuteAsync("bisect bad", workingDirectory);
            }
        }

        // Verify bisect found the bug but challenge isn't complete yet
        var midResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);
        midResult.IsSuccessful.Should().BeFalse("Challenge should not complete until reset");
        midResult.Message.Should().Contain("SUCCESS", "Should indicate bug was found");
        midResult.Message.Should().Contain("bisect reset", "Should prompt for reset");

        // Now reset bisect to complete the challenge
        await GitExecutor.ExecuteAsync("bisect reset", workingDirectory);

        // Act - Validate after reset
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        validationResult.IsSuccessful.Should().BeTrue();
        validationResult.Message.Should().Contain("Version 6");
    }
}
