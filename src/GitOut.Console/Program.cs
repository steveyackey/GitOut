using GitOut.Application.Interfaces;
using GitOut.Application.Services;
using GitOut.Application.UseCases;
using GitOut.Console.Input;
using GitOut.Console.UI;
using GitOut.Domain.Interfaces;
using GitOut.Infrastructure.FileSystem;
using GitOut.Infrastructure.Git;
using GitOut.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

// Setup Dependency Injection
var services = new ServiceCollection();

// Register infrastructure services
services.AddSingleton<GitOut.Domain.Interfaces.IGitCommandExecutor, GitCommandExecutor>();
services.AddSingleton<IRoomRepository, RoomRepository>();
services.AddSingleton<IProgressRepository, JsonProgressRepository>();
services.AddSingleton<TempDirectoryManager>();

// Register application services
services.AddSingleton<GameEngine>();
services.AddSingleton<StartGameUseCase>();
services.AddSingleton<SaveProgressUseCase>();
services.AddSingleton<LoadProgressUseCase>();

// Register console services
services.AddSingleton<GameRenderer>();
services.AddSingleton<CommandHandler>();

var serviceProvider = services.BuildServiceProvider();

// Get services
var gameEngine = serviceProvider.GetRequiredService<GameEngine>();
var startGameUseCase = serviceProvider.GetRequiredService<StartGameUseCase>();
var loadProgressUseCase = serviceProvider.GetRequiredService<LoadProgressUseCase>();
var saveProgressUseCase = serviceProvider.GetRequiredService<SaveProgressUseCase>();
var progressRepository = serviceProvider.GetRequiredService<IProgressRepository>();
var renderer = serviceProvider.GetRequiredService<GameRenderer>();
var commandHandler = serviceProvider.GetRequiredService<CommandHandler>();
var tempDirManager = serviceProvider.GetRequiredService<TempDirectoryManager>();

try
{
    // Render welcome screen
    renderer.RenderWelcome();

    // Check if saved game exists
    var hasSavedGame = await progressRepository.HasSavedProgressAsync();
    GitOut.Domain.Entities.Game? game = null;
    string? workingDirectory = null;

    if (hasSavedGame)
    {
        // Ask if player wants to load saved game
        var loadSaved = AnsiConsole.Confirm("A saved game was found. Do you want to load it?");

        if (loadSaved)
        {
            var loadResult = await loadProgressUseCase.ExecuteAsync();

            if (loadResult.Success && loadResult.Game != null)
            {
                game = loadResult.Game;
                AnsiConsole.MarkupLine($"[green]{loadResult.Message}[/]");
                AnsiConsole.WriteLine();
            }
            else
            {
                renderer.RenderError($"Failed to load saved game: {loadResult.Message}");
                AnsiConsole.MarkupLine("[yellow]Starting a new game instead...[/]");
                AnsiConsole.WriteLine();
            }
        }
        else
        {
            // Ask if they want to delete the old save
            var deleteSave = AnsiConsole.Confirm("Do you want to delete the existing saved game?");
            if (deleteSave)
            {
                await progressRepository.DeleteProgressAsync();
                AnsiConsole.MarkupLine("[yellow]Saved game deleted.[/]");
                AnsiConsole.WriteLine();
            }
        }
    }

    // If no game was loaded, start a new one
    if (game == null)
    {
        // Get player name
        var playerName = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter your name (or press Enter for 'Adventurer'): ")
                .AllowEmpty());

        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "Adventurer";
        }

        // Start the game
        var startResult = await startGameUseCase.ExecuteAsync(playerName);

        if (!startResult.Success || startResult.Game == null)
        {
            renderer.RenderError(startResult.Message);
            return;
        }

        game = startResult.Game;
        AnsiConsole.MarkupLine($"[green]{startResult.Message}[/]");
        AnsiConsole.WriteLine();
    }

    // Create working directory for challenges
    workingDirectory = tempDirManager.CreateTempDirectory("game");

    // Initialize game engine
    gameEngine.StartGame(game, workingDirectory);

    // Setup save progress handler
    gameEngine.SetSaveProgressHandler(async (currentGame) =>
    {
        return await saveProgressUseCase.ExecuteAsync(currentGame);
    });

    // Setup the current room's challenge
    if (game.CurrentRoom.Challenge != null)
    {
        await game.CurrentRoom.Challenge.SetupAsync(workingDirectory);
    }

    // Display working directory
    renderer.RenderWorkingDirectory(workingDirectory);

    // Render initial room
    renderer.RenderRoom(game.CurrentRoom, game.Player);

    // Game loop
    var isRunning = true;

    while (isRunning && game.IsActive)
    {
        // Get user input
        var input = renderer.GetPrompt();

        if (string.IsNullOrWhiteSpace(input))
        {
            continue;
        }

        // Parse command
        var parsedCommand = commandHandler.ParseCommand(input);

        // Check for exit command
        if (commandHandler.IsExitCommand(parsedCommand))
        {
            var confirm = AnsiConsole.Confirm("Are you sure you want to exit?");
            if (confirm)
            {
                isRunning = false;
                AnsiConsole.MarkupLine("[yellow]Thanks for playing GitOut![/]");
                break;
            }
            continue;
        }

        // Execute command through game engine
        var result = await gameEngine.ProcessCommandAsync(parsedCommand.Command);

        // Handle exit command
        if (result.Type == GitOut.Application.Services.CommandType.Exit && result.Success)
        {
            var shouldSave = AnsiConsole.Confirm("Would you like to save your progress before exiting?");
            if (shouldSave)
            {
                var saveResult = await saveProgressUseCase.ExecuteAsync(game);
                if (saveResult.Success)
                {
                    AnsiConsole.MarkupLine($"[green]{saveResult.Message}[/]");
                }
                else
                {
                    renderer.RenderError($"Failed to save: {saveResult.Message}");
                }
            }
            isRunning = false;
            AnsiConsole.MarkupLine("[yellow]Thanks for playing GitOut![/]");
            continue;
        }

        // Render result
        renderer.RenderCommandResult(result);

        // If movement was successful, render new room
        if (result.Type == GitOut.Application.Services.CommandType.Movement && result.Success)
        {
            var gameState = gameEngine.GetCurrentState();
            if (gameState.IsActive)
            {
                renderer.RenderRoom(game.CurrentRoom, game.Player);
            }
        }

        // Check if game is complete
        if (!game.IsActive)
        {
            renderer.RenderGameComplete(game.Player);
            isRunning = false;
        }
    }
}
catch (Exception ex)
{
    renderer.RenderError($"An unexpected error occurred: {ex.Message}");
    AnsiConsole.WriteException(ex);
}
finally
{
    // Cleanup temporary directories
    tempDirManager.Dispose();
}
