using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;

namespace GitOut.Infrastructure.Persistence.RoomFactories;

public class Room23FinalGauntletFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room23FinalGauntletFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        var challenge = new RepositoryChallenge(
            id: "final-gauntlet-challenge",
            description: "The ultimate git mastery challenge - rescue a broken repository using all your skills",
            gitExecutor: _gitExecutor,
            requireGitInit: true,
            customSetup: async (workingDir, gitExec) =>
            {
                // Initialize repo and configure user
                await gitExec.ExecuteAsync("init", workingDir);
                await gitExec.ExecuteAsync("config user.email \"hero@gitout.com\"", workingDir);
                await gitExec.ExecuteAsync("config user.name \"Git Hero\"", workingDir);

                // === STEP 1: Create main branch with initial commits ===
                await File.WriteAllTextAsync(Path.Combine(workingDir, "README.md"),
                    "# The Grand Repository\n\nA legendary codebase that powers the realm.");
                await gitExec.ExecuteAsync("add README.md", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Initial commit\"", workingDir);

                await File.WriteAllTextAsync(Path.Combine(workingDir, "core.js"),
                    "// Core functionality\nfunction initialize() { return 'System initialized'; }");
                await gitExec.ExecuteAsync("add core.js", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Add core functionality\"", workingDir);

                // === STEP 2: Create feature-1 branch with conflicting changes ===
                await gitExec.ExecuteAsync("checkout -b feature-1", workingDir);

                await File.WriteAllTextAsync(Path.Combine(workingDir, "core.js"),
                    "// Core functionality - ENHANCED\nfunction initialize() { return 'System initialized with power'; }");
                await gitExec.ExecuteAsync("add core.js", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Enhance core initialization\"", workingDir);

                await File.WriteAllTextAsync(Path.Combine(workingDir, "feature1.js"),
                    "// Feature 1 implementation\nfunction featureOne() { return 'Feature 1 active'; }");
                await gitExec.ExecuteAsync("add feature1.js", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Implement feature 1\"", workingDir);

                // === STEP 3: Create feature-2 branch (already merged scenario) ===
                await gitExec.ExecuteAsync("checkout main", workingDir);
                await gitExec.ExecuteAsync("checkout -b feature-2", workingDir);

                await File.WriteAllTextAsync(Path.Combine(workingDir, "feature2.js"),
                    "// Feature 2 implementation\nfunction featureTwo() { return 'Feature 2 ready'; }");
                await gitExec.ExecuteAsync("add feature2.js", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Implement feature 2\"", workingDir);

                // Merge feature-2 into main (this one succeeds, no conflict)
                await gitExec.ExecuteAsync("checkout main", workingDir);
                await gitExec.ExecuteAsync("merge feature-2 --no-edit", workingDir);

                // === STEP 4: Create the "critical lost commit" ===
                // This commit will be "lost" (orphaned) by creating it then resetting away from it
                await File.WriteAllTextAsync(Path.Combine(workingDir, "critical-fix.js"),
                    "// Critical bug fix\nfunction criticalFix() { return 'Security vulnerability patched'; }");
                await gitExec.ExecuteAsync("add critical-fix.js", workingDir);
                await gitExec.ExecuteAsync("commit -m \"CRITICAL: Security patch\"", workingDir);

                // Get the commit hash of the critical commit
                var criticalCommitResult = await gitExec.ExecuteAsync("rev-parse HEAD", workingDir);
                var criticalCommitHash = criticalCommitResult.Output?.Trim() ?? "";

                // Reset to previous commit, making the critical commit "lost" (only in reflog)
                await gitExec.ExecuteAsync("reset --hard HEAD~1", workingDir);

                // === STEP 5: Make conflicting changes on main (conflicts with feature-1) ===
                await File.WriteAllTextAsync(Path.Combine(workingDir, "core.js"),
                    "// Core functionality - OPTIMIZED\nfunction initialize() { return 'System initialized efficiently'; }");
                await gitExec.ExecuteAsync("add core.js", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Optimize core initialization\"", workingDir);

                // === STEP 6: Create uncommitted changes (to be stashed) ===
                await File.WriteAllTextAsync(Path.Combine(workingDir, "work-in-progress.js"),
                    "// Work in progress - not ready to commit\nfunction wip() { return 'Still working on this'; }");
                await gitExec.ExecuteAsync("add work-in-progress.js", workingDir);

                await File.WriteAllTextAsync(Path.Combine(workingDir, "notes.txt"),
                    "Random notes that shouldn't be committed yet");

                // === STEP 7: Create comprehensive instruction file ===
                var instructionsContent = @"╔══════════════════════════════════════════════════════════════════════════════╗
║                        THE REPOSITORY RESCUE MISSION                          ║
║                           FINAL BOSS CHALLENGE                                ║
╚══════════════════════════════════════════════════════════════════════════════╝

SCENARIO:
You are the last hope for rescuing a critical repository that has fallen into chaos!
The repository is in a broken state with multiple problems that must be fixed.

Your predecessor left this mess:
• Uncommitted changes cluttering the working tree
• A critical security patch that was accidentally lost (exists only in reflog)
• feature-1 branch needs to be merged, but has conflicts with main
• feature-2 was already merged but was never tagged for release
• The repository needs to be brought to a clean, release-ready state

YOUR MISSION - Complete ALL of the following tasks:

1. STASH THE UNCOMMITTED CHANGES
   • There are uncommitted changes in the working tree
   • Stash them to clean your working area
   • Command: git stash push -m ""Save work in progress""

2. RECOVER THE LOST COMMIT FROM REFLOG
   • A critical security patch commit was lost when someone reset main
   • The commit message was: ""CRITICAL: Security patch""
   • Find it in the reflog and recover it
   • Commands:
     - git reflog                          (find the lost commit hash)
     - git cherry-pick <commit-hash>       (recover it to main)
   • Hint: Look for the commit with ""CRITICAL: Security patch"" in reflog output

3. MERGE feature-1 INTO main (WITH CONFLICT RESOLUTION)
   • Attempt to merge feature-1 branch into main
   • This WILL create a merge conflict in core.js
   • Resolve the conflict by choosing feature-1's version
   • Commands:
     - git merge feature-1                 (this will fail with conflicts)
     - git checkout --theirs core.js       (choose feature-1's version)
     - git add core.js                     (mark as resolved)
     - git commit --no-edit                (complete the merge)

4. TAG THE CURRENT RELEASE
   • Create an annotated tag ""v1.0.0"" on the current main branch
   • This marks the official release after all features are merged
   • Command: git tag -a v1.0.0 -m ""Release version 1.0.0""

5. VERIFY CLEAN WORKING TREE
   • After all the above steps, your working tree should be clean
   • The stash should contain your saved work
   • All branches should exist
   • The tag should be present

╔══════════════════════════════════════════════════════════════════════════════╗
║                           VALIDATION CRITERIA                                 ║
╚══════════════════════════════════════════════════════════════════════════════╝

Your repository will be checked for:
✓ Working tree is clean (no uncommitted changes)
✓ Stash list contains at least 1 entry
✓ Main branch contains the recovered ""CRITICAL: Security patch"" commit
✓ Main branch contains a merge commit from feature-1
✓ File critical-fix.js exists (from recovered commit)
✓ File feature1.js exists (from feature-1 merge)
✓ File feature2.js exists (already merged)
✓ Tag ""v1.0.0"" exists
✓ Tag ""v1.0.0"" points to current HEAD on main branch
✓ All 3 branches exist (main, feature-1, feature-2)
✓ Current branch is main

╔══════════════════════════════════════════════════════════════════════════════╗
║                          RECOMMENDED COMMAND ORDER                            ║
╚══════════════════════════════════════════════════════════════════════════════╝

Step 1: Check current status
  git status

Step 2: Stash uncommitted changes
  git stash push -m ""Save work in progress""
  git status    (verify working tree is clean)

Step 3: Find and recover the lost commit
  git reflog    (look for ""CRITICAL: Security patch"")
  git cherry-pick <commit-hash>

Step 4: Merge feature-1 and resolve conflicts
  git merge feature-1
  git status    (see the conflict)
  git checkout --theirs core.js
  git add core.js
  git commit --no-edit

Step 5: Create the release tag
  git tag -a v1.0.0 -m ""Release version 1.0.0""
  git tag       (verify tag was created)

Step 6: Final verification
  git status    (should show clean working tree)
  git log --oneline -10    (review recent commits)
  git branch    (see all branches)
  git stash list    (verify stash exists)

╔══════════════════════════════════════════════════════════════════════════════╗
║                            HELPFUL COMMANDS                                   ║
╚══════════════════════════════════════════════════════════════════════════════╝

git stash push -m ""message""     Create a new stash with description
git stash list                    Show all stashes
git reflog                        Show history of HEAD movements
git cherry-pick <hash>            Apply a commit from elsewhere
git merge <branch>                Merge another branch into current
git checkout --theirs <file>      Resolve conflict using their version
git tag -a <name> -m ""msg""        Create annotated tag
git tag                           List all tags
git branch                        List all branches
git log --oneline -n              Show recent commits

This is your final test. Everything you've learned across all 23 rooms comes together here.
Use your mastery of git to restore order to this chaos!

Good luck, Git Master!
";

                await File.WriteAllTextAsync(Path.Combine(workingDir, "MISSION_BRIEFING.txt"), instructionsContent);
            },
            customValidator: async (workingDir, gitExec) =>
            {
                var failureMessages = new List<string>();
                var validationsPassed = 0;
                const int totalValidations = 11;

                // Check 1: Working tree must be clean
                var status = await gitExec.GetStatusAsync(workingDir);
                var isClean = status.Contains("working tree clean", StringComparison.OrdinalIgnoreCase) ||
                              status.Contains("nothing to commit", StringComparison.OrdinalIgnoreCase);

                if (!isClean)
                {
                    failureMessages.Add("Working tree is not clean. You need to stash uncommitted changes.");
                }
                else
                {
                    validationsPassed++;
                }

                // Check 2: Stash list should contain at least one entry
                var stashResult = await gitExec.ExecuteAsync("stash list", workingDir);
                if (string.IsNullOrWhiteSpace(stashResult.Output) || !stashResult.Output.Contains("stash@{"))
                {
                    failureMessages.Add("No stash found. You need to stash the uncommitted changes.");
                }
                else
                {
                    validationsPassed++;
                }

                // Check 3: Verify we're on main branch
                var branchResult = await gitExec.ExecuteAsync("branch --show-current", workingDir);
                var currentBranch = branchResult.Output?.Trim() ?? "";
                if (currentBranch != "main")
                {
                    failureMessages.Add($"Current branch is '{currentBranch}', should be 'main'.");
                }
                else
                {
                    validationsPassed++;
                }

                // Check 4: Verify the critical commit was recovered (check commit messages)
                var log = await gitExec.GetLogAsync(workingDir, 15);
                if (!log.Contains("CRITICAL: Security patch", StringComparison.OrdinalIgnoreCase) &&
                    !log.Contains("Security patch", StringComparison.OrdinalIgnoreCase))
                {
                    failureMessages.Add("The critical security patch commit was not recovered from reflog. Use 'git reflog' to find it and 'git cherry-pick <hash>' to recover it.");
                }
                else
                {
                    validationsPassed++;
                }

                // Check 5: Verify critical-fix.js file exists (from recovered commit)
                var criticalFixPath = Path.Combine(workingDir, "critical-fix.js");
                if (!File.Exists(criticalFixPath))
                {
                    failureMessages.Add("critical-fix.js file doesn't exist. The critical commit hasn't been recovered yet.");
                }
                else
                {
                    validationsPassed++;
                }

                // Check 6: Verify feature-1 was merged (check for merge commit)
                if (!log.Contains("Merge branch 'feature-1'", StringComparison.OrdinalIgnoreCase) &&
                    !log.Contains("merge feature-1", StringComparison.OrdinalIgnoreCase))
                {
                    failureMessages.Add("feature-1 branch has not been merged into main. Merge it and resolve conflicts.");
                }
                else
                {
                    validationsPassed++;
                }

                // Check 7: Verify feature1.js exists (from feature-1 merge)
                var feature1Path = Path.Combine(workingDir, "feature1.js");
                if (!File.Exists(feature1Path))
                {
                    failureMessages.Add("feature1.js doesn't exist. feature-1 branch needs to be merged.");
                }
                else
                {
                    validationsPassed++;
                }

                // Check 8: Verify feature2.js exists (should already be there from setup)
                var feature2Path = Path.Combine(workingDir, "feature2.js");
                if (!File.Exists(feature2Path))
                {
                    failureMessages.Add("feature2.js doesn't exist. This is unexpected - feature-2 should have been merged during setup.");
                }
                else
                {
                    validationsPassed++;
                }

                // Check 9: Verify tag v1.0.0 exists
                var tagResult = await gitExec.ExecuteAsync("tag", workingDir);
                if (!tagResult.Output?.Contains("v1.0.0") ?? true)
                {
                    failureMessages.Add("Tag 'v1.0.0' doesn't exist. Create it with: git tag -a v1.0.0 -m \"Release version 1.0.0\"");
                }
                else
                {
                    validationsPassed++;
                }

                // Check 10: Verify tag points to current HEAD
                // Use ^{commit} to dereference annotated tags to their commit
                var tagCommitResult = await gitExec.ExecuteAsync("rev-parse v1.0.0^{commit}", workingDir);
                var headCommitResult = await gitExec.ExecuteAsync("rev-parse HEAD", workingDir);
                var tagCommit = tagCommitResult.Output?.Trim() ?? "";
                var headCommit = headCommitResult.Output?.Trim() ?? "";

                if (tagCommit != headCommit && !string.IsNullOrEmpty(tagCommit))
                {
                    failureMessages.Add("Tag 'v1.0.0' exists but doesn't point to current HEAD. It should tag the latest commit.");
                }
                else if (!string.IsNullOrEmpty(tagCommit))
                {
                    validationsPassed++;
                }

                // Check 11: Verify all 3 branches exist
                var branchListResult = await gitExec.ExecuteAsync("branch", workingDir);
                var branches = branchListResult.Output ?? "";
                var hasMain = branches.Contains("main");
                var hasFeature1 = branches.Contains("feature-1");
                var hasFeature2 = branches.Contains("feature-2");

                if (!hasMain || !hasFeature1 || !hasFeature2)
                {
                    var missingBranches = new List<string>();
                    if (!hasMain) missingBranches.Add("main");
                    if (!hasFeature1) missingBranches.Add("feature-1");
                    if (!hasFeature2) missingBranches.Add("feature-2");
                    failureMessages.Add($"Missing branches: {string.Join(", ", missingBranches)}");
                }
                else
                {
                    validationsPassed++;
                }

                // If not all validations passed, return detailed failure message
                if (failureMessages.Any())
                {
                    var progressMessage = $"\n[yellow]Progress: {validationsPassed}/{totalValidations} checks passed[/]\n\n" +
                                         "[red]Remaining issues:[/]\n" +
                                         string.Join("\n", failureMessages.Select(m => $"  ✗ {m}"));

                    return new ChallengeResult(
                        false,
                        progressMessage,
                        "Read MISSION_BRIEFING.txt for detailed instructions. Complete all tasks in order: stash, recover lost commit, merge with conflict resolution, tag release."
                    );
                }

                // ALL CHECKS PASSED - Epic success message!
                return new ChallengeResult(
                    true,
                    @"
╔══════════════════════════════════════════════════════════════════════════════╗
║                                                                              ║
║                         ⚔️  VICTORY ACHIEVED! ⚔️                              ║
║                                                                              ║
║                    THE REPOSITORY HAS BEEN SAVED!                            ║
║                                                                              ║
╚══════════════════════════════════════════════════════════════════════════════╝

[bold green]CONGRATULATIONS, GIT MASTER![/]

You have conquered the Final Gauntlet and demonstrated complete mastery of Git!

[yellow]═══ Your Achievements ═══[/]

Throughout your journey across all 23 rooms, you have mastered:

[cyan]Fundamentals:[/]
  ✓ Repository initialization (git init)
  ✓ Staging and committing changes (git add, git commit)
  ✓ Viewing history and status (git log, git status)
  ✓ Understanding the git workflow

[cyan]Branching & Merging:[/]
  ✓ Creating and switching branches (git branch, git checkout)
  ✓ Merging branches (git merge)
  ✓ Resolving merge conflicts
  ✓ Working with multiple timelines

[cyan]Advanced Workflows:[/]
  ✓ Undoing changes (git restore, git reset)
  ✓ Temporary storage (git stash)
  ✓ Selective commits (git cherry-pick)
  ✓ Rewriting history (git rebase)
  ✓ Interactive staging (git add -p)

[cyan]Repository Management:[/]
  ✓ Tagging releases (git tag)
  ✓ Recovering lost commits (git reflog)
  ✓ Working with remotes (git remote, git fetch, git push)
  ✓ Binary search debugging (git bisect)
  ✓ Managing worktrees (git worktree)
  ✓ Investigating history (git blame)
  ✓ Automation with hooks
  ✓ Nested repositories (git submodule)

[cyan]In This Final Challenge:[/]
  ✓ Cleaned working tree with git stash
  ✓ Recovered lost commit from reflog using cherry-pick
  ✓ Merged feature branch and resolved conflicts
  ✓ Tagged release for version management
  ✓ Orchestrated complex multi-step repository rescue

[yellow]═══ What You've Proven ═══[/]

You've demonstrated the ability to:
• Navigate complex repository states
• Recover from mistakes and accidents
• Manage parallel development streams
• Prepare code for production releases
• Handle real-world git scenarios with confidence

[yellow]═══ The Journey Continues ═══[/]

Your mastery of Git is now complete within these halls, but your journey as a
developer continues! Git is the foundation of modern software development, and
you now possess the skills to:

• Collaborate confidently with teams of any size
• Contribute to open source projects worldwide
• Manage complex codebases with ease
• Recover from any git mishap
• Teach others the ways of version control

[green]═══ Final Words ═══[/]

Remember: Git is a tool of immense power, but also great responsibility.
• Commit often, commit meaningfully
• Write clear commit messages for your future self
• Never force-push to shared branches without communication
• Use branches to experiment fearlessly
• When in doubt, git reflog is your safety net

[bold cyan]You have completed all 23 rooms of GitOut![/]
[bold cyan]The Git universe is now yours to command![/]

[dim]May your commits be atomic, your merges conflict-free, and your deployments successful![/]

[bold green]🎉 GAME COMPLETED! 🎉[/]
",
                    null
                );
            }
        );

        var room = new Room(
            id: "room-23",
            name: "The Final Gauntlet",
            description: "The ultimate proving ground where all your git mastery is tested",
            narrative: @"You step into a vast chamber pulsing with raw git energy. Unlike the focused challenges
before, this room presents a scenario of complete chaos - a real-world disaster that demands
every skill you've learned.

Before you materializes a massive, corrupted repository - its structure fractured, its history
tangled, its branches in conflict. A holographic message appears:

[yellow]═══ EMERGENCY TRANSMISSION ═══[/]

[red]PRIORITY: CRITICAL[/]

This repository powers the very fabric of our realm. It has fallen into disarray after a
catastrophic series of mishaps. Multiple developers worked simultaneously without coordination,
commits were lost, conflicts remain unresolved, and releases were never tagged.

You are the last hope. You must:
• Clean up uncommitted chaos
• Recover what was lost
• Unify the fractured timelines
• Prepare for release

This is not a drill. This is THE FINAL GAUNTLET.

[yellow]═══ THE CHALLENGE ═══[/]

Read MISSION_BRIEFING.txt for complete instructions.

This challenge combines:
  • Stashing (Room 10)
  • Reflog recovery (Room 14)
  • Merge conflict resolution (Room 9)
  • Cherry-picking (Room 11)
  • Tagging (Room 13)
  • Repository state management

There is no single command that will save you. You must orchestrate multiple git operations
in sequence, making strategic decisions at each step. The repository will only be saved when
ALL criteria are met.

[cyan]Type 'git status' to begin your assessment.[/]
[cyan]Read the mission briefing: cat MISSION_BRIEFING.txt[/]
[cyan](On Windows: type MISSION_BRIEFING.txt)[/]

The fate of the repository - and your journey - ends here.

[bold]Are you ready to prove your mastery?[/]",
            challenge: challenge,
            exits: new Dictionary<string, string>(), // No exits - this is the final room!
            isStartRoom: false,
            isEndRoom: true // This is the end!
        );

        return Task.FromResult(room);
    }
}
