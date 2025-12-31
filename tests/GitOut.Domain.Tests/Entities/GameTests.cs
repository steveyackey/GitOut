using FluentAssertions;
using GitOut.Domain.Entities;
using Xunit;

namespace GitOut.Domain.Tests.Entities;

public class GameTests
{
    [Fact]
    public void Constructor_ShouldInitializeGame_WithStartRoom()
    {
        // Arrange
        var player = new Player("TestPlayer");
        var startRoom = new Room("room-1", "Start Room", "Description", "Narrative", isStartRoom: true);
        var rooms = new Dictionary<string, Room> { { "room-1", startRoom } };

        // Act
        var game = new Game(player, startRoom, rooms);

        // Assert
        game.Player.Should().Be(player);
        game.CurrentRoom.Should().Be(startRoom);
        game.Rooms.Should().Equal(rooms);
        game.IsActive.Should().BeTrue();
        game.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void MoveToRoom_ShouldMoveToValidRoom_AndRecordMove()
    {
        // Arrange
        var player = new Player("TestPlayer");
        var room1 = new Room("room-1", "Room 1", "Desc", "Narrative", exits: new Dictionary<string, string> { { "forward", "room-2" } }, isStartRoom: true);
        var room2 = new Room("room-2", "Room 2", "Desc", "Narrative");
        var rooms = new Dictionary<string, Room>
        {
            { "room-1", room1 },
            { "room-2", room2 }
        };
        var game = new Game(player, room1, rooms);

        // Act
        var result = game.MoveToRoom("room-2");

        // Assert
        result.Should().BeTrue();
        game.CurrentRoom.Should().Be(room2);
        game.Player.MoveCount.Should().Be(1);
        game.Player.CompletedRooms.Should().Contain("room-2");
    }

    [Fact]
    public void MoveToRoom_ShouldReturnFalse_WhenRoomDoesNotExist()
    {
        // Arrange
        var player = new Player("TestPlayer");
        var startRoom = new Room("room-1", "Start Room", "Desc", "Narrative", isStartRoom: true);
        var rooms = new Dictionary<string, Room> { { "room-1", startRoom } };
        var game = new Game(player, startRoom, rooms);

        // Act
        var result = game.MoveToRoom("non-existent");

        // Assert
        result.Should().BeFalse();
        game.CurrentRoom.Should().Be(startRoom);
        game.Player.MoveCount.Should().Be(0);
    }

    [Fact]
    public void MoveToRoom_ShouldEndGame_WhenMovingToEndRoom()
    {
        // Arrange
        var player = new Player("TestPlayer");
        var room1 = new Room("room-1", "Room 1", "Desc", "Narrative", exits: new Dictionary<string, string> { { "forward", "room-2" } }, isStartRoom: true);
        var room2 = new Room("room-2", "Room 2", "Desc", "Narrative", isEndRoom: true);
        var rooms = new Dictionary<string, Room>
        {
            { "room-1", room1 },
            { "room-2", room2 }
        };
        var game = new Game(player, room1, rooms);

        // Act
        game.MoveToRoom("room-2");

        // Assert
        game.IsActive.Should().BeFalse();
        game.CompletedAt.Should().NotBeNull();
        game.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CompleteCurrentChallenge_ShouldRecordChallengeCompletion()
    {
        // Arrange
        var player = new Player("TestPlayer");
        var challenge = new MockChallenge("challenge-1");
        var room = new Room("room-1", "Room 1", "Desc", "Narrative", challenge, isStartRoom: true);
        var rooms = new Dictionary<string, Room> { { "room-1", room } };
        var game = new Game(player, room, rooms);

        // Act
        game.CompleteCurrentChallenge();

        // Assert
        game.Player.CompletedChallenges.Should().Contain("challenge-1");
    }

    [Fact]
    public void CanExitInDirection_ShouldReturnTrue_ForValidDirection()
    {
        // Arrange
        var player = new Player("TestPlayer");
        var room = new Room("room-1", "Room 1", "Desc", "Narrative",
            exits: new Dictionary<string, string> { { "forward", "room-2" } },
            isStartRoom: true);
        var rooms = new Dictionary<string, Room> { { "room-1", room } };
        var game = new Game(player, room, rooms);

        // Act & Assert
        game.CanExitInDirection("forward").Should().BeTrue();
        game.CanExitInDirection("FORWARD").Should().BeTrue(); // Case insensitive
        game.CanExitInDirection("back").Should().BeFalse();
    }

    [Fact]
    public void GetRoomIdInDirection_ShouldReturnRoomId_ForValidDirection()
    {
        // Arrange
        var player = new Player("TestPlayer");
        var room = new Room("room-1", "Room 1", "Desc", "Narrative",
            exits: new Dictionary<string, string> { { "forward", "room-2" } },
            isStartRoom: true);
        var rooms = new Dictionary<string, Room> { { "room-1", room } };
        var game = new Game(player, room, rooms);

        // Act
        var roomId = game.GetRoomIdInDirection("forward");

        // Assert
        roomId.Should().Be("room-2");
    }

    [Fact]
    public void GetRoomIdInDirection_ShouldReturnNull_ForInvalidDirection()
    {
        // Arrange
        var player = new Player("TestPlayer");
        var room = new Room("room-1", "Room 1", "Desc", "Narrative", isStartRoom: true);
        var rooms = new Dictionary<string, Room> { { "room-1", room } };
        var game = new Game(player, room, rooms);

        // Act
        var roomId = game.GetRoomIdInDirection("forward");

        // Assert
        roomId.Should().BeNull();
    }

    private class MockChallenge : Domain.Challenges.IChallenge
    {
        public string Id { get; }
        public Domain.Challenges.ChallengeType Type => Domain.Challenges.ChallengeType.Repository;
        public string Description => "Mock challenge";

        public MockChallenge(string id)
        {
            Id = id;
        }

        public Task SetupAsync(string workingDirectory) => Task.CompletedTask;

        public Task<Domain.Challenges.ChallengeResult> ValidateAsync(string workingDirectory) =>
            Task.FromResult(new Domain.Challenges.ChallengeResult(true, "Success"));
    }
}
