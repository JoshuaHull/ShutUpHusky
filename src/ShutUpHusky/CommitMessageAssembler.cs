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

    private readonly CommitMessageAssemblerOptions _options;

    public CommitMessageAssembler(CommitMessageAssemblerOptions? options = null) {
        _options = options ?? new();
    }

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

        var canAddSnippetToTitle = commitMessage.CanAddSnippetToTitle(h.Value);
        var canAddSnippetToBody = !canAddSnippetToTitle && _options.ShouldEnableBody;

        if (canAddSnippetToTitle)
            return ApplyHeuristics(repo, hs, commitMessage.AddMessageSnippetToTitle(h.Value).WithNextSeparator(h.After ?? string.Empty));

        if (canAddSnippetToBody)
            return ApplyHeuristics(repo, hs, commitMessage.AddMessageSnippetToBody(h.Value));

        return ApplyHeuristics(repo, hs, commitMessage);
    }

    private class PendingCommitMessage {
        private readonly StringBuilder _titleBuilder = new();
        private readonly StringBuilder _bodyBuilder = new();
        private bool HasAppliedSeparator = false;
        private string NextSeparator = string.Empty;

        public bool CanAddSnippetToTitle(string snippet) =>
            Length + NextSeparator.Length + snippet.Length <= Constants.MaxCommitTitleLength;

        private PendingCommitMessage ApplySeparator() {
            if (NextSeparator == string.Empty)
                return this;

            _titleBuilder.Append(NextSeparator);
            HasAppliedSeparator = true;
            return this;
        }

        public PendingCommitMessage AddMessageSnippetToTitle(string snippet) {
            ApplySeparator();
            _titleBuilder.Append(snippet);
            return this;
        }

        public PendingCommitMessage AddMessageSnippetToBody(string snippet) {
            _bodyBuilder.Append(Constants.BodySeparator);
            _bodyBuilder.Append(snippet);
            return this;
        }

        public PendingCommitMessage WithNextSeparator(string separator) {
            NextSeparator = separator;
            return this;
        }

        public int Length => _titleBuilder.Length;

        public override string ToString() {
            var rtn = new StringBuilder()
                .Append(_titleBuilder);

            if (!HasAppliedSeparator)
                rtn
                    .Append(NextSeparator)
                    .Append(Constants.DefaultCommitMessageSnippet);

            return rtn
                .Append(_bodyBuilder)
                .ToString();
        }
    }
}
