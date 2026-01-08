using FluentAssertions;
using GitOut.Console.UI;
using Spectre.Console.Testing;
using Xunit;

namespace GitOut.Infrastructure.Tests.Integration.E2E;

/// <summary>
/// E2E tests for SplashScreen using Spectre.Console.Testing
/// </summary>
public class SplashScreenTests
{
    [Fact]
    public void Show_WithoutAnimation_ShouldDisplayLogoAndFeatures()
    {
        // Arrange
        var console = new TestConsole();

        // Act - Show without animation (faster for tests)
        SplashScreen.Show(console, animate: false);

        // Assert
        var output = console.Output;
        output.Should().Contain("GITOUT", "Should contain the logo text");
        output.Should().Contain("Dungeon Crawler", "Should contain the subtitle");
        output.Should().Contain("git commands", "Should mention git commands");
    }

    [Fact]
    public void Show_ShouldDisplayFeatureList()
    {
        // Arrange
        var console = new TestConsole();

        // Act
        SplashScreen.Show(console, animate: false);

        // Assert
        var output = console.Output;
        output.Should().Contain("dungeon rooms", "Should mention rooms");
        output.Should().Contain("challenges", "Should mention challenges");
    }

    [Fact]
    public void Show_ShouldDisplayGitCommandsPreview()
    {
        // Arrange
        var console = new TestConsole();

        // Act
        SplashScreen.Show(console, animate: false);

        // Assert
        var output = console.Output;
        output.Should().Contain("init", "Should show git init");
        output.Should().Contain("commit", "Should show git commit");
        output.Should().Contain("branch", "Should show git branch");
        output.Should().Contain("merge", "Should show git merge");
    }

    [Fact]
    public void Show_ShouldDisplayHelpHint()
    {
        // Arrange
        var console = new TestConsole();

        // Act
        SplashScreen.Show(console, animate: false);

        // Assert
        var output = console.Output;
        output.Should().Contain("help", "Should mention help command");
    }
}
