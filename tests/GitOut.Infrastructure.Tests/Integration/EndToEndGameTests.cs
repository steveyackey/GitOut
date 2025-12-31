using FluentAssertions;
using GitOut.Application.Services;
using GitOut.Application.UseCases;
using GitOut.Infrastructure.FileSystem;
using GitOut.Infrastructure.Git;
using GitOut.Infrastructure.Persistence;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration;

public class EndToEndGameTests : IDisposable
{
    private readonly TempDirectoryManager _tempDirManager;
    private readonly string _workingDirectory;
    private readonly GitCommandExecutor _gitExecutor;
    private readonly RoomRepository _roomRepository;
    private readonly StartGameUseCase _startGameUseCase;
    private readonly GameEngine _gameEngine;

    public EndToEndGameTests()
    {
        _tempDirManager = new TempDirectoryManager();
        _workingDirectory = _tempDirManager.CreateTempDirectory("e2e-test");
        _gitExecutor = new GitCommandExecutor();
        _roomRepository = new RoomRepository(_gitExecutor);
        _startGameUseCase = new StartGameUseCase(_roomRepository);
        _gameEngine = new GameEngine(_gitExecutor);
    }

    [Fact]
    public async Task CompleteGame_ShouldProgressThroughFirstTwoRooms()
    {
        // Start the game
        var startResult = await _startGameUseCase.ExecuteAsync("TestPlayer");
        startResult.Success.Should().BeTrue();
        startResult.Game.Should().NotBeNull();

        _gameEngine.StartGame(startResult.Game!, _workingDirectory);

        // Room 1: The Initialization Chamber
        var state = _gameEngine.GetCurrentState();
        state.CurrentRoomName.Should().Be("The Initialization Chamber");
        state.IsActive.Should().BeTrue();

        // Try to move without completing challenge - should fail
        var moveResult = await _gameEngine.ProcessCommandAsync("forward");
        moveResult.Success.Should().BeFalse();
        moveResult.Message.Should().Contain("must complete the current room's challenge");

        // Complete Room 1 challenge: git init
        var gitInitResult = await _gameEngine.ProcessCommandAsync("git init");
        gitInitResult.Success.Should().BeTrue();
        gitInitResult.Message.Should().Contain("Challenge completed");

        // Now we can move forward
        moveResult = await _gameEngine.ProcessCommandAsync("forward");
        moveResult.Success.Should().BeTrue();
        state = _gameEngine.GetCurrentState();
        state.CurrentRoomName.Should().Be("The Staging Area");

        // Room 2: The Staging Area
        // README.md should be created by the challenge setup
        _tempDirManager.FileExists(_workingDirectory, "README.md").Should().BeTrue();

        // Configure git for commit
        await _gitExecutor.ExecuteAsync("config user.email \"test@example.com\"", _workingDirectory);
        await _gitExecutor.ExecuteAsync("config user.name \"Test User\"", _workingDirectory);

        // Stage the file
        var gitAddResult = await _gameEngine.ProcessCommandAsync("git add README.md");
        gitAddResult.Success.Should().BeTrue();

        // Commit the file
        var gitCommitResult = await _gameEngine.ProcessCommandAsync("git commit -m \"Initial commit\"");
        gitCommitResult.Success.Should().BeTrue();
        gitCommitResult.Message.Should().Contain("Challenge completed");

        // Game should still be active (not complete yet - more rooms to go)
        state = _gameEngine.GetCurrentState();
        state.IsActive.Should().BeTrue();
        state.IsCompleted.Should().BeFalse();

        // Should be able to move to room 3
        state.CurrentRoomName.Should().Be("The Staging Area");
        var room2 = startResult.Game!.CurrentRoom;
        room2.Exits.Should().ContainKey("forward");

        // Verify player progress
        startResult.Game!.Player.CompletedRooms.Should().Contain("room-2");
        startResult.Game.Player.CompletedChallenges.Should().HaveCount(2);
        startResult.Game.Player.MoveCount.Should().Be(1);
    }

    [Fact]
    public async Task HelpCommand_ShouldShowAvailableCommands()
    {
        // Arrange
        var startResult = await _startGameUseCase.ExecuteAsync("TestPlayer");
        _gameEngine.StartGame(startResult.Game!, _workingDirectory);

        // Act
        var result = await _gameEngine.ProcessCommandAsync("help");

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Available Commands");
        result.Message.Should().Contain("git");
        result.Type.Should().Be(CommandType.Help);
    }

    [Fact]
    public async Task StatusCommand_ShouldShowGameProgress()
    {
        // Arrange
        var startResult = await _startGameUseCase.ExecuteAsync("TestPlayer");
        _gameEngine.StartGame(startResult.Game!, _workingDirectory);

        // Act
        var result = await _gameEngine.ProcessCommandAsync("status");

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("TestPlayer");
        result.Message.Should().Contain("The Initialization Chamber");
        result.Type.Should().Be(CommandType.Status);
    }

    [Fact]
    public async Task LookCommand_ShouldShowRoomDetails()
    {
        // Arrange
        var startResult = await _startGameUseCase.ExecuteAsync("TestPlayer");
        _gameEngine.StartGame(startResult.Game!, _workingDirectory);

        // Act
        var result = await _gameEngine.ProcessCommandAsync("look");

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("The Initialization Chamber");
        result.Type.Should().Be(CommandType.Look);
    }

    [Fact]
    public async Task InvalidCommand_ShouldReturnError()
    {
        // Arrange
        var startResult = await _startGameUseCase.ExecuteAsync("TestPlayer");
        _gameEngine.StartGame(startResult.Game!, _workingDirectory);

        // Act
        var result = await _gameEngine.ProcessCommandAsync("invalid-command");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Unknown command");
    }

    [Fact]
    public async Task ExitCommand_ShouldReturnExitCommandType()
    {
        // Arrange
        var startResult = await _startGameUseCase.ExecuteAsync("TestPlayer");
        _gameEngine.StartGame(startResult.Game!, _workingDirectory);

        // Act
        var result = await _gameEngine.ProcessCommandAsync("exit");

        // Assert
        result.Success.Should().BeTrue();
        result.Type.Should().Be(CommandType.Exit);
        result.Message.Should().Be("EXIT_REQUESTED");
    }

    [Fact]
    public async Task HelpCommand_ShouldIncludeExitCommand()
    {
        // Arrange
        var startResult = await _startGameUseCase.ExecuteAsync("TestPlayer");
        _gameEngine.StartGame(startResult.Game!, _workingDirectory);

        // Act
        var result = await _gameEngine.ProcessCommandAsync("help");

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("exit");
        result.Message.Should().Contain("Exit the game (with option to save)");
    }

    public void Dispose()
    {
        _tempDirManager.Dispose();
    }
}
