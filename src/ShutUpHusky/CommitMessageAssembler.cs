using System.Text;
using LibGit2Sharp;
using ShutUpHusky.Heuristics;
using ShutUpHusky.Heuristics.FileHeuristics;
using ShutUpHusky.Heuristics.RepoHeuristics;

namespace ShutUpHusky;

public class CommitMessageAssembler {
    private readonly CommitMessageAssemblerOptions _options;

    public CommitMessageAssembler(CommitMessageAssemblerOptions? options = null) {
        _options = options ?? new();
    }

    public string Assemble(IRepository repo) {
        var heuristicResults = new HeuristicResult[][] {
            new HeuristicResult[] { new TypeAndScopeHeuristic().Analyse(repo) },
            new HeuristicResult[] { new SubjectHeuristic().Analyse(repo) },
            new CreationHeuristic().Analyse(repo),
            new DeletionHeuristic().Analyse(repo),
            new ModificationHeuristic().Analyse(repo),
            new RenamingHeuristic().Analyse(repo),
        };

        return ApplyHeuristics(new PendingCommitMessage(), repo, heuristicResults)
            .ToString();
    }

    private PendingCommitMessage ApplyHeuristics(PendingCommitMessage commitMessage, IRepository repo, HeuristicResult[][] heuristics) {
        if (heuristics.Length == 0)
            return commitMessage;

        var (r, rs) = (heuristics[0], heuristics[1..]);

        if (r.Length == 0)
            return ApplyHeuristics(commitMessage, repo, rs);

        var (h, hs) = (r[0], r[1..]);

        var canAddSnippetToTitle = commitMessage.CanAddSnippetToTitle(h.Value);
        var canAddSnippetToBody = !canAddSnippetToTitle && _options.EnableBody;

        var nextHeuristics = rs.Append(hs).ToArray();

        if (h.Priority == Constants.NotAPriority)
            return ApplyHeuristics(commitMessage, repo, nextHeuristics);

        if (canAddSnippetToTitle)
            return ApplyHeuristics(commitMessage.AddMessageSnippetToTitle(h.Value).WithNextSeparator(h.After ?? string.Empty), repo, nextHeuristics);

        if (canAddSnippetToBody)
            return ApplyHeuristics(commitMessage.AddMessageSnippetToBody(h.Value), repo, nextHeuristics);

        return ApplyHeuristics(commitMessage, repo, nextHeuristics);
    }

    private class PendingCommitMessage {
        private readonly StringBuilder _titleBuilder = new();
        private readonly StringBuilder _bodyBuilder = new();
        private bool HasAppliedSeparator = false;
        private string NextSeparator = string.Empty;

        public bool CanAddSnippetToTitle(string snippet) =>
            _titleBuilder.Length + NextSeparator.Length + snippet.Length <= Constants.MaxCommitTitleLength;

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
