using LibGit2Sharp;
using ShutUpHusky.Analysis.Summarizers;
using ShutUpHusky.Files;

namespace ShutUpHusky.Heuristics.FileHeuristics;

internal class ModificationHeuristic : IFileHeuristic {
    public FileHeuristicOptions? Options { private get; init; }

    public HeuristicResult[] Analyse(IRepository repo) {
        var modifiedFiles = repo.GetModifiedFiles();

        return modifiedFiles
            .Select(file => {
                var patch = file.ToPatch(repo);
                var fileChangeSnippet = file.ToCommitMessageSnippet(FileChangeType.Modified);
                var includeDiffSummary = Options?.IncludeDiffSummary is true;
                var summarizer = includeDiffSummary ? SummarizerFactory.Create(file) : null;
                var summary = summarizer?.Summarize(patch);

                if (summary is null)
                    return new HeuristicResult {
                        Priority = 1 + patch.LinesAdded + patch.LinesDeleted,
                        Value = fileChangeSnippet,
                    };

                var prefixedSummary = HeuristicResult.WithAllPrefixed(summary, $"{fileChangeSnippet} - ");

                return HeuristicResult.WithLowerPriorityResult(
                    prefixedSummary,
                    new() {
                        Priority = 1 + patch.LinesAdded + patch.LinesDeleted,
                        Value = fileChangeSnippet,
                    }
                );
            })
            .OrderByDescending(_ => _.Priority)
            .ToArray();
    }
}
