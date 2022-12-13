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
}
