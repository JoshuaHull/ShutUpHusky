using FluentAssertions;
using LibGit2Sharp;
using NUnit.Framework;
using ShutUpHusky.Heuristics;
using ShutUpHusky.TestSdk.Mocks;

namespace ShutUpHusky.UnitTests.Heuristics;

public class RenamingHeuristicTests
{
    private RenamingHeuristic Heuristic => new();

    [Test]
    public void ShouldNotReturnALabel_WhenThereAreNoRenamedFiles() {
        // Arrange
        var changedFile = new MockPatch {
            LinesAdded = 50,
            LinesDeleted = 50,
        };

        var deletedFile = new MockPatch {
            LinesAdded = 0,
            LinesDeleted = 100,
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
                        State = FileStatus.DeletedFromIndex,
                        FilePath = "files/deletedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.NewInIndex,
                        FilePath = "files/createdFile",
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("files/changedFile", changedFile.Object)
                .SeedPatch("files/deletedFile", deletedFile.Object)
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
    public void ShouldReturnRenamedLabel_ForRenamedFile_WithNoLinesChanged() {
        // Arrange
        var singleRenamedFile = new MockPatch {
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
                        State = FileStatus.RenamedInIndex,
                        FilePath = "files/singleRenamedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.ModifiedInIndex,
                        FilePath = "files/changedFile",
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("files/singleRenamedFile", singleRenamedFile.Object)
                .SeedPatch("files/changedFile", changedFile.Object)
                .Object
        };

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = Constants.LowPriorty,
                Value = "renamed single-renamed-file",
                After = ", ",
            },
        });
    }

    [Test]
    public void ShouldReturnRenamedLabel_ForSingleRenamedFile() {
        // Arrange
        var singleRenamedFile = new MockPatch {
            LinesAdded = 100,
            LinesDeleted = 0,
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
                        State = FileStatus.RenamedInIndex,
                        FilePath = "files/singleRenamedFile",
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("files/updatedFile", updatedFile.Object)
                .SeedPatch("files/singleRenamedFile", singleRenamedFile.Object)
                .Object
        };

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = Constants.LowPriorty,
                Value = "renamed single-renamed-file",
                After = ", ",
            },
        });
    }

    [Test]
    public void ShouldReturnRenamedLabel_ForEachRenamedFile_WithDescendingPriority()
    {
        // Arrange
        var smallRenamedFile = new MockPatch {
            LinesAdded = 10,
            LinesDeleted = 0,
        };

        var mediumRenamedFile = new MockPatch {
            LinesAdded = 50,
            LinesDeleted = 0,
        };

        var largeRenamedFile = new MockPatch {
            LinesAdded = 100,
            LinesDeleted = 0,
        };

        var modifiedAndRenamedFile = new MockPatch {
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
                        State = FileStatus.RenamedInIndex,
                        FilePath = "files/smallRenamedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.RenamedInIndex,
                        FilePath = "files/mediumRenamedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.RenamedInIndex,
                        FilePath = "files/largeRenamedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.RenamedInIndex,
                        FilePath = "files/modifiedAndRenamedFile",
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("files/smallRenamedFile", smallRenamedFile.Object)
                .SeedPatch("files/mediumRenamedFile", mediumRenamedFile.Object)
                .SeedPatch("files/largeRenamedFile", largeRenamedFile.Object)
                .SeedPatch("files/modifiedAndRenamedFile", modifiedAndRenamedFile.Object)
                .Object,
        };

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = Constants.LowPriorty,
                Value = "renamed large-renamed-file",
                After = ", ",
            },
            new() {
                Priority = 0.5,
                Value = "renamed medium-renamed-file",
                After = ", ",
            },
            new() {
                Priority = Constants.NotAPriority,
                Value = "renamed small-renamed-file",
                After = ", ",
            },
        }).And.BeInDescendingOrder(h => h.Priority);
    }
}
