using LibGit2Sharp;
using ShutUpHusky.Utils;

namespace ShutUpHusky.Heuristics;

internal class SubjectHeuristic : IHeuristic {
    public ICollection<HeuristicResult> Analyse(IRepository repo) {
        var stagedFiles = repo
            .RetrieveStatus(new StatusOptions())
            .Where(file =>
                file.State == FileStatus.ModifiedInIndex ||
                file.State == FileStatus.NewInIndex ||
                file.State == FileStatus.RenamedInIndex ||
                file.State == FileStatus.DeletedFromIndex
            )
            .ToList();

        if (stagedFiles.Count == 0)
            return new HeuristicResult[] {
                new() {
                    Priority = Constants.NotAPriority,
                    Value = string.Empty,
                },
            };

        var fileTerms = stagedFiles.ToFileTerms();

        var fileTermsByCount = fileTerms
            .GroupBy(t => t)
            .ToDictionary(t => t.First(), t => t.Count());

        var mostCommonTerm = fileTermsByCount
            .OrderByDescending(k => k.Value)
            .First();

        if (mostCommonTerm.Value == 1)
            return new HeuristicResult[] {
                new() {
                    Priority = Constants.NotAPriority,
                    Value = string.Empty,
                },
            };

        return new HeuristicResult[] {
            new() {
                Priority = Constants.SubjectPriority,
                Value = mostCommonTerm.Key,
                After = " > ",
            },
        };
    }
}
