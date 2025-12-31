namespace GitOut.Domain.Challenges;

/// <summary>
/// Represents a challenge that the player must complete to progress
/// </summary>
public interface IChallenge
{
    /// <summary>
    /// Unique identifier for the challenge
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Type of challenge (Repository, Quiz, Scenario)
    /// </summary>
    ChallengeType Type { get; }

    /// <summary>
    /// Description/instructions for the player
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Setup the challenge environment (e.g., initialize git repo)
    /// </summary>
    Task SetupAsync(string workingDirectory);

    /// <summary>
    /// Validate if the challenge has been completed successfully
    /// </summary>
    Task<ChallengeResult> ValidateAsync(string workingDirectory);
}

public enum ChallengeType
{
    Repository,
    Quiz,
    Scenario
}

public record ChallengeResult(
    bool IsSuccessful,
    string Message,
    string? Hint = null
);
