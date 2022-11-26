using LibGit2Sharp;
using ShutUpHusky.Utils;

namespace ShutUpHusky.Heuristics;

internal class RenamingHeuristic : IHeuristic {
    public ICollection<HeuristicResult> Analyse(IRepository repo) {
        var renamedFiles = repo.GetRenamedFiles();
        var statusEntriesByPatch = renamedFiles.MapPatchToStatusEntry(repo);
        var patchesOrderedByDiff = statusEntriesByPatch
            .Keys
            .Where(patch => patch.LinesDeleted == 0)
            .OrderByDescending(patch => patch.LinesAdded)
            .ToList();
        var patchCount = patchesOrderedByDiff.Count;

        if (patchCount == 0)
            return new HeuristicResult[] {
                new() {
                    Priority = Constants.NotAPriority,
                    Value = string.Empty,
                },
            };

        var rtn = new HeuristicResult[patchCount];

        for (var i = 0; i < patchCount; i += 1) {
            var currentFile = patchesOrderedByDiff[i];
            var commitMessageSnippet = statusEntriesByPatch[currentFile].ToRenamedCommitMessageSnippet();
            var priority = i.ToPriority(Constants.NotAPriority, Constants.LowPriorty, patchCount);

            rtn[i] = new() {
                Priority = priority,
                Value = commitMessageSnippet,
                After = ", ",
            };
        }

        return rtn;
    }
}
