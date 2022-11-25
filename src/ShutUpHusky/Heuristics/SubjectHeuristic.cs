using LibGit2Sharp;
using ShutUpHusky.Utils;

namespace ShutUpHusky.Heuristics;

public class SubjectHeuristic : IHeuristic {
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
                    Priority = 0,
                    Value = string.Empty,
                },
            };

        var fileTerms = stagedFiles
            .Select(f => f.FilePath.GetFileName().CamelCaseToKebabCase())
            .SelectMany(n => n.Split("-"));

        var fileTermsByCount = fileTerms
            .GroupBy(t => t)
            .ToDictionary(t => t.First(), t => t.Count());

        var mostCommonTerm = fileTermsByCount
            .OrderByDescending(k => k.Value)
            .First();

        if (mostCommonTerm.Value == 1)
            return new HeuristicResult[] {
                new() {
                    Priority = 0,
                    Value = string.Empty,
                },
            };

        return new HeuristicResult[] {
            new() {
                Priority = 1,
                Value = mostCommonTerm.Key,
                After = " > ",
            },
        };
    }
}
