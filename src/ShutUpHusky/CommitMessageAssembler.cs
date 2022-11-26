using LibGit2Sharp;
using ShutUpHusky.Heuristics;
using ShutUpHusky.Utils;

namespace ShutUpHusky;

public class CommitMessageAssembler {
    private IHeuristic[] Heuristics => new IHeuristic[] {
        new TypeAndScopeHeuristic(_randomNumberGenerator),
        new SubjectHeuristic(),
        new CreationHeuristic(),
        new DeletionHeuristic(),
        new ModificationHeuristic(),
        new RenamingHeuristic(),
    };

    private readonly IRandomNumberGenerator _randomNumberGenerator;

    public CommitMessageAssembler(IRandomNumberGenerator randomNumberGenerator) {
        _randomNumberGenerator = randomNumberGenerator;
    }

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
