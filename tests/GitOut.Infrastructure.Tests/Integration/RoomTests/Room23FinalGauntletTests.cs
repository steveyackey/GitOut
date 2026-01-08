using FluentAssertions;
using GitOut.Infrastructure.Tests.Fixtures;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration.RoomTests;

/// <summary>
/// Integration tests for Room 23: The Final Gauntlet (epic boss challenge)
/// </summary>
public class Room23FinalGauntletTests : RoomIntegrationTestFixture
{
    [Fact]
    public async Task Room23_FinalGauntlet_ShouldCompleteWhenAllTasksCompleted()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room23-success-test");
        var room = await RoomHelper.LoadRoomAsync("room-23");

        room.Name.Should().Be("The Final Gauntlet");
        room.IsEndRoom.Should().BeTrue("Room 23 should be marked as the end room");
        room.Exits.Should().BeEmpty("Room 23 should have no exits (final room)");

        // Act - Setup creates complex broken repo state
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify mission briefing exists
        var briefingPath = Path.Combine(workingDirectory, "MISSION_BRIEFING.txt");
        File.Exists(briefingPath).Should().BeTrue("MISSION_BRIEFING.txt should exist");

        // === TASK 1: Stash uncommitted changes ===
        var stashResult = await GitExecutor.ExecuteAsync("stash push -m \"Save work in progress\"", workingDirectory);
        stashResult.Success.Should().BeTrue($"Stash should succeed: {stashResult.Error}");

        // Verify stash was created
        var stashListResult = await GitExecutor.ExecuteAsync("stash list", workingDirectory);
        stashListResult.Output.Should().Contain("stash@{0}", "Stash should be created");

        // === TASK 2: Recover lost commit from reflog ===
        var reflogResult = await GitExecutor.ExecuteAsync("reflog", workingDirectory);
        reflogResult.Success.Should().BeTrue($"Reflog should succeed: {reflogResult.Error}");
        reflogResult.Output.Should().Contain("CRITICAL: Security patch", "Reflog should show the lost commit");

        // Find the commit hash for the critical commit
        var reflogLines = reflogResult.Output?.Split('\n') ?? Array.Empty<string>();
        var criticalLine = reflogLines.FirstOrDefault(line => line.Contains("CRITICAL: Security patch"));
        criticalLine.Should().NotBeNullOrEmpty("Should find the critical commit in reflog");

        var criticalHash = criticalLine!.Split(' ')[0];
        criticalHash.Should().NotBeNullOrEmpty("Should extract commit hash");

        // Cherry-pick the lost commit
        var cherryPickResult = await GitExecutor.ExecuteAsync($"cherry-pick {criticalHash}", workingDirectory);
        cherryPickResult.Success.Should().BeTrue($"Cherry-pick should succeed: {cherryPickResult.Error}");

        // Verify critical-fix.js now exists
        var criticalFixPath = Path.Combine(workingDirectory, "critical-fix.js");
        File.Exists(criticalFixPath).Should().BeTrue("critical-fix.js should exist after cherry-pick");

        // === TASK 3: Merge feature-1 with conflict resolution ===
        var mergeResult = await GitExecutor.ExecuteAsync("merge feature-1", workingDirectory);
        // Merge will fail due to conflict, that's expected
        mergeResult.Success.Should().BeFalse("Merge should fail due to conflict");

        // Check for conflicts
        var statusBeforeResolve = await GitExecutor.GetStatusAsync(workingDirectory);
        statusBeforeResolve.Should().Contain("both modified", "Should show conflict");

        // Resolve conflict by choosing feature-1's version
        var resolveResult = await GitExecutor.ExecuteAsync("checkout --theirs core.js", workingDirectory);
        resolveResult.Success.Should().BeTrue($"Conflict resolution should succeed: {resolveResult.Error}");

        // Stage resolved file
        var addResult = await GitExecutor.ExecuteAsync("add core.js", workingDirectory);
        addResult.Success.Should().BeTrue($"Add should succeed: {addResult.Error}");

        // Also need to add feature1.js which came from the merge
        await GitExecutor.ExecuteAsync("add feature1.js", workingDirectory);

        // Complete merge
        var commitResult = await GitExecutor.ExecuteAsync("commit --no-edit", workingDirectory);
        commitResult.Success.Should().BeTrue($"Merge commit should succeed: {commitResult.Error}");

        // Verify feature1.js exists after merge
        var feature1Path = Path.Combine(workingDirectory, "feature1.js");
        File.Exists(feature1Path).Should().BeTrue("feature1.js should exist after merge");

        // Clean up any leftover stash files that might show as untracked
        var statusAfterMerge = await GitExecutor.GetStatusAsync(workingDirectory);
        if (statusAfterMerge.Contains("Untracked files"))
        {
            // Clean untracked files
            await GitExecutor.ExecuteAsync("clean -fd", workingDirectory);
        }

