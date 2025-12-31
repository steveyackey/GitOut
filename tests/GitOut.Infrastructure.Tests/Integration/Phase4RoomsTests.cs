using FluentAssertions;
using GitOut.Domain.Challenges;
using GitOut.Infrastructure.Tests.Fixtures;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration;

/// <summary>
/// Integration tests for Phase 4 rooms (Room 17-23)
/// Tests advanced git concepts: worktree, blame, hooks, interactive staging, submodules, filter-branch, final boss
/// </summary>
public class Phase4RoomsTests : RoomIntegrationTestFixture
{
    [Fact]
    public async Task Room17_WorktreeWorkshop_ShouldCompleteWhenWorktreeCreated()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room17-test");
        var room = await RoomHelper.LoadRoomAsync("room-17");

        room.Name.Should().Be("The Worktree Workshop");

        // Act - Setup creates repo with initial commit
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify README.md was created
        var readmePath = Path.Combine(workingDirectory, "README.md");
        File.Exists(readmePath).Should().BeTrue("README.md should exist");

        // Verify guide was created
        var guidePath = Path.Combine(workingDirectory, "WORKTREE_GUIDE.txt");
        File.Exists(guidePath).Should().BeTrue("WORKTREE_GUIDE.txt should exist");

        // Player creates a worktree
        // Use a unique name based on the working directory to avoid conflicts
        var workingDirName = Path.GetFileName(workingDirectory);
        var featureWorkName = $"{workingDirName}-feature-work";
        var featureDir = Path.Combine(Path.GetDirectoryName(workingDirectory)!, featureWorkName);

        // Clean up any existing directory from previous test runs
        if (Directory.Exists(featureDir))
        {
            Directory.Delete(featureDir, true);
        }

        var worktreeResult = await GitExecutor.ExecuteAsync($"worktree add ../{featureWorkName} -b feature", workingDirectory);
        worktreeResult.Success.Should().BeTrue($"Worktree creation should succeed: {worktreeResult.Error}");

        // Verify worktree was created
        Directory.Exists(featureDir).Should().BeTrue("Feature worktree directory should exist");

        // Verify worktree list shows 2 worktrees
        var listResult = await GitExecutor.ExecuteAsync("worktree list", workingDirectory);
        listResult.Success.Should().BeTrue();
        var worktreeCount = listResult.Output?.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length ?? 0;
        worktreeCount.Should().BeGreaterThanOrEqualTo(2, "Should have at least 2 worktrees");

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "worktree");
    }

    [Fact]
    public async Task Room17_WorktreeWorkshop_ShouldFailWhenNoWorktreeCreated()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room17-fail-test");
        var room = await RoomHelper.LoadRoomAsync("room-17");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Don't create any worktree

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifyFailure(validationResult, "1 worktree");
    }

    [Fact]
    public async Task Room18_BlameChamber_ShouldCompleteWhenCulpritIdentified()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room18-test");
        var room = await RoomHelper.LoadRoomAsync("room-18");

        room.Name.Should().Be("The Blame Chamber");

        // Act - Setup creates calculator.js with multiple commits
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify the investigation file was created
        var investigationPath = Path.Combine(workingDirectory, "INVESTIGATION.txt");
        File.Exists(investigationPath).Should().BeTrue("INVESTIGATION.txt should exist");

        // Verify calculator.js exists
        var calculatorPath = Path.Combine(workingDirectory, "calculator.js");
        File.Exists(calculatorPath).Should().BeTrue("calculator.js should exist");

        // Player uses git blame to investigate (simulated by knowing the answer from setup)
        // In real gameplay, player would run: git blame calculator.js
        // And see that line 3 was last modified by the commit "Fix bug: use amount instead of price"

        // Create the CULPRIT.txt file with the answer
        var culpritPath = Path.Combine(workingDirectory, "CULPRIT.txt");
        await File.WriteAllTextAsync(culpritPath, "Fix");
        await GitExecutor.ExecuteAsync("add CULPRIT.txt", workingDirectory);
        await GitExecutor.ExecuteAsync("commit -m \"Identify culprit\"", workingDirectory);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "detective");
    }

    [Fact]
    public async Task Room18_BlameChamber_ShouldFailWhenWrongCulpritIdentified()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room18-fail-test");
        var room = await RoomHelper.LoadRoomAsync("room-18");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Create CULPRIT.txt with wrong answer
        var culpritPath = Path.Combine(workingDirectory, "CULPRIT.txt");
        await File.WriteAllTextAsync(culpritPath, "Wrong");
        await GitExecutor.ExecuteAsync("add CULPRIT.txt", workingDirectory);
        await GitExecutor.ExecuteAsync("commit -m \"Wrong answer\"", workingDirectory);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifyFailure(validationResult, "not been identified");
    }

    [Fact]
    public async Task Room18_BlameChamber_ShouldFailWhenNoCulpritFile()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room18-nofile-test");
        var room = await RoomHelper.LoadRoomAsync("room-18");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Don't create CULPRIT.txt

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifyFailure(validationResult, "not been identified");
    }

    [Fact]
    public async Task Room20_InteractiveStagingHall_ShouldCompleteWhenPartialStaging()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room20-test");
        var room = await RoomHelper.LoadRoomAsync("room-20");

        room.Name.Should().Be("The Interactive Staging Hall");

        // Act - Setup creates utils.js with initial commit, then modifies file with unstaged changes
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify the file has unstaged changes after setup
        var statusAfterSetup = await GitExecutor.GetStatusAsync(workingDirectory);
        statusAfterSetup.Should().Contain("utils.js", "File should have unstaged changes after setup");

        // Simulate selective staging by creating a focused commit
        // First, let's manually create the partial content (greet with validation, but no multiply/subtract)
        var utilsPath = Path.Combine(workingDirectory, "utils.js");

        // Save the full content (has all changes)
        var fullContent = await File.ReadAllTextAsync(utilsPath);
        fullContent.Should().Contain("multiply", "Full content should have multiply function");
        fullContent.Should().Contain("subtract", "Full content should have subtract function");
        fullContent.Should().Contain("if (!name)", "Full content should have validation");

        // Create partial content with ONLY the greet validation change
        var partialContent = @"// Utility Functions

