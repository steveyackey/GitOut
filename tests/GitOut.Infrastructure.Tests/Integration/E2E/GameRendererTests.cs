using FluentAssertions;
using GitOut.Application.Services;
using GitOut.Console.UI;
using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using Spectre.Console;
using Spectre.Console.Testing;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration.E2E;

/// <summary>
/// E2E tests for GameRenderer using Spectre.Console.Testing
/// </summary>
public class GameRendererTests
{
    [Fact]
    public void RenderRoom_ShouldDisplayRoomNameAndNarrative()
    {
        // Arrange
        var console = new TestConsole();
        var renderer = new GameRenderer(console);

        var room = new Room(
            id: "test-room",
            name: "Test Chamber",
            description: "A test room",
            narrative: "You enter a mysterious test chamber...",
            challenge: null,
            exits: new Dictionary<string, string> { { "forward", "room-2" } },
            isStartRoom: false,
            isEndRoom: false
        );
        var player = new Player("TestPlayer");

        // Act
        renderer.RenderRoom(room, player);

        // Assert
        var output = console.Output;
        output.Should().Contain("Test Chamber");
        output.Should().Contain("mysterious test chamber");
        output.Should().Contain("forward");
    }

    [Fact]
    public void RenderCommandResult_Help_ShouldDisplayHelpPanel()
    {
        // Arrange
        var console = new TestConsole();
        var renderer = new GameRenderer(console);

        var result = new CommandResult(
            Success: true,
            Message: "Available Commands:\n- help: Show this help\n- look: Look around",
            Type: CommandType.Help
        );

        // Act
        renderer.RenderCommandResult(result);

        // Assert
        var output = console.Output;
        output.Should().Contain("Help");
        output.Should().Contain("Available Commands");
    }

    [Fact]
    public void RenderCommandResult_GitSuccess_ShouldDisplayOutput()
    {
        // Arrange
        var console = new TestConsole();
        var renderer = new GameRenderer(console);

        var result = new CommandResult(
            Success: true,
            Message: "Initialized empty Git repository",
            Type: CommandType.Git
        );

        // Act
        renderer.RenderCommandResult(result);

        // Assert
        var output = console.Output;
        output.Should().Contain("Initialized empty Git repository");
    }

    [Fact]
    public void RenderCommandResult_GitError_ShouldDisplayErrorMessage()
    {
        // Arrange
        var console = new TestConsole();
        var renderer = new GameRenderer(console);

        var result = new CommandResult(
            Success: false,
            Message: "fatal: not a git repository",
            Type: CommandType.Git
        );

        // Act
        renderer.RenderCommandResult(result);

        // Assert
        var output = console.Output;
        output.Should().Contain("Error");
        output.Should().Contain("fatal: not a git repository");
    }

    [Fact]
    public void RenderCommandResult_ChallengeCompleted_ShouldDisplaySuccessPanel()
    {
        // Arrange
        var console = new TestConsole();
        var renderer = new GameRenderer(console);

        var result = new CommandResult(
            Success: true,
            Message: "Initialized empty Git repository\n\n✓ Challenge completed! You've mastered git init!",
            Type: CommandType.Git
        );

        // Act
        renderer.RenderCommandResult(result);

        // Assert
        var output = console.Output;
        output.Should().Contain("Challenge completed");
    }

    [Fact]
    public void RenderError_ShouldDisplayErrorPanel()
    {
        // Arrange
        var console = new TestConsole();
        var renderer = new GameRenderer(console);

        // Act
        renderer.RenderError("Something went wrong!");

        // Assert
        var output = console.Output;
        output.Should().Contain("Error");
        output.Should().Contain("Something went wrong!");
    }

    [Fact]
    public void RenderWorkingDirectory_ShouldDisplayPath()
    {
        // Arrange
        var console = new TestConsole();
        var renderer = new GameRenderer(console);

        // Act
        renderer.RenderWorkingDirectory("/tmp/gitout/test123");

        // Assert
        var output = console.Output;
        output.Should().Contain("Working Directory");
        output.Should().Contain("/tmp/gitout/test123");
    }

