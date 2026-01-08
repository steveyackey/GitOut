using FluentAssertions;
using GitOut.Application.Services;
using GitOut.Application.UseCases;
using GitOut.Console.UI;
using GitOut.Infrastructure.FileSystem;
using GitOut.Infrastructure.Git;
using GitOut.Infrastructure.Persistence;
using Spectre.Console.Testing;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration.E2E;

/// <summary>
/// E2E tests for full game flow using Spectre.Console.Testing
/// </summary>
public class GameFlowE2ETests : IDisposable
{
    private readonly TempDirectoryManager _tempDirManager;
    private readonly GitCommandExecutor _gitExecutor;
    private readonly RoomRepository _roomRepository;
    private readonly StartGameUseCase _startGameUseCase;
    private readonly GameEngine _gameEngine;
    private readonly string _workingDirectory;

    public GameFlowE2ETests()
    {
        _tempDirManager = new TempDirectoryManager();
        _workingDirectory = _tempDirManager.CreateTempDirectory("e2e-flow-test");
        _gitExecutor = new GitCommandExecutor();
        _roomRepository = new RoomRepository(_gitExecutor);
        _startGameUseCase = new StartGameUseCase(_roomRepository);
        _gameEngine = new GameEngine(_gitExecutor);
    }

    [Fact]
    public async Task GameFlow_Room1_CompleteWithGitInit_ShouldShowSuccessAndAllowMovement()
    {
        // Arrange
        var console = new TestConsole();
        var renderer = new GameRenderer(console);

        var startResult = await _startGameUseCase.ExecuteAsync("E2EPlayer");
        startResult.Success.Should().BeTrue();
        startResult.Game.Should().NotBeNull();

        _gameEngine.StartGame(startResult.Game!, _workingDirectory);

        // Act - Render initial room
        renderer.RenderRoom(startResult.Game!.CurrentRoom, startResult.Game.Player);

        // Assert - Room 1 content displayed
        var output = console.Output;
        output.Should().Contain("Initialization Chamber");
        output.Should().Contain("git init");

        // Act - Complete challenge with git init
        var gitInitResult = await _gameEngine.ProcessCommandAsync("git init");
        renderer.RenderCommandResult(gitInitResult);

        // Assert - Challenge completed
        var outputAfterInit = console.Output;
        outputAfterInit.Should().Contain("Challenge completed");

        // Act - Move forward
        var moveResult = await _gameEngine.ProcessCommandAsync("forward");
        renderer.RenderCommandResult(moveResult);

        // Assert - Movement successful
        moveResult.Success.Should().BeTrue();

        // Act - Render new room
        renderer.RenderRoom(startResult.Game!.CurrentRoom, startResult.Game.Player);

        // Assert - Now in Room 2
        var outputRoom2 = console.Output;
        outputRoom2.Should().Contain("Staging Area");
    }

    [Fact]
    public async Task GameFlow_CannotMoveWithoutCompletingChallenge()
    {
        // Arrange
        var console = new TestConsole();
        var renderer = new GameRenderer(console);

        var startResult = await _startGameUseCase.ExecuteAsync("E2EPlayer");
        _gameEngine.StartGame(startResult.Game!, _workingDirectory);

        // Act - Try to move without completing challenge
        var moveResult = await _gameEngine.ProcessCommandAsync("forward");
        renderer.RenderCommandResult(moveResult);

        // Assert
        moveResult.Success.Should().BeFalse();
        console.Output.Should().Contain("must complete");
    }

    [Fact]
    public async Task GameFlow_HelpCommand_ShouldDisplayAllCommands()
    {
        // Arrange
        var console = new TestConsole();
        var renderer = new GameRenderer(console);

        var startResult = await _startGameUseCase.ExecuteAsync("E2EPlayer");
        _gameEngine.StartGame(startResult.Game!, _workingDirectory);

        // Act
        var helpResult = await _gameEngine.ProcessCommandAsync("help");
        renderer.RenderCommandResult(helpResult);

        // Assert
        var output = console.Output;
        output.Should().Contain("help");
        output.Should().Contain("look");
        output.Should().Contain("status");
        output.Should().Contain("git");
        output.Should().Contain("forward");
    }

    [Fact]
    public async Task GameFlow_StatusCommand_ShouldDisplayPlayerProgress()
    {
        // Arrange
        var console = new TestConsole();
        var renderer = new GameRenderer(console);

        var startResult = await _startGameUseCase.ExecuteAsync("TestHero");
        _gameEngine.StartGame(startResult.Game!, _workingDirectory);

        // Act
        var statusResult = await _gameEngine.ProcessCommandAsync("status");
        renderer.RenderCommandResult(statusResult);

        // Assert
        var output = console.Output;
        output.Should().Contain("TestHero");
        output.Should().Contain("Initialization Chamber");
    }

