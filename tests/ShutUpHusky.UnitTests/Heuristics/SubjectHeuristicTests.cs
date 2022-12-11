using LibGit2Sharp;
using LibGit2Sharp.Mocks;
using NUnit.Framework;
using ShutUpHusky.Heuristics;

namespace ShutUpHusky.UnitTests.Heuristics;

public class SubjectHeuristicTests
{
    private SubjectHeuristic Heuristic => new();

    [TestCase("programs", "entries", "programs-directory", ExpectedResult = "programs")]
    [TestCase("cats", "dogs", "fish", ExpectedResult = "")]
    [TestCase("really-cool-file.ts", "less-cool-file.ts", "cool.test.ts", ExpectedResult = "cool")]
    [TestCase("ReallyCoolFile.ts", "less-cool-file.ts", "ThisFile-IsCool.test.ts", ExpectedResult = "cool")]
    [TestCase("SubjectHeuristicTests.cs", "TypeAndScopeHeuristicTests.cs", "CommitMessageAssemblerTests.ts", ExpectedResult = "heuristic")]
    public string ShouldChooseSubjectFromFileNames_WhenThereIsACommonTerm(
        string firstFileName, string secondFileName, string thirdFileName
    ) {
        // Arrange
        var firstEntry = new MockStatusEntry {
            FilePath = firstFileName,
            State = FileStatus.ModifiedInIndex,
        };
        var secondEntry = new MockStatusEntry {
            FilePath = secondFileName,
            State = FileStatus.NewInIndex,
        };
        var thirdEntry = new MockStatusEntry {
            FilePath = thirdFileName,
            State = FileStatus.DeletedFromIndex,
        };

        var repo = new MockRepository {
            Status = new MockRepositoryStatus {
                StatusEntries = new List<StatusEntry> {
                    firstEntry.Object,
                    secondEntry.Object,
                    thirdEntry.Object,
                },
            }.Object,
        };

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        return result.SingleOrDefault()?.Value ?? string.Empty;
    }
}
