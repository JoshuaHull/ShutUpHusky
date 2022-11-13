using LibGit2Sharp;
using ShutUpHusky;

if (args.Length < 3) {
    Console.WriteLine(@"Expected usage: dotnet run ""Author Name"" ""author@email.com"" ""C:\Users\Author\SomeRepo""");
    return;
}

var repo = new Repository(args[2]);
var me = new Signature(args[0], args[1], DateTimeOffset.UtcNow);

var commitMessage = new CommitMessageAssembler().Assemble(repo);

Console.WriteLine(commitMessage);

repo.Commit(commitMessage, me, me);
