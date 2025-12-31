using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;

namespace GitOut.Infrastructure.Persistence.RoomFactories;

public class Room03HistoryArchiveFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room03HistoryArchiveFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        var challenge = new RepositoryChallenge(
            id: "history-archive-challenge",
            description: "View the commit history by running 'git log'",
            gitExecutor: _gitExecutor,
            requireGitInit: true,
            customSetup: async (workingDir, gitExec) =>
            {
                // Initialize repo and create 3-5 commits
                await gitExec.ExecuteAsync("init", workingDir);

                // Create first commit
                await File.WriteAllTextAsync(Path.Combine(workingDir, "scroll1.txt"), "The First Chronicle");
                await gitExec.ExecuteAsync("add scroll1.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"First scroll discovered\"", workingDir);

                // Create second commit
                await File.WriteAllTextAsync(Path.Combine(workingDir, "scroll2.txt"), "The Second Chronicle");
                await gitExec.ExecuteAsync("add scroll2.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Second scroll uncovered\"", workingDir);

                // Create third commit
                await File.WriteAllTextAsync(Path.Combine(workingDir, "scroll3.txt"), "The Third Chronicle");
                await gitExec.ExecuteAsync("add scroll3.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Third scroll revealed\"", workingDir);
            },
            customValidator: async (workingDir, gitExec) =>
            {
                // Just check if they successfully ran git log (we can't directly detect this,
                // but we'll validate that the repo has commits)
                var log = await gitExec.GetLogAsync(workingDir, 10);
                if (!string.IsNullOrWhiteSpace(log) && log.Split('\n').Length >= 3)
                {
                    return new ChallengeResult(
                        true,
                        "You've successfully viewed the ancient chronicles!",
                        null
                    );
                }

                return new ChallengeResult(
                    false,
                    "You haven't viewed the history yet.",
                    "Try running 'git log' to see the commit history."
                );
            }
        );

        var room = new Room(
            id: "room-3",
            name: "The History Archive",
            description: "An ancient library with scrolls floating in the air",
            narrative: "You step into a vast circular chamber. The walls are lined with countless scrolls, " +
                      "each one glowing with an ethereal light. They float gently in the air, arranged in chronological order. " +
                      "An inscription on the floor reads: 'Every action leaves a mark. Every commit tells a story. " +
                      "To proceed, you must witness the chronicles of this repository.' " +
                      "\n\nUse [cyan]git log[/] to view the commit history and understand what has transpired here." +
                      "\n\n[yellow]═══ Command Guide ═══[/]" +
                      "\n[cyan]git log[/] - Shows the history of all commits in the repository" +
                      "\n  • Displays commits in reverse chronological order (newest first)" +
                      "\n  • Shows commit hash, author, date, and message for each commit" +
                      "\n  • Use arrow keys to scroll, press 'q' to quit" +
                      "\n  • Useful flags: --oneline (compact view), --graph (visual tree)",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-4" } },
            isStartRoom: false,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}
