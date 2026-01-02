using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;

namespace GitOut.Infrastructure.Persistence.RoomFactories;

public class Room13TagTowerFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room13TagTowerFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        var challenge = new RepositoryChallenge(
            id: "tag-tower-challenge",
            description: "Create version tags to mark important milestones",
            gitExecutor: _gitExecutor,
            requireGitInit: true,
            customSetup: async (workingDir, gitExec) =>
            {
                // Initialize repo
                await gitExec.ExecuteAsync("init", workingDir);
                await gitExec.ExecuteAsync("config user.email \"adventurer@gitout.com\"", workingDir);
                await gitExec.ExecuteAsync("config user.name \"Adventurer\"", workingDir);

                // Create version commits
                await File.WriteAllTextAsync(Path.Combine(workingDir, "VERSION"), "1.0.0");
                await gitExec.ExecuteAsync("add VERSION", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Release version 1.0.0\"", workingDir);

                await File.WriteAllTextAsync(Path.Combine(workingDir, "VERSION"), "1.1.0");
                await gitExec.ExecuteAsync("add VERSION", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Release version 1.1.0\"", workingDir);

                await File.WriteAllTextAsync(Path.Combine(workingDir, "VERSION"), "2.0.0");
                await gitExec.ExecuteAsync("add VERSION", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Major release version 2.0.0\"", workingDir);
            },
            customValidator: async (workingDir, gitExec) =>
            {
                // Check if tags exist
                var tags = await gitExec.GetTagsAsync(workingDir);

                bool hasV1 = tags.Contains("v1.0.0");
                bool hasV2 = tags.Contains("v2.0.0");

                if (hasV1 && hasV2)
                {
                    return new ChallengeResult(
                        true,
                        "The version markers have been placed in the tower! You've successfully tagged important releases, " +
                        "making it easy to reference and return to these specific versions in the future!",
                        null
                    );
                }

                if (!hasV1 && !hasV2)
                {
                    return new ChallengeResult(
                        false,
                        "No version tags have been created yet.",
                        "Use 'git log --oneline' to find commits, then 'git tag v1.0.0 <commit-hash>' and 'git tag v2.0.0 <commit-hash>'"
                    );
                }

                return new ChallengeResult(
                    false,
                    "You've created some tags, but not all required versions are tagged.",
                    "You need both v1.0.0 and v2.0.0 tags. Use 'git tag' to list existing tags and 'git tag <name> <commit>' to create missing ones"
                );
            }
        );

        var room = new Room(
            id: "room-13",
            name: "The Tag Tower",
            description: "A tower containing markers for significant moments",
            narrative: "You enter a tall tower with crystalline markers floating at different heights, each representing a significant " +
                      "moment in the repository's history. " +
                      "\n\nThe tower keeper explains: 'Tags are permanent bookmarks for important commits. We use them to mark releases, " +
                      "versions, and other milestones. Unlike branches, tags don't move - they permanently point to a specific commit.' " +
                      "\n\nYou see three commits representing three releases: 1.0.0, 1.1.0, and 2.0.0. The tower keeper asks you to " +
                      "create tags for the major releases: v1.0.0 and v2.0.0." +
                      "\n\n[yellow]═══ Command Guide ═══[/]" +
                      "\n[cyan]git tag <tag-name>[/] - Creates a lightweight tag at the current commit" +
                      "\n  • Simple pointer to a specific commit" +
                      "\n  • Commonly used for version markers (v1.0.0, v2.0.0, etc.)" +
                      "\n  • Tags don't move like branches - they're permanent bookmarks" +
                      "\n\n[cyan]git tag <tag-name> <commit-hash>[/] - Creates a tag at a specific commit" +
                      "\n  • Can tag any commit, not just the current one" +
                      "\n  • Useful for marking past releases" +
                      "\n\n[cyan]git tag -a <tag-name> -m \"message\"[/] - Creates an annotated tag" +
                      "\n  • Stores extra metadata (tagger, date, message)" +
                      "\n  • Recommended for releases" +
                      "\n  • Can be signed with GPG for verification" +
                      "\n\n[cyan]git tag[/] - Lists all tags" +
                      "\n[cyan]git show <tag-name>[/] - Shows tag details and the tagged commit" +
                      "\n\n[cyan]Common tag naming:[/]" +
                      "\n  • Semantic versioning: v1.0.0, v2.1.3, v3.0.0-beta" +
                      "\n  • Date-based: release-2024-01-15" +
                      "\n  • Milestone-based: alpha-1, beta-2, rc-1" +
                      "\n\n[dim]Think of tags as bookmarks for important commits![/]" +
                      "\n\n[yellow]To complete this challenge:[/]" +
                      "\n  1. View the commit history: [cyan]git log --oneline[/]" +
                      "\n  2. Find the commit with 'version 1.0.0' message and note its hash" +
                      "\n  3. Tag it: [cyan]git tag v1.0.0 <commit-hash>[/]" +
                      "\n  4. Find the commit with 'version 2.0.0' message and note its hash" +
                      "\n  5. Tag it: [cyan]git tag v2.0.0 <commit-hash>[/]" +
                      "\n  6. Verify your tags: [cyan]git tag[/]" +
                      "\n\n[dim]═══ Tip: Pushing Tags to Remote ═══[/]" +
                      "\n[dim]Tags are local by default. To share them with a remote repository:[/]" +
                      "\n  [cyan]git push origin <tag-name>[/]  - Push a single tag" +
                      "\n  [cyan]git push origin --tags[/]     - Push all tags" +
                      "\n\n[dim]Don't run these commands here (no remote configured), but we'll learn" +
                      "\nmore about remotes soon. This is how you'd share your version tags with" +
                      "\nyour team on GitHub, GitLab, or other hosting services.[/]",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-14" } },
            isStartRoom: false,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}
