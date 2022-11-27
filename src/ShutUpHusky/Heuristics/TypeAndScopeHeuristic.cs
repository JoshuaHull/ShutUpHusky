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
            repo.Head.FriendlyName, Constants.Types.MatchAny, RegexOptions.IgnoreCase
        );

        if (typeMatch.Success)
            return typeMatch.Value.ToLowerInvariant();

        var allAlteredFiles = repo.GetAllAlteredFiles().ToList();
        var deletedFiles = allAlteredFiles.GetDeletedFiles().ToList();

        if (deletedFiles.Count / ((double)allAlteredFiles.Count) >= Constants.TypeOverrideThreshold)
            return Constants.Types.Perf;

        var ciFiles = allAlteredFiles.WithFileExtensions(Constants.FileExtensions.Yaml).ToList();

        if (ciFiles.Count / ((double)allAlteredFiles.Count) >= Constants.TypeOverrideThreshold)
            return Constants.Types.Ci;

        var docsFiles = allAlteredFiles.WithFileExtensions(Constants.FileExtensions.Md).ToList();

        if (docsFiles.Count / ((double)allAlteredFiles.Count) >= Constants.TypeOverrideThreshold)
            return Constants.Types.Docs;

        var testFiles = allAlteredFiles.WithFileTerms(Constants.Terms.AllTestTerms).ToList();

        if (testFiles.Count / ((double)allAlteredFiles.Count) >= Constants.TypeOverrideThreshold)
            return Constants.Types.Test;

        return null;
    }

    private string? GetScope(IRepository repo) {
        var ticketNumberMatch = Regex.Match(repo.Head.FriendlyName, Constants.MatchTicket);

        return ticketNumberMatch.Success
            ? ticketNumberMatch.Value.ToLowerInvariant()
            : null;
    }
}
