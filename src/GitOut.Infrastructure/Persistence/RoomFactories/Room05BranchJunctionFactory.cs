using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;

namespace GitOut.Infrastructure.Persistence.RoomFactories;

public class Room05BranchJunctionFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room05BranchJunctionFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        var challenge = new RepositoryChallenge(
            id: "branch-junction-challenge",
            description: "Create a new branch called 'feature-branch' and switch to it",
            gitExecutor: _gitExecutor,
            requireGitInit: true,
            customSetup: async (workingDir, gitExec) =>
            {
                // Initialize repo with initial commit
                await gitExec.ExecuteAsync("init", workingDir);
                await gitExec.ExecuteAsync("config user.email \"adventurer@gitout.game\"", workingDir);
                await gitExec.ExecuteAsync("config user.name \"Adventurer\"", workingDir);
                await File.WriteAllTextAsync(Path.Combine(workingDir, "main.txt"), "Main branch file");
                await gitExec.ExecuteAsync("add main.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Initial commit on main\"", workingDir);
            },
            customValidator: async (workingDir, gitExec) =>
            {
                // Check if the feature-branch exists
                var branchListResult = await gitExec.ExecuteAsync("branch --list feature-branch", workingDir);

                if (!branchListResult.Success || !branchListResult.Output.Contains("feature-branch"))
                {
                    return new ChallengeResult(
                        false,
                        "The branch has not been created yet.",
                        "Try 'git branch feature-branch' then 'git switch feature-branch', or use 'git checkout -b feature-branch' to do both at once"
                    );
                }

                // Check if the player is currently on the feature-branch
                var currentBranchResult = await gitExec.ExecuteAsync("branch --show-current", workingDir);

                if (currentBranchResult.Success && currentBranchResult.Output.Trim() == "feature-branch")
                {
                    return new ChallengeResult(
                        true,
                        "The timeline shimmers and stabilizes! You've successfully created AND switched to the 'feature-branch'. " +
                        "A new parallel path now exists, and you are now working within it! This allows you to experiment safely without affecting the main timeline. " +
                        "The mystical voice whispers: 'You now understand the power of branches - the foundation of collaborative development!'",
                        null
                    );
                }

                return new ChallengeResult(
                    false,
                    "The 'feature-branch' exists, but you are not currently on it.",
                    "Switch to the branch using 'git switch feature-branch' or 'git checkout feature-branch'"
                );
            }
        );

        var room = new Room(
            id: "room-5",
            name: "The Branch Junction",
            description: "A corridor that splits into multiple paths",
            narrative: "You arrive at a junction where the path diverges. The walls shimmer, showing visions of different timelines. " +
                      "A mystical voice echoes: 'In the realm of version control, one path need not be the only path. " +
                      "Branches allow you to explore new ideas safely, without disturbing the main timeline. " +
                      "Create a branch called \"feature-branch\" AND step into that timeline to prove you understand this fundamental concept.' " +
                      "\n\nBranches let you work on new features, experiments, or bug fixes in isolation." +
                      "\n\n[yellow]═══ Understanding HEAD ═══[/]" +
                      "\n[cyan]HEAD[/] is git's way of saying \"you are here.\" It's a special pointer that tells git which commit you're currently working on." +
                      "\n  • When you're on a branch, HEAD points to that branch" +
                      "\n  • Switching branches moves HEAD to the new branch" +
                      "\n  • [cyan]git branch[/] shows [green]*[/] next to the branch HEAD points to" +
                      "\n  • Many commands use HEAD as a reference (e.g., [cyan]HEAD~1[/] means \"one commit before HEAD\")" +
                      "\n  • Think of HEAD as a bookmark showing your current position in the project's history" +
                      "\n\n[yellow]═══ Command Guide ═══[/]" +
                      "\n[cyan]git branch <name>[/] - Creates a new branch (but doesn't switch to it)" +
                      "\n  • A branch is a separate line of development" +
                      "\n  • Lets you work on features without affecting the main code" +
                      "\n  • [red]Important:[/] Just creates the branch; HEAD stays on your current branch!" +
                      "\n  • Must use 'git switch' or 'git checkout' to move HEAD to the new branch" +
                      "\n  • Use 'git branch' (no arguments) to list all branches (* shows where HEAD is)" +
                      "\n\n[cyan]git switch <name>[/] - Moves HEAD to an existing branch (modern command)" +
                      "\n  • Cleaner, more intuitive than 'git checkout'" +
                      "\n  • Introduced in Git 2.23 to separate branch and file operations" +
                      "\n  • Use 'git switch -c <name>' to create and switch in one step" +
                      "\n\n[cyan]git checkout -b <name>[/] - Creates a new branch AND moves HEAD to it" +
                      "\n  • [green]Recommended:[/] Most efficient way to create and start using a branch" +
                      "\n  • Combines 'git branch' and 'git checkout' into one command" +
                      "\n  • Still widely used and perfectly valid" +
                      "\n  • Modern equivalent: 'git switch -c <name>'" +
                      "\n\n[yellow]To complete this challenge:[/]" +
                      "\n  [green]Option 1 - Two-step approach:[/]" +
                      "\n    1. Create a new branch: [cyan]git branch feature-branch[/]" +
                      "\n    2. Move HEAD to it: [cyan]git switch feature-branch[/]" +
                      "\n\n  [green]Option 2 - One-step approach (recommended):[/]" +
                      "\n    • Create AND switch at once: [cyan]git checkout -b feature-branch[/]" +
                      "\n      (or use modern syntax: [cyan]git switch -c feature-branch[/])",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-6" } },
            isStartRoom: false,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}
