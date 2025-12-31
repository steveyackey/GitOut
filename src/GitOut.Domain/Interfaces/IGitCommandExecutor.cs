namespace GitOut.Domain.Interfaces;

/// <summary>
/// Executes git commands and returns results
/// </summary>
public interface IGitCommandExecutor
{
    /// <summary>
    /// Execute a git command in the specified working directory
    /// </summary>
    Task<GitCommandResult> ExecuteAsync(string command, string workingDirectory);

    /// <summary>
    /// Check if a directory is a valid git repository
    /// </summary>
    Task<bool> IsGitRepositoryAsync(string directory);

    /// <summary>
    /// Get the current git status
    /// </summary>
    Task<string> GetStatusAsync(string workingDirectory);

    /// <summary>
    /// Get the git log
    /// </summary>
    Task<string> GetLogAsync(string workingDirectory, int maxCount = 10);

    /// <summary>
    /// Get the reflog (reference log)
    /// </summary>
    Task<string> GetReflogAsync(string workingDirectory, int maxCount = 10);

    /// <summary>
    /// Get list of tags
    /// </summary>
    Task<string> GetTagsAsync(string workingDirectory);

    /// <summary>
    /// Get list of stashes
    /// </summary>
    Task<string> GetStashListAsync(string workingDirectory);

    /// <summary>
    /// Get list of remotes
    /// </summary>
    Task<string> GetRemotesAsync(string workingDirectory);

    /// <summary>
    /// Get list of branches
    /// </summary>
    Task<string> GetBranchesAsync(string workingDirectory);

    /// <summary>
    /// Get the current branch name
    /// </summary>
    Task<string> GetCurrentBranchAsync(string workingDirectory);

    /// <summary>
    /// Check if there are conflicts in the working directory
    /// </summary>
    Task<bool> HasConflictsAsync(string workingDirectory);
}

public record GitCommandResult(
    bool Success,
    string Output,
    string Error,
    int ExitCode
);
