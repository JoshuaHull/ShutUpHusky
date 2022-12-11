using LibGit2Sharp;
using ShutUpHusky.Analysis.GitTools;
using ShutUpHusky.Analysis.Statistics;
using ShutUpHusky.Analysis.Tokenizing;
using ShutUpHusky.Utils;

namespace ShutUpHusky.Heuristics;
// https://blog.floydhub.com/gentle-introduction-to-text-summarization-in-machine-learning/
// https://arxiv.org/pdf/1805.03989.pdf
// https://github.com/lancopku/Global-Encoding
internal class TypescriptHeuristic : IHeuristic {
    private readonly string[] TokenScoreboardIgnoredTokens = new[] {
        "const", "import", "export", "interface", "class", "type", "var", "function", "let", "break", "continue", "debugger",
        "do", "try", "finally", "catch", "instanceof", "return", "void", "case", "default", "if", "for", "else", "while",
        "typeof", "delete", "enum", "true", "false", "in", "null", "undefined", "with", "satisfies", "as", "public", "private",
        "implements", "extends", "package", "static", "protected", "yield", "=", "==", "===", "[", "]", "|", ",", "+", "-",
    };

    private readonly string[] TokenScoreboardIgnoreLinesStartingWith = new[] {
        "export", "import"
    };

    private readonly string TokenizerSplitRegex =
        """
        [\\\.\(\)\s'";:{}]|\bpublic\b|\bprivate\b|\bget\b|\bset\b|\bthis\b|\babstract\b|\bbase\b|\bvoid\b
        """;

    public ICollection<HeuristicResult> Analyse(IRepository repo) {
        var files = repo.GetFiles(FileStatus.ModifiedInIndex, FileStatus.NewInIndex, FileStatus.DeletedFromIndex);
        var typescriptExtensions = new[] { "ts", "tsx" };
        var typescriptFiles = files.Where(file => typescriptExtensions.Contains(file.GetFileExtension()));

        return typescriptFiles
            .Select(file => file.ToPatch(repo))
            .SelectMany(AnalyseTypescriptFile)
            .ToArray();
    }

    private IEnumerable<HeuristicResult> AnalyseTypescriptFile(Patch typescriptFile) {
        var patchParser = new PatchParser();
        var changedLines = patchParser.Parse(typescriptFile);

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

        for (var i = 0; i < lineCount; i += 1) {
            var line = highestScoringLines[i];
            var commitMessageSnippet = line.ToString();
            var priority = i.ToPriority(Constants.LowPriority, Constants.LanguageSpecificPriority, lineCount);

            yield return new() {
                Priority = priority,
                Value = commitMessageSnippet,
                After = ", ",
            };
        }
    }
}
