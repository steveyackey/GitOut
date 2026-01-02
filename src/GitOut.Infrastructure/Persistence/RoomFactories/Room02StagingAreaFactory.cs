using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;

namespace GitOut.Infrastructure.Persistence.RoomFactories;

public class Room02StagingAreaFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room02StagingAreaFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        var challenge = new RepositoryChallenge(
            id: "staging-area-challenge",
            description: "Stage the README.md file and commit it",
            gitExecutor: _gitExecutor,
            requireGitInit: true,
            requireCleanStatus: true,
            requiredCommitCount: 1,
            requiredFiles: new List<string> { "README.md" },
            setupFiles: new List<string> { "README.md" }
        );

        var room = new Room(
            id: "room-2",
            name: "The Staging Area",
            description: "A mystical chamber with a floating scroll",
            narrative: "You enter a chamber bathed in ethereal light. A scroll materializes before you, labeled 'README.md'. " +
                      "The door behind you seals shut with a resonant boom. An inscription appears on the wall: " +
                      "'To escape, you must preserve this knowledge - stage the scroll and seal it with a commit.' " +
                      "\n\nThe scroll (README.md) has appeared in your working directory. " +
                      "\n\n[yellow]═══ Command Guide ═══[/]" +
                      "\n[cyan]git add <file>[/] - Stages a file, preparing it for commit" +
                      "\n  • Tells git 'I want to save this file in my next snapshot'" +
                      "\n  • Files must be staged before they can be committed" +
                      "\n  • Think of it as selecting which changes to save" +
                      "\n\n[cyan]git commit -m \"message\"[/] - Creates a permanent snapshot of all staged changes" +
                      "\n  • Saves your staged files to the repository history" +
                      "\n  • The message describes what changed and why" +
                      "\n  • Creates a 'save point' you can return to later" +
                      "\n\nTo complete this challenge:" +
                      "\n  1. Stage the file: [cyan]git add README.md[/]" +
                      "\n  2. Commit the changes: [cyan]git commit -m \"Seal the ancient scroll\"[/]",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-3" } },
            isStartRoom: false,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}
