using FluentAssertions;
using GitOut.Application.Interfaces;
using GitOut.Application.UseCases;
using GitOut.Domain.Entities;
using Moq;
using Xunit;

namespace GitOut.Infrastructure.Tests.UseCases;

public class LoadProgressUseCaseTests
{
    private readonly Mock<IProgressRepository> _mockProgressRepository;
    private readonly Mock<IRoomRepository> _mockRoomRepository;
    private readonly LoadProgressUseCase _useCase;

    public LoadProgressUseCaseTests()
    {
        _mockProgressRepository = new Mock<IProgressRepository>();
        _mockRoomRepository = new Mock<IRoomRepository>();
        _useCase = new LoadProgressUseCase(
            _mockProgressRepository.Object,
            _mockRoomRepository.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WithNoSavedProgress_ShouldReturnFailure()
    {
        // Arrange
        _mockProgressRepository
            .Setup(r => r.HasSavedProgressAsync())
            .ReturnsAsync(false);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Game.Should().BeNull();
        result.Message.Should().Contain("No saved game");
    }

    [Fact]
    public async Task ExecuteAsync_WithValidSavedProgress_ShouldLoadGame()
    {
        // Arrange
        var room1 = new Room("room-1", "Room 1", "First room", "First narrative");
        var room2 = new Room("room-2", "Room 2", "Second room", "Second narrative");
        var rooms = new Dictionary<string, Room>
        {
            { "room-1", room1 },
            { "room-2", room2 }
        };

        var progress = new GameProgress(
            PlayerName: "SavedPlayer",
            CurrentRoomId: "room-2",
            CompletedRooms: new List<string> { "room-1" },
            CompletedChallenges: new List<string> { "challenge-1" },
            MoveCount: 3,
            SavedAt: DateTime.UtcNow,
            GameStarted: DateTime.UtcNow.AddMinutes(-10)
        );

        _mockProgressRepository
            .Setup(r => r.HasSavedProgressAsync())
            .ReturnsAsync(true);

        _mockProgressRepository
            .Setup(r => r.LoadProgressAsync())
            .ReturnsAsync(progress);

        _mockRoomRepository
            .Setup(r => r.LoadRoomsAsync())
            .ReturnsAsync(rooms);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("SavedPlayer");
        result.Game.Should().NotBeNull();

        var game = result.Game!;
        game.Player.Name.Should().Be("SavedPlayer");
        game.CurrentRoom.Id.Should().Be("room-2");
        game.Player.CompletedRooms.Should().Contain("room-1");
        game.Player.CompletedChallenges.Should().Contain("challenge-1");
        game.Player.MoveCount.Should().Be(3);
        game.Player.GameStarted.Should().Be(progress.GameStarted);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLoadProgressReturnsNull_ShouldReturnFailure()
    {
        // Arrange
        _mockProgressRepository
            .Setup(r => r.HasSavedProgressAsync())
            .ReturnsAsync(true);

        _mockProgressRepository
            .Setup(r => r.LoadProgressAsync())
            .ReturnsAsync((GameProgress?)null);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Game.Should().BeNull();
        result.Message.Should().Contain("Failed to load");
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoRoomsAvailable_ShouldReturnFailure()
    {
        // Arrange
        var progress = new GameProgress(
            PlayerName: "SavedPlayer",
            CurrentRoomId: "room-1",
            CompletedRooms: new List<string>(),
            CompletedChallenges: new List<string>(),
            MoveCount: 0,
            SavedAt: DateTime.UtcNow,
            GameStarted: DateTime.UtcNow
        );

        _mockProgressRepository
            .Setup(r => r.HasSavedProgressAsync())
            .ReturnsAsync(true);

        _mockProgressRepository
            .Setup(r => r.LoadProgressAsync())
            .ReturnsAsync(progress);

        _mockRoomRepository
            .Setup(r => r.LoadRoomsAsync())
            .ReturnsAsync(new Dictionary<string, Room>());

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Game.Should().BeNull();
        result.Message.Should().Contain("No rooms found");
    }

    [Fact]
    public async Task ExecuteAsync_WhenCurrentRoomNotFound_ShouldReturnFailure()
    {
        // Arrange
        var room1 = new Room("room-1", "Room 1", "First room", "First narrative");
        var rooms = new Dictionary<string, Room>
        {
            { "room-1", room1 }
        };

        var progress = new GameProgress(
            PlayerName: "SavedPlayer",
            CurrentRoomId: "room-999", // Room doesn't exist
            CompletedRooms: new List<string>(),
            CompletedChallenges: new List<string>(),
            MoveCount: 0,
            SavedAt: DateTime.UtcNow,
            GameStarted: DateTime.UtcNow
        );

        _mockProgressRepository
            .Setup(r => r.HasSavedProgressAsync())
            .ReturnsAsync(true);

        _mockProgressRepository
            .Setup(r => r.LoadProgressAsync())
            .ReturnsAsync(progress);

        _mockRoomRepository
            .Setup(r => r.LoadRoomsAsync())
            .ReturnsAsync(rooms);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Game.Should().BeNull();
        result.Message.Should().Contain("room-999");
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryThrows_ShouldReturnFailure()
    {
        // Arrange
        _mockProgressRepository
            .Setup(r => r.HasSavedProgressAsync())
            .ThrowsAsync(new InvalidOperationException("Repository error"));

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Game.Should().BeNull();
        result.Message.Should().Contain("Failed to load");
    }

    [Fact]
    public void Constructor_WithNullProgressRepository_ShouldThrow()
    {
        // Act
        var act = () => new LoadProgressUseCase(null!, _mockRoomRepository.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullRoomRepository_ShouldThrow()
    {
        // Act
        var act = () => new LoadProgressUseCase(_mockProgressRepository.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRestorePlayerStateCorrectly()
    {
        // Arrange
        var room1 = new Room("room-1", "Room 1", "First room", "First narrative");
        var room2 = new Room("room-2", "Room 2", "Second room", "Second narrative");
        var room3 = new Room("room-3", "Room 3", "Third room", "Third narrative");
        var rooms = new Dictionary<string, Room>
        {
            { "room-1", room1 },
            { "room-2", room2 },
            { "room-3", room3 }
        };

        var gameStarted = DateTime.UtcNow.AddMinutes(-20);
        var progress = new GameProgress(
            PlayerName: "DetailedPlayer",
            CurrentRoomId: "room-3",
            CompletedRooms: new List<string> { "room-1", "room-2" },
            CompletedChallenges: new List<string> { "challenge-1", "challenge-2" },
            MoveCount: 5,
            SavedAt: DateTime.UtcNow,
            GameStarted: gameStarted
        );

        _mockProgressRepository
            .Setup(r => r.HasSavedProgressAsync())
            .ReturnsAsync(true);

        _mockProgressRepository
            .Setup(r => r.LoadProgressAsync())
            .ReturnsAsync(progress);

        _mockRoomRepository
            .Setup(r => r.LoadRoomsAsync())
            .ReturnsAsync(rooms);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Game.Should().NotBeNull();

        var player = result.Game!.Player;
        player.Name.Should().Be("DetailedPlayer");
        player.CompletedRooms.Should().HaveCount(2);
        player.CompletedRooms.Should().Contain("room-1");
        player.CompletedRooms.Should().Contain("room-2");
        player.CompletedChallenges.Should().HaveCount(2);
        player.CompletedChallenges.Should().Contain("challenge-1");
        player.CompletedChallenges.Should().Contain("challenge-2");
        player.MoveCount.Should().Be(5);
        player.GameStarted.Should().Be(gameStarted);
    }
}
