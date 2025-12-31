using FluentAssertions;
using GitOut.Domain.Challenges;

namespace GitOut.Domain.Tests.Challenges;

public class QuizChallengeTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange & Act
        var challenge = new QuizChallenge(
            id: "quiz-1",
            description: "Test your git knowledge",
            question: "What is git?",
            options: new List<string> { "A version control system", "A cat", "A tree" },
            correctAnswerIndex: 0
        );

        // Assert
        challenge.Id.Should().Be("quiz-1");
        challenge.Type.Should().Be(ChallengeType.Quiz);
        challenge.Description.Should().Be("Test your git knowledge");
        challenge.Question.Should().Be("What is git?");
        challenge.Options.Should().HaveCount(3);
        challenge.CorrectAnswerIndex.Should().Be(0);
        challenge.HasAnswer.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsArgumentException()
    {
        // Act
        var act = () => new QuizChallenge(
            id: "",
            description: "Test",
            question: "Question?",
            options: new List<string> { "A", "B" },
            correctAnswerIndex: 0
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Challenge ID cannot be empty*");
    }

    [Fact]
    public void Constructor_WithEmptyDescription_ThrowsArgumentException()
    {
        // Act
        var act = () => new QuizChallenge(
            id: "quiz-1",
            description: "",
            question: "Question?",
            options: new List<string> { "A", "B" },
            correctAnswerIndex: 0
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Description cannot be empty*");
    }

    [Fact]
    public void Constructor_WithEmptyQuestion_ThrowsArgumentException()
    {
        // Act
        var act = () => new QuizChallenge(
            id: "quiz-1",
            description: "Test",
            question: "",
            options: new List<string> { "A", "B" },
            correctAnswerIndex: 0
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Question cannot be empty*");
    }

    [Fact]
    public void Constructor_WithLessThanTwoOptions_ThrowsArgumentException()
    {
        // Act
        var act = () => new QuizChallenge(
            id: "quiz-1",
            description: "Test",
            question: "Question?",
            options: new List<string> { "Only one" },
            correctAnswerIndex: 0
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Must provide at least 2 options*");
    }

    [Fact]
    public void Constructor_WithInvalidCorrectAnswerIndex_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => new QuizChallenge(
            id: "quiz-1",
            description: "Test",
            question: "Question?",
            options: new List<string> { "A", "B", "C" },
            correctAnswerIndex: 5
        );

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task SetupAsync_DoesNotRequireSetup()
    {
        // Arrange
        var challenge = CreateValidQuizChallenge();

        // Act
        var act = async () => await challenge.SetupAsync("/tmp/test");

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ValidateAsync_WithoutAnswer_ReturnsFalse()
    {
        // Arrange
        var challenge = CreateValidQuizChallenge();

        // Act
        var result = await challenge.ValidateAsync("/tmp/test");

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Message.Should().Contain("haven't answered");
        result.Hint.Should().Contain("answer <number>");
    }

    [Fact]
    public async Task ValidateAsync_WithCorrectAnswer_ReturnsTrue()
    {
        // Arrange
        var challenge = new QuizChallenge(
            id: "quiz-1",
            description: "Test",
            question: "What command stages all files?",
            options: new List<string> { "git add .", "git commit -a", "git push" },
            correctAnswerIndex: 0
        );

        // Act
        challenge.SubmitAnswer(0);
        var result = await challenge.ValidateAsync("/tmp/test");

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Message.Should().Contain("Correct");
        result.Message.Should().Contain("git add .");
    }

    [Fact]
    public async Task ValidateAsync_WithIncorrectAnswer_ReturnsFalse()
    {
        // Arrange
        var challenge = new QuizChallenge(
            id: "quiz-1",
            description: "Test",
            question: "What command stages all files?",
            options: new List<string> { "git add .", "git commit -a", "git push" },
            correctAnswerIndex: 0,
            hint: "Think about the staging area"
        );

        // Act
        challenge.SubmitAnswer(1);
        var result = await challenge.ValidateAsync("/tmp/test");

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Message.Should().Contain("Incorrect");
        result.Message.Should().Contain("git commit -a");
        result.Hint.Should().Be("Think about the staging area");
    }

    [Fact]
    public void SubmitAnswer_WithValidIndex_SetsAnswer()
    {
        // Arrange
        var challenge = CreateValidQuizChallenge();

        // Act
        challenge.SubmitAnswer(1);

        // Assert
        challenge.HasAnswer.Should().BeTrue();
        challenge.PlayerAnswer.Should().Be(1);
    }

    [Fact]
    public void SubmitAnswer_WithNegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var challenge = CreateValidQuizChallenge();

        // Act
        var act = () => challenge.SubmitAnswer(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SubmitAnswer_WithIndexTooLarge_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var challenge = CreateValidQuizChallenge();

        // Act
        var act = () => challenge.SubmitAnswer(10);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SubmitAnswer_CanBeChangedBeforeValidation()
    {
        // Arrange
        var challenge = CreateValidQuizChallenge();

        // Act
        challenge.SubmitAnswer(0);
        challenge.SubmitAnswer(2);

        // Assert
        challenge.PlayerAnswer.Should().Be(2);
    }

    private static QuizChallenge CreateValidQuizChallenge()
    {
        return new QuizChallenge(
            id: "quiz-test",
            description: "Test quiz",
            question: "Test question?",
            options: new List<string> { "Option A", "Option B", "Option C" },
            correctAnswerIndex: 1
        );
    }
}
