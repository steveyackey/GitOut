using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;

namespace GitOut.Infrastructure.Persistence.RoomFactories;

public class Room10StashSanctumFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room10StashSanctumFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        var challenge = new RepositoryChallenge(
            id: "stash-sanctum-challenge",
            description: "Use git stash to temporarily save work in progress",
            gitExecutor: _gitExecutor,
            requireGitInit: true,
            customSetup: async (workingDir, gitExec) =>
            {
                // Initialize repo
                await gitExec.ExecuteAsync("init", workingDir);
                await gitExec.ExecuteAsync("config user.email \"adventurer@gitout.com\"", workingDir);
                await gitExec.ExecuteAsync("config user.name \"Adventurer\"", workingDir);

                // Create initial commit
                await File.WriteAllTextAsync(Path.Combine(workingDir, "quest-log.txt"),
                    "Quest 1: Slay the dragon\nQuest 2: Find the artifact");
                await gitExec.ExecuteAsync("add quest-log.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Initial quest log\"", workingDir);

                // Create work in progress (modified but not committed)
                await File.WriteAllTextAsync(Path.Combine(workingDir, "quest-log.txt"),
                    "Quest 1: Slay the dragon\nQuest 2: Find the artifact\nQuest 3: Work in progress...");

                // Create a new untracked file
                await File.WriteAllTextAsync(Path.Combine(workingDir, "notes.txt"),
                    "These are my temporary notes");
            },
            customValidator: async (workingDir, gitExec) =>
            {
                // Check if work has been stashed
                var stashList = await gitExec.GetStashListAsync(workingDir);
                if (string.IsNullOrWhiteSpace(stashList))
                {
                    return new ChallengeResult(
                        false,
                        "Your work has not been placed in the sanctum yet.",
                        "Use 'git stash -u' to save your work in progress, including untracked files"
                    );
                }

                // Check if working directory is clean after stashing
                var status = await gitExec.GetStatusAsync(workingDir);
                if (status.Contains("working tree clean"))
                {
                    return new ChallengeResult(
                        true,
                        "Your work has been safely stored in the stash sanctum! The working directory is now clean, " +
                        "and you can switch branches or perform other operations. Use 'git stash pop' to restore your work later.",
                        null
                    );
                }

                return new ChallengeResult(
                    false,
                    "The sanctum shows your stashed work, but the working directory should be clean. " +
                    "You may have untracked files remaining.",
                    "Use 'git stash -u' to include untracked files in your stash"
                );
            }
        );

        var room = new Room(
            id: "room-10",
            name: "The Stash Sanctum",
            description: "A mystical vault for temporary storage",
            narrative: "You enter a chamber filled with swirling portals of light. Each portal preserves a moment in time - " +
                      "work that is incomplete, ideas half-formed, changes not yet ready to commit. " +
                      "\n\nAn ethereal guardian speaks: 'Sometimes you must set aside your current work to handle urgent matters. " +
                      "The stash allows you to save your work in progress without committing it, giving you a clean working directory.' " +
                      "\n\nYou notice that quest-log.txt has been modified with unfinished work, and there's also an untracked notes.txt file. " +
                      "You need to stash ALL changes to get a completely clean working directory!" +
                      "\n\n[yellow]═══ Command Guide ═══[/]" +
                      "\n[cyan]git stash[/] - Saves your uncommitted changes and gives you a clean working directory" +
                      "\n  • Stores both staged and unstaged changes" +
                      "\n  • Does NOT include untracked files by default" +
                      "\n  • Use [cyan]git stash -u[/] to include untracked files" +
                      "\n  • Working directory becomes clean after stashing" +
                      "\n\n[cyan]git stash list[/] - Shows all stashed changes" +
                      "\n  • Each stash is numbered (stash@{0}, stash@{1}, etc.)" +
                      "\n  • Most recent stash is stash@{0}" +
                      "\n\n[cyan]git stash pop[/] - Restores the most recent stash and removes it from the stash list" +
                      "\n  • Applies stash@{0} by default" +
                      "\n  • Removes the stash after applying" +
                      "\n  • Use 'git stash apply' to keep the stash in the list" +
                      "\n\n[cyan]git stash drop[/] - Deletes a stash without applying it" +
                      "\n\n[dim]Think of stash as a clipboard for your work in progress![/]" +
                      "\n\n[yellow]To complete this challenge:[/]" +
                      "\n  1. Check your current changes: [cyan]git status[/]" +
                      "\n  2. View the diff: [cyan]git diff[/]" +
                      "\n  3. Stash your work INCLUDING untracked files: [cyan]git stash -u[/]",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-11" } },
            isStartRoom: false,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}
