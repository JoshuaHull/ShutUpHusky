using LibGit2Sharp;
using ShutUpHusky.Analysis.GitTools;
using ShutUpHusky.Analysis.Statistics;
using ShutUpHusky.Analysis.Tokenizing;
using ShutUpHusky.Heuristics;

namespace ShutUpHusky.Analysis.Summarizers;
// https://blog.floydhub.com/gentle-introduction-to-text-summarization-in-machine-learning/
// https://arxiv.org/pdf/1805.03989.pdf
// https://github.com/lancopku/Global-Encoding
internal class DiffSummarizer {
    public required string[] TokenScoreboardIgnoredTokens { private get; init; }
    public required string[] TokenScoreboardIgnoreLinesStartingWith { private get; init; }
    public required string TokenizerSplitRegex { private get; init; }

    public HeuristicResult? Summarize(Patch patch) {
        var patchParser = new PatchParser();
        var changedLines = patchParser.Parse(patch);

        var tokenizer = new Tokenizer(new() {
            SplitRegex = TokenizerSplitRegex,
        });
        var tokenized = tokenizer.Tokenize(changedLines).ToList();

        var scorboard = new TokenScoreboard(new() {
            IgnoredTokens = TokenScoreboardIgnoredTokens,
            IgnoreLinesStartingWithToken = TokenScoreboardIgnoreLinesStartingWith,
        });
        scorboard.AddAll(tokenized);

        var lines = scorboard.HighestScoringLines();
        HeuristicResult? rtn = null;

        foreach (var line in lines) {
            if (rtn is null) {
                rtn = new() {
                    Value = line.ToString(),
                };
                continue;
            }

            rtn = HeuristicResult.WithLowerPriorityResult(
                rtn,
                new HeuristicResult {
                    Value = line.ToString(),
                }
            );
        }

        return rtn;
    }
}
