using LibGit2Sharp;
using Moq;

namespace ShutUpHusky.TestSdk.Mocks;

public class MockCommit: Mock<Commit> {
    public Tree Tree { get; set; }

    public MockCommit() {
        SetupGet(c => c.Tree).Returns(() => Tree);
    }
}
