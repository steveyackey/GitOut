using FluentAssertions;
using GitOut.Application.Interfaces;
using GitOut.Infrastructure.Persistence;
using Xunit;

namespace GitOut.Infrastructure.Tests.Persistence;

public class JsonProgressRepositoryTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly JsonProgressRepository _repository;

    public JsonProgressRepositoryTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"gitout-test-{Guid.NewGuid()}");
        _repository = new JsonProgressRepository(_testDirectory);
    }

    [Fact]
    public async Task SaveProgressAsync_ShouldCreateFile()
    {
        // Arrange
        var progress = new GameProgress(
            PlayerName: "TestPlayer",
            CurrentRoomId: "room-2",
            CompletedRooms: new List<string> { "room-1" },
            CompletedChallenges: new List<string> { "init-chamber-challenge" },
            MoveCount: 1,
            SavedAt: DateTime.UtcNow,
            GameStarted: DateTime.UtcNow.AddMinutes(-5)
        );

        // Act
        await _repository.SaveProgressAsync(progress);

        // Assert
        var saveFilePath = _repository.GetSaveFilePath();
        File.Exists(saveFilePath).Should().BeTrue();
    }

    [Fact]
    public async Task SaveProgressAsync_WithNull_ShouldThrow()
    {
        // Act
        var act = async () => await _repository.SaveProgressAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task LoadProgressAsync_ShouldReturnSavedProgress()
    {
        // Arrange
        var originalProgress = new GameProgress(
            PlayerName: "TestPlayer",
            CurrentRoomId: "room-3",
            CompletedRooms: new List<string> { "room-1", "room-2" },
            CompletedChallenges: new List<string> { "init-chamber-challenge", "staging-area-challenge" },
            MoveCount: 2,
            SavedAt: DateTime.UtcNow,
            GameStarted: DateTime.UtcNow.AddMinutes(-10)
        );

        await _repository.SaveProgressAsync(originalProgress);

        // Act
        var loadedProgress = await _repository.LoadProgressAsync();

        // Assert
        loadedProgress.Should().NotBeNull();
        loadedProgress!.PlayerName.Should().Be("TestPlayer");
        loadedProgress.CurrentRoomId.Should().Be("room-3");
        loadedProgress.CompletedRooms.Should().HaveCount(2);
        loadedProgress.CompletedChallenges.Should().HaveCount(2);
        loadedProgress.MoveCount.Should().Be(2);
    }

    [Fact]
    public async Task LoadProgressAsync_WithNoSavedFile_ShouldReturnNull()
    {
        // Act
        var progress = await _repository.LoadProgressAsync();

        // Assert
        progress.Should().BeNull();
    }

    [Fact]
    public async Task HasSavedProgressAsync_WithNoFile_ShouldReturnFalse()
    {
        // Act
        var hasSaved = await _repository.HasSavedProgressAsync();

        // Assert
        hasSaved.Should().BeFalse();
    }

    [Fact]
    public async Task HasSavedProgressAsync_WithSavedFile_ShouldReturnTrue()
    {
        // Arrange
        var progress = new GameProgress(
            PlayerName: "TestPlayer",
            CurrentRoomId: "room-1",
            CompletedRooms: new List<string>(),
            CompletedChallenges: new List<string>(),
            MoveCount: 0,
            SavedAt: DateTime.UtcNow,
            GameStarted: DateTime.UtcNow
        );

        await _repository.SaveProgressAsync(progress);

        // Act
        var hasSaved = await _repository.HasSavedProgressAsync();

        // Assert
        hasSaved.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteProgressAsync_ShouldRemoveSaveFile()
    {
        // Arrange
        var progress = new GameProgress(
            PlayerName: "TestPlayer",
            CurrentRoomId: "room-1",
            CompletedRooms: new List<string>(),
            CompletedChallenges: new List<string>(),
            MoveCount: 0,
            SavedAt: DateTime.UtcNow,
            GameStarted: DateTime.UtcNow
        );

        await _repository.SaveProgressAsync(progress);

        // Verify file exists
        (await _repository.HasSavedProgressAsync()).Should().BeTrue();

        // Act
        await _repository.DeleteProgressAsync();

        // Assert
        (await _repository.HasSavedProgressAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteProgressAsync_WithNoFile_ShouldNotThrow()
    {
        // Act
        var act = async () => await _repository.DeleteProgressAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SaveAndLoad_ShouldPreserveAllData()
    {
        // Arrange
        var originalProgress = new GameProgress(
            PlayerName: "DetailedPlayer",
            CurrentRoomId: "room-5",
            CompletedRooms: new List<string> { "room-1", "room-2", "room-3", "room-4" },
            CompletedChallenges: new List<string>
            {
                "init-chamber-challenge",
                "staging-area-challenge",
                "history-archive-challenge",
                "status-chamber-challenge"
            },
            MoveCount: 4,
            SavedAt: new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc),
            GameStarted: new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc)
        );

        // Act
        await _repository.SaveProgressAsync(originalProgress);
        var loadedProgress = await _repository.LoadProgressAsync();

        // Assert
        loadedProgress.Should().NotBeNull();
        loadedProgress.Should().BeEquivalentTo(originalProgress);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }
}
