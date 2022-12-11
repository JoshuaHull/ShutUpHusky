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
    public ICollection<HeuristicResult> Analyse(IRepository repo) {
        var files = repo
            .GetFiles(FileStatus.ModifiedInIndex, FileStatus.NewInIndex, FileStatus.DeletedFromIndex)
            .ToList();

        if (files.Count == 0)
            return Array.Empty<HeuristicResult>();

        var typescriptExtensions = new[] { "ts", "tsx" };
        var typescriptFiles = files
            .Where(file => typescriptExtensions.Contains(file.GetFileExtension()))
            .ToList();

        if (typescriptFiles.Count == 0)
            return Array.Empty<HeuristicResult>();

        var statusEntriesByPatch = typescriptFiles.MapPatchToStatusEntry(repo);

        var patchParser = new PatchParser();
        var changedLines = patchParser.Parse(statusEntriesByPatch.Keys.First());

        var tokenizer = new Tokenizer(new() {
            SplitRegex =
                """
                [\.\(\)\s'";:{}]
                """,
        });
        var tokenized = tokenizer.Tokenize(changedLines).ToList();

        var scorboard = new TokenScoreboard(new() {
            IgnoredTokens = new[] {
                "const", "import", "export", "interface", "class", "type", "var", "function", "let", "break", "continue", "debugger",
                "do", "try", "finally", "catch", "instanceof", "return", "void", "case", "default", "if", "for", "else", "while",
                "typeof", "delete", "enum", "true", "false", "in", "null", "undefined", "with", "satisfies", "as", "public", "private",
                "implements", "extends", "package", "static", "protected", "yield", "=", "==", "==="
            },
        });
        scorboard.AddAll(tokenized);

        var highestScoringLines = scorboard.HighestScoringLines().ToArray();
        var lineCount = highestScoringLines.Length;
        var rtn = new HeuristicResult[lineCount];

        for (var i = 0; i < lineCount; i += 1) {
            var line = highestScoringLines[i];
            var commitMessageSnippet = line.ToString();
            var priority = i.ToPriority(Constants.LowPriorty, Constants.LanguageSpecificPriority, lineCount);

            rtn[i] = new() {
                Priority = priority,
                Value = commitMessageSnippet,
                After = ", ",
            };
        }

        return rtn;
    }
}
