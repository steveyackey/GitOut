using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;

namespace GitOut.Infrastructure.Persistence.RoomFactories;

public class Room17RevertChamberFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room17RevertChamberFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        var challenge = new RepositoryChallenge(
            id: "revert-chamber-challenge",
            description: "Use git revert to safely undo a bad commit without rewriting history",
            gitExecutor: _gitExecutor,
            requireGitInit: true,
            customSetup: async (workingDir, gitExec) =>
            {
                // Initialize repo
                await gitExec.ExecuteAsync("init", workingDir);
                await gitExec.ExecuteAsync("config user.email \"adventurer@gitout.com\"", workingDir);
                await gitExec.ExecuteAsync("config user.name \"Adventurer\"", workingDir);

                // Create a working application with multiple commits
                // Commit 1: Initial setup
                var appCode = @"// Application Configuration
const config = {
    apiUrl: 'https://api.example.com',
    timeout: 5000,
    retries: 3
};

module.exports = config;
";
                await File.WriteAllTextAsync(Path.Combine(workingDir, "config.js"), appCode);
                await gitExec.ExecuteAsync("add config.js", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Initial config setup\"", workingDir);

                // Commit 2: Add a feature
                var featureCode = @"// Feature: User Authentication
function authenticate(user, password) {
    // Secure authentication logic
    return validateCredentials(user, password);
}

module.exports = { authenticate };
";
                await File.WriteAllTextAsync(Path.Combine(workingDir, "auth.js"), featureCode);
                await gitExec.ExecuteAsync("add auth.js", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Add authentication feature\"", workingDir);

                // Commit 3: THE BAD COMMIT - breaks the application
                var brokenConfig = @"// Application Configuration
const config = {
    apiUrl: 'https://api.example.com',
    timeout: 50,  // BROKEN: Changed from 5000 to 50 (too short!)
    retries: 0,   // BROKEN: Changed from 3 to 0 (no retries!)
    debug: true   // BROKEN: Debug mode in production!
};

module.exports = config;
";
                await File.WriteAllTextAsync(Path.Combine(workingDir, "config.js"), brokenConfig);
                await gitExec.ExecuteAsync("add config.js", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Update config settings\"", workingDir);

                // Commit 4: Another good commit after the bad one
                var loggingCode = @"// Logging utility
function log(level, message) {
    const timestamp = new Date().toISOString();
    console.log(`[${timestamp}] [${level}] ${message}`);
}

module.exports = { log };
";
                await File.WriteAllTextAsync(Path.Combine(workingDir, "logger.js"), loggingCode);
                await gitExec.ExecuteAsync("add logger.js", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Add logging utility\"", workingDir);

                // Commit 5: Yet another good commit
                var readmeContent = @"# My Application

A robust application with authentication and logging.

## Features
- Secure authentication
- Comprehensive logging
- Configurable settings
";
                await File.WriteAllTextAsync(Path.Combine(workingDir, "README.md"), readmeContent);
                await gitExec.ExecuteAsync("add README.md", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Add README documentation\"", workingDir);

                // Create instructions file
                await File.WriteAllTextAsync(Path.Combine(workingDir, "CRISIS_REPORT.txt"),
                    "═══════════════════════════════════════════════════════════════\n" +
                    "              🚨 PRODUCTION INCIDENT REPORT 🚨\n" +
                    "═══════════════════════════════════════════════════════════════\n\n" +
                    "INCIDENT: Application experiencing timeout failures in production!\n\n" +
                    "SYMPTOMS:\n" +
                    "  • API calls timing out after only 50ms\n" +
                    "  • No automatic retries on failure\n" +
                    "  • Debug logs flooding production systems\n\n" +
                    "ROOT CAUSE ANALYSIS:\n" +
                    "  The commit \"Update config settings\" introduced breaking changes:\n" +
                    "  • timeout: 5000 → 50 (too short for API calls)\n" +
                    "  • retries: 3 → 0 (no fault tolerance)\n" +
                    "  • debug: true (leaking sensitive info)\n\n" +
                    "CONSTRAINT:\n" +
                    "  This code has already been pushed to production and shared with\n" +
                    "  the team. We CANNOT use git reset or rebase to rewrite history!\n\n" +
                    "SOLUTION: Use git revert\n" +
                    "  git revert creates a NEW commit that undoes the changes from a\n" +
                    "  previous commit. History is preserved, and the fix can be safely\n" +
                    "  pushed without breaking anyone else's repository.\n\n" +
                    "═══════════════════════════════════════════════════════════════\n" +
                    "                    YOUR MISSION\n" +
                    "═══════════════════════════════════════════════════════════════\n\n" +
                    "1. View the commit history:\n" +
                    "   git log --oneline\n\n" +
                    "2. Identify the bad commit (\"Update config settings\")\n" +
                    "   Note its commit hash (the 7-character code on the left)\n\n" +
                    "3. Revert the bad commit:\n" +
                    "   git revert <commit-hash> --no-edit\n" +
                    "   (--no-edit uses the default revert commit message)\n\n" +
                    "4. Verify the fix:\n" +
                    "   git show HEAD:config.js  (should show timeout: 5000, retries: 3)\n" +
                    "   git log --oneline (should show the revert commit)\n\n" +
                    "SUCCESS CRITERIA:\n" +
                    "  • config.js must have timeout: 5000 (not 50)\n" +
                    "  • config.js must have retries: 3 (not 0)\n" +
                    "  • config.js must NOT have debug: true\n" +
                    "  • A revert commit must exist in history\n" +
                    "  • Original bad commit must still be in history (not deleted)\n\n" +
                    "═══════════════════════════════════════════════════════════════\n");
            },
            customValidator: async (workingDir, gitExec) =>
            {
                // Check 1: Verify config.js exists
                var configPath = Path.Combine(workingDir, "config.js");
                if (!File.Exists(configPath))
                {
                    return new ChallengeResult(
                        false,
                        "config.js file is missing!",
                        "Something went wrong. The config.js file should exist."
                    );
                }

                // Check 2: Verify config.js has been fixed
                var configContent = await File.ReadAllTextAsync(configPath);
                
                // Check for correct timeout value
                if (configContent.Contains("timeout: 50") || !configContent.Contains("timeout: 5000"))
                {
                    return new ChallengeResult(
                        false,
                        "The config.js file still has the broken timeout value (50 instead of 5000).",
                        "Use 'git log --oneline' to find the bad commit, then 'git revert <hash> --no-edit' to undo it."
                    );
                }

                // Check for correct retries value
                if (configContent.Contains("retries: 0") || !configContent.Contains("retries: 3"))
                {
                    return new ChallengeResult(
                        false,
                        "The config.js file still has the broken retries value (0 instead of 3).",
                        "The revert should restore the original retries: 3 value."
                    );
                }

                // Check that debug mode is removed
                if (configContent.Contains("debug: true"))
                {
                    return new ChallengeResult(
                        false,
                        "The config.js file still has debug: true which was part of the bad commit.",
                        "The revert should remove the debug line entirely."
                    );
                }

                // Check 3: Verify a revert commit exists in history
                var logResult = await gitExec.ExecuteAsync("log --oneline", workingDir);
                if (!logResult.Success)
                {
                    return new ChallengeResult(
                        false,
                        "Could not read git history.",
                        "Run 'git log --oneline' to check the commit history."
                    );
                }

                var logOutput = logResult.Output ?? "";
                if (!logOutput.Contains("Revert", StringComparison.OrdinalIgnoreCase))
                {
                    return new ChallengeResult(
                        false,
                        "No revert commit found in history. Did you use git revert?",
                        "Use 'git revert <commit-hash> --no-edit' to create a revert commit. Don't use git reset!"
                    );
                }

                // Check 4: Verify the original bad commit still exists (history not rewritten)
                if (!logOutput.Contains("Update config settings", StringComparison.OrdinalIgnoreCase))
                {
                    return new ChallengeResult(
                        false,
                        "The original 'Update config settings' commit is missing from history. Did you rewrite history instead of reverting?",
                        "git revert preserves history. Use 'git revert', not 'git reset' or 'git rebase'."
                    );
                }

                // Check 5: Verify we have the expected number of commits (5 original + 1 revert = 6)
                var commitLines = logOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (commitLines.Length < 6)
                {
                    return new ChallengeResult(
                        false,
                        $"Expected at least 6 commits (5 original + 1 revert), but found {commitLines.Length}. History may have been rewritten.",
                        "Make sure you used 'git revert', not 'git reset'."
                    );
                }

                // All checks passed!
                return new ChallengeResult(
                    true,
                    "Excellent work! You've successfully used git revert to undo the bad commit! " +
                    "Notice that the original bad commit still exists in history - this is intentional. " +
                    "Git revert creates a NEW commit that applies the inverse of the bad commit's changes. " +
                    "This approach is safe for shared repositories because it doesn't rewrite history. " +
                    "Your teammates can simply pull the revert commit without any conflicts. " +
                    "Remember: Use 'git revert' for commits that have been pushed/shared, and 'git reset' only for local commits.",
                    null
                );
            }
        );

        var room = new Room(
            id: "room-17",
            name: "The Revert Chamber",
            description: "A chamber where past mistakes can be undone without erasing history",
            narrative: "You rush into a command center filled with flashing red alerts. Alarms blare as engineers " +
                      "frantically type at their terminals. A senior developer spots you and explains the crisis:\n\n" +
                      "'Production is down! Someone pushed a bad commit that broke our configuration. Users are " +
                      "experiencing timeouts and our logs are flooded with debug output. We need to fix this NOW!'\n\n" +
                      "'But here's the catch,' they continue, 'the code has already been pushed and shared with the team. " +
                      "We can't use git reset or rebase - that would rewrite history and break everyone else's repositories. " +
                      "We need [cyan]git revert[/] - it creates a new commit that undoes the bad changes while preserving history.'\n\n" +
                      "[yellow]═══ Command Guide ═══[/]\n" +
                      "[cyan]git revert <commit>[/] - Create a new commit that undoes a previous commit\n" +
                      "  • Creates a NEW commit (doesn't delete the old one)\n" +
                      "  • Safe for shared/pushed history\n" +
                      "  • Opens editor for commit message (use --no-edit for default)\n" +
                      "  • Example: git revert abc123 --no-edit\n\n" +
                      "[cyan]git revert <commit1>..<commit2>[/] - Revert a range of commits\n" +
                      "  • Reverts all commits in the range\n" +
                      "  • Creates one revert commit per original commit\n\n" +
                      "[cyan]git revert -n <commit>[/] - Revert without committing\n" +
                      "  • Applies the revert to working directory\n" +
                      "  • Useful for combining multiple reverts into one commit\n\n" +
                      "[yellow]═══ Revert vs Reset vs Restore ═══[/]\n\n" +
                      "[cyan]git revert[/] - Undo a COMMIT by creating a new commit\n" +
                      "  • Safe for shared history (doesn't rewrite)\n" +
                      "  • Creates inverse changes as a new commit\n" +
                      "  • Use for: Undoing pushed commits\n\n" +
                      "[cyan]git reset[/] - Move branch pointer backwards\n" +
                      "  • REWRITES history (dangerous for shared code!)\n" +
                      "  • Removes commits from branch\n" +
                      "  • Use for: Undoing LOCAL, unpushed commits only\n\n" +
                      "[cyan]git restore[/] - Discard UNCOMMITTED changes in files\n" +
                      "  • Only affects working directory\n" +
                      "  • Doesn't touch commits at all\n" +
                      "  • Use for: Discarding local file modifications\n\n" +
                      "[green]Pro tips:[/]\n" +
                      "  • Always use revert for commits that have been pushed\n" +
                      "  • Use 'git log --oneline' to find the commit hash\n" +
                      "  • You can revert a revert if you made a mistake!\n" +
                      "  • Merge conflicts can occur if files changed since the bad commit\n" +
                      "  • [cyan]git show HEAD:file[/] shows file contents at HEAD commit\n" +
                      "    (without HEAD:, git show <commit> shows the commit diff instead)\n\n" +
                      "[dim]Revert: The safe way to undo shared mistakes![/]\n\n" +
                      "[yellow]The Crisis:[/]\n" +
                      "  • A bad commit broke the production configuration\n" +
                      "  • The commit has already been pushed and shared\n" +
                      "  • You must fix it WITHOUT rewriting history\n\n" +
                      "[yellow]Your Mission:[/]\n" +
                      "  1. Read [cyan]CRISIS_REPORT.txt[/] for full details\n" +
                      "  2. Use [cyan]git log --oneline[/] to find the bad commit\n" +
                      "  3. Look for the commit \"Update config settings\"\n" +
                      "  4. Revert it: [cyan]git revert <commit-hash> --no-edit[/]\n" +
                      "  5. Verify config.js is fixed: [cyan]git show HEAD:config.js[/]",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-18" } },
            isStartRoom: false,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}

