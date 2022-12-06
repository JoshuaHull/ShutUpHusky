using FluentAssertions;
using LibGit2Sharp;
using LibGit2Sharp.Mocks;
using NUnit.Framework;
using ShutUpHusky.Heuristics;

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
        result.Should().HaveCount(0);
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
                Priority = Constants.MediumPriorty,
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
                Priority = Constants.MediumPriorty,
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
                Priority = Constants.MediumPriorty,
                Value = "renamed large-renamed-file",
                After = ", ",
            },
            new() {
                Priority = 3,
                Value = "renamed medium-renamed-file",
                After = ", ",
            },
            new() {
                Priority = Constants.LowPriorty,
                Value = "renamed small-renamed-file",
                After = ", ",
            },
        }).And.BeInDescendingOrder(h => h.Priority);
    }

    [Test]
    public void ShouldReturnMovedLabel_ForMovedFile_WithNoLinesChanged() {
        // Arrange
        var movedFile = new MockPatch {
            LinesAdded = 0,
            LinesDeleted = 0,
        };

        var renamedFile = new MockPatch {
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
                        FilePath = "files/renamedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.RenamedInIndex,
                        FilePath = "files/movedFile",
                        HeadToIndexRenameDetails = new MockRenameDetails {
                            OldFilePath = "oldFiles/movedFile",
                            NewFilePath = "files/movedFile",
                        }.Object,
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("files/movedFile", movedFile.Object)
                .SeedPatch("files/renamedFile", renamedFile.Object)
                .Object
        };

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = Constants.HighPriorty,
                Value = "moved moved-file",
                After = ", ",
            },
            new() {
                Priority = Constants.MediumPriorty,
                Value = "renamed renamed-file",
                After = ", ",
            },
        });
    }

    [Test]
    public void ShouldReturnMovedLabel_ForSingleMovedFile() {
        // Arrange
        var singleMovedFile = new MockPatch {
            LinesAdded = 100,
            LinesDeleted = 0,
        };

        var renamedFile = new MockPatch {
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
                        State = FileStatus.RenamedInIndex,
                        FilePath = "files/renamedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.RenamedInIndex,
                        FilePath = "files/singleMovedFile",
                        HeadToIndexRenameDetails = new MockRenameDetails {
                            OldFilePath = "oldFiles/singleMovedFile",
                            NewFilePath = "files/singleMovedFile",
                        }.Object,
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("files/renamedFile", renamedFile.Object)
                .SeedPatch("files/singleMovedFile", singleMovedFile.Object)
                .Object
        };

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = Constants.HighPriorty,
                Value = "moved single-moved-file",
                After = ", ",
            },
        });
    }

    [Test]
    public void ShouldReturnMovedLabel_ForEachMovedFile_WithDescendingPriority()
    {
        // Arrange
        var simplyRenamedFile = new MockPatch {
            LinesAdded = 10,
            LinesDeleted = 0,
        };

        var mediumMovedFile = new MockPatch {
            LinesAdded = 50,
            LinesDeleted = 0,
        };

        var largeMovedFile = new MockPatch {
            LinesAdded = 100,
            LinesDeleted = 0,
        };

        var modifiedAndMovedFile = new MockPatch {
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
                        FilePath = "files/simplyRenamedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.RenamedInIndex,
                        FilePath = "files/mediumMovedFile",
                        HeadToIndexRenameDetails = new MockRenameDetails {
                            OldFilePath = "oldFiles/mediumMovedFile",
                            NewFilePath = "files/mediumMovedFile",
                        }.Object,
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.RenamedInIndex,
                        FilePath = "files/largeMovedFile",
                        HeadToIndexRenameDetails = new MockRenameDetails {
                            OldFilePath = "oldFiles/largeMovedFile",
                            NewFilePath = "files/largeMovedFile",
                        }.Object,
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.RenamedInIndex,
                        FilePath = "files/modifiedAndMovedFile",
                        HeadToIndexRenameDetails = new MockRenameDetails {
                            OldFilePath = "oldFiles/modifiedAndMovedFile",
                            NewFilePath = "files/modifiedAndMovedFile",
                        }.Object,
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("files/simplyRenamedFile", simplyRenamedFile.Object)
                .SeedPatch("files/mediumMovedFile", mediumMovedFile.Object)
                .SeedPatch("files/largeMovedFile", largeMovedFile.Object)
                .SeedPatch("files/modifiedAndMovedFile", modifiedAndMovedFile.Object)
                .Object,
        };

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = Constants.HighPriorty,
                Value = "moved large-moved-file",
                After = ", ",
            },
            new() {
                Priority = 5.5,
                Value = "moved medium-moved-file",
                After = ", ",
            },
            new() {
                Priority = Constants.LowPriorty,
                Value = "moved modified-and-moved-file",
                After = ", ",
            },
            new() {
                Priority = Constants.MediumPriorty,
                Value = "renamed simply-renamed-file",
                After = ", ",
            },
        }).And.BeInDescendingOrder(h => h.Priority);
    }
}
