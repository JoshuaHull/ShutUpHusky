using LibGit2Sharp;
using ShutUpHusky.Files;
using ShutUpHusky.Utils;

namespace ShutUpHusky.Heuristics.FileHeuristics;

internal class CreationHeuristic : IFileHeuristic {
    public ICollection<HeuristicResult> Analyse(IRepository repo) {
        var createdFiles = repo.GetCreatedFiles();

        return createdFiles
            .Select(file => new HeuristicResult {
                Priority = Constants.LowPriority + file.ToPatch(repo).LinesAdded,
                Value = file.ToCommitMessageSnippet(FileChangeType.Created),
            })
            .OrderByDescending(_ => _.Priority)
            .ToArray();
    }
}
