using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;

namespace GitOut.Infrastructure.Persistence.RoomFactories;

public class Room07RestorationVaultFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room07RestorationVaultFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        var challenge = new RepositoryChallenge(
            id: "restoration-vault-challenge",
            description: "Restore the corrupted sacred-text.txt file to its original state",
            gitExecutor: _gitExecutor,
            requireGitInit: true,
            customSetup: async (workingDir, gitExec) =>
            {
                // Initialize repo
                await gitExec.ExecuteAsync("init", workingDir);
                await gitExec.ExecuteAsync("config user.email \"adventurer@gitout.game\"", workingDir);
                await gitExec.ExecuteAsync("config user.name \"Adventurer\"", workingDir);

                // Create and commit the sacred text
                var originalContent = "Sacred Text: In the beginning, there was git init...";
                await File.WriteAllTextAsync(Path.Combine(workingDir, "sacred-text.txt"), originalContent);
                await gitExec.ExecuteAsync("add sacred-text.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Preserve sacred text\"", workingDir);

                // Corrupt the file
                await File.WriteAllTextAsync(Path.Combine(workingDir, "sacred-text.txt"),
                    "CORRUPTED: The text has been damaged and must be restored!");
            },
            customValidator: async (workingDir, gitExec) =>
            {
                // Check if the file has been restored
                var filePath = Path.Combine(workingDir, "sacred-text.txt");
                if (File.Exists(filePath))
                {
                    var content = await File.ReadAllTextAsync(filePath);
                    if (content.Contains("Sacred Text: In the beginning"))
                    {
                        return new ChallengeResult(
                            true,
                            "The sacred text has been restored to its original form! You've learned how to discard unwanted changes and return files to their last committed state. Git restore is your safety net when experiments go wrong.",
                            null
                        );
                    }
                }

                return new ChallengeResult(
                    false,
                    "The sacred text is still corrupted.",
                    "Use 'git restore sacred-text.txt' or 'git checkout -- sacred-text.txt' to restore the file"
                );
            }
        );

        var room = new Room(
            id: "room-7",
            name: "The Restoration Vault",
            description: "A vault containing a corrupted sacred text",
            narrative: "You enter a solemn vault. In the center, a pedestal holds a glowing manuscript labeled 'sacred-text.txt'. " +
                      "But something is wrong - the text flickers with corruption! Dark energy has altered its contents. " +
                      "A guardian spirit whispers: 'Fear not, for git remembers all. Every committed change is preserved. " +
                      "You can restore files to their last committed state, undoing unwanted modifications.' " +
                      "\n\nThe sacred-text.txt file has been corrupted!" +
                      "\n\n[yellow]═══ Command Guide ═══[/]" +
                      "\n[cyan]git diff <file>[/] - Shows exactly what changed in a file" +
                      "\n  • Compares working directory to last commit" +
                      "\n  • Lines starting with - were removed, + were added" +
                      "\n  • Shows changes before you commit or restore them" +
                      "\n\n[cyan]git restore <file>[/] - Discards uncommitted changes in a file (modern command)" +
                      "\n  • Reverts the file back to the last committed version" +
                      "\n  • [green]Works on:[/] Modified or staged files" +
                      "\n  • [red]Doesn't work on:[/] Committed changes or untracked files" +
                      "\n  • WARNING: This permanently deletes your uncommitted changes!" +
                      "\n  • Older alternative: 'git checkout -- <file>' (still works)" +
                      "\n\n[dim]Think of commits as save points - restore takes you back to the last save![/]" +
                      "\n\n[yellow]To complete this challenge:[/]" +
                      "\n  1. Check the file status: [cyan]git status[/]" +
                      "\n  2. See what changed: [cyan]git diff sacred-text.txt[/]" +
                      "\n  3. Restore the original: [cyan]git restore sacred-text.txt[/]",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-8" } },
            isStartRoom: false,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}
