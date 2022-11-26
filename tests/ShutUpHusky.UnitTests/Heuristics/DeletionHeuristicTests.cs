using FluentAssertions;
using LibGit2Sharp;
using NUnit.Framework;
using ShutUpHusky.Heuristics;
using ShutUpHusky.TestSdk.Mocks;

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
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = Constants.NotAPriority,
                Value = string.Empty,
                After = null,
            },
        });
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
                Priority = Constants.MediumPriorty,
                Value = "deleted single-deleted-file",
                After = ", ",
            },
        });
    }

    [Test]
    public void ShouldReturnDeletedLabel_ForSingleDeletedFile() {
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
                        FilePath = "files/singleDeletedFile",
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("files/updatedFile", updatedFile.Object)
                .SeedPatch("files/singleDeletedFile", singleDeletedFile.Object)
                .Object
        };

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = Constants.MediumPriorty,
                Value = "deleted single-deleted-file",
                After = ", ",
            },
        });
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
                Priority = Constants.MediumPriorty,
                Value = "deleted large-deleted-file",
                After = ", ",
            },
            new() {
                Priority = 2.5,
                Value = "deleted medium-deleted-file",
                After = ", ",
            },
            new() {
                Priority = Constants.NotAPriority,
                Value = "deleted small-deleted-file",
                After = ", ",
            },
        }).And.BeInDescendingOrder(h => h.Priority);
    }
}
