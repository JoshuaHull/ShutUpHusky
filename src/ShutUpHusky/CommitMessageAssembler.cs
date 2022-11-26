using LibGit2Sharp;
using ShutUpHusky.Heuristics;

namespace ShutUpHusky;

public class CommitMessageAssembler {
    private IHeuristic[] Heuristics => new IHeuristic[] {
        new TypeAndScopeHeuristic(),
        new SubjectHeuristic(),
        new CreationHeuristic(),
        new DeletionHeuristic(),
        new ModificationHeuristic(),
        new RenamingHeuristic(),
    };

    public string Assemble(IRepository repo) {
        var heuristicResults = Heuristics
            .SelectMany(h => h.Analyse(repo))
            .Where(h => h.Priority > Constants.NotAPriority)
            .OrderByDescending(h => h.Priority)
            .ToArray();

        return ApplyHeuristics(repo, heuristicResults, string.Empty, string.Empty);
    }

    private string ApplyHeuristics(IRepository repo, HeuristicResult[] heuristicResults, string commitMessage, string separator) {
        if (heuristicResults.Length == 0 || commitMessage.Length + separator.Length >= Constants.MaxCommitTitleLength)
            return commitMessage;

        var (h, hs) = (heuristicResults[0], heuristicResults[1..]);

        var shouldIncludeHeuristic = commitMessage.Length + separator.Length + h.Value.Length <= Constants.MaxCommitTitleLength;

        if (!shouldIncludeHeuristic)
            return ApplyHeuristics(repo, hs, commitMessage, separator);

        return ApplyHeuristics(repo, hs, $"{commitMessage}{separator}{h.Value}", h.After ?? "");
    }
}
