using GitOut.Domain.Challenges;
using GitOut.Domain.Entities;
using GitOut.Domain.Interfaces;

namespace GitOut.Infrastructure.Persistence.RoomFactories;

public class Room21SubmoduleSanctumFactory
{
    private readonly IGitCommandExecutor _gitExecutor;

    public Room21SubmoduleSanctumFactory(IGitCommandExecutor gitExecutor)
    {
        _gitExecutor = gitExecutor ?? throw new ArgumentNullException(nameof(gitExecutor));
    }

    public Task<Room> CreateAsync()
    {
        var challenge = new RepositoryChallenge(
            id: "submodule-sanctum-challenge",
            description: "Add a library repository as a git submodule",
            gitExecutor: _gitExecutor,
            requireGitInit: true,
            customSetup: async (workingDir, gitExec) =>
            {
                // Initialize main repo
                await gitExec.ExecuteAsync("init", workingDir);
                await gitExec.ExecuteAsync("config user.email \"adventurer@gitout.com\"", workingDir);
                await gitExec.ExecuteAsync("config user.name \"Adventurer\"", workingDir);

                // Allow file:// protocol for submodules (needed for local testing)
                await gitExec.ExecuteAsync("config protocol.file.allow always", workingDir);

                // Create initial commit in main repo
                await File.WriteAllTextAsync(Path.Combine(workingDir, "main-app.txt"),
                    "Main application code");
                await gitExec.ExecuteAsync("add main-app.txt", workingDir);
                await gitExec.ExecuteAsync("commit -m \"Initial main app\"", workingDir);

                // Create a "library" repository in a separate location (simulated external repo)
                var parentDir = Path.GetDirectoryName(workingDir)!;
                var libraryPath = Path.Combine(parentDir, "magic-library");
                Directory.CreateDirectory(libraryPath);

                // Initialize the library repo
                await gitExec.ExecuteAsync("init", libraryPath);
                await gitExec.ExecuteAsync("config user.email \"library@gitout.com\"", libraryPath);
                await gitExec.ExecuteAsync("config user.name \"Library Maintainer\"", libraryPath);

                // Add some content to library repo
                await File.WriteAllTextAsync(Path.Combine(libraryPath, "magic-spell.js"),
                    "export function castSpell(name) { return `Casting ${name}!`; }");
                await gitExec.ExecuteAsync("add magic-spell.js", libraryPath);
                await gitExec.ExecuteAsync("commit -m \"Initial magic library\"", libraryPath);

                // Add more commits to library
                await File.WriteAllTextAsync(Path.Combine(libraryPath, "README.md"),
                    "# Magic Library\n\nA collection of powerful spells.");
                await gitExec.ExecuteAsync("add README.md", libraryPath);
                await gitExec.ExecuteAsync("commit -m \"Add README\"", libraryPath);

                // Create instructions file in main repo
                await File.WriteAllTextAsync(Path.Combine(workingDir, "SUBMODULE_INSTRUCTIONS.txt"),
                    $"The Magic Library Repository Path:\n{libraryPath}\n\n" +
                    "To complete this challenge:\n" +
                    "1. Add the library as a submodule in a 'lib' directory:\n" +
                    $"   git -c protocol.file.allow=always submodule add {libraryPath} lib\n\n" +
                    "   Note: The -c flag allows local file:// protocol (needed for this challenge)\n\n" +
                    "2. Check the status to see what changed:\n" +
                    "   git status\n\n" +
                    "3. Note that git created:\n" +
                    "   - .gitmodules file (tracks submodule configuration)\n" +
                    "   - lib/ directory (contains the submodule repository)\n\n" +
                    "4. Commit the submodule addition:\n" +
                    "   git add .gitmodules lib\n" +
                    "   git commit -m \"Add magic library as submodule\"\n\n" +
                    "What are submodules?\n" +
                    "- A way to keep a git repository inside another git repository\n" +
                    "- Useful for including external libraries or shared code\n" +
                    "- Each submodule tracks a specific commit of the external repo\n" +
                    "- Common in projects that depend on third-party libraries\n");
            },
            customValidator: async (workingDir, gitExec) =>
            {
                // Check 1: .gitmodules file must exist
                var gitmodulesPath = Path.Combine(workingDir, ".gitmodules");
                if (!File.Exists(gitmodulesPath))
                {
                    return new ChallengeResult(
                        false,
                        "The .gitmodules configuration file doesn't exist yet.",
                        "Add the library as a submodule using: git submodule add <path> lib"
                    );
                }

                // Check 2: Parse .gitmodules to verify submodule entry
                var gitmodulesContent = await File.ReadAllTextAsync(gitmodulesPath);
                if (!gitmodulesContent.Contains("[submodule"))
                {
                    return new ChallengeResult(
                        false,
                        "The .gitmodules file exists but doesn't contain a submodule configuration.",
                        "Make sure you used 'git submodule add <path> lib' correctly"
                    );
                }

                // Check 3: Verify submodule directory exists
                var submodulePath = Path.Combine(workingDir, "lib");
                if (!Directory.Exists(submodulePath))
                {
                    return new ChallengeResult(
                        false,
                        "The submodule directory 'lib' doesn't exist.",
                        "The submodule should be added in a directory called 'lib'"
                    );
                }

                // Check 4: Verify submodule is a git repository
                var submoduleGitPath = Path.Combine(submodulePath, ".git");
                if (!File.Exists(submoduleGitPath) && !Directory.Exists(submoduleGitPath))
                {
                    return new ChallengeResult(
                        false,
                        "The lib directory exists but doesn't appear to be a git repository.",
                        "Make sure to use 'git submodule add' command, not just copying files"
                    );
                }

                // Check 5: Verify the submodule was committed
                var log = await gitExec.GetLogAsync(workingDir, 5);
                if (!log.Contains("submodule", StringComparison.OrdinalIgnoreCase) &&
                    !log.Contains("library", StringComparison.OrdinalIgnoreCase) &&
                    !log.Contains("lib", StringComparison.OrdinalIgnoreCase))
                {
                    return new ChallengeResult(
                        false,
                        "The submodule has been added but not committed.",
                        "Commit the changes with: git add .gitmodules lib && git commit -m 'Add magic library as submodule'"
                    );
                }

                // Check 6: Verify the library content is accessible
                var magicSpellPath = Path.Combine(submodulePath, "magic-spell.js");
                if (!File.Exists(magicSpellPath))
                {
                    return new ChallengeResult(
                        false,
                        "The submodule was added but the library files aren't present.",
                        "You may need to run: git submodule update --init"
                    );
                }

                return new ChallengeResult(
                    true,
                    "The sanctum resonates with nested power! You've successfully mastered git submodules! " +
                    "Submodules allow you to include external repositories within your project, each tracking its own history. " +
                    "The .gitmodules file stores the configuration, mapping the submodule path to its repository URL. " +
                    "This is incredibly useful for managing dependencies, sharing common code across projects, or including " +
                    "third-party libraries. When others clone your repo, they use 'git submodule init' and 'git submodule update' " +
                    "to fetch the submodule contents. Think of submodules as 'repositories within repositories' - a powerful " +
                    "tool for code organization!",
                    null
                );
            }
        );

        var room = new Room(
            id: "room-21",
            name: "The Submodule Sanctum",
            description: "A chamber within a chamber, repositories nested in infinite recursion",
            narrative: "You enter a mystical sanctum where reality folds upon itself. Through archways, you see chambers within " +
                      "chambers, each maintaining its own independent existence yet connected to the whole. " +
                      "\n\nA keeper of nested realms speaks: 'Git submodules are like these chambers - complete repositories " +
                      "existing within your repository. They maintain their own history, commits, and branches, yet are referenced " +
                      "from your main project. This is how modern projects manage external dependencies and shared libraries.'" +
                      "\n\nBefore you lies a powerful magic library that many projects use. Instead of copying its code (which " +
                      "becomes outdated), you'll link to it as a submodule. The library lives in its own repository with its own " +
                      "commits, but your project will track a specific version of it." +
                      "\n\n[yellow]═══ Command Guide ═══[/]" +
                      "\n[cyan]git submodule add <url> <path>[/] - Add a repository as a submodule" +
                      "\n  • <url> can be a remote URL or local path" +
                      "\n  • <path> is where to place the submodule (e.g., 'lib', 'vendor', 'external')" +
                      "\n  • Creates .gitmodules file with configuration" +
                      "\n  • Clones the repository into the specified path" +
                      "\n  • Automatically stages .gitmodules and the submodule directory" +
                      "\n\n[cyan]git submodule init[/] - Initialize submodules after cloning a repo" +
                      "\n  • Registers submodules in .git/config" +
                      "\n  • Required before 'git submodule update'" +
                      "\n\n[cyan]git submodule update[/] - Check out the correct submodule commits" +
                      "\n  • Fetches and checks out the commit specified in the parent repo" +
                      "\n  • Use after pulling changes that update submodule references" +
                      "\n  • Common: [cyan]git submodule update --init --recursive[/]" +
                      "\n\n[cyan]git submodule status[/] - Show status of all submodules" +
                      "\n  • Shows current commit for each submodule" +
                      "\n  • Indicates if submodules are out of sync" +
                      "\n\n[cyan].gitmodules file[/] - Stores submodule configuration" +
                      "\n  • Contains submodule path and URL" +
                      "\n  • Committed to the repository (shared with team)" +
                      "\n  • Format: INI-style configuration" +
                      "\n\n[cyan]Common workflow with submodules:[/]" +
                      "\n  1. [cyan]git submodule add <url> <path>[/] - Add a submodule" +
                      "\n  2. [cyan]git commit -m 'Add submodule'[/] - Commit the addition" +
                      "\n  3. When cloning a repo with submodules:" +
                      "\n     [cyan]git clone --recurse-submodules <repo-url>[/]" +
                      "\n     Or after cloning:" +
                      "\n     [cyan]git submodule update --init --recursive[/]" +
                      "\n  4. To update a submodule to latest:" +
                      "\n     [cyan]cd <submodule-path>[/]" +
                      "\n     [cyan]git pull origin main[/]" +
                      "\n     [cyan]cd ..[/]" +
                      "\n     [cyan]git add <submodule-path>[/]" +
                      "\n     [cyan]git commit -m 'Update submodule to latest'[/]" +
                      "\n\n[cyan]When to use submodules:[/]" +
                      "\n  • Including external libraries maintained separately" +
                      "\n  • Sharing common code across multiple projects" +
                      "\n  • Vendoring dependencies (locking to specific versions)" +
                      "\n  • Working with monorepo-style architectures" +
                      "\n\n[cyan]Important notes:[/]" +
                      "\n  • Submodules track a specific commit, not a branch" +
                      "\n  • Your project stores the submodule's commit hash" +
                      "\n  • Team members must run 'git submodule update' after pulling" +
                      "\n  • Changes inside submodules require separate commits" +
                      "\n\n[green]Pro tips:[/]" +
                      "\n  • Use --recurse-submodules when cloning repos with submodules" +
                      "\n  • Set git config submodule.recurse true for automatic updates" +
                      "\n  • Consider alternatives for dependencies: package managers, git subtree" +
                      "\n  • Document submodule setup in your README" +
                      "\n\n[dim]Submodules: repositories within repositories, history within history![/]" +
                      "\n\n[yellow]The Challenge:[/]" +
                      "\n  • Add the magic-library as a submodule in a directory called 'lib'" +
                      "\n  • The library path is provided in SUBMODULE_INSTRUCTIONS.txt" +
                      "\n  • Commit the submodule addition to your repository" +
                      "\n\n[yellow]Steps to Complete:[/]" +
                      "\n  1. Read the instructions: [cyan]cat SUBMODULE_INSTRUCTIONS.txt[/]" +
                      "\n     (On Windows: [cyan]type SUBMODULE_INSTRUCTIONS.txt[/])" +
                      "\n  2. Add the submodule (use exact command from SUBMODULE_INSTRUCTIONS.txt):" +
                      "\n     [cyan]git -c protocol.file.allow=always submodule add <path> lib[/]" +
                      "\n     (The -c flag is needed for local file:// protocol)" +
                      "\n  3. Check what changed: [cyan]git status[/]" +
                      "\n     You'll see .gitmodules and lib/ were added" +
                      "\n  4. Examine .gitmodules: [cyan]cat .gitmodules[/]" +
                      "\n     (On Windows: [cyan]type .gitmodules[/])" +
                      "\n  5. Stage the changes: [cyan]git add .gitmodules lib[/]" +
                      "\n  6. Commit: [cyan]git commit -m \"Add magic library as submodule\"[/]" +
                      "\n  7. Explore the lib directory: [cyan]ls lib/[/]" +
                      "\n     (On Windows: [cyan]dir lib[/])",
            challenge: challenge,
            exits: new Dictionary<string, string> { { "forward", "room-22" } },
            isStartRoom: false,
            isEndRoom: false
        );

        return Task.FromResult(room);
    }
}
