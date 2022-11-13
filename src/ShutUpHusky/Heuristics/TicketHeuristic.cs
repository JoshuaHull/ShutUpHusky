using LibGit2Sharp;

namespace ShutUpHusky.Heuristics;

public class TicketHeuristic : IHeuristic {
    public HeuristicResult Analyse(IRepository _) =>
        new() {
            Priority = 10,
            Value = $"feat(rand-{(Math.Floor(new Random().NextDouble() * 9999))})",
        };
}
