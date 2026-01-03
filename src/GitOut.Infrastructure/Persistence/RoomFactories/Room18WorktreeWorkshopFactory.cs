using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;

namespace GitOut.Infrastructure.Persistence.RoomFactories;

public class Room18WorktreeWorkshopFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room18WorktreeWorkshopFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        var challenge = new RepositoryChallenge(
            id: "worktree-workshop-challenge",
            description: "Use git worktree to create an additional working tree for a different branch",
            gitExecutor: _gitExecutor,
            requireGitInit: true,
            customSetup: async (workingDir, gitExec) =>
            {
                // Initialize repo
                await gitExec.ExecuteAsync("init", workingDir);
                await gitExec.ExecuteAsync("config user.email \"adventurer@gitout.com\"", workingDir);
                await gitExec.ExecuteAsync("config user.name \"Adventurer\"", workingDir);

                // Create initial commit on main
                await File.WriteAllTextAsync(Path.Combine(workingDir, "README.md"),
                    "# Project Documentation\n\nMain branch development.\n");
                await gitExec.ExecuteAsync("add README.md", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Initial commit\"", workingDir);

                // Create instructions file
                await File.WriteAllTextAsync(Path.Combine(workingDir, "WORKTREE_GUIDE.txt"),
                    "=== Git Worktree Challenge ===\n\n" +
                    "Problem: You're working on the main branch, but need to quickly switch to a feature\n" +
                    "branch to fix a bug. Switching branches would disrupt your current work.\n\n" +
                    "Solution: Git worktrees allow multiple branches to be checked out simultaneously\n" +
                    "in different directories!\n\n" +
                    "Your Task:\n" +
                    "1. Create a new worktree in a parallel directory (e.g., ../feature-work)\n" +
                    "2. The worktree should check out a new branch (e.g., 'feature' or 'bugfix')\n\n" +
                    "Example Commands:\n" +
                    "  git worktree add ../feature-work feature\n" +
                    "    - Creates new directory '../feature-work'\n" +
                    "    - Creates and checks out new branch 'feature' in that directory\n" +
                    "    - Main directory stays on current branch\n\n" +
                    "  git worktree add ../bugfix-123 -b bugfix-123\n" +
                    "    - Explicitly names the new branch with -b flag\n\n" +
                    "  git worktree add ../hotfix HEAD~1\n" +
                    "    - Creates worktree at specific commit\n\n" +
                    "Verification:\n" +
                    "  git worktree list\n" +
                    "    - Shows all worktrees and their branches\n\n" +
                    "Note: The challenge will pass when 'git worktree list' shows at least 2 worktrees.\n");
            },
            customValidator: async (workingDir, gitExec) =>
            {
                // Get worktree list
                var worktreeResult = await gitExec.ExecuteAsync("worktree list", workingDir);
                if (!worktreeResult.Success)
                {
                    return new ChallengeResult(
                        false,
                        "Could not retrieve worktree list. Ensure the repository is initialized.",
                        "Run 'git worktree list' to check the current state."
                    );
                }

                // Parse worktree list output
                // Format: <path> <commit> [<branch>]
                // Example output:
                // /path/to/main    abc123 [main]
                // /path/to/feature def456 [feature]
                var worktreeLines = worktreeResult.Output?
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .ToList() ?? new List<string>();

                if (worktreeLines.Count < 2)
                {
                    return new ChallengeResult(
                        false,
                        $"Only {worktreeLines.Count} worktree(s) found. You need at least 2 (main + one additional).",
                        "Use 'git worktree add <path> <branch>' to create an additional worktree. For example: 'git worktree add ../feature-work feature'"
                    );
                }

                // Success! Multiple worktrees exist
                return new ChallengeResult(
                    true,
                    $"Excellent! You've created {worktreeLines.Count} worktrees! Git worktrees are incredibly useful for working on multiple " +
                    "branches simultaneously without the overhead of cloning the repository multiple times or constantly switching branches. " +
                    "This is perfect for scenarios like: working on a feature while being able to quickly switch to a hotfix, running tests " +
                    "on one branch while developing on another, or comparing different implementations side-by-side. Each worktree has its own " +
                    "working directory and staging area, but they all share the same repository history and objects, saving disk space. " +
                    "Use 'git worktree remove <path>' when you're done with a worktree, or 'git worktree prune' to clean up deleted ones.",
                    null
                );
            }
        );

        var room = new Room(
            id: "room-18",
            name: "The Worktree Workshop",
            description: "A workshop with multiple workbenches, each for different tasks",
            narrative: "You enter a spacious workshop filled with parallel workbenches. A master craftsman explains: " +
                      "'Sometimes you need to work on multiple tasks at once without losing your current progress. " +
                      "Git worktrees let you check out multiple branches simultaneously in separate directories!'" +
                      "\n\n'Each worktree is like having a separate workspace for a different branch, but they all share " +
                      "the same repository history. No need to stash, switch branches, or clone the repo multiple times.'" +
                      "\n\n[yellow]═══ Command Guide ═══[/]" +
                      "\n[cyan]git worktree add <path> <branch>[/] - Create new worktree" +
                      "\n  • <path>: Directory for the new worktree (can be relative like ../feature)" +
                      "\n  • <branch>: Branch to check out (creates it if it doesn't exist)" +
                      "\n  • Example: git worktree add ../feature-work feature" +
                      "\n\n[cyan]git worktree add <path> -b <new-branch> <start-point>[/]" +
                      "\n  • Explicitly create a new branch" +
                      "\n  • Example: git worktree add ../fix -b bugfix-123 main" +
                      "\n\n[cyan]git worktree list[/] - Show all worktrees" +
                      "\n  • Lists all worktrees with their paths, commits, and branches" +
                      "\n  • Example output:" +
                      "\n    /home/user/project      abc123 [[main]]" +
                      "\n    /home/user/feature-work def456 [[feature]]" +
                      "\n\n[cyan]git worktree remove <path>[/] - Remove a worktree" +
                      "\n  • Deletes the worktree directory and unregisters it" +
                      "\n  • Example: git worktree remove ../feature-work" +
                      "\n\n[cyan]git worktree prune[/] - Clean up worktree information" +
                      "\n  • Removes worktree entries for directories that were deleted manually" +
                      "\n\n[cyan]git worktree lock <path>[/] - Prevent a worktree from being pruned" +
                      "\n  • Useful for worktrees on removable media" +
                      "\n\n[cyan]git worktree unlock <path>[/] - Allow pruning again" +
                      "\n\n[green]Use Cases:[/]" +
                      "\n  • Work on feature while being able to quickly fix production bugs" +
                      "\n  • Run tests on one branch while developing on another" +
                      "\n  • Compare implementations in different branches side-by-side" +
                      "\n  • Review pull requests without disrupting current work" +
                      "\n  • Build/deploy from one branch while working in another" +
                      "\n\n[green]Pro tips:[/]" +
                      "\n  • All worktrees share the same .git directory (saves space)" +
                      "\n  • Each worktree has its own HEAD, index (staging area), and working directory" +
                      "\n  • You cannot check out the same branch in multiple worktrees" +
                      "\n  • Commits made in any worktree affect the shared repository history" +
                      "\n  • Use 'git worktree list' to see which branches are checked out where" +
                      "\n\n[dim]Multiple workspaces, one repository. Work smarter, not harder![/]" +
                      "\n\n[yellow]The Challenge:[/]" +
                      "\n  • Create an additional worktree in a parallel directory" +
                      "\n  • The worktree should check out a new or existing branch" +
                      "\n  • Both the main directory and the new worktree will coexist" +
                      "\n\n[yellow]Your Mission:[/]" +
                      "\n  1. Read WORKTREE_GUIDE.txt for detailed instructions" +
                      "\n  2. Use [cyan]git worktree add <path> <branch>[/] to create a new worktree" +
                      "\n  3. Verify with [cyan]git worktree list[/] that you have 2+ worktrees",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-19" } },
            isStartRoom: false,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}

