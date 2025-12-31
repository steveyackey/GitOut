namespace GitOut.Domain.Entities;

/// <summary>
/// Represents the overall game state
/// </summary>
public class Game
{
    public Player Player { get; init; }
    public Room CurrentRoom { get; private set; }
    public Dictionary<string, Room> Rooms { get; init; }
    public bool IsActive { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public Game(Player player, Room startRoom, Dictionary<string, Room> rooms)
    {
        Player = player;
        CurrentRoom = startRoom;
        Rooms = rooms;
        IsActive = true;
    }

    public bool MoveToRoom(string roomId)
    {
        if (!Rooms.ContainsKey(roomId))
        {
            return false;
        }

        CurrentRoom = Rooms[roomId];
        Player.RecordMove();
        Player.CompleteRoom(roomId);

        if (CurrentRoom.IsEndRoom)
        {
            EndGame();
        }

        return true;
    }

    public void CompleteCurrentChallenge()
    {
        if (CurrentRoom.Challenge != null)
        {
            Player.CompleteChallenge(CurrentRoom.Challenge.Id);
        }
    }

    public bool CanExitInDirection(string direction)
    {
        return CurrentRoom.Exits.ContainsKey(direction.ToLower());
    }

    public string? GetRoomIdInDirection(string direction)
    {
        CurrentRoom.Exits.TryGetValue(direction.ToLower(), out var roomId);
        return roomId;
    }

    private void EndGame()
    {
        IsActive = false;
        CompletedAt = DateTime.UtcNow;
    }
}