function greet(name) {
    if (!name) throw new Error('Name is required');
    return `Hello, ${name}!`;
}

function add(a, b) {
    return a + b;
}
";

        // Replace file with partial content, commit it, then restore full content
        await File.WriteAllTextAsync(utilsPath, partialContent);

        // Check status before add
        var statusBeforeAdd = await GitExecutor.GetStatusAsync(workingDirectory);

        var addResult = await GitExecutor.ExecuteAsync("add utils.js", workingDirectory);
        addResult.Success.Should().BeTrue($"Add should succeed: {addResult.Error}");

        // Check staged changes
        var diffCached = await GitExecutor.ExecuteAsync("diff --cached", workingDirectory);

        var commitResult = await GitExecutor.ExecuteAsync("commit -m \"Add validation to greet\"", workingDirectory);
        if (!commitResult.Success)
        {
            throw new Exception($"Commit failed!\nError: {commitResult.Error}\nOutput: {commitResult.Output}\n\nStatus before add:\n{statusBeforeAdd}\n\nDiff cached:\n{diffCached.Output}");
        }

        // Restore full content to make multiply/subtract unstaged again
        await File.WriteAllTextAsync(utilsPath, fullContent);

        // Verify working directory now has unstaged changes
        var statusAfterCommit = await GitExecutor.GetStatusAsync(workingDirectory);
        statusAfterCommit.Should().Contain("utils.js", "utils.js should have unstaged changes");

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        if (!validationResult.IsSuccessful)
        {
            // Debug: check what's in HEAD
            var headContent = await GitExecutor.ExecuteAsync("show HEAD:utils.js", workingDirectory);
            throw new Exception($"Validation failed: {validationResult.Message}\nHint: {validationResult.Hint}\n\nHEAD content:\n{headContent.Output}");
        }
        RoomHelper.VerifySuccess(validationResult, "selective");
    }

    [Fact]
    public async Task Room20_InteractiveStagingHall_ShouldFailWhenWorkingTreeClean()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room20-clean-test");
        var room = await RoomHelper.LoadRoomAsync("room-20");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Stage and commit ALL changes (wrong approach)
        await GitExecutor.ExecuteAsync("add utils.js", workingDirectory);
        await GitExecutor.ExecuteAsync("commit -m \"Add validation to greet\"", workingDirectory);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifyFailure(validationResult, "unstaged changes");
    }

    [Fact]
    public async Task Room20_InteractiveStagingHall_ShouldFailWhenCommitIncludesTooMuch()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room20-toomuch-test");
        var room = await RoomHelper.LoadRoomAsync("room-20");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Stage all, commit, but create a fake unstaged change
        await GitExecutor.ExecuteAsync("add utils.js", workingDirectory);
        await GitExecutor.ExecuteAsync("commit -m \"Add validation to greet\"", workingDirectory);

        // Add a different file to make working tree dirty
        await File.WriteAllTextAsync(Path.Combine(workingDirectory, "other.txt"), "other content");

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert - Should fail because commit includes multiply/subtract (working tree clean for utils.js)
        RoomHelper.VerifyFailure(validationResult, "unstaged changes");
    }

    [Fact]
    public async Task Room21_SubmoduleSanctum_ShouldCompleteWhenSubmoduleAdded()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room21-test");
        var room = await RoomHelper.LoadRoomAsync("room-21");

        room.Name.Should().Be("The Submodule Sanctum");

        // Act - Setup creates main repo + library repo in sibling directory
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify instructions file was created
        var instructionsPath = Path.Combine(workingDirectory, "SUBMODULE_INSTRUCTIONS.txt");
        File.Exists(instructionsPath).Should().BeTrue("SUBMODULE_INSTRUCTIONS.txt should exist");

        // Read the library path from the instructions
        var instructions = await File.ReadAllTextAsync(instructionsPath);
        var libraryPathLine = instructions.Split('\n').FirstOrDefault(line => line.Contains("magic-library"));
        libraryPathLine.Should().NotBeNullOrEmpty("Instructions should contain library path");

        var libraryPath = libraryPathLine!.Trim();

        // Verify library repo exists
        Directory.Exists(libraryPath).Should().BeTrue("Library repository should exist");

        // Player adds the submodule
        // Use -c flag to allow file:// protocol for this command only (security restriction in modern git)
        var submoduleAddResult = await GitExecutor.ExecuteAsync($"-c protocol.file.allow=always submodule add {libraryPath} lib", workingDirectory);
        submoduleAddResult.Success.Should().BeTrue($"Submodule add should succeed: {submoduleAddResult.Error}");

        // Verify .gitmodules was created
        var gitmodulesPath = Path.Combine(workingDirectory, ".gitmodules");
        File.Exists(gitmodulesPath).Should().BeTrue(".gitmodules should exist after adding submodule");

        // Commit the submodule addition
        await GitExecutor.ExecuteAsync("add .gitmodules lib", workingDirectory);
        var commitResult = await GitExecutor.ExecuteAsync("commit -m \"Add magic library as submodule\"", workingDirectory);
        commitResult.Success.Should().BeTrue($"Commit should succeed: {commitResult.Error}");

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        if (!validationResult.IsSuccessful)
        {
            throw new Exception($"Validation failed: {validationResult.Message}\nHint: {validationResult.Hint}");
        }
        RoomHelper.VerifySuccess(validationResult, "submodule");
    }

    [Fact]
    public async Task Room21_SubmoduleSanctum_ShouldFailWhenNoGitmodules()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room21-nogitmodules-test");
        var room = await RoomHelper.LoadRoomAsync("room-21");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Don't add submodule, just try to validate

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifyFailure(validationResult, ".gitmodules");
    }

    [Fact]
    public async Task Room21_SubmoduleSanctum_ShouldFailWhenSubmoduleNotCommitted()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room21-notcommitted-test");
        var room = await RoomHelper.LoadRoomAsync("room-21");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Read library path
        var instructions = await File.ReadAllTextAsync(Path.Combine(workingDirectory, "SUBMODULE_INSTRUCTIONS.txt"));
        var libraryPathLine = instructions.Split('\n').FirstOrDefault(line => line.Contains("magic-library"));
        var libraryPath = libraryPathLine!.Trim();

        // Add submodule but don't commit
        // Use -c flag to allow file:// protocol for this command only
        await GitExecutor.ExecuteAsync($"-c protocol.file.allow=always submodule add {libraryPath} lib", workingDirectory);

        // Act - Validate (should fail because not committed)
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifyFailure(validationResult, "not committed");
    }

    [Fact]
    public async Task Room21_SubmoduleSanctum_ShouldVerifySubmoduleContainsLibraryFiles()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room21-verify-test");
        var room = await RoomHelper.LoadRoomAsync("room-21");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Read library path
        var instructions = await File.ReadAllTextAsync(Path.Combine(workingDirectory, "SUBMODULE_INSTRUCTIONS.txt"));
        var libraryPathLine = instructions.Split('\n').FirstOrDefault(line => line.Contains("magic-library"));
        var libraryPath = libraryPathLine!.Trim();

        // Add and commit submodule
        // Use -c flag to allow file:// protocol for this command only
        await GitExecutor.ExecuteAsync($"-c protocol.file.allow=always submodule add {libraryPath} lib", workingDirectory);
        await GitExecutor.ExecuteAsync("add .gitmodules lib", workingDirectory);
        await GitExecutor.ExecuteAsync("commit -m \"Add magic library as submodule\"", workingDirectory);

        // Verify submodule files exist
        var magicSpellPath = Path.Combine(workingDirectory, "lib", "magic-spell.js");
        File.Exists(magicSpellPath).Should().BeTrue("Submodule should contain magic-spell.js");

        var readmePath = Path.Combine(workingDirectory, "lib", "README.md");
        File.Exists(readmePath).Should().BeTrue("Submodule should contain README.md");

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "submodule");
    }

    [Fact]
    public async Task Room19_HookHollow_ShouldCompleteWhenHookCreatedAndExecutable()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room19-test");
        var room = await RoomHelper.LoadRoomAsync("room-19");

        room.Name.Should().Be("The Hook Hollow");

        // Act - Setup creates hook template
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify template exists
        var templatePath = Path.Combine(workingDirectory, "pre-commit-template");
        File.Exists(templatePath).Should().BeTrue("Hook template should exist");

        // Verify .git/hooks directory exists
        var hooksDir = Path.Combine(workingDirectory, ".git", "hooks");
        Directory.Exists(hooksDir).Should().BeTrue(".git/hooks directory should exist");

        // Copy template to hooks directory
        var hookPath = Path.Combine(hooksDir, "pre-commit");
        File.Copy(templatePath, hookPath, overwrite: true);

        // Make it executable on Unix/Mac
        if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            File.SetUnixFileMode(hookPath, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "hook");
    }

    [Fact]
    public async Task Room19_HookHollow_ShouldFailWhenHookNotInstalled()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room19-noinstall-test");
        var room = await RoomHelper.LoadRoomAsync("room-19");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Don't install the hook - just leave template in working directory

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifyFailure(validationResult, "not been installed");
    }

    [Fact]
    public async Task Room19_HookHollow_ShouldFailWhenHookNotExecutableOnUnix()
    {
        // Skip this test on Windows
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            return;
        }

        // Arrange
        var workingDirectory = CreateWorkingDirectory("room19-notexecutable-test");
        var room = await RoomHelper.LoadRoomAsync("room-19");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Copy hook but don't make it executable
        var templatePath = Path.Combine(workingDirectory, "pre-commit-template");
        var hookPath = Path.Combine(workingDirectory, ".git", "hooks", "pre-commit");
        File.Copy(templatePath, hookPath, overwrite: true);

        // Explicitly remove execute permissions
        File.SetUnixFileMode(hookPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifyFailure(validationResult, "not executable");
    }

    [Fact]
    public async Task Room22_RewriteReliquary_ShouldCompleteWhenSecretsRemovedFromHistory()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room22-test");
        var room = await RoomHelper.LoadRoomAsync("room-22");

        room.Name.Should().Be("The Rewrite Reliquary");

        // Act - Setup creates repo with secrets.txt in commit history
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify the setup created secrets.txt in history
        var secretsPath = Path.Combine(workingDirectory, "secrets.txt");
        File.Exists(secretsPath).Should().BeTrue("secrets.txt should exist after setup");

        // Verify secrets.txt appears in git history before filter-branch
        var logBefore = await GitExecutor.ExecuteAsync("log --all --oneline -- secrets.txt", workingDirectory);
        logBefore.Success.Should().BeTrue();
        logBefore.Output.Should().NotBeNullOrWhiteSpace("secrets.txt should be in history before filter-branch");

        // Player uses filter-branch to remove secrets.txt from all history
        // Use -c to set config option that squelches the deprecation warning
        var filterBranchResult = await GitExecutor.ExecuteAsync(
            "-c filter.branch.squelchWarning=true filter-branch --force --index-filter \"git rm --cached --ignore-unmatch secrets.txt\" --prune-empty --tag-name-filter cat -- --all",
            workingDirectory);

        filterBranchResult.Success.Should().BeTrue($"Filter-branch should succeed: {filterBranchResult.Error}\nOutput: {filterBranchResult.Output}");

        // Delete the file from working directory as well
        if (File.Exists(secretsPath))
        {
            File.Delete(secretsPath);
        }

        // Verify secrets.txt is gone from HEAD history (not --all, because filter-branch creates backup refs)
        var logAfter = await GitExecutor.ExecuteAsync("log --oneline HEAD -- secrets.txt", workingDirectory);
        logAfter.Success.Should().BeTrue();
        var logAfterOutput = logAfter.Output?.Trim() ?? string.Empty;
        logAfterOutput.Should().BeEmpty("secrets.txt should not appear in any commit after filter-branch");

        // Verify other files still exist
        var readmeLog = await GitExecutor.ExecuteAsync("log --all --oneline -- README.md", workingDirectory);
        readmeLog.Success.Should().BeTrue();
        readmeLog.Output.Should().NotBeNullOrWhiteSpace("README.md should still be in history");

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        if (!validationResult.IsSuccessful)
        {
            throw new Exception($"Validation failed: {validationResult.Message}\nHint: {validationResult.Hint}");
        }
        RoomHelper.VerifySuccess(validationResult, "history");
    }

    [Fact]
    public async Task Room22_RewriteReliquary_ShouldFailWhenSecretsStillInHistory()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room22-fail-test");
        var room = await RoomHelper.LoadRoomAsync("room-22");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Don't run filter-branch - secrets.txt remains in history

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert - Will fail because secrets.txt file still exists (checked before history)
        RoomHelper.VerifyFailure(validationResult, "still exists");
    }

    [Fact]
    public async Task Room22_RewriteReliquary_ShouldFailWhenSecretsFileStillExists()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room22-fileexists-test");
        var room = await RoomHelper.LoadRoomAsync("room-22");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Run filter-branch to remove from history
        // NOTE: filter-branch removes the file from BOTH history AND working directory
        await GitExecutor.ExecuteAsync(
            "-c filter.branch.squelchWarning=true filter-branch --force --index-filter \"git rm --cached --ignore-unmatch secrets.txt\" --prune-empty --tag-name-filter cat -- --all",
            workingDirectory);

        // Verify file was removed from working directory by filter-branch
        var secretsPath = Path.Combine(workingDirectory, "secrets.txt");
        File.Exists(secretsPath).Should().BeFalse("filter-branch removes file from working directory");

        // Re-create the file to test validation
        await File.WriteAllTextAsync(secretsPath, "recreated secret");

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifyFailure(validationResult, "still exists");
    }

    [Fact]
    public async Task Room22_RewriteReliquary_ShouldVerifyOtherFilesRemainIntact()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room22-intact-test");
        var room = await RoomHelper.LoadRoomAsync("room-22");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify initial state has multiple files
        File.Exists(Path.Combine(workingDirectory, "README.md")).Should().BeTrue();
        File.Exists(Path.Combine(workingDirectory, "app.js")).Should().BeTrue();
        File.Exists(Path.Combine(workingDirectory, "utils.js")).Should().BeTrue();
        File.Exists(Path.Combine(workingDirectory, "config.example.js")).Should().BeTrue();

        // Player uses filter-branch correctly
        await GitExecutor.ExecuteAsync(
            "-c filter.branch.squelchWarning=true filter-branch --force --index-filter \"git rm --cached --ignore-unmatch secrets.txt\" --prune-empty --tag-name-filter cat -- --all",
            workingDirectory);

        // Delete secrets.txt
        var secretsPath = Path.Combine(workingDirectory, "secrets.txt");
        if (File.Exists(secretsPath))
        {
            File.Delete(secretsPath);
        }

        // Verify other files still exist in working directory
        File.Exists(Path.Combine(workingDirectory, "README.md")).Should().BeTrue("README.md should still exist");
        File.Exists(Path.Combine(workingDirectory, "app.js")).Should().BeTrue("app.js should still exist");
        File.Exists(Path.Combine(workingDirectory, "utils.js")).Should().BeTrue("utils.js should still exist");
        File.Exists(Path.Combine(workingDirectory, "config.example.js")).Should().BeTrue("config.example.js should still exist");

        // Verify other files still exist in history
        var readmeLog = await GitExecutor.ExecuteAsync("log --all --oneline -- README.md", workingDirectory);
        readmeLog.Output.Should().NotBeNullOrWhiteSpace("README.md should be in history");

        var appLog = await GitExecutor.ExecuteAsync("log --all --oneline -- app.js", workingDirectory);
        appLog.Output.Should().NotBeNullOrWhiteSpace("app.js should be in history");

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "history");
    }

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
