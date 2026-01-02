using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;

namespace GitOut.Infrastructure.Persistence.RoomFactories;

public class Room04StatusChamberFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room04StatusChamberFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        var challenge = new RepositoryChallenge(
            id: "status-chamber-challenge",
            description: "Examine the repository state using 'git status'",
            gitExecutor: _gitExecutor,
            requireGitInit: true,
            customSetup: async (workingDir, gitExec) =>
            {
                // Initialize repo with initial commit
                await gitExec.ExecuteAsync("init", workingDir);
                await gitExec.ExecuteAsync("config user.email \"adventurer@gitout.game\"", workingDir);
                await gitExec.ExecuteAsync("config user.name \"Adventurer\"", workingDir);
                await File.WriteAllTextAsync(Path.Combine(workingDir, "tracked.txt"), "I am tracked");
                await gitExec.ExecuteAsync("add tracked.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Initial commit\"", workingDir);

                // Create an untracked file
                await File.WriteAllTextAsync(Path.Combine(workingDir, "untracked.txt"), "I am untracked");

                // Modify a tracked file
                await File.WriteAllTextAsync(Path.Combine(workingDir, "tracked.txt"), "I am now modified");

                // Create and stage a new file
                await File.WriteAllTextAsync(Path.Combine(workingDir, "staged.txt"), "I am staged");
                await gitExec.ExecuteAsync("add staged.txt", workingDir);
            },
            customValidator: async (workingDir, gitExec) =>
            {
                // Check if they ran git status by verifying the repo has the expected state
                var status = await gitExec.GetStatusAsync(workingDir);

                // We'll consider it successful if they've examined the status
                // (we can't directly track if they ran the command, but the challenge setup ensures state)
                if (status.Contains("Changes") || status.Contains("Untracked") || status.Contains("modified"))
                {
                    return new ChallengeResult(
                        true,
                        "You've successfully examined the state of the repository! Understanding git status is crucial for knowing what changes exist in your working directory and staging area. This command will become your constant companion in daily git work.",
                        null
                    );
                }

                return new ChallengeResult(
                    false,
                    "The mirrors remain clouded.",
                    "Use 'git status' to see the state of files in the repository."
                );
            }
        );

        var room = new Room(
            id: "room-4",
            name: "The Status Chamber",
            description: "A room with three mystical mirrors",
            narrative: "You enter a chamber dominated by three large mirrors. Each mirror shows a different reflection: " +
                      "one shows files in pristine condition, another shows files in flux, and the third shows files in shadow. " +
                      "A plaque reads: 'Understanding the state of your realm is the key to mastery. " +
                      "Files exist in many states: tracked, modified, staged, and untracked.' " +
                      "\n\n[yellow]═══ Command Guide ═══[/]" +
                      "\n[cyan]git status[/] - Shows the current state of your working directory and staging area" +
                      "\n  • Lists which files are modified (changed but not staged)" +
                      "\n  • Lists which files are staged (ready to be committed)" +
                      "\n  • Lists which files are untracked (not yet added to git)" +
                      "\n  • Shows which branch you're on" +
                      "\n  • Use this constantly to see what's happening in your repo!" +
                      "\n\nRun [cyan]git status[/] to understand the current state of the repository.",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-5" } },
            isStartRoom: false,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}
