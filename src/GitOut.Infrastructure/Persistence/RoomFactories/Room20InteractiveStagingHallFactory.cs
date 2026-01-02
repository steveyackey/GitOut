using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;

namespace GitOut.Infrastructure.Persistence.RoomFactories;

public class Room20InteractiveStagingHallFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room20InteractiveStagingHallFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        var challenge = new RepositoryChallenge(
            id: "interactive-staging-hall-challenge",
            description: "Use git add -p or selective staging to commit only some changes from a modified file",
            gitExecutor: _gitExecutor,
            requireGitInit: true,
            customSetup: async (workingDir, gitExec) =>
            {
                // Initialize repo
                await gitExec.ExecuteAsync("init", workingDir);
                await gitExec.ExecuteAsync("config user.email \"adventurer@gitout.com\"", workingDir);
                await gitExec.ExecuteAsync("config user.name \"Adventurer\"", workingDir);

                // Create initial file with some functions
                var initialCode = @"// Utility Functions

function greet(name) {
    return `Hello, ${name}!`;
}

function add(a, b) {
    return a + b;
}
";
                await File.WriteAllTextAsync(Path.Combine(workingDir, "utils.js"), initialCode);
                await gitExec.ExecuteAsync("add utils.js", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Initial utility functions\"", workingDir);

                // Make multiple distinct changes to the file
                var modifiedCode = @"// Utility Functions

function greet(name) {
    if (!name) throw new Error('Name is required');
    return `Hello, ${name}!`;
}

function add(a, b) {
    return a + b;
}

function multiply(a, b) {
    return a * b;
}

function subtract(a, b) {
    return a - b;
}
";
                await File.WriteAllTextAsync(Path.Combine(workingDir, "utils.js"), modifiedCode);

                // Create instructions file
                await File.WriteAllTextAsync(Path.Combine(workingDir, "CHALLENGE.txt"),
                    "You've made multiple changes to utils.js:\n" +
                    "1. Added validation to greet() function (lines 3-5)\n" +
                    "2. Added multiply() function (lines 12-14)\n" +
                    "3. Added subtract() function (lines 16-18)\n\n" +
                    "Your mission: Create a focused commit with ONLY the greet() validation change.\n" +
                    "The multiply() and subtract() functions should remain unstaged.\n\n" +
                    "Options:\n" +
                    "A) Use git add -p utils.js (interactive patch mode)\n" +
                    "   - Git will show you each change (hunk)\n" +
                    "   - Type 'y' to stage a hunk, 'n' to skip it\n" +
                    "   - Type '?' for help, 's' to split hunks if needed\n\n" +
                    "B) Use git add utils.js followed by git restore --staged for unwanted hunks\n\n" +
                    "C) Manually edit the file, stage it, commit, then add back the other changes\n\n" +
                    "Requirements for success:\n" +
                    "1. Create a commit with message 'Add validation to greet'\n" +
                    "2. The commit should include the greet validation changes\n" +
                    "3. Working tree must NOT be clean (multiply/subtract should still be unstaged)\n" +
                    "4. At least 1 line must be staged (git diff --cached shows content)\n");
            },
            customValidator: async (workingDir, gitExec) =>
            {
                // Check if we can access HEAD:utils.js (means at least one commit with this file exists)
                var showResult = await gitExec.ExecuteAsync("show HEAD:utils.js", workingDir);
                if (!showResult.Success || showResult.Output == null)
                {
                    return new ChallengeResult(
                        false,
                        "No committed version of utils.js found.",
                        "Create a commit with your staged changes."
                    );
                }

                // Check 2: Working tree must NOT be clean (unstaged changes should remain)
                var status = await gitExec.GetStatusAsync(workingDir);
                if (status.Contains("working tree clean", StringComparison.OrdinalIgnoreCase) ||
                    status.Contains("nothing to commit", StringComparison.OrdinalIgnoreCase))
                {
                    return new ChallengeResult(
                        false,
                        "The working tree is clean, but it should have unstaged changes (multiply and subtract functions).",
                        "You should have committed only the greet validation, leaving the multiply and subtract functions unstaged. Try using git add -p to selectively stage changes."
                    );
                }

                // Check 3: Verify the file has unstaged changes
                if (!status.Contains("utils.js", StringComparison.OrdinalIgnoreCase))
                {
                    return new ChallengeResult(
                        false,
                        "No unstaged changes detected in utils.js.",
                        "Make sure the multiply and subtract functions remain as unstaged changes after your commit."
                    );
                }

                // Check 4: Verify the committed version has the validation code
                var committedContent = showResult.Output;

                // Should have the validation
                if (!committedContent.Contains("if (!name)"))
                {
                    return new ChallengeResult(
                        false,
                        "The commit doesn't include the greet validation change.",
                        "Make sure your commit includes the validation added to the greet function."
                    );
                }

                // Should NOT have multiply or subtract
                if (committedContent.Contains("multiply") || committedContent.Contains("subtract"))
                {
                    return new ChallengeResult(
                        false,
                        "The commit includes multiply or subtract functions, but should only have the greet validation.",
                        "You staged too much! Use git reset --soft HEAD~1 to undo the commit, then try git add -p to selectively stage only the greet changes."
                    );
                }

                return new ChallengeResult(
                    true,
                    "Brilliant! You've mastered selective staging! By using git add -p (or another selective method), you created a focused commit " +
                    "with only the greet validation changes, while leaving multiply and subtract as unstaged work-in-progress. This is crucial for " +
                    "maintaining clean, logical commit history. Each commit should represent one logical change, making code review easier and " +
                    "git bisect more effective. Interactive staging is one of git's most powerful workflow tools!",
                    null
                );
            }
        );

        var room = new Room(
            id: "room-20",
            name: "The Interactive Staging Hall",
            description: "A grand hall of mirrors, each reflecting a different change to your code",
            narrative: "You step into an immense hall lined with mirrors. Each mirror shows a different change you've made to your code. " +
                      "A git master appears: 'Sometimes you work on multiple features at once, making various changes to the same file. " +
                      "But when it's time to commit, you want clean, focused commits—each representing one logical change.'" +
                      "\n\n'Git's interactive staging (git add -p) lets you stage specific changes within a file, not just entire files. " +
                      "You can pick and choose which hunks (chunks of changes) to commit, creating focused, reviewable commits.'" +
                      "\n\n[yellow]═══ Command Guide ═══[/]" +
                      "\n[cyan]git add -p <file>[/] - Interactively stage changes (patch mode)" +
                      "\n  • Shows each change (hunk) separately" +
                      "\n  • Prompts: 'Stage this hunk [y,n,q,a,d,s,e,?]?'" +
                      "\n  • y = yes, stage this hunk" +
                      "\n  • n = no, skip this hunk" +
                      "\n  • s = split into smaller hunks" +
                      "\n  • e = manually edit the hunk" +
                      "\n  • q = quit (don't stage remaining hunks)" +
                      "\n  • ? = show help" +
                      "\n\n[cyan]git diff[/] - Show unstaged changes" +
                      "\n  • What's modified but not yet staged" +
                      "\n\n[cyan]git diff --cached[/] - Show staged changes" +
                      "\n  • What will be included in the next commit" +
                      "\n  • Also works: git diff --staged" +
                      "\n\n[cyan]git add <file>[/] - Stage entire file" +
                      "\n  • Traditional way (all or nothing)" +
                      "\n\n[cyan]git restore --staged <file>[/] - Unstage a file" +
                      "\n  • Remove from staging area, keep changes in working tree" +
                      "\n\n[cyan]Alternative approaches:[/]" +
                      "\n  • Add all: git add utils.js" +
                      "\n  • Then unstage unwanted: git restore --staged utils.js" +
                      "\n  • Then re-stage wanted parts: git add -p utils.js" +
                      "\n\n[green]Pro tips:[/]" +
                      "\n  • Use 's' to split large hunks into smaller ones for finer control" +
                      "\n  • Use 'e' to manually edit hunks (advanced)" +
                      "\n  • git add -p works great with git commit --verbose to review staged changes" +
                      "\n  • This technique creates cleaner git history for code review" +
                      "\n\n[dim]Selective staging: the key to logical, focused commits![/]" +
                      "\n\n[yellow]The Challenge:[/]" +
                      "\n  • You've modified utils.js with THREE separate changes" +
                      "\n  • Create a commit with ONLY the greet() validation change" +
                      "\n  • Leave multiply() and subtract() functions unstaged" +
                      "\n\n[yellow]Steps to Complete:[/]" +
                      "\n  1. Read CHALLENGE.txt for full details" +
                      "\n  2. Check current status: [cyan]git status[/]" +
                      "\n  3. View all changes: [cyan]git diff utils.js[/]" +
                      "\n  4. Stage selectively: [cyan]git add -p utils.js[/]" +
                      "\n  5. When prompted for each hunk:" +
                      "\n     • Type 'y' to stage the greet validation hunk" +
                      "\n     • Type 'n' to skip the multiply and subtract hunks" +
                      "\n     • Type '?' if you need help with options" +
                      "\n  6. Verify what's staged: [cyan]git diff --cached[/]" +
                      "\n  7. Verify what remains unstaged: [cyan]git diff[/]" +
                      "\n  8. Commit: [cyan]git commit -m 'Add validation to greet'[/]" +
                      "\n  9. Confirm unstaged changes remain: [cyan]git status[/]",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-21" } },
            isStartRoom: false,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}
