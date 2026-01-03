using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;

namespace GitOut.Infrastructure.Persistence.RoomFactories;

public class Room23RewriteReliquaryFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room23RewriteReliquaryFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        var challenge = new RepositoryChallenge(
            id: "rewrite-reliquary-challenge",
            description: "Use git filter-repo (or filter-branch in this simulation) to remove a sensitive file from all commits in the repository's history",
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

                // Create instructions file - Updated to focus on filter-repo
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
                    "═══════════════════════════════════════════════════════════════\n" +
                    "               THE MODERN SOLUTION: git filter-repo\n" +
                    "═══════════════════════════════════════════════════════════════\n\n" +
                    "git filter-repo is the RECOMMENDED tool for rewriting git history.\n" +
                    "It replaces the deprecated git filter-branch command.\n\n" +
                    "WHY filter-repo OVER filter-branch?\n" +
                    "  • 10-100x FASTER than filter-branch\n" +
                    "  • SAFER with built-in protections against data loss\n" +
                    "  • SIMPLER syntax and better error messages\n" +
                    "  • MAINTAINED actively (filter-branch is deprecated)\n" +
                    "  • RECOMMENDED by the Git project itself\n\n" +
                    "INSTALLATION:\n" +
                    "  macOS:    brew install git-filter-repo\n" +
                    "  Ubuntu:   apt install git-filter-repo\n" +
                    "  Fedora:   dnf install git-filter-repo\n" +
                    "  pip:      pip install git-filter-repo\n" +
                    "  Manual:   Copy git-filter-repo to your PATH\n" +
                    "            https://github.com/newren/git-filter-repo\n\n" +
                    "THE COMMAND:\n" +
                    "  git filter-repo --path secrets.txt --invert-paths\n\n" +
                    "  NOTE: filter-repo requires a FRESH CLONE by default.\n" +
                    "  Use --force to override: git filter-repo --path secrets.txt --invert-paths --force\n\n" +
                    "WHAT THIS DOES:\n" +
                    "  --path secrets.txt     : Select this file for filtering\n" +
                    "  --invert-paths         : REMOVE selected paths (keep everything else)\n" +
                    "  --force                : Run even if not a fresh clone (use with caution)\n\n" +
                    "OTHER USEFUL filter-repo OPTIONS:\n" +
                    "  --path-glob '*.log'              : Remove all .log files\n" +
                    "  --strip-blobs-bigger-than 10M    : Remove files larger than 10MB\n" +
                    "  --replace-text <file>            : Replace text patterns from file\n" +
                    "  --mailmap <file>                 : Rewrite author/committer info\n" +
                    "  --analyze                        : Generate reports without modifying repo\n" +
                    "  --dry-run                        : Preview changes without applying\n\n" +
                    "═══════════════════════════════════════════════════════════════\n" +
                    "              GAME SIMULATION NOTE\n" +
                    "═══════════════════════════════════════════════════════════════\n\n" +
                    "Since git filter-repo requires separate installation, this game\n" +
                    "uses filter-branch to simulate the challenge. In the real world,\n" +
                    "ALWAYS use filter-repo instead!\n\n" +
                    "FOR THIS CHALLENGE, USE:\n" +
                    "  git filter-branch --force --index-filter \\\n" +
                    "    'git rm --cached --ignore-unmatch secrets.txt' \\\n" +
                    "    --prune-empty --tag-name-filter cat -- --all\n\n" +
                    "(Or on Windows without line continuation:)\n" +
                    "  git filter-branch --force --index-filter \"git rm --cached --ignore-unmatch secrets.txt\" --prune-empty --tag-name-filter cat -- --all\n\n" +
                    "═══════════════════════════════════════════════════════════════\n" +
                    "              WHY filter-branch IS DEPRECATED\n" +
                    "═══════════════════════════════════════════════════════════════\n\n" +
                    "The Git project deprecated filter-branch because:\n\n" +
                    "1. EXTREMELY SLOW\n" +
                    "   filter-branch spawns a shell for each commit, making it\n" +
                    "   orders of magnitude slower than filter-repo.\n\n" +
                    "2. GOTCHAS AND FOOTGUNS\n" +
                    "   - Creates backup refs that preserve old history\n" +
                    "   - Complex quoting requirements across platforms\n" +
                    "   - Silent failures in many edge cases\n" +
                    "   - Can corrupt repos if interrupted\n\n" +
                    "3. CONFUSING OUTPUT\n" +
                    "   The output is cryptic and hard to understand.\n\n" +
                    "4. NO SAFETY CHECKS\n" +
                    "   filter-branch will happily destroy your repo if you\n" +
                    "   make a mistake. filter-repo requires a fresh clone.\n\n" +
                    "═══════════════════════════════════════════════════════════════\n" +
                    "                    CRITICAL WARNINGS\n" +
                    "═══════════════════════════════════════════════════════════════\n\n" +
                    "⚠️  History rewriting is DANGEROUS and should ONLY be done on:\n" +
                    "   • Local repositories that have NEVER been pushed\n" +
                    "   • Repositories where you can force-push safely\n" +
                    "   • With team coordination if history was shared\n\n" +
                    "⚠️  NEVER rewrite history that others have based work on!\n" +
                    "   • It will break everyone else's repositories\n" +
                    "   • They'll need to re-clone\n" +
                    "   • Ongoing work may be lost\n\n" +
                    "⚠️  All commit hashes will CHANGE!\n" +
                    "   • Any references to old commits will break\n" +
                    "   • PR/issue references become invalid\n\n" +
                    "⚠️  If secrets were ever pushed, ASSUME THEY'RE COMPROMISED!\n" +
                    "   • Rotate credentials IMMEDIATELY\n" +
                    "   • Removing from history doesn't undo exposure\n\n" +
                    "═══════════════════════════════════════════════════════════════\n" +
                    "                    YOUR MISSION\n" +
                    "═══════════════════════════════════════════════════════════════\n\n" +
                    "1. Examine the repository: git log --oneline\n" +
                    "2. Verify secrets.txt in history: git log --all -- secrets.txt\n" +
                    "3. Run the filter-branch command above\n" +
                    "4. Delete the working copy: rm secrets.txt (if still exists)\n" +
                    "5. Verify it's gone: git log --all --full-history -- secrets.txt\n\n" +
                    "SUCCESS CRITERIA:\n" +
                    "  • secrets.txt removed from ALL commits in history\n" +
                    "  • secrets.txt not in working directory\n" +
                    "  • All other files preserved\n\n" +
                    "═══════════════════════════════════════════════════════════════\n" +
                    "                PREVENTION IS BETTER THAN CURE\n" +
                    "═══════════════════════════════════════════════════════════════\n\n" +
                    "Avoid this problem entirely:\n" +
                    "  • Use .gitignore for sensitive file patterns\n" +
                    "  • Store secrets in environment variables\n" +
                    "  • Use secret managers (Vault, AWS Secrets Manager, etc.)\n" +
                    "  • Use pre-commit hooks to scan for secrets\n" +
                    "  • Always review: git diff --cached before committing\n\n" +
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
                        "You need to rewrite history to remove it from ALL commits.",
                        "Read DANGER_INSTRUCTIONS.txt and use:\n" +
                        "git filter-branch --force --index-filter 'git rm --cached --ignore-unmatch secrets.txt' --prune-empty --tag-name-filter cat -- --all\n\n" +
                        "(In the real world, you'd use: git filter-repo --path secrets.txt --invert-paths)"
                    );
                }

                // Check 4: Verify other files still exist in history
                var readmeLog = await gitExec.ExecuteAsync("log --all --oneline -- README.md", workingDir);
                if (!readmeLog.Success || string.IsNullOrWhiteSpace(readmeLog.Output))
                {
                    return new ChallengeResult(
                        false,
                        "README.md has been removed from history! The filter should only remove secrets.txt, not other files.",
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
                        "Something went wrong with the filter operation."
                    );
                }

                var commitLines = allLog.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (commitLines.Length < 3)
                {
                    return new ChallengeResult(
                        false,
                        $"Only {commitLines.Length} commit(s) found. The repository should have at least 3 commits after removing the secrets commit.",
                        "The filter may have removed too many commits. Use --prune-empty carefully."
                    );
                }

                // Success!
                return new ChallengeResult(
                    true,
                    "INCREDIBLE! You've successfully rewritten history! The secrets.txt file has been completely purged from all commits. " +
                    "In real projects, always use 'git filter-repo' (install via pip/brew) as it's faster, safer, and recommended over the deprecated filter-branch. " +
                    "Remember: ALWAYS rotate any exposed credentials immediately - assume they were already compromised. Prevention is best: use .gitignore, environment variables, and secret managers. " +
                    "NEVER rewrite shared history without team coordination. You've learned one of git's most powerful and dangerous capabilities - use this knowledge wisely!",
                    null
                );
            }
        );

        var room = new Room(
            id: "room-23",
            name: "The Rewrite Reliquary",
            description: "A forbidden archive where history itself can be rewritten - featuring the modern git filter-repo approach",
            narrative: "You enter a dimly lit chamber filled with ancient scrolls and artifacts. At the center stands a mystical altar " +
                      "pulsing with dangerous energy. An ancient keeper appears, their voice grave:\n\n" +
                      "'Welcome to the Rewrite Reliquary, the most dangerous room in this dungeon. Here, you will learn to alter history itself. " +
                      "But heed my warning: this power must be used with EXTREME caution!'\n\n" +
                      "'Sometimes, terrible mistakes happen. Secrets are committed, passwords exposed, massive files bloat the repository. " +
                      "Simply deleting them from the current version isn't enough - they remain in git's history, accessible to anyone who looks.'\n\n" +
                      "[green]═══ THE MODERN WAY: git filter-repo ═══[/]\n\n" +
                      "The Git project recommends [cyan]git filter-repo[/] for rewriting history.\n" +
                      "It's 10-100x faster and much safer than the old filter-branch.\n\n" +
                      "[yellow]Installation:[/]\n" +
                      "  macOS:    [cyan]brew install git-filter-repo[/]\n" +
                      "  Ubuntu:   [cyan]apt install git-filter-repo[/]\n" +
                      "  pip:      [cyan]pip install git-filter-repo[/]\n\n" +
                      "[yellow]The filter-repo command:[/]\n" +
                      "  [cyan]git filter-repo --path secrets.txt --invert-paths[/]\n" +
                      "  [dim](Add --force if not a fresh clone)[/]\n\n" +
                      "[dim](Since filter-repo requires separate installation, this game uses\n" +
                      "filter-branch to simulate the challenge. In real projects, use filter-repo!)[/]\n\n" +
                      "[yellow]═══ For This Game (filter-branch) ═══[/]\n" +
                      "[cyan]git filter-branch --force --index-filter \\\n" +
                      "  'git rm --cached --ignore-unmatch secrets.txt' \\\n" +
                      "  --prune-empty --tag-name-filter cat -- --all[/]\n\n" +
                      "[green]═══ Prevention Tips ═══[/]\n" +
                      "  • Use .gitignore for sensitive file patterns\n" +
                      "  • Store secrets in environment variables\n" +
                      "  • Use pre-commit hooks to scan for secrets\n" +
                      "  • Always review: [cyan]git diff --cached[/] before committing\n\n" +
                      "[dim]With great power comes great responsibility. Rewrite history wisely![/]\n\n" +
                      "[red]⚠️  CRITICAL WARNINGS:[/]\n" +
                      "  • ONLY rewrite history on local, unshared repositories\n" +
                      "  • NEVER rewrite history that others have based work on\n" +
                      "  • All commit hashes will change - breaking references\n" +
                      "  • If secrets were pushed, ASSUME THEY'RE COMPROMISED\n\n" +
                      "[yellow]The Challenge:[/]\n" +
                      "  • A secrets.txt file was accidentally committed\n" +
                      "  • Several commits were made AFTER that mistake\n" +
                      "  • Remove secrets.txt from ALL commits in history\n" +
                      "  • Preserve all other files and commits\n\n" +
                      "[yellow]Your Mission:[/]\n" +
                      "  1. Read [cyan]DANGER_INSTRUCTIONS.txt[/] thoroughly!\n" +
                      "  2. Examine history: [cyan]git log --oneline[/]\n" +
                      "  3. Verify secrets in history: [cyan]git log --all -- secrets.txt[/]\n" +
                      "  4. Rewrite history using the filter-branch command from the instructions\n" +
                      "  5. Delete working copy: [cyan]rm secrets.txt[/]\n" +
                      "  6. Verify it's gone: [cyan]git log --all --full-history -- secrets.txt[/]",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-24" } },
            isStartRoom: false,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}

