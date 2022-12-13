using System;
using Moq;

namespace LibGit2Sharp.Mocks;

public class MockRepository: Mock<IRepository>, IHaveSensibleDefaults<MockRepository> {
    public RepositoryStatus? Status { get; set; }
    public Diff? Diff { get; set; }
    public Branch? Head { get; set; }

    public MockRepository() {
        Setup(r => r.RetrieveStatus(It.IsAny<StatusOptions>())).Returns(() => Status ?? throw new NullReferenceException());
        SetupGet(r => r.Diff).Returns(() => Diff ?? throw new NullReferenceException());
        SetupGet(r => r.Head).Returns(() => Head ?? throw new NullReferenceException());
    }

    public MockRepository WithSensibleDefaults() {
        Head ??= new MockBranch {
            Tip = new MockCommit {
                Tree = new MockTree {
                }.Object,
            }.Object,
        }.Object;

        return this;
    }
}
