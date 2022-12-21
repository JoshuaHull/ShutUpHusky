using LibGit2Sharp;

namespace ShutUpHusky.Files;

internal static class WithFileTermsFunctions {

    public static IEnumerable<string> WithFileTerms(this IEnumerable<StatusEntry> entries, params string[] terms) =>
        entries
            .ToFileTerms()
            .Where(term => terms.Contains(term));
}
