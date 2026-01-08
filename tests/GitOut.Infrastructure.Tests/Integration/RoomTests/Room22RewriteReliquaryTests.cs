using FluentAssertions;
using GitOut.Infrastructure.Tests.Fixtures;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration.RoomTests;

/// <summary>
/// Integration tests for Room 22: The Rewrite Reliquary (git filter-branch)
/// </summary>
public class Room22RewriteReliquaryTests : RoomIntegrationTestFixture
{
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
}
