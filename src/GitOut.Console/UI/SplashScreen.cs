using Spectre.Console;

namespace GitOut.Console.UI;

/// <summary>
/// Renders an epic animated splash screen for GitOut
/// </summary>
public static class SplashScreen
{
    private static readonly string[] DungeonLogo = new[]
    {
        @"     ██████╗ ██╗████████╗ ██████╗ ██╗   ██╗████████╗",
        @"    ██╔════╝ ██║╚══██╔══╝██╔═══██╗██║   ██║╚══██╔══╝",
        @"    ██║  ███╗██║   ██║   ██║   ██║██║   ██║   ██║   ",
        @"    ██║   ██║██║   ██║   ██║   ██║██║   ██║   ██║   ",
        @"    ╚██████╔╝██║   ██║   ╚██████╔╝╚██████╔╝   ██║   ",
        @"     ╚═════╝ ╚═╝   ╚═╝    ╚═════╝  ╚═════╝    ╚═╝   "
    };

    private static readonly string[] DungeonArt = new[]
    {
        @"                        ▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄",
        @"                    ▄█████████████████████████▄",
        @"                  ▄███████████████████████████████▄",
        @"                ▄██████████▀▀▀▀▀▀▀▀▀▀▀██████████████▄",
        @"               ████████▀                 ▀█████████████",
        @"              ████████   ┌─────────────┐   ████████████",
        @"             █████████   │ ╔═══════════╗│   █████████████",
        @"            ██████████   │ ║ > git init ║│   ██████████████",
        @"            ██████████   │ ╚═══════════╝│   ██████████████",
        @"            ██████████   └──────┬┬──────┘   ██████████████",
        @"             █████████          ││          █████████████",
        @"              ████████    ╔═════╧╧═════╗    ████████████",
        @"               ████████   ║   ENTER    ║   █████████████",
        @"                ▀██████   ║  THE DUNGEON║   ██████████▀",
        @"                  ▀████   ╚═════════════╝   ████████▀",
        @"                    ▀█████████████████████████████▀",
        @"                        ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀"
    };

