using FluentAssertions;
using GitOut.Domain.Challenges;
using GitOut.Infrastructure.Tests.Fixtures;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration;

/// <summary>
/// Integration tests for Phase 3 rooms (Room 9-16)
/// Tests intermediate git concepts: conflicts, stash, cherry-pick, rebase, tags, reflog, remotes, bisect
/// </summary>
public class Phase3RoomsTests : RoomIntegrationTestFixture
{

    [Fact]
    public async Task Room9_ConflictCatacombs_ShouldCompleteWhenConflictIsResolved()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room9-test");
        var room = await RoomHelper.LoadRoomAsync("room-9");

        room.Name.Should().Be("The Conflict Catacombs");

        // Act - Setup creates a merge conflict
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify conflict exists
        await GitHelper.VerifyHasConflictsAsync(workingDirectory);

        // Read the conflicted file
        var spellBookPath = Path.Combine(workingDirectory, "spell-book.txt");
        var conflictedContent = await File.ReadAllTextAsync(spellBookPath);
        conflictedContent.Should().Contain("<<<<<<<", "Conflict markers should be present");

        // Resolve conflict by choosing one version
        await File.WriteAllTextAsync(spellBookPath,
            "Chapter 1: Advanced Fire Magic\nChapter 2: Water Magic\nChapter 3: Earth Magic");

        // Stage the resolved file
        await GitExecutor.ExecuteAsync("add spell-book.txt", workingDirectory);

