using LibGit2Sharp;
using ShutUpHusky.Heuristics;

namespace ShutUpHusky;

public class CommitMessageAssembler {
    private const int MaxCommitTitleLength = 72;

    public string Assemble(Repository repo) {
        var result = string.Empty;
        var separator = string.Empty;

        if (MaybeAddTicket(repo, separator, ref result))
            separator = ": ";

        if (MaybeAddSubject(repo, separator, ref result))
            separator = " > ";

        if (MaybeAddCreation(repo, separator, ref result))
            separator = ", ";

        if (MaybeAddDeletion(repo, separator, ref result))
            separator = ", ";

        if (MaybeAddModification(repo, separator, ref result))
            separator = ", ";

        MaybeAddRenaming(repo, separator, ref result);

        return result;
    }

    private bool MaybeAddTicket(Repository repo, string before, ref string commitMessage) =>
        MaybeAddHeuristic(repo, before, new TicketHeuristic(), ref commitMessage);

    private bool MaybeAddSubject(Repository repo, string before, ref string commitMessage) =>
        MaybeAddHeuristic(repo, before, new SubjectHeuristic(), ref commitMessage);

    private bool MaybeAddCreation(Repository repo, string before, ref string commitMessage) =>
        MaybeAddHeuristic(repo, before, new CreationHeuristic(), ref commitMessage);

    private bool MaybeAddDeletion(Repository repo, string before, ref string commitMessage) =>
        MaybeAddHeuristic(repo, before, new DeletionHeuristic(), ref commitMessage);

    private bool MaybeAddModification(Repository repo, string before, ref string commitMessage) =>
        MaybeAddHeuristic(repo, before, new ModificationHeuristic(), ref commitMessage);

    private bool MaybeAddRenaming(Repository repo, string before, ref string commitMessage) =>
        MaybeAddHeuristic(repo, before, new RenamingHeuristic(), ref commitMessage);

    private bool MaybeAddHeuristic(Repository repo, string before, IHeuristic heuristic, ref string commitMessage) {
        var heuristicResult = heuristic.Analyse(repo);

        var shouldIncludeHeuristic = heuristicResult.Priority > 0 &&
            commitMessage.Length + before.Length + heuristicResult.Value.Length <= MaxCommitTitleLength;

        if (shouldIncludeHeuristic)
            commitMessage += $"{before}{heuristicResult.Value}";

        return shouldIncludeHeuristic;
    }
}
