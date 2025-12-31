using GitOut.Application.Interfaces;
using GitOut.Domain.Interfaces;
using GitOut.Infrastructure.FileSystem;
using GitOut.Infrastructure.Git;
using GitOut.Infrastructure.Persistence;
using GitOut.Infrastructure.Tests.Helpers;

namespace GitOut.Infrastructure.Tests.Fixtures;

/// <summary>
/// Base class for room integration tests that provides common setup and helper utilities
/// </summary>
public abstract class RoomIntegrationTestFixture : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// Manages temporary directories for tests
    /// </summary>
    protected TempDirectoryManager TempDirManager { get; }

    /// <summary>
    /// Executes git commands
    /// </summary>
    protected Domain.Interfaces.IGitCommandExecutor GitExecutor { get; }

    /// <summary>
    /// Provides access to room data
    /// </summary>
    protected IRoomRepository RoomRepository { get; }

    /// <summary>
    /// Helper for common git operations
    /// </summary>
    protected GitTestHelper GitHelper { get; }

    /// <summary>
    /// Helper for common room operations
    /// </summary>
    protected RoomTestHelper RoomHelper { get; }

    /// <summary>
    /// Working directory for the current test (must be set by each test)
    /// </summary>
    protected string? WorkingDirectory { get; set; }

    protected RoomIntegrationTestFixture()
    {
        TempDirManager = new TempDirectoryManager();
        GitExecutor = new GitCommandExecutor();
        RoomRepository = new RoomRepository(GitExecutor);
        GitHelper = new GitTestHelper(GitExecutor);
        RoomHelper = new RoomTestHelper(RoomRepository);
    }

    /// <summary>
    /// Create a temporary working directory for a test
    /// </summary>
    protected string CreateWorkingDirectory(string testName)
    {
        WorkingDirectory = TempDirManager.CreateTempDirectory(testName);
        return WorkingDirectory;
    }

    /// <summary>
    /// Dispose of all resources
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose pattern implementation
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            TempDirManager.Dispose();
        }

        _disposed = true;
    }
}
