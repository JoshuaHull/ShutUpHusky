namespace ShutUpHusky.Utils;

public interface IRandomNumberGenerator {
    public double Next();
}

internal class RandomNumberGenerator : IRandomNumberGenerator {
    public double Next() => new Random().NextDouble();
}
