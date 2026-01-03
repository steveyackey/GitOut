using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;

namespace GitOut.Infrastructure.Persistence.RoomFactories;

public class Room19BlameChamberFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room19BlameChamberFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        var challenge = new RepositoryChallenge(
            id: "blame-chamber-challenge",
            description: "Use git blame to investigate which commit modified specific lines of code",
            gitExecutor: _gitExecutor,
            requireGitInit: true,
            customSetup: async (workingDir, gitExec) =>
            {
                // Initialize repo
                await gitExec.ExecuteAsync("init", workingDir);
                await gitExec.ExecuteAsync("config user.email \"adventurer@gitout.com\"", workingDir);
                await gitExec.ExecuteAsync("config user.name \"Adventurer\"", workingDir);

                // Create a file and make multiple commits modifying different lines
                var codeContent = "function calculateTotal() {\n    return 0; // TODO: implement\n}\n";
                await File.WriteAllTextAsync(Path.Combine(workingDir, "calculator.js"), codeContent);
                await gitExec.ExecuteAsync("add calculator.js", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Initial calculator stub\"", workingDir);

                // Second commit: implement the function
                codeContent = "function calculateTotal(items) {\n    return items.reduce((sum, item) => sum + item.price, 0);\n}\n";
                await File.WriteAllTextAsync(Path.Combine(workingDir, "calculator.js"), codeContent);
                await gitExec.ExecuteAsync("add calculator.js", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Implement calculateTotal function\"", workingDir);

                // Third commit: add validation
                codeContent = "function calculateTotal(items) {\n    if (!Array.isArray(items)) throw new Error('Items must be an array');\n    return items.reduce((sum, item) => sum + item.price, 0);\n}\n";
                await File.WriteAllTextAsync(Path.Combine(workingDir, "calculator.js"), codeContent);
                await gitExec.ExecuteAsync("add calculator.js", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Add validation to calculateTotal\"", workingDir);

                // Fourth commit: fix a bug (wrong price field)
                codeContent = "function calculateTotal(items) {\n    if (!Array.isArray(items)) throw new Error('Items must be an array');\n    return items.reduce((sum, item) => sum + item.amount, 0);\n}\n";
                await File.WriteAllTextAsync(Path.Combine(workingDir, "calculator.js"), codeContent);
                await gitExec.ExecuteAsync("add calculator.js", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Fix bug: use amount instead of price\"", workingDir);

                // Create investigation file
                await File.WriteAllTextAsync(Path.Combine(workingDir, "INVESTIGATION.txt"),
                    "A bug was discovered in calculator.js!\n\n" +
                    "The function currently uses 'item.amount', but our tests expect 'item.price'.\n" +
                    "Seems like someone changed it in one of the commits.\n\n" +
                    "Use git blame to find out which commit last modified line 3 (the reduce line).\n\n" +
                    "Once you know which commit is responsible, create a file named CULPRIT.txt\n" +
                    "containing the word 'Fix' (since the culprit commit message starts with 'Fix').");
                await gitExec.ExecuteAsync("add INVESTIGATION.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Add investigation notes\"", workingDir);
            },
            customValidator: async (workingDir, gitExec) =>
            {
                // Check if player created CULPRIT.txt with correct answer
                var culpritPath = Path.Combine(workingDir, "CULPRIT.txt");
                if (File.Exists(culpritPath))
                {
                    var content = await File.ReadAllTextAsync(culpritPath);
                    if (content.Contains("Fix", StringComparison.OrdinalIgnoreCase))
                    {
                        return new ChallengeResult(
                            true,
                            "Excellent detective work! You've used git blame to trace line 3 back to the commit 'Fix bug: use amount instead of price'. " +
                            "This commit introduced the breaking change. In real projects, git blame is invaluable for understanding who changed what " +
                            "and why, especially when investigating bugs or understanding unfamiliar code. You can see the commit hash, author, " +
                            "date, and line content all together. Combined with git show or git log, it's a powerful forensic tool!",
                            null
                        );
                    }
                }

                return new ChallengeResult(
                    false,
                    "The culprit has not been identified yet.",
                    "Use 'git blame calculator.js' to see which commit last modified each line. Look for the commit that changed line 3. Create CULPRIT.txt with the keyword from that commit message."
                );
            }
        );

        var room = new Room(
            id: "room-19",
            name: "The Blame Chamber",
            description: "A forensic lab where code history is investigated line by line",
            narrative: "You enter a chamber filled with ancient scrolls and magnifying glasses. Each scroll represents a line of code, " +
                      "annotated with the name of the scribe who last modified it. A master investigator greets you: " +
                      "'When bugs appear or code confuses you, git blame reveals the history of each line—who wrote it, when, and in which commit.'" +
                      "\n\n[yellow]═══ Command Guide ═══[/]" +
                      "\n[cyan]git blame <file>[/] - Show line-by-line commit information" +
                      "\n  • Each line shows: commit hash | author | date | line number | content" +
                      "\n  • Example: a1b2c3d4 (Alice 2024-01-15 10) const x = 5;" +
                      "\n  • The commit hash can be used with git show or git log" +
                      "\n\n[cyan]git blame -L <start>,<end> <file>[/] - Blame specific line range" +
                      "\n  • Example: git blame -L 10,20 app.js" +
                      "\n  • Useful for focusing on a particular section" +
                      "\n\n[cyan]git blame -e <file>[/] - Show email addresses instead of names" +
                      "\n  • Helpful for identifying who to contact about a change" +
                      "\n\n[cyan]git blame -w <file>[/] - Ignore whitespace changes" +
                      "\n  • Helps skip commits that only reformatted code" +
                      "\n\n[cyan]git show <commit>[/] - View full commit details" +
                      "\n  • After finding a commit with blame, view the entire change" +
                      "\n  • Shows commit message, author, date, and full diff" +
                      "\n\n[cyan]Common workflow:[/]" +
                      "\n  1. git blame <file>  # Find the commit that changed a problematic line" +
                      "\n  2. Copy the commit hash" +
                      "\n  3. git show <hash>   # See the full context of that change" +
                      "\n  4. git log <hash>    # See commit history from that point" +
                      "\n\n[green]Pro tips:[/]" +
                      "\n  • Git blame isn't about assigning blame—it's about understanding history!" +
                      "\n  • Use it to learn why code was written a certain way" +
                      "\n  • Combine with git log -p to see how code evolved over time" +
                      "\n  • Some teams use 'git praise' as an alias to emphasize learning over blaming" +
                      "\n\n[dim]Every line tells a story. Git blame helps you read it![/]" +
                      "\n\n[yellow]The Scenario:[/]" +
                      "\n  • A bug was introduced in calculator.js" +
                      "\n  • The function uses 'item.amount' but should use 'item.price'" +
                      "\n  • You need to find which commit made this change" +
                      "\n\n[yellow]Your Mission:[/]" +
                      "\n  1. Read INVESTIGATION.txt for the full story" +
                      "\n  2. Use [cyan]git blame calculator.js[/] to see line-by-line authorship" +
                      "\n  3. Identify which commit modified line 3 (the one with 'reduce')" +
                      "\n  4. Notice the commit message starts with a specific keyword" +
                      "\n  5. Use git commands to create and commit a file named CULPRIT.txt" +
                      "\n  6. The file should contain the keyword from the culprit's commit message",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-20" } },
            isStartRoom: false,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}

