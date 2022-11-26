using FluentAssertions;
using LibGit2Sharp;
using NUnit.Framework;
using ShutUpHusky.TestSdk.Mocks;

namespace ShutUpHusky.UnitTests;

public class CommitMessageAssemblerTests {
    [Test]
    public void ShouldCreateCommitMessage_FromManyHeuristics_WithoutExceeding72Characters() {
        // Arrange
        var renamedFile = new MockPatch {
            LinesAdded = 0,
            LinesDeleted = 0,
        };

        var createdFile = new MockPatch {
            LinesAdded = 100,
            LinesDeleted = 0,
        };

        var modifiedFile = new MockPatch {
            LinesAdded = 10,
            LinesDeleted = 20,
        };

        var deletedFile = new MockPatch {
            LinesAdded = 0,
            LinesDeleted = 20,
        };

        var repo = new MockRepository {
            Head = new MockBranch {
                Tip = new MockCommit {
                    Tree = new MockTree {
                    }.Object,
                }.Object,
                FriendlyName = "main",
            }.Object,
            Status = new MockRepositoryStatus {
                StatusEntries = new[] {
                    new MockStatusEntry {
                        State = FileStatus.RenamedInIndex,
                        FilePath = "files/renamedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.NewInIndex,
                        FilePath = "files/createdFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.ModifiedInIndex,
                        FilePath = "files/modifiedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.DeletedFromIndex,
                        FilePath = "files/deletedFile",
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("files/renamedFile", renamedFile.Object)
                .SeedPatch("files/createdFile", createdFile.Object)
                .SeedPatch("files/modifiedFile", modifiedFile.Object)
                .SeedPatch("files/deletedFile", deletedFile.Object)
                .Object,
        };

        var assembler = new CommitMessageAssembler();

        // Act
        var result = assembler.Assemble(repo.Object);

        // Assert
        result.Should().Be(
            "feat: file > created created-file, deleted deleted-file"
        );
    }
}
