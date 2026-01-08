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
        Show(AnsiConsole.Console, animate);
    }

    public static void Show(IAnsiConsole console, bool animate = true)
    {
        console.Clear();
        console.Cursor.Hide();

        try
        {
            if (animate)
            {
                ShowAnimatedIntro(console);
            }

            RenderMainScreen(console);
        }
        finally
        {
            console.Cursor.Show();
        }
    }

    private static void ShowAnimatedIntro(IAnsiConsole console)
    {
        // Fade in effect with the logo
        var colors = new[] { Color.Grey19, Color.Grey30, Color.Grey42, Color.Grey54, Color.Cyan1 };

        foreach (var color in colors)
        {
            console.Clear();
            RenderLogo(console, color);
            Thread.Sleep(100);
        }

        // Show subtitle with typing effect
        console.WriteLine();
        console.WriteLine();

        var subtitle = "A Dungeon Crawler That Teaches Git";
        console.Cursor.MoveLeft(console.Profile.Width);

        var padding = (console.Profile.Width - subtitle.Length) / 2;
        console.Write(new string(' ', Math.Max(0, padding)));

        foreach (var c in subtitle)
        {
            console.Markup($"[bold yellow]{c}[/]");
            Thread.Sleep(30);
        }

        Thread.Sleep(500);

        // Animate torch flicker briefly
        for (int i = 0; i < 6; i++)
        {
            RenderTorches(console, i % TorchFrames.Length);
            Thread.Sleep(150);
        }
    }

    private static void RenderLogo(IAnsiConsole console, Color color)
    {
        var logoWidth = DungeonLogo[0].Length;
        var consoleWidth = console.Profile.Width;
        var padding = Math.Max(0, (consoleWidth - logoWidth) / 2);

        console.WriteLine();
        console.WriteLine();

        foreach (var line in DungeonLogo)
        {
            console.Write(new string(' ', padding));
            console.MarkupLine($"[{color.ToMarkup()}]{Markup.Escape(line)}[/]");
        }
    }

    private static void RenderTorches(IAnsiConsole console, int frame)
    {
        var consoleWidth = console.Profile.Width;

        // Left torch
        var leftX = Math.Max(5, consoleWidth / 6);
        var rightX = Math.Min(consoleWidth - 15, consoleWidth * 5 / 6);

        var torchLines = TorchFrames[frame].Split('\n');

        var currentRow = 10;
        console.Cursor.SetPosition(0, currentRow);

        foreach (var line in torchLines)
        {
            console.Cursor.SetPosition(leftX, currentRow);
            console.Markup($"[yellow]{Markup.Escape(line)}[/]");

            console.Cursor.SetPosition(rightX, currentRow);
            console.MarkupLine($"[yellow]{Markup.Escape(line)}[/]");
            currentRow++;
        }
    }

    private static void RenderMainScreen(IAnsiConsole console)
    {
        console.Clear();

        // Top border
        var width = Math.Min(80, console.Profile.Width - 4);
        var borderTop = "╔" + new string('═', width - 2) + "╗";
        var borderBot = "╚" + new string('═', width - 2) + "╝";

        var consoleWidth = console.Profile.Width;
        var leftPad = Math.Max(0, (consoleWidth - width) / 2);
        var padding = new string(' ', leftPad);

        console.WriteLine();
        console.MarkupLine($"{padding}[cyan]{borderTop}[/]");

        // Render logo centered
        foreach (var line in DungeonLogo)
        {
            var linePad = Math.Max(0, (width - line.Length - 2) / 2);
            var content = new string(' ', linePad) + line;
            content = content.PadRight(width - 2);
            console.MarkupLine($"{padding}[cyan]║[/][bold cyan]{Markup.Escape(content)}[/][cyan]║[/]");
        }

        // Empty line
        console.MarkupLine($"{padding}[cyan]║{new string(' ', width - 2)}║[/]");

        // Subtitle
        var subtitle = "⚔ A Dungeon Crawler That Teaches Git ⚔";
        var subPad = Math.Max(0, (width - subtitle.Length - 2) / 2);
        var subContent = new string(' ', subPad) + subtitle;
        subContent = subContent.PadRight(width - 2);
        console.MarkupLine($"{padding}[cyan]║[/][bold yellow]{subContent}[/][cyan]║[/]");

        // Empty line
        console.MarkupLine($"{padding}[cyan]║{new string(' ', width - 2)}║[/]");

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
            console.MarkupLine($"{padding}[cyan]║[/][dim]{featureContent}[/][cyan]║[/]");
        }

        // Empty line
        console.MarkupLine($"{padding}[cyan]║{new string(' ', width - 2)}║[/]");

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
            console.MarkupLine($"{padding}[cyan]║[/][grey]{Markup.Escape(artContent)}[/][cyan]║[/]");
        }

        // Empty line
        console.MarkupLine($"{padding}[cyan]║{new string(' ', width - 2)}║[/]");

        // Commands hint
        var cmdHint = "Type 'help' for commands • 'quit' to exit";
        var cmdPad = Math.Max(0, (width - cmdHint.Length - 2) / 2);
        var cmdContent = new string(' ', cmdPad) + cmdHint;
        cmdContent = cmdContent.PadRight(width - 2);
        console.MarkupLine($"{padding}[cyan]║[/][dim italic]{cmdContent}[/][cyan]║[/]");

        // Bottom border
        console.MarkupLine($"{padding}[cyan]{borderBot}[/]");
        console.WriteLine();

        // Git commands preview
        RenderGitPreview(console, padding, width);

        console.WriteLine();
    }

    private static void RenderGitPreview(IAnsiConsole console, string padding, int width)
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

        console.Write(new Padder(table).PadLeft(padding.Length));
    }
}
