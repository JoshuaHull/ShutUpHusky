using FluentAssertions;
using LibGit2Sharp;
using NUnit.Framework;
using ShutUpHusky.Heuristics;
using ShutUpHusky.TestSdk.Mocks;

namespace ShutUpHusky.UnitTests.Heuristics;

public class ModificationHeuristicTests
{
    private ModificationHeuristic Heuristic => new();

    [Test]
    public void ShouldNotReturnALabel_WhenThereAreNoFilesChanged() {
        // Arrange
        var createdFile = new MockPatch {
            LinesAdded = 100,
            LinesDeleted = 0,
        };

        var renamedFile = new MockPatch {
            LinesAdded = 50,
            LinesDeleted = 50,
        };

        var deletedFile = new MockPatch {
            LinesAdded = 0,
            LinesDeleted = 100,
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
                        FilePath = "files/createdFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.RenamedInIndex,
                        FilePath = "files/renamedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.DeletedFromIndex,
                        FilePath = "files/deletedFile",
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("files/createdFile", createdFile.Object)
                .SeedPatch("files/renamedFile", renamedFile.Object)
                .SeedPatch("files/deletedFile", deletedFile.Object)
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
    public void ShouldReturnUpdatedLabel_ForModifiedFile_WithNoLinesChanged() {
        // Arrange
        var singleChangedFile = new MockPatch {
            LinesAdded = 0,
            LinesDeleted = 0,
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
                        State = FileStatus.NewInIndex,
                        FilePath = "files/createdFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.ModifiedInIndex,
                        FilePath = "files/singleChangedFile",
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("files/createdFile", createdFile.Object)
                .SeedPatch("files/singleChangedFile", singleChangedFile.Object)
                .Object
        };

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = Constants.LowPriorty,
                Value = "updated single-changed-file",
                After = ", ",
            },
        });
    }

    [Test]
    public void ShouldReturnUpdatedLabel_ForSingleChangedFile() {
        // Arrange
        var singleChangedFile = new MockPatch {
            LinesAdded = 50,
            LinesDeleted = 20,
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
                        State = FileStatus.NewInIndex,
                        FilePath = "files/createdFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.ModifiedInIndex,
                        FilePath = "files/singleChangedFile",
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("files/createdFile", createdFile.Object)
                .SeedPatch("files/singleChangedFile", singleChangedFile.Object)
                .Object
        };

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = Constants.LowPriorty,
                Value = "updated single-changed-file",
                After = ", ",
            },
        });
    }

    [Test]
    public void ShouldReturnUpdatedLabel_ForEachChangedFile_WithDescendingPriority()
    {
        // Arrange
        var leastChangedFile = new MockPatch {
            LinesAdded = 50,
            LinesDeleted = 10,
        };

        var brandNewFile = new MockPatch {
            LinesAdded = 100,
            LinesDeleted = 0,
        };

        var mildlyChangedFile = new MockPatch {
            LinesAdded = 50,
            LinesDeleted = 50,
        };

        var mostChangedFile = new MockPatch {
            LinesAdded = 100,
            LinesDeleted = 10,
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
                        FilePath = "files/brandNewFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.ModifiedInIndex,
                        FilePath = "files/mildlyChangedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.ModifiedInIndex,
                        FilePath = "files/leastChangedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.ModifiedInIndex,
                        FilePath = "files/mostChangedFile",
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("files/brandNewFile", brandNewFile.Object)
                .SeedPatch("files/mildlyChangedFile", mildlyChangedFile.Object)
                .SeedPatch("files/leastChangedFile", leastChangedFile.Object)
                .SeedPatch("files/mostChangedFile", mostChangedFile.Object)
                .Object,
        };

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = Constants.LowPriorty,
                Value = "updated most-changed-file",
                After = ", ",
            },
            new() {
                Priority = 0.5,
                Value = "updated mildly-changed-file",
                After = ", ",
            },
            new() {
                Priority = Constants.NotAPriority,
                Value = "updated least-changed-file",
                After = ", ",
            },
        }).And.BeInDescendingOrder(h => h.Priority);
    }
}
