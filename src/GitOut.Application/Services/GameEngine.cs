using GitOut.Application.Interfaces;
using GitOut.Application.UseCases;
using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;

namespace GitOut.Application.Services;

/// <summary>
/// Main game engine that orchestrates game state and commands
/// </summary>
public class GameEngine
{
    private readonly Domain.Interfaces.IGitCommandExecutor _gitExecutor;
    private Game? _currentGame;
    private string? _workingDirectory;
    private Func<Game, Task<SaveProgressResult>>? _saveProgressHandler;

    public GameEngine(Domain.Interfaces.IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public void SetSaveProgressHandler(Func<Game, Task<SaveProgressResult>> handler)
    {
        _saveProgressHandler = handler;
    }

    public Game? CurrentGame => _currentGame;
    public string? WorkingDirectory => _workingDirectory;

    public void StartGame(Game game, string workingDirectory)
    {
        if (game == null)
        {
            throw new ArgumentNullException(nameof(game));
        }

        if (string.IsNullOrWhiteSpace(workingDirectory))
        {
            throw new ArgumentException("Working directory cannot be empty", nameof(workingDirectory));
        }

        if (!Directory.Exists(workingDirectory))
        {
            throw new DirectoryNotFoundException($"Working directory not found: {workingDirectory}");
        }

        _currentGame = game;
        _workingDirectory = workingDirectory;
    }

    public async Task<CommandResult> ProcessCommandAsync(string command)
    {
        if (_currentGame == null || _workingDirectory == null)
        {
            return new CommandResult(
                false,
                "No active game. Please start a game first.",
                CommandType.Unknown
            );
        }

        if (string.IsNullOrWhiteSpace(command))
        {
            return new CommandResult(
                false,
                "Command cannot be empty.",
                CommandType.Unknown
            );
        }

        command = command.Trim();

        // Handle game commands
        if (command.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return HandleHelp();
        }

        if (command.Equals("status", StringComparison.OrdinalIgnoreCase))
        {
            return await HandleStatusAsync();
        }

        if (command.Equals("look", StringComparison.OrdinalIgnoreCase) ||
            command.Equals("examine", StringComparison.OrdinalIgnoreCase))
        {
            return HandleLook();
        }

        // Handle movement commands
        if (command.StartsWith("go ", StringComparison.OrdinalIgnoreCase) ||
            command.Equals("forward", StringComparison.OrdinalIgnoreCase) ||
            command.Equals("back", StringComparison.OrdinalIgnoreCase))
        {
            return await HandleMovementAsync(command);
        }

        // Handle answer commands for quiz challenges
        if (command.StartsWith("answer ", StringComparison.OrdinalIgnoreCase))
        {
            return await HandleAnswerAsync(command);
        }

        // Handle hint command
        if (command.Equals("hint", StringComparison.OrdinalIgnoreCase))
        {
            return HandleHint();
        }

        // Handle save command
        if (command.Equals("save", StringComparison.OrdinalIgnoreCase))
        {
            return await HandleSaveAsync();
        }

        // Handle exit command
        if (command.Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            return HandleExit();
        }

        // Handle git commands
        if (command.StartsWith("git ", StringComparison.OrdinalIgnoreCase))
        {
            return await HandleGitCommandAsync(command);
        }

        return new CommandResult(
            false,
            $"Unknown command: {command}. Type 'help' for available commands.",
            CommandType.Unknown
        );
    }

    private CommandResult HandleHelp()
    {
        var helpText = """
            Available Commands:

            Game Commands:
              help              - Show this help message
              status            - Show game progress and current room info
              look/examine      - Examine the current room
              go <direction>    - Move in a direction (e.g., 'go forward')
              forward/back      - Quick movement commands
              answer <number>   - Answer a quiz question (e.g., 'answer 1')
              hint              - Get a hint for the current challenge
              save              - Save your game progress
              exit              - Exit the game (with option to save)

            Git Commands:
              git <command>     - Execute any git command (e.g., 'git init', 'git status')

            Tips:
              - Read the room narrative carefully for clues
              - Use 'git status' to check your repository state
              - Complete the challenge to unlock exits
            """;

        return new CommandResult(true, helpText, CommandType.Help);
    }

    private async Task<CommandResult> HandleStatusAsync()
    {
        if (_currentGame == null)
        {
            return new CommandResult(false, "No active game.", CommandType.Status);
        }

        var status = $"""
            Game Status:
            ------------
            Player: {_currentGame.Player.Name}
            Current Room: {_currentGame.CurrentRoom.Name}
            Rooms Completed: {_currentGame.Player.CompletedRooms.Count}/{_currentGame.Rooms.Count}
            Challenges Completed: {_currentGame.Player.CompletedChallenges.Count}
            Moves: {_currentGame.Player.MoveCount}

            Room Info:
            {_currentGame.CurrentRoom.Description}

            """;

        if (_currentGame.CurrentRoom.Challenge != null)
        {
            var isCompleted = _currentGame.Player.HasCompletedChallenge(_currentGame.CurrentRoom.Challenge.Id);
            status += $"Challenge: {(isCompleted ? "✓ Completed" : "○ In Progress")}\n";
        }

        if (_currentGame.CurrentRoom.Exits.Count > 0)
        {
            status += $"\nAvailable Exits: {string.Join(", ", _currentGame.CurrentRoom.Exits.Keys)}";
        }

        return new CommandResult(true, status, CommandType.Status);
    }

    private CommandResult HandleLook()
    {
        if (_currentGame == null)
        {
            return new CommandResult(false, "No active game.", CommandType.Look);
        }

        var room = _currentGame.CurrentRoom;
        var description = $"""
            {room.Name}
            {new string('=', room.Name.Length)}

            {room.Narrative}

            """;

        if (room.Challenge != null)
        {
            var isCompleted = _currentGame.Player.HasCompletedChallenge(room.Challenge.Id);
            description += $"\nChallenge: {room.Challenge.Description}";
            description += $"\nStatus: {(isCompleted ? "✓ Completed" : "○ Not completed")}";
        }

        if (room.Exits.Count > 0)
        {
            description += $"\n\nExits: {string.Join(", ", room.Exits.Keys)}";
        }

        return new CommandResult(true, description, CommandType.Look);
    }

    private async Task<CommandResult> HandleMovementAsync(string command)
    {
        if (_currentGame == null)
        {
            return new CommandResult(false, "No active game.", CommandType.Movement);
        }

        string direction;
        if (command.StartsWith("go ", StringComparison.OrdinalIgnoreCase))
        {
            direction = command.Substring(3).Trim().ToLower();
        }
        else
        {
            direction = command.ToLower();
        }

        // Check if the challenge is completed
        if (_currentGame.CurrentRoom.Challenge != null &&
            !_currentGame.Player.HasCompletedChallenge(_currentGame.CurrentRoom.Challenge.Id))
        {
            return new CommandResult(
                false,
                "You must complete the current room's challenge before you can leave!",
                CommandType.Movement
            );
        }

        if (!_currentGame.CanExitInDirection(direction))
        {
            return new CommandResult(
                false,
                $"You cannot go '{direction}' from here. Available exits: {string.Join(", ", _currentGame.CurrentRoom.Exits.Keys)}",
                CommandType.Movement
            );
        }

        var nextRoomId = _currentGame.GetRoomIdInDirection(direction);
        if (nextRoomId == null)
        {
            return new CommandResult(false, "Invalid exit.", CommandType.Movement);
        }

        var moved = _currentGame.MoveToRoom(nextRoomId);
        if (!moved)
        {
            return new CommandResult(false, "Failed to move to the next room.", CommandType.Movement);
        }

        // Setup the new room's challenge
        if (_currentGame.CurrentRoom.Challenge != null && _workingDirectory != null)
        {
            await _currentGame.CurrentRoom.Challenge.SetupAsync(_workingDirectory);
        }

        var message = $"You move {direction}...";

        if (_currentGame.CurrentRoom.IsEndRoom)
        {
            message += "\n\n*** CONGRATULATIONS! You've completed the game! ***";
        }

        return new CommandResult(true, message, CommandType.Movement);
    }

    private async Task<CommandResult> HandleGitCommandAsync(string command)
    {
        if (_currentGame == null || _workingDirectory == null)
        {
            return new CommandResult(false, "No active game.", CommandType.Git);
        }

        // Extract the git command (remove "git " prefix)
        var gitCommand = command.Substring(4).Trim();

        // Execute the git command
        var result = await _gitExecutor.ExecuteAsync(gitCommand, _workingDirectory);

        var output = string.IsNullOrWhiteSpace(result.Output) ? result.Error : result.Output;

        // Check if the challenge is now complete
        if (_currentGame.CurrentRoom.Challenge != null &&
            !_currentGame.Player.HasCompletedChallenge(_currentGame.CurrentRoom.Challenge.Id))
        {
            var validationResult = await _currentGame.CurrentRoom.Challenge.ValidateAsync(_workingDirectory);

            if (validationResult.IsSuccessful)
            {
                _currentGame.CompleteCurrentChallenge();
                output += $"\n\n✓ Challenge completed! {validationResult.Message}";

                if (_currentGame.CurrentRoom.Exits.Count > 0)
                {
                    output += $"\nYou can now exit: {string.Join(", ", _currentGame.CurrentRoom.Exits.Keys)}";
                }
            }
        }

        return new CommandResult(
            result.Success,
            output,
            CommandType.Git,
            gitCommand
        );
    }

    private async Task<CommandResult> HandleAnswerAsync(string command)
    {
        if (_currentGame == null || _workingDirectory == null)
        {
            return new CommandResult(false, "No active game.", CommandType.Answer);
        }

        // Check if current challenge is a quiz
        if (_currentGame.CurrentRoom.Challenge is not QuizChallenge quizChallenge)
        {
            return new CommandResult(
                false,
                "This command only works in quiz challenges.",
                CommandType.Answer
            );
        }

        // Parse the answer number
        var answerText = command.Substring(7).Trim();
        if (!int.TryParse(answerText, out var answerIndex) || answerIndex < 1)
        {
            return new CommandResult(
                false,
                "Please provide a valid answer number (e.g., 'answer 1' for option 1).",
                CommandType.Answer
            );
        }

        // Convert from 1-based to 0-based index
        answerIndex--;

        // Submit the answer
        try
        {
            quizChallenge.SubmitAnswer(answerIndex);
        }
        catch (ArgumentOutOfRangeException)
        {
            return new CommandResult(
                false,
                $"Invalid answer. Please choose a number between 1 and {quizChallenge.Options.Count}.",
                CommandType.Answer
            );
        }

        // Validate the answer
        var validationResult = await quizChallenge.ValidateAsync(_workingDirectory);

        if (validationResult.IsSuccessful)
        {
            _currentGame.CompleteCurrentChallenge();

            var message = $"{validationResult.Message}\n\nChallenge completed!";
            if (_currentGame.CurrentRoom.Exits.Count > 0)
            {
                message += $"\nYou can now exit: {string.Join(", ", _currentGame.CurrentRoom.Exits.Keys)}";
            }

            return new CommandResult(true, message, CommandType.Answer);
        }

        return new CommandResult(false, validationResult.Message, CommandType.Answer);
    }

    private CommandResult HandleHint()
    {
        if (_currentGame == null)
        {
            return new CommandResult(false, "No active game.", CommandType.Hint);
        }

        var challenge = _currentGame.CurrentRoom.Challenge;
        if (challenge == null)
        {
            return new CommandResult(
                false,
                "There is no active challenge in this room.",
                CommandType.Hint
            );
        }

        // Check if player has already completed the challenge
        if (_currentGame.Player.HasCompletedChallenge(challenge.Id))
        {
            return new CommandResult(
                false,
                "You've already completed this challenge!",
                CommandType.Hint
            );
        }

        // Get hint based on challenge type
        string? hint = challenge switch
        {
            QuizChallenge quiz => quiz.Hint ?? "Read the question carefully and think about what each command does.",
            RepositoryChallenge => "Read the room narrative carefully for clues about what git commands to use.",
            ScenarioChallenge => "Consider what git commands would help in this scenario.",
            _ => "Try using 'look' to examine the room again."
        };

        return new CommandResult(
            true,
            $"Hint: {hint}",
            CommandType.Hint
        );
    }

    private async Task<CommandResult> HandleSaveAsync()
    {
        if (_currentGame == null)
        {
            return new CommandResult(false, "No active game to save.", CommandType.Save);
        }

        if (_saveProgressHandler == null)
        {
            return new CommandResult(
                false,
                "Save functionality is not available.",
                CommandType.Save
            );
        }

        try
        {
            var result = await _saveProgressHandler(_currentGame);
            return new CommandResult(
                result.Success,
                result.Message,
                CommandType.Save
            );
        }
        catch (Exception ex)
        {
            return new CommandResult(
                false,
                $"Failed to save game: {ex.Message}",
                CommandType.Save
            );
        }
    }

    private CommandResult HandleExit()
    {
        return new CommandResult(
            true,
            "EXIT_REQUESTED",
            CommandType.Exit
        );
    }

    public GameState GetCurrentState()
    {
        if (_currentGame == null)
        {
            return new GameState(
                IsActive: false,
                CurrentRoomId: null,
                CurrentRoomName: null,
                IsCompleted: false,
                PlayerName: null,
                CompletedRoomsCount: 0,
                TotalRoomsCount: 0
            );
        }

        return new GameState(
            IsActive: _currentGame.IsActive,
            CurrentRoomId: _currentGame.CurrentRoom.Id,
            CurrentRoomName: _currentGame.CurrentRoom.Name,
            IsCompleted: !_currentGame.IsActive,
            PlayerName: _currentGame.Player.Name,
            CompletedRoomsCount: _currentGame.Player.CompletedRooms.Count,
            TotalRoomsCount: _currentGame.Rooms.Count
        );
    }
}

public record CommandResult(
    bool Success,
    string Message,
    CommandType Type,
    string? GitCommand = null
);

public enum CommandType
{
    Unknown,
    Help,
    Status,
    Look,
    Movement,
    Git,
    Answer,
    Hint,
    Save,
    Exit
}

public record GameState(
    bool IsActive,
    string? CurrentRoomId,
    string? CurrentRoomName,
    bool IsCompleted,
    string? PlayerName,
    int CompletedRoomsCount,
    int TotalRoomsCount
);
