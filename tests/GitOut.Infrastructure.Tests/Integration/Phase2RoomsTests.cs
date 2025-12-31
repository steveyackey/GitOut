using FluentAssertions;
using GitOut.Domain.Challenges;
using GitOut.Infrastructure.Tests.Fixtures;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration;

/// <summary>
/// Integration tests for Phase 2 rooms (Room 3-8)
/// </summary>
public class Phase2RoomsTests : RoomIntegrationTestFixture
{

    [Fact]
    public async Task Room3_HistoryArchive_ShouldCompleteWhenLogIsViewed()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room3-test");
        var room = await RoomHelper.LoadRoomAsync("room-3");

        room.Name.Should().Be("The History Archive");
        room.Challenge.Should().NotBeNull();

        // Act - Setup creates commits
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify repo was initialized and has commits
        await GitHelper.VerifyRepositoryInitializedAsync(workingDirectory);
        await GitHelper.VerifyMinimumCommitCountAsync(workingDirectory, 1);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "chronicles");
    }

    [Fact]
    public async Task Room4_StatusChamber_ShouldCompleteWhenStatusIsChecked()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room4-test");
        var room = await RoomHelper.LoadRoomAsync("room-4");

        room.Name.Should().Be("The Status Chamber");

        // Act - Setup creates files in various states
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify setup created expected files
        GitHelper.VerifyFileExists(workingDirectory, "tracked.txt");
        GitHelper.VerifyFileExists(workingDirectory, "untracked.txt");
        GitHelper.VerifyFileExists(workingDirectory, "staged.txt");

        // Act - Validate (challenge auto-completes since repo has state)
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult);
    }

    [Fact]
    public async Task Room5_BranchJunction_ShouldCompleteWhenFeatureBranchIsCreated()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room5-test");
        var room = await RoomHelper.LoadRoomAsync("room-5");

        room.Name.Should().Be("The Branch Junction");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify repo was initialized
        await GitHelper.VerifyRepositoryInitializedAsync(workingDirectory);

        // Create the feature-branch and switch to it
        await GitHelper.CreateBranchAsync(workingDirectory, "feature-branch");

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "feature-branch");
    }

    [Fact]
    public async Task Room5_BranchJunction_ShouldFailWhenBranchNotCreated()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room5-fail-test");
        var room = await RoomHelper.LoadRoomWithChallengeAsync("room-5");

        // Act - Setup only, don't create branch
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifyFailure(validationResult, "not been created");
    }

    [Fact]
    public async Task Room5_BranchJunction_ShouldFailWhenBranchCreatedButNotSwitched()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room5-not-switched-test");
        var room = await RoomHelper.LoadRoomWithChallengeAsync("room-5");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Create the branch but DON'T switch to it
        await GitExecutor.ExecuteAsync("branch feature-branch", workingDirectory);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifyFailure(validationResult, "not currently on it");
        validationResult.Hint.Should().Contain("switch");
    }

    [Fact]
    public async Task Room6_MergeNexus_ShouldCompleteWhenFeatureIsMerged()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room6-test");
        var room = await RoomHelper.LoadRoomAsync("room-6");

        room.Name.Should().Be("The Merge Nexus");

        // Act - Setup creates my-feature branch with spell files already created
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify we're on main and my-feature branch exists
        await GitHelper.VerifyCurrentBranchAsync(workingDirectory, "main");
        await GitHelper.VerifyBranchExistsAsync(workingDirectory, "my-feature");

        // Switch to my-feature branch
        await GitHelper.CheckoutBranchAsync(workingDirectory, "my-feature");

        // Verify the three spell files already exist (created by setup)
        GitHelper.VerifyFileExists(workingDirectory, "fireball.txt");
        GitHelper.VerifyFileExists(workingDirectory, "icebolt.txt");
        GitHelper.VerifyFileExists(workingDirectory, "lightning.txt");

        // Stage and commit all spell files (files are already created, just need to be staged)
        await GitHelper.StageAllFilesAsync(workingDirectory);
        await GitHelper.CommitAsync(workingDirectory, "Add three combat spells");

        // Switch back to main
        await GitHelper.CheckoutBranchAsync(workingDirectory, "main");

        // Merge my-feature into main
        await GitHelper.MergeBranchAsync(workingDirectory, "my-feature");

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "merged");
    }

    [Fact]
    public async Task Room7_RestorationVault_ShouldCompleteWhenFileIsRestored()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room7-test");
        var room = await RoomHelper.LoadRoomAsync("room-7");

        room.Name.Should().Be("The Restoration Vault");

        // Configure git
        await GitHelper.ConfigureGitUserAsync(workingDirectory);

        // Act - Setup creates corrupted file
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify file is corrupted
        await GitHelper.VerifyFileContainsAsync(workingDirectory, "sacred-text.txt", "CORRUPTED");

        // Restore the file
        await GitExecutor.ExecuteAsync("restore sacred-text.txt", workingDirectory);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "restored");

        // Verify file content is restored
        await GitHelper.VerifyFileContainsAsync(workingDirectory, "sacred-text.txt", "Sacred Text");
    }

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

    [Fact]
    public async Task AllRooms_ShouldHaveChallenges()
    {
        // Arrange & Act & Assert
        await RoomHelper.VerifyAllRoomsHaveChallengesAsync();
    }

    [Fact]
    public async Task AllRooms_ExceptEndRoom_ShouldHaveExits()
    {
        // Arrange & Act & Assert
        await RoomHelper.VerifyAllNonEndRoomsHaveExitsAsync();
    }
}
