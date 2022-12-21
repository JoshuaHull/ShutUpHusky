using LibGit2Sharp;

namespace ShutUpHusky.Files;

internal static class GetDeletedFilesFunctions {
    public static IEnumerable<StatusEntry> GetDeletedFiles(this IRepository repo) =>
        repo
            .RetrieveStatus(new StatusOptions())
            .GetFiles(FileStatus.DeletedFromIndex);
}
