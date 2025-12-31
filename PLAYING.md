# How to Play GitOut

GitOut is a dungeon crawler game that teaches you git commands. Complete challenges in each room to progress through the dungeon!

## Running the Game

```bash
dotnet run --project src/GitOut.Console
```

## Game Commands

### Navigation
- `help` - Show available commands
- `status` - Show your progress and current room info
- `look` or `examine` - Examine the current room
- `forward`, `back`, etc. - Move in available directions
- `go <direction>` - Move in a specific direction

### Git Commands
- `git <command>` - Execute any git command (e.g., `git init`, `git status`, `git add`, `git commit`)

### Challenge Commands
- `answer <number>` - Answer a quiz question (e.g., `answer 1` for first option)
- `hint` - Get a hint for the current challenge

### System Commands
- `exit` - Exit the game (prompts to save before exiting)

## Walkthrough: Completing the Game

### Room 1: The Initialization Chamber
**Git Command**: `git init`
**Objective**: Initialize a git repository

1. Read the room narrative (shown automatically)
2. Run: `git init`
3. Once successful, move forward: `forward`

### Room 2: The Staging Area
**Git Commands**: `git add`, `git commit`
**Objective**: Stage and commit the README.md file

The game will create a `README.md` file in your working directory.

1. Stage the file: `git add README.md`
2. Commit the changes: `git commit -m "Your message here"`
3. Move forward: `forward`

### Room 3: The History Archive
**Git Command**: `git log`
**Objective**: View the commit history

The room contains 3 commits representing ancient scrolls.

1. Run: `git log` to view the commit history
2. The challenge auto-completes once you can see the commits
3. Move forward: `forward`

### Room 4: The Status Chamber
**Git Command**: `git status`
**Objective**: Examine repository state

Files exist in various states (tracked, modified, staged, untracked).

1. Run: `git status` to see the file states
2. The challenge completes once status is checked
3. Move forward: `forward`

### Room 5: The Branch Junction
**Git Command**: `git branch feature-branch` or `git checkout -b feature-branch`
**Objective**: Create a new branch called "feature-branch"

1. Run: `git branch feature-branch` to create the branch
2. Or use: `git checkout -b feature-branch` to create and switch to it
3. Move forward: `forward`

### Room 6: The Merge Nexus
**Git Command**: `git merge feature`
**Objective**: Merge the feature branch into main

You'll be on the main branch. A feature branch exists with new commits.

1. Run: `git merge feature` to merge the branches
2. The merge will be a fast-forward (no conflicts)
3. Move forward: `forward`

### Room 7: The Restoration Vault
**Git Command**: `git restore sacred-text.txt` or `git checkout -- sacred-text.txt`
**Objective**: Restore a corrupted file to its committed state

A file has been corrupted and needs restoration.

1. Run: `git restore sacred-text.txt` (or `git checkout -- sacred-text.txt`)
2. The file will be restored to its original content
3. Move forward: `forward`

### Room 8: The Quiz Master's Hall
**Challenge Type**: Quiz
**Objective**: Answer the quiz question correctly

The Quiz Master asks: "What command stages all modified and new files in the current directory?"

Options:
1. git add .
2. git commit -a
3. git stage *
4. git push --all

1. Use: `answer 1` to select the first option (the correct answer)
2. Challenge complete! You've won the game!

## Tips

- Use `git status` frequently to check your repository state
- Read room narratives carefully for hints
- You must complete a room's challenge before you can exit
- The game creates a temporary working directory that is cleaned up when you exit

## Testing

Run the automated test suite:

```bash
dotnet test
```

The project has 120+ tests with comprehensive coverage of:
- Domain logic (Game, Player, Room entities)
- All 3 challenge types (Repository, Quiz, Scenario)
- Git command execution
- File system management
- JSON progress save/load
- End-to-end gameplay scenarios
- All 8 rooms integration tested

## Architecture

GitOut follows Clean Architecture principles:

- **Domain Layer** (`GitOut.Domain`): Core entities and business logic
- **Application Layer** (`GitOut.Application`): Use cases and interfaces
- **Infrastructure Layer** (`GitOut.Infrastructure`): Git execution, file system, persistence
- **Console Layer** (`GitOut.Console`): User interface with Spectre.Console

Enjoy learning git through gameplay!
