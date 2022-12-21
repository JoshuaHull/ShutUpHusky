using LibGit2Sharp;
using ShutUpHusky.Files;

namespace ShutUpHusky.Heuristics.FileHeuristics;

internal class RenamingHeuristic : IFileHeuristic {
    public FileHeuristicOptions? Options { private get; init; }

    public HeuristicResult[] Analyse(IRepository repo) {
        var renamedFiles = repo.GetRenamedFiles();

        return renamedFiles
            .Select(file => {
                var patch = file.ToPatch(repo);
                var renameDetails = file.HeadToIndexRenameDetails;
                var isMovedFile = renameDetails is not null &&
                    renameDetails.OldFilePath.GetFileName() == renameDetails.NewFilePath.GetFileName();

                if (patch.LinesDeleted != 0 && !isMovedFile)
                    return null;

                return new HeuristicResult {
                    Priority = 1 + patch.LinesAdded,
                    Value = file.ToCommitMessageSnippet(isMovedFile ? FileChangeType.Moved : FileChangeType.Renamed),
                };
            })
            .Where(_ => _ is not null)
            .Cast<HeuristicResult>()
            .OrderByDescending(_ => _.Priority)
            .ToArray();
    }
}
