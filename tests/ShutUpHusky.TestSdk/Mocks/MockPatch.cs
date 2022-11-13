using LibGit2Sharp;
using Moq;

namespace ShutUpHusky.TestSdk.Mocks;

public class MockPatch: Mock<Patch> {
    public int LinesAdded { get; set; }
    public int LinesDeleted { get; set; }

    public MockPatch() {
        SetupGet(p => p.LinesAdded).Returns(() => LinesAdded);
        SetupGet(p => p.LinesDeleted).Returns(() => LinesDeleted);
    }
}
