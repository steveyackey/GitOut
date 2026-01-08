using FluentAssertions;
using GitOut.Infrastructure.Tests.Fixtures;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration.RoomTests;

/// <summary>
/// Integration tests for Room 18: The Blame Chamber (git blame)
/// </summary>
public class Room18BlameChamberTests : RoomIntegrationTestFixture
{
    [Fact]
    public async Task Room18_BlameChamber_ShouldCompleteWhenCulpritIdentified()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room18-test");
        var room = await RoomHelper.LoadRoomAsync("room-18");

        room.Name.Should().Be("The Blame Chamber");

        // Act - Setup creates calculator.js with multiple commits
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify the investigation file was created
        var investigationPath = Path.Combine(workingDirectory, "INVESTIGATION.txt");
        File.Exists(investigationPath).Should().BeTrue("INVESTIGATION.txt should exist");

        // Verify calculator.js exists
        var calculatorPath = Path.Combine(workingDirectory, "calculator.js");
        File.Exists(calculatorPath).Should().BeTrue("calculator.js should exist");

        // Player uses git blame to investigate (simulated by knowing the answer from setup)
        // In real gameplay, player would run: git blame calculator.js
        // And see that line 3 was last modified by the commit "Fix bug: use amount instead of price"

        // Create the CULPRIT.txt file with the answer
        var culpritPath = Path.Combine(workingDirectory, "CULPRIT.txt");
        await File.WriteAllTextAsync(culpritPath, "Fix");
        await GitExecutor.ExecuteAsync("add CULPRIT.txt", workingDirectory);
        await GitExecutor.ExecuteAsync("commit -m \"Identify culprit\"", workingDirectory);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "detective");
    }

    [Fact]
    public async Task Room18_BlameChamber_ShouldFailWhenWrongCulpritIdentified()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room18-fail-test");
        var room = await RoomHelper.LoadRoomAsync("room-18");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Create CULPRIT.txt with wrong answer
        var culpritPath = Path.Combine(workingDirectory, "CULPRIT.txt");
        await File.WriteAllTextAsync(culpritPath, "Wrong");
        await GitExecutor.ExecuteAsync("add CULPRIT.txt", workingDirectory);
        await GitExecutor.ExecuteAsync("commit -m \"Wrong answer\"", workingDirectory);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifyFailure(validationResult, "not been identified");
    }

    [Fact]
    public async Task Room18_BlameChamber_ShouldFailWhenNoCulpritFile()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room18-nofile-test");
        var room = await RoomHelper.LoadRoomAsync("room-18");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Don't create CULPRIT.txt

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifyFailure(validationResult, "not been identified");
    }
}