    [Fact]
    public void RenderGameComplete_ShouldDisplayVictoryAndStats()
    {
        // Arrange
        var console = new TestConsole();
        var renderer = new GameRenderer(console);

        var player = new Player("Hero");
        player.CompleteRoom("room-1");
        player.CompleteRoom("room-2");
        player.CompleteChallenge("challenge-1");
        player.CompleteChallenge("challenge-2");
        player.RecordMove();

        // Act
        renderer.RenderGameComplete(player);

        // Assert
        var output = console.Output;
        output.Should().Contain("Victory");
        output.Should().Contain("Hero");
        output.Should().Contain("Congratulations");
    }

    [Fact]
    public void RenderRoom_WithQuizChallenge_ShouldDisplayQuestionAndOptions()
    {
        // Arrange
        var console = new TestConsole();
        var renderer = new GameRenderer(console);

        var quizChallenge = new QuizChallenge(
            id: "quiz-1",
            description: "Test your knowledge",
            question: "What command initializes a repo?",
            options: new List<string> { "git init", "git start", "git create" },
            correctAnswerIndex: 0
        );

        var room = new Room(
            id: "quiz-room",
            name: "Quiz Chamber",
            description: "A quiz room",
            narrative: "The Quiz Master appears...",
            challenge: quizChallenge,
            exits: new Dictionary<string, string>(),
            isStartRoom: false,
            isEndRoom: true
        );
        var player = new Player("TestPlayer");

        // Act
        renderer.RenderRoom(room, player);

        // Assert
        var output = console.Output;
        output.Should().Contain("Quiz Challenge");
        output.Should().Contain("What command initializes a repo?");
        output.Should().Contain("git init");
        output.Should().Contain("git start");
        output.Should().Contain("git create");
        output.Should().Contain("answer");
    }

    [Fact]
    public void RenderCommandResult_Answer_Correct_ShouldDisplayCorrectPanel()
    {
        // Arrange
        var console = new TestConsole();
        var renderer = new GameRenderer(console);

        var result = new CommandResult(
            Success: true,
            Message: "That's correct! Well done!",
            Type: CommandType.Answer
        );

        // Act
        renderer.RenderCommandResult(result);

        // Assert
        var output = console.Output;
        output.Should().Contain("Correct");
    }

    [Fact]
    public void RenderCommandResult_Answer_Wrong_ShouldDisplayIncorrectPanel()
    {
        // Arrange
        var console = new TestConsole();
        var renderer = new GameRenderer(console);

        var result = new CommandResult(
            Success: false,
            Message: "That's not right. Try again!",
            Type: CommandType.Answer
        );

        // Act
        renderer.RenderCommandResult(result);

        // Assert
        var output = console.Output;
        output.Should().Contain("Incorrect");
    }

    [Fact]
    public void RenderCommandResult_Hint_ShouldDisplayHintPanel()
    {
        // Arrange
        var console = new TestConsole();
        var renderer = new GameRenderer(console);

        var result = new CommandResult(
            Success: true,
            Message: "Try using git init to initialize the repository",
            Type: CommandType.Hint
        );

        // Act
        renderer.RenderCommandResult(result);

        // Assert
        var output = console.Output;
        output.Should().Contain("Hint");
        output.Should().Contain("git init");
    }

    [Fact]
    public void RenderCommandResult_Movement_Success_ShouldDisplayGreenMessage()
    {
        // Arrange
        var console = new TestConsole();
        var renderer = new GameRenderer(console);

        var result = new CommandResult(
            Success: true,
            Message: "You move forward into the next room...",
            Type: CommandType.Movement
        );

        // Act
        renderer.RenderCommandResult(result);

        // Assert
        var output = console.Output;
        output.Should().Contain("move forward");
    }

    [Fact]
    public void RenderCommandResult_Movement_Failure_ShouldDisplayRedMessage()
    {
        // Arrange
        var console = new TestConsole();
        var renderer = new GameRenderer(console);

        var result = new CommandResult(
            Success: false,
            Message: "You must complete the current room's challenge first!",
            Type: CommandType.Movement
        );

        // Act
        renderer.RenderCommandResult(result);

        // Assert
        var output = console.Output;
        output.Should().Contain("must complete");
    }
}
