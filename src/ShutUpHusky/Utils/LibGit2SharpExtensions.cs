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

    /* RepositoryStatus extensions */

    public static IEnumerable<StatusEntry> GetFiles(this RepositoryStatus status, params FileStatus[] statuses) =>
        status
            .Where(file => statuses.Any(status => (file.State & status) == status));

    /* IEnumerable<StatusEntry> Extensions */

    public static IEnumerable<StatusEntry> WithFileExtensions(this IEnumerable<StatusEntry> entries, params string[] extensions) =>
        entries.Where(entry => extensions.Contains(entry.GetFileExtension().ToLowerInvariant()));

    public static IEnumerable<string> ToFileTerms(this IEnumerable<StatusEntry> entries) =>
        entries
            .Select(entry => entry.GetFileName().CamelCaseToKebabCase())
            .SelectMany(n => n.Split("-"));

    public static IEnumerable<string> WithFileTerms(this IEnumerable<StatusEntry> entries, params string[] terms) =>
        entries
            .ToFileTerms()
            .Where(term => terms.Contains(term));
}
