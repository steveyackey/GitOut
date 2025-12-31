using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;

namespace GitOut.Infrastructure.Persistence.RoomFactories;

public class Room01InitializationChamberFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room01InitializationChamberFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        var challenge = new RepositoryChallenge(
            id: "init-chamber-challenge",
            description: "Initialize a git repository by running 'git init'",
            gitExecutor: _gitExecutor,
            requireGitInit: true
        );

        var room = new Room(
            id: "room-1",
            name: "The Initialization Chamber",
            description: "A barren chamber with ancient walls",
            narrative: "You've entered a barren chamber. The walls are covered in ancient runes that seem to pulse with a faint light. " +
                      "In the center of the room, you see an empty pedestal with an inscription: " +
                      "'To proceed, you must create the foundation - initialize the repository of knowledge.' " +
                      "\n\nTo unlock the door forward, you must run: [cyan]git init[/]" +
                      "\n\n[yellow]═══ Command Guide ═══[/]" +
                      "\n[cyan]git init[/] - Creates a new git repository in the current directory" +
                      "\n  • Creates a hidden .git folder that stores all version history" +
                      "\n  • This is always the first command when starting a new project" +
                      "\n  • Only needs to be run once per project",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-2" } },
            isStartRoom: true,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}
