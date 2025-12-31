using GitOut.Domain.Interfaces;

namespace GitOut.Domain.Challenges;

/// <summary>
/// A story-driven challenge that combines narrative with git repository tasks
/// </summary>
public class ScenarioChallenge : IChallenge
{
    private readonly IGitCommandExecutor _gitExecutor;
    private readonly Func<string, IGitCommandExecutor, Task<ChallengeResult>>? _customValidator;
    private readonly Func<string, IGitCommandExecutor, Task>? _customSetup;

    public string Id { get; init; }
    public ChallengeType Type => ChallengeType.Scenario;
    public string Description { get; init; }
    public string Scenario { get; init; }
    public List<string> SetupFiles { get; init; } = new();

    // Similar validation criteria to RepositoryChallenge but with more story context
    public bool RequireGitInit { get; init; }
    public bool RequireCleanStatus { get; init; }
    public int? RequiredCommitCount { get; init; }
    public List<string> RequiredFiles { get; init; } = new();
    public List<string> RequiredBranches { get; init; } = new();
    public string? RequiredCurrentBranch { get; init; }

    public ScenarioChallenge(
        string id,
        string description,
        string scenario,
        IGitCommandExecutor gitExecutor,
        bool requireGitInit = false,
        bool requireCleanStatus = false,
        int? requiredCommitCount = null,
        List<string>? requiredFiles = null,
        List<string>? requiredBranches = null,
        string? requiredCurrentBranch = null,
        List<string>? setupFiles = null,
        Func<string, IGitCommandExecutor, Task<ChallengeResult>>? customValidator = null,
        Func<string, IGitCommandExecutor, Task>? customSetup = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Challenge ID cannot be empty", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description cannot be empty", nameof(description));
        }

        if (string.IsNullOrWhiteSpace(scenario))
        {
            throw new ArgumentException("Scenario cannot be empty", nameof(scenario));
        }

        Id = id;
        Description = description;
        Scenario = scenario;
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
        RequireGitInit = requireGitInit;
        RequireCleanStatus = requireCleanStatus;
        RequiredCommitCount = requiredCommitCount;
        RequiredFiles = requiredFiles ?? new List<string>();
        RequiredBranches = requiredBranches ?? new List<string>();
        RequiredCurrentBranch = requiredCurrentBranch;
        SetupFiles = setupFiles ?? new List<string>();
        _customValidator = customValidator;
        _customSetup = customSetup;
    }

    public async Task SetupAsync(string workingDirectory)
    {
        if (!Directory.Exists(workingDirectory))
        {
            throw new DirectoryNotFoundException($"Working directory not found: {workingDirectory}");
        }

        // Create any required setup files
        foreach (var fileName in SetupFiles)
        {
            var filePath = Path.Combine(workingDirectory, fileName);
            if (!File.Exists(filePath))
            {
                await File.WriteAllTextAsync(filePath,
                    $"# {fileName}\n\nThis file is part of the scenario.\n");
            }
        }

        // Run custom setup if provided
        if (_customSetup != null)
        {
            await _customSetup(workingDirectory, _gitExecutor);
        }
    }

    public async Task<ChallengeResult> ValidateAsync(string workingDirectory)
    {
        if (!Directory.Exists(workingDirectory))
        {
            return new ChallengeResult(
                false,
                "Working directory does not exist.",
                "Make sure you're in the correct directory."
            );
        }

        // Use custom validator if provided
        if (_customValidator != null)
        {
            return await _customValidator(workingDirectory, _gitExecutor);
        }

        // Check if git init is required
        if (RequireGitInit)
        {
            var isRepo = await _gitExecutor.IsGitRepositoryAsync(workingDirectory);
            if (!isRepo)
            {
                return new ChallengeResult(
                    false,
                    "This directory is not a git repository.",
                    "Try running 'git init' to initialize a repository."
                );
            }
        }

        // Check required files exist
        foreach (var fileName in RequiredFiles)
        {
            var filePath = Path.Combine(workingDirectory, fileName);
            if (!File.Exists(filePath))
            {
                return new ChallengeResult(
                    false,
                    $"Required file '{fileName}' not found.",
                    $"The scenario requires the file '{fileName}' to exist."
                );
            }
        }

        // Check required branches
        if (RequiredBranches.Count > 0)
        {
            var branchOutput = await _gitExecutor.ExecuteAsync("branch", workingDirectory);

            foreach (var branchName in RequiredBranches)
            {
                if (!branchOutput.Output.Contains(branchName))
                {
                    return new ChallengeResult(
                        false,
                        $"Required branch '{branchName}' not found.",
                        $"Try creating the branch with 'git branch {branchName}'"
                    );
                }
            }
        }

        // Check current branch
        if (!string.IsNullOrWhiteSpace(RequiredCurrentBranch))
        {
            var branchOutput = await _gitExecutor.ExecuteAsync("branch", workingDirectory);
            if (!branchOutput.Output.Contains($"* {RequiredCurrentBranch}"))
            {
                return new ChallengeResult(
                    false,
                    $"You must be on the '{RequiredCurrentBranch}' branch.",
                    $"Switch to the branch with 'git checkout {RequiredCurrentBranch}'"
                );
            }
        }

        // Check commit count
        if (RequiredCommitCount.HasValue)
        {
            var log = await _gitExecutor.GetLogAsync(workingDirectory, RequiredCommitCount.Value + 10);
            var commitCount = string.IsNullOrWhiteSpace(log) ? 0 :
                log.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length;

            if (commitCount < RequiredCommitCount.Value)
            {
                return new ChallengeResult(
                    false,
                    $"Expected at least {RequiredCommitCount.Value} commit(s), but found {commitCount}.",
                    "Try creating a commit with 'git commit -m \"your message\"'"
                );
            }
        }

        // Check clean status
        if (RequireCleanStatus)
        {
            var status = await _gitExecutor.GetStatusAsync(workingDirectory);
            if (!status.Contains("nothing to commit, working tree clean") &&
                !status.Contains("nothing added to commit"))
            {
                return new ChallengeResult(
                    false,
                    "Working tree is not clean.",
                    "Make sure all changes are committed. Use 'git status' to check."
                );
            }
        }

        return new ChallengeResult(
            true,
            "Scenario completed successfully! The story continues...",
            null
        );
    }
}
