using LibGit2Sharp;
using ShutUpHusky.Files;

namespace ShutUpHusky.Utils;

internal static class LibGit2SharpExtensions {
    public static IEnumerable<StatusEntry> GetDeletedFiles(this IRepository repo) =>
        repo
            .RetrieveStatus(new StatusOptions())
            .GetFiles(FileStatus.DeletedFromIndex);

    public static IEnumerable<StatusEntry> GetRenamedFiles(this IRepository repo) =>
        repo
            .RetrieveStatus(new StatusOptions())
            .GetFiles(FileStatus.RenamedInIndex);
}
