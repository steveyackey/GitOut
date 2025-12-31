using FluentAssertions;
using GitOut.Application.Interfaces;
using GitOut.Application.UseCases;
using GitOut.Domain.Entities;
using Moq;
using Xunit;

namespace GitOut.Infrastructure.Tests.UseCases;

public class SaveProgressUseCaseTests
{
    private readonly Mock<IProgressRepository> _mockRepository;
    private readonly SaveProgressUseCase _useCase;

    public SaveProgressUseCaseTests()
    {
        _mockRepository = new Mock<IProgressRepository>();
        _useCase = new SaveProgressUseCase(_mockRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidGame_ShouldSaveProgress()
    {
        // Arrange
        var player = new Player("TestPlayer");
        player.RecordMove();
        player.CompleteRoom("room-1");
        player.CompleteChallenge("challenge-1");

        var room = new Room("room-2", "Test Room", "Test description", "Test narrative");
        var rooms = new Dictionary<string, Room>
        {
            { "room-1", new Room("room-1", "Room 1", "First room", "First narrative") },
            { "room-2", room }
        };

        var game = new Game(player, room, rooms);

        GameProgress? savedProgress = null;
        _mockRepository
            .Setup(r => r.SaveProgressAsync(It.IsAny<GameProgress>()))
            .Callback<GameProgress>(p => savedProgress = p)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(game);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("TestPlayer");

        _mockRepository.Verify(r => r.SaveProgressAsync(It.IsAny<GameProgress>()), Times.Once);

        savedProgress.Should().NotBeNull();
        savedProgress!.PlayerName.Should().Be("TestPlayer");
        savedProgress.CurrentRoomId.Should().Be("room-2");
        savedProgress.CompletedRooms.Should().Contain("room-1");
        savedProgress.CompletedChallenges.Should().Contain("challenge-1");
        savedProgress.MoveCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullGame_ShouldReturnFailure()
    {
        // Act
        var result = await _useCase.ExecuteAsync(null!);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("No active game");

        _mockRepository.Verify(r => r.SaveProgressAsync(It.IsAny<GameProgress>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryThrows_ShouldReturnFailure()
    {
        // Arrange
        var player = new Player("TestPlayer");
        var room = new Room("room-1", "Test Room", "Test description", "Test narrative");
        var rooms = new Dictionary<string, Room> { { "room-1", room } };
        var game = new Game(player, room, rooms);

        _mockRepository
            .Setup(r => r.SaveProgressAsync(It.IsAny<GameProgress>()))
            .ThrowsAsync(new InvalidOperationException("Save failed"));

        // Act
        var result = await _useCase.ExecuteAsync(game);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Failed to save");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSaveCurrentTimestamp()
    {
        // Arrange
        var player = new Player("TestPlayer");
        var room = new Room("room-1", "Test Room", "Test description", "Test narrative");
        var rooms = new Dictionary<string, Room> { { "room-1", room } };
        var game = new Game(player, room, rooms);

        GameProgress? savedProgress = null;
        _mockRepository
            .Setup(r => r.SaveProgressAsync(It.IsAny<GameProgress>()))
            .Callback<GameProgress>(p => savedProgress = p)
            .Returns(Task.CompletedTask);

        var beforeSave = DateTime.UtcNow;

        // Act
        await _useCase.ExecuteAsync(game);

        var afterSave = DateTime.UtcNow;

        // Assert
        savedProgress.Should().NotBeNull();
        savedProgress!.SavedAt.Should().BeOnOrAfter(beforeSave);
        savedProgress.SavedAt.Should().BeOnOrBefore(afterSave);
    }

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrow()
    {
        // Act
        var act = () => new SaveProgressUseCase(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
