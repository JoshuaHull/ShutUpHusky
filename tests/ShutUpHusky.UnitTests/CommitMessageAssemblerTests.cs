using FluentAssertions;
using LibGit2Sharp;
using LibGit2Sharp.Mocks;
using NUnit.Framework;

namespace ShutUpHusky.UnitTests;

public class CommitMessageAssemblerTests {
    [Test]
    public void ShouldCreateCommitMessage_FromManyHeuristics_WithoutExceeding72Characters() {
        // Arrange
        var repo = new MockRepository {
            Head = new MockBranch {
                FriendlyName = "main",
            }
            .WithSensibleDefaults(),
        }
        .WithSensibleDefaults()
        .SeedPatch(
            new MockStatusEntry {
                State = FileStatus.RenamedInIndex,
                FilePath = "files/renamedFile",
                HeadToIndexRenameDetails = new MockRenameDetails {
                    OldFilePath = "files/file",
                    NewFilePath = "files/renamedFile",
                },
            },
            new MockPatch {
                LinesAdded = 0,
                LinesDeleted = 0,
            }
        )
        .SeedPatch(
            new MockStatusEntry {
                State = FileStatus.NewInIndex,
                FilePath = "files/createdFile",
            },
            new MockPatch {
                LinesAdded = 100,
                LinesDeleted = 0,
            }
        )
        .SeedPatch(
            new MockStatusEntry {
                State = FileStatus.ModifiedInIndex,
                FilePath = "files/modifiedFile",
            },
            new MockPatch {
                LinesAdded = 10,
                LinesDeleted = 20,
            }
        )
        .SeedPatch(
            new MockStatusEntry {
                State = FileStatus.DeletedFromIndex,
                FilePath = "files/deletedFile",
            },
            new MockPatch {
                LinesAdded = 0,
                LinesDeleted = 20,
            }
        );

        var assembler = new CommitMessageAssembler();

        // Act
        var result = assembler.Assemble(repo.Object);

        // Assert
        result.Should().Be(
            "chore: file > created created-file, deleted deleted-file"
        );
    }

    [Test]
    public void ShouldCreateCommitMessage_WithDefaultMessageSnippet_WhenNoHeuristicsReturnResults() {
        // Arrange
        var repo = new MockRepository {
            Head = new MockBranch {
                FriendlyName = "main",
            }
            .WithSensibleDefaults(),
        }
        .WithSensibleDefaults();

        var assembler = new CommitMessageAssembler();

        // Act
        var result = assembler.Assemble(repo.Object);

        // Assert
        result.Should().Be(
            $"chore: {Constants.DefaultCommitMessageSnippet}"
        );
    }

    [Test]
    public void ShouldCreateCommitMessage_FromManyHeuristics_WithMessageBody_WhenMessageBodiesAreEnabled() {
        // Arrange
        var repo = new MockRepository {
            Head = new MockBranch {
                FriendlyName = "main",
            }
            .WithSensibleDefaults(),
        }
        .WithSensibleDefaults()
        .SeedPatch(
            new MockStatusEntry {
                State = FileStatus.RenamedInIndex,
                FilePath = "files/renamedFile",
                HeadToIndexRenameDetails = new MockRenameDetails {
                    OldFilePath = "files/file",
                    NewFilePath = "files/renamedFile",
                },
            },
            new MockPatch {
                LinesAdded = 0,
                LinesDeleted = 0,
            }
        )
        .SeedPatch(
            new MockStatusEntry {
                State = FileStatus.NewInIndex,
                FilePath = "files/createdFile",
            },
            new MockPatch {
                LinesAdded = 100,
                LinesDeleted = 0,
            }
        )
        .SeedPatch(
            new MockStatusEntry {
                State = FileStatus.ModifiedInIndex,
                FilePath = "files/modifiedFile",
            },
            new MockPatch {
                LinesAdded = 10,
                LinesDeleted = 20,
            }
        )
        .SeedPatch(
            new MockStatusEntry {
                State = FileStatus.DeletedFromIndex,
                FilePath = "files/deletedFile",
            },
            new MockPatch {
                LinesAdded = 0,
                LinesDeleted = 20,
            }
        );

        var assembler = new CommitMessageAssembler(new() {
            EnableBody = true,
        });

        // Act
        var result = assembler.Assemble(repo.Object);

        // Assert
        result.Should().Be(
            """
            chore: file > created created-file, deleted deleted-file

            * updated modified-file

            * renamed renamed-file
            """
        );
    }

    [TestCase(
    """
    diff --git a/src/ShutUpHusky/CommitMessageAssembler.cs b/src/ShutUpHusky/CommitMessageAssembler.cs
    index a0f2b51..8aa3fbc 100644
    --- a/src/ShutUpHusky/CommitMessageAssembler.cs
    +++ b/src/ShutUpHusky/CommitMessageAssembler.cs
    @@ -63,14 +63,20 @@ public class CommitMessageAssembler {
     
             var (h, hs) = (r[0], r[1..]);
     
    -        var canAddSnippetToTitle = commitMessage.CanAddSnippetToTitle(h.Value);
    -        var canAddSnippetToBody = !canAddSnippetToTitle && _options.EnableBody;
    -
             var nextHeuristics = rs.Append(hs).ToArray();
     
             if (h.Priority == Constants.NotAPriority)
                 return ApplyHeuristics(commitMessage, repo, nextHeuristics);
     
    +        var canAddSnippetToTitle = commitMessage.CanAddSnippetToTitle(h.Value);
    +        var canAddSnippetToBody = !canAddSnippetToTitle && _options.EnableBody;
    +
    +        if (!canAddSnippetToTitle && h.Shortened is not null) {
    +            var hsWithShortened = hs.Append(h.Shortened).OrderByDescending(r => r.Priority).ToArray();
    +            var nextHeuristicsWithShortened = rs.Append(hsWithShortened).ToArray();
    +            return ApplyHeuristics(commitMessage, repo, nextHeuristicsWithShortened);
    +        }
    +
             if (canAddSnippetToTitle)
                 return ApplyHeuristics(commitMessage.AddMessageSnippetToTitle(h.Value).WithNextSeparator(h.After ?? string.Empty), repo, nextHeuristics);
     
    """,
    "src/ShutUpHusky/CommitMessageAssembler.cs",
    FileStatus.ModifiedInIndex,
    ExpectedResult = "chore: updated commit-message-assembler"
    )]
    [TestCase(
    """
    diff --git a/files/CreatedFile.cs b/files/CreatedFile.cs
    index 811984f..b4d43d5 100644
    --- /dev/null
    +++ b/files/CreatedFile.cs
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
    "files/CreatedFile.cs",
    FileStatus.NewInIndex,
    ExpectedResult = "chore: created created-file - added var trimmed = snippet Trim"
    )]
    public string ShouldCreateCommitMessage_WithASummary_WhenSummariesAreEnabled(string patchContent, string filePath, FileStatus state) {
        // Arrange
        var repo = new MockRepository {
            Head = new MockBranch {
                FriendlyName = "main",
            }
            .WithSensibleDefaults(),
        }
        .WithSensibleDefaults()
        .SeedPatch(
            new MockStatusEntry {
                State = state,
                FilePath = filePath,
            },
            new MockPatch {
                LinesAdded = 100,
                LinesDeleted = 0,
                Content = patchContent,
            }
        );

        var assembler = new CommitMessageAssembler(new() {
            EnableSummaries = true,
        });

        // Act & Assert
        return assembler.Assemble(repo.Object);
    }
}
