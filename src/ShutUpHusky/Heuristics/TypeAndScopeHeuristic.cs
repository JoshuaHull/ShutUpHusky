using LibGit2Sharp;
using ShutUpHusky.Utils;
using System.Text.RegularExpressions;

namespace ShutUpHusky.Heuristics;

internal class TypeAndScopeHeuristic : IHeuristic {
    public ICollection<HeuristicResult> Analyse(IRepository repo) {
        var scope = GetScope(repo);
        var type = GetType(repo) ?? (scope is null ? Constants.Types.Chore : Constants.Types.Feat);

        var typeAndScope = scope is null
            ? type
            : $"{type}({scope})";

        return new HeuristicResult[] {
            new() {
                Priority = Constants.TypeAndScopePriority,
                Value = typeAndScope,
                After = ": ",
            },
        };
    }

    private string? GetType(IRepository repo) {
        var typeMatch = Regex.Match(
            repo.Head.FriendlyName, $"\\b{Constants.Types.MatchAny}\\b", RegexOptions.IgnoreCase
        );

        if (typeMatch.Success)
            return typeMatch.Value.ToLowerInvariant();

        var allAlteredFiles = repo.GetAllAlteredFiles();
        var deletedFiles = allAlteredFiles.GetDeletedFiles();

        if (deletedFiles.Count / ((double)allAlteredFiles.Count) >= Constants.TypeOverrideThreshold)
            return Constants.Types.Perf;

        var ciFiles = allAlteredFiles.WithFileExtensions(Constants.FileExtensions.Yaml);

        if (ciFiles.Count / ((double)allAlteredFiles.Count) >= Constants.TypeOverrideThreshold)
            return Constants.Types.Ci;

        var docsFiles = allAlteredFiles.WithFileExtensions(Constants.FileExtensions.Md);

        if (docsFiles.Count / ((double)allAlteredFiles.Count) >= Constants.TypeOverrideThreshold)
            return Constants.Types.Docs;

        var testFiles = allAlteredFiles.WithFileTerms(Constants.Terms.AllTestTerms);

        if (testFiles.Count / ((double)allAlteredFiles.Count) >= Constants.TypeOverrideThreshold)
            return Constants.Types.Test;

        return null;
    }

    private string? GetScope(IRepository repo) {
        var ticketNumberMatch = Regex.Match(repo.Head.FriendlyName, "[a-zA-Z]+\\-[0-9]+");

        return ticketNumberMatch.Success
            ? ticketNumberMatch.Value.ToLowerInvariant()
            : null;
    }
}
