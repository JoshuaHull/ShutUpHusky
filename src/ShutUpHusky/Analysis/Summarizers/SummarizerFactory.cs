using LibGit2Sharp;
using ShutUpHusky.Files;

namespace ShutUpHusky.Analysis.Summarizers;

internal static class SummarizerFactory {
    public static DiffSummarizer? Create(StatusEntry statusEntry) {
        var fileExtension = statusEntry.GetFileExtension().ToLowerInvariant();

        if (new string[] { "cs", }.Contains(fileExtension))
            return new DiffSummarizer {
                TokenScoreboardIgnoredTokens = new[] {
                    "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue",
                    "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern", "false", "finally",
                    "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long",
                    "namespace", "new", "null", "object", "operator", "out", "override", "params", "private", "protected", "public",
                    "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "var",
                    "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual",
                    "void", "volatile", "while", "=", "==", "[", "]", "|", ",", "+", "-",
                },
                TokenScoreboardIgnoreLinesStartingWith = new[] {
                    "namespace", "/**", "**/", "* ", "//", "using",
                },
                TokenizerSplitRegex =
                    """
                    [\\\.\(\)\s'";:{}]|\bpublic\b|\bprivate\b|\bprotected\b|\binternal\b|\bget\b|\bset\b|\binit\b|\bthis\b|\babstract\b|\bbase\b|\bvoid\b
                    """,
            };

        if (new string[] { "ts", "tsx", }.Contains(fileExtension))
            return new DiffSummarizer {
                TokenScoreboardIgnoredTokens = new[] {
                    "const", "import", "export", "interface", "class", "type", "var", "function", "let", "break", "continue", "debugger",
                    "do", "try", "finally", "catch", "instanceof", "return", "void", "case", "default", "if", "for", "else", "while",
                    "typeof", "delete", "enum", "true", "false", "in", "null", "undefined", "with", "satisfies", "as", "public", "private",
                    "implements", "extends", "package", "static", "protected", "yield", "=", "==", "===", "[", "]", "|", ",", "+", "-",
                },
                TokenScoreboardIgnoreLinesStartingWith = new[] {
                    "export", "import", "/**", "**/", "* ", "//",
                },
                TokenizerSplitRegex =
                    """
                    [\\\.\(\)\s'";:{}]|\bpublic\b|\bprivate\b|\bget\b|\bset\b|\bthis\b|\babstract\b|\bbase\b|\bvoid\b
                    """,
            };

        return new DiffSummarizer {
            TokenScoreboardIgnoredTokens = new[] {
                "const", "interface", "class", "var", "function", "let", "break", "continue",
                "do", "try", "finally", "catch", "return", "void", "case", "default", "if", "for", "else", "while",
                "enum", "true", "false", "in", "null", "with", "as", "public", "private",
                "implements", "extends", "static", "protected", "yield", "=", "==", "===", "[", "]", "|", ",", "+", "-",
            },
            TokenScoreboardIgnoreLinesStartingWith = new[] {
                "/**", "**/", "* ", "//",
            },
            TokenizerSplitRegex =
                """
                [\\\.\(\)\s'";:{}]|\bpublic\b|\bprivate\b|\bget\b|\bset\b|\bthis\b|\babstract\b|\bbase\b|\bvoid\b
                """,
        };
    }
}