        // === TASK 4: Create release tag (AFTER merge is complete) ===
        var tagResult = await GitExecutor.ExecuteAsync("tag -a v1.0.0 -m \"Release version 1.0.0\"", workingDirectory);
        tagResult.Success.Should().BeTrue($"Tag creation should succeed: {tagResult.Error}");

        // Verify tag exists
        var tagListResult = await GitExecutor.ExecuteAsync("tag", workingDirectory);
        tagListResult.Output.Should().Contain("v1.0.0", "Tag v1.0.0 should exist");

        // Debug: Show git log to understand commit structure
        var logResult = await GitExecutor.ExecuteAsync("log --oneline -10", workingDirectory);
        var headCommit = await GitExecutor.ExecuteAsync("rev-parse HEAD", workingDirectory);
        var tagCommit = await GitExecutor.ExecuteAsync("rev-parse v1.0.0", workingDirectory);

        // Act - Validate (the validator will check that working tree is clean)
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        if (!validationResult.IsSuccessful)
        {
            var debugInfo = $"\n\nDEBUG INFO:\n" +
                           $"Log:\n{logResult.Output}\n\n" +
                           $"HEAD: {headCommit.Output?.Trim()}\n" +
                           $"Tag:  {tagCommit.Output?.Trim()}\n";
            throw new Exception($"Validation failed!\n{validationResult.Message}\n\nHint: {validationResult.Hint}{debugInfo}");
        }

