using LibGit2Sharp;
using Moq;

namespace ShutUpHusky.TestSdk.Mocks;

public class MockBranch: Mock<Branch> {
    public Commit Tip { get; set; }

    public MockBranch() {
        SetupGet(b => b.Tip).Returns(() => Tip);
    }
}
