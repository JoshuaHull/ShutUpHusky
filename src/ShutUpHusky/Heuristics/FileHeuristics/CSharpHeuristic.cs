using LibGit2Sharp;
using ShutUpHusky.Analysis.GitTools;
using ShutUpHusky.Analysis.Statistics;
using ShutUpHusky.Analysis.Tokenizing;
using ShutUpHusky.Files;
using ShutUpHusky.Utils;

namespace ShutUpHusky.Heuristics.FileHeuristics;
// https://blog.floydhub.com/gentle-introduction-to-text-summarization-in-machine-learning/
// https://arxiv.org/pdf/1805.03989.pdf
// https://github.com/lancopku/Global-Encoding
internal class CSharpHeuristic : IFileHeuristic {
    private readonly string[] TokenScoreboardIgnoredTokens = new[] {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue",
        "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern", "false", "finally",
        "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long",
        "namespace", "new", "null", "object", "operator", "out", "override", "params", "private", "protected", "public",
        "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct",
        "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual",
        "void", "volatile", "while", "=", "==", "[", "]", "|", ",", "+", "-",
    };

    private readonly string[] TokenScoreboardIgnoreLinesStartingWith = new[] {
        "namespace", "/**", "**/", "* ", "//", "using"
    };

    private readonly string TokenizerSplitRegex =
        """
        [\\\.\(\)\s'";:{}]|\bpublic\b|\bprivate\b|\bprotected\b|\binternal\b|\bget\b|\bset\b|\binit\b|\bthis\b|\babstract\b|\bbase\b|\bvoid\b
        """;

    public ICollection<HeuristicResult> Analyse(IRepository repo) {
        var files = repo.GetFiles(FileStatus.ModifiedInIndex, FileStatus.NewInIndex, FileStatus.DeletedFromIndex);
        var cSharpExtensions = new[] { "cs" };
        var cSharpFiles = files.Where(file => cSharpExtensions.Contains(file.GetFileExtension()));

        return cSharpFiles
            .Select(file => file.ToPatch(repo))
            .SelectMany(AnalyseCSharpFile)
            .ToArray();
    }

    private IEnumerable<HeuristicResult> AnalyseCSharpFile(Patch cSharpFile) {
        var patchParser = new PatchParser();
        var changedLines = patchParser.Parse(cSharpFile);

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
