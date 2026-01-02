using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;

namespace GitOut.Infrastructure.Persistence.RoomFactories;

public class Room16BisectBattlefieldFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room16BisectBattlefieldFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        var challenge = new RepositoryChallenge(
            id: "bisect-battlefield-challenge",
            description: "Use git bisect to find the commit that introduced a bug, then reset",
            gitExecutor: _gitExecutor,
            requireGitInit: true,
            customSetup: async (workingDir, gitExec) =>
            {
                // Initialize repo
                await gitExec.ExecuteAsync("init", workingDir);
                await gitExec.ExecuteAsync("config user.email \"adventurer@gitout.com\"", workingDir);
                await gitExec.ExecuteAsync("config user.name \"Adventurer\"", workingDir);

                // Create 10 commits - Version 6 introduces a bug in the multiply function
                for (int i = 1; i <= 10; i++)
                {
                    // The bug: Version 6 changes * to + in the multiply function
                    var multiplyOp = i >= 6 ? "+" : "*";
                    var calculator = $@"# Calculator Module - Version {i}

def add(a, b):
    return a + b

def subtract(a, b):
    return a - b

def multiply(a, b):
    return a {multiplyOp} b

def divide(a, b):
    if b == 0:
        raise ValueError(""Cannot divide by zero"")
    return a / b
";
                    await File.WriteAllTextAsync(Path.Combine(workingDir, "calculator.py"), calculator);
                    await gitExec.ExecuteAsync("add calculator.py", workingDir);
                    await gitExec.ExecuteAsync($"commit -m \"Version {i}\"", workingDir);
                }

                // Create a test output file showing the current failure
                var testOutput = @"=== TEST RESULTS ===

Running calculator tests...

test_add(2, 3) = 5          ✓ PASS
test_subtract(5, 3) = 2     ✓ PASS
test_multiply(4, 3) = 7     ✗ FAIL (expected 12)
test_divide(10, 2) = 5      ✓ PASS

FAILURE: multiply() returns wrong result!
The function is adding instead of multiplying.

Someone broke the multiply function, but we don't know when.
Use git bisect to find the commit that introduced this bug.
";
                await File.WriteAllTextAsync(Path.Combine(workingDir, "TEST_RESULTS.txt"), testOutput);
                await gitExec.ExecuteAsync("add TEST_RESULTS.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Add failing test results\"", workingDir);
            },
            customValidator: async (workingDir, gitExec) =>
            {
                var bisectLogPath = Path.Combine(workingDir, ".git", "BISECT_LOG");
                var bisectStartedPath = Path.Combine(workingDir, ".git", "BISECT_START");
                
                // Check if bisect has been reset (challenge complete!)
                // After reset, BISECT_LOG and BISECT_START no longer exist
                if (!File.Exists(bisectLogPath) && !File.Exists(bisectStartedPath))
                {
                    // Check if they completed bisect by looking at reflog for multiple checkout movements
                    // (bisect causes multiple "checkout: moving from X to Y" entries)
                    // Also verify they're back on the main branch
                    var branchResult = await gitExec.ExecuteAsync("branch --show-current", workingDir);
                    var reflogResult = await gitExec.ExecuteAsync("reflog", workingDir);
                    
                    var onMainBranch = branchResult.Success && 
                        (branchResult.Output.Trim() == "main" || branchResult.Output.Trim() == "master");
                    
                    // Count checkout movements in reflog - bisect creates several
                    var checkoutCount = reflogResult.Success ? 
                        reflogResult.Output.Split('\n').Count(line => line.Contains("checkout: moving")) : 0;
                    
                    if (onMainBranch && checkoutCount >= 3)
                    {
                        // They ran bisect and reset - challenge complete!
                        return new ChallengeResult(
                            true,
                            "Excellent detective work! You successfully used git bisect to find that Version 6 " +
                            "introduced the bug, then cleaned up with git bisect reset. " +
                            "Using binary search, you checked only ~4 commits instead of all 10. " +
                            "In a real project with 1000 commits, bisect finds bugs in just ~10 steps!",
                            null
                        );
                    }
                    
                    // No evidence of bisect - they haven't started
                    return new ChallengeResult(
                        false,
                        "The hunt begins! Start your bisect session with: git bisect start",
                        "Run 'git bisect start' to begin, then 'git bisect bad' to mark the current commit as broken"
                    );
                }
                
                // Bisect is active - check the state
                var bisectLog = File.Exists(bisectLogPath) ? await File.ReadAllTextAsync(bisectLogPath) : "";
                
                // Check if they've marked bad yet
                if (!bisectLog.Contains("# bad:"))
                {
                    return new ChallengeResult(
                        false,
                        "Good start! Now mark the current commit as bad: git bisect bad",
                        "The current code is broken, so mark it with 'git bisect bad'"
                    );
                }
                
                // Check if they've marked a good commit yet
                if (!bisectLog.Contains("# good:"))
                {
                    return new ChallengeResult(
                        false,
                        "Now tell git a known good commit. Version 1 worked fine: git bisect good HEAD~10",
                        "HEAD~10 refers to 10 commits ago (Version 1). Mark it as good so git knows the range to search."
                    );
                }
                
                // Check if bisect has completed by looking for "# first bad commit:" in the log
                // This line only appears when bisect has identified the culprit
                if (bisectLog.Contains("# first bad commit:") && bisectLog.Contains("Version 6"))
                {
                    // Bisect found Version 6 as the culprit! Now they need to reset
                    return new ChallengeResult(
                        false,
                        "SUCCESS! Git bisect identified Version 6 as the first bad commit!\n\n" +
                        "You found the bug using only ~4 checks instead of 10. But you're still in " +
                        "bisect mode - your HEAD is detached.\n\n" +
                        "To complete the challenge, clean up the bisect session: git bisect reset\n\n" +
                        "This returns you to where you started (the main branch).",
                        "Run 'git bisect reset' to end the bisect session and return to your branch"
                    );
                }
                
                // Still searching - give helpful hints
                return new ChallengeResult(
                    false,
                    "Bisect in progress! Git has checked out a commit for you to test.\n\n" +
                    "Check the multiply function: git show HEAD:calculator.py\n" +
                    "  • If multiply uses * → it's good: git bisect good\n" +
                    "  • If multiply uses + → it's bad: git bisect bad\n\n" +
                    "Keep marking until git announces the first bad commit!",
                    "Look at calculator.py with 'git show HEAD:calculator.py' and check the multiply function"
                );
            }
        );

        var room = new Room(
            id: "room-16",
            name: "The Bisect Battlefield",
            description: "A battlefield where bugs are hunted through time",
            narrative: "You enter a war room filled with monitors displaying failing tests. Red warning lights flash as a frantic developer explains: " +
                      "'Our calculator's multiply function is broken! It's returning 7 instead of 12 for 4×3. " +
                      "We know it worked before, but with 10 versions of the code, manually checking each one would take forever!'" +
                      "\n\nA seasoned debugger steps forward: '[cyan]git bisect[/] is your weapon here. Instead of checking every commit, " +
                      "it uses binary search - cutting the search space in half with each step. Tell git one bad commit and one good commit, " +
                      "and it will find the culprit in just a few checks.'" +
                      "\n\n[yellow]═══ THE SCENARIO ═══[/]" +
                      "\n  • [cyan]calculator.py[/] has add, subtract, multiply, and divide functions" +
                      "\n  • The [red]multiply[/] function is broken - it's adding instead of multiplying" +
                      "\n  • There are 10 versions of the code (commits \"Version 1\" through \"Version 10\")" +
                      "\n  • The bug was introduced in one of these commits - find which one!" +
                      "\n\n[yellow]═══ HOW TO BISECT ═══[/]" +
                      "\n[white]1. Start:[/]        [cyan]git bisect start[/]" +
                      "\n[white]2. Mark bad:[/]     [cyan]git bisect bad[/]          [dim](current code is broken)[/]" +
                      "\n[white]3. Mark good:[/]    [cyan]git bisect good HEAD~10[/]  [dim](Version 1 was fine)[/]" +
                      "\n[white]4. Test:[/]         [cyan]git show HEAD:calculator.py[/]" +
                      "\n[white]5. Mark result:[/]  [cyan]git bisect good[/] or [cyan]git bisect bad[/]" +
                      "\n[white]6. Repeat:[/]       Steps 4-5 until git announces \"first bad commit\"" +
                      "\n[white]7. Clean up:[/]     [cyan]git bisect reset[/]        [dim](return to your branch)[/]" +
                      "\n\n[yellow]═══ WHY BISECT IS POWERFUL ═══[/]" +
                      "\n  • Binary search = O(log n) complexity" +
                      "\n  • 10 commits → ~4 checks | 1000 commits → ~10 checks | 1,000,000 commits → ~20 checks" +
                      "\n  • Remember: [cyan]HEAD~10[/] means \"10 commits before HEAD\" (you learned about HEAD earlier!)" +
                      "\n\n[dim]The challenge completes when you find the bad commit AND run git bisect reset.[/]",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-17" } },
            isStartRoom: false,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}
