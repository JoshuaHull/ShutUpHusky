namespace ShutUpHusky.Utils;

public interface IRandomNumberGenerator {
    public double Next();
}

public class RandomNumberGenerator : IRandomNumberGenerator {
    public double Next() => new Random().NextDouble();
}
