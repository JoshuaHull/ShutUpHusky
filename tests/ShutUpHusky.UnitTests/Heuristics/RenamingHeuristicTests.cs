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
        }.WithSensibleDefaults();

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
        }.WithSensibleDefaults();

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

    [TestCase("singleRenamedFile", ExpectedResult = "renamed single-renamed-file")]
    [TestCase("singleRenamedFile.ts", ExpectedResult = "renamed single-renamed-file")]
    [TestCase("files/SingleRenamedFile.cs", ExpectedResult = "renamed single-renamed-file")]
    [TestCase("singleRenamedFile.spec.ts", ExpectedResult = "renamed single-renamed-file tests")]
    [TestCase("singleRenamedFile.specs.ts", ExpectedResult = "renamed single-renamed-file tests")]
    [TestCase("singleRenamedFile.test.ts", ExpectedResult = "renamed single-renamed-file tests")]
    [TestCase("singleRenamedFile.tests.ts", ExpectedResult = "renamed single-renamed-file tests")]
    public string ShouldReturnRenamedLabel_ForSingleRenamedFile(string fileName) {
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
            Status = new MockRepositoryStatus {
                StatusEntries = new[] {
                    new MockStatusEntry {
                        State = FileStatus.ModifiedInIndex,
                        FilePath = "files/updatedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.RenamedInIndex,
                        FilePath = fileName,
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("files/updatedFile", updatedFile.Object)
                .SeedPatch(fileName, singleRenamedFile.Object)
                .Object
        }.WithSensibleDefaults();

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        return result.Single().Value;
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
        }.WithSensibleDefaults();

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
                Priority = Constants.LowPriority,
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
        }.WithSensibleDefaults();

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

    [TestCase("oldFiles/singleMovedFile", "newFiles/singleMovedFile", ExpectedResult = "moved single-moved-file")]
    [TestCase("oldFiles/singleMovedFile.ts", "newFiles/singleMovedFile.ts", ExpectedResult = "moved single-moved-file")]
    [TestCase("oldFiles/SingleMovedFile.cs", "newFiles/SingleMovedFile.cs", ExpectedResult = "moved single-moved-file")]
    [TestCase("oldFiles/singleMovedFile.spec.ts", "newFiles/singleMovedFile.spec.ts", ExpectedResult = "moved single-moved-file tests")]
    [TestCase("oldFiles/singleMovedFile.specs.ts", "newFiles/singleMovedFile.specs.ts", ExpectedResult = "moved single-moved-file tests")]
    [TestCase("oldFiles/singleMovedFile.test.ts", "newFiles/singleMovedFile.test.ts", ExpectedResult = "moved single-moved-file tests")]
    [TestCase("oldFiles/singleMovedFile.tests.ts", "newFiles/singleMovedFile.tests.ts", ExpectedResult = "moved single-moved-file tests")]
    public string ShouldReturnMovedLabel_ForSingleMovedFile(string fromFileName, string toFileName) {
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
            Status = new MockRepositoryStatus {
                StatusEntries = new[] {
                    new MockStatusEntry {
                        State = FileStatus.RenamedInIndex,
                        FilePath = "files/renamedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.RenamedInIndex,
                        FilePath = toFileName,
                        HeadToIndexRenameDetails = new MockRenameDetails {
                            OldFilePath = fromFileName,
                            NewFilePath = toFileName,
                        }.Object,
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("files/renamedFile", renamedFile.Object)
                .SeedPatch(toFileName, singleMovedFile.Object)
                .Object
        }.WithSensibleDefaults();

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        return result.Single().Value;
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
        }.WithSensibleDefaults();

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
                Priority = Constants.LowPriority,
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
