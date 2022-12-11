using LibGit2Sharp;
using ShutUpHusky.Utils;

namespace ShutUpHusky.Heuristics;

internal class SubjectHeuristic : IHeuristic {
    private static readonly string[] ExcludedTerms = new[] {
        "test",
        "tests",
        "spec",
        "specs",
    };

    public ICollection<HeuristicResult> Analyse(IRepository repo) {
        var stagedFiles = repo.GetAllAlteredFiles().ToList();

        if (stagedFiles.Count == 0)
            return Array.Empty<HeuristicResult>();

        var fileTerms = stagedFiles
            .ToFileTerms()
            .Where(term => !ExcludedTerms.Contains(term));

        var fileTermsByCount = fileTerms
            .GroupBy(t => t)
            .ToDictionary(t => t.First(), t => t.Count());

        var mostCommonTerm = fileTermsByCount
            .OrderByDescending(k => k.Value)
            .First();

        if (mostCommonTerm.Value == 1)
            return Array.Empty<HeuristicResult>();

        return new HeuristicResult[] {
            new() {
                Priority = Constants.SubjectPriority,
                Value = mostCommonTerm.Key,
                After = " > ",
            },
        };
    }
}
