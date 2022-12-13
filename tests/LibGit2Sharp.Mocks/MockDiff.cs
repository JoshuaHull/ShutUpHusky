using System.Collections.Generic;
using System.Linq;
using Moq;

namespace LibGit2Sharp.Mocks;

public class MockDiff: Mock<Diff> {
    private readonly Dictionary<string, MockPatch> _patches = new();

    public MockDiff() {
        Setup(d => d.Compare<Patch>(It.IsAny<IEnumerable<string>>()))
            .Returns<IEnumerable<string>>(paths => _patches[paths.Single()].Object);

        Setup(d => d.Compare<Patch>(It.IsAny<Tree>(), It.IsAny<DiffTargets>(), It.IsAny<IEnumerable<string>>()))
            .Returns<Tree, DiffTargets, IEnumerable<string>>((tree, targets, paths) => _patches[paths.Single()].Object);
    }

    public MockDiff SeedPatch(string filePath, MockPatch patch) {
        _patches.Add(filePath, patch);
        return this;
    }
}
