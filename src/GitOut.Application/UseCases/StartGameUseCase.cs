using GitOut.Application.Interfaces;
using GitOut.Domain.Entities;

namespace GitOut.Application.UseCases;

/// <summary>
/// Use case for starting a new game
/// </summary>
public class StartGameUseCase
{
    private readonly IRoomRepository _roomRepository;

    public StartGameUseCase(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository ?? throw new ArgumentNullException(nameof(roomRepository));
    }

    public async Task<StartGameResult> ExecuteAsync(string playerName)
    {
        return await ExecuteAsync(playerName, startingRoomId: null);
    }

    /// <summary>
    /// Start a new game, optionally at a specific room (for testing/debugging)
    /// </summary>
    /// <param name="playerName">Player's name</param>
    /// <param name="startingRoomId">Optional room ID to start at (e.g., "room-5"). If null, uses the default start room.</param>
    public async Task<StartGameResult> ExecuteAsync(string playerName, string? startingRoomId)
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "Adventurer";
        }

        // Load all rooms
        var rooms = await _roomRepository.LoadRoomsAsync();

        if (rooms.Count == 0)
        {
            return new StartGameResult(
                false,
                null,
                "No rooms found. Cannot start game."
            );
        }

        Room? startRoom;

        // If a specific starting room was requested, use it
        if (!string.IsNullOrWhiteSpace(startingRoomId))
        {
            if (!rooms.TryGetValue(startingRoomId, out startRoom))
            {
                return new StartGameResult(
                    false,
                    null,
                    $"Room '{startingRoomId}' not found. Available rooms: {string.Join(", ", rooms.Keys.Order())}"
                );
            }
        }
        else
        {
            // Get the default starting room
            startRoom = await _roomRepository.GetStartRoomAsync();

            if (startRoom == null)
            {
                return new StartGameResult(
                    false,
                    null,
                    "No starting room found. Cannot start game."
                );
            }
        }

        // Create player
        var player = new Player(playerName);

        // Create game
        var game = new Game(player, startRoom, rooms);

        var message = startingRoomId != null
            ? $"Game started at {startRoom.Name}! Welcome, {playerName}! (Debug mode: --room {startingRoomId})"
            : $"Game started! Welcome, {playerName}!";

        return new StartGameResult(
            true,
            game,
            message
        );
    }
}

public record StartGameResult(
    bool Success,
    Game? Game,
    string Message
);
