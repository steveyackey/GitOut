using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;

namespace GitOut.Infrastructure.Persistence.RoomFactories;

public class Room08QuizMastersHallFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room08QuizMastersHallFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        var challenge = new QuizChallenge(
            id: "quiz-master-challenge",
            description: "Answer the Quiz Master's question about git commands",
            question: "What command stages all modified and new files in the current directory?",
            options: new List<string>
            {
                "git add .",
                "git commit -a",
                "git stage *",
                "git push --all"
            },
            correctAnswerIndex: 0,
            hint: "Think about the command that adds files to the staging area. The '.' means current directory."
        );

        var room = new Room(
            id: "room-8",
            name: "The Quiz Master's Hall",
            description: "A grand hall where an ancient sage tests your knowledge",
            narrative: "You enter a magnificent hall. At the far end, seated on a throne of crystallized commits, " +
                      "sits the Quiz Master - an ancient sage who has witnessed countless repository lifecycles. " +
                      "The sage speaks: 'You have traveled far and learned the basics. But your journey is not yet complete. " +
                      "Answer my question correctly, and I shall open the path to deeper mysteries.' " +
                      "\n\nThe Quiz Master asks: " + challenge.Question +
                      "\n\nUse 'answer <number>' to respond (e.g., 'answer 1' for the first option)",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-9" } },
            isStartRoom: false,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}
