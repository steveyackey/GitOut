---
description: Create a semantic version release based on conventional commits
---

# Release Command

Analyze commits since the last tag and create a new semantic version release.

## Instructions

When the user runs `/release`, perform the following steps:

### 1. Get Current Version

Run: `git describe --tags --abbrev=0 2>/dev/null || echo "v0.0.0"`

If no tags exist, start from v0.0.0.

### 2. List Commits Since Last Tag

Run: `git log <last-tag>..HEAD --oneline`

If this is the first release (v0.0.0), use: `git log --oneline`

### 3. Analyze Conventional Commits

Parse each commit message to determine the version bump:

**MAJOR bump** (breaking change) if ANY commit:
- Contains `BREAKING CHANGE:` in the body/footer
- Has `!` after the type (e.g., `feat!:`, `fix!:`, `refactor!:`)

**MINOR bump** (new feature) if ANY commit:
- Starts with `feat:` or `feat(scope):`

**PATCH bump** (bug fix, etc.) for:
- `fix:`, `perf:`, or any other conventional commit type
- This is the default if no feat or breaking changes

### 4. Calculate New Version

Parse current version (e.g., v1.2.3) into MAJOR.MINOR.PATCH:

- If MAJOR bump: increment MAJOR, reset MINOR and PATCH to 0
- If MINOR bump: increment MINOR, reset PATCH to 0
- If PATCH bump: increment PATCH

### 5. Show Summary and Confirm

Display to the user:
```
Analyzing commits since <last-tag>...

Found <N> commits:
- <commit summaries>

Detected changes:
- Breaking changes: <yes/no>
- New features: <count>
- Bug fixes: <count>
- Other: <count>

Version bump: <MAJOR/MINOR/PATCH>
Current version: <current>
New version: <new>

Ready to create release?
```

Use AskUserQuestion to confirm before proceeding.

### 6. Create and Push Tag

If confirmed:

```bash
git tag -a v<new-version> -m "Release v<new-version>"
git push origin v<new-version>
```

### 7. Report Success

Tell the user:
```
Release v<new-version> created!

The GitHub Actions release workflow will now:
- Build binaries for Windows, Linux, and macOS
- Build and push Docker image
- Create GitHub release with artifacts

Monitor progress at: https://github.com/<owner>/<repo>/actions
```

## Command Options

If the user provides arguments:

- `--dry-run`: Show what would happen without creating the tag
- `--major`: Force a MAJOR version bump (ignore commit analysis)
- `--minor`: Force a MINOR version bump (ignore commit analysis)
- `--patch`: Force a PATCH version bump (ignore commit analysis)

## Example

```
User: /release

Claude: Analyzing commits since v1.2.3...

Found 5 commits:
- feat(room): add Room 24 for advanced worktree
- fix(challenge): correct merge validation
- docs: update README
- test: add integration tests for Room 24
- chore: update dependencies

Detected changes:
- Breaking changes: no
- New features: 1
- Bug fixes: 1
- Other: 3

Version bump: MINOR
Current version: v1.2.3
New version: v1.3.0

[Confirms with user]

Creating tag v1.3.0...
Pushing to origin...

Release v1.3.0 created! GitHub Actions will build and publish artifacts.
```

## Error Handling

- If working directory is dirty: warn user and suggest committing first
- If not on main branch: warn user (releases typically from main)
- If tag already exists: error and suggest different version
- If push fails: provide troubleshooting steps