        validationResult.IsSuccessful.Should().BeTrue("Final gauntlet should be completed");
        validationResult.Message.Should().Contain("VICTORY ACHIEVED", "Success message should be epic");
        validationResult.Message.Should().Contain("GIT MASTER", "Success message should acknowledge mastery");
        validationResult.Message.Should().Contain("23 rooms", "Success message should reference full journey");
    }

    [Fact]
    public async Task Room23_FinalGauntlet_ShouldFailWhenNoActionsCompleted()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room23-noaction-test");
        var room = await RoomHelper.LoadRoomAsync("room-23");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Don't complete any tasks

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        validationResult.IsSuccessful.Should().BeFalse("Should fail when no tasks completed");
        // Note: Setup leaves repo on main with feature-2 merged, so 3 checks pass (on main, feature-2 exists, feature2.js exists)
        validationResult.Message.Should().Contain("3/11 checks passed", "Should show progress 3/11 from setup state");
        validationResult.Message.Should().Contain("Working tree is not clean", "Should mention uncommitted changes");
        validationResult.Message.Should().Contain("No stash found", "Should mention missing stash");
        validationResult.Hint.Should().Contain("MISSION_BRIEFING.txt", "Hint should reference instructions");
    }

    [Fact]
    public async Task Room23_FinalGauntlet_ShouldShowProgressWhenPartiallyCompleted()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room23-partial-test");
        var room = await RoomHelper.LoadRoomAsync("room-23");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Complete only TASK 1: Stash
        await GitExecutor.ExecuteAsync("stash push -m \"Save work in progress\"", workingDirectory);

        // Complete only TASK 2: Recover lost commit (but not the rest)
        var reflogResult = await GitExecutor.ExecuteAsync("reflog", workingDirectory);
        var reflogLines = reflogResult.Output?.Split('\n') ?? Array.Empty<string>();
        var criticalLine = reflogLines.FirstOrDefault(line => line.Contains("CRITICAL: Security patch"));
        var criticalHash = criticalLine!.Split(' ')[0];
        await GitExecutor.ExecuteAsync($"cherry-pick {criticalHash}", workingDirectory);

        // Don't complete merge, tag, etc.

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        validationResult.IsSuccessful.Should().BeFalse("Should fail when only partially completed");
        validationResult.Message.Should().Contain("checks passed", "Should show progress");

        // Should have passed at least 5 checks: clean tree, stash, current branch, critical commit recovered, critical-fix.js exists
        validationResult.Message.Should().Contain("/11", "Should show out of 11 total checks");

        validationResult.Message.Should().Contain("feature-1 branch has not been merged", "Should identify missing merge");
        validationResult.Message.Should().NotContain("No stash found", "Should NOT mention stash (already done)");
        validationResult.Message.Should().NotContain("critical security patch commit was not recovered", "Should NOT mention recovery (already done)");
    }

    [Fact]
    public async Task Room23_FinalGauntlet_ShouldRequireAllBranchesToExist()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room23-branches-test");
        var room = await RoomHelper.LoadRoomAsync("room-23");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Complete all tasks correctly
        await GitExecutor.ExecuteAsync("stash push -m \"Save work in progress\"", workingDirectory);

        var reflogResult = await GitExecutor.ExecuteAsync("reflog", workingDirectory);
        var reflogLines = reflogResult.Output?.Split('\n') ?? Array.Empty<string>();
        var criticalLine = reflogLines.FirstOrDefault(line => line.Contains("CRITICAL: Security patch"));
        var criticalHash = criticalLine!.Split(' ')[0];
        await GitExecutor.ExecuteAsync($"cherry-pick {criticalHash}", workingDirectory);

        await GitExecutor.ExecuteAsync("merge feature-1", workingDirectory);
        await GitExecutor.ExecuteAsync("checkout --theirs core.js", workingDirectory);
        await GitExecutor.ExecuteAsync("add core.js", workingDirectory);
        await GitExecutor.ExecuteAsync("commit --no-edit", workingDirectory);

        await GitExecutor.ExecuteAsync("tag -a v1.0.0 -m \"Release version 1.0.0\"", workingDirectory);

        // Delete a branch (breaking a requirement)
        await GitExecutor.ExecuteAsync("branch -D feature-2", workingDirectory);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        validationResult.IsSuccessful.Should().BeFalse("Should fail when branch is deleted");
        validationResult.Message.Should().Contain("Missing branches", "Should identify missing branch");
        validationResult.Message.Should().Contain("feature-2", "Should specifically mention feature-2");
    }

    [Fact]
    public async Task Room23_FinalGauntlet_ShouldRequireTagToPointToHead()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room23-tagpoint-test");
        var room = await RoomHelper.LoadRoomAsync("room-23");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Complete tasks but create tag at wrong commit
        await GitExecutor.ExecuteAsync("stash push -m \"Save work in progress\"", workingDirectory);

        // Create tag BEFORE recovering and merging (wrong location)
        await GitExecutor.ExecuteAsync("tag -a v1.0.0 -m \"Release version 1.0.0\"", workingDirectory);

        // Now continue with rest of tasks
        var reflogResult = await GitExecutor.ExecuteAsync("reflog", workingDirectory);
        var reflogLines = reflogResult.Output?.Split('\n') ?? Array.Empty<string>();
        var criticalLine = reflogLines.FirstOrDefault(line => line.Contains("CRITICAL: Security patch"));
        var criticalHash = criticalLine!.Split(' ')[0];
        await GitExecutor.ExecuteAsync($"cherry-pick {criticalHash}", workingDirectory);

        await GitExecutor.ExecuteAsync("merge feature-1", workingDirectory);
        await GitExecutor.ExecuteAsync("checkout --theirs core.js", workingDirectory);
        await GitExecutor.ExecuteAsync("add core.js", workingDirectory);
        await GitExecutor.ExecuteAsync("commit --no-edit", workingDirectory);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        validationResult.IsSuccessful.Should().BeFalse("Should fail when tag doesn't point to HEAD");
        validationResult.Message.Should().Contain("doesn't point to current HEAD", "Should identify tag location issue");
    }

    [Fact]
    public async Task Room23_FinalGauntlet_ShouldVerifyAllRequiredFilesExist()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room23-files-test");
        var room = await RoomHelper.LoadRoomAsync("room-23");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify all expected files are created by setup
        File.Exists(Path.Combine(workingDirectory, "README.md")).Should().BeTrue("README.md should exist");
        File.Exists(Path.Combine(workingDirectory, "core.js")).Should().BeTrue("core.js should exist");
        File.Exists(Path.Combine(workingDirectory, "feature2.js")).Should().BeTrue("feature2.js should exist");
        File.Exists(Path.Combine(workingDirectory, "work-in-progress.js")).Should().BeTrue("work-in-progress.js should exist");
        File.Exists(Path.Combine(workingDirectory, "notes.txt")).Should().BeTrue("notes.txt should exist");
        File.Exists(Path.Combine(workingDirectory, "MISSION_BRIEFING.txt")).Should().BeTrue("MISSION_BRIEFING.txt should exist");

        // Verify branches exist
        var branchResult = await GitExecutor.ExecuteAsync("branch", workingDirectory);
        branchResult.Output.Should().Contain("main", "main branch should exist");
        branchResult.Output.Should().Contain("feature-1", "feature-1 branch should exist");
        branchResult.Output.Should().Contain("feature-2", "feature-2 branch should exist");

        // Verify uncommitted changes exist
        var statusResult = await GitExecutor.GetStatusAsync(workingDirectory);
        statusResult.Should().Contain("work-in-progress.js", "Should have staged changes");
        statusResult.Should().Contain("notes.txt", "Should have unstaged changes");

        // Verify the lost commit is in reflog
        var reflogResult = await GitExecutor.ExecuteAsync("reflog", workingDirectory);
        reflogResult.Output.Should().Contain("CRITICAL: Security patch", "Lost commit should be in reflog");
    }
}
