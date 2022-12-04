using LibGit2Sharp;
using Moq;

namespace ShutUpHusky.TestSdk.Mocks;

public class MockStatusEntry : Mock<StatusEntry> {
    public string FilePath { get; set; } = string.Empty;
    public FileStatus State { get; set; }

    public MockStatusEntry() {
        SetupGet(e => e.FilePath).Returns(() => FilePath);
        SetupGet(e => e.State).Returns(() => State);
    }
}
