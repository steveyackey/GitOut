using FluentAssertions;
using GitOut.Domain.Interfaces;

namespace GitOut.Infrastructure.Tests.Helpers;

/// <summary>
/// Helper class for common git operations used across tests
/// </summary>
public class GitTestHelper
{
    private readonly IGitCommandExecutor _gitExecutor;

    public GitTestHelper(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    /// <summary>
    /// Configure git user email and name for commits
    /// </summary>
    public async Task ConfigureGitUserAsync(string workingDirectory)
    {
        await _gitExecutor.ExecuteAsync("config user.email \"test@example.com\"", workingDirectory);
        await _gitExecutor.ExecuteAsync("config user.name \"Test User\"", workingDirectory);
    }

    /// <summary>
    /// Initialize a git repository
    /// </summary>
    public async Task InitializeRepositoryAsync(string workingDirectory)
    {
        var result = await _gitExecutor.ExecuteAsync("init", workingDirectory);
        result.Success.Should().BeTrue($"Git init should succeed: {result.Error}");
    }

    /// <summary>
    /// Create a file, stage it, and commit it
    /// </summary>
    public async Task CreateAndCommitFileAsync(
        string workingDirectory,
        string filename,
        string content,
        string commitMessage)
    {
        // Create file
        var filePath = Path.Combine(workingDirectory, filename);
        await File.WriteAllTextAsync(filePath, content);

        // Stage file
        var addResult = await _gitExecutor.ExecuteAsync($"add {filename}", workingDirectory);
        addResult.Success.Should().BeTrue($"Git add should succeed: {addResult.Error}");

        // Commit file
        var commitResult = await _gitExecutor.ExecuteAsync($"commit -m \"{commitMessage}\"", workingDirectory);
        commitResult.Success.Should().BeTrue($"Git commit should succeed: {commitResult.Error}");
    }

    /// <summary>
    /// Create a new branch and switch to it
    /// </summary>
    public async Task CreateBranchAsync(string workingDirectory, string branchName)
    {
        var result = await _gitExecutor.ExecuteAsync($"checkout -b {branchName}", workingDirectory);
        result.Success.Should().BeTrue($"Git checkout -b should succeed: {result.Error}");
    }

    /// <summary>
    /// Switch to an existing branch
    /// </summary>
    public async Task CheckoutBranchAsync(string workingDirectory, string branchName)
    {
        var result = await _gitExecutor.ExecuteAsync($"checkout {branchName}", workingDirectory);
        result.Success.Should().BeTrue($"Git checkout should succeed: {result.Error}");
    }

    /// <summary>
    /// Merge a branch into the current branch
    /// </summary>
    public async Task MergeBranchAsync(string workingDirectory, string branchName)
    {
        var result = await _gitExecutor.ExecuteAsync($"merge {branchName}", workingDirectory);
        result.Success.Should().BeTrue($"Git merge should succeed: {result.Error}");
    }

    /// <summary>
    /// Verify that the working tree is clean
    /// </summary>
    public async Task VerifyWorkingTreeCleanAsync(string workingDirectory)
    {
        var status = await _gitExecutor.GetStatusAsync(workingDirectory);
        status.Should().Contain("working tree clean", "Working tree should be clean");
    }

    /// <summary>
    /// Verify that the repository has the expected number of commits
    /// </summary>
    public async Task VerifyCommitCountAsync(string workingDirectory, int expectedCount)
    {
        var log = await _gitExecutor.GetLogAsync(workingDirectory, expectedCount + 10);
        var commitCount = string.IsNullOrWhiteSpace(log) ? 0 : log.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length;
        commitCount.Should().Be(expectedCount, $"Repository should have exactly {expectedCount} commits");
    }

    /// <summary>
    /// Verify that a commit count meets or exceeds the minimum
    /// </summary>
    public async Task VerifyMinimumCommitCountAsync(string workingDirectory, int minimumCount)
    {
        var log = await _gitExecutor.GetLogAsync(workingDirectory, minimumCount + 10);
        var commitCount = string.IsNullOrWhiteSpace(log) ? 0 : log.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length;
        commitCount.Should().BeGreaterThanOrEqualTo(minimumCount, $"Repository should have at least {minimumCount} commits");
    }

    /// <summary>
    /// Verify that a file exists in the working directory
    /// </summary>
    public void VerifyFileExists(string workingDirectory, string filename)
    {
        var filePath = Path.Combine(workingDirectory, filename);
        File.Exists(filePath).Should().BeTrue($"File {filename} should exist");
    }

    /// <summary>
    /// Verify that a file does not exist in the working directory
    /// </summary>
    public void VerifyFileDoesNotExist(string workingDirectory, string filename)
    {
        var filePath = Path.Combine(workingDirectory, filename);
        File.Exists(filePath).Should().BeFalse($"File {filename} should not exist");
    }

    /// <summary>
    /// Verify that a file exists with specific content
    /// </summary>
    public async Task VerifyFileContentAsync(string workingDirectory, string filename, string expectedContent)
    {
        var filePath = Path.Combine(workingDirectory, filename);
        File.Exists(filePath).Should().BeTrue($"File {filename} should exist");

        var actualContent = await File.ReadAllTextAsync(filePath);
        actualContent.Should().Contain(expectedContent, $"File {filename} should contain the expected content");
    }

    /// <summary>
    /// Verify that a file contains specific text
    /// </summary>
    public async Task VerifyFileContainsAsync(string workingDirectory, string filename, string expectedText)
    {
        var filePath = Path.Combine(workingDirectory, filename);
        File.Exists(filePath).Should().BeTrue($"File {filename} should exist");

        var content = await File.ReadAllTextAsync(filePath);
        content.Should().Contain(expectedText, $"File {filename} should contain '{expectedText}'");
    }

    /// <summary>
    /// Verify that the repository is initialized
    /// </summary>
    public async Task VerifyRepositoryInitializedAsync(string workingDirectory)
    {
        var isRepo = await _gitExecutor.IsGitRepositoryAsync(workingDirectory);
        isRepo.Should().BeTrue("Directory should be a git repository");
    }

    /// <summary>
    /// Verify that a specific branch exists
    /// </summary>
    public async Task VerifyBranchExistsAsync(string workingDirectory, string branchName)
    {
        var branches = await _gitExecutor.GetBranchesAsync(workingDirectory);
        branches.Should().Contain(branchName, $"Branch {branchName} should exist");
    }

    /// <summary>
    /// Verify that the current branch is the expected branch
    /// </summary>
    public async Task VerifyCurrentBranchAsync(string workingDirectory, string expectedBranch)
    {
        var currentBranch = await _gitExecutor.GetCurrentBranchAsync(workingDirectory);
        currentBranch.Should().Be(expectedBranch, $"Current branch should be {expectedBranch}");
    }

    /// <summary>
    /// Verify that a tag exists
    /// </summary>
    public async Task VerifyTagExistsAsync(string workingDirectory, string tagName)
    {
        var tags = await _gitExecutor.GetTagsAsync(workingDirectory);
        tags.Should().Contain(tagName, $"Tag {tagName} should exist");
    }

    /// <summary>
    /// Verify that a remote exists
    /// </summary>
    public async Task VerifyRemoteExistsAsync(string workingDirectory, string remoteName)
    {
        var remotes = await _gitExecutor.GetRemotesAsync(workingDirectory);
        remotes.Should().Contain(remoteName, $"Remote {remoteName} should exist");
    }

    /// <summary>
    /// Verify that there are conflicts in the working directory
    /// </summary>
    public async Task VerifyHasConflictsAsync(string workingDirectory)
    {
        var hasConflicts = await _gitExecutor.HasConflictsAsync(workingDirectory);
        hasConflicts.Should().BeTrue("Working directory should have merge conflicts");
    }

    /// <summary>
    /// Verify that there are no conflicts in the working directory
    /// </summary>
    public async Task VerifyNoConflictsAsync(string workingDirectory)
    {
        var hasConflicts = await _gitExecutor.HasConflictsAsync(workingDirectory);
        hasConflicts.Should().BeFalse("Working directory should not have merge conflicts");
    }

    /// <summary>
    /// Verify that the stash list is not empty
    /// </summary>
    public async Task VerifyStashExistsAsync(string workingDirectory)
    {
        var stashList = await _gitExecutor.GetStashListAsync(workingDirectory);
        stashList.Should().NotBeEmpty("Stash list should not be empty");
    }

    /// <summary>
    /// Verify that the log contains specific text
    /// </summary>
    public async Task VerifyLogContainsAsync(string workingDirectory, string expectedText)
    {
        var log = await _gitExecutor.GetLogAsync(workingDirectory, 50);
        log.Should().Contain(expectedText, $"Git log should contain '{expectedText}'");
    }

    /// <summary>
    /// Stage all files in the working directory
    /// </summary>
    public async Task StageAllFilesAsync(string workingDirectory)
    {
        var result = await _gitExecutor.ExecuteAsync("add .", workingDirectory);
        result.Success.Should().BeTrue($"Git add . should succeed: {result.Error}");
    }

    /// <summary>
    /// Create a commit with a message (requires files to be staged)
    /// </summary>
    public async Task CommitAsync(string workingDirectory, string message)
    {
        var result = await _gitExecutor.ExecuteAsync($"commit -m \"{message}\"", workingDirectory);
        result.Success.Should().BeTrue($"Git commit should succeed: {result.Error}");
    }

    /// <summary>
    /// Create a tag at the current commit
    /// </summary>
    public async Task CreateTagAsync(string workingDirectory, string tagName, string? commitHash = null)
    {
        var command = commitHash != null ? $"tag {tagName} {commitHash}" : $"tag {tagName}";
        var result = await _gitExecutor.ExecuteAsync(command, workingDirectory);
        result.Success.Should().BeTrue($"Git tag should succeed: {result.Error}");
    }

    /// <summary>
    /// Add a remote repository
    /// </summary>
    public async Task AddRemoteAsync(string workingDirectory, string remoteName, string remoteUrl)
    {
        var result = await _gitExecutor.ExecuteAsync($"remote add {remoteName} {remoteUrl}", workingDirectory);
        result.Success.Should().BeTrue($"Git remote add should succeed: {result.Error}");
    }

    /// <summary>
    /// Stash changes (including untracked files)
    /// </summary>
    public async Task StashAsync(string workingDirectory, bool includeUntracked = true)
    {
        var command = includeUntracked ? "stash -u" : "stash";
        var result = await _gitExecutor.ExecuteAsync(command, workingDirectory);
        result.Success.Should().BeTrue($"Git stash should succeed: {result.Error}");
    }

    /// <summary>
    /// Reset to a specific commit
    /// </summary>
    public async Task ResetAsync(string workingDirectory, string commitHash, bool hard = false)
    {
        var command = hard ? $"reset --hard {commitHash}" : $"reset {commitHash}";
        var result = await _gitExecutor.ExecuteAsync(command, workingDirectory);
        result.Success.Should().BeTrue($"Git reset should succeed: {result.Error}");
    }
}
