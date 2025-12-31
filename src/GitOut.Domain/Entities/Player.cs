namespace GitOut.Domain.Entities;

/// <summary>
/// Represents the player's state and progress
/// </summary>
public class Player
{
    public string Name { get; init; } = "Adventurer";
    public HashSet<string> CompletedRooms { get; init; } = new();
    public HashSet<string> CompletedChallenges { get; init; } = new();
    public DateTime GameStarted { get; init; } = DateTime.UtcNow;
    public int MoveCount { get; private set; }

    public Player() { }

    public Player(string name)
    {
        Name = name;
    }

    public void RecordMove()
    {
        MoveCount++;
    }

    public void CompleteRoom(string roomId)
    {
        CompletedRooms.Add(roomId);
    }

    public void CompleteChallenge(string challengeId)
    {
        CompletedChallenges.Add(challengeId);
    }

    public bool HasCompletedRoom(string roomId)
    {
        return CompletedRooms.Contains(roomId);
    }

    public bool HasCompletedChallenge(string challengeId)
    {
        return CompletedChallenges.Contains(challengeId);
    }

    public double GetCompletionPercentage(int totalRooms)
    {
        if (totalRooms == 0) return 0;
        return (double)CompletedRooms.Count / totalRooms * 100;
    }
}
