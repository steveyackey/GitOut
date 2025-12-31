namespace GitOut.Console.Input;

/// <summary>
/// Handles parsing and validation of user input
/// </summary>
public class CommandHandler
{
    public ParsedCommand ParseCommand(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new ParsedCommand(
                CommandCategory.Unknown,
                string.Empty,
                "Command cannot be empty"
            );
        }

        input = input.Trim();

        // Handle exit command
        if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            return new ParsedCommand(CommandCategory.System, input, null);
        }

        // Handle help commands
        if (input.Equals("help", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("?", StringComparison.OrdinalIgnoreCase))
        {
            return new ParsedCommand(CommandCategory.Game, "help", null);
        }

        // Handle game commands
        if (input.Equals("status", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("look", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("examine", StringComparison.OrdinalIgnoreCase))
        {
            return new ParsedCommand(CommandCategory.Game, input, null);
        }

        // Handle movement commands
        if (input.StartsWith("go ", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("forward", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("back", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("north", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("south", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("east", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("west", StringComparison.OrdinalIgnoreCase))
        {
            return new ParsedCommand(CommandCategory.Movement, input, null);
        }

        // Handle git commands
        if (input.StartsWith("git ", StringComparison.OrdinalIgnoreCase))
        {
            return new ParsedCommand(CommandCategory.Git, input, null);
        }

        // Unknown command
        return new ParsedCommand(
            CommandCategory.Unknown,
            input,
            $"Unknown command: {input}. Type 'help' for available commands."
        );
    }

    public bool IsExitCommand(ParsedCommand command)
    {
        return command.Category == CommandCategory.System &&
               command.Command.Equals("exit", StringComparison.OrdinalIgnoreCase);
    }
}

public enum CommandCategory
{
    Unknown,
    System,
    Game,
    Movement,
    Git
}

public record ParsedCommand(
    CommandCategory Category,
    string Command,
    string? ErrorMessage
)
{
    public bool IsValid => string.IsNullOrEmpty(ErrorMessage);
}
