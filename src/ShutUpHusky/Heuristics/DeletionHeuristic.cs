using LibGit2Sharp;
using ShutUpHusky.Utils;

namespace ShutUpHusky.Heuristics;

public class DeletionHeuristic : IHeuristic {
    public HeuristicResult Analyse(IRepository repo) {
        var status = repo.RetrieveStatus(new StatusOptions());

        var modifiedFiles = status
            .Where(file => file.State == FileStatus.DeletedFromIndex)
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

        var largestDeletedFile = patches
            .Keys
            .OrderByDescending(patch => patch.LinesDeleted)
            .First();

        if (largestDeletedFile.LinesDeleted == 0)
            return new() {
                Priority = 0,
                Value = string.Empty,
            };

        var fileName = patches[largestDeletedFile].FilePath.GetFileName().CamelCaseToKebabCase();

        return new() {
            Priority = 1,
            Value = $"deleted {fileName}",
            After = ", ",
        };
    }
}
