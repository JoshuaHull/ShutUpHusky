using LibGit2Sharp;
using ShutUpHusky.Utils;

namespace ShutUpHusky.Heuristics;

internal class RenamingHeuristic : IHeuristic {
    public ICollection<HeuristicResult> Analyse(IRepository repo) {
        var renamedFiles = repo.GetRenamedFiles();
        var movedFiles = renamedFiles
            .Where(file => file.HeadToIndexRenameDetails is not null)
            .Where(file => file.HeadToIndexRenameDetails.NewFilePath.GetFileName() == file.HeadToIndexRenameDetails.OldFilePath.GetFileName())
            .ToList();
        var actuallyRenamedFiles = renamedFiles.Except(movedFiles);

        return GetRenamedFilesResults(repo, actuallyRenamedFiles)
            .Union(GetMovedFilesResults(repo, movedFiles))
            .OrderByDescending(r => r.Priority)
            .ToArray();
    }

    private IEnumerable<HeuristicResult> GetMovedFilesResults(IRepository repo, List<StatusEntry> movedFiles) {
        var statusEntriesByPatch = movedFiles.MapPatchToStatusEntry(repo);
        var patchesOrderedByDiff = statusEntriesByPatch
            .Keys
            .OrderByDescending(patch => patch.LinesAdded)
            .ToList();
        var patchCount = patchesOrderedByDiff.Count;

        if (patchCount == 0)
            yield break;

        for (var i = 0; i < patchCount; i += 1) {
            var currentFile = patchesOrderedByDiff[i];
            var commitMessageSnippet = statusEntriesByPatch[currentFile].ToMovedCommitMessageSnippet();
            var priority = i.ToPriority(Constants.LowPriorty, Constants.HighPriorty, patchCount);

            yield return new() {
                Priority = priority,
                Value = commitMessageSnippet,
                After = ", ",
            };
        }
    }

    private IEnumerable<HeuristicResult> GetRenamedFilesResults(IRepository repo, IEnumerable<StatusEntry> renamedFiles) {
        var statusEntriesByPatch = renamedFiles.MapPatchToStatusEntry(repo);
        var patchesOrderedByDiff = statusEntriesByPatch
            .Keys
            .Where(patch => patch.LinesDeleted == 0)
            .OrderByDescending(patch => patch.LinesAdded)
            .ToList();
        var patchCount = patchesOrderedByDiff.Count;

        if (patchCount == 0)
            yield break;

        for (var i = 0; i < patchCount; i += 1) {
            var currentFile = patchesOrderedByDiff[i];
            var commitMessageSnippet = statusEntriesByPatch[currentFile].ToRenamedCommitMessageSnippet();
            var priority = i.ToPriority(Constants.LowPriorty, Constants.MediumPriorty, patchCount);

            yield return new() {
                Priority = priority,
                Value = commitMessageSnippet,
                After = ", ",
            };
        }
    }
}
