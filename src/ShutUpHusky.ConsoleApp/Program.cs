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

var inProcessOption = new Option<bool>("--in-process", "Git commit directly, do not start a new process. Can not run hooks in this mode") {
    Arity = ArgumentArity.ZeroOrOne,
};

var nameOption = new Option<string>("--name", "Name of the author of the commit. Required if --in-process is true, ignored otherwise") {
    Arity = ArgumentArity.ZeroOrOne,
};

var emailOption = new Option<string>("--email", "Email of the author of the commit. Required if --in-process is true, ignored otherwise") {
    Arity = ArgumentArity.ZeroOrOne,
};

var command = new RootCommand {
    noWindowOption,
    repoOption,
    inProcessOption,
    nameOption,
    emailOption,
};

command.AddValidator(result => {
    if (result.GetValueForOption(repoOption) is null)
        result.ErrorMessage = "--repo is required";
});

command.AddValidator(result => {
    if(result.GetValueForOption(inProcessOption) && result.GetValueForOption(nameOption) is null)
        result.ErrorMessage = "--name is required whenever --in-process is true";
});

command.AddValidator(result => {
    if(result.GetValueForOption(inProcessOption) && result.GetValueForOption(emailOption) is null)
        result.ErrorMessage = "--email is required whenever --in-process is true";
});

command.SetHandler<bool, bool, string?, string?, string>((noWindow, inProcess, name, email, repoLocation) => {
    var repo = new Repository(repoLocation);

    var commitMessage = new CommitMessageAssembler().Assemble(repo);

    Console.WriteLine("ShutUpHusky! Committing with message:");
    Console.WriteLine(commitMessage);

    if (inProcess) {
        var author = new Signature(name, email, DateTimeOffset.Now);
        repo.Commit(commitMessage, author, author);
        return;
    }

    var info = new ProcessStartInfo {
        FileName = "cmd.exe",
        Arguments = @$"/K git commit -m ""{commitMessage}"" && exit",
        CreateNoWindow = noWindow,
        UseShellExecute = false,
        WorkingDirectory = repoLocation,
    };

    var process = Process.Start(info);

    process?.WaitForExit();
}, noWindowOption, inProcessOption, nameOption, emailOption, repoOption);

await command.InvokeAsync(args);
