using FluentAssertions;
using GitOut.Domain.Challenges;
using GitOut.Domain.Interfaces;
using Moq;
using Xunit;

namespace GitOut.Domain.Tests.Challenges;

public class RepositoryChallengeTests
{
    private readonly Mock<IGitCommandExecutor> _gitExecutorMock;

    public RepositoryChallengeTests()
    {
        _gitExecutorMock = new Mock<IGitCommandExecutor>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenIdIsEmpty()
    {
        // Act
        var act = () => new RepositoryChallenge(
            id: "",
            description: "Test",
            gitExecutor: _gitExecutorMock.Object
        );

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenDescriptionIsEmpty()
    {
        // Act
        var act = () => new RepositoryChallenge(
            id: "test-id",
            description: "",
            gitExecutor: _gitExecutorMock.Object
        );

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenGitExecutorIsNull()
    {
        // Act
        var act = () => new RepositoryChallenge(
            id: "test-id",
            description: "Test",
            gitExecutor: null!
        );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task SetupAsync_ShouldCreateSetupFiles()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var challenge = new RepositoryChallenge(
                id: "test-id",
                description: "Test",
                gitExecutor: _gitExecutorMock.Object,
                setupFiles: new List<string> { "README.md", "test.txt" }
            );

            // Act
            await challenge.SetupAsync(tempDir);

            // Assert
            File.Exists(Path.Combine(tempDir, "README.md")).Should().BeTrue();
            File.Exists(Path.Combine(tempDir, "test.txt")).Should().BeTrue();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task SetupAsync_ShouldThrowException_WhenDirectoryDoesNotExist()
    {
        // Arrange
        var challenge = new RepositoryChallenge(
            id: "test-id",
            description: "Test",
            gitExecutor: _gitExecutorMock.Object
        );

        // Act
        var act = async () => await challenge.SetupAsync("/nonexistent/directory");

        // Assert
        await act.Should().ThrowAsync<DirectoryNotFoundException>();
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnFailure_WhenGitInitRequired_AndNotInitialized()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            _gitExecutorMock.Setup(x => x.IsGitRepositoryAsync(tempDir))
                .ReturnsAsync(false);

            var challenge = new RepositoryChallenge(
                id: "test-id",
                description: "Test",
                gitExecutor: _gitExecutorMock.Object,
                requireGitInit: true
            );

            // Act
            var result = await challenge.ValidateAsync(tempDir);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.Message.Should().Contain("not a git repository");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnSuccess_WhenGitInitRequired_AndInitialized()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            _gitExecutorMock.Setup(x => x.IsGitRepositoryAsync(tempDir))
                .ReturnsAsync(true);

            var challenge = new RepositoryChallenge(
                id: "test-id",
                description: "Test",
                gitExecutor: _gitExecutorMock.Object,
                requireGitInit: true
            );

            // Act
            var result = await challenge.ValidateAsync(tempDir);

            // Assert
            result.IsSuccessful.Should().BeTrue();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnFailure_WhenRequiredFileIsMissing()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var challenge = new RepositoryChallenge(
                id: "test-id",
                description: "Test",
                gitExecutor: _gitExecutorMock.Object,
                requiredFiles: new List<string> { "README.md" }
            );

            // Act
            var result = await challenge.ValidateAsync(tempDir);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.Message.Should().Contain("README.md");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnFailure_WhenCommitCountInsufficient()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            _gitExecutorMock.Setup(x => x.IsGitRepositoryAsync(tempDir))
                .ReturnsAsync(true);
            _gitExecutorMock.Setup(x => x.GetLogAsync(tempDir, It.IsAny<int>()))
                .ReturnsAsync(string.Empty); // No commits

            var challenge = new RepositoryChallenge(
                id: "test-id",
                description: "Test",
                gitExecutor: _gitExecutorMock.Object,
                requireGitInit: true,
                requiredCommitCount: 1
            );

            // Act
            var result = await challenge.ValidateAsync(tempDir);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.Message.Should().Contain("commit");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnSuccess_WhenCommitCountSufficient()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            _gitExecutorMock.Setup(x => x.IsGitRepositoryAsync(tempDir))
                .ReturnsAsync(true);
            _gitExecutorMock.Setup(x => x.GetLogAsync(tempDir, It.IsAny<int>()))
                .ReturnsAsync("abc123 Initial commit\ndef456 Second commit");

            var challenge = new RepositoryChallenge(
                id: "test-id",
                description: "Test",
                gitExecutor: _gitExecutorMock.Object,
                requireGitInit: true,
                requiredCommitCount: 2
            );

            // Act
            var result = await challenge.ValidateAsync(tempDir);

            // Assert
            result.IsSuccessful.Should().BeTrue();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnFailure_WhenWorkingTreeNotClean()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            _gitExecutorMock.Setup(x => x.IsGitRepositoryAsync(tempDir))
                .ReturnsAsync(true);
            _gitExecutorMock.Setup(x => x.GetStatusAsync(tempDir))
                .ReturnsAsync("Changes not staged for commit");

            var challenge = new RepositoryChallenge(
                id: "test-id",
                description: "Test",
                gitExecutor: _gitExecutorMock.Object,
                requireGitInit: true,
                requireCleanStatus: true
            );

            // Act
            var result = await challenge.ValidateAsync(tempDir);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.Message.Should().Contain("not clean");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnSuccess_WhenWorkingTreeClean()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            _gitExecutorMock.Setup(x => x.IsGitRepositoryAsync(tempDir))
                .ReturnsAsync(true);
            _gitExecutorMock.Setup(x => x.GetStatusAsync(tempDir))
                .ReturnsAsync("nothing to commit, working tree clean");

            var challenge = new RepositoryChallenge(
                id: "test-id",
                description: "Test",
                gitExecutor: _gitExecutorMock.Object,
                requireGitInit: true,
                requireCleanStatus: true
            );

            // Act
            var result = await challenge.ValidateAsync(tempDir);

            // Assert
            result.IsSuccessful.Should().BeTrue();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