    [Fact]
    public async Task GameFlow_LookCommand_ShouldDisplayCurrentRoomDetails()
    {
        // Arrange
        var console = new TestConsole();
        var renderer = new GameRenderer(console);

        var startResult = await _startGameUseCase.ExecuteAsync("E2EPlayer");
        _gameEngine.StartGame(startResult.Game!, _workingDirectory);

        // Act
        var lookResult = await _gameEngine.ProcessCommandAsync("look");
        renderer.RenderCommandResult(lookResult);

        // Assert
        var output = console.Output;
        output.Should().Contain("Initialization Chamber");
    }

    [Fact]
    public async Task GameFlow_InvalidCommand_ShouldShowErrorMessage()
    {
        // Arrange
        var console = new TestConsole();
        var renderer = new GameRenderer(console);

        var startResult = await _startGameUseCase.ExecuteAsync("E2EPlayer");
        _gameEngine.StartGame(startResult.Game!, _workingDirectory);

        // Act
        var invalidResult = await _gameEngine.ProcessCommandAsync("dance");
        renderer.RenderCommandResult(invalidResult);

        // Assert
        invalidResult.Success.Should().BeFalse();
        console.Output.Should().Contain("Unknown command");
    }

    [Fact]
    public async Task GameFlow_HintCommand_ShouldProvideGuidance()
    {
        // Arrange
        var console = new TestConsole();
        var renderer = new GameRenderer(console);

        var startResult = await _startGameUseCase.ExecuteAsync("E2EPlayer");
        _gameEngine.StartGame(startResult.Game!, _workingDirectory);

        // Act
        var hintResult = await _gameEngine.ProcessCommandAsync("hint");
        renderer.RenderCommandResult(hintResult);

        // Assert
        var output = console.Output;
        output.Should().Contain("Hint");
    }

    [Fact]
    public async Task GameFlow_StartAtSpecificRoom_ShouldBeginAtThatRoom()
    {
        // Arrange
        var console = new TestConsole();
        var renderer = new GameRenderer(console);

        // Start at room 5 (The Branch Junction)
        var startResult = await _startGameUseCase.ExecuteAsync("E2EPlayer", "room-5");
        startResult.Success.Should().BeTrue();
        startResult.Game.Should().NotBeNull();
        startResult.Message.Should().Contain("Debug mode");

        _gameEngine.StartGame(startResult.Game!, _workingDirectory);

        // Act
        renderer.RenderRoom(startResult.Game!.CurrentRoom, startResult.Game.Player);

        // Assert
        var output = console.Output;
        output.Should().Contain("Branch Junction");
        output.Should().Contain("feature-branch");
    }

    [Fact]
    public async Task GameFlow_StartAtInvalidRoom_ShouldReturnError()
    {
        // Act
        var startResult = await _startGameUseCase.ExecuteAsync("E2EPlayer", "room-999");

        // Assert
        startResult.Success.Should().BeFalse();
        startResult.Message.Should().Contain("not found");
        startResult.Message.Should().Contain("Available rooms");
    }

    [Fact]
    public async Task GameFlow_Room8_QuizChallenge_ShouldWorkWithAnswer()
    {
        // Arrange
        var console = new TestConsole();
        var renderer = new GameRenderer(console);

        // Start at quiz room
        var startResult = await _startGameUseCase.ExecuteAsync("E2EPlayer", "room-8");
        startResult.Success.Should().BeTrue();

        _gameEngine.StartGame(startResult.Game!, _workingDirectory);

        // Act - Render the quiz room
        renderer.RenderRoom(startResult.Game!.CurrentRoom, startResult.Game.Player);

        // Assert - Quiz content displayed
        var output = console.Output;
        output.Should().Contain("Quiz");

        // Act - Submit correct answer (1 = git add .)
        var answerResult = await _gameEngine.ProcessCommandAsync("answer 1");
        renderer.RenderCommandResult(answerResult);

        // Assert - Correct answer acknowledged
        var outputAfterAnswer = console.Output;
        outputAfterAnswer.Should().Contain("Correct");
    }

    [Fact]
    public async Task GameFlow_ProgressTracking_ShouldRecordCompletedRooms()
    {
        // Arrange
        var startResult = await _startGameUseCase.ExecuteAsync("E2EPlayer");
        _gameEngine.StartGame(startResult.Game!, _workingDirectory);

        // Act - Complete Room 1
        await _gameEngine.ProcessCommandAsync("git init");

        // Assert
        startResult.Game!.Player.CompletedChallenges.Should().Contain("init-chamber-challenge");

        // Act - Move to Room 2
        await _gameEngine.ProcessCommandAsync("forward");

        // Assert
        startResult.Game.Player.CompletedRooms.Should().Contain("room-2");
        startResult.Game.Player.MoveCount.Should().Be(1);
    }

    [Fact]
    public async Task GameFlow_ExitCommand_ShouldReturnExitType()
    {
        // Arrange
        var startResult = await _startGameUseCase.ExecuteAsync("E2EPlayer");
        _gameEngine.StartGame(startResult.Game!, _workingDirectory);

        // Act
        var exitResult = await _gameEngine.ProcessCommandAsync("exit");

        // Assert
        exitResult.Type.Should().Be(CommandType.Exit);
        exitResult.Success.Should().BeTrue();
    }

    public void Dispose()
    {
        _tempDirManager.Dispose();
    }
}
