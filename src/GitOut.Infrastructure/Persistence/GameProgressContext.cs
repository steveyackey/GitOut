using System.Text.Json.Serialization;
using GitOut.Application.Interfaces;

namespace GitOut.Infrastructure.Persistence;

/// <summary>
/// Source-generated JSON serialization context for AOT compatibility.
/// This eliminates runtime reflection for JSON serialization.
/// </summary>
[JsonSerializable(typeof(GameProgress))]
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class GameProgressContext : JsonSerializerContext
{
}
