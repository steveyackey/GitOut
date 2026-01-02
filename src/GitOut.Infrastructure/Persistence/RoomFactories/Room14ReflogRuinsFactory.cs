using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;

namespace GitOut.Infrastructure.Persistence.RoomFactories;

public class Room14ReflogRuinsFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room14ReflogRuinsFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        var challenge = new RepositoryChallenge(
            id: "reflog-ruins-challenge",
            description: "Use git reflog to recover a lost commit",
            gitExecutor: _gitExecutor,
            requireGitInit: true,
            customSetup: async (workingDir, gitExec) =>
            {
                // Initialize repo
                await gitExec.ExecuteAsync("init", workingDir);
                await gitExec.ExecuteAsync("config user.email \"adventurer@gitout.com\"", workingDir);
                await gitExec.ExecuteAsync("config user.name \"Adventurer\"", workingDir);

                // Create several commits
                await File.WriteAllTextAsync(Path.Combine(workingDir, "artifact.txt"), "Ancient artifact found");
                await gitExec.ExecuteAsync("add artifact.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Found the artifact\"", workingDir);

                await File.WriteAllTextAsync(Path.Combine(workingDir, "notes.txt"), "Important discovery notes");
                await gitExec.ExecuteAsync("add notes.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Important notes about discovery\"", workingDir);

                // Save this commit hash - it's the one we'll "lose"
                var importantCommit = await gitExec.ExecuteAsync("rev-parse HEAD", workingDir);
                var lostCommitHash = importantCommit.Output.Trim().Substring(0, 7);

                // Create a file with the lost commit info
                await File.WriteAllTextAsync(Path.Combine(workingDir, "LOST_COMMIT_INFO.txt"),
                    $"The lost commit hash starts with: {lostCommitHash}\nUse 'git reflog' to find it and restore it with 'git reset --hard <hash>'");
                await gitExec.ExecuteAsync("add LOST_COMMIT_INFO.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Info about lost commit\"", workingDir);

                // Now "lose" the important commit by resetting
                await gitExec.ExecuteAsync("reset --hard HEAD~2", workingDir);
                // The notes commit is now "lost" but still in reflog
            },
            customValidator: async (workingDir, gitExec) =>
            {
                // Check if notes.txt exists (it would only exist if they recovered the commit)
                var notesPath = Path.Combine(workingDir, "notes.txt");
                if (File.Exists(notesPath))
                {
                    var content = await File.ReadAllTextAsync(notesPath);
                    if (content.Contains("Important discovery"))
                    {
                        return new ChallengeResult(
                            true,
                            "The lost commit has been recovered from the ruins! You've successfully used reflog to find and restore the \"lost\" commit. Nothing is truly lost in git - the reflog remembers everything for 90 days, making it your ultimate safety net.",
                            null
                        );
                    }
                }

                return new ChallengeResult(
                    false,
                    "The lost commit has not been recovered yet.",
                    "Use 'git reflog' to see all recent HEAD movements, find the lost commit, then 'git reset --hard <commit-hash>' to restore it"
                );
            }
        );

        var room = new Room(
            id: "room-14",
            name: "The Reflog Ruins",
            description: "Ancient ruins that remember everything",
            narrative: "You enter crumbling ruins where time seems fluid. The walls shimmer with ghostly images of past commits, " +
                      "even those that seem to have vanished from history. " +
                      "\n\nA spectral archivist materializes: 'Many believe commits can be lost forever with a reset or rebase. " +
                      "But git remembers ALL movements of HEAD in the reflog - a safety net that tracks every change for 90 days.' " +
                      "\n\nYou notice that an important commit with discovery notes has been lost! It was reset away, but the reflog " +
                      "still remembers it. Check LOST_COMMIT_INFO.txt for clues about the lost commit." +
                      "\n\n[yellow]═══ Command Guide ═══[/]" +
                      "\n[cyan]git reflog[/] - Shows the history of HEAD movements" +
                      "\n  • Records every time HEAD moves (commits, checkouts, resets, etc.)" +
                      "\n  • Each entry shows: commit hash, HEAD position, and action description" +
                      "\n  • Git keeps reflog entries for 90 days by default" +
                      "\n  • Your safety net for recovering \"lost\" commits" +
                      "\n\n[cyan]git reset --hard <commit>[/] - Moves HEAD and updates working directory" +
                      "\n  • Resets your branch to a specific commit" +
                      "\n  • --hard updates both staging area AND working directory" +
                      "\n  • Useful for recovering lost commits found in reflog" +
                      "\n  • WARNING: Discards uncommitted changes!" +
                      "\n\n[cyan]Common reflog uses:[/]" +
                      "\n  • Recovering commits after accidental reset" +
                      "\n  • Finding commits after rebasing" +
                      "\n  • Undoing a bad merge" +
                      "\n  • Seeing what you were working on yesterday" +
                      "\n\n[cyan]Reflog vs Log:[/]" +
                      "\n  • [cyan]git log:[/] Shows commit history (the story git tells)" +
                      "\n  • [cyan]git reflog:[/] Shows HEAD history (the story of what you did)" +
                      "\n\n[green]Remember:[/] As long as you committed it, reflog can help you find it!" +
                      "\n\n[dim]Think of reflog as git's \"undo history\"![/]" +
                      "\n\n[yellow]To complete this challenge:[/]" +
                      "\n  1. View the reflog: [cyan]git reflog[/]" +
                      "\n  2. Find the commit with 'Important notes about discovery' in reflog" +
                      "\n  3. Note its commit hash (shown on the left)" +
                      "\n  4. Restore it: [cyan]git reset --hard <commit-hash>[/]" +
                      "\n  5. The lost commit with notes.txt will be restored!",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-15" } },
            isStartRoom: false,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}
