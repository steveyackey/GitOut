using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;

namespace GitOut.Infrastructure.Persistence.RoomFactories;

public class Room11CherryPickGardenFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room11CherryPickGardenFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        var challenge = new RepositoryChallenge(
            id: "cherry-pick-garden-challenge",
            description: "Use cherry-pick to selectively apply a commit from another branch",
            gitExecutor: _gitExecutor,
            requireGitInit: true,
            customSetup: async (workingDir, gitExec) =>
            {
                // Initialize repo
                await gitExec.ExecuteAsync("init", workingDir);
                await gitExec.ExecuteAsync("config user.email \"adventurer@gitout.com\"", workingDir);
                await gitExec.ExecuteAsync("config user.name \"Adventurer\"", workingDir);

                // Create initial commit on main
                await File.WriteAllTextAsync(Path.Combine(workingDir, "garden.txt"),
                    "Rose: Red\nTulip: Yellow");
                await gitExec.ExecuteAsync("add garden.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Initial garden\"", workingDir);

                // Rename default branch to main (for CI compatibility where default might be master)
                await gitExec.ExecuteAsync("branch -M main", workingDir);

                // Create feature branch with multiple commits
                await gitExec.ExecuteAsync("checkout -b experimental-flowers", workingDir);

                // First commit - the one we want to cherry-pick
                await File.WriteAllTextAsync(Path.Combine(workingDir, "garden.txt"),
                    "Rose: Red\nTulip: Yellow\nLavender: Purple");
                await gitExec.ExecuteAsync("add garden.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Add lavender flower\"", workingDir);

                // Store the commit hash we want to cherry-pick
                var lavenderCommit = await gitExec.ExecuteAsync("rev-parse HEAD", workingDir);
                var commitHash = lavenderCommit.Output.Trim();

                // Write the commit hash to a file for reference
                await File.WriteAllTextAsync(Path.Combine(workingDir, "CHERRY_PICK_THIS.txt"),
                    $"Cherry-pick this commit: {commitHash.Substring(0, 7)}");

                // Second commit - we DON'T want this one
                await File.WriteAllTextAsync(Path.Combine(workingDir, "garden.txt"),
                    "Rose: Red\nTulip: Yellow\nLavender: Purple\nPoisonIvy: Green");
                await gitExec.ExecuteAsync("add garden.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Add poison ivy (dangerous!)\"", workingDir);

                // Switch back to main
                await gitExec.ExecuteAsync("checkout main", workingDir);

                // Add the reference file to main as well
                await gitExec.ExecuteAsync("checkout experimental-flowers CHERRY_PICK_THIS.txt", workingDir);
                await gitExec.ExecuteAsync("add CHERRY_PICK_THIS.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Add reference file\"", workingDir);
            },
            customValidator: async (workingDir, gitExec) =>
            {
                // Check if lavender is in the file
                var gardenPath = Path.Combine(workingDir, "garden.txt");
                if (File.Exists(gardenPath))
                {
                    var content = await File.ReadAllTextAsync(gardenPath);
                    bool hasLavender = content.Contains("Lavender");
                    bool hasPoisonIvy = content.Contains("PoisonIvy");

                    if (hasLavender && !hasPoisonIvy)
                    {
                        // Check we're on main branch
                        var currentBranch = await gitExec.GetCurrentBranchAsync(workingDir);
                        if (currentBranch == "main")
                        {
                            return new ChallengeResult(
                                true,
                                "Perfect! You've cherry-picked only the lavender commit, leaving the dangerous poison ivy behind! " +
                                "This is the power of selective commit application.",
                                null
                            );
                        }
                    }

                    if (hasPoisonIvy)
                    {
                        return new ChallengeResult(
                            false,
                            "The poison ivy has spread to the garden! You picked the wrong commit.",
                            "You need to cherry-pick only the lavender commit. Check CHERRY_PICK_THIS.txt for the commit hash, then use 'git cherry-pick <hash>'"
                        );
                    }
                }

                return new ChallengeResult(
                    false,
                    "The lavender flower has not appeared in the main garden yet.",
                    "Read CHERRY_PICK_THIS.txt for the commit hash, then use 'git cherry-pick <hash>' while on the main branch"
                );
            }
        );

        var room = new Room(
            id: "room-11",
            name: "The Cherry-Pick Garden",
            description: "A garden where you can selectively cultivate commits",
            narrative: "You step into a beautiful garden filled with magical flowers. Two separate plots lie before you: " +
                      "the main garden and an experimental garden (the 'experimental-flowers' branch). " +
                      "\n\nThe head gardener explains: 'Sometimes you want just ONE specific change from another branch, " +
                      "not the entire branch. Cherry-pick lets you select a single commit and apply it to your current branch.' " +
                      "\n\nIn the experimental garden, someone added a beautiful lavender flower AND a dangerous poison ivy plant. " +
                      "You want the lavender, but NOT the poison ivy! " +
                      "\n\nA scroll labeled 'CHERRY_PICK_THIS.txt' contains the commit hash for the lavender flower." +
                      "\n\n[yellow]═══ Understanding Git Hashes (Commit IDs) ═══[/]" +
                      "\n[cyan]What is a commit hash?[/]" +
                      "\n  • A unique identifier for each commit (like a fingerprint)" +
                      "\n  • Example: [dim]a1b2c3d[/] or full version [dim]a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0[/]" +
                      "\n  • Generated using SHA-1 cryptographic algorithm based on commit content" +
                      "\n  • Each commit has a completely unique hash - no two commits share the same hash" +
                      "\n\n[cyan]Why are hashes important?[/]" +
                      "\n  • [yellow]Precise identification:[/] Let you reference a specific commit exactly" +
                      "\n  • [yellow]Integrity:[/] Hash changes if commit content changes - detects tampering" +
                      "\n  • [yellow]Navigation:[/] Use hashes to checkout, cherry-pick, reset, or reference commits" +
                      "\n  • [yellow]Universal:[/] Same hash means identical commit, even across different repos" +
                      "\n\n[cyan]How to find commit hashes:[/]" +
                      "\n  • [cyan]git log[/] - Shows full hashes and commit messages" +
                      "\n  • [cyan]git log --oneline[/] - Shows shortened hashes (first 7 characters)" +
                      "\n  • [cyan]git log <branch> --oneline[/] - View hashes on a specific branch" +
                      "\n  • Hashes appear on the left side of the log output" +
                      "\n\n[cyan]Short vs. Full hashes:[/]" +
                      "\n  • Full hash: 40 characters (e.g., a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0)" +
                      "\n  • Short hash: [yellow]The first 7-8 characters of the full hash[/] (e.g., a1b2c3d)" +
                      "\n  • Example: If full hash is [dim]a1b2c3d4e5f6...[/], short hash is [dim]a1b2c3d[/] (first 7 chars)" +
                      "\n  • Git truncates from the beginning/prefix - not random characters!" +
                      "\n  • Git accepts short hashes in commands as long as they're unique in the repo" +
                      "\n  • Most git commands show short hashes by default (--oneline)" +
                      "\n\n[yellow]To complete this challenge:[/]" +
                      "\n  1. Make sure you're on main: [cyan]git branch[/] (should show * main)" +
                      "\n  2. View the experimental branch: [cyan]git log experimental-flowers --oneline[/]" +
                      "\n  3. Find the 'Add lavender flower' commit [yellow]hash[/] from the log" +
                      "\n  4. Cherry-pick using the hash: [cyan]git cherry-pick <commit-hash>[/]" +
                      "\n     (You can use the short 7-character hash from --oneline)" +
                      "\n\n[yellow]═══ Cherry-Pick Command Guide ═══[/]" +
                      "\n[cyan]git cherry-pick <commit-hash>[/] - Applies a specific commit to your current branch" +
                      "\n  • Takes ONE commit (identified by its hash) and applies it to current branch" +
                      "\n  • Creates a new commit with the same changes (but a different hash!)" +
                      "\n  • Useful for pulling bug fixes from one branch to another" +
                      "\n  • Can cherry-pick from any branch, even if you don't want to merge the whole branch" +
                      "\n\n[cyan]When to use cherry-pick:[/]" +
                      "\n  • You need a specific bug fix from a feature branch" +
                      "\n  • You want one commit but not the whole branch" +
                      "\n  • You need to backport a fix to an older version" +
                      "\n\n[dim]Think of cherry-pick as copy-paste for commits - the hash tells git exactly which commit to copy![/]",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-12" } },
            isStartRoom: false,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}
