using LibGit2Sharp;
using ShutUpHusky.Analysis.Summarizers;
using ShutUpHusky.Files;

namespace ShutUpHusky.Heuristics.FileHeuristics;

internal class CreationHeuristic : IFileHeuristic {
    public FileHeuristicOptions? Options { private get; init; }

    public HeuristicResult[] Analyse(IRepository repo) {
        var createdFiles = repo.GetCreatedFiles();

        return createdFiles
            .Select(file => {
                var patch = file.ToPatch(repo);
                var fileChangeSnippet = file.ToCommitMessageSnippet(FileChangeType.Created);
                var includeDiffSummary = Options?.IncludeDiffSummary is true;
                var summarizer = includeDiffSummary ? SummarizerFactory.Create(file) : null;
                var summary = summarizer?.Summarize(patch);

                if (summary is null)
                    return new HeuristicResult {
                        Priority = 1 + patch.LinesAdded,
                        Value = fileChangeSnippet,
                    };

                var prefixedSummary = HeuristicResult.WithAllPrefixed(summary, $"{fileChangeSnippet} - ");

                return HeuristicResult.WithLowerPriorityResult(
                    prefixedSummary,
                    new() {
                        Priority = 1 + patch.LinesAdded,
                        Value = fileChangeSnippet,
                    }
                );
            })
            .OrderByDescending(_ => _.Priority)
            .ToArray();
    }
}
