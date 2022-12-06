using System.Text;
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

        return ApplyHeuristics(repo, heuristicResults, new PendingCommitMessage())
            .ToString();
    }

    private PendingCommitMessage ApplyHeuristics(IRepository repo, HeuristicResult[] heuristicResults, PendingCommitMessage commitMessage) {
        if (heuristicResults.Length == 0)
            return commitMessage;

        var (h, hs) = (heuristicResults[0], heuristicResults[1..]);

        var shouldIncludeHeuristic = commitMessage.CanAddMessageSnippet(h.Value);

        if (!shouldIncludeHeuristic)
            return ApplyHeuristics(repo, hs, commitMessage);

        return ApplyHeuristics(repo, hs, commitMessage.AddMessageSnippet(h.Value).WithNextSeparator(h.After ?? string.Empty));
    }

    private class PendingCommitMessage {
        private readonly StringBuilder _messageBuilder = new();
        private bool HasAppliedSeparator = false;
        private string NextSeparator = string.Empty;

        public bool CanAddMessageSnippet(string snippet) =>
            Length + NextSeparator.Length + snippet.Length <= Constants.MaxCommitTitleLength;

        private PendingCommitMessage ApplySeparator() {
            if (NextSeparator == string.Empty)
                return this;

            _messageBuilder.Append(NextSeparator);
            HasAppliedSeparator = true;
            return this;
        }

        public PendingCommitMessage AddMessageSnippet(string snippet) {
            ApplySeparator();
            _messageBuilder.Append(snippet);
            return this;
        }

        public PendingCommitMessage WithNextSeparator(string separator) {
            NextSeparator = separator;
            return this;
        }

        public int Length => _messageBuilder.Length;

        public override string ToString() =>
            (HasAppliedSeparator ? this : AddMessageSnippet(Constants.DefaultCommitMessageSnippet))._messageBuilder.ToString();
    }
}
