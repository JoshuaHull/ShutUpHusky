using LibGit2Sharp;

namespace ShutUpHusky.Files;

internal static class ToPatchFunctions {
    public static Patch ToPatch(this StatusEntry entry, IRepository repo) =>
        repo.Diff.Compare<Patch>(repo.Head.Tip.Tree, DiffTargets.Index, new List<string> {
            entry.FilePath,
        });
}
