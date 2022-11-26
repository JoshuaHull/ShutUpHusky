using LibGit2Sharp;
using ShutUpHusky.Utils;

namespace ShutUpHusky.Heuristics;

internal class CreationHeuristic : IHeuristic {
    public ICollection<HeuristicResult> Analyse(IRepository repo) {
        var status = repo.RetrieveStatus(new StatusOptions());

        var modifiedFiles = status
            .Where(file => file.State == FileStatus.NewInIndex)
            .ToList();

        if (modifiedFiles.Count == 0)
            return new HeuristicResult[] {
                new() {
                    Priority = Constants.NotAPriority,
                    Value = string.Empty,
                },
            };

        var patches = modifiedFiles
            .ToDictionary(file => repo.Diff.Compare<Patch>(repo.Head.Tip.Tree, DiffTargets.Index, new List<string> {
                file.FilePath,
            }));

        var largestCreatedFile = patches
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

        var fileName = patches[largestCreatedFile].FilePath.GetFileName().CamelCaseToKebabCase();

        return new HeuristicResult[] {
            new() {
                Priority = Constants.HighPriorty,
                Value = $"created {fileName}",
                After = ", ",
            },
        };
    }
}
