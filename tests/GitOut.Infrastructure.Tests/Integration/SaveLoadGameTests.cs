using FluentAssertions;
using GitOut.Application.Interfaces;
using GitOut.Application.UseCases;
using GitOut.Domain.Entities;
using GitOut.Infrastructure.Git;
using GitOut.Infrastructure.Persistence;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration;

/// <summary>
/// End-to-end tests for save/load game functionality
/// </summary>
public class SaveLoadGameTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly IProgressRepository _repository;
    private readonly IRoomRepository _roomRepository;
    private readonly SaveProgressUseCase _saveUseCase;
    private readonly LoadProgressUseCase _loadUseCase;

    public SaveLoadGameTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"gitout-saveload-test-{Guid.NewGuid()}");
        _repository = new JsonProgressRepository(_testDirectory);

        // Create git executor for room repository
        var gitExecutor = new GitCommandExecutor();
        _roomRepository = new RoomRepository(gitExecutor);

        _saveUseCase = new SaveProgressUseCase(_repository);
        _loadUseCase = new LoadProgressUseCase(_repository, _roomRepository);
    }

    [Fact]
    public async Task SaveAndLoad_ShouldPreserveCompleteGameState()
    {
        // Arrange - Create a game in progress
        var player = new Player("TestAdventurer")
        {
            GameStarted = DateTime.UtcNow.AddMinutes(-15)
        };
        player.RecordMove();
        player.RecordMove();
        player.RecordMove();
        player.CompleteRoom("room-1-init");
        player.CompleteRoom("room-2-staging");
        player.CompleteChallenge("init-chamber-challenge");
        player.CompleteChallenge("staging-area-challenge");

        var rooms = await _roomRepository.LoadRoomsAsync();
        rooms.Should().NotBeEmpty("rooms should be loaded from room repository");

        // Get the third room to be the current room
        var currentRoom = rooms.Values.Skip(2).First();
        var game = new Game(player, currentRoom, rooms);

        // Act - Save the game
        var saveResult = await _saveUseCase.ExecuteAsync(game);

        // Assert - Save should succeed
        saveResult.Success.Should().BeTrue();

        // Act - Load the game
        var loadResult = await _loadUseCase.ExecuteAsync();

        // Assert - Load should succeed
        loadResult.Success.Should().BeTrue();
        loadResult.Game.Should().NotBeNull();

        // Assert - Verify all game state is preserved
        var loadedGame = loadResult.Game!;
        loadedGame.Player.Name.Should().Be("TestAdventurer");
        loadedGame.Player.MoveCount.Should().Be(3);
        loadedGame.Player.CompletedRooms.Should().HaveCount(2);
        loadedGame.Player.CompletedRooms.Should().Contain("room-1-init");
        loadedGame.Player.CompletedRooms.Should().Contain("room-2-staging");
        loadedGame.Player.CompletedChallenges.Should().HaveCount(2);
        loadedGame.Player.CompletedChallenges.Should().Contain("init-chamber-challenge");
        loadedGame.Player.CompletedChallenges.Should().Contain("staging-area-challenge");
        loadedGame.CurrentRoom.Id.Should().Be(currentRoom.Id);
        loadedGame.Player.GameStarted.Should().BeCloseTo(player.GameStarted, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task SaveAndLoad_MultipleTimes_ShouldOverwritePreviousSave()
    {
        // Arrange - Create initial game
        var rooms = await _roomRepository.LoadRoomsAsync();
        var startRoom = rooms.Values.First();

        var player1 = new Player("Player1");
        player1.RecordMove();
        var game1 = new Game(player1, startRoom, rooms);

        // Act - Save first game
        await _saveUseCase.ExecuteAsync(game1);

        // Arrange - Create second game with more progress
        var player2 = new Player("Player2");
        player2.RecordMove();
        player2.RecordMove();
        player2.RecordMove();
        player2.CompleteRoom("room-1-init");
        var game2 = new Game(player2, startRoom, rooms);

        // Act - Save second game (should overwrite)
        await _saveUseCase.ExecuteAsync(game2);

        // Act - Load the game
        var loadResult = await _loadUseCase.ExecuteAsync();

        // Assert - Should have loaded the second game
        loadResult.Success.Should().BeTrue();
        loadResult.Game.Should().NotBeNull();
        loadResult.Game!.Player.Name.Should().Be("Player2");
        loadResult.Game.Player.MoveCount.Should().Be(3);
        loadResult.Game.Player.CompletedRooms.Should().Contain("room-1-init");
    }

    [Fact]
    public async Task Load_WithNoSavedGame_ShouldReturnFailure()
    {
        // Act
        var loadResult = await _loadUseCase.ExecuteAsync();

        // Assert
        loadResult.Success.Should().BeFalse();
        loadResult.Game.Should().BeNull();
        loadResult.Message.Should().Contain("No saved game");
    }

    [Fact]
    public async Task SaveAndLoad_WithMinimalProgress_ShouldWork()
    {
        // Arrange - Create a brand new game with no progress
        var rooms = await _roomRepository.LoadRoomsAsync();
        var startRoom = rooms.Values.First();
        var player = new Player("NewPlayer");
        var game = new Game(player, startRoom, rooms);

        // Act - Save and load
        await _saveUseCase.ExecuteAsync(game);
        var loadResult = await _loadUseCase.ExecuteAsync();

        // Assert
        loadResult.Success.Should().BeTrue();
        loadResult.Game.Should().NotBeNull();
        loadResult.Game!.Player.Name.Should().Be("NewPlayer");
        loadResult.Game.Player.MoveCount.Should().Be(0);
        loadResult.Game.Player.CompletedRooms.Should().BeEmpty();
        loadResult.Game.Player.CompletedChallenges.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveAndLoad_WithMaximumProgress_ShouldWork()
    {
        // Arrange - Create a game with lots of progress
        var rooms = await _roomRepository.LoadRoomsAsync();
        var currentRoom = rooms.Values.Last();

        var player = new Player("ExpertPlayer")
        {
            GameStarted = DateTime.UtcNow.AddHours(-2)
        };

        // Simulate completing all rooms and challenges
        foreach (var room in rooms.Values)
        {
            player.CompleteRoom(room.Id);
            player.RecordMove();

            if (room.Challenge != null)
            {
                player.CompleteChallenge(room.Challenge.Id);
            }
        }

        var game = new Game(player, currentRoom, rooms);

        // Act - Save and load
        await _saveUseCase.ExecuteAsync(game);
        var loadResult = await _loadUseCase.ExecuteAsync();

        // Assert
        loadResult.Success.Should().BeTrue();
        loadResult.Game.Should().NotBeNull();

        var loadedGame = loadResult.Game!;
        loadedGame.Player.Name.Should().Be("ExpertPlayer");
        loadedGame.Player.CompletedRooms.Should().HaveCount(rooms.Count);
        loadedGame.Player.MoveCount.Should().Be(rooms.Count);
        loadedGame.CurrentRoom.Id.Should().Be(currentRoom.Id);
    }

    [Fact]
    public async Task Delete_AfterSave_ShouldRemoveSavedGame()
    {
        // Arrange
        var rooms = await _roomRepository.LoadRoomsAsync();
        var startRoom = rooms.Values.First();
        var player = new Player("TestPlayer");
        var game = new Game(player, startRoom, rooms);

        // Act - Save the game
        await _saveUseCase.ExecuteAsync(game);

        // Verify save exists
        var hasSaved = await _repository.HasSavedProgressAsync();
        hasSaved.Should().BeTrue();

        // Delete the save
        await _repository.DeleteProgressAsync();

        // Try to load
        var loadResult = await _loadUseCase.ExecuteAsync();

        // Assert
        loadResult.Success.Should().BeFalse();
        loadResult.Game.Should().BeNull();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }
}
