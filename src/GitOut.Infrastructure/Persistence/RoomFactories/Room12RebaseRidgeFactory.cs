using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;

namespace GitOut.Infrastructure.Persistence.RoomFactories;

public class Room12RebaseRidgeFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room12RebaseRidgeFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        var challenge = new RepositoryChallenge(
            id: "rebase-ridge-challenge",
            description: "Use git rebase to replay commits on a new base",
            gitExecutor: _gitExecutor,
            requireGitInit: true,
            customSetup: async (workingDir, gitExec) =>
            {
                // Initialize repo
                await gitExec.ExecuteAsync("init", workingDir);
                await gitExec.ExecuteAsync("config user.email \"adventurer@gitout.com\"", workingDir);
                await gitExec.ExecuteAsync("config user.name \"Adventurer\"", workingDir);

                // Create initial commit on main
                await File.WriteAllTextAsync(Path.Combine(workingDir, "timeline.txt"), "Event 1: Beginning");
                await gitExec.ExecuteAsync("add timeline.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Event 1\"", workingDir);

                // Rename default branch to main (for CI compatibility where default might be master)
                await gitExec.ExecuteAsync("branch -M main", workingDir);

                // Create feature branch
                await gitExec.ExecuteAsync("checkout -b feature-timeline", workingDir);
                await File.WriteAllTextAsync(Path.Combine(workingDir, "timeline.txt"), "Event 1: Beginning\nEvent 2: Feature work");
                await gitExec.ExecuteAsync("add timeline.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Event 2 on feature\"", workingDir);

                // Go back to main and add more commits
                await gitExec.ExecuteAsync("checkout main", workingDir);
                await File.WriteAllTextAsync(Path.Combine(workingDir, "timeline.txt"), "Event 1: Beginning\nEvent A: Main progress");
                await gitExec.ExecuteAsync("add timeline.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Event A on main\"", workingDir);

                await File.WriteAllTextAsync(Path.Combine(workingDir, "timeline.txt"), "Event 1: Beginning\nEvent A: Main progress\nEvent B: More main work");
                await gitExec.ExecuteAsync("add timeline.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Event B on main\"", workingDir);

                // Switch back to feature branch for player to rebase
                await gitExec.ExecuteAsync("checkout feature-timeline", workingDir);
            },
            customValidator: async (workingDir, gitExec) =>
            {
                // Check if we're on feature-timeline branch
                var currentBranch = await gitExec.GetCurrentBranchAsync(workingDir);
                if (currentBranch != "feature-timeline")
                {
                    return new ChallengeResult(
                        false,
                        "You must be on the feature-timeline branch to complete this challenge.",
                        "Switch to feature-timeline with 'git checkout feature-timeline'"
                    );
                }

                // Check if rebase happened by looking at the log
                var log = await gitExec.GetLogAsync(workingDir, 10);
                // After rebase, feature branch should have main's commits (Event A, Event B) in its history
                if (log.Contains("Event A") && log.Contains("Event B") && log.Contains("Event 2"))
                {
                    return new ChallengeResult(
                        true,
                        "The timelines have been realigned! You've successfully rebased your feature branch onto the latest main, " +
                        "creating a clean, linear history. Your feature work now appears to have been built on top of the latest main commits!",
                        null
                    );
                }

                return new ChallengeResult(
                    false,
                    "The feature branch has not been rebased yet.",
                    "While on feature-timeline, use 'git rebase main' to replay your feature commits on top of main's latest changes"
                );
            }
        );

        var room = new Room(
            id: "room-12",
            name: "The Rebase Ridge",
            description: "A ridge where timelines can be realigned",
            narrative: "You stand atop a ridge overlooking two diverging paths through time. The main timeline has progressed " +
                      "with new events (Event A and Event B), while your feature-timeline branched off earlier and added Event 2. " +
                      "\n\nA time-weaver appears: 'Merge would combine these timelines with a merge commit, but rebase can do something " +
                      "more elegant - it can replay your feature work as if it was built on the latest main from the start!' " +
                      "\n\nYou are currently on the feature-timeline branch. This branch needs to be updated with the latest changes from main." +
                      "\n\n[yellow]═══ Command Guide ═══[/]" +
                      "\n[cyan]git rebase <branch>[/] - Replays your current branch's commits on top of another branch" +
                      "\n  • Takes your commits and replays them on a new base" +
                      "\n  • Creates a linear history (no merge commits)" +
                      "\n  • Makes it look like you started your work from the latest version" +
                      "\n  • Rewrites commit history (creates new commit hashes)" +
                      "\n\n[cyan]Rebase vs Merge:[/]" +
                      "\n  • [cyan]Merge:[/] Combines branches with a merge commit (preserves exact history)" +
                      "\n  • [cyan]Rebase:[/] Replays commits to create linear history (cleaner, but rewrites commits)" +
                      "\n  • Rebase is great for keeping feature branches up-to-date with main" +
                      "\n  • Never rebase commits that have been pushed and shared with others!" +
                      "\n\n[cyan]When to use rebase:[/]" +
                      "\n  • Updating your feature branch with latest main" +
                      "\n  • Cleaning up commit history before merging" +
                      "\n  • Creating a linear project history" +
                      "\n  • When working on local commits only (not pushed yet)" +
                      "\n\n[red]Golden Rule:[/] Never rebase public/shared commits - only rebase your local unpushed work!" +
                      "\n\n[dim]Think of rebase as \"change the base of my branch\"![/]" +
                      "\n\n[yellow]To complete this challenge:[/]" +
                      "\n  1. View your current commits: [cyan]git log --oneline[/]" +
                      "\n  2. View main's commits: [cyan]git log main --oneline[/]" +
                      "\n  3. Rebase onto main: [cyan]git rebase main[/]" +
                      "\n  4. View the new history: [cyan]git log --oneline[/]",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-13" } },
            isStartRoom: false,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}
