using LibGit2Sharp;
using Moq;

namespace ShutUpHusky.TestSdk.Mocks;

public class MockRepository: Mock<IRepository> {
    public RepositoryStatus? Status { get; set; }
    public Diff? Diff { get; set; }
    public Branch? Head { get; set; }

    public MockRepository() {
        Setup(r => r.RetrieveStatus(It.IsAny<StatusOptions>())).Returns(() => Status ?? throw new NullReferenceException());
        SetupGet(r => r.Diff).Returns(() => Diff ?? throw new NullReferenceException());
        SetupGet(r => r.Head).Returns(() => Head ?? throw new NullReferenceException());
    }
}
