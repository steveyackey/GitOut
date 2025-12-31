using FluentAssertions;
using GitOut.Domain.Challenges;
using GitOut.Domain.Interfaces;
using Moq;

namespace GitOut.Domain.Tests.Challenges;

public class ScenarioChallengeTests
{
    private readonly Mock<IGitCommandExecutor> _mockGitExecutor;

    public ScenarioChallengeTests()
    {
        _mockGitExecutor = new Mock<IGitCommandExecutor>();
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange & Act
        var challenge = new ScenarioChallenge(
            id: "scenario-1",
            description: "Test scenario",
            scenario: "A long time ago in a repository far, far away...",
            gitExecutor: _mockGitExecutor.Object
        );

        // Assert
        challenge.Id.Should().Be("scenario-1");
        challenge.Type.Should().Be(ChallengeType.Scenario);
        challenge.Description.Should().Be("Test scenario");
        challenge.Scenario.Should().Contain("long time ago");
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsArgumentException()
    {
        // Act
        var act = () => new ScenarioChallenge(
            id: "",
            description: "Test",
            scenario: "Story",
            gitExecutor: _mockGitExecutor.Object
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Challenge ID cannot be empty*");
    }

    [Fact]
    public void Constructor_WithEmptyScenario_ThrowsArgumentException()
    {
        // Act
        var act = () => new ScenarioChallenge(
            id: "scenario-1",
            description: "Test",
            scenario: "",
            gitExecutor: _mockGitExecutor.Object
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Scenario cannot be empty*");
    }

    [Fact]
    public void Constructor_WithNullGitExecutor_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ScenarioChallenge(
            id: "scenario-1",
            description: "Test",
            scenario: "Story",
            gitExecutor: null!
        );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task SetupAsync_CreatesSetupFiles()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var challenge = new ScenarioChallenge(
                id: "scenario-1",
                description: "Test",
                scenario: "Story",
                gitExecutor: _mockGitExecutor.Object,
                setupFiles: new List<string> { "test.txt", "config.json" }
            );

            // Act
            await challenge.SetupAsync(tempDir);

            // Assert
            File.Exists(Path.Combine(tempDir, "test.txt")).Should().BeTrue();
            File.Exists(Path.Combine(tempDir, "config.json")).Should().BeTrue();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task SetupAsync_WithNonExistentDirectory_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        var challenge = new ScenarioChallenge(
            id: "scenario-1",
            description: "Test",
            scenario: "Story",
            gitExecutor: _mockGitExecutor.Object
        );

        // Act
        var act = async () => await challenge.SetupAsync("/nonexistent/path");

        // Assert
        await act.Should().ThrowAsync<DirectoryNotFoundException>();
    }

    [Fact]
    public async Task ValidateAsync_WithRequireGitInit_ChecksRepository()
    {
        // Arrange
        _mockGitExecutor.Setup(x => x.IsGitRepositoryAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var challenge = new ScenarioChallenge(
            id: "scenario-1",
            description: "Test",
            scenario: "Story",
            gitExecutor: _mockGitExecutor.Object,
            requireGitInit: true
        );

        var tempDir = Path.GetTempPath();

        // Act
        var result = await challenge.ValidateAsync(tempDir);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Message.Should().Contain("not a git repository");
        _mockGitExecutor.Verify(x => x.IsGitRepositoryAsync(tempDir), Times.Once);
    }

    [Fact]
    public async Task ValidateAsync_WithRequiredBranches_ChecksBranchesExist()
    {
        // Arrange
        _mockGitExecutor.Setup(x => x.ExecuteAsync("branch", It.IsAny<string>()))
            .ReturnsAsync(new GitCommandResult(
                true,
                "* main\n  feature",
                "",
                0
            ));

        var challenge = new ScenarioChallenge(
            id: "scenario-1",
            description: "Test",
            scenario: "Story",
            gitExecutor: _mockGitExecutor.Object,
            requiredBranches: new List<string> { "main", "feature" }
        );

        var tempDir = Path.GetTempPath();

        // Act
        var result = await challenge.ValidateAsync(tempDir);

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WithMissingRequiredBranch_ReturnsFalse()
    {
        // Arrange
        _mockGitExecutor.Setup(x => x.ExecuteAsync("branch", It.IsAny<string>()))
            .ReturnsAsync(new GitCommandResult(
                true,
                "* main",
                "",
                0
            ));

        var challenge = new ScenarioChallenge(
            id: "scenario-1",
            description: "Test",
            scenario: "Story",
            gitExecutor: _mockGitExecutor.Object,
            requiredBranches: new List<string> { "feature-branch" }
        );

        var tempDir = Path.GetTempPath();

        // Act
        var result = await challenge.ValidateAsync(tempDir);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Message.Should().Contain("feature-branch");
        result.Hint.Should().Contain("git branch");
    }

    [Fact]
    public async Task ValidateAsync_WithRequiredCurrentBranch_ChecksCurrentBranch()
    {
        // Arrange
        _mockGitExecutor.Setup(x => x.ExecuteAsync("branch", It.IsAny<string>()))
            .ReturnsAsync(new GitCommandResult(
                true,
                "* main\n  feature",
                "",
                0
            ));

        var challenge = new ScenarioChallenge(
            id: "scenario-1",
            description: "Test",
            scenario: "Story",
            gitExecutor: _mockGitExecutor.Object,
            requiredCurrentBranch: "main"
        );

        var tempDir = Path.GetTempPath();

        // Act
        var result = await challenge.ValidateAsync(tempDir);

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WithWrongCurrentBranch_ReturnsFalse()
    {
        // Arrange
        _mockGitExecutor.Setup(x => x.ExecuteAsync("branch", It.IsAny<string>()))
            .ReturnsAsync(new GitCommandResult(
                true,
                "  main\n* feature",
                "",
                0
            ));

        var challenge = new ScenarioChallenge(
            id: "scenario-1",
            description: "Test",
            scenario: "Story",
            gitExecutor: _mockGitExecutor.Object,
            requiredCurrentBranch: "main"
        );

        var tempDir = Path.GetTempPath();

        // Act
        var result = await challenge.ValidateAsync(tempDir);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Message.Should().Contain("must be on the 'main' branch");
        result.Hint.Should().Contain("git checkout");
    }

    [Fact]
    public async Task ValidateAsync_WithRequiredCommitCount_ChecksCommits()
    {
        // Arrange
        _mockGitExecutor.Setup(x => x.GetLogAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync("commit abc123\ncommit def456\ncommit ghi789");

        var challenge = new ScenarioChallenge(
            id: "scenario-1",
            description: "Test",
            scenario: "Story",
            gitExecutor: _mockGitExecutor.Object,
            requiredCommitCount: 3
        );

        var tempDir = Path.GetTempPath();

        // Act
        var result = await challenge.ValidateAsync(tempDir);

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WithInsufficientCommits_ReturnsFalse()
    {
        // Arrange
        _mockGitExecutor.Setup(x => x.GetLogAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync("commit abc123");

        var challenge = new ScenarioChallenge(
            id: "scenario-1",
            description: "Test",
            scenario: "Story",
            gitExecutor: _mockGitExecutor.Object,
            requiredCommitCount: 3
        );

        var tempDir = Path.GetTempPath();

        // Act
        var result = await challenge.ValidateAsync(tempDir);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Message.Should().Contain("Expected at least 3 commit(s)");
    }

    [Fact]
    public async Task ValidateAsync_WithCustomValidator_UsesCustomLogic()
    {
        // Arrange
        var customValidatorCalled = false;
        Func<string, IGitCommandExecutor, Task<ChallengeResult>> customValidator =
            (dir, executor) =>
            {
                customValidatorCalled = true;
                return Task.FromResult(new ChallengeResult(
                    true,
                    "Custom validation passed",
                    null
                ));
            };

        var challenge = new ScenarioChallenge(
            id: "scenario-1",
            description: "Test",
            scenario: "Story",
            gitExecutor: _mockGitExecutor.Object,
            customValidator: customValidator
        );

        var tempDir = Path.GetTempPath();

        // Act
        var result = await challenge.ValidateAsync(tempDir);

        // Assert
        customValidatorCalled.Should().BeTrue();
        result.IsSuccessful.Should().BeTrue();
        result.Message.Should().Be("Custom validation passed");
    }
}
