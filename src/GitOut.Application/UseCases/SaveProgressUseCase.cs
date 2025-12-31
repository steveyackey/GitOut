using GitOut.Application.Interfaces;
using GitOut.Domain.Entities;

namespace GitOut.Application.UseCases;

/// <summary>
/// Use case for saving game progress
/// </summary>
public class SaveProgressUseCase
{
    private readonly IProgressRepository _progressRepository;

    public SaveProgressUseCase(IProgressRepository progressRepository)
    {
        _progressRepository = progressRepository ?? throw new ArgumentNullException(nameof(progressRepository));
    }

    public async Task<SaveProgressResult> ExecuteAsync(Game game)
    {
        if (game == null)
        {
            return new SaveProgressResult(
                false,
                "Cannot save: No active game."
            );
        }

        try
        {
            // Convert game state to GameProgress DTO
            var progress = new GameProgress(
                PlayerName: game.Player.Name,
                CurrentRoomId: game.CurrentRoom.Id,
                CompletedRooms: game.Player.CompletedRooms.ToList(),
                CompletedChallenges: game.Player.CompletedChallenges.ToList(),
                MoveCount: game.Player.MoveCount,
                SavedAt: DateTime.UtcNow,
                GameStarted: game.Player.GameStarted
            );

            // Save to repository
            await _progressRepository.SaveProgressAsync(progress);

            return new SaveProgressResult(
                true,
                $"Game progress saved successfully for {game.Player.Name}."
            );
        }
        catch (Exception ex)
        {
            return new SaveProgressResult(
                false,
                $"Failed to save game: {ex.Message}"
            );
        }
    }
}

public record SaveProgressResult(
    bool Success,
    string Message
);
