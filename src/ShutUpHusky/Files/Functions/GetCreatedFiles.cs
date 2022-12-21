using LibGit2Sharp;

namespace ShutUpHusky.Files;

internal static class GetCreatedFilesFunctions {
    public static IEnumerable<StatusEntry> GetCreatedFiles(this IRepository repo) =>
        repo
            .RetrieveStatus(new StatusOptions())
            .GetFiles(FileStatus.NewInIndex);
}
