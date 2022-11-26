using LibGit2Sharp;
using ShutUpHusky.Utils;

namespace ShutUpHusky.Heuristics;

internal class CreationHeuristic : IHeuristic {
    public ICollection<HeuristicResult> Analyse(IRepository repo) {
        var createdFiles = repo.GetCreatedFiles();

        if (createdFiles.Count == 0)
            return new HeuristicResult[] {
                new() {
                    Priority = Constants.NotAPriority,
                    Value = string.Empty,
                },
            };

        var statusEntriesByPatch = createdFiles.MapPatchToStatusEntry(repo);

        var largestCreatedFile = statusEntriesByPatch
            .Keys
            .OrderByDescending(patch => patch.LinesAdded)
            .First();

        if (largestCreatedFile.LinesAdded == 0)
            return new HeuristicResult[] {
                new() {
                    Priority = Constants.NotAPriority,
                    Value = string.Empty,
                },
            };

        var commitMessageSnippet = statusEntriesByPatch[largestCreatedFile].ToCreatedCommitMessageSnippet();

        return new HeuristicResult[] {
            new() {
                Priority = Constants.HighPriorty,
                Value = commitMessageSnippet,
                After = ", ",
            },
        };
    }
}
