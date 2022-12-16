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
    public required string[] TokenScoreboardIgnoredTokens;
    public required string[] TokenScoreboardIgnoreLinesStartingWith;
    public required string TokenizerSplitRegex;

    public IEnumerable<HeuristicResult> Summarize(Patch patch) {
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

        var highestScoringLines = scorboard.HighestScoringLines().ToArray();
        var lineCount = highestScoringLines.Length;

        for (var i = 0; i < lineCount; i += 1)
            yield return new() {
                Priority = Constants.LanguageSpecificPriority,
                Value = highestScoringLines[i].ToString(),
                After = ", ",
            };
    }
}
