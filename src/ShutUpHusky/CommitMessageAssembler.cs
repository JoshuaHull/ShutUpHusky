using LibGit2Sharp;
using ShutUpHusky.Heuristics;
using ShutUpHusky.Utils;

namespace ShutUpHusky;

public class CommitMessageAssembler {
    private const int MaxCommitTitleLength = 72;

    private IHeuristic[] Heuristics => new IHeuristic[] {
        new TicketHeuristic(_randomNumberGenerator),
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

    public string Assemble(IRepository repo) => ApplyHeuristics(repo, Heuristics, string.Empty, string.Empty);

    private string ApplyHeuristics(IRepository repo, IHeuristic[] heuristics, string commitMessage, string separator) {
        if (heuristics.Length == 0)
            return commitMessage;

        var (h, hs) = (heuristics[0], heuristics[1..]);
        var heuristicResult = h.Analyse(repo).Single();

        var shouldIncludeHeuristic = heuristicResult.Priority > 0 &&
            commitMessage.Length + separator.Length + heuristicResult.Value.Length <= MaxCommitTitleLength;

        if (!shouldIncludeHeuristic)
            return ApplyHeuristics(repo, hs, commitMessage, separator);

        return ApplyHeuristics(repo, hs, $"{commitMessage}{separator}{heuristicResult.Value}", heuristicResult.After ?? "");
    }
}
