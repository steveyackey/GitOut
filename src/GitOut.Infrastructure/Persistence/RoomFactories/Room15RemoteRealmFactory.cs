using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;

namespace GitOut.Infrastructure.Persistence.RoomFactories;

public class Room15RemoteRealmFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room15RemoteRealmFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        var challenge = new RepositoryChallenge(
            id: "remote-realm-challenge",
            description: "Set up a simulated remote repository connection and push your work",
            gitExecutor: _gitExecutor,
            requireGitInit: true,
            customSetup: async (workingDir, gitExec) =>
            {
                // Initialize repo
                await gitExec.ExecuteAsync("init", workingDir);
                await gitExec.ExecuteAsync("config user.email \"adventurer@gitout.com\"", workingDir);
                await gitExec.ExecuteAsync("config user.name \"Adventurer\"", workingDir);

                // Create a commit
                await File.WriteAllTextAsync(Path.Combine(workingDir, "local-work.txt"), "Work done locally");
                await gitExec.ExecuteAsync("add local-work.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Local work\"", workingDir);

                // Create a "remote" repository in a separate directory (simulated remote)
                var remotePath = Path.Combine(Path.GetDirectoryName(workingDir)!, "remote-repo.git");
                Directory.CreateDirectory(remotePath);
                await gitExec.ExecuteAsync("init --bare", remotePath);

                // Create a file with instructions
                await File.WriteAllTextAsync(Path.Combine(workingDir, "REMOTE_PATH.txt"),
                    $"Add this as your remote:\n{remotePath}\n\nUse: git remote add origin {remotePath}");
                await gitExec.ExecuteAsync("add REMOTE_PATH.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Add remote instructions\"", workingDir);
            },
            customValidator: async (workingDir, gitExec) =>
            {
                // Check if remote has been added
                var remotes = await gitExec.GetRemotesAsync(workingDir);

                if (string.IsNullOrWhiteSpace(remotes))
                {
                    return new ChallengeResult(
                        false,
                        "No remote connection has been established yet.",
                        "Check REMOTE_PATH.txt for the path, then use 'git remote add origin <path>'"
                    );
                }

                if (!remotes.Contains("origin"))
                {
                    return new ChallengeResult(
                        false,
                        "A remote exists but it's not named 'origin'.",
                        "Use 'git remote add origin <path>' to add a remote named 'origin'"
                    );
                }

                // Check if commits have been pushed to the remote
                // Check if the remote has any refs (indicating a push was made)
                var remoteRefsResult = await gitExec.ExecuteAsync("ls-remote origin", workingDir);
                
                if (!remoteRefsResult.Success || string.IsNullOrWhiteSpace(remoteRefsResult.Output) || !remoteRefsResult.Output.Contains("refs/heads/"))
                {
                    return new ChallengeResult(
                        false,
                        "Remote 'origin' is configured, but you haven't pushed your commits yet!",
                        "Use 'git push -u origin main' (or 'master') to push your local commits to the remote"
                    );
                }

                return new ChallengeResult(
                    true,
                    "The portal to the remote realm has been opened AND your work has been transmitted! You've successfully " +
                    "configured a remote repository connection and pushed your commits. " +
                    "In real projects, this would be GitHub, GitLab, or another server. Remotes enable collaboration and backup!",
                    null
                );
            }
        );

        var room = new Room(
            id: "room-15",
            name: "The Remote Realm",
            description: "A gateway to distant repositories",
            narrative: "You stand before a shimmering portal that leads to distant realms. Through it, you can see other repositories - " +
                      "copies of your work existing in far-off servers. " +
                      "\n\nA portal guardian explains: 'Remotes are connections to other repositories. The most common remote is called " +
                      "\"origin\" - typically your GitHub, GitLab, or Bitbucket repository. Remotes let you push your work for backup " +
                      "and collaboration, and fetch others' work.' " +
                      "\n\nA simulated remote repository has been prepared for you. The file REMOTE_PATH.txt in your working directory contains the path you'll need. " +
                      "You need to connect to it by adding it as a remote named 'origin', then push your local commits to share them with the remote realm!" +
                      "\n\n[yellow]═══ Command Guide ═══[/]" +
                      "\n[cyan]git remote add <name> <url>[/] - Adds a new remote repository" +
                      "\n  • <name> is typically 'origin' (the default remote)" +
                      "\n  • <url> can be: HTTPS (https://github.com/user/repo.git)" +
                      "\n                 SSH (git@github.com:user/repo.git)" +
                      "\n                 Local path (for testing)" +
                      "\n  • You can have multiple remotes with different names" +
                      "\n\n[cyan]git remote -v[/] - Lists all remotes with their URLs" +
                      "\n  • Shows both fetch and push URLs" +
                      "\n  • Helps verify your remote configuration" +
                      "\n\n[cyan]git remote remove <name>[/] - Removes a remote" +
                      "\n[cyan]git remote rename <old> <new>[/] - Renames a remote" +
                      "\n\n[cyan]Common workflow with remotes:[/]" +
                      "\n  1. [cyan]git clone <url>[/] - Downloads repo and auto-sets up 'origin' remote" +
                      "\n  2. [cyan]git fetch origin[/] - Downloads new commits from remote (doesn't merge)" +
                      "\n  3. [cyan]git pull origin main[/] - Fetches and merges remote changes into your branch" +
                      "\n  4. [cyan]git push origin main[/] - Uploads your commits to remote" +
                      "\n\n[cyan]Why remotes matter:[/]" +
                      "\n  • Backup your work in the cloud" +
                      "\n  • Collaborate with team members" +
                      "\n  • Contribute to open source projects" +
                      "\n  • Access your code from anywhere" +
                      "\n\n[dim]Think of remotes as bookmarks to other repositories![/]" +
                      "\n\n[yellow]To complete this challenge:[/]" +
                      "\n  1. View the remote path: [cyan]git show HEAD:REMOTE_PATH.txt[/]" +
                      "\n  2. Add the remote: [cyan]git remote add origin <path-from-REMOTE_PATH.txt>[/]" +
                      "\n  3. Verify the remote: [cyan]git remote -v[/]" +
                      "\n  4. Push your commits to the remote: [cyan]git push -u origin main[/]" +
                      "\n     (use 'master' instead of 'main' if that's your branch name)",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-16" } },
            isStartRoom: false,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}
