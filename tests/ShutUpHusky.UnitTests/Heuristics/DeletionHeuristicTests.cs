using FluentAssertions;
using LibGit2Sharp;
using LibGit2Sharp.Mocks;
using NUnit.Framework;
using ShutUpHusky.Heuristics;

namespace ShutUpHusky.UnitTests.Heuristics;

public class DeletionHeuristicTests
{
    private DeletionHeuristic Heuristic => new();

    [Test]
    public void ShouldNotReturnALabel_WhenThereAreNoDeletedFiles() {
        // Arrange
        var changedFile = new MockPatch {
            LinesAdded = 50,
            LinesDeleted = 50,
        };

        var renamedFile = new MockPatch {
            LinesAdded = 50,
            LinesDeleted = 50,
        };

        var createdFile = new MockPatch {
            LinesAdded = 100,
            LinesDeleted = 0,
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
                        FilePath = "files/changedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.RenamedInIndex,
                        FilePath = "files/renamedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.NewInIndex,
                        FilePath = "files/createdFile",
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("files/changedFile", changedFile.Object)
                .SeedPatch("files/renamedFile", renamedFile.Object)
                .SeedPatch("files/createdFile", createdFile.Object)
                .Object
        };

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().HaveCount(0);
    }

    [Test]
    public void ShouldReturnDeletedLabel_ForDeletedFile_WithNoLinesChanged() {
        // Arrange
        var singleDeletedFile = new MockPatch {
            LinesAdded = 0,
            LinesDeleted = 0,
        };

        var changedFile = new MockPatch {
            LinesAdded = 100,
            LinesDeleted = 0,
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
                        State = FileStatus.DeletedFromIndex,
                        FilePath = "files/singleDeletedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.ModifiedInIndex,
                        FilePath = "files/changedFile",
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("files/singleDeletedFile", singleDeletedFile.Object)
                .SeedPatch("files/changedFile", changedFile.Object)
                .Object
        };

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = Constants.HighPriorty,
                Value = "deleted single-deleted-file",
                After = ", ",
            },
        });
    }

    [TestCase("singleDeletedFile", ExpectedResult = "deleted single-deleted-file")]
    [TestCase("singleDeletedFile.ts", ExpectedResult = "deleted single-deleted-file")]
    [TestCase("files/SingleDeletedFile.cs", ExpectedResult = "deleted single-deleted-file")]
    [TestCase("singleDeletedFile.spec.ts", ExpectedResult = "deleted single-deleted-file tests")]
    [TestCase("singleDeletedFile.specs.ts", ExpectedResult = "deleted single-deleted-file tests")]
    [TestCase("singleDeletedFile.test.ts", ExpectedResult = "deleted single-deleted-file tests")]
    [TestCase("singleDeletedFile.tests.ts", ExpectedResult = "deleted single-deleted-file tests")]
    public string ShouldReturnDeletedLabel_ForSingleDeletedFile(string fileName) {
        // Arrange
        var singleDeletedFile = new MockPatch {
            LinesAdded = 0,
            LinesDeleted = 20,
        };

        var updatedFile = new MockPatch {
            LinesAdded = 50,
            LinesDeleted = 50,
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
                        FilePath = "files/updatedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.DeletedFromIndex,
                        FilePath = fileName,
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("files/updatedFile", updatedFile.Object)
                .SeedPatch(fileName, singleDeletedFile.Object)
                .Object
        };

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        return result.Single().Value;
    }

    [Test]
    public void ShouldReturnDeletionLabel_ForEachDeletedFile_WithDescendingPriority()
    {
        // Arrange
        var smallDeletedFile = new MockPatch {
            LinesAdded = 0,
            LinesDeleted = 50,
        };

        var mediumDeletedFile = new MockPatch {
            LinesAdded = 0,
            LinesDeleted = 75,
        };

        var largeDeletedFile = new MockPatch {
            LinesAdded = 0,
            LinesDeleted = 100,
        };

        var modifiedFile = new MockPatch {
            LinesAdded = 10,
            LinesDeleted = 20,
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
                        State = FileStatus.DeletedFromIndex,
                        FilePath = "files/smallDeletedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.DeletedFromIndex,
                        FilePath = "files/mediumDeletedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.DeletedFromIndex,
                        FilePath = "files/largeDeletedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.ModifiedInIndex,
                        FilePath = "files/modifiedFile",
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("files/smallDeletedFile", smallDeletedFile.Object)
                .SeedPatch("files/mediumDeletedFile", mediumDeletedFile.Object)
                .SeedPatch("files/largeDeletedFile", largeDeletedFile.Object)
                .SeedPatch("files/modifiedFile", modifiedFile.Object)
                .Object,
        };

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = Constants.HighPriorty,
                Value = "deleted large-deleted-file",
                After = ", ",
            },
            new() {
                Priority = 5.5,
                Value = "deleted medium-deleted-file",
                After = ", ",
            },
            new() {
                Priority = Constants.LowPriority,
                Value = "deleted small-deleted-file",
                After = ", ",
            },
        }).And.BeInDescendingOrder(h => h.Priority);
    }
}
