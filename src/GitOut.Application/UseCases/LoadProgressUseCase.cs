using GitOut.Application.Interfaces;
using GitOut.Domain.Entities;

namespace GitOut.Application.UseCases;

/// <summary>
/// Use case for loading saved game progress
/// </summary>
public class LoadProgressUseCase
{
    private readonly IProgressRepository _progressRepository;
    private readonly IRoomRepository _roomRepository;

    public LoadProgressUseCase(
        IProgressRepository progressRepository,
        IRoomRepository roomRepository)
    {
        _progressRepository = progressRepository ?? throw new ArgumentNullException(nameof(progressRepository));
        _roomRepository = roomRepository ?? throw new ArgumentNullException(nameof(roomRepository));
    }

    public async Task<LoadProgressResult> ExecuteAsync()
    {
        try
        {
            // Check if saved progress exists
            var hasSaved = await _progressRepository.HasSavedProgressAsync();
            if (!hasSaved)
            {
                return new LoadProgressResult(
                    false,
                    null,
                    "No saved game found."
                );
            }

            // Load progress
            var progress = await _progressRepository.LoadProgressAsync();
            if (progress == null)
            {
                return new LoadProgressResult(
                    false,
                    null,
                    "Failed to load saved game."
                );
            }

            // Load all rooms
            var rooms = await _roomRepository.LoadRoomsAsync();
            if (rooms.Count == 0)
            {
                return new LoadProgressResult(
                    false,
                    null,
                    "No rooms found. Cannot load game."
                );
            }

            // Get the current room from progress
            if (!rooms.TryGetValue(progress.CurrentRoomId, out var currentRoom))
            {
                return new LoadProgressResult(
                    false,
                    null,
                    $"Saved room '{progress.CurrentRoomId}' not found."
                );
            }

            // Reconstruct player with saved state
            // Note: Player properties are init-only, so we need to use object initializer
            var player = new Player
            {
                Name = progress.PlayerName,
                CompletedRooms = progress.CompletedRooms.ToHashSet(),
                CompletedChallenges = progress.CompletedChallenges.ToHashSet(),
                GameStarted = progress.GameStarted
            };

            // Restore move count by recording moves
            // Since MoveCount is private set, we need to recreate the moves
            for (int i = 0; i < progress.MoveCount; i++)
            {
                player.RecordMove();
            }

            // Create game with restored state
            var game = new Game(player, currentRoom, rooms);

            return new LoadProgressResult(
                true,
                game,
                $"Game loaded successfully! Welcome back, {progress.PlayerName}."
            );
        }
        catch (Exception ex)
        {
            return new LoadProgressResult(
                false,
                null,
                $"Failed to load game: {ex.Message}"
            );
        }
    }
}

public record LoadProgressResult(
    bool Success,
    Game? Game,
    string Message
);
