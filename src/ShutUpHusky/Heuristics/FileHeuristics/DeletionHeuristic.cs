using LibGit2Sharp;
using ShutUpHusky.Files;
using ShutUpHusky.Utils;

namespace ShutUpHusky.Heuristics.FileHeuristics;

internal class DeletionHeuristic : IFileHeuristic {
    public HeuristicResult[] Analyse(IRepository repo) {
        var deletedFiles = repo.GetDeletedFiles();

        return deletedFiles
            .Select(file => new HeuristicResult {
                Priority = Constants.LowPriority + file.ToPatch(repo).LinesDeleted,
                Value = file.ToCommitMessageSnippet(FileChangeType.Deleted),
            })
            .OrderByDescending(_ => _.Priority)
            .ToArray();
    }
}
