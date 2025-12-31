using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;
using System.Runtime.InteropServices;

namespace GitOut.Infrastructure.Persistence.RoomFactories;

public class Room19HookHollowFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room19HookHollowFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        var challenge = new RepositoryChallenge(
            id: "hook-hollow-challenge",
            description: "Create and configure a git hook to automate pre-commit checks",
            gitExecutor: _gitExecutor,
            requireGitInit: true,
            customSetup: async (workingDir, gitExec) =>
            {
                // Initialize repo
                await gitExec.ExecuteAsync("init", workingDir);
                await gitExec.ExecuteAsync("config user.email \"adventurer@gitout.com\"", workingDir);
                await gitExec.ExecuteAsync("config user.name \"Adventurer\"", workingDir);

                // Create a sample file
                await File.WriteAllTextAsync(Path.Combine(workingDir, "app.js"),
                    "// Main application\nconsole.log('Hello, world!');\n");
                await gitExec.ExecuteAsync("add app.js", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Initial commit\"", workingDir);

                // Create a pre-commit hook template in the working directory
                var hookTemplate = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "#!/bin/sh\n# Pre-commit hook template\n# This hook prevents commits with debugging statements\n\necho \"Running pre-commit checks...\"\n\nif git diff --cached --name-only | xargs grep -l \"console.log\" > /dev/null 2>&1; then\n    echo \"Error: Found console.log statements in staged files.\"\n    echo \"Please remove debugging statements before committing.\"\n    exit 1\nfi\n\necho \"Pre-commit checks passed!\"\nexit 0\n"
                    : "#!/bin/sh\n# Pre-commit hook template\n# This hook prevents commits with debugging statements\n\necho \"Running pre-commit checks...\"\n\nif git diff --cached --name-only | xargs grep -l \"console.log\" > /dev/null 2>&1; then\n    echo \"Error: Found console.log statements in staged files.\"\n    echo \"Please remove debugging statements before committing.\"\n    exit 1\nfi\n\necho \"Pre-commit checks passed!\"\nexit 0\n";

                await File.WriteAllTextAsync(Path.Combine(workingDir, "pre-commit-template"), hookTemplate);

                // Create instructions file
                await File.WriteAllTextAsync(Path.Combine(workingDir, "HOOK_INSTRUCTIONS.txt"),
                    "Git hooks are scripts that run automatically at specific points in the git workflow.\n\n" +
                    "Your mission: Install a pre-commit hook that checks for debugging statements.\n\n" +
                    "Steps:\n" +
                    "1. A hook template file 'pre-commit-template' has been created in the working directory\n" +
                    "2. Copy it to .git/hooks/pre-commit\n" +
                    (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                        ? "3. On Windows, the hook should work once copied\n"
                        : "3. Make it executable: chmod +x .git/hooks/pre-commit\n") +
                    "4. The hook will run automatically before each commit\n\n" +
                    "Example commands:\n" +
                    (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                        ? "  copy pre-commit-template .git\\hooks\\pre-commit\n"
                        : "  cp pre-commit-template .git/hooks/pre-commit\n  chmod +x .git/hooks/pre-commit\n") +
                    "\nNote: Hooks are stored in .git/hooks/ and are NOT tracked by git.\n" +
                    "This means each developer must set up hooks independently.");
            },
            customValidator: async (workingDir, gitExec) =>
            {
                // Check if .git/hooks/pre-commit exists
                var hookPath = Path.Combine(workingDir, ".git", "hooks", "pre-commit");

                if (!File.Exists(hookPath))
                {
                    return new ChallengeResult(
                        false,
                        "The pre-commit hook has not been installed yet.",
                        "Copy the pre-commit-template file to .git/hooks/pre-commit. You can use shell commands like 'cp' (Unix/Mac) or 'copy' (Windows)."
                    );
                }

                // On Unix systems, check if the hook is executable
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    try
                    {
                        var fileInfo = new FileInfo(hookPath);
                        var unixFileMode = File.GetUnixFileMode(hookPath);

                        // Check if owner execute permission is set (at minimum)
                        var isExecutable = (unixFileMode & UnixFileMode.UserExecute) == UnixFileMode.UserExecute;

                        if (!isExecutable)
                        {
                            return new ChallengeResult(
                                false,
                                "The hook file exists but is not executable.",
                                "Make the hook executable with: chmod +x .git/hooks/pre-commit"
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        return new ChallengeResult(
                            false,
                            $"Could not verify hook permissions: {ex.Message}",
                            "Ensure the hook file has execute permissions: chmod +x .git/hooks/pre-commit"
                        );
                    }
                }

                // Verify the hook has meaningful content (not just an empty file)
                var hookContent = await File.ReadAllTextAsync(hookPath);
                if (string.IsNullOrWhiteSpace(hookContent) || hookContent.Length < 50)
                {
                    return new ChallengeResult(
                        false,
                        "The hook file exists but appears to be empty or incomplete.",
                        "Make sure you copied the entire pre-commit-template file to .git/hooks/pre-commit"
                    );
                }

                // Success!
                var successMessage = "Excellent work! You've successfully installed a git hook. " +
                    "Git hooks are powerful automation tools that run at specific points in your git workflow:\n\n" +
                    "[cyan]Common Hook Types:[/]\n" +
                    "  • pre-commit - Runs before a commit is created (validate code, run linters)\n" +
                    "  • prepare-commit-msg - Modify commit message template\n" +
                    "  • commit-msg - Validate commit message format\n" +
                    "  • post-commit - Run after commit (notify teams, trigger CI)\n" +
                    "  • pre-push - Run before pushing (run tests, prevent bad pushes)\n" +
                    "  • pre-rebase - Run before rebase operations\n\n" +
                    "[cyan]Key Insights:[/]\n" +
                    "  • Hooks are local and NOT committed to the repository\n" +
                    "  • Exit code 0 = success (continue), non-zero = abort operation\n" +
                    "  • Can be written in any language (shell, Python, Ruby, etc.)\n" +
                    "  • Useful for enforcing team standards and catching errors early\n" +
                    "  • Teams often share hook templates in docs, but each dev installs them\n" +
                    (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                        ? "  • On Windows, hooks run via Git Bash shell\n"
                        : "  • Hooks must be executable (chmod +x) on Unix/Mac\n") +
                    "\n[green]Pro Tip:[/] Many teams use tools like Husky (Node.js) or pre-commit (Python) " +
                    "to manage hooks automatically across the team. These tools can commit hook configurations " +
                    "to the repo and install them for all developers!";

                return new ChallengeResult(true, successMessage, null);
            }
        );

        var room = new Room(
            id: "room-19",
            name: "The Hook Hollow",
            description: "A chamber of automation where git hooks guard the repository",
            narrative: "You enter a hollow chamber filled with clockwork mechanisms and tripwires. " +
                      "A master automaton greets you: 'Welcome to the Hook Hollow! Here, we use git hooks " +
                      "to automate checks and enforce standards. Hooks are scripts that run automatically " +
                      "at key moments in your git workflow—like guards that check everything before allowing an action.'" +
                      "\n\n[yellow]The Concept:[/]" +
                      "\n  • Git hooks are scripts stored in .git/hooks/" +
                      "\n  • They run automatically at specific git events (commit, push, etc.)" +
                      "\n  • Can prevent bad commits, enforce code quality, run tests" +
                      "\n  • If a hook exits with non-zero code, the git operation is aborted" +
                      "\n\n[yellow]Your Mission:[/]" +
                      "\n  1. Read HOOK_INSTRUCTIONS.txt for details" +
                      "\n  2. Copy pre-commit-template to .git/hooks/pre-commit" +
                      (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                          ? "\n  3. On Windows, the hook will work once copied"
                          : "\n  3. Make it executable with chmod +x .git/hooks/pre-commit") +
                      "\n  4. The hook will prevent commits with console.log statements" +
                      "\n\n[yellow]═══ Hook Commands ═══[/]" +
                      "\n[cyan]ls .git/hooks/[/] - List available hook files" +
                      "\n  • Git provides sample hooks (*.sample files)" +
                      "\n  • Remove .sample extension to activate them" +
                      "\n\n[cyan]Installing a hook:[/]" +
                      (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                          ? "\n  copy pre-commit-template .git\\hooks\\pre-commit"
                          : "\n  cp pre-commit-template .git/hooks/pre-commit\n  chmod +x .git/hooks/pre-commit") +
                      "\n\n[cyan]Testing the hook:[/]" +
                      "\n  • Make a change that violates the hook's rules" +
                      "\n  • Try to commit - the hook should block it" +
                      "\n  • Fix the issue and commit again successfully" +
                      "\n\n[cyan]Common Hook Use Cases:[/]" +
                      "\n  • pre-commit: Run linters, formatters, or quick tests" +
                      "\n  • commit-msg: Enforce commit message format (e.g., 'feat:', 'fix:')" +
                      "\n  • pre-push: Run full test suite before pushing" +
                      "\n  • post-checkout: Install dependencies when switching branches" +
                      "\n\n[cyan]Bypassing Hooks (Use Sparingly!):[/]" +
                      "\n  [dim]git commit --no-verify[/]  # Skip pre-commit and commit-msg hooks" +
                      "\n  [dim]git push --no-verify[/]    # Skip pre-push hooks" +
                      "\n  • Only bypass when you understand the consequences!" +
                      "\n\n[green]Pro Tips:[/]" +
                      "\n  • Hooks are local - not tracked by git (each dev sets up their own)" +
                      "\n  • Keep hooks fast - slow hooks frustrate developers" +
                      "\n  • Share hook templates in repo docs or use hook managers (Husky, pre-commit)" +
                      "\n  • Use hooks to catch errors early, before CI/CD runs" +
                      "\n  • Exit with code 0 for success, non-zero to abort the git operation" +
                      "\n\n[dim]Hooks are your repository's guardians, enforcing quality at every step![/]",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-20" } },
            isStartRoom: false,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}
