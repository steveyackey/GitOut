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
            description: "Use git bisect to find the commit that introduced a bug",
            gitExecutor: _gitExecutor,
            requireGitInit: true,
            customSetup: async (workingDir, gitExec) =>
            {
                // Initialize repo
                await gitExec.ExecuteAsync("init", workingDir);
                await gitExec.ExecuteAsync("config user.email \"adventurer@gitout.com\"", workingDir);
                await gitExec.ExecuteAsync("config user.name \"Adventurer\"", workingDir);

                // Create multiple commits, one will introduce a "bug"
                for (int i = 1; i <= 10; i++)
                {
                    var status = i == 6 ? "BUGGED" : "OK";
                    await File.WriteAllTextAsync(Path.Combine(workingDir, "code.txt"),
                        $"Version {i}\nStatus: {status}");
                    await gitExec.ExecuteAsync("add code.txt", workingDir);
                    await gitExec.ExecuteAsync($"commit -m \"Version {i}\"", workingDir);
                }

                // Create info file
                await File.WriteAllTextAsync(Path.Combine(workingDir, "BISECT_INFO.txt"),
                    "A bug was introduced somewhere in commits 1-10.\n" +
                    "The bug is in code.txt - it says 'Status: BUGGED' instead of 'Status: OK'.\n\n" +
                    "Use git bisect to find which commit introduced it:\n" +
                    "1. git bisect start\n" +
                    "2. git bisect bad (current commit is bad)\n" +
                    "3. git bisect good HEAD~9 (commit 1 was good)\n" +
                    "4. Git will checkout commits - check code.txt each time\n" +
                    "5. Use 'git bisect good' or 'git bisect bad' based on what you find\n" +
                    "6. When done, 'git bisect reset' to return to normal\n\n" +
                    "Create a file named FOUND_BUG.txt containing the commit number when you find it!");
                await gitExec.ExecuteAsync("add BISECT_INFO.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Add bisect instructions\"", workingDir);
            },
            customValidator: async (workingDir, gitExec) =>
            {
                // Check if player created the FOUND_BUG.txt file
                var foundBugPath = Path.Combine(workingDir, "FOUND_BUG.txt");
                if (File.Exists(foundBugPath))
                {
                    var content = await File.ReadAllTextAsync(foundBugPath);
                    if (content.Contains("6") || content.Contains("Version 6"))
                    {
                        return new ChallengeResult(
                            true,
                            "Victory! You've identified Version 6 as the culprit! By systematically using git bisect, you performed a binary search " +
                            "through history to find the exact commit that introduced the bug. This is one of git's most powerful debugging tools! " +
                            "\n\nCongratulations, brave adventurer! You've mastered the intermediate concepts of git and completed Phase 3 of your journey!",
                            null
                        );
                    }
                }

                return new ChallengeResult(
                    false,
                    "The bug has not been identified yet.",
                    "Follow the instructions in BISECT_INFO.txt. Use git bisect to find the bad commit, then create FOUND_BUG.txt with the version number"
                );
            }
        );

        var room = new Room(
            id: "room-16",
            name: "The Bisect Battlefield",
            description: "A battlefield where bugs are hunted through time",
            narrative: "You enter a vast battlefield strewn with the remnants of countless commits. Each marker represents a version in history. " +
                      "Somewhere among these commits, a bug was introduced, corrupting the code. " +
                      "\n\nA master debugger appears: 'When you know a bug exists now but didn't before, git bisect performs a binary search " +
                      "through history to find the exact commit that introduced it. Instead of checking every commit, it uses divide-and-conquer!' " +
                      "\n\nThere are 11 commits in this repository. Commit 1 was good, the current commit is bad. Your mission: find which commit " +
                      "introduced the bug." +
                      "\n\n[yellow]═══ Command Guide ═══[/]" +
                      "\n[cyan]git bisect start[/] - Begins a bisect session" +
                      "\n  • Puts git into bisect mode" +
                      "\n  • You'll mark commits as good or bad to narrow down the culprit" +
                      "\n\n[cyan]git bisect bad [commit][/] - Marks a commit as bad (has the bug)" +
                      "\n  • Without argument, marks current commit" +
                      "\n  • Usually start with 'git bisect bad' on HEAD" +
                      "\n\n[cyan]git bisect good [commit][/] - Marks a commit as good (no bug)" +
                      "\n  • Marks the last known good commit" +
                      "\n  • Can use HEAD~5, commit hash, or tag" +
                      "\n\n[cyan]git bisect reset[/] - Ends bisect and returns to original commit" +
                      "\n  • Always run this when done" +
                      "\n  • Returns HEAD to where you started" +
                      "\n\n[cyan]How bisect works:[/]" +
                      "\n  • Uses binary search (O(log n) time complexity)" +
                      "\n  • With 1000 commits, finds bug in ~10 steps" +
                      "\n  • Each step, git checks out the middle commit" +
                      "\n  • You test and mark it good or bad" +
                      "\n  • Git narrows the range and repeats" +
                      "\n\n[cyan]Advanced bisect:[/]" +
                      "\n  • [cyan]git bisect run <script>[/] - Automates bisect with a test script" +
                      "\n  • Script exits 0 for good, 1-127 (except 125) for bad" +
                      "\n  • Extremely powerful for regressions with automated tests" +
                      "\n\n[green]Pro tip:[/] Bisect is most powerful when you have good automated tests!" +
                      "\n\n[dim]Think of bisect as binary search through git history![/]" +
                      "\n\n[yellow]To complete this challenge:[/]" +
                      "\n  1. A bug exists in code.txt - one version says 'BUGGED' instead of 'OK'" +
                      "\n  2. Start bisecting: [cyan]git bisect start[/]" +
                      "\n  3. Mark current as bad: [cyan]git bisect bad[/]" +
                      "\n  4. Mark old commit as good: [cyan]git bisect good HEAD~9[/]" +
                      "\n  5. Git checks out a commit - view the file: [cyan]git show HEAD:code.txt[/]" +
                      "\n  6. If it says BUGGED, mark bad: [cyan]git bisect bad[/], else: [cyan]git bisect good[/]" +
                      "\n  7. Repeat until git identifies the first bad commit (it's Version 6)" +
                      "\n  8. Note the version number and reset: [cyan]git bisect reset[/]" +
                      "\n  9. Use 'git' commands to create and commit a file named FOUND_BUG.txt containing 'Version 6'",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-17" } },
            isStartRoom: false,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}
