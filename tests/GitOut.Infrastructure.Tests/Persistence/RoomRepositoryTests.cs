using FluentAssertions;
using GitOut.Infrastructure.Git;
using GitOut.Infrastructure.Persistence;
using Xunit;

namespace GitOut.Infrastructure.Tests.Persistence;

public class RoomRepositoryTests
{
    [Fact]
    public async Task LoadRoomsAsync_ShouldReturnAllRooms()
    {
        // Arrange
        var gitExecutor = new GitCommandExecutor();
        var repository = new RoomRepository(gitExecutor);

        // Act
        var rooms = await repository.LoadRoomsAsync();

        // Assert - Phase 2 (8 rooms) + Phase 3 (8 rooms) + Phase 4 (7 rooms: 17, 18, 19, 20, 21, 22, 23) = 23 total
        rooms.Should().HaveCount(23);
        rooms.Should().ContainKey("room-1");
        rooms.Should().ContainKey("room-2");
        rooms.Should().ContainKey("room-3");
        rooms.Should().ContainKey("room-4");
        rooms.Should().ContainKey("room-5");
        rooms.Should().ContainKey("room-6");
        rooms.Should().ContainKey("room-7");
        rooms.Should().ContainKey("room-8");
        rooms.Should().ContainKey("room-9");
        rooms.Should().ContainKey("room-10");
        rooms.Should().ContainKey("room-11");
        rooms.Should().ContainKey("room-12");
        rooms.Should().ContainKey("room-13");
        rooms.Should().ContainKey("room-14");
        rooms.Should().ContainKey("room-15");
        rooms.Should().ContainKey("room-16");
        rooms.Should().ContainKey("room-17");
        rooms.Should().ContainKey("room-18");
        rooms.Should().ContainKey("room-19");
        rooms.Should().ContainKey("room-20");
        rooms.Should().ContainKey("room-21");
        rooms.Should().ContainKey("room-22");
        rooms.Should().ContainKey("room-23");
    }

    [Fact]
    public async Task LoadRoomsAsync_ShouldCacheResults()
    {
        // Arrange
        var gitExecutor = new GitCommandExecutor();
        var repository = new RoomRepository(gitExecutor);

        // Act
        var rooms1 = await repository.LoadRoomsAsync();
        var rooms2 = await repository.LoadRoomsAsync();

        // Assert
        rooms1.Should().BeSameAs(rooms2);
    }

    [Fact]
    public async Task GetStartRoomAsync_ShouldReturnRoom1()
    {
        // Arrange
        var gitExecutor = new GitCommandExecutor();
        var repository = new RoomRepository(gitExecutor);

        // Act
        var startRoom = await repository.GetStartRoomAsync();

        // Assert
        startRoom.Should().NotBeNull();
        startRoom!.Id.Should().Be("room-1");
        startRoom.IsStartRoom.Should().BeTrue();
        startRoom.Name.Should().Be("The Initialization Chamber");
    }

    [Fact]
    public async Task GetRoomByIdAsync_ShouldReturnCorrectRoom()
    {
        // Arrange
        var gitExecutor = new GitCommandExecutor();
        var repository = new RoomRepository(gitExecutor);

        // Act
        var room = await repository.GetRoomByIdAsync("room-2");

        // Assert
        room.Should().NotBeNull();
        room!.Id.Should().Be("room-2");
        room.Name.Should().Be("The Staging Area");
        room.IsEndRoom.Should().BeFalse();
    }

    [Fact]
    public async Task GetRoomByIdAsync_ShouldReturnNull_ForInvalidId()
    {
        // Arrange
        var gitExecutor = new GitCommandExecutor();
        var repository = new RoomRepository(gitExecutor);

        // Act
        var room = await repository.GetRoomByIdAsync("non-existent");

        // Assert
        room.Should().BeNull();
    }

    [Fact]
    public async Task Room1_ShouldHaveCorrectChallenge()
    {
        // Arrange
        var gitExecutor = new GitCommandExecutor();
        var repository = new RoomRepository(gitExecutor);

        // Act
        var room = await repository.GetRoomByIdAsync("room-1");

        // Assert
        room.Should().NotBeNull();
        room!.Challenge.Should().NotBeNull();
        room.Challenge!.Id.Should().Be("init-chamber-challenge");
        room.Challenge.Type.Should().Be(Domain.Challenges.ChallengeType.Repository);
    }

    [Fact]
    public async Task Room2_ShouldHaveCorrectChallenge()
    {
        // Arrange
        var gitExecutor = new GitCommandExecutor();
        var repository = new RoomRepository(gitExecutor);

        // Act
        var room = await repository.GetRoomByIdAsync("room-2");

        // Assert
        room.Should().NotBeNull();
        room!.Challenge.Should().NotBeNull();
        room.Challenge!.Id.Should().Be("staging-area-challenge");
    }

    [Fact]
    public async Task Room1_ShouldHaveExitToRoom2()
    {
        // Arrange
        var gitExecutor = new GitCommandExecutor();
        var repository = new RoomRepository(gitExecutor);

        // Act
        var room = await repository.GetRoomByIdAsync("room-1");

        // Assert
        room.Should().NotBeNull();
        room!.Exits.Should().ContainKey("forward");
        room.Exits["forward"].Should().Be("room-2");
    }

    [Fact]
    public async Task Room2_ShouldHaveExitToRoom3()
    {
        // Arrange
        var gitExecutor = new GitCommandExecutor();
        var repository = new RoomRepository(gitExecutor);

        // Act
        var room = await repository.GetRoomByIdAsync("room-2");

        // Assert
        room.Should().NotBeNull();
        room!.Exits.Should().ContainKey("forward");
        room.Exits["forward"].Should().Be("room-3");
    }

    [Fact]
    public async Task Room16_ShouldNotBeEndRoom()
    {
        // Arrange
        var gitExecutor = new GitCommandExecutor();
        var repository = new RoomRepository(gitExecutor);

        // Act
        var room = await repository.GetRoomByIdAsync("room-16");

        // Assert
        room.Should().NotBeNull();
        room!.IsEndRoom.Should().BeFalse("Room 16 is no longer the end room in Phase 4");
        room.Name.Should().Be("The Bisect Battlefield");
        room.Exits.Should().ContainKey("forward").WhoseValue.Should().Be("room-17");
    }

    [Fact]
    public async Task Room8_ShouldNotBeEndRoom()
    {
        // Arrange
        var gitExecutor = new GitCommandExecutor();
        var repository = new RoomRepository(gitExecutor);

        // Act
        var room = await repository.GetRoomByIdAsync("room-8");

        // Assert
        room.Should().NotBeNull();
        room!.IsEndRoom.Should().BeFalse("Room 8 is no longer the end room in Phase 3");
        room.Name.Should().Be("The Quiz Master's Hall");
        room.Exits.Should().ContainKey("forward", "Room 8 should now connect to Room 9");
        room.Exits["forward"].Should().Be("room-9");
    }

    [Fact]
    public async Task Room8_ShouldHaveQuizChallenge()
    {
        // Arrange
        var gitExecutor = new GitCommandExecutor();
        var repository = new RoomRepository(gitExecutor);

        // Act
        var room = await repository.GetRoomByIdAsync("room-8");

        // Assert
        room.Should().NotBeNull();
        room!.Challenge.Should().NotBeNull();
        room.Challenge!.Type.Should().Be(Domain.Challenges.ChallengeType.Quiz);
    }
}
