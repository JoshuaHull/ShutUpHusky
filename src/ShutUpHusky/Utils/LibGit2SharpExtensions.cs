using LibGit2Sharp;

namespace ShutUpHusky.Utils;

internal static class LibGit2SharpExtensions {
    public static Dictionary<Patch, StatusEntry> MapPatchToStatusEntry(
        this IEnumerable<StatusEntry> statusEntries, IRepository repo
    ) => statusEntries.ToDictionary(entry => repo.Diff.Compare<Patch>(repo.Head.Tip.Tree, DiffTargets.Index, new List<string> {
        entry.FilePath,
    }));

    public static List<Patch> ToOrderedPatches(
        this Dictionary<Patch, StatusEntry> entriesByPatch, Func<Patch, int> eval
    ) => entriesByPatch
        .Keys
        .Where(patch => eval(patch) != 0)
        .OrderByDescending(eval)
        .ToList();

    /* IRepository extensions */

    public static List<StatusEntry> GetAllAlteredFiles(this IRepository repo) =>
        repo
            .RetrieveStatus(new StatusOptions())
            .GetAllAlteredFiles();

    public static List<StatusEntry> GetModifiedFiles(this IRepository repo) =>
        repo
            .RetrieveStatus(new StatusOptions())
            .GetModifiedFiles();

    public static List<StatusEntry> GetDeletedFiles(this IRepository repo) =>
        repo
            .RetrieveStatus(new StatusOptions())
            .GetDeletedFiles();

    /* RepositoryStatus extensions */

    public static List<StatusEntry> GetFiles(this RepositoryStatus status, params FileStatus[] statuses) =>
        status
            .Where(file => statuses.Contains(file.State))
            .ToList();

    public static List<StatusEntry> GetAllAlteredFiles(this RepositoryStatus status) =>
        status.GetFiles(FileStatus.ModifiedInIndex, FileStatus.NewInIndex, FileStatus.DeletedFromIndex, FileStatus.RenamedInIndex);

    public static List<StatusEntry> GetModifiedFiles(this RepositoryStatus status) =>
        status.GetFiles(FileStatus.ModifiedInIndex);

    public static List<StatusEntry> GetDeletedFiles(this RepositoryStatus status) =>
        status.GetFiles(FileStatus.DeletedFromIndex);

    /* List<StatusEntry> Extensions */

    public static List<StatusEntry> WithFileExtensions(this List<StatusEntry> entries, params string[] extensions) =>
        entries
            .Where(entry => extensions.Contains(entry.GetFileExtension().ToLowerInvariant()))
            .ToList();

    public static IEnumerable<string> ToFileTerms(this List<StatusEntry> entries) =>
        entries
            .Select(entry => entry.GetFileName().CamelCaseToKebabCase())
            .SelectMany(n => n.Split("-"));

    public static List<string> WithFileTerms(this List<StatusEntry> entries, params string[] terms) =>
        entries
            .ToFileTerms()
            .Where(term => terms.Contains(term))
            .ToList();

    public static List<StatusEntry> GetFiles(this List<StatusEntry> entries, params FileStatus[] statuses) =>
        entries
            .Where(entry => statuses.Contains(entry.State))
            .ToList();

    public static List<StatusEntry> GetDeletedFiles(this List<StatusEntry> entries) =>
        entries.GetFiles(FileStatus.DeletedFromIndex);

    /* StatusEntry Extensions */

    public static string GetFileExtension(this StatusEntry entry) =>
        entry.FilePath.GetFileExtension();

    public static string GetFileName(this StatusEntry entry) =>
        entry.FilePath.GetFileName();

    public static string ToUpdatedCommitMessageSnippet(this StatusEntry statusEntry) =>
        $"updated {statusEntry.FilePath.GetFileName().CamelCaseToKebabCase()}";
}
