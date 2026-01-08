using FluentAssertions;
using GitOut.Infrastructure.Tests.Fixtures;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration.RoomTests;

/// <summary>
/// Integration tests for Room 21: The Submodule Sanctum (git submodule)
/// </summary>
public class Room21SubmoduleSanctumTests : RoomIntegrationTestFixture
{
    [Fact]
    public async Task Room21_SubmoduleSanctum_ShouldCompleteWhenSubmoduleAdded()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room21-test");
        var room = await RoomHelper.LoadRoomAsync("room-21");

        room.Name.Should().Be("The Submodule Sanctum");

        // Act - Setup creates main repo + library repo in sibling directory
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify instructions file was created
        var instructionsPath = Path.Combine(workingDirectory, "SUBMODULE_INSTRUCTIONS.txt");
        File.Exists(instructionsPath).Should().BeTrue("SUBMODULE_INSTRUCTIONS.txt should exist");

        // Read the library path from the instructions
        var instructions = await File.ReadAllTextAsync(instructionsPath);
        var libraryPathLine = instructions.Split('\n').FirstOrDefault(line => line.Contains("magic-library"));
        libraryPathLine.Should().NotBeNullOrEmpty("Instructions should contain library path");

        var libraryPath = libraryPathLine!.Trim();

        // Verify library repo exists
        Directory.Exists(libraryPath).Should().BeTrue("Library repository should exist");

        // Player adds the submodule
        // Use -c flag to allow file:// protocol for this command only (security restriction in modern git)
        var submoduleAddResult = await GitExecutor.ExecuteAsync($"-c protocol.file.allow=always submodule add {libraryPath} lib", workingDirectory);
        submoduleAddResult.Success.Should().BeTrue($"Submodule add should succeed: {submoduleAddResult.Error}");

        // Verify .gitmodules was created
        var gitmodulesPath = Path.Combine(workingDirectory, ".gitmodules");
        File.Exists(gitmodulesPath).Should().BeTrue(".gitmodules should exist after adding submodule");

        // Commit the submodule addition
        await GitExecutor.ExecuteAsync("add .gitmodules lib", workingDirectory);
        var commitResult = await GitExecutor.ExecuteAsync("commit -m \"Add magic library as submodule\"", workingDirectory);
        commitResult.Success.Should().BeTrue($"Commit should succeed: {commitResult.Error}");

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        if (!validationResult.IsSuccessful)
        {
            throw new Exception($"Validation failed: {validationResult.Message}\nHint: {validationResult.Hint}");
        }
        RoomHelper.VerifySuccess(validationResult, "submodule");
    }

    [Fact]
    public async Task Room21_SubmoduleSanctum_ShouldFailWhenNoGitmodules()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room21-nogitmodules-test");
        var room = await RoomHelper.LoadRoomAsync("room-21");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Don't add submodule, just try to validate

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifyFailure(validationResult, ".gitmodules");
    }

    [Fact]
    public async Task Room21_SubmoduleSanctum_ShouldFailWhenSubmoduleNotCommitted()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room21-notcommitted-test");
        var room = await RoomHelper.LoadRoomAsync("room-21");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Read library path
        var instructions = await File.ReadAllTextAsync(Path.Combine(workingDirectory, "SUBMODULE_INSTRUCTIONS.txt"));
        var libraryPathLine = instructions.Split('\n').FirstOrDefault(line => line.Contains("magic-library"));
        var libraryPath = libraryPathLine!.Trim();

        // Add submodule but don't commit
        // Use -c flag to allow file:// protocol for this command only
        await GitExecutor.ExecuteAsync($"-c protocol.file.allow=always submodule add {libraryPath} lib", workingDirectory);

        // Act - Validate (should fail because not committed)
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifyFailure(validationResult, "not committed");
    }

    [Fact]
    public async Task Room21_SubmoduleSanctum_ShouldVerifySubmoduleContainsLibraryFiles()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room21-verify-test");
        var room = await RoomHelper.LoadRoomAsync("room-21");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Read library path
        var instructions = await File.ReadAllTextAsync(Path.Combine(workingDirectory, "SUBMODULE_INSTRUCTIONS.txt"));
        var libraryPathLine = instructions.Split('\n').FirstOrDefault(line => line.Contains("magic-library"));
        var libraryPath = libraryPathLine!.Trim();

        // Add and commit submodule
        // Use -c flag to allow file:// protocol for this command only
        await GitExecutor.ExecuteAsync($"-c protocol.file.allow=always submodule add {libraryPath} lib", workingDirectory);
        await GitExecutor.ExecuteAsync("add .gitmodules lib", workingDirectory);
        await GitExecutor.ExecuteAsync("commit -m \"Add magic library as submodule\"", workingDirectory);

        // Verify submodule files exist
        var magicSpellPath = Path.Combine(workingDirectory, "lib", "magic-spell.js");
        File.Exists(magicSpellPath).Should().BeTrue("Submodule should contain magic-spell.js");

        var readmePath = Path.Combine(workingDirectory, "lib", "README.md");
        File.Exists(readmePath).Should().BeTrue("Submodule should contain README.md");

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "submodule");
    }
}
