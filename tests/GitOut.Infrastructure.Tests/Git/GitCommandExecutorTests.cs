using FluentAssertions;
using GitOut.Infrastructure.Git;
using Xunit;

namespace GitOut.Infrastructure.Tests.Git;

public class GitCommandExecutorTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly GitCommandExecutor _executor;

    public GitCommandExecutorTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "GitOutTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
        _executor = new GitCommandExecutor();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldExecuteGitInit_Successfully()
    {
        // Act
        var result = await _executor.ExecuteAsync("init", _tempDirectory);

        // Assert
        result.Success.Should().BeTrue();
        result.ExitCode.Should().Be(0);
        Directory.Exists(Path.Combine(_tempDirectory, ".git")).Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnError_WhenCommandFails()
    {
        // Act
        var result = await _executor.ExecuteAsync("invalid-command", _tempDirectory);

        // Assert
        result.Success.Should().BeFalse();
        result.ExitCode.Should().NotBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenWorkingDirectoryDoesNotExist()
    {
        // Act
        var act = async () => await _executor.ExecuteAsync("init", "/nonexistent/directory");

        // Assert
        await act.Should().ThrowAsync<DirectoryNotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_WhenCommandIsEmpty()
    {
        // Act
        var act = async () => await _executor.ExecuteAsync("", _tempDirectory);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task IsGitRepositoryAsync_ShouldReturnFalse_WhenNotInitialized()
    {
        // Act
        var isRepo = await _executor.IsGitRepositoryAsync(_tempDirectory);

        // Assert
        isRepo.Should().BeFalse();
    }

    [Fact]
    public async Task IsGitRepositoryAsync_ShouldReturnTrue_WhenInitialized()
    {
        // Arrange
        await _executor.ExecuteAsync("init", _tempDirectory);

        // Act
        var isRepo = await _executor.IsGitRepositoryAsync(_tempDirectory);

        // Assert
        isRepo.Should().BeTrue();
    }

    [Fact]
    public async Task IsGitRepositoryAsync_ShouldReturnFalse_WhenDirectoryDoesNotExist()
    {
        // Act
        var isRepo = await _executor.IsGitRepositoryAsync("/nonexistent/directory");

        // Assert
        isRepo.Should().BeFalse();
    }

    [Fact]
    public async Task GetStatusAsync_ShouldReturnStatus_WhenRepositoryInitialized()
    {
        // Arrange
        await _executor.ExecuteAsync("init", _tempDirectory);

        // Act
        var status = await _executor.GetStatusAsync(_tempDirectory);

        // Assert
        status.Should().NotBeEmpty();
        status.Should().Contain("On branch");
    }

    [Fact]
    public async Task GetStatusAsync_ShouldThrowException_WhenNotARepository()
    {
        // Act
        var act = async () => await _executor.GetStatusAsync(_tempDirectory);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetLogAsync_ShouldReturnEmptyString_WhenNoCommits()
    {
        // Arrange
        await _executor.ExecuteAsync("init", _tempDirectory);

        // Act
        var log = await _executor.GetLogAsync(_tempDirectory);

        // Assert
        log.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLogAsync_ShouldReturnCommits_WhenCommitsExist()
    {
        // Arrange
        await _executor.ExecuteAsync("init", _tempDirectory);
        await _executor.ExecuteAsync("config user.email \"test@example.com\"", _tempDirectory);
        await _executor.ExecuteAsync("config user.name \"Test User\"", _tempDirectory);

        // Create a file and commit
        var testFile = Path.Combine(_tempDirectory, "test.txt");
        await File.WriteAllTextAsync(testFile, "test content");
        await _executor.ExecuteAsync("add test.txt", _tempDirectory);
        await _executor.ExecuteAsync("commit -m \"Initial commit\"", _tempDirectory);

        // Act
        var log = await _executor.GetLogAsync(_tempDirectory);

        // Assert
        log.Should().NotBeEmpty();
        log.Should().Contain("Initial commit");
    }

    [Fact]
    public async Task GetLogAsync_ShouldLimitCommits_WhenMaxCountSpecified()
    {
        // Arrange
        await _executor.ExecuteAsync("init", _tempDirectory);
        await _executor.ExecuteAsync("config user.email \"test@example.com\"", _tempDirectory);
        await _executor.ExecuteAsync("config user.name \"Test User\"", _tempDirectory);

        // Create multiple commits
        for (int i = 1; i <= 5; i++)
        {
            var testFile = Path.Combine(_tempDirectory, $"test{i}.txt");
            await File.WriteAllTextAsync(testFile, $"test content {i}");
            await _executor.ExecuteAsync($"add test{i}.txt", _tempDirectory);
            await _executor.ExecuteAsync($"commit -m \"Commit {i}\"", _tempDirectory);
        }

        // Act
        var log = await _executor.GetLogAsync(_tempDirectory, maxCount: 3);

        // Assert
        var commitCount = log.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length;
        commitCount.Should().Be(3);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }
}
