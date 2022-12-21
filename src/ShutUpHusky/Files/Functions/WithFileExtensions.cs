using LibGit2Sharp;

namespace ShutUpHusky.Files;

internal static class WithFileExtensionsFunctions {

    public static IEnumerable<StatusEntry> WithFileExtensions(this IEnumerable<StatusEntry> entries, params string[] extensions) =>
        entries
            .Where(entry => extensions.Contains(entry.GetFileExtension().ToLowerInvariant()));
}
