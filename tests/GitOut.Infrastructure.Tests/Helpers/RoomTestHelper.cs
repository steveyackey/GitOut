using FluentAssertions;
using GitOut.Application.Interfaces;
using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;

namespace GitOut.Infrastructure.Tests.Helpers;

/// <summary>
/// Helper class for common room operations used across tests
/// </summary>
public class RoomTestHelper
{
    private readonly IRoomRepository _roomRepository;

    public RoomTestHelper(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository ?? throw new ArgumentNullException(nameof(roomRepository));
    }

    /// <summary>
    /// Load a room by ID and verify it exists
    /// </summary>
    public async Task<Room> LoadRoomAsync(string roomId)
    {
        var room = await _roomRepository.GetRoomByIdAsync(roomId);
        room.Should().NotBeNull($"Room {roomId} should exist");
        return room!;
    }

    /// <summary>
    /// Load a room by ID
    /// </summary>
    public async Task<Room?> TryLoadRoomAsync(string roomId)
    {
        return await _roomRepository.GetRoomByIdAsync(roomId);
    }

    /// <summary>
    /// Verify that a room exists and has the expected name
    /// </summary>
    public async Task VerifyRoomAsync(string roomId, string expectedName)
    {
        var room = await LoadRoomAsync(roomId);
        room.Name.Should().Be(expectedName, $"Room {roomId} should have the expected name");
    }

    /// <summary>
    /// Verify that a room exists, has the expected name, and has a challenge
    /// </summary>
    public async Task VerifyRoomWithChallengeAsync(string roomId, string expectedName)
    {
        var room = await LoadRoomAsync(roomId);
        room.Name.Should().Be(expectedName, $"Room {roomId} should have the expected name");
        room.Challenge.Should().NotBeNull($"Room {roomId} should have a challenge");
    }

    /// <summary>
    /// Load a room and verify it has a challenge
    /// </summary>
    public async Task<Room> LoadRoomWithChallengeAsync(string roomId)
    {
        var room = await LoadRoomAsync(roomId);
        room.Challenge.Should().NotBeNull($"Room {roomId} should have a challenge");
        return room;
    }

    /// <summary>
    /// Setup a room's challenge in the specified working directory
    /// </summary>
    public async Task SetupChallengeAsync(Room room, string workingDirectory)
    {
        room.Challenge.Should().NotBeNull($"Room {room.Id} should have a challenge");
        await room.Challenge!.SetupAsync(workingDirectory);
    }

    /// <summary>
    /// Setup a room's challenge by room ID
    /// </summary>
    public async Task SetupChallengeAsync(string roomId, string workingDirectory)
    {
        var room = await LoadRoomWithChallengeAsync(roomId);
        await SetupChallengeAsync(room, workingDirectory);
    }

    /// <summary>
    /// Validate a room's challenge in the specified working directory
    /// </summary>
    public async Task<ChallengeResult> ValidateChallengeAsync(Room room, string workingDirectory)
    {
        room.Challenge.Should().NotBeNull($"Room {room.Id} should have a challenge");
        return await room.Challenge!.ValidateAsync(workingDirectory);
    }

    /// <summary>
    /// Validate a room's challenge by room ID
    /// </summary>
    public async Task<ChallengeResult> ValidateChallengeAsync(string roomId, string workingDirectory)
    {
        var room = await LoadRoomWithChallengeAsync(roomId);
        return await ValidateChallengeAsync(room, workingDirectory);
    }

    /// <summary>
    /// Setup and validate a challenge, asserting it succeeds
    /// </summary>
    public async Task<ChallengeResult> SetupAndValidateAsync(string roomId, string workingDirectory)
    {
        var room = await LoadRoomWithChallengeAsync(roomId);
        await SetupChallengeAsync(room, workingDirectory);
        var result = await ValidateChallengeAsync(room, workingDirectory);
        return result;
    }

    /// <summary>
    /// Verify that a challenge validation succeeds
    /// </summary>
    public void VerifySuccess(ChallengeResult result, string? expectedMessageContains = null)
    {
        result.IsSuccessful.Should().BeTrue($"Challenge should succeed. Message: {result.Message}, Hint: {result.Hint}");

        if (expectedMessageContains != null)
        {
            result.Message.Should().Contain(expectedMessageContains, "Success message should contain expected text");
        }
    }

    /// <summary>
    /// Verify that a challenge validation fails
    /// </summary>
    public void VerifyFailure(ChallengeResult result, string? expectedMessageContains = null)
    {
        result.IsSuccessful.Should().BeFalse("Challenge should fail");

        if (expectedMessageContains != null)
        {
            result.Message.Should().Contain(expectedMessageContains, "Failure message should contain expected text");
        }
    }

