using Moq;
using ShutUpHusky.Utils;

namespace ShutUpHusky.TestSdk.Mocks;

public class MockRandomNumberGenerator : Mock<IRandomNumberGenerator> {
    public MockRandomNumberGenerator(double value) {
        Setup(g => g.Next()).Returns(() => value);
    }
}
