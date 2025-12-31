namespace GitOut.Application.Interfaces;

/// <summary>
/// Repository for saving and loading game progress
/// </summary>
public interface IProgressRepository
{
    /// <summary>
    /// Save game progress to persistent storage
    /// </summary>
    Task SaveProgressAsync(GameProgress progress);

    /// <summary>
    /// Load game progress from persistent storage
    /// </summary>
    /// <returns>GameProgress if saved data exists, null otherwise</returns>
    Task<GameProgress?> LoadProgressAsync();

    /// <summary>
    /// Check if saved progress exists
    /// </summary>
    Task<bool> HasSavedProgressAsync();

    /// <summary>
    /// Delete saved progress
    /// </summary>
    Task DeleteProgressAsync();
}

/// <summary>
/// Represents saved game progress
/// </summary>
public record GameProgress(
    string PlayerName,
    string CurrentRoomId,
    List<string> CompletedRooms,
    List<string> CompletedChallenges,
    int MoveCount,
    DateTime SavedAt,
    DateTime GameStarted
);
