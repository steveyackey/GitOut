using FluentAssertions;
using GitOut.Infrastructure.FileSystem;
using Xunit;

namespace GitOut.Infrastructure.Tests.FileSystem;

public class TempDirectoryManagerTests
{
    [Fact]
    public void CreateTempDirectory_ShouldCreateDirectory()
    {
        // Arrange
        using var manager = new TempDirectoryManager();

        // Act
        var directory = manager.CreateTempDirectory();

        // Assert
        Directory.Exists(directory).Should().BeTrue();
        directory.Should().Contain("GitOut");
    }

    [Fact]
    public void CreateTempDirectory_ShouldCreateDirectoryWithPrefix()
    {
        // Arrange
        using var manager = new TempDirectoryManager();

        // Act
        var directory = manager.CreateTempDirectory("test-prefix");

        // Assert
        Directory.Exists(directory).Should().BeTrue();
        directory.Should().Contain("test-prefix");
    }

    [Fact]
    public void CreateTempDirectory_ShouldCreateUniqueDirectories()
    {
        // Arrange
        using var manager = new TempDirectoryManager();

        // Act
        var dir1 = manager.CreateTempDirectory();
        var dir2 = manager.CreateTempDirectory();

        // Assert
        dir1.Should().NotBe(dir2);
        Directory.Exists(dir1).Should().BeTrue();
        Directory.Exists(dir2).Should().BeTrue();
    }

    [Fact]
    public async Task CreateFileAsync_ShouldCreateFile_InDirectory()
    {
        // Arrange
        using var manager = new TempDirectoryManager();
        var directory = manager.CreateTempDirectory();

        // Act
        await manager.CreateFileAsync(directory, "test.txt", "test content");

        // Assert
        var filePath = Path.Combine(directory, "test.txt");
        File.Exists(filePath).Should().BeTrue();
        var content = await File.ReadAllTextAsync(filePath);
        content.Should().Be("test content");
    }

    [Fact]
    public async Task CreateFileAsync_ShouldThrowException_WhenDirectoryDoesNotExist()
    {
        // Arrange
        using var manager = new TempDirectoryManager();

        // Act
        var act = async () => await manager.CreateFileAsync("/nonexistent/directory", "test.txt", "content");

        // Assert
        await act.Should().ThrowAsync<DirectoryNotFoundException>();
    }

    [Fact]
    public void FileExists_ShouldReturnTrue_WhenFileExists()
    {
        // Arrange
        using var manager = new TempDirectoryManager();
        var directory = manager.CreateTempDirectory();
        var filePath = Path.Combine(directory, "test.txt");
        File.WriteAllText(filePath, "test");

        // Act
        var exists = manager.FileExists(directory, "test.txt");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public void FileExists_ShouldReturnFalse_WhenFileDoesNotExist()
    {
        // Arrange
        using var manager = new TempDirectoryManager();
        var directory = manager.CreateTempDirectory();

        // Act
        var exists = manager.FileExists(directory, "nonexistent.txt");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public void CleanupDirectory_ShouldRemoveDirectory()
    {
        // Arrange
        using var manager = new TempDirectoryManager();
        var directory = manager.CreateTempDirectory();

        // Act
        manager.CleanupDirectory(directory);

        // Assert
        Directory.Exists(directory).Should().BeFalse();
    }

    [Fact]
    public void CleanupAll_ShouldRemoveAllManagedDirectories()
    {
        // Arrange
        using var manager = new TempDirectoryManager();
        var dir1 = manager.CreateTempDirectory();
        var dir2 = manager.CreateTempDirectory();
        var dir3 = manager.CreateTempDirectory();

        // Act
        manager.CleanupAll();

        // Assert
        Directory.Exists(dir1).Should().BeFalse();
        Directory.Exists(dir2).Should().BeFalse();
        Directory.Exists(dir3).Should().BeFalse();
    }

    [Fact]
    public void Dispose_ShouldCleanupAllDirectories()
    {
        // Arrange
        var manager = new TempDirectoryManager();
        var dir1 = manager.CreateTempDirectory();
        var dir2 = manager.CreateTempDirectory();

        // Act
        manager.Dispose();

        // Assert
        Directory.Exists(dir1).Should().BeFalse();
        Directory.Exists(dir2).Should().BeFalse();
    }

    [Fact]
    public void Dispose_ShouldBeIdempotent()
    {
        // Arrange
        var manager = new TempDirectoryManager();
        var directory = manager.CreateTempDirectory();

        // Act
        manager.Dispose();
        var act = () => manager.Dispose();

        // Assert
        act.Should().NotThrow();
        Directory.Exists(directory).Should().BeFalse();
    }
}
