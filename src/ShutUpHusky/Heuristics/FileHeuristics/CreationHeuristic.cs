using LibGit2Sharp;
using ShutUpHusky.Files;
using ShutUpHusky.Utils;

namespace ShutUpHusky.Heuristics.FileHeuristics;

internal class CreationHeuristic : IHeuristic {
    public ICollection<HeuristicResult> Analyse(IRepository repo) {
        var createdFiles = repo.GetCreatedFiles();
        var statusEntriesByPatch = createdFiles.MapPatchToStatusEntry(repo);
        var patchesOrderedByDiff = statusEntriesByPatch
            .Keys
            .OrderByDescending(patch => patch.LinesAdded)
            .ToList();
        var patchCount = patchesOrderedByDiff.Count;

        if (patchCount == 0)
            return Array.Empty<HeuristicResult>();

        var rtn = new HeuristicResult[patchCount];

        for (var i = 0; i < patchCount; i += 1) {
            var currentFile = patchesOrderedByDiff[i];
            var commitMessageSnippet = statusEntriesByPatch[currentFile].ToCommitMessageSnippet(FileChangeType.Created);
            var priority = i.ToPriority(Constants.LowPriority, Constants.HigherPriorty, patchCount);

            rtn[i] = new() {
                Priority = priority,
                Value = commitMessageSnippet,
                After = ", ",
            };
        }

        return rtn;
    }
}
