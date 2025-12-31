namespace GitOut.Domain.Challenges;

/// <summary>
/// A multiple choice quiz challenge about git commands and concepts
/// </summary>
public class QuizChallenge : IChallenge
{
    public string Id { get; init; }
    public ChallengeType Type => ChallengeType.Quiz;
    public string Description { get; init; }
    public string Question { get; init; }
    public List<string> Options { get; init; }
    public int CorrectAnswerIndex { get; init; }
    public string? Hint { get; init; }

    private int? _playerAnswer;

    public QuizChallenge(
        string id,
        string description,
        string question,
        List<string> options,
        int correctAnswerIndex,
        string? hint = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Challenge ID cannot be empty", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description cannot be empty", nameof(description));
        }

        if (string.IsNullOrWhiteSpace(question))
        {
            throw new ArgumentException("Question cannot be empty", nameof(question));
        }

        if (options == null || options.Count < 2)
        {
            throw new ArgumentException("Must provide at least 2 options", nameof(options));
        }

        if (correctAnswerIndex < 0 || correctAnswerIndex >= options.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(correctAnswerIndex),
                $"Correct answer index must be between 0 and {options.Count - 1}"
            );
        }

        Id = id;
        Description = description;
        Question = question;
        Options = options;
        CorrectAnswerIndex = correctAnswerIndex;
        Hint = hint;
    }

    public Task SetupAsync(string workingDirectory)
    {
        // Quiz challenges don't need any git repository setup
        return Task.CompletedTask;
    }

    public Task<ChallengeResult> ValidateAsync(string workingDirectory)
    {
        if (!_playerAnswer.HasValue)
        {
            return Task.FromResult(new ChallengeResult(
                false,
                "You haven't answered the question yet.",
                "Use 'answer <number>' to select an option (e.g., 'answer 1' for the first option)"
            ));
        }

        if (_playerAnswer.Value == CorrectAnswerIndex)
        {
            return Task.FromResult(new ChallengeResult(
                true,
                $"Correct! {Options[CorrectAnswerIndex]} is the right answer.",
                null
            ));
        }

        return Task.FromResult(new ChallengeResult(
            false,
            $"Incorrect. '{Options[_playerAnswer.Value]}' is not the right answer.",
            Hint
        ));
    }

    /// <summary>
    /// Submit an answer to the quiz question
    /// </summary>
    /// <param name="answerIndex">Zero-based index of the selected answer</param>
    public void SubmitAnswer(int answerIndex)
    {
        if (answerIndex < 0 || answerIndex >= Options.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(answerIndex),
                $"Answer index must be between 0 and {Options.Count - 1}"
            );
        }

        _playerAnswer = answerIndex;
    }

    /// <summary>
    /// Check if the player has submitted an answer
    /// </summary>
    public bool HasAnswer => _playerAnswer.HasValue;

    /// <summary>
    /// Get the player's submitted answer index
    /// </summary>
    public int? PlayerAnswer => _playerAnswer;
}
