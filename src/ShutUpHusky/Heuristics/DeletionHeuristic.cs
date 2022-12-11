using LibGit2Sharp;
using ShutUpHusky.Utils;

namespace ShutUpHusky.Heuristics;

internal class DeletionHeuristic : IHeuristic {
    public ICollection<HeuristicResult> Analyse(IRepository repo) {
        var deletedFiles = repo.GetDeletedFiles();
        var statusEntriesByPatch = deletedFiles.MapPatchToStatusEntry(repo);
        var patchesOrderedByDiff = statusEntriesByPatch
            .Keys
            .OrderByDescending(patch => patch.LinesDeleted)
            .ToList();
        var patchCount = patchesOrderedByDiff.Count;

        if (patchCount == 0)
            return Array.Empty<HeuristicResult>();

        var rtn = new HeuristicResult[patchCount];

        for (var i = 0; i < patchCount; i += 1) {
            var currentFile = patchesOrderedByDiff[i];
            var commitMessageSnippet = statusEntriesByPatch[currentFile].ToDeletedCommitMessageSnippet();
            var priority = i.ToPriority(Constants.LowPriority, Constants.HighPriorty, patchCount);

            rtn[i] = new() {
                Priority = priority,
                Value = commitMessageSnippet,
                After = ", ",
            };
        }

        return rtn;
    }
}
