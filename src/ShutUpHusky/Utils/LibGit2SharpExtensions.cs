using LibGit2Sharp;

namespace ShutUpHusky.Utils;

internal static class LibGit2SharpExtensions {
    public static Dictionary<Patch, StatusEntry> MapPatchToStatusEntry(
        this IEnumerable<StatusEntry> statusEntries, IRepository repo
    ) => statusEntries.ToDictionary(entry => repo.Diff.Compare<Patch>(repo.Head.Tip.Tree, DiffTargets.Index, new List<string> {
        entry.FilePath,
    }));

    /* IRepository extensions */

    public static IEnumerable<StatusEntry> GetAllAlteredFiles(this IRepository repo) =>
        repo
            .RetrieveStatus(new StatusOptions())
            .GetAllAlteredFiles();

    public static IEnumerable<StatusEntry> GetCreatedFiles(this IRepository repo) =>
        repo
            .RetrieveStatus(new StatusOptions())
            .GetCreatedFiles();

    public static IEnumerable<StatusEntry> GetModifiedFiles(this IRepository repo) =>
        repo
            .RetrieveStatus(new StatusOptions())
            .GetModifiedFiles();

    public static IEnumerable<StatusEntry> GetDeletedFiles(this IRepository repo) =>
        repo
            .RetrieveStatus(new StatusOptions())
            .GetDeletedFiles();

    public static IEnumerable<StatusEntry> GetRenamedFiles(this IRepository repo) =>
        repo
            .RetrieveStatus(new StatusOptions())
            .GetRenamedFiles();

    public static IEnumerable<StatusEntry> GetFiles(this IRepository repo, params FileStatus[] statuses) =>
        repo.RetrieveStatus(new StatusOptions())
            .GetFiles(statuses);

    /* RepositoryStatus extensions */

    public static IEnumerable<StatusEntry> GetFiles(this RepositoryStatus status, params FileStatus[] statuses) =>
        status
            .Where(file => statuses.Contains(file.State));

    public static IEnumerable<StatusEntry> GetAllAlteredFiles(this RepositoryStatus status) =>
        status.GetFiles(FileStatus.ModifiedInIndex, FileStatus.NewInIndex, FileStatus.DeletedFromIndex, FileStatus.RenamedInIndex);

    public static IEnumerable<StatusEntry> GetCreatedFiles(this RepositoryStatus status) =>
        status.GetFiles(FileStatus.NewInIndex);

    public static IEnumerable<StatusEntry> GetModifiedFiles(this RepositoryStatus status) =>
        status.GetFiles(FileStatus.ModifiedInIndex);

    public static IEnumerable<StatusEntry> GetDeletedFiles(this RepositoryStatus status) =>
        status.GetFiles(FileStatus.DeletedFromIndex);

    public static IEnumerable<StatusEntry> GetRenamedFiles(this RepositoryStatus status) =>
        status.GetFiles(FileStatus.RenamedInIndex);

    /* List<StatusEntry> Extensions */

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

    public static IEnumerable<StatusEntry> GetFiles(this IEnumerable<StatusEntry> entries, params FileStatus[] statuses) =>
        entries.Where(entry => statuses.Contains(entry.State));

    public static IEnumerable<StatusEntry> GetDeletedFiles(this IEnumerable<StatusEntry> entries) =>
        entries.GetFiles(FileStatus.DeletedFromIndex);

    /* StatusEntry Extensions */

    public static string GetFileExtension(this StatusEntry entry) =>
        entry.FilePath.GetFileExtension();

    public static string GetFileName(this StatusEntry entry) =>
        entry.FilePath.GetFileName();

    public static string ToCreatedCommitMessageSnippet(this StatusEntry statusEntry) =>
        $"created {statusEntry.FilePath.GetFileName().CamelCaseToKebabCase()}";

    public static string ToDeletedCommitMessageSnippet(this StatusEntry statusEntry) =>
        $"deleted {statusEntry.FilePath.GetFileName().CamelCaseToKebabCase()}";

    public static string ToRenamedCommitMessageSnippet(this StatusEntry statusEntry) =>
        $"renamed {statusEntry.FilePath.GetFileName().CamelCaseToKebabCase()}";

    public static string ToMovedCommitMessageSnippet(this StatusEntry statusEntry) =>
        $"moved {statusEntry.FilePath.GetFileName().CamelCaseToKebabCase()}";

    public static string ToUpdatedCommitMessageSnippet(this StatusEntry statusEntry) =>
        $"updated {statusEntry.FilePath.GetFileName().CamelCaseToKebabCase()}";
}
