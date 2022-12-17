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
        var options = new FileHeuristicOptions {
            IncludeDiffSummary = _options.EnableSummaries,
        };

        var typeAndScope = new TypeAndScopeHeuristic().Analyse(repo).Value;
        var subject = new SubjectHeuristic().Analyse(repo)?.Value;
        var commitMessagePrefix = $"{typeAndScope}: {(subject == null ? string.Empty : $"{subject} > ")}";

        var heuristicResults = Zip(
            new CreationHeuristic {
                Options = options,
            }
            .Analyse(repo),

            new DeletionHeuristic {
                Options = options,
            }
            .Analyse(repo),

            new ModificationHeuristic {
                Options = options,
            }
            .Analyse(repo),

            new RenamingHeuristic {
                Options = options,
            }
            .Analyse(repo)
        )
        .ToArray();

        return ApplyHeuristics(
            new PendingCommitMessage(commitMessagePrefix),
            repo,
            heuristicResults
        )
        .ToString();
    }

    private IEnumerable<HeuristicResult> Zip(params HeuristicResult[][] heuristicResults) {
        var indices = new int[heuristicResults.Length];
        int empties = 0;

        for (int i = 0; i < heuristicResults.Length; i = (i + 1) % heuristicResults.Length) {
            if (empties >= heuristicResults.Length)
                break;

            var results = heuristicResults[i];
            var idx = indices[i];

            if (idx >= results.Length) {
                empties += 1;
                continue;
            }

            empties = 0;
            indices[i] += 1;

            yield return results[idx];
        }
    }

    private PendingCommitMessage ApplyHeuristics(PendingCommitMessage commitMessage, IRepository repo, HeuristicResult[] heuristicResults) {
        if (heuristicResults.Length == 0)
            return commitMessage;

        var (h, hs) = (heuristicResults[0], heuristicResults[1..]);

        var canAddSnippetToTitle = commitMessage.CanAddSnippetToTitle(h.Value);
        var canAddSnippetToBody = !canAddSnippetToTitle && _options.EnableBody;

        if (!canAddSnippetToTitle && h.LowerPriorityResult is not null) {
            var hsWithLowerPriorityResult = hs.Append(h.LowerPriorityResult).OrderByDescending(r => r.Priority).ToArray();
            return ApplyHeuristics(commitMessage, repo, hsWithLowerPriorityResult);
        }

        if (canAddSnippetToTitle)
            return ApplyHeuristics(commitMessage.AddMessageSnippetToTitle(h.Value), repo, hs);

        if (canAddSnippetToBody)
            return ApplyHeuristics(commitMessage.AddMessageSnippetToBody(h.Value), repo, hs);

        return ApplyHeuristics(commitMessage, repo, hs);
    }

    private class PendingCommitMessage {
        private readonly StringBuilder _titleBuilder;
        private readonly StringBuilder _bodyBuilder;
        private bool HasAddedToTitle = false;
        private string NextSeparator = string.Empty;

        public PendingCommitMessage(string initialTitle) {
            _titleBuilder = new(initialTitle);
            _bodyBuilder = new();
        }

        public bool CanAddSnippetToTitle(string snippet) =>
            _titleBuilder.Length + NextSeparator.Length + snippet.Length <= Constants.MaxCommitTitleLength;

        private PendingCommitMessage ApplySeparator() {
            if (NextSeparator == string.Empty)
                return this;

            _titleBuilder.Append(NextSeparator);
            return this;
        }

        public PendingCommitMessage AddMessageSnippetToTitle(string snippet) {
            ApplySeparator();
            HasAddedToTitle = true;
            NextSeparator = ", ";
            _titleBuilder.Append(snippet);
            return this;
        }

        public PendingCommitMessage AddMessageSnippetToBody(string snippet) {
            _bodyBuilder.Append(Constants.BodySeparator);
            _bodyBuilder.Append(snippet);
            return this;
        }

        public override string ToString() {
            var rtn = new StringBuilder()
                .Append(_titleBuilder);

            if (!HasAddedToTitle)
                rtn
                    .Append(NextSeparator)
                    .Append(Constants.DefaultCommitMessageSnippet);

            return rtn
                .Append(_bodyBuilder)
                .ToString();
        }
    }
}
