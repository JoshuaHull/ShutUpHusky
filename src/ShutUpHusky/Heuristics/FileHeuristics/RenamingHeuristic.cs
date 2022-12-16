using LibGit2Sharp;
using ShutUpHusky.Files;
using ShutUpHusky.Utils;

namespace ShutUpHusky.Heuristics.FileHeuristics;

internal class RenamingHeuristic : IFileHeuristic {
    public HeuristicResult[] Analyse(IRepository repo) {
        var renamedFiles = repo.GetRenamedFiles();

        return renamedFiles
            .Select(file => {
                var patch = file.ToPatch(repo);
                var renameDetails = file.HeadToIndexRenameDetails;
                var isMovedFile = renameDetails is not null &&
                    renameDetails.OldFilePath.GetFileName() == renameDetails.NewFilePath.GetFileName();

                if (patch.LinesDeleted != 0 && !isMovedFile)
                    return HeuristicResult.Default;

                return new HeuristicResult {
                    Priority = Constants.LowPriority + patch.LinesAdded,
                    Value = file.ToCommitMessageSnippet(isMovedFile ? FileChangeType.Moved : FileChangeType.Renamed),
                };
            })
            .Where(_ => _.Priority > Constants.NotAPriority)
            .OrderByDescending(_ => _.Priority)
            .ToArray();
    }
}
