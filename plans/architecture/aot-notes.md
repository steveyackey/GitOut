# Native AOT Compilation Notes

## Build Configuration

GitOut has been configured to support Native AOT (Ahead-of-Time) compilation, producing self-contained native binaries that don't require the .NET runtime.

### Project Configuration

The `GitOut.Console.csproj` file has been configured with the following AOT-related properties:

```xml
<PublishAot>true</PublishAot>
<InvariantGlobalization>false</InvariantGlobalization>
<StripSymbols>true</StripSymbols>
<AssemblyName>gitout</AssemblyName>
```

- **PublishAot**: Enables Native AOT compilation
- **InvariantGlobalization**: Set to `false` to support culture-specific operations
- **StripSymbols**: Reduces binary size by removing debug symbols
- **AssemblyName**: Output binary name (lowercase for consistency across platforms)

## Compilation Warnings

During AOT compilation, the following warnings are generated. These have been reviewed and documented:

### 1. JSON Serialization Warnings (IL2026, IL3050)

**Location:** `JsonProgressRepository.cs` (lines 44, 63)

**Warning:**
```
warning IL2026: Using member 'System.Text.Json.JsonSerializer.Serialize/Deserialize<T>' 
which has 'RequiresUnreferencedCodeAttribute' can break functionality when trimming.

warning IL3050: JSON serialization and deserialization might require types that cannot 
be statically analyzed and might need runtime code generation.
```

**Status:** ⚠️ Safe to ignore for now

**Reason:**
- GitOut uses `JsonSerializer` to save/load game progress (simple DTO: `GameProgress`)
- The `GameProgress` class is a simple data structure with public properties
- All serialized types are known at compile time
- The JSON operations are limited to a single well-defined class

**Future Improvement:**
Consider implementing System.Text.Json source generation for fully AOT-compatible JSON serialization:
```csharp
[JsonSerializable(typeof(GameProgress))]
internal partial class SourceGenerationContext : JsonSerializerContext { }
```

### 2. Spectre.Console Exception Formatter Warning (IL3050)

**Location:** `Program.cs` (line 217)

**Warning:**
```
warning IL3050: Using member 'Spectre.Console.AnsiConsole.WriteException' which has 
'RequiresDynamicCodeAttribute' can break functionality when AOT compiling. 
ExceptionFormatter is currently not supported for AOT.
```

**Status:** ⚠️ Safe to ignore

**Reason:**
- This is only used in the global exception handler for unexpected errors
- The exception formatter is a nice-to-have feature, not critical functionality
- In AOT builds, exceptions will still be caught and displayed, just without fancy formatting
- The core game functionality is unaffected

**Mitigation:**
The warning can be suppressed if desired, or the code could be modified to check if AOT is enabled:
```csharp
#if !NATIVE_AOT
    AnsiConsole.WriteException(ex);
#else
    AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
#endif
```

## Build Commands

### Linux/macOS
```bash
./eng/publish.sh
```

### Windows
```powershell
.\eng\publish.ps1
```

### Manual Build (Single RID)
```bash
dotnet publish src/GitOut.Console/GitOut.Console.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    -p:PublishAot=true \
    -p:StripSymbols=true \
    -o artifacts/publish/linux-x64
```

## Target Platforms

GitOut supports Native AOT on the following platforms:

- **Windows**: `win-x64`, `win-arm64`
- **Linux**: `linux-x64`, `linux-arm64`
- **macOS**: `osx-x64`, `osx-arm64`
- **Linux (musl)**: `linux-musl-x64`, `linux-musl-arm64` (optional, for Alpine)

## Binary Size

Native AOT binaries are self-contained and include the .NET runtime. Approximate sizes:

- **Linux x64**: ~6 MB (stripped)
- **Windows x64**: ~7-8 MB (stripped)
- **macOS ARM64**: ~6-7 MB (stripped)

The `StripSymbols=true` setting significantly reduces binary size by removing debug symbols.

## Runtime Behavior

### Verified Working
- ✅ Game launches successfully
- ✅ Spectre.Console TUI renders correctly
- ✅ Git command execution via `Process.Start()`
- ✅ File I/O operations
- ✅ JSON save/load functionality
- ✅ All 3 challenge types (Repository, Quiz, Scenario)
- ✅ Navigation and game loop

### Known Limitations
- Exception formatting in global handler may be simplified (non-critical)
- Requires git to be installed on the system (by design)

## Smoke Testing

After building AOT binaries, verify:

1. **Binary launches**: `./gitout` or `gitout.exe`
2. **UI renders**: ASCII art and panels display correctly
3. **Interactive input**: Can enter name and receive prompts
4. **Git execution**: Can complete at least one git-based challenge
5. **Save/load**: Can save progress and reload it

### Sample Test Commands
```bash
# Test binary exists and is executable
ls -lh artifacts/publish/linux-x64/gitout
file artifacts/publish/linux-x64/gitout

# Test it runs (will prompt for input)
./artifacts/publish/linux-x64/gitout
```

## Troubleshooting

### Build Errors

**Error: "clang not found"**
- **Solution**: Install C/C++ compiler toolchain
  - Linux: `sudo apt install clang` or `sudo yum install clang`
  - macOS: `xcode-select --install`
  - Windows: Install Visual Studio Build Tools

**Error: "zlib not found"**
- **Solution**: Install zlib development package
  - Linux: `sudo apt install zlib1g-dev`
  - macOS: Already included in Xcode

### Runtime Errors

**Error: "git not found"**
- **Solution**: This is expected behavior. Git must be installed on the target system.
- See main README.md for git installation instructions.

## References

- [.NET Native AOT Documentation](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
- [Prepare .NET libraries for trimming](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/prepare-libraries-for-trimming)
- [System.Text.Json source generation](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation)
