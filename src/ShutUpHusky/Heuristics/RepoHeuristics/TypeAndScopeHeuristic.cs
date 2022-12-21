using LibGit2Sharp;
using ShutUpHusky.Files;
using ShutUpHusky.Utils;
using System.Text.RegularExpressions;

namespace ShutUpHusky.Heuristics.RepoHeuristics;

internal class TypeAndScopeHeuristic : IRepoHeuristic {
    public HeuristicResult Analyse(IRepository repo) {
        var scope = GetScope(repo);
        var type = GetType(repo) ?? (scope is null ? Constants.Types.Chore : Constants.Types.Feat);

        var typeAndScope = scope is null
            ? type
            : $"{type}({scope})";

        return new() {
            Value = typeAndScope,
        };
    }

    private string? GetType(IRepository repo) {
        var allAlteredFiles = repo.GetAllAlteredFiles().ToArray();
        var deletedFiles = allAlteredFiles.Where(file => (file.State & FileStatus.DeletedFromIndex) == FileStatus.DeletedFromIndex).ToArray();

        if (deletedFiles.Length / ((double)allAlteredFiles.Length) >= Constants.TypeOverrideThreshold)
            return Constants.Types.Perf;

        var ciFiles = allAlteredFiles.WithFileExtensions(Constants.FileExtensions.Yaml).ToArray();

        if (ciFiles.Length / ((double)allAlteredFiles.Length) >= Constants.TypeOverrideThreshold)
            return Constants.Types.Ci;

        var docsFiles = allAlteredFiles.WithFileExtensions(Constants.FileExtensions.Md).ToArray();

        if (docsFiles.Length / ((double)allAlteredFiles.Length) >= Constants.TypeOverrideThreshold)
            return Constants.Types.Docs;

        var testFiles = allAlteredFiles.WithFileTerms(Constants.Terms.AllTestTerms).ToArray();

        if (testFiles.Length / ((double)allAlteredFiles.Length) >= Constants.TypeOverrideThreshold)
            return Constants.Types.Test;

        var typeMatch = Regex.Match(
            repo.Head.FriendlyName, Constants.Types.MatchAny, RegexOptions.IgnoreCase
        );

        return typeMatch.Success
            ? typeMatch.Value.ToLowerInvariant()
            : null;
    }

    private string? GetScope(IRepository repo) {
        var ticketNumberMatch = Regex.Match(repo.Head.FriendlyName, Constants.MatchTicket);

        return ticketNumberMatch.Success
            ? ticketNumberMatch.Value.ToLowerInvariant()
            : null;
    }
}
