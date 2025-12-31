using FluentAssertions;
using GitOut.Domain.Entities;
using Xunit;

namespace GitOut.Domain.Tests.Entities;

public class RoomTests
{
    [Fact]
    public void Constructor_ShouldInitializeRoom_WithAllProperties()
    {
        // Arrange & Act
        var exits = new Dictionary<string, string> { { "forward", "room-2" } };
        var room = new Room(
            id: "room-1",
            name: "Test Room",
            description: "A test room",
            narrative: "This is a narrative",
            challenge: null,
            exits: exits,
            isStartRoom: true,
            isEndRoom: false
        );

        // Assert
        room.Id.Should().Be("room-1");
        room.Name.Should().Be("Test Room");
        room.Description.Should().Be("A test room");
        room.Narrative.Should().Be("This is a narrative");
        room.Challenge.Should().BeNull();
        room.Exits.Should().Equal(exits);
        room.IsStartRoom.Should().BeTrue();
        room.IsEndRoom.Should().BeFalse();
    }

    [Fact]
    public void Constructor_ShouldInitializeRoom_WithDefaultValues()
    {
        // Act
        var room = new Room();

        // Assert
        room.Id.Should().Be(string.Empty);
        room.Name.Should().Be(string.Empty);
        room.Description.Should().Be(string.Empty);
        room.Narrative.Should().Be(string.Empty);
        room.Challenge.Should().BeNull();
        room.Exits.Should().NotBeNull();
        room.Exits.Should().BeEmpty();
        room.IsStartRoom.Should().BeFalse();
        room.IsEndRoom.Should().BeFalse();
    }

    [Fact]
    public void Constructor_ShouldInitializeEmptyExits_WhenExitsIsNull()
    {
        // Act
        var room = new Room(
            id: "room-1",
            name: "Test Room",
            description: "A test room",
            narrative: "This is a narrative",
            challenge: null,
            exits: null
        );

        // Assert
        room.Exits.Should().NotBeNull();
        room.Exits.Should().BeEmpty();
    }
}
