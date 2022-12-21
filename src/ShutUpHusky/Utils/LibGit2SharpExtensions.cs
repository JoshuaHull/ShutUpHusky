using LibGit2Sharp;
using ShutUpHusky.Files;

namespace ShutUpHusky.Utils;

internal static class LibGit2SharpExtensions {
    public static Patch ToPatch(this StatusEntry entry, IRepository repo) =>
        repo.Diff.Compare<Patch>(repo.Head.Tip.Tree, DiffTargets.Index, new List<string> {
            entry.FilePath,
        });

    /* IRepository extensions */

    public static IEnumerable<StatusEntry> GetAllAlteredFiles(this IRepository repo) =>
        repo
            .RetrieveStatus(new StatusOptions())
            .GetFiles(FileStatus.ModifiedInIndex, FileStatus.NewInIndex, FileStatus.DeletedFromIndex, FileStatus.RenamedInIndex);

    public static IEnumerable<StatusEntry> GetCreatedFiles(this IRepository repo) =>
        repo
            .RetrieveStatus(new StatusOptions())
            .GetFiles(FileStatus.NewInIndex);

    public static IEnumerable<StatusEntry> GetModifiedFiles(this IRepository repo) =>
        repo
            .RetrieveStatus(new StatusOptions())
            .GetFiles(FileStatus.ModifiedInIndex);

    public static IEnumerable<StatusEntry> GetDeletedFiles(this IRepository repo) =>
        repo
            .RetrieveStatus(new StatusOptions())
            .GetFiles(FileStatus.DeletedFromIndex);

    public static IEnumerable<StatusEntry> GetRenamedFiles(this IRepository repo) =>
        repo
            .RetrieveStatus(new StatusOptions())
            .GetFiles(FileStatus.RenamedInIndex);
}
