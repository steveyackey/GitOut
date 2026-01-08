using FluentAssertions;
using GitOut.Domain.Challenges;
using GitOut.Infrastructure.Tests.Fixtures;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration.RoomTests;

/// <summary>
/// Integration tests for Room 8: The Quiz Master's Hall (quiz challenge)
/// </summary>
public class Room08QuizMastersHallTests : RoomIntegrationTestFixture
{
    [Fact]
    public async Task Room8_QuizMaster_ShouldBeQuizChallenge()
    {
        // Arrange
        var room = await RoomHelper.LoadRoomAsync("room-8");

        // Assert
        room.Name.Should().Be("The Quiz Master's Hall");
        RoomHelper.VerifyChallengeType<QuizChallenge>(room);

        var quizChallenge = RoomHelper.GetChallengeAs<QuizChallenge>(room);
        quizChallenge.Options.Should().HaveCountGreaterThan(1);
        quizChallenge.Question.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Room8_QuizMaster_ShouldCompleteWithCorrectAnswer()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room8-test");
        var room = await RoomHelper.LoadRoomAsync("room-8");
        var quizChallenge = RoomHelper.GetChallengeAs<QuizChallenge>(room);

        // Act - Submit correct answer (index 0 is "git add .")
        quizChallenge.SubmitAnswer(0);

        // Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "Correct");
    }

    [Fact]
    public async Task Room8_QuizMaster_ShouldFailWithWrongAnswer()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room8-fail-test");
        var room = await RoomHelper.LoadRoomAsync("room-8");
        var quizChallenge = RoomHelper.GetChallengeAs<QuizChallenge>(room);

        // Act - Submit wrong answer
        quizChallenge.SubmitAnswer(1);

        // Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifyFailure(validationResult, "Incorrect");
    }
}
