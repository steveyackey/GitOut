using System.Text.Json;
using GitOut.Application.Interfaces;

namespace GitOut.Infrastructure.Persistence;

/// <summary>
/// Saves and loads game progress as JSON files
/// </summary>
public class JsonProgressRepository : IProgressRepository
{
    private readonly string _saveFilePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonProgressRepository(string? saveDirectory = null)
    {
        // Default to ~/.gitout/save.json
        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var gitoutDirectory = saveDirectory ?? Path.Combine(homeDirectory, ".gitout");

        // Ensure directory exists
        if (!Directory.Exists(gitoutDirectory))
        {
            Directory.CreateDirectory(gitoutDirectory);
        }

        _saveFilePath = Path.Combine(gitoutDirectory, "save.json");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task SaveProgressAsync(GameProgress progress)
    {
        if (progress == null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        try
        {
            var json = JsonSerializer.Serialize(progress, _jsonOptions);
            await File.WriteAllTextAsync(_saveFilePath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save progress to {_saveFilePath}", ex);
        }
    }

    public async Task<GameProgress?> LoadProgressAsync()
    {
        if (!await HasSavedProgressAsync())
        {
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_saveFilePath);
            var progress = JsonSerializer.Deserialize<GameProgress>(json, _jsonOptions);
            return progress;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load progress from {_saveFilePath}", ex);
        }
    }

    public Task<bool> HasSavedProgressAsync()
    {
        return Task.FromResult(File.Exists(_saveFilePath));
    }

    public Task DeleteProgressAsync()
    {
        if (File.Exists(_saveFilePath))
        {
            File.Delete(_saveFilePath);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Get the path where save files are stored
    /// </summary>
    public string GetSaveFilePath() => _saveFilePath;
}
