using LibGit2Sharp;
using ShutUpHusky.Utils;

namespace ShutUpHusky.Heuristics;

public class ModificationHeuristic : IHeuristic {
    public ICollection<HeuristicResult> Analyse(IRepository repo) {
        var status = repo.RetrieveStatus(new StatusOptions());

        var modifiedFiles = status
            .Where(file => file.State == FileStatus.ModifiedInIndex)
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

        var mostChangedFile = patches
            .Keys
            .OrderByDescending(patch => patch.LinesAdded + patch.LinesDeleted)
            .First();

        if (mostChangedFile.LinesAdded + mostChangedFile.LinesDeleted == 0)
            return new HeuristicResult[] {
                new() {
                    Priority = Constants.NotAPriority,
                    Value = string.Empty,
                },
            };

        var fileName = patches[mostChangedFile].FilePath.GetFileName().CamelCaseToKebabCase();

        return new HeuristicResult[] {
            new() {
                Priority = Constants.LowPriorty,
                Value = $"updated {fileName}",
                After = ", ",
            },
        };
    }
}
