using GitOut.Domain.Entities;

namespace GitOut.Application.Interfaces;

/// <summary>
/// Repository for loading and managing rooms
/// </summary>
public interface IRoomRepository
{
    /// <summary>
    /// Load all rooms from storage
    /// </summary>
    Task<Dictionary<string, Room>> LoadRoomsAsync();

    /// <summary>
    /// Get a specific room by ID
    /// </summary>
    Task<Room?> GetRoomByIdAsync(string roomId);

    /// <summary>
    /// Get the starting room
    /// </summary>
    Task<Room?> GetStartRoomAsync();
}
