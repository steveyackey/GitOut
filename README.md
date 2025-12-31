# GitOut

A terminal-based dungeon crawler that teaches git commands through gameplay.

## What is GitOut?

GitOut is an educational game where you navigate through dungeon rooms and solve challenges using **real git commands**. Each room presents a unique git scenario that you must solve using your command-line git skills. Learn git by playing!

## Supported Platforms

- **Windows** (x64, ARM64)
- **macOS** (x64, ARM64)  
- **Linux** (x64, ARM64)

## Prerequisites

**Git must be installed and available on your PATH.** GitOut uses real git commands, so you need git installed on your system.

- Windows: [Download Git for Windows](https://git-scm.com/download/win)
- macOS: Install via Homebrew `brew install git` or [download](https://git-scm.com/download/mac)
- Linux: Install via package manager (e.g., `apt install git`, `yum install git`)

Verify git is installed:
```bash
git --version
```

## Installation

### Option 1: Install Latest Release (Recommended)

Download and install the latest release for your operating system with a single command:

**Linux:**
```bash
mkdir -p ~/.local/bin && curl -fsSL https://api.github.com/repos/steveyackey/GitOut/releases/latest | python3 -c "import sys,json,platform; d=json.load(sys.stdin); s=platform.machine().lower(); rid='linux-arm64' if s in ('aarch64','arm64') else 'linux-x64'; print([a['browser_download_url'] for a in d['assets'] if rid in a['name'] and a['name'].endswith('.tar.gz')][0])" | xargs -I{} sh -c 'curl -fsSL {} | tar -xz -C /tmp && install -m 755 /tmp/gitout ~/.local/bin/gitout' && ~/.local/bin/gitout --version
```

Make sure `~/.local/bin` is in your PATH, or add it:
```bash
echo 'export PATH="$HOME/.local/bin:$PATH"' >> ~/.bashrc
source ~/.bashrc
```

**macOS:**
```bash
curl -fsSL https://api.github.com/repos/steveyackey/GitOut/releases/latest | python3 -c "import sys,json,platform; d=json.load(sys.stdin); s=platform.machine().lower(); rid='osx-arm64' if s in ('arm64','aarch64') else 'osx-x64'; print([a['browser_download_url'] for a in d['assets'] if rid in a['name'] and a['name'].endswith('.tar.gz')][0])" | xargs -I{} sh -c 'curl -fsSL {} | tar -xz -C /tmp && sudo install -m 755 /tmp/gitout /usr/local/bin/gitout' && gitout --version
```

**Windows (PowerShell):**
```powershell
powershell -NoProfile -Command "$ErrorActionPreference='Stop'; $arch=if($env:PROCESSOR_ARCHITECTURE -match 'ARM64'){ 'win-arm64' } else { 'win-x64' }; $r=Invoke-RestMethod https://api.github.com/repos/steveyackey/GitOut/releases/latest; $a=($r.assets | Where-Object { $_.name -match $arch -and $_.name -match '\.zip$' } | Select-Object -First 1); $dir=Join-Path $env:LOCALAPPDATA 'GitOut'; New-Item -Force -ItemType Directory $dir | Out-Null; $zip=Join-Path $env:TEMP $a.name; Invoke-WebRequest $a.browser_download_url -OutFile $zip; Expand-Archive -Force $zip $dir; $exe=Join-Path $dir 'gitout.exe'; if(-not (Test-Path $exe)){ $exe=(Get-ChildItem $dir -Recurse -Filter gitout.exe | Select-Object -First 1).FullName }; $p=[Environment]::GetEnvironmentVariable('Path','User'); if($p -notlike \"*$dir*\"){ [Environment]::SetEnvironmentVariable('Path', \"$p;$dir\", 'User') }; & $exe --version"
```

After installation, restart your terminal and run:
```bash
gitout
```

### Option 2: Run with Docker

If you have Docker installed, you can run GitOut without installing anything else:

**Basic run:**
```bash
docker run -it --rm ghcr.io/steveyackey/gitout:latest
```

**With volume mount (for git-based challenges):**
```bash
docker run -it --rm -v "$PWD:/work" -w /work ghcr.io/steveyackey/gitout:latest
```

**With persistent app data:**
```bash
docker volume create gitout-data
docker run -it --rm -v gitout-data:/data -e XDG_DATA_HOME=/data -e XDG_CONFIG_HOME=/data ghcr.io/steveyackey/gitout:latest
```

### Option 3: Build from Source

If you want to build from source or contribute to development, see [CONTRIBUTING.md](CONTRIBUTING.md).

## How to Play

1. **Start the game**: Run `gitout` (or `docker run -it --rm ghcr.io/steveyackey/gitout:latest`)
2. **Navigate**: Use commands like `move north`, `move south`, etc.
3. **Examine**: Use `look` to see room details
4. **Solve challenges**: Use real git commands in the provided directory
5. **Progress**: Complete challenges to unlock new rooms

### Available Commands

- `help` - Show available commands
- `look` - Examine current room
- `status` - View challenge status and progress
- `move <direction>` - Move to adjacent room (north, south, east, west)
- `hint` - Get a hint for the current challenge (if available)
- `save` - Save your progress
- `quit` - Exit the game

### Challenge Types

- **Repository Challenges** - Manipulate a git repository to reach a target state
- **Quiz Challenges** - Answer questions about git commands and concepts
- **Scenario Challenges** - Story-driven problems requiring git solutions

## Troubleshooting

### "git not found" error

GitOut requires git to be installed and available on your PATH. 

**Verify git is installed:**
```bash
git --version
```

If git is not found:
- **Windows**: Install [Git for Windows](https://git-scm.com/download/win) and restart your terminal
- **macOS**: Install with `brew install git` or download from [git-scm.com](https://git-scm.com/download/mac)
- **Linux**: Install via package manager (e.g., `sudo apt install git`)

### Terminal display issues

GitOut uses ANSI color codes for the terminal UI. If you see strange characters or formatting issues:

- Use a modern terminal emulator (Windows Terminal, iTerm2, GNOME Terminal, etc.)
- Ensure your terminal supports ANSI color codes
- Try setting `TERM=xterm-256color` environment variable

### Docker issues

If Docker commands fail:
- Ensure Docker is installed and running
- Check Docker daemon is running: `docker ps`
- On Linux, ensure your user is in the `docker` group or use `sudo`

## What You'll Learn

GitOut covers git concepts from beginner to advanced:

- Basic commands: `init`, `add`, `commit`, `status`, `log`
- Branching: `branch`, `checkout`, `merge`, conflict resolution
- Time travel: `restore`, `reset`, `reflog`, `cherry-pick`
- Advanced: `rebase`, `stash`, `tag`, `bisect`, `worktree`, `blame`, `hooks`
- Expert: interactive staging, submodules, filter-branch, and more!

## Contributing

Contributions are welcome! See [CONTRIBUTING.md](CONTRIBUTING.md) for development setup, architecture overview, and guidelines.

## Planning & Architecture

For internal planning documents and architecture decisions, see the [plans/](plans/) directory.

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Acknowledgments

Built with:
- [.NET 10](https://dotnet.microsoft.com/download/dotnet/10.0) for modern C# features
- [Spectre.Console](https://spectreconsole.net/) for beautiful terminal UI
- Git for teaching one of the most important developer tools
