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
            MaybeAddHeuristic(repo, heuristic, ref result, ref separator);

        return result;
    }

    private void MaybeAddHeuristic(Repository repo, IHeuristic heuristic, ref string commitMessage, ref string before) {
        var heuristicResult = heuristic.Analyse(repo);

        var shouldIncludeHeuristic = heuristicResult.Priority > 0 &&
            commitMessage.Length + before.Length + heuristicResult.Value.Length <= MaxCommitTitleLength;

        if (!shouldIncludeHeuristic)
            return;

        commitMessage += $"{before}{heuristicResult.Value}";
        before = heuristicResult.After ?? "";
    }
}
