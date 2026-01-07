using FluentAssertions;
using GitOut.Infrastructure.Tests.Fixtures;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration.RoomTests;

/// <summary>
/// Integration tests for Room 19: The Hook Hollow (git hooks)
/// </summary>
public class Room19HookHollowTests : RoomIntegrationTestFixture
{
    [Fact]
    public async Task Room19_HookHollow_ShouldCompleteWhenHookCreatedAndExecutable()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room19-test");
        var room = await RoomHelper.LoadRoomAsync("room-19");

        room.Name.Should().Be("The Hook Hollow");

        // Act - Setup creates hook template
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Verify template exists
        var templatePath = Path.Combine(workingDirectory, "pre-commit-template");
        File.Exists(templatePath).Should().BeTrue("Hook template should exist");

        // Verify .git/hooks directory exists
        var hooksDir = Path.Combine(workingDirectory, ".git", "hooks");
        Directory.Exists(hooksDir).Should().BeTrue(".git/hooks directory should exist");

        // Copy template to hooks directory
        var hookPath = Path.Combine(hooksDir, "pre-commit");
        File.Copy(templatePath, hookPath, overwrite: true);

        // Make it executable on Unix/Mac
        if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            File.SetUnixFileMode(hookPath, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifySuccess(validationResult, "hook");
    }

    [Fact]
    public async Task Room19_HookHollow_ShouldFailWhenHookNotInstalled()
    {
        // Arrange
        var workingDirectory = CreateWorkingDirectory("room19-noinstall-test");
        var room = await RoomHelper.LoadRoomAsync("room-19");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Don't install the hook - just leave template in working directory

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifyFailure(validationResult, "not been installed");
    }

    [Fact]
    public async Task Room19_HookHollow_ShouldFailWhenHookNotExecutableOnUnix()
    {
        // Skip this test on Windows
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            return;
        }

        // Arrange
        var workingDirectory = CreateWorkingDirectory("room19-notexecutable-test");
        var room = await RoomHelper.LoadRoomAsync("room-19");

        // Act - Setup
        await RoomHelper.SetupChallengeAsync(room, workingDirectory);

        // Copy hook but don't make it executable
        var templatePath = Path.Combine(workingDirectory, "pre-commit-template");
        var hookPath = Path.Combine(workingDirectory, ".git", "hooks", "pre-commit");
        File.Copy(templatePath, hookPath, overwrite: true);

        // Explicitly remove execute permissions
        File.SetUnixFileMode(hookPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);

        // Act - Validate
        var validationResult = await RoomHelper.ValidateChallengeAsync(room, workingDirectory);

        // Assert
        RoomHelper.VerifyFailure(validationResult, "not executable");
    }
}
