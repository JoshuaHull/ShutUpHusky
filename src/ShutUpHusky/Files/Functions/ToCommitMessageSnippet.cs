using LibGit2Sharp;

namespace ShutUpHusky.Files;

internal static class ToCommitMessageSnippetFunctions {
    private static readonly string[] TestExtensions =  new[] { "spec", "test", "specs", "tests" };

    public static string ToCommitMessageSnippet(this StatusEntry statusEntry, FileChangeType type) =>
        type switch {
            FileChangeType.Created => statusEntry.ToCommitMessageSnippet("created"),
            FileChangeType.Deleted => statusEntry.ToCommitMessageSnippet("deleted"),
            FileChangeType.Renamed => statusEntry.ToCommitMessageSnippet("renamed"),
            FileChangeType.Moved => statusEntry.ToCommitMessageSnippet("moved"),
            _ => statusEntry.ToCommitMessageSnippet("updated"),
        };

    private static string ToCommitMessageSnippet(this StatusEntry statusEntry, string prefix) {
        var fileName = statusEntry.FilePath.GetFileName().CamelCaseToKebabCase();

        foreach (var testExtension in TestExtensions) {
            var dashExtension = $"-{testExtension}";

            if (!fileName.EndsWith(dashExtension))
                continue;

            var actualFileName = fileName.Substring(0, fileName.Length - dashExtension.Length);

            return $"{prefix} {actualFileName} tests";
        }

        return $"{prefix} {fileName}";
    }
}
