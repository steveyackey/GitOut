using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;

namespace GitOut.Infrastructure.Persistence.RoomFactories;

public class Room22RewriteReliquaryFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room22RewriteReliquaryFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        var challenge = new RepositoryChallenge(
            id: "rewrite-reliquary-challenge",
            description: "Use git filter-branch to remove a sensitive file from all commits in the repository's history",
            gitExecutor: _gitExecutor,
            requireGitInit: true,
            customSetup: async (workingDir, gitExec) =>
            {
                // Initialize repo
                await gitExec.ExecuteAsync("init", workingDir);
                await gitExec.ExecuteAsync("config user.email \"adventurer@gitout.com\"", workingDir);
                await gitExec.ExecuteAsync("config user.name \"Adventurer\"", workingDir);

                // Commit 1: Initial project files
                await File.WriteAllTextAsync(Path.Combine(workingDir, "README.md"),
                    "# Ancient Repository\n\nThis repository contains project files.\n");
                await File.WriteAllTextAsync(Path.Combine(workingDir, "app.js"),
                    "// Main application file\nconsole.log('Application started');\n");
                await gitExec.ExecuteAsync("add README.md app.js", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Initial commit: Add project files\"", workingDir);

                // Commit 2: Accidentally commit secrets file
                await File.WriteAllTextAsync(Path.Combine(workingDir, "secrets.txt"),
                    "API_KEY=sk_live_1234567890abcdef\n" +
                    "DATABASE_PASSWORD=super_secret_password\n" +
                    "SECRET_TOKEN=ghp_supersecrettoken123\n");
                await gitExec.ExecuteAsync("add secrets.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Add configuration files\"", workingDir);

                // Commit 3: Add more legitimate changes
                await File.WriteAllTextAsync(Path.Combine(workingDir, "utils.js"),
                    "// Utility functions\nfunction formatDate(date) {\n    return date.toISOString();\n}\n");
                await gitExec.ExecuteAsync("add utils.js", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Add utility functions\"", workingDir);

                // Commit 4: One more commit after the secret
                await File.WriteAllTextAsync(Path.Combine(workingDir, "config.example.js"),
                    "// Example configuration\nmodule.exports = {\n    apiUrl: 'https://api.example.com'\n};\n");
                await gitExec.ExecuteAsync("add config.example.js", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Add example config\"", workingDir);

                // Create instructions file
                await File.WriteAllTextAsync(Path.Combine(workingDir, "DANGER_INSTRUCTIONS.txt"),
                    "═══════════════════════════════════════════════════════════════\n" +
                    "⚠️  THE REWRITE RELIQUARY - HISTORY REWRITING CHALLENGE  ⚠️\n" +
                    "═══════════════════════════════════════════════════════════════\n\n" +
                    "SCENARIO:\n" +
                    "You've made a terrible mistake. During development, you accidentally\n" +
                    "committed 'secrets.txt' containing API keys, passwords, and tokens.\n" +
                    "Worse, you've made several more commits since then.\n\n" +
                    "Simply deleting the file now won't help - it's still in the git history!\n" +
                    "Anyone who clones this repository can access those secrets from past commits.\n\n" +
                    "CRITICAL WARNINGS:\n" +
                    "⚠️  History rewriting is DANGEROUS and should ONLY be done on:\n" +
                    "   • Local repositories that have NEVER been pushed\n" +
                    "   • Repositories where you have absolute control and can force-push\n" +
                    "   • Commits that have NOT been shared with others\n\n" +
                    "⚠️  NEVER rewrite history that others have based work on!\n" +
                    "   • It will break everyone else's repositories\n" +
                    "   • They'll need to force-pull or re-clone\n" +
                    "   • Ongoing work may be lost or conflict\n\n" +
                    "⚠️  This rewrites ALL commits - commit hashes will CHANGE!\n" +
                    "   • Any references to old commits will break\n" +
                    "   • Pull requests, issue references, documentation will be invalid\n\n" +
                    "YOUR MISSION:\n" +
                    "Remove 'secrets.txt' from ALL commits in the repository history.\n" +
                    "After successful rewriting:\n" +
                    "  • The file should not exist in ANY commit\n" +
                    "  • The file should not appear in git log\n" +
                    "  • All other files should remain intact\n\n" +
                    "THE COMMAND:\n" +
                    "git filter-branch --force --index-filter \\\n" +
                    "  'git rm --cached --ignore-unmatch secrets.txt' \\\n" +
                    "  --prune-empty --tag-name-filter cat -- --all\n\n" +
                    "WHAT THIS DOES:\n" +
                    "  --force                  : Force the operation even if backup exists\n" +
                    "  --index-filter 'CMD'     : Run CMD on the index (staging area) of each commit\n" +
                    "  git rm --cached          : Remove file from index (staging area)\n" +
                    "  --ignore-unmatch         : Don't fail if file doesn't exist in this commit\n" +
                    "  secrets.txt              : The file to remove\n" +
                    "  --prune-empty            : Remove commits that become empty after filtering\n" +
                    "  --tag-name-filter cat    : Rewrite tags to point to new commits\n" +
                    "  -- --all                 : Apply to all branches and tags\n\n" +
                    "ALTERNATIVE APPROACHES:\n" +
                    "1. Use tree-filter (slower but more flexible):\n" +
                    "   git filter-branch --tree-filter 'rm -f secrets.txt' HEAD\n\n" +
                    "2. Use filter-repo (modern, recommended tool - not in core git):\n" +
                    "   git filter-repo --path secrets.txt --invert-paths\n" +
                    "   NOTE: Requires separate installation: pip install git-filter-repo\n\n" +
                    "3. Use BFG Repo-Cleaner (fast, user-friendly):\n" +
                    "   bfg --delete-files secrets.txt\n\n" +
                    "VERIFICATION:\n" +
                    "After running filter-branch, verify the file is gone:\n" +
                    "  git log --all --full-history -- secrets.txt\n" +
                    "  (Should return empty - no commits touching secrets.txt)\n\n" +
                    "  git log --oneline\n" +
                    "  (Check that commits exist but with new hashes)\n\n" +
                    "CLEANUP:\n" +
                    "After successful rewrite, git creates backup refs:\n" +
                    "  .git/refs/original/\n" +
                    "To fully remove the data:\n" +
                    "  rm -rf .git/refs/original/\n" +
                    "  git reflog expire --expire=now --all\n" +
                    "  git gc --prune=now --aggressive\n\n" +
                    "MODERN RECOMMENDATION:\n" +
                    "git filter-branch is deprecated. For production use, prefer:\n" +
                    "  • git filter-repo (faster, safer, more features)\n" +
                    "  • BFG Repo-Cleaner (simpler interface)\n" +
                    "However, filter-branch is still useful for understanding the concepts!\n\n" +
                    "WHEN TO USE THIS:\n" +
                    "  ✓ Remove accidentally committed secrets/passwords\n" +
                    "  ✓ Remove large binary files that bloat repository\n" +
                    "  ✓ Split a monorepo into separate repositories\n" +
                    "  ✓ Change author information across all commits\n" +
                    "  ✗ NEVER on shared/pushed history without team coordination\n\n" +
                    "═══════════════════════════════════════════════════════════════\n" +
                    "Ready to rewrite history? Remember: with great power comes\n" +
                    "great responsibility. Use this knowledge wisely!\n" +
                    "═══════════════════════════════════════════════════════════════\n");
            },
            customValidator: async (workingDir, gitExec) =>
            {
                // Check 1: Verify repository is initialized
                var isRepo = await gitExec.IsGitRepositoryAsync(workingDir);
                if (!isRepo)
                {
                    return new ChallengeResult(
                        false,
                        "Repository is not initialized.",
                        "Ensure you're working in a git repository."
                    );
                }

                // Check 2: Verify secrets.txt doesn't exist in working directory
                var secretsPath = Path.Combine(workingDir, "secrets.txt");
                if (File.Exists(secretsPath))
                {
                    return new ChallengeResult(
                        false,
                        "The secrets.txt file still exists in the working directory. While it might be removed from history, " +
                        "you should also delete it from the current state.",
                        "Delete the file with: rm secrets.txt (or manually delete it)"
                    );
                }

                // Check 3: Verify secrets.txt doesn't appear in CURRENT branch history
                // Note: --all includes backup refs that filter-branch creates, so we check the main branch only
                var logResult = await gitExec.ExecuteAsync("log --oneline HEAD -- secrets.txt", workingDir);

                if (!logResult.Success)
                {
                    return new ChallengeResult(
                        false,
                        "Could not check git history for secrets.txt.",
                        "Run: git log --oneline HEAD -- secrets.txt"
                    );
                }

                // If the output is not empty, the file still exists in history
                var logOutput = logResult.Output?.Trim() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(logOutput))
                {
                    var commitCount = logOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length;
                    return new ChallengeResult(
                        false,
                        $"The secrets.txt file still appears in {commitCount} commit(s) in the repository history! " +
                        "You need to use git filter-branch to remove it from ALL commits.",
                        "Read DANGER_INSTRUCTIONS.txt and use:\n" +
                        "git filter-branch --force --index-filter 'git rm --cached --ignore-unmatch secrets.txt' --prune-empty --tag-name-filter cat -- --all"
                    );
                }

                // Check 4: Verify other files still exist in history
                var readmeLog = await gitExec.ExecuteAsync("log --all --oneline -- README.md", workingDir);
                if (!readmeLog.Success || string.IsNullOrWhiteSpace(readmeLog.Output))
                {
                    return new ChallengeResult(
                        false,
                        "README.md has been removed from history! The filter-branch should only remove secrets.txt, not other files.",
                        "You may have used the wrong filter. Check the command carefully."
                    );
                }

                // Check 5: Verify we still have commits
                var allLog = await gitExec.GetLogAsync(workingDir, maxCount: 10);
                if (string.IsNullOrWhiteSpace(allLog))
                {
                    return new ChallengeResult(
                        false,
                        "No commits found in the repository!",
                        "Something went wrong with the filter-branch operation."
                    );
                }

                var commitLines = allLog.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (commitLines.Length < 3)
                {
                    return new ChallengeResult(
                        false,
                        $"Only {commitLines.Length} commit(s) found. The repository should have at least 3 commits after removing the secrets commit.",
                        "The filter-branch may have removed too many commits. Use --prune-empty carefully."
                    );
                }

                // Success!
                return new ChallengeResult(
                    true,
                    "INCREDIBLE! You've successfully rewritten history! The secrets.txt file has been completely purged from all commits. " +
                    "This is one of git's most powerful and dangerous capabilities. You've learned how to remove sensitive data that was " +
                    "accidentally committed, but remember: this should ONLY be done on local, unshared repositories, or in coordination with " +
                    "your entire team on shared repositories. In production, you'd want to rotate any exposed credentials immediately, even " +
                    "after removing them from history (assume they were already compromised). For modern workflows, consider using 'git filter-repo' " +
                    "or 'BFG Repo-Cleaner' which are faster and safer alternatives to filter-branch. Most importantly: use .gitignore and secret " +
                    "management tools (like environment variables, secret vaults, or .env files) to prevent secrets from being committed in the first place!",
                    null
                );
            }
        );

        var room = new Room(
            id: "room-22",
            name: "The Rewrite Reliquary",
            description: "A forbidden archive where history itself can be rewritten - use with extreme caution",
            narrative: "You enter a dimly lit chamber filled with ancient scrolls and artifacts. At the center stands a mystical altar " +
                      "pulsing with dangerous energy. An ancient keeper appears, their voice grave:\n\n" +
                      "'Welcome to the Rewrite Reliquary, the most dangerous room in this dungeon. Here, you will learn to alter history itself. " +
                      "But heed my warning: this power must be used with EXTREME caution!'\n\n" +
                      "'Sometimes, terrible mistakes happen. Secrets are committed, passwords exposed, massive files bloat the repository. " +
                      "Simply deleting them from the current version isn't enough - they remain in git's history, accessible to anyone who looks.'\n\n" +
                      "'Git filter-branch allows you to rewrite commits, removing files from history as if they never existed. " +
                      "But this changes commit hashes - it literally rewrites history!'\n\n" +
                      "[red]⚠️  CRITICAL WARNINGS:[/]\n" +
                      "  • ONLY rewrite history on local, unshared repositories\n" +
                      "  • NEVER rewrite history that others have based work on\n" +
                      "  • All commit hashes will change - breaking references\n" +
                      "  • Anyone who pulled before must force-pull or re-clone\n" +
                      "  • This is a DESTRUCTIVE operation - use with care!\n\n" +
                      "[yellow]The Challenge:[/]\n" +
                      "  • A secrets.txt file was accidentally committed\n" +
                      "  • Several commits were made AFTER that mistake\n" +
                      "  • Remove secrets.txt from ALL commits in history\n" +
                      "  • Preserve all other files and commits\n\n" +
                      "[yellow]Your Mission:[/]\n" +
                      "  1. Read DANGER_INSTRUCTIONS.txt thoroughly (this is important!)\n" +
                      "  2. Examine the repository history: [cyan]git log --oneline[/]\n" +
                      "  3. Verify secrets.txt exists in history: [cyan]git log --all -- secrets.txt[/]\n" +
                      "  4. Use filter-branch to remove it from all commits\n" +
                      "  5. Verify it's gone: [cyan]git log --all --full-history -- secrets.txt[/]\n\n" +
                      "[yellow]═══ Command Guide ═══[/]\n" +
                      "[cyan]git filter-branch[/] - Rewrite branches and history\n" +
                      "  • Applies filters to each commit\n" +
                      "  • Changes commit hashes\n" +
                      "  • DANGEROUS - rewrites history\n\n" +
                      "[cyan]git filter-branch --index-filter 'CMD' --all[/]\n" +
                      "  • Runs CMD on the index of each commit\n" +
                      "  • --all applies to all branches\n\n" +
                      "[cyan]git rm --cached --ignore-unmatch <file>[/]\n" +
                      "  • Remove file from index (staging)\n" +
                      "  • --cached: only remove from index, not working tree\n" +
                      "  • --ignore-unmatch: don't fail if file doesn't exist\n\n" +
                      "Full command to remove secrets.txt:\n" +
                      "[cyan]git filter-branch --force --index-filter \\\n" +
                      "  'git rm --cached --ignore-unmatch secrets.txt' \\\n" +
                      "  --prune-empty --tag-name-filter cat -- --all[/]\n\n" +
                      "[cyan]git log --all --full-history -- <file>[/]\n" +
                      "  • Show all commits that touched a file\n" +
                      "  • --all: search all branches\n" +
                      "  • --full-history: don't simplify history\n" +
                      "  • Returns empty if file never existed\n\n" +
                      "[green]Alternative Tools (Modern Recommended):[/]\n" +
                      "  • git filter-repo (faster, safer, more features)\n" +
                      "    - Not in core git, requires installation\n" +
                      "    - pip install git-filter-repo\n" +
                      "    - Usage: git filter-repo --path secrets.txt --invert-paths\n\n" +
                      "  • BFG Repo-Cleaner (simple, fast)\n" +
                      "    - Java-based tool\n" +
                      "    - Usage: bfg --delete-files secrets.txt\n\n" +
                      "[green]When to Use History Rewriting:[/]\n" +
                      "  ✓ Remove accidentally committed secrets/credentials\n" +
                      "  ✓ Remove large binary files bloating repository\n" +
                      "  ✓ Split monorepo into separate repositories\n" +
                      "  ✓ Change commit author information\n" +
                      "  ✗ NEVER on shared history without team coordination\n\n" +
                      "[green]Prevention is Better than Cure:[/]\n" +
                      "  • Use .gitignore to exclude sensitive files\n" +
                      "  • Use environment variables for secrets\n" +
                      "  • Use secret management tools (Vault, AWS Secrets Manager)\n" +
                      "  • Use pre-commit hooks to scan for secrets\n" +
                      "  • Review changes before committing (git diff --cached)\n\n" +
                      "[red]Remember:[/] If secrets were pushed to a remote, assume they're compromised!\n" +
                      "Rotate credentials immediately, even after removing from history.\n\n" +
                      "[dim]With great power comes great responsibility. Rewrite history wisely![/]",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-23" } },
            isStartRoom: false,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}