        // Complete the merge
        await GitExecutor.ExecuteAsync("commit -m \"Resolve merge conflict\"", workingDirectory);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "harmonized");
    }

    [Fact]
    public async Task Room9_ConflictCatacombs_ShouldCompleteUsingGitCheckoutOurs()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room9-ours-test");
        var room = await RoomHelper.LoadRoomAsync("room-9");

        room.Name.Should().Be("The Conflict Catacombs");

        // Act - Setup creates a merge conflict
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify conflict exists
        await GitHelper.VerifyHasConflictsAsync(workingDirectory);

        // Resolve using git checkout --ours (keep main's version)
        var checkoutResult = await GitExecutor.ExecuteAsync("checkout --ours spell-book.txt", workingDirectory);
        checkoutResult.Success.Should().BeTrue($"checkout --ours should succeed: {checkoutResult.Error}");

        // Stage and commit (use --no-edit to accept git's auto-generated merge message)
        var addResult = await GitExecutor.ExecuteAsync("add spell-book.txt", workingDirectory);
        addResult.Success.Should().BeTrue($"add should succeed: {addResult.Error}");

        var commitResult = await GitExecutor.ExecuteAsync("commit --no-edit", workingDirectory);
        commitResult.Success.Should().BeTrue($"commit should succeed: {commitResult.Error}");

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        if (!validationResult.IsSuccessful)
        {
            // Debug output
            var status = await GitExecutor.GetStatusAsync(workingDirectory);
            var log = await GitExecutor.GetLogAsync(workingDirectory, 5);
            throw new Exception($"Validation failed: {validationResult.Message}\nHint: {validationResult.Hint}\nStatus: {status}\nLog: {log}");
        }
        RoomHelper.VerifySuccess(validationResult, "harmonized");
    }

    [Fact]
    public async Task Room9_ConflictCatacombs_ShouldCompleteUsingGitCheckoutTheirs()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room9-theirs-test");
        var room = await RoomHelper.LoadRoomAsync("room-9");

        // Act - Setup creates a merge conflict
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Resolve using git checkout --theirs (keep branch's version)
        await GitExecutor.ExecuteAsync("checkout --theirs spell-book.txt", workingDirectory);

        // Stage and commit (use --no-edit to accept git's auto-generated merge message)
        await GitExecutor.ExecuteAsync("add spell-book.txt", workingDirectory);
        await GitExecutor.ExecuteAsync("commit --no-edit", workingDirectory);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "harmonized");
    }

    [Fact]
    public async Task Room9_ConflictCatacombs_ShouldFailWhenConflictNotResolved()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room9-fail-test");
        var room = await RoomHelper.LoadRoomAsync("room-9");

        // Act - Setup only
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Act - Validate without resolving
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifyFailure(validationResult, "clash");
    }

    [Fact]
    public async Task Room10_StashSanctum_ShouldCompleteWhenWorkIsStashed()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room10-test");
        var room = await RoomHelper.LoadRoomAsync("room-10");

        room.Name.Should().Be("The Stash Sanctum");

        // Act - Setup creates work in progress
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify there are changes
        var statusBefore = await GitExecutor.GetStatusAsync(workingDirectory);
        statusBefore.Should().Contain("modified", "Setup should create modified files");

        // Stash the work (including untracked files)
        await GitHelper.StashAsync(workingDirectory, includeUntracked: true);

        // Verify working directory is clean
        await GitHelper.VerifyWorkingTreeCleanAsync(workingDirectory);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "stash");
    }

    [Fact]
    public async Task Room10_StashSanctum_ShouldFailWhenStashingWithoutUntrackedFiles()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room10-fail-test");
        var room = await RoomHelper.LoadRoomAsync("room-10");

        room.Name.Should().Be("The Stash Sanctum");

        // Act - Setup creates work in progress (both modified and untracked files)
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Stash without -u flag (does not include untracked files)
        await GitHelper.StashAsync(workingDirectory, includeUntracked: false);

        // Verify stash was created
        await GitHelper.VerifyStashExistsAsync(workingDirectory);

        // Verify working directory still has untracked files
        var statusAfter = await GitExecutor.GetStatusAsync(workingDirectory);
        statusAfter.Should().Contain("notes.txt", "Untracked file should remain");
        statusAfter.Should().NotContain("working tree clean", "Working directory should not be clean");

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        validationResult.IsSuccessful.Should().BeFalse("Should fail when untracked files remain");
        validationResult.Message.Should().Contain("clean");
        validationResult.Hint.Should().Contain("-u");
    }

    [Fact]
    public async Task Room11_CherryPickGarden_ShouldCompleteWhenCorrectCommitIsPicked()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room11-test");
        var room = await RoomHelper.LoadRoomAsync("room-11");

        room.Name.Should().Be("The Cherry-Pick Garden");

        // Act - Setup creates branches with commits
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Read the reference file to get commit hash
        var refFilePath = Path.Combine(workingDirectory, "CHERRY_PICK_THIS.txt");
        var refContent = await File.ReadAllTextAsync(refFilePath);
        refContent.Should().Contain("Cherry-pick this commit");

        // Extract commit hash from file
        var lines = refContent.Split('\n');
        var hashLine = lines[0];
        var commitHash = hashLine.Split(':')[1].Trim();

        // Verify we're on main
        await GitHelper.VerifyCurrentBranchAsync(workingDirectory, "main");

        // Cherry-pick the lavender commit
        var cherryPickResult = await GitExecutor.ExecuteAsync($"cherry-pick {commitHash}", workingDirectory);
        cherryPickResult.Success.Should().BeTrue("Cherry-pick should succeed");

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "lavender");

        // Verify file content
        await GitHelper.VerifyFileContainsAsync(workingDirectory, "garden.txt", "Lavender");
        var gardenContent = await File.ReadAllTextAsync(Path.Combine(workingDirectory, "garden.txt"));
        gardenContent.Should().NotContain("PoisonIvy");
    }

    [Fact]
    public async Task Room12_RebaseRidge_ShouldCompleteWhenFeatureBranchIsRebased()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room12-test");
        var room = await RoomHelper.LoadRoomAsync("room-12");

        room.Name.Should().Be("The Rebase Ridge");

        // Act - Setup creates diverged branches
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify we're on feature-timeline
        await GitHelper.VerifyCurrentBranchAsync(workingDirectory, "feature-timeline");

        // Rebase feature branch onto main
        // Note: This might create conflicts in real scenarios, but our setup creates non-conflicting changes
        var rebaseResult = await GitExecutor.ExecuteAsync("rebase main", workingDirectory);

        // If there are conflicts, we need to resolve them
        if (!rebaseResult.Success)
        {
            // Check if it's a conflict
            var hasConflicts = await GitExecutor.HasConflictsAsync(workingDirectory);
            if (hasConflicts)
            {
                // Resolve by keeping the feature version
                var timelinePath = Path.Combine(workingDirectory, "timeline.txt");
                var content = await File.ReadAllTextAsync(timelinePath);

                // Remove conflict markers and keep both changes
                content = content.Replace("<<<<<<< HEAD", "")
                                 .Replace("=======", "")
                                 .Replace(">>>>>>> main", "")
                                 .Trim();

                // Or just write the expected combined version
                await File.WriteAllTextAsync(timelinePath,
                    "Event 1: Beginning\nEvent A: Main progress\nEvent B: More main work\nEvent 2: Feature work");

                await GitExecutor.ExecuteAsync("add timeline.txt", workingDirectory);
                await GitExecutor.ExecuteAsync("rebase --continue", workingDirectory);
            }
        }

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "realigned");

        // Verify log contains commits from both branches
        await GitHelper.VerifyLogContainsAsync(workingDirectory, "Event A");
        await GitHelper.VerifyLogContainsAsync(workingDirectory, "Event B");
        await GitHelper.VerifyLogContainsAsync(workingDirectory, "Event 2");
    }

    [Fact]
    public async Task Room13_TagTower_ShouldCompleteWhenRequiredTagsAreCreated()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room13-test");
        var room = await RoomHelper.LoadRoomAsync("room-13");

        room.Name.Should().Be("The Tag Tower");

        // Act - Setup creates version commits
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Get commit log
        var log = await GitExecutor.GetLogAsync(workingDirectory, 10);
        var logLines = log.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Find the commits for v1.0.0 and v2.0.0
        string? v1Commit = null;
        string? v2Commit = null;

        foreach (var line in logLines)
        {
            if (line.Contains("version 1.0.0"))
            {
                v1Commit = line.Split(' ')[0]; // First part is commit hash
            }
            else if (line.Contains("version 2.0.0"))
            {
                v2Commit = line.Split(' ')[0];
            }
        }

        v1Commit.Should().NotBeNull("Should find v1.0.0 commit");
        v2Commit.Should().NotBeNull("Should find v2.0.0 commit");

        // Create tags
        await GitHelper.CreateTagAsync(workingDirectory, "v1.0.0", v1Commit);
        await GitHelper.CreateTagAsync(workingDirectory, "v2.0.0", v2Commit);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "tagged");

        // Verify tags exist
        await GitHelper.VerifyTagExistsAsync(workingDirectory, "v1.0.0");
        await GitHelper.VerifyTagExistsAsync(workingDirectory, "v2.0.0");
    }

    [Fact]
    public async Task Room14_ReflogRuins_ShouldCompleteWhenLostCommitIsRecovered()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room14-test");
        var room = await RoomHelper.LoadRoomAsync("room-14");

        room.Name.Should().Be("The Reflog Ruins");

        // Act - Setup creates and "loses" a commit
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify notes.txt doesn't exist (commit was reset away)
        GitHelper.VerifyFileDoesNotExist(workingDirectory, "notes.txt");

        // Use reflog to find the lost commit
        var reflog = await GitExecutor.GetReflogAsync(workingDirectory, 20);
        reflog.Should().Contain("Important notes", "Reflog should contain the lost commit");

        // Parse reflog to find the commit hash
        var reflogLines = reflog.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        string? lostCommitHash = null;

        foreach (var line in reflogLines)
        {
            if (line.Contains("Important notes"))
            {
                lostCommitHash = line.Split(' ')[0]; // First part is commit hash
                break;
            }
        }

        lostCommitHash.Should().NotBeNull("Should find lost commit in reflog");

        // Recover the lost commit
        await GitHelper.ResetAsync(workingDirectory, lostCommitHash!, hard: true);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "recovered");

        // Verify notes.txt now exists
        GitHelper.VerifyFileExists(workingDirectory, "notes.txt");
    }

    [Fact]
    public async Task Room15_RemoteRealm_ShouldCompleteWhenRemoteIsAdded()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room15-test");
        var room = await RoomHelper.LoadRoomAsync("room-15");

        room.Name.Should().Be("The Remote Realm");

        // Act - Setup creates a remote repo
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Read the remote path
        var remotePathFile = Path.Combine(workingDirectory, "REMOTE_PATH.txt");
        var remotePathContent = await File.ReadAllTextAsync(remotePathFile);

        // Extract path from file - skip header line
        var lines = remotePathContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var remotePath = lines.Length > 1 ? lines[1] : lines[0]; // Second line or first if only one line

        // Add the remote
        await GitHelper.AddRemoteAsync(workingDirectory, "origin", remotePath);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "remote");

        // Verify remote exists
        await GitHelper.VerifyRemoteExistsAsync(workingDirectory, "origin");
    }

    [Fact]
    public async Task Room16_BisectBattlefield_ShouldCompleteWhenBugIsFound()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room16-test");
        var room = await RoomHelper.LoadRoomAsync("room-16");

        room.Name.Should().Be("The Bisect Battlefield");
        RoomHelper.VerifyEndRoom(room);

        // Act - Setup creates commits with a bug
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // For this test, we'll manually identify the bug instead of using bisect
        // (Testing actual bisect interactively is complex in automated tests)

        // We know from the setup that Version 6 has the bug
        // Create the FOUND_BUG.txt file
        var foundBugPath = Path.Combine(workingDirectory, "FOUND_BUG.txt");
        await File.WriteAllTextAsync(foundBugPath, "Version 6");

        // Stage and commit
        await GitExecutor.ExecuteAsync("add FOUND_BUG.txt", workingDirectory);
        await GitExecutor.ExecuteAsync("commit -m \"Found the bug\"", workingDirectory);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        validationResult.IsSuccessful.Should().BeTrue();
        validationResult.Message.Should().Contain("Victory");
        validationResult.Message.Should().Contain("Version 6");
    }

    [Fact]
    public async Task AllPhase3Rooms_ShouldExistAndHaveChallenges()
    {
        // Arrange & Act & Assert
        await RoomHelper.VerifyRoomRangeExistsAsync(9, 16);
    }

    [Fact]
    public async Task Phase3Rooms_ShouldHaveCorrectConnections()
    {
        // Arrange & Act
        var rooms = await RoomHelper.LoadAllRoomsAsync();

        // Assert - Verify room 8 connects to room 9
        RoomHelper.VerifyExit(rooms["room-8"], "forward", "room-9");

        // Verify Phase 3 room chain (9-16)
        await RoomHelper.VerifyRoomChainAsync(9, 10, 11, 12, 13, 14, 15, 16);
    }

    [Fact]
    public async Task Phase3Rooms_ShouldAllBeRepositoryChallenges()
    {
        // Arrange & Act
        var rooms = await RoomHelper.LoadAllRoomsAsync();

        // Assert - All Phase 3 rooms use RepositoryChallenge
        for (int i = 9; i <= 16; i++)
        {
            var roomId = $"room-{i}";
            RoomHelper.VerifyChallengeType<RepositoryChallenge>(rooms[roomId]);
        }
    }
}
