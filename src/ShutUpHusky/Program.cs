using System.Diagnostics;
using LibGit2Sharp;
using ShutUpHusky;

if (args.Length < 1) {
    Console.WriteLine(@"Expected usage: dotnet run ""C:\Users\User\SomeRepo""");
    return;
}

var repo = new Repository(args[0]);

var commitMessage = new CommitMessageAssembler().Assemble(repo);

Console.WriteLine(commitMessage);

var info = new ProcessStartInfo {
    FileName = "cmd.exe",
    Arguments = @$"/K git commit -m ""{commitMessage}"" && exit",
    CreateNoWindow = false,
    UseShellExecute = false,
    WorkingDirectory = args[0],
};

var process = Process.Start(info);

process?.WaitForExit();
