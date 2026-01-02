```
     ██████╗ ██╗████████╗ ██████╗ ██╗   ██╗████████╗
    ██╔════╝ ██║╚══██╔══╝██╔═══██╗██║   ██║╚══██╔══╝
    ██║  ███╗██║   ██║   ██║   ██║██║   ██║   ██║   
    ██║   ██║██║   ██║   ██║   ██║██║   ██║   ██║   
    ╚██████╔╝██║   ██║   ╚██████╔╝╚██████╔╝   ██║   
     ╚═════╝ ╚═╝   ╚═╝    ╚═════╝  ╚═════╝    ╚═╝   
```

# GitOut

A dungeon crawler game that teaches git through real command-line experience.

## Quick Install

### From Release (Recommended)

```bash
# macOS (Apple Silicon)
curl -L https://github.com/steveyackey/gitout/releases/latest/download/gitout-macos-arm64 -o gitout
chmod +x gitout && ./gitout

# Linux x64
curl -L https://github.com/steveyackey/gitout/releases/latest/download/gitout-linux-x64 -o gitout
chmod +x gitout && ./gitout

# Linux ARM64
curl -L https://github.com/steveyackey/gitout/releases/latest/download/gitout-linux-arm64 -o gitout
chmod +x gitout && ./gitout

# Windows (PowerShell)
Invoke-WebRequest -Uri https://github.com/steveyackey/gitout/releases/latest/download/gitout-win-x64.exe -OutFile gitout.exe
.\gitout.exe
```

### Docker

```bash
docker run -it ghcr.io/steveyackey/gitout
```

### From Source

```bash
git clone https://github.com/steveyackey/gitout.git
cd gitout
dotnet run --project src/GitOut.Console
```

## How to Play

Navigate dungeon rooms and solve challenges using **real git commands**:

| Command | Description |
|---------|-------------|
| `help` | Show all available commands |
| `look` | Examine current room and challenge |
| `forward`, `back`, `left`, `right` | Move between rooms |
| `git <command>` | Execute any git command |
| `hint` | Get help on current challenge |
| `save` | Save your progress |
| `quit` | Exit the game |

### Example Session

```
> look
You enter the Initialization Chamber...
Challenge: Initialize a git repository

> git init
Initialized empty Git repository in /tmp/GitOut/...

Challenge completed! You may now move forward.

> forward
You enter the Staging Area...
```

## What You'll Learn

23 progressive rooms covering:

**Basics**
- `git init` - Initialize repositories
- `git add` / `git commit` - Stage and commit changes
- `git log` / `git status` - View history and status

**Branching**
- `git branch` - Create and manage branches
- `git merge` - Merge branches
- `git rebase` - Rebase commits
- `git cherry-pick` - Apply specific commits

**Recovery**
- `git restore` - Restore working tree files
- `git stash` - Stash changes temporarily
- `git reflog` - Recover lost commits

**Advanced**
- Merge conflict resolution
- `git bisect` - Binary search for bugs
- `git worktree` - Multiple working trees
- `git blame` - Line-by-line history
- Git hooks - Automate workflows
- Interactive staging (`git add -p`)
- `git submodule` - Nested repositories
- `git filter-repo` - History rewriting

**Epic Final Boss** - A comprehensive challenge combining everything!

## Requirements

- **Git** installed and available in PATH
- One of:
  - Downloaded release binary (no dependencies)
  - Docker
  - [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (for building from source)

## Save System

Your progress is automatically saved to `~/.gitout/save.json`. Use `save` command anytime, and you'll be prompted to continue when you restart.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for development setup and guidelines.

## License

TBD

## Acknowledgments

Built with [Spectre.Console](https://spectreconsole.net/) for beautiful terminal UI.
