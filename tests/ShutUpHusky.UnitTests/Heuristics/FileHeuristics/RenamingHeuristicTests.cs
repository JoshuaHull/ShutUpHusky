using FluentAssertions;
using LibGit2Sharp;
using LibGit2Sharp.Mocks;
using NUnit.Framework;
using ShutUpHusky.Heuristics;
using ShutUpHusky.Heuristics.FileHeuristics;

namespace ShutUpHusky.UnitTests.Heuristics.FileHeuristics;

public class RenamingHeuristicTests
{
    private RenamingHeuristic Heuristic => new();

    [Test]
    public void ShouldNotReturnALabel_WhenThereAreNoRenamedFiles() {
        // Arrange
        var repo = new MockRepository()
            .WithSensibleDefaults()
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.ModifiedInIndex,
                    FilePath = "files/changedFile",
                },
                new MockPatch {
                    LinesAdded = 50,
                    LinesDeleted = 50,
                }
            )
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.DeletedFromIndex,
                    FilePath = "files/deletedFile",
                },
                new MockPatch {
                    LinesAdded = 0,
                    LinesDeleted = 100,
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
            );

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().HaveCount(0);
    }

    [Test]
    public void ShouldReturnRenamedLabel_ForRenamedFile_WithNoLinesChanged() {
        // Arrange
        var repo = new MockRepository()
            .WithSensibleDefaults()
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.RenamedInIndex,
                    FilePath = "files/singleRenamedFile",
                    HeadToIndexRenameDetails = new MockRenameDetails {
                        OldFilePath = "files/singleFile",
                        NewFilePath = "files/singleRenamedFile",
                    },
                },
                new MockPatch {
                    LinesAdded = 0,
                    LinesDeleted = 0,
                }
            )
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.ModifiedInIndex,
                    FilePath = "files/changedFile",
                },
                new MockPatch {
                    LinesAdded = 100,
                    LinesDeleted = 0,
                }
            );

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = 1,
                Value = "renamed single-renamed-file",
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
        var repo = new MockRepository()
            .WithSensibleDefaults()
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.ModifiedInIndex,
                    FilePath = "files/updatedFile",
                },
                new MockPatch {
                    LinesAdded = 50,
                    LinesDeleted = 50,
                }
            )
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.RenamedInIndex,
                    FilePath = fileName,
                    HeadToIndexRenameDetails = new MockRenameDetails {
                        OldFilePath = "oldFileName",
                        NewFilePath = fileName,
                    },
                },
                new MockPatch {
                    LinesAdded = 100,
                    LinesDeleted = 0,
                }
            );

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        return result.Single().Value;
    }

    [Test]
    public void ShouldReturnRenamedLabel_ForEachRenamedFile_WithDescendingPriority()
    {
        // Arrange
        var repo = new MockRepository()
            .WithSensibleDefaults()
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.RenamedInIndex,
                    FilePath = "files/smallRenamedFile",
                    HeadToIndexRenameDetails = new MockRenameDetails {
                        OldFilePath = "files/smallFile",
                        NewFilePath = "files/smallRenamedFile",
                    },
                },
                new MockPatch {
                    LinesAdded = 10,
                    LinesDeleted = 0,
                }
            )
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.RenamedInIndex,
                    FilePath = "files/mediumRenamedFile",
                    HeadToIndexRenameDetails = new MockRenameDetails {
                        OldFilePath = "files/mediumFile",
                        NewFilePath = "files/mediumRenamedFile",
                    },
                },
                new MockPatch {
                    LinesAdded = 50,
                    LinesDeleted = 0,
                }
            )
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.RenamedInIndex,
                    FilePath = "files/largeRenamedFile",
                    HeadToIndexRenameDetails = new MockRenameDetails {
                        OldFilePath = "files/largeFile",
                        NewFilePath = "files/largeRenamedFile",
                    },
                },
                new MockPatch {
                    LinesAdded = 100,
                    LinesDeleted = 0,
                }
            )
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.RenamedInIndex,
                    FilePath = "files/modifiedAndRenamedFile",
                    HeadToIndexRenameDetails = new MockRenameDetails {
                        OldFilePath = "files/modifiedFile",
                        NewFilePath = "files/modifiedAndRenamedFile",
                    },
                },
                new MockPatch {
                    LinesAdded = 10,
                    LinesDeleted = 20,
                }
            );

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = 101,
                Value = "renamed large-renamed-file",
            },
            new() {
                Priority = 51,
                Value = "renamed medium-renamed-file",
            },
            new() {
                Priority = 11,
                Value = "renamed small-renamed-file",
            },
        }).And.BeInDescendingOrder(h => h.Priority);
    }

    [Test]
    public void ShouldReturnMovedLabel_ForMovedFile_WithNoLinesChanged() {
        // Arrange
        var repo = new MockRepository()
            .WithSensibleDefaults()
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.RenamedInIndex,
                    FilePath = "files/movedFile",
                    HeadToIndexRenameDetails = new MockRenameDetails {
                        OldFilePath = "oldFiles/movedFile",
                        NewFilePath = "files/movedFile",
                    },
                },
                new MockPatch {
                    LinesAdded = 0,
                    LinesDeleted = 0,
                }
            )
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
                    LinesAdded = 100,
                    LinesDeleted = 0,
                }
            );

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = 1,
                Value = "moved moved-file",
            },
            new() {
                Priority = 101,
                Value = "renamed renamed-file",
            },
        }).And.BeInDescendingOrder(h => h.Priority);
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
        var repo = new MockRepository()
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
                    LinesAdded = 50,
                    LinesDeleted = 50,
                }
            )
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.RenamedInIndex,
                    FilePath = toFileName,
                    HeadToIndexRenameDetails = new MockRenameDetails {
                        OldFilePath = fromFileName,
                        NewFilePath = toFileName,
                    },
                },
                new MockPatch {
                    LinesAdded = 100,
                    LinesDeleted = 0,
                }
            );

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        return result.Single().Value;
    }

    [Test]
    public void ShouldReturnMovedLabel_ForEachMovedFile_WithDescendingPriority()
    {
        // Arrange
        var repo = new MockRepository()
            .WithSensibleDefaults()
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.RenamedInIndex,
                    FilePath = "files/simplyRenamedFile",
                    HeadToIndexRenameDetails = new MockRenameDetails {
                        OldFilePath = "files/simpleFile",
                        NewFilePath = "files/simplyRenamedFile",
                    },
                },
                new MockPatch {
                    LinesAdded = 10,
                    LinesDeleted = 0,
                }
            )
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.RenamedInIndex,
                    FilePath = "files/mediumMovedFile",
                    HeadToIndexRenameDetails = new MockRenameDetails {
                        OldFilePath = "oldFiles/mediumMovedFile",
                        NewFilePath = "files/mediumMovedFile",
                    },
                },
                new MockPatch {
                    LinesAdded = 50,
                    LinesDeleted = 0,
                }
            )
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.RenamedInIndex,
                    FilePath = "files/largeMovedFile",
                    HeadToIndexRenameDetails = new MockRenameDetails {
                        OldFilePath = "oldFiles/largeMovedFile",
                        NewFilePath = "files/largeMovedFile",
                    },
                },
                new MockPatch {
                    LinesAdded = 100,
                    LinesDeleted = 0,
                }
            )
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.RenamedInIndex,
                    FilePath = "files/modifiedAndMovedFile",
                    HeadToIndexRenameDetails = new MockRenameDetails {
                        OldFilePath = "oldFiles/modifiedAndMovedFile",
                        NewFilePath = "files/modifiedAndMovedFile",
                    },
                },
                new MockPatch {
                    LinesAdded = 10,
                    LinesDeleted = 20,
                }
            );

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = 101,
                Value = "moved large-moved-file",
            },
            new() {
                Priority = 51,
                Value = "moved medium-moved-file",
            },
            new() {
                Priority = 11,
                Value = "moved modified-and-moved-file",
            },
            new() {
                Priority = 11,
                Value = "renamed simply-renamed-file",
            },
        }).And.BeInDescendingOrder(h => h.Priority);
    }
}
