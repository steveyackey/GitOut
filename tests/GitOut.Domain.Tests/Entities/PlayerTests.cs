using FluentAssertions;
using GitOut.Domain.Entities;
using Xunit;

namespace GitOut.Domain.Tests.Entities;

public class PlayerTests
{
    [Fact]
    public void Constructor_ShouldInitializePlayer_WithDefaultName()
    {
        // Act
        var player = new Player();

        // Assert
        player.Name.Should().Be("Adventurer");
        player.CompletedRooms.Should().BeEmpty();
        player.CompletedChallenges.Should().BeEmpty();
        player.MoveCount.Should().Be(0);
        player.GameStarted.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_ShouldInitializePlayer_WithCustomName()
    {
        // Act
        var player = new Player("TestPlayer");

        // Assert
        player.Name.Should().Be("TestPlayer");
    }

    [Fact]
    public void RecordMove_ShouldIncrementMoveCount()
    {
        // Arrange
        var player = new Player();

        // Act
        player.RecordMove();
        player.RecordMove();

        // Assert
        player.MoveCount.Should().Be(2);
    }

    [Fact]
    public void CompleteRoom_ShouldAddRoomToCompletedRooms()
    {
        // Arrange
        var player = new Player();

        // Act
        player.CompleteRoom("room-1");
        player.CompleteRoom("room-2");

        // Assert
        player.CompletedRooms.Should().Contain("room-1");
        player.CompletedRooms.Should().Contain("room-2");
        player.CompletedRooms.Should().HaveCount(2);
    }

    [Fact]
    public void CompleteRoom_ShouldNotDuplicateCompletedRooms()
    {
        // Arrange
        var player = new Player();

        // Act
        player.CompleteRoom("room-1");
        player.CompleteRoom("room-1");

        // Assert
        player.CompletedRooms.Should().HaveCount(1);
    }

    [Fact]
    public void CompleteChallenge_ShouldAddChallengeToCompletedChallenges()
    {
        // Arrange
        var player = new Player();

        // Act
        player.CompleteChallenge("challenge-1");
        player.CompleteChallenge("challenge-2");

        // Assert
        player.CompletedChallenges.Should().Contain("challenge-1");
        player.CompletedChallenges.Should().Contain("challenge-2");
        player.CompletedChallenges.Should().HaveCount(2);
    }

    [Fact]
    public void HasCompletedRoom_ShouldReturnTrue_WhenRoomCompleted()
    {
        // Arrange
        var player = new Player();
        player.CompleteRoom("room-1");

        // Act & Assert
        player.HasCompletedRoom("room-1").Should().BeTrue();
        player.HasCompletedRoom("room-2").Should().BeFalse();
    }

    [Fact]
    public void HasCompletedChallenge_ShouldReturnTrue_WhenChallengeCompleted()
    {
        // Arrange
        var player = new Player();
        player.CompleteChallenge("challenge-1");

        // Act & Assert
        player.HasCompletedChallenge("challenge-1").Should().BeTrue();
        player.HasCompletedChallenge("challenge-2").Should().BeFalse();
    }

    [Fact]
    public void GetCompletionPercentage_ShouldCalculateCorrectly()
    {
        // Arrange
        var player = new Player();
        player.CompleteRoom("room-1");
        player.CompleteRoom("room-2");

        // Act
        var percentage = player.GetCompletionPercentage(4);

        // Assert
        percentage.Should().Be(50.0);
    }

    [Fact]
    public void GetCompletionPercentage_ShouldReturnZero_WhenNoTotalRooms()
    {
        // Arrange
        var player = new Player();

        // Act
        var percentage = player.GetCompletionPercentage(0);

        // Assert
        percentage.Should().Be(0);
    }
}
