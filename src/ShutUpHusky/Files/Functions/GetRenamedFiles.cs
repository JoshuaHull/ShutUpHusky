using LibGit2Sharp;

namespace ShutUpHusky.Files;

internal static class GetRenamedFilesFunctions {
    public static IEnumerable<StatusEntry> GetRenamedFiles(this IRepository repo) =>
        repo
            .RetrieveStatus(new StatusOptions())
            .GetFiles(FileStatus.RenamedInIndex);
}
