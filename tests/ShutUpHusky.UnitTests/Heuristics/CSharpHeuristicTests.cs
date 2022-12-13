using FluentAssertions;
using LibGit2Sharp;
using LibGit2Sharp.Mocks;
using NUnit.Framework;
using ShutUpHusky.Heuristics;
using ShutUpHusky.Utils;

namespace ShutUpHusky.UnitTests.Heuristics;

public class CSharpHeuristicTests
{
    private CSharpHeuristic Heuristic => new();

    [Test]
    public void ShouldWriteAReasonableCommitMessage_FromReplacedLines() {
        // Arrange
        var modifiedCsharpFile = new MockPatch {
            Content =
                """
                diff --git a/src/modified.cs b/src/modified.cs
                index 811984f..b4d43d5 100644
                --- a/src/modified.cs
                +++ b/src/modified.cs
                @@ -3,5 +3,7 @@ using Namespace.Stuff;
                 string modifiedFile = 'new file';
                 
                 public void makeChange() {
                -    string wewStatement = 'wow';
                +    string wewStatement = 'wew';
                +
                +    string wowStatement = 'wowow';
                -    var secondReplacedLine = new SomeObject(11);
                +    var unrelatedArray = new[] { 1, 2, 3 };
                 }
                """,
        };

        var repo = new MockRepository {
            Status = new MockRepositoryStatus {
                StatusEntries = new[] {
                    new MockStatusEntry {
                        State = FileStatus.ModifiedInIndex,
                        FilePath = "src/modifiedFile.cs",
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("src/modifiedFile.cs", modifiedCsharpFile.Object)
                .Object,
        }.WithSensibleDefaults();

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new HeuristicResult[] {
            new() {
                Priority = 0.ToPriority(Constants.LowPriority, Constants.LanguageSpecificPriority, 3),
                Value = "replaced secondReplacedLine new SomeObject 11 with unrelatedArray new[] 1, 2, 3",
                After = ", ",
            },
            new() {
                Priority = 1.ToPriority(Constants.LowPriority, Constants.LanguageSpecificPriority, 3),
                Value = "replaced wow with wew",
                After = ", ",
            },
            new() {
                Priority = 2.ToPriority(Constants.LowPriority, Constants.LanguageSpecificPriority, 3),
                Value = "added string wowStatement = wowow",
                After = ", ",
            },
        }).And.BeInDescendingOrder(r => r.Priority);
    }

    [Test]
    public void ShouldWriteMeaningfulCommitMessage_FromAddedProperties_AndFunctions_AndClasses_Etc() {
        // Arrange
        var newCSharpFile = new MockPatch {
            Content =
                """
                diff --git a/src/CommitMessage.cs b/src/CommitMessage.cs
                index 811984f..b4d43d5 100644
                --- /dev/null
                +++ b/src/CommitMessage.cs
                @@ -3,5 +3,7 @@+namespace ShutUpHusky.Commits;
                +public abstract class CommitMessage {
                +
                +    public string Value { get; private set; }
                +
                +    public void AddSnippet(string snippet) {
                +        var trimmed = snippet.Trim();
                +    }
                +
                +    protected abstract double Evaluate();
                +}
                """,
        };

        var repo = new MockRepository {
            Status = new MockRepositoryStatus {
                StatusEntries = new[] {
                    new MockStatusEntry {
                        State = FileStatus.NewInIndex,
                        FilePath = "src/CommitMessage.cs",
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("src/CommitMessage.cs", newCSharpFile.Object)
                .Object,
        }.WithSensibleDefaults();

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new HeuristicResult[] {
            new() {
                Priority = 0.ToPriority(Constants.LowPriority, Constants.LanguageSpecificPriority, 5),
                Value = "added var trimmed = snippet Trim",
                After = ", ",
            },
            new() {
                Priority = 1.ToPriority(Constants.LowPriority, Constants.LanguageSpecificPriority, 5),
                Value = "added AddSnippet string snippet",
                After = ", ",
            },
            new() {
                Priority = 2.ToPriority(Constants.LowPriority, Constants.LanguageSpecificPriority, 5),
                Value = "added class CommitMessage",
                After = ", ",
            },
            new() {
                Priority = 3.ToPriority(Constants.LowPriority, Constants.LanguageSpecificPriority, 5),
                Value = "added string Value",
                After = ", ",
            },
            new() {
                Priority = 4.ToPriority(Constants.LowPriority, Constants.LanguageSpecificPriority, 5),
                Value = "added double Evaluate",
                After = ", ",
            },
        }).And.BeInDescendingOrder(r => r.Priority);
    }

    [TestCase(
    """
    diff --git a/src/ShutUpHusky/Heuristics/TypescriptHeuristic.cs b/src/ShutUpHusky/Heuristics/TypescriptHeuristic.cs
    index 03e3916..02cd15e 100644
    --- a/src/ShutUpHusky/Heuristics/TypescriptHeuristic.cs
    +++ b/src/ShutUpHusky/Heuristics/TypescriptHeuristic.cs
    @@ -33,7 +33,7 @@ internal class TypescriptHeuristic : IHeuristic {
             var tokenizer = new Tokenizer(new() {
                 SplitRegex =
                     \"\"\"
    -                [\\.\\(\\)\\s'\";:{}]
    +                [\\.\\(\\)\\s'\";:{}]|\\bpublic\\b|\\bprivate\\b|\\bget\\b|\\bset\\b|\\bthis\\b|\\babstract\\b|\\bbase\\b|\\bvoid\\b
                     \"\"\",
             });
             var tokenized = tokenizer.Tokenize(changedLines).ToList();
    @@ -45,6 +45,9 @@ internal class TypescriptHeuristic : IHeuristic {
                     \"typeof\", \"delete\", \"enum\", \"true\", \"false\", \"in\", \"null\", \"undefined\", \"with\", \"satisfies\", \"as\", \"public\", \"private\",
                     \"implements\", \"extends\", \"package\", \"static\", \"protected\", \"yield\", \"=\", \"==\", \"===\"
                 },
    +            IgnoreLinesStartingWithToken = new[] {
    +                \"export\", \"import\"
    +            },
             });
             scorboard.AddAll(tokenized);
     
    """,
    TestName = "TypescriptHeuristic.cs",
    ExpectedResult = "replaced ] with ]| bpublic b| bprivate bget bset bthis babstract bbase bvoid b"
    )]
    [TestCase(
    """
    diff --git a/tests/ShutUpHusky.UnitTests/Heuristics/TypescriptHeuristicTests.cs b/tests/ShutUpHusky.UnitTests/Heuristics/TypescriptHeuristicTests.cs
    index 97dc7be..b7340df 100644
    --- a/tests/ShutUpHusky.UnitTests/Heuristics/TypescriptHeuristicTests.cs
    +++ b/tests/ShutUpHusky.UnitTests/Heuristics/TypescriptHeuristicTests.cs
    @@ -75,6 +75,83 @@ public class TypescriptHeuristicTests
                     Value = \"added const wowStatement = wowow\",
                     After = \", \",
                 },
    -        });
    +        }).And.BeInDescendingOrder(r => r.Priority);
    +    }
    +
    +    [Test]
    +    public void ShouldWriteMeaningfulCommitMessage_FromAddedProperties_AndFunctions_AndClasses_Etc() {
    +        // Arrange
    +        var newTypescriptFile = new MockPatch {
    +            Content =
    +                \"\"\"
    +                diff --git a/src/CommitMessage.ts b/src/CommitMessage.ts
    +                index 811984f..b4d43d5 100644
    +                --- /dev/null
    +                +++ b/src/CommitMessage.ts
    +                @@ -3,5 +3,7 @@+import { git } from './command-line';
    +                +public class CommitMessage implements ICommitMessage {
    +                +
    +                +    public value: string;
    +                +
    +                +    public addSnippet(snippet: string): void {
    +                +        const trimmed = snippet.trim();
    +                +    }
    +                +
    +                +    evaluate: () => double = () => value.length / 3.00;
    +                +}
    +                \"\"\",
    +        };
    +
    +        var repo = new MockRepository {
    +            Head = new MockBranch {
    +                Tip = new MockCommit {
    +                    Tree = new MockTree {
    +                    }.Object,
    +                }.Object,
    +            }.Object,
    +            Status = new MockRepositoryStatus {
    +                StatusEntries = new[] {
    +                    new MockStatusEntry {
    +                        State = FileStatus.NewInIndex,
    +                        FilePath = \"src/CommitMessage.ts\",
    +                    }.Object,
    +                },
    +            }.Object,
    +            Diff = new MockDiff()
    +                .SeedPatch(\"src/CommitMessage.ts\", newTypescriptFile.Object)
    +                .Object,
    +        };
    +
    +        // Act
    +        var result = Heuristic.Analyse(repo.Object);
    +
    +        // Assert
    +        result.Should().BeEquivalentTo(new HeuristicResult[] {
    +            new() {
    +                Priority = 0.ToPriority(Constants.LowPriority, Constants.LanguageSpecificPriority, 5),
    +                Value = \"added evaluate => double = => value length / 3 00\",
    +                After = \", \",
    +            },
    +            new() {
    +                Priority = 1.ToPriority(Constants.LowPriority, Constants.LanguageSpecificPriority, 5),
    +                Value = \"added addSnippet snippet string\",
    +                After = \", \",
    +            },
    +            new() {
    +                Priority = 2.ToPriority(Constants.LowPriority, Constants.LanguageSpecificPriority, 5),
    +                Value = \"added value string\",
    +                After = \", \",
    +            },
    +            new() {
    +                Priority = 3.ToPriority(Constants.LowPriority, Constants.LanguageSpecificPriority, 5),
    +                Value = \"added const trimmed = snippet trim\",
    +                After = \", \",
    +            },
    +            new() {
    +                Priority = 4.ToPriority(Constants.LowPriority, Constants.LanguageSpecificPriority, 5),
    +                Value = \"added class CommitMessage implements ICommitMessage\",
    +                After = \", \",
    +            },
    +        }).And.BeInDescendingOrder(r => r.Priority);
         }
     }
    """,
    TestName = "TypescriptHeuristicTests.cs",
    ExpectedResult = "added Priority = 3 ToPriority Constants LowPriority, Constants LanguageSpecificPriority, 5 ,"
    )]
    public string ShouldWriteMeaningfulCommitMessages_ForRealCommitDiffs(string patchContent) {
        // Arrange
        var newCSharpFile = new MockPatch {
            Content = patchContent,
        };

        var repo = new MockRepository {
            Status = new MockRepositoryStatus {
                StatusEntries = new[] {
                    new MockStatusEntry {
                        State = FileStatus.NewInIndex,
                        FilePath = "src/CommitMessage.cs",
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("src/CommitMessage.cs", newCSharpFile.Object)
                .Object,
        }.WithSensibleDefaults();

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        return result.First().Value;
    }
}
