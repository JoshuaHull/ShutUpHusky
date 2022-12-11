using LibGit2Sharp;
using ShutUpHusky.Analysis.GitTools;
using ShutUpHusky.Analysis.Statistics;
using ShutUpHusky.Analysis.Tokenizing;
using ShutUpHusky.Utils;

namespace ShutUpHusky.Heuristics;
// https://blog.floydhub.com/gentle-introduction-to-text-summarization-in-machine-learning/
// https://arxiv.org/pdf/1805.03989.pdf
// https://github.com/lancopku/Global-Encoding
internal class CSharpHeuristic : IHeuristic {
    public ICollection<HeuristicResult> Analyse(IRepository repo) {
        var files = repo
            .GetFiles(FileStatus.ModifiedInIndex, FileStatus.NewInIndex, FileStatus.DeletedFromIndex)
            .ToList();

        if (files.Count == 0)
            return Array.Empty<HeuristicResult>();

        var csharpExtensions = new[] { "cs" };
        var csharpFiles = files
            .Where(file => csharpExtensions.Contains(file.GetFileExtension()))
            .ToList();

        if (csharpFiles.Count == 0)
            return Array.Empty<HeuristicResult>();

        var statusEntriesByPatch = csharpFiles.MapPatchToStatusEntry(repo);

        var patchParser = new PatchParser();
        var changedLines = patchParser.Parse(statusEntriesByPatch.Keys.First());

        var tokenizer = new Tokenizer(new() {
            SplitRegex =
                """
                [\\\.\(\)\s'";:{}]|\bpublic\b|\bprivate\b|\bprotected\b|\binternal\b|\bget\b|\bset\b|\binit\b|\bthis\b|\babstract\b|\bbase\b|\bvoid\b
                """,
        });
        var tokenized = tokenizer.Tokenize(changedLines).ToList();

        var scorboard = new TokenScoreboard(new() {
            IgnoredTokens = new[] {
                "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue",
                "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern", "false", "finally",
                "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long",
                "namespace", "new", "null", "object", "operator", "out", "override", "params", "private", "protected", "public",
                "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct",
                "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual",
                "void", "volatile", "while", "=", "==", "[", "]", "|", ",", "+", "-",
            },
            IgnoreLinesStartingWithToken = new[] {
                "namespace",
            },
        });
        scorboard.AddAll(tokenized);

        var highestScoringLines = scorboard.HighestScoringLines().ToArray();
        var lineCount = highestScoringLines.Length;
        var rtn = new HeuristicResult[lineCount];

        for (var i = 0; i < lineCount; i += 1) {
            var line = highestScoringLines[i];
            var commitMessageSnippet = line.ToString();
            var priority = i.ToPriority(Constants.LowPriority, Constants.LanguageSpecificPriority, lineCount);

            rtn[i] = new() {
                Priority = priority,
                Value = commitMessageSnippet,
                After = ", ",
            };
        }

        return rtn;
    }
}