    /// <summary>
    /// Verify that a room has an exit in the specified direction
    /// </summary>
    public void VerifyExit(Room room, string direction, string expectedRoomId)
    {
        room.Exits.Should().ContainKey(direction, $"Room {room.Id} should have an exit in direction {direction}");
        room.Exits[direction].Should().Be(expectedRoomId, $"Exit {direction} should lead to {expectedRoomId}");
    }

    /// <summary>
    /// Verify that a room is the end room
    /// </summary>
    public void VerifyEndRoom(Room room)
    {
        room.IsEndRoom.Should().BeTrue($"Room {room.Id} should be marked as an end room");
        room.Exits.Should().BeEmpty($"End room {room.Id} should have no exits");
    }

    /// <summary>
    /// Verify that a room is not the end room
    /// </summary>
    public void VerifyNotEndRoom(Room room)
    {
        room.IsEndRoom.Should().BeFalse($"Room {room.Id} should not be marked as an end room");
        room.Exits.Should().NotBeEmpty($"Non-end room {room.Id} should have at least one exit");
    }

    /// <summary>
    /// Verify that a room's challenge is of a specific type
    /// </summary>
    public void VerifyChallengeType<T>(Room room) where T : IChallenge
    {
        room.Challenge.Should().NotBeNull($"Room {room.Id} should have a challenge");
        room.Challenge.Should().BeOfType<T>($"Room {room.Id} challenge should be of type {typeof(T).Name}");
    }

    /// <summary>
    /// Get a room's challenge as a specific type
    /// </summary>
    public T GetChallengeAs<T>(Room room) where T : IChallenge
    {
        room.Challenge.Should().NotBeNull($"Room {room.Id} should have a challenge");
        room.Challenge.Should().BeOfType<T>($"Room {room.Id} challenge should be of type {typeof(T).Name}");
        return (T)room.Challenge!;
    }

    /// <summary>
    /// Load all rooms and verify count
    /// </summary>
    public async Task<Dictionary<string, Room>> LoadAllRoomsAsync(int? expectedCount = null)
    {
        var rooms = await _roomRepository.LoadRoomsAsync();
        rooms.Should().NotBeNull("Rooms dictionary should not be null");

        if (expectedCount.HasValue)
        {
            rooms.Should().HaveCount(expectedCount.Value, $"Should have exactly {expectedCount.Value} rooms");
        }

        return rooms;
    }

    /// <summary>
    /// Verify that all rooms in a range exist
    /// </summary>
    public async Task VerifyRoomRangeExistsAsync(int startRoomNumber, int endRoomNumber)
    {
        var rooms = await _roomRepository.LoadRoomsAsync();

        for (int i = startRoomNumber; i <= endRoomNumber; i++)
        {
            var roomId = $"room-{i}";
            rooms.Should().ContainKey(roomId, $"Room {roomId} should exist");
            rooms[roomId].Challenge.Should().NotBeNull($"Room {roomId} should have a challenge");
        }
    }

    /// <summary>
    /// Verify that a sequential chain of rooms is properly connected
    /// </summary>
    public async Task VerifyRoomChainAsync(params int[] roomNumbers)
    {
        var rooms = await _roomRepository.LoadRoomsAsync();

        for (int i = 0; i < roomNumbers.Length - 1; i++)
        {
            var currentRoomId = $"room-{roomNumbers[i]}";
            var nextRoomId = $"room-{roomNumbers[i + 1]}";

            rooms.Should().ContainKey(currentRoomId, $"Room {currentRoomId} should exist");
            var room = rooms[currentRoomId];

            VerifyExit(room, "forward", nextRoomId);
        }

        // Verify last room is end room
        var lastRoomId = $"room-{roomNumbers[^1]}";
        rooms.Should().ContainKey(lastRoomId, $"Room {lastRoomId} should exist");
        VerifyEndRoom(rooms[lastRoomId]);
    }

    /// <summary>
    /// Verify that all rooms have challenges
    /// </summary>
    public async Task VerifyAllRoomsHaveChallengesAsync()
    {
        var rooms = await _roomRepository.LoadRoomsAsync();

        foreach (var room in rooms.Values)
        {
            room.Challenge.Should().NotBeNull($"Room {room.Id} should have a challenge");
        }
    }

    /// <summary>
    /// Verify that all non-end rooms have exits
    /// </summary>
    public async Task VerifyAllNonEndRoomsHaveExitsAsync()
    {
        var rooms = await _roomRepository.LoadRoomsAsync();

        foreach (var room in rooms.Values)
        {
            if (!room.IsEndRoom)
            {
                room.Exits.Should().NotBeEmpty($"Non-end room {room.Id} should have at least one exit");
            }
            else
            {
                room.Exits.Should().BeEmpty($"End room {room.Id} should have no exits");
            }
        }
    }
}
