namespace GitOut.Infrastructure.FileSystem;

/// <summary>
/// Manages temporary directories for challenges
/// </summary>
public class TempDirectoryManager : IDisposable
{
    private readonly List<string> _createdDirectories = new();
    private bool _disposed;

    /// <summary>
    /// Create a new temporary directory
    /// </summary>
    public string CreateTempDirectory(string? prefix = null)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), "GitOut", prefix ?? "challenge", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempPath);
        _createdDirectories.Add(tempPath);
        return tempPath;
    }

    /// <summary>
    /// Create a file in the specified directory
    /// </summary>
    public async Task CreateFileAsync(string directory, string fileName, string content)
    {
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directory}");
        }

        var filePath = Path.Combine(directory, fileName);
        await File.WriteAllTextAsync(filePath, content);
    }

    /// <summary>
    /// Check if a file exists in the directory
    /// </summary>
    public bool FileExists(string directory, string fileName)
    {
        var filePath = Path.Combine(directory, fileName);
        return File.Exists(filePath);
    }

    /// <summary>
    /// Clean up a specific directory
    /// </summary>
    public void CleanupDirectory(string directory)
    {
        if (Directory.Exists(directory))
        {
            try
            {
                Directory.Delete(directory, recursive: true);
                _createdDirectories.Remove(directory);
            }
            catch (Exception ex)
            {
                // Log but don't throw - cleanup is best-effort
                Console.Error.WriteLine($"Warning: Failed to cleanup directory {directory}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Clean up all managed directories
    /// </summary>
    public void CleanupAll()
    {
        foreach (var directory in _createdDirectories.ToList())
        {
            CleanupDirectory(directory);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        CleanupAll();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~TempDirectoryManager()
    {
        Dispose();
    }
}