    private static readonly string[] TorchFrames = new[]
    {
        @"
    )
   (  )
    )(
   ████
   ████",
        @"
   (
  ( ) )
   )(
   ████
   ████",
        @"
    )
  (  )
   ()
   ████
   ████",
        @"
  (  )
   ( )
   )(
   ████
   ████"
    };

    public static void Show(bool animate = true)
    {
        AnsiConsole.Clear();
        AnsiConsole.Cursor.Hide();

        try
        {
            if (animate)
            {
                ShowAnimatedIntro();
            }

            RenderMainScreen();
        }
        finally
        {
            AnsiConsole.Cursor.Show();
        }
    }

    private static void ShowAnimatedIntro()
    {
        // Fade in effect with the logo
        var colors = new[] { Color.Grey19, Color.Grey30, Color.Grey42, Color.Grey54, Color.Cyan1 };

        foreach (var color in colors)
        {
            AnsiConsole.Clear();
            RenderLogo(color);
            Thread.Sleep(100);
        }

        // Show subtitle with typing effect
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();

        var subtitle = "A Dungeon Crawler That Teaches Git";
        AnsiConsole.Cursor.MoveLeft(AnsiConsole.Console.Profile.Width);

        var padding = (AnsiConsole.Console.Profile.Width - subtitle.Length) / 2;
        AnsiConsole.Write(new string(' ', Math.Max(0, padding)));

        foreach (var c in subtitle)
        {
            AnsiConsole.Markup($"[bold yellow]{c}[/]");
            Thread.Sleep(30);
        }

        Thread.Sleep(500);

        // Animate torch flicker briefly
        for (int i = 0; i < 6; i++)
        {
            RenderTorches(i % TorchFrames.Length);
            Thread.Sleep(150);
        }
    }

    private static void RenderLogo(Color color)
    {
        var logoWidth = DungeonLogo[0].Length;
        var consoleWidth = AnsiConsole.Console.Profile.Width;
        var padding = Math.Max(0, (consoleWidth - logoWidth) / 2);

        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();

        foreach (var line in DungeonLogo)
        {
            AnsiConsole.Write(new string(' ', padding));
            AnsiConsole.MarkupLine($"[{color.ToMarkup()}]{Markup.Escape(line)}[/]");
        }
    }

    private static void RenderTorches(int frame)
    {
        var consoleWidth = AnsiConsole.Console.Profile.Width;

        // Left torch
        var leftX = Math.Max(5, consoleWidth / 6);
        var rightX = Math.Min(consoleWidth - 15, consoleWidth * 5 / 6);

        var torchLines = TorchFrames[frame].Split('\n');

        var currentRow = 10;
        AnsiConsole.Cursor.SetPosition(0, currentRow);

        foreach (var line in torchLines)
        {
            AnsiConsole.Cursor.SetPosition(leftX, currentRow);
            AnsiConsole.Markup($"[yellow]{Markup.Escape(line)}[/]");

            AnsiConsole.Cursor.SetPosition(rightX, currentRow);
            AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(line)}[/]");
            currentRow++;
        }
    }

    private static void RenderMainScreen()
    {
        AnsiConsole.Clear();

        // Top border
        var width = Math.Min(80, AnsiConsole.Console.Profile.Width - 4);
        var borderTop = "╔" + new string('═', width - 2) + "╗";
        var borderBot = "╚" + new string('═', width - 2) + "╝";

        var consoleWidth = AnsiConsole.Console.Profile.Width;
        var leftPad = Math.Max(0, (consoleWidth - width) / 2);
        var padding = new string(' ', leftPad);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"{padding}[cyan]{borderTop}[/]");

        // Render logo centered
        foreach (var line in DungeonLogo)
        {
            var linePad = Math.Max(0, (width - line.Length - 2) / 2);
            var content = new string(' ', linePad) + line;
            content = content.PadRight(width - 2);
            AnsiConsole.MarkupLine($"{padding}[cyan]║[/][bold cyan]{Markup.Escape(content)}[/][cyan]║[/]");
        }

        // Empty line
        AnsiConsole.MarkupLine($"{padding}[cyan]║{new string(' ', width - 2)}║[/]");

        // Subtitle
        var subtitle = "⚔ A Dungeon Crawler That Teaches Git ⚔";
        var subPad = Math.Max(0, (width - subtitle.Length - 2) / 2);
        var subContent = new string(' ', subPad) + subtitle;
        subContent = subContent.PadRight(width - 2);
        AnsiConsole.MarkupLine($"{padding}[cyan]║[/][bold yellow]{subContent}[/][cyan]║[/]");

        // Empty line
        AnsiConsole.MarkupLine($"{padding}[cyan]║{new string(' ', width - 2)}║[/]");

        // Features section
        var features = new[]
        {
            ("🏰", "23 dungeon rooms to conquer"),
            ("📜", "Learn real git commands"),
            ("🎯", "Solve challenges to progress"),
            ("🏆", "Master git from basics to expert")
        };

        foreach (var (icon, text) in features)
        {
            var featureText = $"  {icon} {text}";
            var featureContent = featureText.PadRight(width - 2);
            AnsiConsole.MarkupLine($"{padding}[cyan]║[/][dim]{featureContent}[/][cyan]║[/]");
        }

        // Empty line
        AnsiConsole.MarkupLine($"{padding}[cyan]║{new string(' ', width - 2)}║[/]");

        // Dungeon entrance art (simplified)
        var entranceArt = new[]
        {
            "            ▄▄████████████▄▄            ",
            "         ▄██▀▀            ▀▀██▄         ",
            "        ██▀   ┌──────────┐   ▀██        ",
            "       ██     │  ENTER   │     ██       ",
            "       ██     └────┬┬────┘     ██       ",
            "       ██▄▄▄▄▄▄▄▄▄▄██▄▄▄▄▄▄▄▄▄▄██       "
        };

        foreach (var line in entranceArt)
        {
            var artPad = Math.Max(0, (width - line.Length - 2) / 2);
            var artContent = new string(' ', artPad) + line;
            artContent = artContent.PadRight(width - 2);
            AnsiConsole.MarkupLine($"{padding}[cyan]║[/][grey]{Markup.Escape(artContent)}[/][cyan]║[/]");
        }

        // Empty line
        AnsiConsole.MarkupLine($"{padding}[cyan]║{new string(' ', width - 2)}║[/]");

        // Commands hint
        var cmdHint = "Type 'help' for commands • 'quit' to exit";
        var cmdPad = Math.Max(0, (width - cmdHint.Length - 2) / 2);
        var cmdContent = new string(' ', cmdPad) + cmdHint;
        cmdContent = cmdContent.PadRight(width - 2);
        AnsiConsole.MarkupLine($"{padding}[cyan]║[/][dim italic]{cmdContent}[/][cyan]║[/]");

        // Bottom border
        AnsiConsole.MarkupLine($"{padding}[cyan]{borderBot}[/]");
        AnsiConsole.WriteLine();

        // Git commands preview
        RenderGitPreview(padding, width);

        AnsiConsole.WriteLine();
    }

    private static void RenderGitPreview(string padding, int width)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey42)
            .AddColumn(new TableColumn("[bold grey]Git Commands You'll Master[/]").Centered())
            .Width(width);

        var commands = new Grid()
            .AddColumn()
            .AddColumn()
            .AddColumn()
            .AddRow(
                "[cyan]init[/] [dim]add[/] [cyan]commit[/]",
                "[cyan]branch[/] [dim]merge[/] [cyan]rebase[/]",
                "[cyan]stash[/] [dim]reflog[/] [cyan]bisect[/]")
            .AddRow(
                "[dim]log[/] [cyan]status[/] [dim]restore[/]",
                "[dim]cherry-pick[/] [cyan]tag[/] [dim]remote[/]",
                "[dim]worktree[/] [cyan]blame[/] [dim]hooks[/]");

        table.AddRow(commands);

        AnsiConsole.Write(new Padder(table).PadLeft(padding.Length));
    }
}
