using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;

namespace GitOut.Infrastructure.Persistence.RoomFactories;

public class Room09ConflictCatacombsFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room09ConflictCatacombsFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        var challenge = new RepositoryChallenge(
            id: "conflict-catacombs-challenge",
            description: "Resolve the merge conflict between two branches",
            gitExecutor: _gitExecutor,
            requireGitInit: true,
            customSetup: async (workingDir, gitExec) =>
            {
                // Initialize repo
                await gitExec.ExecuteAsync("init", workingDir);
                await gitExec.ExecuteAsync("config user.email \"adventurer@gitout.com\"", workingDir);
                await gitExec.ExecuteAsync("config user.name \"Adventurer\"", workingDir);

                // Create initial commit on main
                await File.WriteAllTextAsync(Path.Combine(workingDir, "spell-book.txt"),
                    "Chapter 1: Fire Magic\nChapter 2: Water Magic\nChapter 3: Earth Magic");
                await gitExec.ExecuteAsync("add spell-book.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Initial spell book\"", workingDir);

                // Create branch and modify file
                await gitExec.ExecuteAsync("checkout -b arcane-updates", workingDir);
                await File.WriteAllTextAsync(Path.Combine(workingDir, "spell-book.txt"),
                    "Chapter 1: Advanced Fire Magic\nChapter 2: Water Magic\nChapter 3: Earth Magic");
                await gitExec.ExecuteAsync("add spell-book.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Update fire magic chapter\"", workingDir);

                // Switch back to main and make conflicting change
                await gitExec.ExecuteAsync("checkout main", workingDir);
                await File.WriteAllTextAsync(Path.Combine(workingDir, "spell-book.txt"),
                    "Chapter 1: Elemental Fire Magic\nChapter 2: Water Magic\nChapter 3: Earth Magic");
                await gitExec.ExecuteAsync("add spell-book.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Revise fire magic title\"", workingDir);

                // Attempt merge to create conflict (this will fail, that's expected)
                await gitExec.ExecuteAsync("merge arcane-updates", workingDir);
            },
            customValidator: async (workingDir, gitExec) =>
            {
                // Check if conflicts are resolved
                var hasConflicts = await gitExec.HasConflictsAsync(workingDir);
                if (hasConflicts)
                {
                    return new ChallengeResult(
                        false,
                        "The magical energies still clash! Conflicts remain unresolved.",
                        "Use 'git checkout --ours spell-book.txt' or 'git checkout --theirs spell-book.txt' to choose a version, then 'git add spell-book.txt' and 'git commit --no-edit'"
                    );
                }

                // Check if merge was completed
                var log = await gitExec.GetLogAsync(workingDir, 5);
                if (log.Contains("Merge") || log.Contains("merge"))
                {
                    return new ChallengeResult(
                        true,
                        "The conflicting energies have been harmonized! You've successfully resolved the merge conflict and unified the timelines!",
                        null
                    );
                }

                return new ChallengeResult(
                    false,
                    "The conflict has been resolved, but you haven't completed the merge.",
                    "After resolving conflicts and staging changes, complete the merge with 'git commit --no-edit'"
                );
            }
        );

        var room = new Room(
            id: "room-9",
            name: "The Conflict Catacombs",
            description: "A chamber where two magical forces collide",
            narrative: "You descend into the catacombs, where the air crackles with opposing magical energies. " +
                      "Two timelines have collided here - both attempting to modify the same ancient spell book. " +
                      "The arcane-updates branch and the main branch have diverged, and now they clash! " +
                      "\n\nA ghostly librarian appears: 'When parallel timelines modify the same text, git cannot " +
                      "automatically decide which version to keep. This is a MERGE CONFLICT - one of the most important " +
                      "skills to master!' " +
                      "\n\nThe merge has already been attempted and failed. The spell-book.txt file now contains conflict markers." +
                      "\n\n[yellow]To resolve this conflict:[/]" +
                      "\n  1. Check the status: [cyan]git status[/]" +
                      "\n  2. View the conflict in the file: [cyan]git diff spell-book.txt[/]" +
                      "\n  3. For simple conflicts, git provides shortcuts to choose a version:" +
                      "\n     [cyan]git checkout --ours spell-book.txt[/]   (keep main's version: 'Elemental Fire Magic')" +
                      "\n     [cyan]git checkout --theirs spell-book.txt[/] (keep branch's version: 'Advanced Fire Magic')" +
                      "\n  4. Choose either command above to resolve the conflict" +
                      "\n  5. Stage the resolved file: [cyan]git add spell-book.txt[/]" +
                      "\n  6. Complete the merge: [cyan]git commit --no-edit[/] (accept git's auto-generated merge message)" +
                      "\n\n[yellow]═══ Understanding Merge Conflicts ═══[/]" +
                      "\n[cyan]What is a merge conflict?[/]" +
                      "\n  • Happens when two branches modify the same line(s) of a file differently" +
                      "\n  • Git can't automatically decide which version to keep" +
                      "\n  • YOU must manually choose or combine the changes" +
                      "\n\n[cyan]Viewing the conflict:[/]" +
                      "\n  • Use [cyan]git diff spell-book.txt[/] to see the conflicting changes" +
                      "\n  • You'll see conflict markers showing both versions" +
                      "\n\n[cyan]What conflict markers look like:[/]" +
                      "\n  • [red]<<<<<<< HEAD[/] marks the start of YOUR current branch's version" +
                      "\n  • [red]=======[/] divides the two conflicting versions" +
                      "\n  • [red]>>>>>>> branch-name[/] marks the end of the INCOMING branch's version" +
                      "\n  • Everything between [red]<<<<<<< HEAD[/] and [red]=======[/] is your version" +
                      "\n  • Everything between [red]=======[/] and [red]>>>>>>> branch-name[/] is their version" +
                      "\n\n[cyan]Git's Conflict Resolution Shortcuts:[/]" +
                      "\n  • [cyan]git checkout --ours <file>[/] - Keep your current branch's version (HEAD)" +
                      "\n  • [cyan]git checkout --theirs <file>[/] - Keep the incoming branch's version" +
                      "\n  • These commands are perfect for simple 'pick one version' conflicts" +
                      "\n  • For complex conflicts requiring manual merge, you'd edit the file directly" +
                      "\n  • In this game, use --ours or --theirs to resolve conflicts quickly" +
                      "\n  • After choosing, stage with 'git add <file>' and commit",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-10" } },
            isStartRoom: false,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}
