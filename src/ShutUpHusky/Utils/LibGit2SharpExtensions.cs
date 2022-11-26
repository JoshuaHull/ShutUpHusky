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

    public static List<StatusEntry> GetModifiedFiles(this IRepository repo) =>
        repo
            .RetrieveStatus(new StatusOptions())
            .GetModifiedFiles();

    public static List<StatusEntry> GetModifiedFiles(this RepositoryStatus status) =>
        status
            .Where(file => file.State == FileStatus.ModifiedInIndex)
            .ToList();

    public static string ToUpdatedCommitMessageSnippet(this StatusEntry statusEntry) =>
        $"updated {statusEntry.FilePath.GetFileName().CamelCaseToKebabCase()}";
}
