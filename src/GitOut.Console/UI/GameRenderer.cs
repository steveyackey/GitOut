using GitOut.Application.Services;
using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using Spectre.Console;

namespace GitOut.Console.UI;

/// <summary>
/// Renders game content using Spectre.Console
/// </summary>
public class GameRenderer
{
    private readonly IAnsiConsole _console;

    public GameRenderer() : this(AnsiConsole.Console)
    {
    }

    public GameRenderer(IAnsiConsole console)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
    }

    public void RenderWelcome(bool animate = true)
    {
        SplashScreen.Show(_console, animate);
    }

    public void RenderRoom(Room room, Player player)
    {
        _console.WriteLine();

        // Room title
        var title = new Rule($"[bold cyan]{room.Name}[/]")
            .RuleStyle("cyan dim")
            .LeftJustified();
        _console.Write(title);

        // Room narrative
        var narrativePanel = new Panel(new Markup($"[italic]{room.Narrative}[/]"))
            .BorderColor(Color.Grey)
            .Padding(1, 0);
        _console.Write(narrativePanel);

        _console.WriteLine();

        // Challenge info
        if (room.Challenge != null)
        {
            RenderChallenge(room.Challenge, player);
        }

        // Exits
        if (room.Exits.Count > 0)
        {
            var exitsText = string.Join(", ", room.Exits.Keys.Select(k => $"[cyan]{k}[/]"));
            _console.MarkupLine($"[dim]Exits:[/] {exitsText}");
            _console.WriteLine();
        }
    }

    public void RenderCommandResult(CommandResult result)
    {
        if (result.Type == CommandType.Help)
        {
            var helpPanel = new Panel(result.Message)
                .Header("[bold cyan]Help[/]")
                .BorderColor(Color.Cyan1)
                .Padding(1, 0);
            _console.Write(helpPanel);
            return;
        }

        if (result.Type == CommandType.Status)
        {
            var statusPanel = new Panel(result.Message)
                .Header("[bold cyan]Status[/]")
                .BorderColor(Color.Cyan1)
                .Padding(1, 0);
            _console.Write(statusPanel);
            return;
        }

        if (result.Type == CommandType.Git)
        {
            if (result.Success)
            {
                if (!string.IsNullOrWhiteSpace(result.Message))
                {
                    // Check if challenge completed
                    if (result.Message.Contains("✓ Challenge completed!"))
                    {
                        var splitMarker = "\n\n✓ Challenge completed!";
                        var splitIndex = result.Message.IndexOf(splitMarker);

                        if (splitIndex > 0)
                        {
                            // Regular git output (before the success marker)
                            var gitOutput = result.Message.Substring(0, splitIndex);
                            _console.MarkupLine($"[dim]{Markup.Escape(gitOutput)}[/]");
                            _console.WriteLine();

                            // Success message (from the marker onwards, remove the leading newlines)
                            var successMessage = result.Message.Substring(splitIndex + 2); // Skip the \n\n
                            var successPanel = new Panel(Markup.Escape(successMessage))
                                .BorderColor(Color.Green)
                                .Padding(1, 0);
                            _console.Write(successPanel);
                        }
                        else
                        {
                            _console.MarkupLine($"[dim]{Markup.Escape(result.Message)}[/]");
                        }
                    }
                    else
                    {
                        _console.MarkupLine($"[dim]{Markup.Escape(result.Message)}[/]");
                    }
                }
            }
            else
            {
                _console.MarkupLine($"[red]Error:[/] {Markup.Escape(result.Message)}");
            }
            _console.WriteLine();
            return;
        }

        if (result.Type == CommandType.Movement)
        {
            if (result.Success)
            {
                _console.Clear();
                _console.MarkupLine($"[green]{Markup.Escape(result.Message)}[/]");
                _console.WriteLine();
            }
            else
            {
                _console.MarkupLine($"[red]{Markup.Escape(result.Message)}[/]");
                _console.WriteLine();
            }
            return;
        }

        if (result.Type == CommandType.Look)
        {
            var lookPanel = new Panel(Markup.Escape(result.Message))
                .BorderColor(Color.Grey)
                .Padding(1, 0);
            _console.Write(lookPanel);
            _console.WriteLine();
            return;
        }

        if (result.Type == CommandType.Answer)
        {
            if (result.Success)
            {
                var successPanel = new Panel(Markup.Escape(result.Message))
                    .BorderColor(Color.Green)
                    .Header("[bold green]Correct![/]")
                    .Padding(1, 0);
                _console.Write(successPanel);
            }
            else
            {
                var errorPanel = new Panel(Markup.Escape(result.Message))
                    .BorderColor(Color.Red)
                    .Header("[bold red]Incorrect[/]")
                    .Padding(1, 0);
                _console.Write(errorPanel);
            }
            _console.WriteLine();
            return;
        }

        if (result.Type == CommandType.Hint)
        {
            var hintPanel = new Panel(Markup.Escape(result.Message))
                .BorderColor(Color.Yellow)
                .Header("[bold yellow]Hint[/]")
                .Padding(1, 0);
            _console.Write(hintPanel);
            _console.WriteLine();
            return;
        }

        // Default rendering
        if (result.Success)
        {
            _console.MarkupLine($"[green]{Markup.Escape(result.Message)}[/]");
        }
        else
        {
            _console.MarkupLine($"[red]{Markup.Escape(result.Message)}[/]");
        }
        _console.WriteLine();
    }

    /// <summary>
    /// Render challenge based on its type
    /// </summary>
    private void RenderChallenge(IChallenge challenge, Player player)
    {
        var isCompleted = player.HasCompletedChallenge(challenge.Id);
        var statusIcon = isCompleted ? "✓" : "○";
        var statusColor = isCompleted ? "green" : "yellow";

        switch (challenge)
        {
            case QuizChallenge quiz:
                RenderQuizChallenge(quiz, statusIcon, statusColor, isCompleted);
                break;

            case ScenarioChallenge scenario:
                RenderScenarioChallenge(scenario, statusIcon, statusColor, isCompleted);
                break;

            case RepositoryChallenge repository:
                RenderRepositoryChallenge(repository, statusIcon, statusColor, isCompleted);
                break;

            default:
                // Fallback for unknown challenge types
                var challengeTable = new Table()
                    .BorderColor(Color.Grey)
                    .Border(TableBorder.Rounded)
                    .AddColumn(new TableColumn("Challenge").Centered())
                    .AddRow($"[{statusColor}]{statusIcon} {challenge.Description}[/]");
                _console.Write(challengeTable);
                _console.WriteLine();
                break;
        }
    }

    private void RenderQuizChallenge(QuizChallenge quiz, string statusIcon, string statusColor, bool isCompleted)
    {
        var table = new Table()
            .BorderColor(Color.Yellow)
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("[bold]Quiz Challenge[/]").Centered());

        table.AddRow($"[{statusColor}]{statusIcon} {quiz.Description}[/]");

        if (!isCompleted)
        {
            table.AddEmptyRow();
            table.AddRow($"[bold yellow]{quiz.Question}[/]");
            table.AddEmptyRow();

            for (int i = 0; i < quiz.Options.Count; i++)
            {
                table.AddRow($"  {i + 1}. {quiz.Options[i]}");
            }

            table.AddEmptyRow();
            table.AddRow("[dim]Use 'answer <number>' to respond (e.g., 'answer 1')[/]");
        }

        _console.Write(table);
        _console.WriteLine();
    }

    private void RenderScenarioChallenge(ScenarioChallenge scenario, string statusIcon, string statusColor, bool isCompleted)
    {
        var panel = new Panel(
            new Markup($"""
                [{statusColor}]{statusIcon} {scenario.Description}[/]

                [italic cyan]{scenario.Scenario}[/]
                """)
        )
        {
            Header = new PanelHeader("[bold magenta]Scenario Challenge[/]", Justify.Center),
            Border = BoxBorder.Double,
            BorderStyle = new Style(Color.Magenta)
        };

        _console.Write(panel);
        _console.WriteLine();
    }

    private void RenderRepositoryChallenge(RepositoryChallenge repository, string statusIcon, string statusColor, bool isCompleted)
    {
        var challengeTable = new Table()
            .BorderColor(Color.Grey)
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("Challenge").Centered())
            .AddRow($"[{statusColor}]{statusIcon} {repository.Description}[/]");

        _console.Write(challengeTable);
        _console.WriteLine();
    }

    public void RenderError(string message)
    {
        var errorPanel = new Panel($"[red bold]Error:[/] {Markup.Escape(message)}")
            .BorderColor(Color.Red)
            .Padding(1, 0);
        _console.Write(errorPanel);
        _console.WriteLine();
    }

    public void RenderGameComplete(Player player)
    {
        _console.Clear();

        var title = new FigletText("Victory!")
            .Centered()
            .Color(Color.Green);
        _console.Write(title);

        // Epic narrative epilogue
        var epilogue = new Panel(
            new Markup("""
                [italic]With a final incantation of the ancient [cyan]git push[/] command, the dungeon's exit materializes before you in a cascade of ethereal light. The stone walls that once imprisoned you crumble into cascading streams of [green]commits[/], each one a testament to your journey through the depths.[/]

                [italic]You step through the threshold, emerging from the [bold]GitOut dungeon[/] as a master of the arcane version control arts. The [cyan]branches[/] that once seemed like tangled paths through time now bend to your will. The [green]merges[/] that threatened to tear reality asunder are merely converging timelines in your capable hands. The sacred [yellow]commits[/] you learned to craft seal your victories like ancient scrolls of power.[/]

                [italic]As sunlight washes over you, you realize the true treasure was never gold or glory—it was [bold green]knowledge[/]. The ability to track change, to navigate history, to collaborate across the fabric of time itself. You have conquered the rooms of confusion, solved the riddles of repositories, and emerged victorious.[/]

                [italic]The dungeon may be behind you, brave adventurer, but the [bold cyan]git magic[/] you've mastered will serve you well in all future quests. Go forth, for you are now a [bold yellow]keeper of the sacred version control[/]![/]
                """)
        )
        {
            Header = new PanelHeader("[bold cyan]The Tale of Your Triumph[/]", Justify.Center),
            Border = BoxBorder.Double,
            BorderStyle = new Style(Color.Cyan)
        };

        _console.Write(epilogue);
        _console.WriteLine();

        var stats = new Table()
            .BorderColor(Color.Green)
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Statistic[/]")
            .AddColumn("[bold]Value[/]")
            .AddRow("Player", player.Name)
            .AddRow("Rooms Completed", player.CompletedRooms.Count.ToString())
            .AddRow("Challenges Completed", player.CompletedChallenges.Count.ToString())
            .AddRow("Total Moves", player.MoveCount.ToString())
            .AddRow("Time Played", (DateTime.UtcNow - player.GameStarted).ToString(@"hh\:mm\:ss"));

        var panel = new Panel(stats)
            .Header("[bold green]Game Statistics[/]")
            .BorderColor(Color.Green)
            .Padding(1, 1)
            .Expand();

        _console.Write(panel);
        _console.WriteLine();

        _console.MarkupLine("[bold yellow]Congratulations, brave adventurer! You've mastered the sacred arts of Git and escaped the dungeon![/]");
        _console.WriteLine();
    }

    public void RenderWorkingDirectory(string directory)
    {
        _console.MarkupLine($"[dim]Working Directory: {Markup.Escape(directory)}[/]");
        _console.WriteLine();
    }

    public string GetPrompt()
    {
        return _console.Prompt(
            new TextPrompt<string>("[bold cyan]>[/] ")
                .AllowEmpty());
    }
}
