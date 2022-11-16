using LibGit2Sharp;
using ShutUpHusky.Heuristics;

namespace ShutUpHusky;

public class CommitMessageAssembler {
    private const int MaxCommitTitleLength = 72;

    private static IEnumerable<IHeuristic> Heuristics => new IHeuristic[] {
        new TicketHeuristic(),
        new SubjectHeuristic(),
        new CreationHeuristic(),
        new DeletionHeuristic(),
        new ModificationHeuristic(),
        new RenamingHeuristic(),
    };

    public string Assemble(Repository repo) {
        var result = string.Empty;
        var separator = string.Empty;
        var heuristics = Heuristics;

        foreach (var heuristic in heuristics)
            separator = MaybeAddHeuristic(repo, separator, heuristic, ref result) ?? separator;

        return result;
    }

    private string? MaybeAddHeuristic(Repository repo, string before, IHeuristic heuristic, ref string commitMessage) {
        var heuristicResult = heuristic.Analyse(repo);

        var shouldIncludeHeuristic = heuristicResult.Priority > 0 &&
            commitMessage.Length + before.Length + heuristicResult.Value.Length <= MaxCommitTitleLength;

        if (shouldIncludeHeuristic)
            commitMessage += $"{before}{heuristicResult.Value}";

        return heuristicResult.After;
    }
}
