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

        // Get the starting room
        var startRoom = await _roomRepository.GetStartRoomAsync();

        if (startRoom == null)
        {
            return new StartGameResult(
                false,
                null,
                "No starting room found. Cannot start game."
            );
        }

        // Create player
        var player = new Player(playerName);

        // Create game
        var game = new Game(player, startRoom, rooms);

        return new StartGameResult(
            true,
            game,
            $"Game started! Welcome, {playerName}!"
        );
    }
}

public record StartGameResult(
    bool Success,
    Game? Game,
    string Message
);
