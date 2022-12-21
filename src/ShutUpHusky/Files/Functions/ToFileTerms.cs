using LibGit2Sharp;

namespace ShutUpHusky.Files;

internal static class ToFileTermsFunctions {

    public static IEnumerable<string> ToFileTerms(this IEnumerable<StatusEntry> entries) =>
        entries
            .Select(entry => entry.GetFileName().CamelCaseToKebabCase())
            .SelectMany(n => n.Split("-"));
}
