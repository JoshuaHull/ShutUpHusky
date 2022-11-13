using LibGit2Sharp;
using Moq;

namespace ShutUpHusky.TestSdk.Mocks;

public class MockRepositoryStatus: Mock<RepositoryStatus> {
    public IEnumerable<StatusEntry> Staged { get; set; }
    public IEnumerable<StatusEntry> StatusEntries { get; set; }

    public MockRepositoryStatus() {
        SetupGet(e => e.Staged).Returns(() => Staged);
        Setup(s => s.GetEnumerator()).Returns(() => StatusEntries.GetEnumerator());
    }
}
