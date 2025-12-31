using GitOut.Application.Services;
using Spectre.Console;

namespace GitOut.Console.UI;

/// <summary>
/// Displays game progress and available commands
/// </summary>
public class ProgressDisplay
{
    /// <summary>
    /// Render progress information for the current game state
    /// </summary>
    public void RenderProgress(GameState gameState)
    {
        if (!gameState.IsActive)
        {
            return;
        }

        var completionPercentage = gameState.TotalRoomsCount > 0
            ? (int)((double)gameState.CompletedRoomsCount / gameState.TotalRoomsCount * 100)
            : 0;

        var panel = new Panel(
            new Markup($"""
                [bold]Player:[/] {gameState.PlayerName}
                [bold]Current Room:[/] {gameState.CurrentRoomName}
                [bold]Progress:[/] {gameState.CompletedRoomsCount}/{gameState.TotalRoomsCount} rooms completed ([green]{completionPercentage}%[/])
                """)
        )
        {
            Header = new PanelHeader("[yellow]Game Progress[/]", Justify.Center),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Yellow)
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Render available commands in a formatted table
    /// </summary>
    public void RenderAvailableCommands()
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Blue)
            .AddColumn(new TableColumn("[bold]Category[/]").Centered())
            .AddColumn(new TableColumn("[bold]Command[/]"))
            .AddColumn(new TableColumn("[bold]Description[/]"));

        // Game commands
        table.AddRow("[cyan]Game[/]", "help", "Show help message");
        table.AddRow("[cyan]Game[/]", "status", "Show game progress");
        table.AddRow("[cyan]Game[/]", "look", "Examine current room");
        table.AddRow("[cyan]Game[/]", "hint", "Get a hint");

        // Movement commands
        table.AddRow("[green]Movement[/]", "forward", "Move forward");
        table.AddRow("[green]Movement[/]", "go <direction>", "Move in a direction");

        // Quiz commands
        table.AddRow("[yellow]Quiz[/]", "answer <number>", "Answer quiz question");

        // Git commands
        table.AddRow("[magenta]Git[/]", "git <command>", "Execute git command");

        var panel = new Panel(table)
        {
            Header = new PanelHeader("[blue]Available Commands[/]", Justify.Center),
            Border = BoxBorder.Double
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Render a simple ASCII map showing room progression
    /// </summary>
    public void RenderMap(GameState gameState, int completedRooms, int totalRooms)
    {
        var mapBuilder = new System.Text.StringBuilder();
        mapBuilder.AppendLine("Map:");
        mapBuilder.AppendLine();

        // Simple linear map representation
        for (int i = 1; i <= totalRooms; i++)
        {
            if (i == completedRooms + 1)
            {
                // Current room
                mapBuilder.Append("[yellow][@][/]");
            }
            else if (i <= completedRooms)
            {
                // Completed room
                mapBuilder.Append("[green][X][/]");
            }
            else
            {
                // Locked room
                mapBuilder.Append("[dim][?][/]");
            }

            // Add connector except for last room
            if (i < totalRooms)
            {
                mapBuilder.Append("---");
            }
        }

        var panel = new Panel(new Markup(mapBuilder.ToString()))
        {
            Header = new PanelHeader("[cyan]Dungeon Map[/]", Justify.Center),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Cyan)
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();

        // Legend
        AnsiConsole.MarkupLine("[dim]Legend: [yellow][@][/] Current  [green][X][/] Completed  [dim][?][/] Locked[/]");
        AnsiConsole.WriteLine();
    }
}
