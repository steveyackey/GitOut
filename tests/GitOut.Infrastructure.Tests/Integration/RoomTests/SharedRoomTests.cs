using FluentAssertions;
using GitOut.Domain.Challenges;
using GitOut.Infrastructure.Tests.Fixtures;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration.RoomTests;

/// <summary>
/// Shared tests that apply to all rooms or validate room structure
/// </summary>
public class SharedRoomTests : RoomIntegrationTestFixture
{
    [Fact]
    public async Task AllRooms_ShouldHaveChallenges()
    {
        // Arrange & Act & Assert
        await RoomHelper.VerifyAllRoomsHaveChallengesAsync();
    }

    [Fact]
    public async Task AllRooms_ExceptEndRoom_ShouldHaveExits()
    {
        // Arrange & Act & Assert
        await RoomHelper.VerifyAllNonEndRoomsHaveExitsAsync();
    }

    [Fact]
    public async Task AllPhase2Rooms_ShouldExistAndHaveChallenges()
    {
        // Arrange & Act & Assert
        await RoomHelper.VerifyRoomRangeExistsAsync(3, 8);
    }

    [Fact]
    public async Task AllPhase3Rooms_ShouldExistAndHaveChallenges()
    {
        // Arrange & Act & Assert
        await RoomHelper.VerifyRoomRangeExistsAsync(9, 16);
    }

    [Fact]
    public async Task Phase2Rooms_ShouldHaveCorrectConnections()
    {
        // Arrange & Act
        var rooms = await RoomHelper.LoadAllRoomsAsync();

        // Assert - Verify room chain (3-8)
        RoomHelper.VerifyExit(rooms["room-3"], "forward", "room-4");
        RoomHelper.VerifyExit(rooms["room-4"], "forward", "room-5");
        RoomHelper.VerifyExit(rooms["room-5"], "forward", "room-6");
        RoomHelper.VerifyExit(rooms["room-6"], "forward", "room-7");
        RoomHelper.VerifyExit(rooms["room-7"], "forward", "room-8");
        RoomHelper.VerifyExit(rooms["room-8"], "forward", "room-9");
    }

    [Fact]
    public async Task Phase3Rooms_ShouldHaveCorrectConnections()
    {
        // Arrange & Act
        var rooms = await RoomHelper.LoadAllRoomsAsync();

        // Assert - Verify room 8 connects to room 9
        RoomHelper.VerifyExit(rooms["room-8"], "forward", "room-9");

        // Verify Phase 3 room chain (9-16)
        // Note: Room 16 now connects to room-17 (Phase 4), so it's no longer an end room
        RoomHelper.VerifyExit(rooms["room-9"], "forward", "room-10");
        RoomHelper.VerifyExit(rooms["room-10"], "forward", "room-11");
        RoomHelper.VerifyExit(rooms["room-11"], "forward", "room-12");
        RoomHelper.VerifyExit(rooms["room-12"], "forward", "room-13");
        RoomHelper.VerifyExit(rooms["room-13"], "forward", "room-14");
        RoomHelper.VerifyExit(rooms["room-14"], "forward", "room-15");
        RoomHelper.VerifyExit(rooms["room-15"], "forward", "room-16");
        RoomHelper.VerifyExit(rooms["room-16"], "forward", "room-17");
    }

    [Fact]
    public async Task Phase3Rooms_ShouldAllBeRepositoryChallenges()
    {
        // Arrange & Act
        var rooms = await RoomHelper.LoadAllRoomsAsync();

        // Assert - All Phase 3 rooms use RepositoryChallenge
        for (int i = 9; i <= 16; i++)
        {
            var roomId = $"room-{i}";
            RoomHelper.VerifyChallengeType<RepositoryChallenge>(rooms[roomId]);
        }
    }

    [Fact]
    public async Task AllRooms_ShouldFormConnectedChain()
    {
        // Arrange & Act
        var rooms = await RoomHelper.LoadAllRoomsAsync();

        // Assert - Verify the entire chain from room-1 to room-23
        for (int i = 1; i <= 22; i++)
        {
            var roomId = $"room-{i}";
            var nextRoomId = $"room-{i + 1}";
            rooms.Should().ContainKey(roomId, $"Room {roomId} should exist");
            RoomHelper.VerifyExit(rooms[roomId], "forward", nextRoomId);
        }

        // Room 23 should be the end room
        rooms.Should().ContainKey("room-23", "Room 23 should exist");
        RoomHelper.VerifyEndRoom(rooms["room-23"]);
    }

    [Fact]
    public async Task Room1_ShouldBeStartRoom()
    {
        // Arrange & Act
        var rooms = await RoomHelper.LoadAllRoomsAsync();

        // Assert
        rooms.Should().ContainKey("room-1", "Room 1 should exist");
        rooms["room-1"].IsStartRoom.Should().BeTrue("Room 1 should be the start room");
        rooms["room-1"].Name.Should().Be("The Initialization Chamber");
    }
}
