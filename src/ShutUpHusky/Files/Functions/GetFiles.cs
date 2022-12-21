using LibGit2Sharp;

namespace ShutUpHusky.Files;

internal static class GetFilesFunctions {

    public static IEnumerable<StatusEntry> GetFiles(this RepositoryStatus status, params FileStatus[] statuses) =>
        status
            .Where(file => statuses.Any(status => (file.State & status) == status));
}
