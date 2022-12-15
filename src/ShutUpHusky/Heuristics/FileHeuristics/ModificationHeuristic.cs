using LibGit2Sharp;
using ShutUpHusky.Files;
using ShutUpHusky.Utils;

namespace ShutUpHusky.Heuristics.FileHeuristics;

internal class ModificationHeuristic : IFileHeuristic {
    public ICollection<HeuristicResult> Analyse(IRepository repo) {
        var modifiedFiles = repo.GetModifiedFiles();

        return modifiedFiles
            .Select(file => {
                var patch = file.ToPatch(repo);

                return new HeuristicResult {
                    Priority = Constants.LowPriority + patch.LinesAdded + patch.LinesDeleted,
                    Value = file.ToCommitMessageSnippet(FileChangeType.Modified),
                };
            })
            .OrderByDescending(_ => _.Priority)
            .ToArray();
    }
}
