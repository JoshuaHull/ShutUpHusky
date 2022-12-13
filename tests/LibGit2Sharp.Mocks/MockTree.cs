using Moq;

namespace LibGit2Sharp.Mocks;

public class MockTree: Mock<Tree>, IHaveSensibleDefaults<MockTree> {
    public MockTree WithSensibleDefaults() => this;
}
