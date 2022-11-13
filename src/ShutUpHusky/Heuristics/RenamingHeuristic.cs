using LibGit2Sharp;
using ShutUpHusky.Utils;

namespace ShutUpHusky.Heuristics;

public class RenamingHeuristic : IHeuristic {
    public HeuristicResult Analyse(IRepository repo) {
        var status = repo.RetrieveStatus(new StatusOptions());

        var modifiedFiles = status
            .Where(file => file.State == FileStatus.RenamedInIndex)
            .ToList();

        if (modifiedFiles.Count == 0)
            return new() {
                Priority = 0,
                Value = string.Empty,
            };

        var patches = modifiedFiles
            .ToDictionary(file => repo.Diff.Compare<Patch>(repo.Head.Tip.Tree, DiffTargets.Index, new List<string> {
                file.FilePath,
            }));

        var renamedFile = patches
            .Keys
            .First(patch => patch.LinesAdded == 0 && patch.LinesDeleted == 0);

        var fileName = patches[renamedFile].FilePath.GetFileName().CamelCaseToKebabCase();

        return new() {
            Priority = 1,
            Value = $"renamed {fileName}",
        };
    }
}
