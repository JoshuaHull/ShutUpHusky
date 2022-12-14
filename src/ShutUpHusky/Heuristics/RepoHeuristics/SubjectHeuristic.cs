using LibGit2Sharp;
using ShutUpHusky.Files;

namespace ShutUpHusky.Heuristics.RepoHeuristics;

internal class SubjectHeuristic : IRepoHeuristic {
    private static readonly string[] ExcludedTerms = new[] {
        "test",
        "tests",
        "spec",
        "specs",
    };

    public HeuristicResult? Analyse(IRepository repo) {
        var stagedFiles = repo.GetAllAlteredFiles().ToList();

        var fileTerms = stagedFiles
            .ToFileTerms()
            .Where(term => !ExcludedTerms.Contains(term));

        var fileTermsByCount = fileTerms
            .GroupBy(t => t)
            .ToDictionary(t => t.First(), t => t.Count());

        var mostCommonTerm = fileTermsByCount
            .OrderByDescending(k => k.Value)
            .FirstOrDefault();

        if (mostCommonTerm.Value <= 1)
            return null;

        return new() {
            Value = mostCommonTerm.Key,
        };
    }
}
