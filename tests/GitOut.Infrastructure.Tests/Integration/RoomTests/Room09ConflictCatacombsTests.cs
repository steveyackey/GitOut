using FluentAssertions;
using GitOut.Infrastructure.Tests.Fixtures;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration.RoomTests;

/// <summary>
/// Integration tests for Room 9: The Conflict Catacombs (merge conflicts)
/// </summary>
public class Room09ConflictCatacombsTests : RoomIntegrationTestFixture
{
    [Fact]
    public async Task Room9_ConflictCatacombs_ShouldCompleteWhenConflictIsResolved()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room9-test");
        var room = await RoomHelper.LoadRoomAsync("room-9");

        room.Name.Should().Be("The Conflict Catacombs");

        // Act - Setup creates a merge conflict
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify conflict exists
        await GitHelper.VerifyHasConflictsAsync(workingDirectory);

        // Read the conflicted file
        var spellBookPath = Path.Combine(workingDirectory, "spell-book.txt");
        var conflictedContent = await File.ReadAllTextAsync(spellBookPath);
        conflictedContent.Should().Contain("<<<<<<<", "Conflict markers should be present");

        // Resolve conflict by choosing one version
        await File.WriteAllTextAsync(spellBookPath,
            "Chapter 1: Advanced Fire Magic\nChapter 2: Water Magic\nChapter 3: Earth Magic");

        // Stage the resolved file
        await GitExecutor.ExecuteAsync("add spell-book.txt", workingDirectory);

        // Complete the merge
        await GitExecutor.ExecuteAsync("commit -m \"Resolve merge conflict\"", workingDirectory);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "harmonized");
    }

    [Fact]
    public async Task Room9_ConflictCatacombs_ShouldCompleteUsingGitCheckoutOurs()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room9-ours-test");
        var room = await RoomHelper.LoadRoomAsync("room-9");

        room.Name.Should().Be("The Conflict Catacombs");

        // Act - Setup creates a merge conflict
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify conflict exists
        await GitHelper.VerifyHasConflictsAsync(workingDirectory);

        // Resolve using git checkout --ours (keep main's version)
        var checkoutResult = await GitExecutor.ExecuteAsync("checkout --ours spell-book.txt", workingDirectory);
        checkoutResult.Success.Should().BeTrue($"checkout --ours should succeed: {checkoutResult.Error}");

        // Stage and commit (use --no-edit to accept git's auto-generated merge message)
        var addResult = await GitExecutor.ExecuteAsync("add spell-book.txt", workingDirectory);
        addResult.Success.Should().BeTrue($"add should succeed: {addResult.Error}");

        var commitResult = await GitExecutor.ExecuteAsync("commit --no-edit", workingDirectory);
        commitResult.Success.Should().BeTrue($"commit should succeed: {commitResult.Error}");

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        if (!validationResult.IsSuccessful)
        {
            // Debug output
            var status = await GitExecutor.GetStatusAsync(workingDirectory);
            var log = await GitExecutor.GetLogAsync(workingDirectory, 5);
            throw new Exception($"Validation failed: {validationResult.Message}\nHint: {validationResult.Hint}\nStatus: {status}\nLog: {log}");
        }
        RoomHelper.VerifySuccess(validationResult, "harmonized");
    }

    [Fact]
    public async Task Room9_ConflictCatacombs_ShouldCompleteUsingGitCheckoutTheirs()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room9-theirs-test");
        var room = await RoomHelper.LoadRoomAsync("room-9");

        // Act - Setup creates a merge conflict
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Resolve using git checkout --theirs (keep branch's version)
        await GitExecutor.ExecuteAsync("checkout --theirs spell-book.txt", workingDirectory);

        // Stage and commit (use --no-edit to accept git's auto-generated merge message)
        await GitExecutor.ExecuteAsync("add spell-book.txt", workingDirectory);
        await GitExecutor.ExecuteAsync("commit --no-edit", workingDirectory);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "harmonized");
    }

    [Fact]
    public async Task Room9_ConflictCatacombs_ShouldFailWhenConflictNotResolved()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room9-fail-test");
        var room = await RoomHelper.LoadRoomAsync("room-9");

        // Act - Setup only
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Act - Validate without resolving
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifyFailure(validationResult, "clash");
    }
}
