using LibGit2Sharp;

namespace ShutUpHusky.Files;

internal static class GetModifiedFilesFunctions {
    public static IEnumerable<StatusEntry> GetModifiedFiles(this IRepository repo) =>
        repo
            .RetrieveStatus(new StatusOptions())
            .GetFiles(FileStatus.ModifiedInIndex);
}
