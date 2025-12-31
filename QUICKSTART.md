# GitOut - Quick Start Guide

## Installation & Running

```bash
# From the project root directory
cd /Users/steve/code/claude

# Run the game
dotnet run --project src/GitOut.Console
```

## Complete Walkthrough

### Starting the Game

```
Enter your name (or press Enter for 'Adventurer'): Alice
```

### Room 1: The Initialization Chamber

**What you'll see:**
- Title screen with ASCII art
- Room narrative about ancient runes
- Challenge description: "Initialize a git repository by running 'git init'"

**Commands to type:**
```
> git init
```

**Expected result:**
- "Challenge completed successfully!" message
- Exit "forward" becomes available

**Move to next room:**
```
> forward
```

### Room 2: The Staging Area

**What you'll see:**
- New room narrative about a floating scroll (README.md)
- The README.md file will be created in your temporary working directory
- Challenge description about staging and committing

**Commands to type:**
```
> git add README.md
> git commit -m "Initial commit"
```

**Expected result:**
- "Challenge completed successfully!" message
- Victory screen with your statistics

### Victory!

You'll see:
- Large "Victory!" ASCII art
- Game statistics table showing:
  - Player name
  - Rooms completed
  - Challenges completed
  - Total moves
  - Time played
- Congratulations message

## Example Full Session

```
$ dotnet run --project src/GitOut.Console

  ____  _ _    ___        _
 / ___|(_) |_ / _ \ _   _| |_
| |  _ | | __| | | | | | | __|
| |_| || | |_| |_| | |_| | |_
 \____||_|\__|\___/ \__,_|\__|

╔══════════════════════════════════════════════════════════╗
║ A Dungeon Crawler That Teaches Git                      ║
║                                                          ║
║ Learn git commands by escaping the dungeon!             ║
╚══════════════════════════════════════════════════════════╝

Enter your name (or press Enter for 'Adventurer'): Alice
Game started! Welcome, Alice!

Working Directory: /var/folders/.../GitOut/game/abc-123-def

━━━━━━━━━━━━━━━━━━━ The Initialization Chamber ━━━━━━━━━━━━━━━━━━━

You've entered a barren chamber. The walls are covered in ancient runes...
To unlock the door forward, you must run: git init

╭────────────────────────────────────────────╮
│              Challenge                     │
├────────────────────────────────────────────┤
│ ○ Initialize a git repository by running  │
│   'git init'                               │
╰────────────────────────────────────────────╯

Exits: forward

> git init
Initialized empty Git repository...

✓ Challenge completed! Challenge completed successfully!
You can now exit: forward

> forward
You move forward...

━━━━━━━━━━━━━━━━━━━ The Staging Area ━━━━━━━━━━━━━━━━━━━

You enter a chamber bathed in ethereal light...
The scroll (README.md) has appeared in your working directory.

To complete this challenge:
  1. Stage the file: git add README.md
  2. Commit the changes: git commit -m "Your message here"

╭────────────────────────────────────────────╮
│              Challenge                     │
├────────────────────────────────────────────┤
│ ○ Stage the README.md file and commit it  │
╰────────────────────────────────────────────╯

> git add README.md

> git commit -m "Initial commit"
[master (root-commit) abc123] Initial commit
 1 file changed, 3 insertions(+)
 create mode 100644 README.md

✓ Challenge completed! Challenge completed successfully!

*** CONGRATULATIONS! You've completed the game! ***

__     ___      _                    _
\ \   / (_) ___| |_ ___  _ __ _   _| |
 \ \ / /| |/ __| __/ _ \| '__| | | | |
  \ V / | | (__| || (_) | |  | |_| |_|
   \_/  |_|\___|\__\___/|_|   \__, (_)
                              |___/

╔════════════════════════════════════════╗
║        Game Statistics                 ║
╠════════════════════════════════════════╣
║ Player              │ Alice            ║
║ Rooms Completed     │ 1                ║
║ Challenges Complete │ 2                ║
║ Total Moves         │ 1                ║
║ Time Played         │ 00:02:15         ║
╚════════════════════════════════════════╝

Congratulations! You've mastered the basics of Git!
```

## Helpful Commands While Playing

- `help` - See all available commands
- `status` - Check your progress
- `look` - Re-read the current room description
- `git status` - Check your git repository status
- `exit` - Exit the game (with confirmation and save prompt)

## Testing Your Installation

Run the test suite to verify everything works:

```bash
dotnet test
```

Expected output:
- 33 tests passed in GitOut.Domain.Tests
- 37 tests passed in GitOut.Infrastructure.Tests
- Total: 70 tests, 0 failures

## Troubleshooting

**Problem**: Git commands don't work
- **Solution**: Make sure git is installed (`git --version`)

**Problem**: "Directory not found" errors
- **Solution**: The game creates temporary directories automatically. Ensure you have write permissions to /tmp

**Problem**: Tests fail
- **Solution**: Run `dotnet clean` followed by `dotnet build` and try again

## Learning Objectives

By completing GitOut Phase 1, you will learn:
- ✅ How to initialize a git repository (`git init`)
- ✅ How to stage files for commit (`git add`)
- ✅ How to create commits (`git commit`)
- ✅ Basic git workflow concepts

Enjoy the game!
