using LibGit2Sharp;
using ShutUpHusky.Analysis.Summarizers;
using ShutUpHusky.Files;
using ShutUpHusky.Utils;

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
                        Priority = Constants.LowPriority + patch.LinesAdded,
                        Value = fileChangeSnippet,
                        After = ", ",
                    };

                var prefixedSummary = HeuristicResult.PrefixAll(summary, $"{fileChangeSnippet} - ");

                return HeuristicResult.AddShortened(
                    prefixedSummary,
                    new() {
                        Priority = Constants.LowPriority + patch.LinesAdded,
                        Value = fileChangeSnippet,
                        After = ", ",
                    }
                );
            })
            .OrderByDescending(_ => _.Priority)
            .ToArray();
    }
}
