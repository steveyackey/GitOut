using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;

namespace GitOut.Infrastructure.Persistence.RoomFactories;

public class Room06MergeNexusFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room06MergeNexusFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        var challenge = new RepositoryChallenge(
            id: "merge-nexus-challenge",
            description: "Create multiple spells on the feature branch and merge them into main",
            gitExecutor: _gitExecutor,
            requireGitInit: true,
            customSetup: async (workingDir, gitExec) =>
            {
                // Initialize repo
                await gitExec.ExecuteAsync("init", workingDir);

                // Configure git user (required for commits)
                await gitExec.ExecuteAsync("config user.email \"wizard@gitout.com\"", workingDir);
                await gitExec.ExecuteAsync("config user.name \"Wizard Player\"", workingDir);

                // Create initial commit on main
                await File.WriteAllTextAsync(Path.Combine(workingDir, "grimoire.txt"), "Ancient Grimoire - Main Branch");
                await gitExec.ExecuteAsync("add grimoire.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Initialize grimoire\"", workingDir);

                // Rename default branch to main (for CI compatibility where default might be master)
                await gitExec.ExecuteAsync("branch -M main", workingDir);

                // Create and switch to my-feature branch
                await gitExec.ExecuteAsync("checkout -b my-feature", workingDir);

                // Create the three spell files (unstaged, so player needs to stage them)
                await File.WriteAllTextAsync(Path.Combine(workingDir, "fireball.txt"), "Fireball - A blazing sphere of flame");
                await File.WriteAllTextAsync(Path.Combine(workingDir, "icebolt.txt"), "Icebolt - A freezing shard of ice");
                await File.WriteAllTextAsync(Path.Combine(workingDir, "lightning.txt"), "Lightning - A crackling bolt of electricity");

                // Switch back to main so player starts there
                await gitExec.ExecuteAsync("checkout main", workingDir);
            },
            customValidator: async (workingDir, gitExec) =>
            {
                // Check if we're on main branch
                var branchResult = await gitExec.ExecuteAsync("branch", workingDir);
                if (!branchResult.Output.Contains("* main") && !branchResult.Output.Contains("*main"))
                {
                    return new ChallengeResult(
                        false,
                        "You must be on the main branch to complete this challenge.",
                        "Switch to main with 'git checkout main' or 'git switch main'"
                    );
                }

                // Check if the spell files exist on main (indicating merge happened)
                var fireball = Path.Combine(workingDir, "fireball.txt");
                var icebolt = Path.Combine(workingDir, "icebolt.txt");
                var lightning = Path.Combine(workingDir, "lightning.txt");

                if (File.Exists(fireball) && File.Exists(icebolt) && File.Exists(lightning))
                {
                    // Verify they were actually committed (not just created)
                    var logResult = await gitExec.ExecuteAsync("log --all --oneline", workingDir);
                    if (logResult.Output.Contains("spell") || logResult.Output.Contains("Spell"))
                    {
                        return new ChallengeResult(
                            true,
                            "The paths have converged! You've successfully merged the three powerful spells from the feature branch into the main timeline! " +
                            "The parallel universes are now unified, and your work is preserved.",
                            null
                        );
                    }
                }

                // Check if they've at least created files on the feature branch
                var currentBranch = await gitExec.ExecuteAsync("branch --show-current", workingDir);
                if (currentBranch.Output.Trim() == "my-feature")
                {
                    return new ChallengeResult(
                        false,
                        "You're on the my-feature branch. You need to switch back to main and merge.",
                        "After committing your spells, use 'git checkout main' then 'git merge my-feature'"
                    );
                }

                return new ChallengeResult(
                    false,
                    "The spells have not been merged into the main branch yet.",
                    "Follow all the steps: switch to my-feature, create 3 spell files, stage all with 'git add .', commit, switch to main, merge my-feature"
                );
            }
        );

        var room = new Room(
            id: "room-6",
            name: "The Merge Nexus",
            description: "A convergence point where multiple timelines meet",
            narrative: "You stand at a nexus where two glowing paths converge. Energy crackles in the air as separate timelines " +
                      "shimmer before you. The grimoire rests on the main pedestal, but a parallel timeline beckons - the 'my-feature' branch. " +
                      "\n\nAn ancient tome materializes: 'To master git, you must learn to work in parallel timelines. " +
                      "Create your work on a feature branch, then merge it back when ready.' " +
                      "\n\nYou are currently on 'main'. A 'my-feature' branch exists with three powerful spell scrolls waiting to be preserved!" +
                      "\n\n[yellow]═══ Command Guide ═══[/]" +
                      "\n[cyan]git add .[/] - Stages ALL modified and new files in the current directory" +
                      "\n  • The '.' means 'current directory and everything in it'" +
                      "\n  • Much faster than adding files one by one" +
                      "\n  • Most common way to stage changes in real projects" +
                      "\n  • Alternative: 'git add <file>' to stage specific files" +
                      "\n\n[cyan]git checkout <branch>[/] - Switches to a different branch" +
                      "\n  • Changes your working directory to match that branch's state" +
                      "\n  • Modern alternative: 'git switch <branch>'" +
                      "\n\n[cyan]git merge <branch>[/] - Combines another branch into your current branch" +
                      "\n  • Takes commits from the specified branch and adds them to your current branch" +
                      "\n  • Must be on the branch you want to merge INTO (usually main)" +
                      "\n  • Creates a unified history from two separate lines of development" +
                      "\n\n[yellow]Complete these steps:[/]" +
                      "\n  1. Switch to the my-feature branch: [cyan]git checkout my-feature[/] (or [cyan]git switch my-feature[/])" +
                      "\n  2. Check the status to see the spell files: [cyan]git status[/]" +
                      "\n  3. Stage ALL the spell files at once: [cyan]git add .[/]" +
                      "\n  4. Commit the spells: [cyan]git commit -m \"Add three combat spells\"[/]" +
                      "\n  5. Switch back to main: [cyan]git checkout main[/]" +
                      "\n  6. Merge the my-feature branch: [cyan]git merge my-feature[/]",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-7" } },
            isStartRoom: false,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}
