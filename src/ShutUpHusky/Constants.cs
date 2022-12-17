internal static class Constants {
    public const int MaxCommitTitleLength = 72;
    public const double TypeOverrideThreshold = 0.5;
    public const string MatchTicket = "[a-zA-Z]+\\-[0-9]+";
    public const string DefaultCommitMessageSnippet = "committed something";
    public static string BodySeparator = $"{Environment.NewLine}{Environment.NewLine}* ";

    public static class FileExtensions {
        public const string Yaml = "yaml";
        public const string Md = "md";
    }

    public static class Types {
        public const string Feat = "feat";
        public const string Fix = "fix";
        public const string Perf = "perf";
        public const string Ci = "ci";
        public const string Test = "test";
        public const string Chore = "chore";
        public const string Docs = "docs";
        public const string MatchAny = "\\bfix|ci|perf|feat|docs|test|chore\\b";
    }

    public static class Terms {
        public const string Test = "test";
        public const string Tests = "tests";
        public const string Spec = "spec";
        public const string Specs = "specs";
        public static string[] AllTestTerms => new[] {
            Test,
            Tests,
            Spec,
            Specs,
        };
    }
}
