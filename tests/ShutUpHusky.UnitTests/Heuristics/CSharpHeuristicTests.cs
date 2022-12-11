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
            Head = new MockBranch {
                Tip = new MockCommit {
                    Tree = new MockTree {
                    }.Object,
                }.Object,
            }.Object,
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
        };

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new HeuristicResult[] {
            new() {
                Priority = 0.ToPriority(Constants.LowPriorty, Constants.LanguageSpecificPriority, 3),
                Value = "replaced secondReplacedLine new SomeObject 11 with unrelatedArray new[] 1, 2, 3",
                After = ", ",
            },
            new() {
                Priority = 1.ToPriority(Constants.LowPriorty, Constants.LanguageSpecificPriority, 3),
                Value = "replaced wow with wew",
                After = ", ",
            },
            new() {
                Priority = 2.ToPriority(Constants.LowPriorty, Constants.LanguageSpecificPriority, 3),
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
            Head = new MockBranch {
                Tip = new MockCommit {
                    Tree = new MockTree {
                    }.Object,
                }.Object,
            }.Object,
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
        };

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new HeuristicResult[] {
            new() {
                Priority = 0.ToPriority(Constants.LowPriorty, Constants.LanguageSpecificPriority, 5),
                Value = "added var trimmed = snippet Trim",
                After = ", ",
            },
            new() {
                Priority = 1.ToPriority(Constants.LowPriorty, Constants.LanguageSpecificPriority, 5),
                Value = "added AddSnippet string snippet",
                After = ", ",
            },
            new() {
                Priority = 2.ToPriority(Constants.LowPriorty, Constants.LanguageSpecificPriority, 5),
                Value = "added class CommitMessage",
                After = ", ",
            },
            new() {
                Priority = 3.ToPriority(Constants.LowPriorty, Constants.LanguageSpecificPriority, 5),
                Value = "added string Value",
                After = ", ",
            },
            new() {
                Priority = 4.ToPriority(Constants.LowPriorty, Constants.LanguageSpecificPriority, 5),
                Value = "added double Evaluate",
                After = ", ",
            },
        }).And.BeInDescendingOrder(r => r.Priority);
    }
}
