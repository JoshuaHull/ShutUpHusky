using System.CommandLine;
using System.Diagnostics;
using LibGit2Sharp;
using ShutUpHusky;

var noWindowOption = new Option<bool>("--no-window", "Whether or not a console window should be launched") {
    Arity = ArgumentArity.ZeroOrOne,
};

var repoOption = new Option<string>("--repo", "The location of the repo to commit to") {
    Arity = ArgumentArity.ExactlyOne,
};

var command = new RootCommand {
    noWindowOption,
    repoOption,
};

command.AddValidator(result => {
    if (result.GetValueForOption(repoOption) is null)
        result.ErrorMessage = "--repo is required";
});

command.SetHandler<bool, string>((noWindow, repoLocation) => {
    var repo = new Repository(repoLocation);

    var commitMessage = new CommitMessageAssembler().Assemble(repo);

    Console.WriteLine("ShutUpHusky! Committing with message:");
    Console.WriteLine(commitMessage);

    var info = new ProcessStartInfo {
        FileName = "cmd.exe",
        Arguments = @$"/K git commit -m ""{commitMessage}"" && exit",
        CreateNoWindow = noWindow,
        UseShellExecute = false,
        WorkingDirectory = repoLocation,
    };

    var process = Process.Start(info);

    process?.WaitForExit();
}, noWindowOption, repoOption);

await command.InvokeAsync(args);
