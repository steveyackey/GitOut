using System.Diagnostics;
using System.Text;
using GitOut.Domain.Interfaces;

namespace GitOut.Infrastructure.Git;

/// <summary>
/// Executes git commands using Process.Start()
/// </summary>
public class GitCommandExecutor : IGitCommandExecutor
{
    public async Task<GitCommandResult> ExecuteAsync(string command, string workingDirectory)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            throw new ArgumentException("Command cannot be null or empty", nameof(command));
        }

        if (string.IsNullOrWhiteSpace(workingDirectory))
        {
            throw new ArgumentException("Working directory cannot be null or empty", nameof(workingDirectory));
        }

        if (!Directory.Exists(workingDirectory))
        {
            throw new DirectoryNotFoundException($"Working directory not found: {workingDirectory}");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = command,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Prevent git from trying to open an editor (important for rebase, merge, etc. on CI)
        startInfo.Environment["GIT_EDITOR"] = "true";
        startInfo.Environment["GIT_SEQUENCE_EDITOR"] = "true";

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        using var process = new Process { StartInfo = startInfo };

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                outputBuilder.AppendLine(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                errorBuilder.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        var exitCode = process.ExitCode;
        var output = outputBuilder.ToString().TrimEnd();
        var error = errorBuilder.ToString().TrimEnd();

        return new GitCommandResult(
            Success: exitCode == 0,
            Output: output,
            Error: error,
            ExitCode: exitCode
        );
    }

    public async Task<bool> IsGitRepositoryAsync(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return false;
        }

        var gitDir = Path.Combine(directory, ".git");
        if (Directory.Exists(gitDir))
        {
            return true;
        }

        // Also check using git command
        try
        {
            var result = await ExecuteAsync("rev-parse --is-inside-work-tree", directory);
            return result.Success && result.Output.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetStatusAsync(string workingDirectory)
    {
        var result = await ExecuteAsync("status", workingDirectory);

        if (!result.Success)
        {
            throw new InvalidOperationException($"Failed to get git status: {result.Error}");
        }

        return result.Output;
    }

    public async Task<string> GetLogAsync(string workingDirectory, int maxCount = 10)
    {
        var result = await ExecuteAsync($"log --oneline -n {maxCount}", workingDirectory);

        if (!result.Success)
        {
            // If there are no commits yet, return empty string instead of throwing
            if (result.Error.Contains("does not have any commits yet") ||
                result.Error.Contains("your current branch") ||
                result.Error.Contains("fatal: bad default revision"))
            {
                return string.Empty;
            }

            throw new InvalidOperationException($"Failed to get git log: {result.Error}");
        }

        return result.Output;
    }

    public async Task<string> GetReflogAsync(string workingDirectory, int maxCount = 10)
    {
        var result = await ExecuteAsync($"reflog -n {maxCount}", workingDirectory);

        if (!result.Success)
        {
            // If reflog doesn't exist yet, return empty string
            if (result.Error.Contains("does not have any commits yet") ||
                result.Error.Contains("fatal: bad default revision"))
            {
                return string.Empty;
            }

            throw new InvalidOperationException($"Failed to get git reflog: {result.Error}");
        }

        return result.Output;
    }

    public async Task<string> GetTagsAsync(string workingDirectory)
    {
        var result = await ExecuteAsync("tag -l", workingDirectory);

        if (!result.Success)
        {
            throw new InvalidOperationException($"Failed to get git tags: {result.Error}");
        }

        return result.Output;
    }

    public async Task<string> GetStashListAsync(string workingDirectory)
    {
        var result = await ExecuteAsync("stash list", workingDirectory);

        if (!result.Success)
        {
            throw new InvalidOperationException($"Failed to get git stash list: {result.Error}");
        }

        return result.Output;
    }

    public async Task<string> GetRemotesAsync(string workingDirectory)
    {
        var result = await ExecuteAsync("remote -v", workingDirectory);

        if (!result.Success)
        {
            throw new InvalidOperationException($"Failed to get git remotes: {result.Error}");
        }

        return result.Output;
    }

    public async Task<string> GetBranchesAsync(string workingDirectory)
    {
        var result = await ExecuteAsync("branch", workingDirectory);

        if (!result.Success)
        {
            throw new InvalidOperationException($"Failed to get git branches: {result.Error}");
        }

        return result.Output;
    }

    public async Task<string> GetCurrentBranchAsync(string workingDirectory)
    {
        var result = await ExecuteAsync("branch --show-current", workingDirectory);

        if (!result.Success)
        {
            throw new InvalidOperationException($"Failed to get current branch: {result.Error}");
        }

        return result.Output.Trim();
    }

    public async Task<bool> HasConflictsAsync(string workingDirectory)
    {
        try
        {
            var status = await GetStatusAsync(workingDirectory);
            // Check for conflict markers in git status output
            return status.Contains("both modified") ||
                   status.Contains("both added") ||
                   status.Contains("Unmerged paths") ||
                   status.Contains("fix conflicts");
        }
        catch
        {
            return false;
        }
    }
}
