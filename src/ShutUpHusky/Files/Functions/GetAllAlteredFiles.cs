using LibGit2Sharp;

namespace ShutUpHusky.Files;

internal static class GetAllAlteredFilesFunctions {
    public static IEnumerable<StatusEntry> GetAllAlteredFiles(this IRepository repo) =>
        repo
            .RetrieveStatus(new StatusOptions())
            .GetFiles(FileStatus.ModifiedInIndex, FileStatus.NewInIndex, FileStatus.DeletedFromIndex, FileStatus.RenamedInIndex);
}
