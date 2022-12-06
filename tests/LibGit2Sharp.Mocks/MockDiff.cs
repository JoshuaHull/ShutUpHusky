using System.Collections.Generic;
using System.Linq;
using Moq;

namespace LibGit2Sharp.Mocks;

public class MockDiff: Mock<Diff> {
    private readonly Dictionary<string, Patch> _patches = new();

    public MockDiff() {
        Setup(d => d.Compare<Patch>(It.IsAny<IEnumerable<string>>()))
            .Returns<IEnumerable<string>>(paths => _patches[paths.Single()]);

        Setup(d => d.Compare<Patch>(It.IsAny<Tree>(), It.IsAny<DiffTargets>(), It.IsAny<IEnumerable<string>>()))
            .Returns<Tree, DiffTargets, IEnumerable<string>>((tree, targets, paths) => _patches[paths.Single()]);
    }

    public MockDiff SeedPatch(string filePath, Patch patch) {
        _patches.Add(filePath, patch);
        return this;
    }
}
