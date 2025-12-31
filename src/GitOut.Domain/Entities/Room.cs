using GitOut.Domain.Challenges;

namespace GitOut.Domain.Entities;

/// <summary>
/// Represents a room in the dungeon
/// </summary>
public class Room
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Narrative { get; init; } = string.Empty;
    public IChallenge? Challenge { get; init; }
    public Dictionary<string, string> Exits { get; init; } = new();
    public bool IsStartRoom { get; init; }
    public bool IsEndRoom { get; init; }

    public Room() { }

    public Room(
        string id,
        string name,
        string description,
        string narrative,
        IChallenge? challenge = null,
        Dictionary<string, string>? exits = null,
        bool isStartRoom = false,
        bool isEndRoom = false)
    {
        Id = id;
        Name = name;
        Description = description;
        Narrative = narrative;
        Challenge = challenge;
        Exits = exits ?? new Dictionary<string, string>();
        IsStartRoom = isStartRoom;
        IsEndRoom = isEndRoom;
    }
}
